from fastapi import FastAPI
from pydantic import BaseModel
from tokenizers import Tokenizer
from onnxruntime import InferenceSession
import numpy as np
import pathlib

app = FastAPI()

MODEL_DIR = pathlib.Path("/app/model")
ONNX_PATH = MODEL_DIR / "model.onnx"
TOKENIZER_PATH = MODEL_DIR / "tokenizer.json"

tokenizer = None
session = None

def load_model():
    global tokenizer, session
    tokenizer = Tokenizer.from_file(str(TOKENIZER_PATH))
    tokenizer.enable_padding(pad_id=0, pad_token="[PAD]")
    tokenizer.enable_truncation(max_length=128)
    session = InferenceSession(str(ONNX_PATH))
    print("Model loaded successfully")

load_model()

def mean_pooling(token_embeddings, attention_mask):
    mask = attention_mask[..., np.newaxis].astype(float)
    return (token_embeddings * mask).sum(axis=1) / mask.sum(axis=1)

def normalize(v):
    norm = np.linalg.norm(v, axis=1, keepdims=True)
    return v / np.maximum(norm, 1e-10)

class EmbedRequest(BaseModel):
    texts: list[str]

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

if __name__ == "__main__":
    import uvicorn
    import os
    port = int(os.environ.get("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)
