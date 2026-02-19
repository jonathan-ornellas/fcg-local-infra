# FCG — Kubernetes (EKS) Deploy Guide

Repositório com todos os manifests Kubernetes para o projeto FIAP Cloud Games no Amazon EKS.

## Estrutura

```
fcg-k8s/
├── namespace/          # Namespace fcg
├── configmaps/         # Configs não-sensíveis por serviço
├── secrets/            # Secrets (JWT, DB, RabbitMQ) — NÃO commitar com valores reais
├── deployments/        # Deployments dos 4 serviços
├── services/           # ClusterIP + LoadBalancer
├── hpa/                # Horizontal Pod Autoscaler (requer metrics-server)
├── ingress/            # ALB Ingress via AWS Load Balancer Controller
├── jobs/               # Migration Jobs (EF Core)
└── helm-values/        # Values para RabbitMQ, Prometheus+Grafana, Jaeger
```

---

## Pré-requisitos

- AWS CLI configurado com permissões EKS
- `kubectl` apontando para o cluster
- `helm` v3+
- Cluster EKS criado (mínimo: 2 nodes `t3.medium`)
- Amazon ECR com as imagens dos 4 serviços

---

## Passo a Passo

### 1. Criar o cluster EKS (se ainda não existe)

```bash
eksctl create cluster \
  --name fcg-cluster \
  --region us-east-1 \
  --nodegroup-name fcg-nodes \
  --node-type t3.medium \
  --nodes-min 2 \
  --nodes-max 6 \
  --managed
```

### 2. Instalar o metrics-server (obrigatório para HPA)

```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

### 3. Instalar o AWS Load Balancer Controller (para Ingress ALB)

```bash
helm repo add eks https://aws.github.io/eks-charts
helm install aws-load-balancer-controller eks/aws-load-balancer-controller \
  -n kube-system \
  --set clusterName=fcg-cluster \
  --set serviceAccount.create=true \
  --set serviceAccount.name=aws-load-balancer-controller
```

### 4. Criar o Namespace

```bash
kubectl apply -f namespace/namespace.yaml
```

### 5. Instalar dependências via Helm

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add jaegertracing https://jaegertracing.github.io/helm-charts
helm repo update

# RabbitMQ
helm install rabbitmq bitnami/rabbitmq -n fcg -f helm-values/rabbitmq-values.yaml

# Prometheus + Grafana
helm install monitoring prometheus-community/kube-prometheus-stack -n fcg -f helm-values/prometheus-stack-values.yaml

# Jaeger
helm install jaeger jaegertracing/jaeger -n fcg -f helm-values/jaeger-values.yaml
```

### 6. Configurar Secrets

> ⚠️ Edite o arquivo `secrets/secrets.yaml` substituindo os valores base64 pelos seus reais antes de aplicar.

Para gerar base64:
```bash
echo -n "minha-string-secreta" | base64
```

```bash
kubectl apply -f secrets/secrets.yaml
```

### 7. Aplicar ConfigMaps

```bash
kubectl apply -f configmaps/
```

### 8. Fazer o build e push das imagens para o ECR

```bash
# Autenticar no ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin <ACCOUNT_ID>.dkr.ecr.us-east-1.amazonaws.com

# Para cada serviço (repita trocando o nome):
docker build -t fcg-users-service .
docker tag fcg-users-service:latest <ACCOUNT_ID>.dkr.ecr.us-east-1.amazonaws.com/fcg-users-service:latest
docker push <ACCOUNT_ID>.dkr.ecr.us-east-1.amazonaws.com/fcg-users-service:latest
```

> Lembre de substituir `<SEU_ECR>` nos arquivos de deployment pelo seu endpoint real do ECR.

### 9. Rodar as Migrations (antes dos Deployments)

```bash
kubectl apply -f jobs/migration-jobs.yaml

# Aguardar os 3 jobs completarem
kubectl wait --for=condition=complete job/fcg-users-migration -n fcg --timeout=120s
kubectl wait --for=condition=complete job/fcg-games-migration -n fcg --timeout=120s
kubectl wait --for=condition=complete job/fcg-payments-migration -n fcg --timeout=120s
```

### 10. Aplicar Deployments e Services

```bash
kubectl apply -f deployments/
kubectl apply -f services/services.yaml
```

### 11. Aplicar HPA e Ingress

```bash
kubectl apply -f hpa/hpa.yaml
kubectl apply -f ingress/ingress.yaml
```

---

## Verificando o deploy

```bash
# Ver todos os pods
kubectl get pods -n fcg

# Ver HPAs
kubectl get hpa -n fcg

# Ver o endereço do ALB (aguarde ~2 min para provisionar)
kubectl get ingress -n fcg

# Logs de um serviço
kubectl logs -l app=fcg-users -n fcg --tail=50

# Ver uso de CPU/memória dos pods
kubectl top pods -n fcg
```

## Acessando Grafana e Jaeger (port-forward local)

```bash
# Grafana (admin / senha configurada no helm-values)
kubectl port-forward svc/monitoring-grafana 3000:80 -n fcg

# Jaeger UI
kubectl port-forward svc/jaeger-query 16686:16686 -n fcg

# RabbitMQ Management
kubectl port-forward svc/rabbitmq 15672:15672 -n fcg
```

---

## Notas importantes

- **Secrets**: nunca commite `secrets/secrets.yaml` com valores reais. Considere usar [External Secrets Operator](https://external-secrets.io/) com AWS Secrets Manager em produção.
- **Migrations**: os Jobs usam `--migrate-only`. Você pode precisar adicionar suporte a esse flag na sua API, ou alternativamente usar `dotnet ef database update` diretamente como command.
- **Dockerfile**: certifique-se de que os Dockerfiles usam multi-stage build com imagem base `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` no estágio final e `USER app` para rodar sem root.
- **HPA**: para funcionar, cada Deployment precisa ter `resources.requests` definido (já configurado nos yamls).
