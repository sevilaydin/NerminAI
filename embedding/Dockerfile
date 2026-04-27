FROM python:3.11-slim

WORKDIR /app

RUN pip install --no-cache-dir \
    fastapi \
    uvicorn \
    onnxruntime \
    tokenizers \
    numpy

RUN mkdir -p /app/model && \
    python -c "\
import urllib.request; \
urllib.request.urlretrieve('https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/resolve/main/onnx/model.onnx', '/app/model/model.onnx'); \
urllib.request.urlretrieve('https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/resolve/main/tokenizer.json', '/app/model/tokenizer.json'); \
print('Models downloaded successfully')"

COPY embedding_server.py .

EXPOSE 8000

CMD ["uvicorn", "embedding_server:app", "--host", "0.0.0.0", "--port", "8000"]
