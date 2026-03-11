# RAG Services Backend

This project provides a RAG (Retrieval-Augmented Generation) backend service using Qdrant for vector storage and Ollama for language model inference.

## Getting Started

Follow these steps to run the application:

### Start Qdrant
```bash
docker compose up -d
```

### Ollama Serve
```bash
ollama serve
```

Start Ollama
```bash
ollama run llama3
```

### Ingest Documents

1. Place your files in the `docs/` folder
2. Call endpoint /ingest:
```bash
GET http://localhost:5000/ingest
```

### Call /ask endpoint

Call endpoint /ask
```bash
POST http://localhost:5000/ingest

{
  "question": "Can I return after 14 days?"
}
```

example of generated asnwer by Ollama based on data fdrom qdrant:

```bash
{
  "answer": "Yes, you can return the product. The refund policy states that customers can return products within 69 days, and since 14 days is less than 69 days, you are still within the allowed timeframe for a return."
}
```

## Prerequisites

- Docker (for Qdrant)
- Ollama
- .NET SDK
- Documents to ingest in the `docs/` folder

## Architecture

The project consists of multiple services:
- **Qdrant**: Vector database for storing document embeddings
- **Ollama**: Local language model for generating responses
- **Ingest Service**: Processes and stores documents
- **API Service**: Provides REST endpoints for querying
- **Chat UI**: User interface for interacting with the system