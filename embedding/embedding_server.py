import os
os.environ["TRANSFORMERS_OFFLINE"] = "1"

from fastapi import FastAPI
from pydantic import BaseModel
from tokenizers import Tokenizer
from onnxruntime import InferenceSession
import numpy as np
import urllib.request
import pathlib
import json

app = FastAPI()

MODEL_DIR = pathlib.Path("/app/model")
ONNX_PATH = MODEL_DIR / "model.onnx"
TOKENIZER_PATH = MODEL_DIR / "tokenizer.json"
SPECIAL_TOKENS_PATH = MODEL_DIR / "special_tokens_map.json"
TOKENIZER_CONFIG_PATH = MODEL_DIR / "tokenizer_config.json"

tokenizer = None
session = None

BASE_URL = "https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/resolve/main"

def download(url, dest):
    if not dest.exists():
        print(f"Downloading {dest.name}...")
        urllib.request.urlretrieve(url, dest)

def load_model():
    global tokenizer, session
    MODEL_DIR.mkdir(exist_ok=True)
    download(f"{BASE_URL}/onnx/model.onnx", ONNX_PATH)
    download(f"{BASE_URL}/tokenizer.json", TOKENIZER_PATH)
    tokenizer = Tokenizer.from_file(str(TOKENIZER_PATH))
    tokenizer.enable_padding(pad_id=0, pad_token="[PAD]")
    tokenizer.enable_truncation(max_length=128)
    session = InferenceSession(str(ONNX_PATH))

load_model()

def mean_pooling(token_embeddings, attention_mask):
    mask = attention_mask[..., np.newaxis].astype(float)
    return (token_embeddings * mask).sum(axis=1) / mask.sum(axis=1)

def normalize(v):
    norm = np.linalg.norm(v, axis=1, keepdims=True)
    return v / np.maximum(norm, 1e-10)

class EmbedRequest(BaseModel):
    texts: list[str]

class EmbedResponse(BaseModel):
    Embeddings: list[list[float]]

@app.post("/embed")
def embed(request: EmbedRequest):
    encodings = tokenizer.encode_batch(request.texts)
    input_ids = np.array([e.ids for e in encodings], dtype=np.int64)
    attention_mask = np.array([e.attention_mask for e in encodings], dtype=np.int64)
    token_type_ids = np.zeros_like(input_ids)
    outputs = session.run(None, {
        "input_ids": input_ids,
        "attention_mask": attention_mask,
        "token_type_ids": token_type_ids,
    })
    embeddings = mean_pooling(outputs[0], attention_mask)
    embeddings = normalize(embeddings)
    return {"Embeddings": embeddings.tolist()}

@app.get("/health")
def health():
    return {"status": "ok"}
