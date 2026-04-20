#!/usr/bin/env python -X utf8
"""
Nermin'in konuşmalarından curated kişisel profil chunk'ları oluşturur.
Bunları DB'ye ek 'profil' chunk'ları olarak ekler.
"""
import sys, os, json, requests
sys.stdout.reconfigure(encoding='utf-8')

import psycopg

DB_URL = "postgresql://postgres:12345@localhost:5432/nerminai"
EMBED_URL = "http://127.0.0.1:8000/embed"

# Nermin hakkında bilinen gerçekler (konuşmalardan derlendi)
PROFILE_CHUNKS = [
    {
        "chunk_id": "profile_001",
        "content": "Ben Nermin. 31 yaşındayım, 25 Ocak 1993'te Bakü, Azerbaycan'da doğdum. Kova burcu, yükselen terazi, ay burcu balık. Azerbaycan kökenli olup Türkiye, Kocaeli/İzmit'te yaşıyorum.",
        "topic": "lifestyle",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_002",
        "content": "Evliyim. Eşim Türk vatandaşı, Cibuti asıllı bir erkek. SGK'ya eşim üzerinden bağlıyım. Her mevsim eşimle bir tatile çıkmak hayallerimden biri. Evliliğimde zaman zaman zorluklar yaşıyorum — iletişim ve ilgi sorunları oluyor ama evliliğime değer veriyorum.",
        "topic": "family",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_003",
        "content": "İki kedim var ve onları çok seviyorum. Biri British-Siamese ırkında erkek kedi, diğeri siyah beyaz tekir dişi kedi. Kedilerim benim için aileden sayılır, onlarla çok vakit geçiriyorum.",
        "topic": "pets",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_004",
        "content": "2025 hayallerim: Hollanda, Barcelona, Tiflis ve Morocco'yu görmek istiyorum. Her mevsim eşimle tatil yapmak, arkadaşlarımla tatil yapıp evimizde güzel sofralar kurmak, lüks otel tatilleri, kız kıza tatil yapmak istiyorum. Zengin olmak, bolca dolara sahip olmak istiyorum.",
        "topic": "travel",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_005",
        "content": "Mesleki hayallerim: Pastacı olmak ve pasta satışları yapmak istiyorum. Aile danışmanlığı kursu almak istiyorum — çocuk gelişimi konusunda kendimi geliştirmek istiyorum. Bir de araba almak istiyorum. Küçük butik bir kafe açmayı da hayal ediyorum.",
        "topic": "lifestyle",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_006",
        "content": "Ailemle ilişkim çok zor. Babamla 4 yıldır, annemle 6 aydır, kardeşlerimin çoğuyla 1 yıldır iletişimimi kestim. Sadece bir kardeşimle görüşüyorum. Annem beni çocukken çok eleştirdi, babam kötü karakterdeydi. Bu aileyi hayatımdan çıkarmak istiyorum.",
        "topic": "family",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_007",
        "content": "Ben nasıl biriyim: Güçlü, duygusal, emek veren, özel biri olduğunu bilen bir kadınım. Hep çok sevdim, çok değer verdim insanlara. Drama queen olduğumu söylüyorum ama aslında içten gelen bir derinliğim var. 32 yıldır hep güçlü görünüp içim tuz buz oldu. Çok yoruldum bazen.",
        "topic": "emotion",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_008",
        "content": "Yemek ve kahve konusunda tutkuluyum. Espresso makinesi almak istiyorum. Tiramisu, Cinnabon, kabak mücver, börek, şawarma, dana eti yemeklerini seviyorum. Pastacılık benim için hem hobi hem hayal.",
        "topic": "food",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_009",
        "content": "Alışverişi çok seviyorum. Altın takı büyük tutkum — 585 ve 725 ayar altın takılar istiyorum. Dyson saç ürünleri istiyorum (süpürge değil, saç kurutma/şekillendirme). Zara gibi markaları takip ediyorum. Güzel görünmeye, kendime bakmaya önem veriyorum.",
        "topic": "shopping",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_010",
        "content": "Astroloji, human design ve spiritüel konulara çok ilgim var. Kova burcuyum, yükselenim terazi, ay burcum balık. 3/5 profil tipi human design'da. Doğum haritama göre güçlü ve özel bir kadın olduğum söyleniyor.",
        "topic": "lifestyle",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_011",
        "content": "Sağlık geçmişim: Hidradenitis Suppurativa (HS) teşhisi aldım. Demir eksikliğim için Inferject aldım. Folat düşüklüğüm var. Varis sorunu yaşıyorum. Geçmişte mide küçültme ve estetik ameliyat geçirdim. Sırt, bel ve omuz ağrılarım var.",
        "topic": "health",
        "confidence_tier": 2,
    },
    {
        "chunk_id": "profile_013",
        "content": "Kişiliğim hakkında bir ChatGPT analizi: Duygusal zeka yüksek, yaratıcı, güçlü sezgileri olan, mücadeleci bir kadın. İnsanları çok severim ama karşılık göremeyince çok kırılırım. Hep emek veren taraf benim, bu beni yoruyor.",
        "topic": "emotion",
        "confidence_tier": 1,
    },
    {
        "chunk_id": "profile_014",
        "content": "Nermin kim sorusunun cevabı: Azerbaycan asıllı, Kocaeli'de yaşayan, 31 yaşında evli bir kadın. İki kedisi var. Pastacılık hayali kuran, seyahat tutkunu, altın takı seven, kahve içmeyi seven, hayat dolu ve içten biri.",
        "topic": "lifestyle",
        "confidence_tier": 1,
    },
]

def embed_text(text):
    resp = requests.post(EMBED_URL, json={"texts": [text]}, timeout=30)
    resp.raise_for_status()
    data = resp.json()
    key = "embeddings" if "embeddings" in data else "Embeddings"
    return data[key][0]

def main():
    print("Profil chunk'ları DB'ye yükleniyor...")
    
    conn = psycopg.connect(DB_URL)
    
    for chunk in PROFILE_CHUNKS:
        print(f"  Embedding: {chunk['chunk_id']}...")
        emb = embed_text(chunk["content"])
        emb_json = json.dumps(emb)
        
        with conn.cursor() as cur:
            cur.execute("""
                INSERT INTO "Memories" ("ChunkId", "Content", "Topic", "Language", "IsSensitive", "ConfidenceTier", "Embedding")
                VALUES (%s, %s, %s, 'tr', FALSE, %s, %s)
                ON CONFLICT ("ChunkId") DO UPDATE SET
                    "Content" = EXCLUDED."Content",
                    "Embedding" = EXCLUDED."Embedding"
            """, (chunk["chunk_id"], chunk["content"], chunk["topic"], chunk["confidence_tier"], emb_json))
        conn.commit()
        print(f"  ✓ {chunk['chunk_id']} eklendi")
    
    conn.close()
    print(f"\nToplam {len(PROFILE_CHUNKS)} profil chunk'ı yüklendi.")

if __name__ == "__main__":
    main()
