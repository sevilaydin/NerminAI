"""
NerminAI — JSON Parser + Chunker
Adım 1: Ham ChatGPT export JSON → temizlenmiş, tag'li chunk'lar → chunks.json
"""

import json
import os
import re
from datetime import datetime, timezone
from pathlib import Path

# ── Konfigürasyon ──────────────────────────────────────────────────────────────

DATA_DIR = Path(__file__).parent.parent / "NerminData"
OUTPUT_FILE = Path(__file__).parent / "chunks.json"

CHUNK_SIZE = 4          # kaç mesaj bir arada chunk olacak
CHUNK_OVERLAP = 1       # kaç mesaj örtüşme (bağlam kaybı önleme)
MIN_MSG_LENGTH = 8      # bu kadar karakterden kısa mesajlar atlanır

# ── Sensitivity kuralları ──────────────────────────────────────────────────────

SENSITIVE_KEYWORDS = [
    # tıbbi
    "duphaston", "inferject", "ferapplic", "regl", "kanama", "varis",
    "duphastone", "hormon", "ilaç", "doktor", "muayene", "randevu",
    # beden imgesi
    "obez", "şişko", "kilo",
    # aile çatışması / hukuki
    "ev satıyorlar", "anahtar", "belge", "polis başvurusu", "haram",
    # finansal/kimlik
    "pasaport", "vize", "vatandaş",
]

# ── Konu etiketleme kuralları ──────────────────────────────────────────────────

TOPIC_RULES = [
    ("travel",   ["hollanda", "barcelona", "tiflis", "morocco", "fas", "seyahat",
                  "tatil", "otel", "gezi", "dolmabahçe", "ortaköy", "kadıköy",
                  "beşiktaş", "galata", "taksim", "kıbrıs", "azerbaycan"]),
    ("food",     ["yemek", "tarif", "börek", "milföy", "hamur", "pasta",
                  "kahve", "espresso", "americano", "filtre", "makine",
                  "coleslaw", "hamburger", "mücver", "pıras", "dana",
                  "airfry", "fırın", "tencere", "sos", "krema", "napolyon"]),
    ("shopping", ["dyson", "alışveriş", "marka", "ürün", "altın", "ayar",
                  "cream", "watsons", "585", "750", "takı"]),
    ("pets",     ["kedi", "kediler", "kısırlaştır", "zehir", "zararlı",
                  "devetabanı", "british", "siamese"]),
    ("health",   ["sağlık", "doktor", "regl", "kanama", "varis", "ilaç",
                  "muayene", "hormon", "obez", "kilo"]),
    ("lifestyle",["vision board", "manifest", "hayal", "hedef", "2025",
                  "kendimi geliştir", "yoga", "yüzme", "hollanda", "barcelona",
                  "tiflis", "morocco", "fas", "dyson", "pasta", "zengin"]),
    ("emotion",  ["canım sıkkın", "üzgün", "kötü rüya", "huzur", "haksızlık",
                  "kafam karışık", "mutlu", "sevgi", "aile"]),
    ("family",   ["eşim", "eşi", "evliyiz", "evlilik", "arkadaş", "anne",
                  "baba", "komşu", "aile"]),
]


# ── Yardımcı fonksiyonlar ──────────────────────────────────────────────────────

def detect_language(text: str) -> str:
    az_chars = set("əğşçöüıİƏĞŞÇÖÜ")
    tr_chars = set("ğşçöüıİĞŞÇÖÜ")
    lower = text.lower()
    az_words = ["deyil", "bilirəm", "qonşu", "açarını", "elediyim",
                "salam", "heç", "mene", "sene", "amma", "nece"]
    if any(w in lower for w in az_words):
        return "az"
    if any(c in text for c in tr_chars):
        return "tr"
    return "tr"


def detect_topic(text: str) -> str:
    lower = text.lower()
    for topic, keywords in TOPIC_RULES:
        if any(kw in lower for kw in keywords):
            return topic
    return "misc"


def is_sensitive(text: str) -> bool:
    lower = text.lower()
    return any(kw in lower for kw in SENSITIVE_KEYWORDS)


def clean_text(text: str) -> str:
    text = text.strip()
    text = re.sub(r"\s+", " ", text)
    return text


def ts_to_iso(ts: float) -> str:
    return datetime.fromtimestamp(ts, tz=timezone.utc).isoformat()


# ── Mesaj çıkarma ──────────────────────────────────────────────────────────────

def extract_user_messages(conversation: dict) -> list[dict]:
    """Bir konuşmadan user mesajlarını sıralı liste olarak çıkarır."""
    mapping = conversation.get("mapping", {})
    messages = []

    for node_id, node in mapping.items():
        msg = node.get("message")
        if not msg:
            continue
        if msg.get("author", {}).get("role") != "user":
            continue
        if msg.get("weight", 1.0) == 0:
            continue  # silinmiş/alternatif dal

        content = msg.get("content", {})
        if content.get("content_type") != "text":
            continue

        parts = content.get("parts", [])
        text = " ".join(p for p in parts if isinstance(p, str)).strip()

        if len(text) < MIN_MSG_LENGTH:
            continue

        create_time = msg.get("create_time")
        messages.append({
            "text": clean_text(text),
            "create_time": create_time,
            "node_id": node_id,
        })

    # Zamana göre sırala
    messages.sort(key=lambda m: m.get("create_time") or 0)
    return messages


# ── Chunking ───────────────────────────────────────────────────────────────────

def chunk_messages(
    messages: list[dict],
    conv_id: str,
    size: int = CHUNK_SIZE,
    overlap: int = CHUNK_OVERLAP,
) -> list[dict]:
    chunks = []
    i = 0
    chunk_index = 0

    while i < len(messages):
        window = messages[i : i + size]
        combined_text = " | ".join(m["text"] for m in window)

        earliest_ts = window[0].get("create_time")
        latest_ts = window[-1].get("create_time")

        topic = detect_topic(combined_text)
        lang = detect_language(combined_text)
        sensitive = is_sensitive(combined_text)

        # Güven seviyesi: sensitivity + tekrar sayısına göre basit heuristik
        # Gerçek confidence scoring Adım 2'de embedding tabanlı yapılacak
        confidence = 1 if not sensitive else 2

        chunks.append({
            "chunk_id": f"{conv_id[:8]}_{chunk_index:04d}",
            "source_conv_id": conv_id,
            "text": combined_text,
            "message_count": len(window),
            "topic": topic,
            "language": lang,
            "is_sensitive": sensitive,
            "confidence_tier": confidence,
            "created_at": ts_to_iso(earliest_ts) if earliest_ts else None,
            "latest_at": ts_to_iso(latest_ts) if latest_ts else None,
            "embedding": None,  # Adım 2'de doldurulacak
        })

        i += size - overlap
        chunk_index += 1

    return chunks


# ── Ana pipeline ───────────────────────────────────────────────────────────────

def run():
    all_chunks = []
    json_files = sorted(DATA_DIR.glob("conversations-*.json"))

    if not json_files:
        print(f"[ERROR] {DATA_DIR} altında conversations-*.json bulunamadı.")
        return

    for file_path in json_files:
        print(f"\n[+] Dosya: {file_path.name}")
        with open(file_path, encoding="utf-8") as f:
            conversations = json.load(f)

        print(f"    {len(conversations)} konuşma bulundu")
        file_chunks = []
        total_msgs = 0

        for conv in conversations:
            conv_id = conv.get("id", conv.get("conversation_id", "unknown"))
            messages = extract_user_messages(conv)
            if not messages:
                continue
            total_msgs += len(messages)
            chunks = chunk_messages(messages, conv_id)
            file_chunks.extend(chunks)

        sensitive_count = sum(1 for c in file_chunks if c["is_sensitive"])
        topic_counts = {}
        for c in file_chunks:
            topic_counts[c["topic"]] = topic_counts.get(c["topic"], 0) + 1

        print(f"    {total_msgs} kullanici mesaji -> {len(file_chunks)} chunk")
        print(f"    Hassas chunk: {sensitive_count}")
        print(f"    Konular: {topic_counts}")
        all_chunks.extend(file_chunks)

    # Özet
    print(f"\n{'='*50}")
    print(f"TOPLAM: {len(all_chunks)} chunk")
    print(f"Hassas: {sum(1 for c in all_chunks if c['is_sensitive'])}")
    all_topics = {}
    for c in all_chunks:
        all_topics[c["topic"]] = all_topics.get(c["topic"], 0) + 1
    print(f"Konular: {all_topics}")

    # Kaydet
    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(all_chunks, f, ensure_ascii=False, indent=2)

    print(f"\n[✓] Kaydedildi: {OUTPUT_FILE}")


if __name__ == "__main__":
    run()
