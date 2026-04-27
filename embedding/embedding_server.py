import os
os.environ["TRANSFORMERS_OFFLINE"] = "0"

from fastapi import FastAPI
from pydantic import BaseModel
from transformers import AutoTokenizer
from onnxruntime import InferenceSession
import numpy as np
import urllib.request
import pathlib

app = FastAPI()

MODEL_NAME = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
MODEL_DIR = pathlib.Path("/app/model")
ONNX_PATH = MODEL_DIR / "model.onnx"

tokenizer = None
session = None

def load_model():
    global tokenizer, session
    MODEL_DIR.mkdir(exist_ok=True)
    tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME)
    if not ONNX_PATH.exists():
        url = "https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/resolve/main/onnx/model.onnx"
        urllib.request.urlretrieve(url, ONNX_PATH)
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
    encoded = tokenizer(request.texts, padding=True, truncation=True, max_length=128, return_tensors="np")
    outputs = session.run(None, {
        "input_ids": encoded["input_ids"],
        "attention_mask": encoded["attention_mask"],
        "token_type_ids": encoded.get("token_type_ids", np.zeros_like(encoded["input_ids"]))
    })
    embeddings = mean_pooling(outputs[0], encoded["attention_mask"])
    embeddings = normalize(embeddings)
    return {"Embeddings": embeddings.tolist()}

@app.get("/health")
def health():
    return {"status": "ok"}
