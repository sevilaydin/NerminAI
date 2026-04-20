"""
NerminAI — Embedding Pipeline
Adim 2: chunks.json -> embedding_server.py -> chunks_embedded.json
"""

import json
import time
from pathlib import Path

import requests

# ── Konfigurasyon ──────────────────────────────────────────────────────────────

INPUT_FILE = Path(__file__).parent / "chunks.json"
OUTPUT_FILE = Path(__file__).parent / "chunks_embedded.json"
EMBEDDING_URL = "http://127.0.0.1:8000/embed"
BATCH_SIZE = 32  # tek seferde kac chunk embed edilecek

# ── Yardimci ──────────────────────────────────────────────────────────────────

def embed_batch(texts: list[str]) -> list[list[float]]:
    resp = requests.post(EMBEDDING_URL, json={"texts": texts}, timeout=60)
    resp.raise_for_status()
    return resp.json()["Embeddings"]


# ── Ana pipeline ───────────────────────────────────────────────────────────────

def run():
    with open(INPUT_FILE, encoding="utf-8") as f:
        chunks = json.load(f)

    print(f"[+] {len(chunks)} chunk yuklendu")

    # Zaten embedding'i olanları atla (resume desteği)
    to_embed = [i for i, c in enumerate(chunks) if not c.get("embedding")]
    print(f"[+] {len(to_embed)} chunk embed edilecek")

    start = time.time()
    done = 0

    for batch_start in range(0, len(to_embed), BATCH_SIZE):
        batch_indices = to_embed[batch_start : batch_start + BATCH_SIZE]
        texts = [chunks[i]["text"] for i in batch_indices]

        embeddings = embed_batch(texts)

        for idx, emb in zip(batch_indices, embeddings):
            chunks[idx]["embedding"] = emb

        done += len(batch_indices)
        elapsed = time.time() - start
        rate = done / elapsed if elapsed > 0 else 0
        remaining = (len(to_embed) - done) / rate if rate > 0 else 0
        print(f"    {done}/{len(to_embed)} chunk  |  {rate:.1f} chunk/s  |  ~{remaining:.0f}s kaldi")

        # Ara kayit (her 10 batch'te bir)
        if (batch_start // BATCH_SIZE) % 10 == 0:
            with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
                json.dump(chunks, f, ensure_ascii=False)

    # Final kayit
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(chunks, f, ensure_ascii=False, indent=2)

    elapsed = time.time() - start
    dims = len(chunks[0]["embedding"]) if chunks[0].get("embedding") else 0
    print(f"\n[OK] Tamamlandi: {len(chunks)} chunk, {dims}d vektor, {elapsed:.1f}s")
    print(f"[OK] Kaydedildi: {OUTPUT_FILE}")


if __name__ == "__main__":
    run()
