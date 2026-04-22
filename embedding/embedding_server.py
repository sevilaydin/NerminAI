import os
os.environ["TRANSFORMERS_OFFLINE"] = "1"
os.environ["HF_DATASETS_OFFLINE"] = "1"

from fastapi import FastAPI
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer

app = FastAPI()
model = SentenceTransformer("paraphrase-multilingual-MiniLM-L12-v2")


class EmbedRequest(BaseModel):
    texts: list[str]


class EmbedResponse(BaseModel):
    Embeddings: list[list[float]]


@app.post("/embed")
def embed(request: EmbedRequest):
    embeddings = model.encode(request.texts).tolist()
    return {"Embeddings": embeddings}


@app.get("/health")
def health():
    return {"status": "ok"}
