# FCG Local Infra (Gateway + Mensageria + Observabilidade)

> Projeto de pós-graduação em Arquitetura .NET usando .NET 8, Gateway YARP, OpenTelemetry e stack de observabilidade completa (Prometheus + Grafana + Jaeger) com suporte a RabbitMQ e Elastic.

## Visão geral
- Gateway (`gateway/Fcg.Gateway`): ASP.NET Core 8 com YARP, Serilog, health checks e métricas Prometheus. Roteia tráfego para os microsserviços `users`, `games` e `payments`.
- Microsserviços: expostos via Swagger (5001/5002/5003) e comunicando eventos via RabbitMQ.
- Observabilidade: traces exportados via OTLP para Jaeger, métricas expostas em `/metrics` e coletadas pelo Prometheus.

## Requisitos
- Docker e Docker Compose
- .NET 8 SDK (apenas para desenvolvimento local do gateway)

## Como subir tudo
Na raiz do repositório:
```bash
docker compose up -d --build
```

Para parar e limpar:
```bash
docker compose down -v
```

## Endpoints principais
- Gateway: http://localhost:8080
- Users:   http://localhost:5001/swagger
- Games:   http://localhost:5002/swagger
- Payments:http://localhost:5003/swagger
- RabbitMQ UI: http://localhost:15672 (guest/guest)
- Elasticsearch: http://localhost:9200
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000
- Jaeger: http://localhost:16686

## Rotas do Gateway (YARP)
O gateway utiliza YARP para mapear os caminhos externos para os serviços internos (configurados em `gateway/Fcg.Gateway/appsettings.json`):
- `/users/{**catch-all}` ? `fcg-users-api:8080`
- `/auth/{**catch-all}`  ? `fcg-users-api:8080`
- `/admin/{**catch-all}` ? `fcg-users-api:8080`
- `/games/{**catch-all}` ? `fcg-games-api:8080`
- `/search/{**catch-all}`? `fcg-games-api:8080`
- `/recommendations/{**catch-all}` ? `fcg-games-api:8080`
- `/payments/{**catch-all}` ? `fcg-payments-api:8080`

## Observabilidade e saúde
- Health check: `GET http://localhost:8080/health`
- Métricas Prometheus: `GET http://localhost:8080/metrics`
- Raiz do gateway: `GET http://localhost:8080/` retorna identificação do serviço
- Traces: exportados via OTLP para `http://jaeger:4317` (configurável em `OpenTelemetry:OtlpEndpoint`)

## Desenvolvimento do gateway
1. Entre na pasta do gateway:
   ```bash
   cd gateway/Fcg.Gateway
   ```
2. Execute em modo desenvolvimento:
   ```bash
   dotnet run
   ```
3. Ajuste endpoints de destino, rotas YARP e exportação OTLP em `appsettings.json` conforme necessário.

## Arquitetura (Mermaid)
```mermaid
flowchart LR
  Client --> GW[API Gateway (YARP)]
  GW --> U[Users]
  GW --> G[Games]
  GW --> P[Payments]
  G -->|games.purchase.requested| MQ[(RabbitMQ)]
  MQ --> P
  P -->|payments.payment.succeeded| MQ
  MQ --> G
  G --> ES[(Elasticsearch)]
  U --> DB1[(Users DB)]
  G --> DB2[(Games DB)]
  P --> DB3[(Payments DB)]
```

---
jonathan Ornellas
