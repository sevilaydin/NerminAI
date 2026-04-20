#!/usr/bin/env python
"""
rebuild_from_export.py
======================
ChatGPT export JSON dosyalarından sadece anlamlı kullanıcı mesajlarını çeker,
temizler, chunk'lara böler, embed eder ve Memories tablosuna yükler.

Çalıştırmadan önce:
  - embedding_server.py aktif olmalı (port 8000)
  - PostgreSQL çalışıyor olmalı
  - pv2_ profil chunk'ları zaten DB'de olmalı (build_profile_v2.py çalıştırılmış)

Kullanım:
  python data/rebuild_from_export.py
"""

import sys, json, re, hashlib, requests, time
sys.stdout.reconfigure(encoding='utf-8')
import psycopg
from pathlib import Path
from datetime import datetime

# ── Ayarlar ──────────────────────────────────────────────────────────────────
DB_URL    = "postgresql://postgres:12345@localhost:5432/nerminai"
EMBED_URL = "http://127.0.0.1:8000/embed"

EXPORT_DIRS = [
    Path("data/chatgpt-export"),
    Path("data/new-chatgpt-export"),
]

# Conversation chunk'larının chunk_id prefix'i — silinip yeniden yüklenecek
CONV_CHUNK_PREFIX = "conv_"

# ── Gürültü filtreleri ───────────────────────────────────────────────────────

# Bu kelimeleri içeren mesajlar büyük ihtimalle noise
NOISE_PATTERNS = [
    r"^(merhaba|selam|hi|hello|hey|tamam|ok|okay|teşekkür|thanks|thank you|evet|hayır|yes|no)\s*[.!?]?\s*$",
    r"dalle|image|görsel üret|fotoğraf çiz|resim yap|canva",
    r"^\s*[\W\d]+\s*$",  # sadece rakam/noktalama
]

# Bu pattern'lar kişisel sinyal içermez (araştırma soruları, genel bilgi)
GENERIC_PATTERNS = [
    r"nasıl yapılır\??\s*$",
    r"nedir\??\s*$",
    r"ne demek\??\s*$",
    r"tarifi\s",
    r"^(python|javascript|sql|kod|yazılım|program)",
]

# Güçlü kişisel sinyal taşıyan anahtar kelimeler (boost için)
PERSONAL_SIGNALS = [
    "ben", "benim", "bana", "bende", "benimle", "eşim", "eşimle", "samir",
    "kedim", "kedilerim", "annem", "babam", "ailem", "arkadaş",
    "istiyorum", "seviyorum", "sevmiyorum", "hayal", "hayalim",
    "almak istiyorum", "gitmek istiyorum", "olmak istiyorum",
    "dyson", "altın", "takı", "barcelona", "hollanda", "tiflis", "morocco",
    "pastane", "kafe", "kahve", "kocaeli", "izmit", "azerbaycan",
    "doğum", "evlendim", "evlilik", "düğün",
]

# Konu sınıflandırıcı
TOPIC_KEYWORDS = {
    "travel":    ["seyahat", "tatil", "gez", "hollanda", "barcelona", "tiflis", "morocco", "fas", "mısır", "otel", "uçuş", "vize"],
    "food":      ["yemek", "ye", "tarif", "mutfak", "kahve", "espresso", "börek", "pasta", "tiramisu", "sushi", "turşu", "lezzet", "restoran"],
    "shopping":  ["al", "satın", "dyson", "marka", "ürün", "altın", "takı", "mağaza", "kıyafet", "zara"],
    "pets":      ["kedi", "köpek", "hayvan", "pati", "mama", "veteriner", "british", "siamese"],
    "lifestyle": ["hayal", "hedef", "vision", "manifest", "plan", "gelecek", "2025", "2026", "kendim", "pastane", "kafe"],
    "emotion":   ["his", "duygu", "mutlu", "üzgün", "canım", "moral", "stres", "kaygı", "sinirli", "kızgın"],
    "family":    ["eşim", "evlilik", "aile", "anne", "baba", "arkadaş", "ilişki", "samir", "kızıyorum"],
    "health":    ["sağlık", "doktor", "ilaç", "ameliyat", "ağrı", "yorgun", "lustral", "sertralin"],
    "astrology": ["burç", "kova", "terazi", "balık", "yükselen", "ay burcu", "human design", "astroloji"],
    "finance":   ["para", "dolar", "zengin", "maaş", "birikim", "yatırım", "kredi", "borç"],
}


def classify_topic(text: str) -> str:
    lower = text.lower()
    scores = {topic: 0 for topic in TOPIC_KEYWORDS}
    for topic, keywords in TOPIC_KEYWORDS.items():
        for kw in keywords:
            if kw in lower:
                scores[topic] += 1
    best = max(scores, key=scores.get)
    return best if scores[best] > 0 else "misc"


def signal_strength(text: str) -> str:
    """Kişisel sinyal gücünü ölç: high / medium / low"""
    lower = text.lower()
    hits = sum(1 for s in PERSONAL_SIGNALS if s in lower)
    if hits >= 3:
        return "high"
    if hits >= 1:
        return "medium"
    return "low"


def is_noise(text: str) -> bool:
    """True = bu mesajı atla"""
    text_stripped = text.strip()
    # Çok kısa
    word_count = len(text_stripped.split())
    if word_count < 5:
        return True
    # URL'den ibaret
    if re.match(r"^https?://\S+$", text_stripped):
        return True
    # Noise pattern
    for pat in NOISE_PATTERNS:
        if re.search(pat, text_stripped, re.IGNORECASE):
            return True
    return False


def is_low_signal_generic(text: str) -> bool:
    """Genel bilgi sorusu / kişisel sinyal yok → atla"""
    lower = text.lower()
    # Kişisel sinyal var mı?
    has_personal = any(s in lower for s in PERSONAL_SIGNALS)
    if has_personal:
        return False
    # Generic kalıp
    for pat in GENERIC_PATTERNS:
        if re.search(pat, lower):
            return True
    return False


def extract_text(part) -> str:
    """parts içindeki metni çıkart"""
    if isinstance(part, str):
        return part
    if isinstance(part, dict) and part.get("content_type") == "text":
        return part.get("text", "")
    return ""


def iter_user_messages(conversations: list) -> list:
    """JSON listesinden kullanıcı mesajlarını çek"""
    messages = []
    for conv in conversations:
        conv_id = conv.get("conversation_id") or conv.get("id", "unknown")
        conv_title = conv.get("title", "")
        mapping = conv.get("mapping", {})

        for node_id, node in mapping.items():
            msg = node.get("message")
            if not msg:
                continue

            # Sadece kullanıcı mesajları
            if msg.get("author", {}).get("role") != "user":
                continue

            content = msg.get("content", {})
            if content.get("content_type") != "text":
                continue

            parts = content.get("parts", [])
            text = " ".join(extract_text(p) for p in parts if p).strip()
            if not text:
                continue

            ts = msg.get("create_time")

            messages.append({
                "text": text,
                "conv_id": conv_id,
                "conv_title": conv_title,
                "timestamp": ts,
                "node_id": node_id,
            })

    return messages


def clean_text(text: str) -> str:
    """Temel temizlik"""
    # Birden fazla boşluk
    text = re.sub(r"\s+", " ", text)
    # URL'leri çıkart
    text = re.sub(r"https?://\S+", "", text)
    return text.strip()


def chunk_id_for(text: str, conv_id: str) -> str:
    h = hashlib.md5(f"{conv_id}:{text[:100]}".encode()).hexdigest()[:10]
    return f"{CONV_CHUNK_PREFIX}{h}"


def embed_texts(texts: list) -> list:
    """Embedding server'dan vektör al"""
    resp = requests.post(EMBED_URL, json={"texts": texts}, timeout=60)
    resp.raise_for_status()
    data = resp.json()
    key = "embeddings" if "embeddings" in data else "Embeddings"
    return data[key]


def load_json_files(dirs: list) -> list:
    conversations = []
    for d in dirs:
        p = Path(d)
        if not p.exists():
            print(f"  [uyarı] dizin bulunamadı: {d}")
            continue
        for f in sorted(p.glob("conversations-*.json")):
            print(f"  Yükleniyor: {f}")
            with open(f, encoding="utf-8") as fh:
                data = json.load(fh)
                conversations.extend(data)
    return conversations


def main():
    print("=" * 60)
    print("NerminAI — Konuşma verisi yeniden işleniyor")
    print("=" * 60)

    # 1. JSON dosyalarını yükle
    print("\n[1/6] JSON dosyaları yükleniyor...")
    conversations = load_json_files(EXPORT_DIRS)
    print(f"  Toplam konuşma: {len(conversations)}")

    # 2. Kullanıcı mesajlarını çek
    print("\n[2/6] Kullanıcı mesajları çıkarılıyor...")
    raw_messages = iter_user_messages(conversations)
    print(f"  Ham mesaj: {len(raw_messages)}")

    # 3. Temizle + filtrele
    print("\n[3/6] Gürültü filtreleniyor...")
    chunks = []
    skipped_noise = 0
    skipped_generic = 0
    seen_texts = set()

    for msg in raw_messages:
        text = clean_text(msg["text"])

        # Noise filtresi
        if is_noise(text):
            skipped_noise += 1
            continue

        # Generic / kişisel sinyal yok filtresi
        if is_low_signal_generic(text):
            skipped_generic += 1
            continue

        # Duplicate kontrol (aynı metin)
        text_key = text.lower()[:80]
        if text_key in seen_texts:
            continue
        seen_texts.add(text_key)

        topic = classify_topic(text)
        strength = signal_strength(text)

        # Low signal + misc topic → atla
        if strength == "low" and topic == "misc":
            skipped_generic += 1
            continue

        chunk_id = chunk_id_for(text, msg["conv_id"])

        chunks.append({
            "chunk_id": chunk_id,
            "content": text,
            "topic": topic,
            "signal_strength": strength,
            "conv_id": msg["conv_id"],
            "timestamp": msg["timestamp"],
        })

    print(f"  Noise atlandı: {skipped_noise}")
    print(f"  Generic atlandı: {skipped_generic}")
    print(f"  Kalan (embed edilecek): {len(chunks)}")

    if not chunks:
        print("Hiç chunk kalmadı, çıkılıyor.")
        return

    # 4. Embed et (batch)
    print("\n[4/6] Embedding hesaplanıyor...")
    BATCH = 32
    embeddings = []
    for i in range(0, len(chunks), BATCH):
        batch = chunks[i:i+BATCH]
        texts = [c["content"] for c in batch]
        print(f"  Batch {i//BATCH + 1}/{(len(chunks)-1)//BATCH + 1} ({len(texts)} chunk)...", end=" ", flush=True)
        try:
            embs = embed_texts(texts)
            embeddings.extend(embs)
            print("ok")
        except Exception as e:
            print(f"HATA: {e}")
            # Hata durumunda null embedding ile devam et
            embeddings.extend([None] * len(batch))
        time.sleep(0.1)

    # 5. DB'ye yükle
    print("\n[5/6] Veritabanına yükleniyor...")
    conn = psycopg.connect(DB_URL)

    # Önce eski conv_ chunk'larını sil
    with conn.cursor() as cur:
        cur.execute('DELETE FROM "Memories" WHERE "ChunkId" LIKE %s', (f"{CONV_CHUNK_PREFIX}%",))
        deleted = cur.rowcount
        conn.commit()
    print(f"  Eski conv_ chunk'ları silindi: {deleted}")

    inserted = 0
    for chunk, emb in zip(chunks, embeddings):
        if emb is None:
            continue
        emb_json = json.dumps(emb)

        # ConfidenceTier: high=2, medium=1, low=0
        tier = {"high": 2, "medium": 1, "low": 0}.get(chunk["signal_strength"], 1)

        with conn.cursor() as cur:
            cur.execute("""
                INSERT INTO "Memories"
                    ("ChunkId","Content","Topic","Language","IsSensitive","ConfidenceTier","Embedding")
                VALUES (%s,%s,%s,'tr',FALSE,%s,%s)
                ON CONFLICT ("ChunkId") DO UPDATE SET
                    "Content"=EXCLUDED."Content",
                    "Embedding"=EXCLUDED."Embedding",
                    "ConfidenceTier"=EXCLUDED."ConfidenceTier"
            """, (chunk["chunk_id"], chunk["content"], chunk["topic"], tier, emb_json))
        conn.commit()
        inserted += 1

    conn.close()

    # 6. Özet
    print(f"\n[6/6] Tamamlandı!")
    print(f"  Yüklenen chunk: {inserted}")
    topic_counts = {}
    for c in chunks:
        topic_counts[c["topic"]] = topic_counts.get(c["topic"], 0) + 1
    print("\n  Konu dağılımı:")
    for topic, count in sorted(topic_counts.items(), key=lambda x: -x[1]):
        print(f"    {topic:15s}: {count}")

    strength_counts = {}
    for c in chunks:
        s = c["signal_strength"]
        strength_counts[s] = strength_counts.get(s, 0) + 1
    print("\n  Sinyal gücü:")
    for s, count in strength_counts.items():
        print(f"    {s:10s}: {count}")

    print("\nBitti. API'yi yeniden başlatın.")


if __name__ == "__main__":
    main()
