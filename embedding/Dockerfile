FROM python:3.11-slim

WORKDIR /app

RUN pip install --no-cache-dir \
    fastapi==0.104.1 \
    uvicorn==0.24.0 \
    numpy==1.26.4 \
    torch==2.1.2+cpu \
    transformers==4.36.2 \
    sentence-transformers==2.2.2 \
    --extra-index-url https://download.pytorch.org/whl/cpu

COPY embedding_server.py .

RUN python -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')"

EXPOSE 8000

CMD ["uvicorn", "embedding_server:app", "--host", "0.0.0.0", "--port", "8000"]
