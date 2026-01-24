# FCG Local Infra (Gateway + RabbitMQ + Elastic + Observability)

## Run everything
From this folder:
```bash
docker compose up -d --build
```

## Endpoints
- Gateway: http://localhost:8080
- Users:   http://localhost:5001/swagger
- Games:   http://localhost:5002/swagger
- Payments:http://localhost:5003/swagger
- RabbitMQ UI: http://localhost:15672 (guest/guest)
- Elasticsearch: http://localhost:9200
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000
- Jaeger: http://localhost:16686

## Architecture (Mermaid)
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
