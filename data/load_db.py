"""
NerminAI — Ingestion Service
Adim 3: chunks_embedded.json -> PostgreSQL (Memories tablosu)
"""

import json
import time
from pathlib import Path

import psycopg

# ── Konfigurasyon ──────────────────────────────────────────────────────────────

INPUT_FILE = Path(__file__).parent / "chunks_embedded.json"

DB_CONFIG = dict(
    host="127.0.0.1",
    port=5432,
    dbname="nerminai",
    user="postgres",
    password="12345",
)

BATCH_SIZE = 50


# ── Ana pipeline ───────────────────────────────────────────────────────────────

def run():
    with open(INPUT_FILE, encoding="utf-8") as f:
        chunks = json.load(f)

    print(f"[+] {len(chunks)} chunk yuklendu")

    conn = psycopg.connect(**DB_CONFIG)
    cur = conn.cursor()

    # Zaten var olan chunk_id'leri cek (resume destegi)
    cur.execute('SELECT "ChunkId" FROM "Memories"')
    existing = {row[0] for row in cur.fetchall()}
    print(f"[+] DB'de zaten {len(existing)} chunk var, atlanacak")

    to_insert = [c for c in chunks if c["chunk_id"] not in existing]
    print(f"[+] {len(to_insert)} chunk eklenecek")

    if not to_insert:
        print("[OK] Hepsi zaten yuklu, islem yok.")
        conn.close()
        return

    inserted = 0
    start = time.time()

    for i in range(0, len(to_insert), BATCH_SIZE):
        batch = to_insert[i : i + BATCH_SIZE]

        rows = []
        for c in batch:
            embedding_json = json.dumps(c["embedding"]) if c.get("embedding") else None
            rows.append((
                c["chunk_id"],
                c.get("source_conv_id"),
                c["text"],
                c.get("topic"),
                c.get("language"),
                c.get("is_sensitive", False),
                c.get("confidence_tier", 2),
                embedding_json,
                c.get("created_at"),
                c.get("latest_at"),
            ))

        cur.executemany(
            '''
            INSERT INTO "Memories"
                ("ChunkId", "SourceConvId", "Content", "Topic", "Language",
                 "IsSensitive", "ConfidenceTier", "Embedding", "CreatedAt", "LatestAt")
            VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
            ON CONFLICT ("ChunkId") DO NOTHING
            ''',
            rows,
        )
        conn.commit()

        inserted += len(batch)
        elapsed = time.time() - start
        print(f"    {inserted}/{len(to_insert)} chunk eklendi  ({elapsed:.1f}s)")

    # Ozet
    cur.execute('SELECT COUNT(*) FROM "Memories"')
    total = cur.fetchone()[0]
    cur.execute('SELECT COUNT(*) FROM "Memories" WHERE "IsSensitive" = TRUE')
    sensitive = cur.fetchone()[0]
    cur.execute('SELECT "Topic", COUNT(*) FROM "Memories" GROUP BY "Topic" ORDER BY COUNT(*) DESC')
    topics = cur.fetchall()

    print(f"\n[OK] Tamamlandi!")
    print(f"     Toplam DB'de: {total} chunk")
    print(f"     Hassas:       {sensitive}")
    print(f"     Konular:      {dict(topics)}")

    conn.close()


if __name__ == "__main__":
    run()
