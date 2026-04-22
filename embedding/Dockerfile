FROM python:3.11-slim

WORKDIR /app

RUN pip install --no-cache-dir fastapi uvicorn sentence-transformers

COPY embedding_server.py .

RUN python -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')"

EXPOSE 8000

CMD ["uvicorn", "embedding_server:app", "--host", "0.0.0.0", "--port", "8000"]
