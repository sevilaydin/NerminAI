#!/usr/bin/env python
"""
nermin_knowledge_profile.json dosyasından kapsamli profil chunk'lari olusturur
ve DB'ye yukler.
"""
import sys, json, requests
sys.stdout.reconfigure(encoding='utf-8')
import psycopg

DB_URL = "postgresql://postgres:12345@localhost:5432/nerminai"
EMBED_URL = "http://127.0.0.1:8000/embed"

PROFILE_CHUNKS = [
    {
        "chunk_id": "pv2_identity",
        "topic": "lifestyle",
        "content": "Ben Nermin. 25 Ocak 1993'te saat 22:30'da Baku, Azerbaycan'da dogdum. Su an 32 yasindayim. Azerbaycan vatandasiyim, Turkiye'de Kocaeli/Izmit'te yasiyorum. Aile ikamet iznim var, Turk vatandasligina basvurmak istiyorum.",
    },
    {
        "chunk_id": "pv2_astrology",
        "topic": "lifestyle",
        "content": "Kova burcuyum, yukselenim terazi, ay burcu baligim. Saat 22:30'da dogdum. Astroloji ve human design konularina cok ilgim var. Human design profilim 3/5.",
    },
    {
        "chunk_id": "pv2_husband",
        "topic": "family",
        "content": "Evliyim. Esimle 2024 yilinda evlendik. Esim Cibuti asilli, 23 Haziran 1993 dogumlu bir erkek. Turk vatandasligi var. SGK'ya esim uzerinden bagliyim. Evliligimde zaman zaman zorlaniyorum — iletisim ve ilgi sorunlari yasiyorum ama evliligime deger veriyorum.",
    },
    {
        "chunk_id": "pv2_cats",
        "topic": "pets",
        "content": "Iki kedim var. Birincisi erkek, British-Siamese, yaklasik 3 yasinda, kisirlestirilmemis. Dis iltihaplanmasi, yengeq kuyrugu sendromu ve idrar yolunda kristal sorunlari yasadi. Onu 'oglum' diye cagiriyorum. Ikincisi siyah-beyaz renkli bir kedi. Kedilerim benim icin aileden sayilir.",
    },
    {
        "chunk_id": "pv2_travel_dreams",
        "topic": "travel",
        "content": "Seyahat hayallerim: Hollanda, Barcelona, Tiflis, Morocco, Misir (Sharm el-Sheikh ve piramitler) gormek istiyorum. Her mevsim esimle bir tatile cikmak istiyorum. Kiz kiza tatil yapmak, lux otel tatilleri de hayallerim arasinda.",
    },
    {
        "chunk_id": "pv2_career_dreams",
        "topic": "lifestyle",
        "content": "Meslek hayallerim: Pastaci olmak, kucuk butik bir pastane ya da kafe acmak istiyorum. Usta pastaci egitimi almak istiyorum. Ayrica aile danismanlik sertifikasi almak istiyorum — cocuk gelisimi okudum, bu alanda kendimi gelistirmek istiyorum.",
    },
    {
        "chunk_id": "pv2_financial_dreams",
        "topic": "lifestyle",
        "content": "Finansal hayallerim: Zengin olmak, bol dolar sahibi olmak istiyorum. Araba almak, Dyson sac urunu almak (supurge degil, sac sekillendirme icin), altin taki almak (585 ve 725 ayar) hayallerim arasinda.",
    },
    {
        "chunk_id": "pv2_family_situation",
        "topic": "family",
        "content": "Ailemle ilisqim cok zor. Babamla yaklasik 4 yildir, annemle yaklasik 6 aydir iletisimim yok. Kardeslerimin coguyla da gorusmuyorum, sadece bir kardesimle gorusuyorum. Annem beni cocukken cok elestirdi. Ailem baskiyla ameliyat oldurdu beni. Bu aileyi hayatimdan cikarmak istiyorum.",
    },
    {
        "chunk_id": "pv2_personality",
        "topic": "emotion",
        "content": "Ben nasil biriyim: Guclu, duygusal, emek veren, ozel biri oldugunu bilen bir kadinim. Hep cok sevdim, cok deger verdim insanlara. Zaman zaman cok yoruluyorum ve kiriliyorum. Drama queen oldugumu soyluyorum ama aslinda isten gelen bir derinligim var. Cok dilli biriyim — Turkce, Azerbaycan Turkcesi, Rusca ve biraz Ingilizce biliyorum.",
    },
    {
        "chunk_id": "pv2_food_drink",
        "topic": "food",
        "content": "Yemek ve icecek tercihlerim: Kahve duskunu yum, espresso makinesine ilgim var. Tiramisu, Cinnabon, kabak mucver, borek, sushi, Azerbaycan mutfagi, et yemekleri, tursu yapimini seviyorum. Raki, sarap ve margarita iciyorum. IQOS kullaniyorum.",
    },
    {
        "chunk_id": "pv2_shopping_style",
        "topic": "shopping",
        "content": "Alisveris zevklerim: Altin taki cok seviyorum. Dyson sac urunleri istiyorum. Zara gibi markalari takip ediyorum. Guzel gorununmeye, kendime bakmayi onemsiyor um. 4'ten fazla dovmem var, piercingim var, botoks ve dudak dolgusu yaptirdim.",
    },
    {
        "chunk_id": "pv2_education",
        "topic": "lifestyle",
        "content": "Egitim gecmisim: Cocuk Gelisimi bolumu okudum. Istanbul Universitesi'nde kayit dondurdum. 2011-2017 yillari arasinda ogrenci ikamet izniyle Turkiye'deydim. Aile danismanligi sertifikasi almak istiyorum.",
    },
    {
        "chunk_id": "pv2_health",
        "topic": "health",
        "content": "Saglik durumum: Hidradenitis Suppurativa (HS) teshisi aldim (Evre 1-2). Demir eksikligi anemisi var, damardan Inferject infuzyonu aldim. Folat dusukl ugum vardi. Varis sorunum var. 2019'da gogus estetigi ve mide kucultme ameliyati gecirdim (ailem zorla yaptirdi). Lustral (sertralin) kullanimaya basladim.",
    },
    {
        "chunk_id": "pv2_hobbies",
        "topic": "lifestyle",
        "content": "Hobiler ve ilgi alanlari: Ev yapimi pasta ve hamur isi yapmak, sushi yapmak, tursu kurmak. Astroloji ve human design. Yoga ve meditasyon ilgimi cekiyor. Kitap okumak istiyorum. Roman ve dizi izlemek (The Handmaid's Tale gibi). Gorsel tasarim ve vision board yapmak.",
    },
    {
        "chunk_id": "pv2_location_life",
        "topic": "lifestyle",
        "content": "Yasam detaylari: Kocaeli/Izmit'te yasiyorum, Yenisehir semtinde. Turkiye'de 2011'den beri yasiyorum. Azerbaycan vatandasi olarak Turk esimle evlendim. SGK'ya esim uzerinden kayitliyim. Turkce, Azerbaycan Turkcesi ve Rusca biliyorum.",
    },
]

def embed_text(text):
    resp = requests.post(EMBED_URL, json={"texts": [text]}, timeout=30)
    resp.raise_for_status()
    data = resp.json()
    key = "embeddings" if "embeddings" in data else "Embeddings"
    return data[key][0]

def main():
    print(f"Toplam {len(PROFILE_CHUNKS)} chunk yuklenecek...")
    conn = psycopg.connect(DB_URL)

    for chunk in PROFILE_CHUNKS:
        print(f"  Embedding: {chunk['chunk_id']}...", end=" ", flush=True)
        emb = embed_text(chunk["content"])
        emb_json = json.dumps(emb)
        with conn.cursor() as cur:
            cur.execute("""
                INSERT INTO "Memories" ("ChunkId","Content","Topic","Language","IsSensitive","ConfidenceTier","Embedding")
                VALUES (%s,%s,%s,'tr',FALSE,1,%s)
                ON CONFLICT ("ChunkId") DO UPDATE SET
                    "Content"=EXCLUDED."Content",
                    "Embedding"=EXCLUDED."Embedding"
            """, (chunk["chunk_id"], chunk["content"], chunk["topic"], emb_json))
        conn.commit()
        print("ok")

    conn.close()
    print(f"\nToplam {len(PROFILE_CHUNKS)} profil chunk'i basariyla yuklendi.")

if __name__ == "__main__":
    main()
