# APM Data Compilation — MongoDB Instrumentation

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  Sample App (MongoDbClientSample)                                │
│  dotnet run --project examples/MongoDbClientSample               │
└──────────────┬──────────────────────────────────────────────────┘
               │ OTLP gRPC :4317
               ▼
┌──────────────────────────────┐
│  OpenTelemetry Collector      │
│  otel/opentelemetry-collector │
│  -contrib:0.111.0             │
└──────────────┬────────────────┘
               │ OTLP HTTP :8200
               ▼
┌──────────────────────────────┐
│  Elastic APM Server 8.15.0   │
└──────────────┬────────────────┘
               │ Elasticsearch bulk API :9200
               ▼
┌──────────────────────────────┐
│  Elasticsearch 8.15.0        │
│  Indices:                    │
│  • .ds-traces-apm-*          │
│  • .ds-metrics-apm.app.*     │
└──────────────┬────────────────┘
               │
               ▼
┌──────────────────────────────┐
│  Kibana 8.15.0               │
│  APM UI → /app/apm           │
└──────────────────────────────┘
```

## Traces Enviados (10 spans)

| # | Operação | Coleção | Duração (ms) | Trace ID |
|---|----------|---------|-------------|----------|
| 1 | `isMaster` | `unknown` | 27.6 | `bdcd53a479fb7c182c9a0a44f5dda650` |
| 2 | `drop` | `sample_collection` | 5.7 | `bdcd53...` |
| 3 | `insert` | `sample_collection` | 217.2 | `bdcd53...` |
| 4 | `find` | `sample_collection` | 3.6 | `bdcd53...` |
| 5 | `find` | `sample_collection` | 2.0 | `bdcd53...` |
| 6 | `update` | `sample_collection` | 7.3 | `bdcd53...` |
| 7 | `delete` | `sample_collection` | 2.3 | `bdcd53...` |
| 8 | `find` | `sample_collection` | 1.8 | `bdcd53...` |
| 9 | **Transaction root** `MongoDbSampleOperations` | — | 755.7 | `bdcd53...` |
| 10 | *(test trace)* `test-span` | — | 1000.0 | `0af765...` |

### Atributos por span

```json
{
  "span.name": "insert sample_collection",
  "service.name": "MongoDbClientSample",
  "agent.name": "opentelemetry/dotnet",
  "agent.version": "1.7.0",
  "db.system.name": "mongodb",
  "db.namespace": "sample_db",
  "db.operation.name": "insert",
  "db.collection.name": "sample_collection",
  "server.address": "Unspecified/localhost",
  "server.port": 27017,
  "mongodb.request_id": 6,
  "mongodb.request_duration_ms": 227.8137
}
```

### Hierarquia de spans

```
MongoDbSampleOperations (root, 755.7ms)
├── isMaster unknown (27.6ms)
├── drop sample_collection (5.7ms)
├── insert sample_collection (217.2ms)
├── find sample_collection (3.6ms)
├── find sample_collection (2.0ms)  ← filtered query (value > 50)
├── update sample_collection (7.3ms)
├── delete sample_collection (2.3ms)
└── find sample_collection (1.8ms)  ← final state
```

## Métricas Enviadas (12 entradas)

### `db.client.operation.duration` — Histograma por operação

| Operação | Count | Sum (s) | Min (s) | Max (s) |
|----------|-------|---------|---------|---------|
| `isMaster` | 1 | 0.0387 | 0.0387 | 0.0387 |
| `drop` | 1 | 0.0149 | 0.0149 | 0.0149 |
| `insert` | 1 | 0.2278 | 0.2278 | 0.2278 |
| `find` | 3 | 0.0085 | 0.0021 | 0.0039 |
| `update` | 1 | 0.0113 | 0.0113 | 0.0113 |
| `delete` | 1 | 0.0032 | 0.0032 | 0.0032 |

### Dimensões das métricas

```json
{
  "metricset.name": "app",
  "db.client.operation.duration": { "values": [2.5], "counts": [1] },
  "labels.db_operation_name": "insert",
  "labels.db_response_status_code": "OK",
  "labels.db_system_name": "mongodb",
  "labels.server_address": "Unspecified/localhost:27017"
}
```

## Como reproduzir

```bash
# 1. Subir infraestrutura
make infra-up

# 2. Executar sample app
make run

# 3. Verificar dados no Elasticsearch
curl -u elastic:changeme 'http://localhost:9200/_cat/indices/*apm*'

# 4. Abrir Kibana: http://localhost:5601 (elastic / changeme)
#    Observability → APM → MongoDbClientSample
```

## Como consultar os dados via API

### Traces
```bash
curl -u elastic:changeme \
  'http://localhost:9200/.ds-traces-apm-default-*/_search' \
  -H 'Content-Type: application/json' \
  -d '{"size": 20, "query": {"match_all": {}}}'
```

### Métricas
```bash
curl -u elastic:changeme \
  'http://localhost:9200/.ds-metrics-apm.app.*/ _search' \
  -H 'Content-Type: application/json' \
  -d '{"size": 20, "query": {"match_all": {}}}'
```

## Observações

- O sample executa 8 operações MongoDB dentro de uma transaction raiz (`MongoDbSampleOperations`)
- Cada operação gera 1 span de client (tracing) e 1 entrada de métrica (histograma de duração)
- O pipeline OTLP usa gRPC na porta 4317 (app → collector) e HTTP na porta 8200 (collector → APM Server)
- APM Server 8.15.0 requer segurança habilitada no Elasticsearch para o Fleet/APM integration
- Service account token para Kibana foi gerado via API do Elasticsearch
