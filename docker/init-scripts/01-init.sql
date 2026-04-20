-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Documents table
CREATE TABLE IF NOT EXISTS "Documents" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Title" VARCHAR(500) NOT NULL,
    "Content" TEXT NOT NULL,
    "Type" INTEGER NOT NULL,
    "Metadata" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);

-- Chunks table
CREATE TABLE IF NOT EXISTS "Chunks" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DocumentId" UUID NOT NULL REFERENCES "Documents"("Id") ON DELETE CASCADE,
    "Content" TEXT NOT NULL,
    "ChunkIndex" INTEGER NOT NULL,
    "TokenCount" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);

-- Embeddings table
CREATE TABLE IF NOT EXISTS "Embeddings" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ChunkId" UUID NOT NULL UNIQUE REFERENCES "Chunks"("Id") ON DELETE CASCADE,
    "Vector" vector(384) NOT NULL,
    "Model" VARCHAR(100) NOT NULL DEFAULT 'all-MiniLM-L6-v2',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);

-- Indexes
CREATE INDEX IF NOT EXISTS "IX_Documents_Type" ON "Documents"("Type");
CREATE INDEX IF NOT EXISTS "IX_Documents_CreatedAt" ON "Documents"("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Chunks_DocumentId" ON "Chunks"("DocumentId");
CREATE INDEX IF NOT EXISTS "IX_Chunks_DocumentId_ChunkIndex" ON "Chunks"("DocumentId", "ChunkIndex");
CREATE INDEX IF NOT EXISTS "IX_Embeddings_ChunkId" ON "Embeddings"("ChunkId");

-- IVFFlat index for vector similarity search
CREATE INDEX IF NOT EXISTS "IX_Embeddings_Vector" ON "Embeddings"
USING ivfflat ("Vector" vector_cosine_ops) WITH (lists = 10);
