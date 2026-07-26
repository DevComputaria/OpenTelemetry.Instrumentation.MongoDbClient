# ═══════════════════════════════════════════════════════════════
# OpenTelemetry.Instrumentation.MongoDbClient - Makefile
# ═══════════════════════════════════════════════════════════════

COMPOSE_FILE      ?= docker-compose.yml
DOTNET_PROJECT    ?= examples/MongoDbClientSample
OTLP_ENDPOINT     ?= http://localhost:8200
MONGO_CONNECTION  ?= mongodb://localhost:27017

.DEFAULT_GOAL := help

.PHONY: help
help:
	@echo ""
	@echo "╔══════════════════════════════════════════════════════════╗"
	@echo "║  OpenTelemetry MongoDbClient - Makefile                 ║"
	@echo "╚══════════════════════════════════════════════════════════╝"
	@echo ""
	@echo "── Infraestrutura ──────────────────────────"
	@echo "  make infra-up         Iniciar todos os containers"
	@echo "  make infra-down       Parar containers (preserva volumes)"
	@echo "  make infra-destroy    Parar e remover volumes"
	@echo "  make infra-status     Status dos containers"
	@echo "  make infra-logs       Logs de todos os servicos"
	@echo "  make infra-logs-svc   Logs de um servico (SVC=nome)"
	@echo ""
	@echo "── Build & Run ─────────────────────────────"
	@echo "  make build            Compilar o projeto sample"
	@echo "  make run              Iniciar infra + build + sample"
	@echo "  make run-only         Executar sample sem gerenciar infra"
	@echo ""
	@echo "── Health Checks ───────────────────────────"
	@echo "  make check-all        Validar todos os servicos"
	@echo ""
	@echo "── Validacao APM ───────────────────────────"
	@echo "  make validate-all     Buscar traces/metrics no Elasticsearch"
	@echo ""
	@echo "── Limpeza ─────────────────────────────────"
	@echo "  make clean            Limpar artefatos de build"
	@echo "  make full-clean       clean + infra-destroy"

# ─── Infraestrutura ────────────────────────────────────────

.PHONY: infra-up
infra-up:
	@echo ">>> Iniciando infraestrutura (MongoDB + Elastic Stack)..."
	docker compose -f $(COMPOSE_FILE) up -d --wait
	@echo ">>> Infraestrutura pronta!"
	@echo "    MongoDB:     mongodb://localhost:27017"
	@echo "    Elasticsearch: http://localhost:9200"
	@echo "    Kibana:       http://localhost:5601"
	@echo "    APM Server:   http://localhost:8200"

.PHONY: infra-down
infra-down:
	@echo ">>> Parando infraestrutura..."
	docker compose -f $(COMPOSE_FILE) down
	@echo ">>> Infraestrutura parada."

.PHONY: infra-destroy
infra-destroy:
	@echo ">>> Destruindo infraestrutura (volumes serao removidos)..."
	docker compose -f $(COMPOSE_FILE) down -v
	@echo ">>> Volumes removidos."

.PHONY: infra-status
infra-status:
	docker compose -f $(COMPOSE_FILE) ps

.PHONY: infra-logs
infra-logs:
	docker compose -f $(COMPOSE_FILE) logs -f

.PHONY: infra-logs-svc
infra-logs-svc:
ifndef SVC
	$(error SVC is undefined. Use: make infra-logs-svc SVC=mongodb)
endif
	docker compose -f $(COMPOSE_FILE) logs -f $(SVC)

# ─── Build & Run ────────────────────────────────────────────

.PHONY: build
build:
	@echo ">>> Compilando projeto sample..."
	dotnet build $(DOTNET_PROJECT)
	@echo ">>> Compilacao concluida."

.PHONY: run
run: infra-up build
	@echo ">>> Executando sample (OTLP -> $(OTLP_ENDPOINT))..."
	OTEL_SEMCONV_STABILITY_OPT_IN=database \
	OTEL_EXPORTER_OTLP_ENDPOINT=$(OTLP_ENDPOINT) \
	dotnet run --project $(DOTNET_PROJECT)

.PHONY: run-only
run-only:
	@echo ">>> Executando sample (OTLP -> $(OTLP_ENDPOINT))..."
	OTEL_SEMCONV_STABILITY_OPT_IN=database \
	OTEL_EXPORTER_OTLP_ENDPOINT=$(OTLP_ENDPOINT) \
	dotnet run --project $(DOTNET_PROJECT)

# ─── Health Checks ──────────────────────────────────────────

.PHONY: check-all
check-all:
	@echo ">>> Verificando saude dos servicos..."
	@echo ""
	@echo "── MongoDB ──"
	@echo -n "   Status: "; docker compose -f $(COMPOSE_FILE) exec mongodb mongosh --eval "db.adminCommand('ping').ok" --quiet 2>/dev/null || echo "FAIL"
	@echo ""
	@echo "── Elasticsearch ──"
	@curl -s http://localhost:9200/_cluster/health | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'   Status: {d[\"status\"]}')" 2>/dev/null || echo "   Status: FAIL"
	@echo ""
	@echo "── APM Server ──"
	@curl -s http://localhost:8200/ 2>/dev/null | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'   Status: {d.get(\"status\", \"unknown\")}')" 2>/dev/null || echo "   Status: FAIL"
	@echo ""
	@echo "── Kibana ──"
	@curl -s http://localhost:5601/api/status 2>/dev/null | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'   Status: {d[\"status\"][\"overall\"][\"level\"]}')" 2>/dev/null || echo "   Status: FAIL"

# ─── Validacao APM ──────────────────────────────────────────

.PHONY: validate-all
validate-all:
	@echo ">>> Buscando dados APM no Elasticsearch..."
	@echo ""
	@echo "── Traces (ultimos 5 min) ──"
	@curl -s 'http://localhost:9200/traces-apm-*/_search?pretty' \
		-H 'Content-Type: application/json' \
		-d '{"query":{"range":{"@timestamp":{"gte":"now-5m"}}},"size":5,"sort":[{"@timestamp":"desc"}],"_source":["span.name","span.action","service.name","transaction.id"]}' 2>/dev/null \
		| python3 -c "
import sys,json
data = json.load(sys.stdin)
hits = data.get('hits',{}).get('hits',[])
if hits:
    print(f'   Encontrados {len(hits)} spans:')
    for h in hits:
        src = h['_source']
        name = src.get('span',{}).get('name','N/A')
        svc = src.get('service',{}).get('name','N/A')
        print(f'     - {name} (service: {svc})')
else:
    print('   Nenhum trace encontrado nos ultimos 5 min')
    print('   (execute make run primeiro)')
" 2>/dev/null || echo "   Elasticsearch indisponivel"
	@echo ""
	@echo "── Metricas (ultimos 5 min) ──"
	@curl -s 'http://localhost:9200/metrics-*/_search?pretty' \
		-H 'Content-Type: application/json' \
		-d '{"query":{"range":{"@timestamp":{"gte":"now-5m"}}},"size":5,"sort":[{"@timestamp":"desc"}],"_source":["metricset.name","db.system.name","db.operation.name"]}' 2>/dev/null \
		| python3 -c "
import sys,json
data = json.load(sys.stdin)
hits = data.get('hits',{}).get('hits',[])
if hits:
    print(f'   Encontrados {len(hits)} metricas:')
    for h in hits:
        src = h['_source']
        name = src.get('metricset',{}).get('name','N/A')
        db_op = src.get('db',{}).get('operation',{}).get('name','N/A')
        print(f'     - {name} (op: {db_op})')
else:
    print('   Nenhuma metrica encontrada nos ultimos 5 min')
    print('   (execute make run primeiro)')
" 2>/dev/null || echo "   Elasticsearch indisponivel"

# ─── Limpeza ────────────────────────────────────────────────

.PHONY: clean
clean:
	@echo ">>> Limpando artefatos de build..."
	dotnet clean $(DOTNET_PROJECT)
	rm -rf $(DOTNET_PROJECT)/bin $(DOTNET_PROJECT)/obj
	@echo ">>> Limpeza concluida."

.PHONY: full-clean
full-clean: clean infra-destroy
	@echo ">>> Limpeza total concluida."
