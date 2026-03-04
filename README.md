# Prueba Yape - Demo Examen

Stack local para el challenge con:
- `api-transaction` (API .NET)
- `listenet-antifraud` (worker .NET consumidor de Kafka)
- `kafka` (KRaft, sin ZooKeeper)
- `kafka-ui` (GUI para validar mensajes)
- `postgres`

## ⚡ Redeploy rápido

```bash
# Ambos servicios (build + reinicio)
docker compose up -d --build api-transaction listenet-antifraud

# Solo API
docker compose up -d --build api-transaction

# Solo worker antifraud
docker compose up -d --build listenet-antifraud

# Ver logs en tiempo real de ambos
docker compose logs -f api-transaction listenet-antifraud
```

---

## 1) Levantar todo desde Dev Container

1. Abrir el proyecto en VS Code.
2. Ejecutar: **Dev Containers: Rebuild and Reopen in Container**.
3. El `devcontainer` levanta automáticamente los servicios del `docker-compose`.

> Si quieres validar manualmente dentro del contenedor:

```bash
docker compose ps
```

## 2) Endpoints de prueba

- Health API:

```bash
curl -s http://localhost:${API_TRANSACTION_PORT:-8080}/health
```

- Crear transacción (publica en Kafka):

```bash
curl -s -X POST http://localhost:${API_TRANSACTION_PORT:-8080}/transactions \
  -H "Content-Type: application/json" \
  -d '{"sourceAccountId":"11111111-1111-1111-1111-111111111111","targetAccountId":"22222222-2222-2222-2222-222222222222","tranferTypeId":1,"value":3000}'
```

- Response:

```json
{
  "transactionExternalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2026-03-03T20:00:00Z"
}
```

- Criterios de envío a antifraud (`pending`):
  - Toda transacción con `value > 2000`
  - Acumulado diario por cuenta origen `> 20000`

- Estados manejados:
  - `pending`
  - `approved`
  - `rejected`

## 3) Validar en Kafka UI (GUI)

1. Abrir en el navegador:

```text
http://localhost:${KAFKA_UI_PORT:-8081}
```

2. Entrar al cluster `local`.
3. Ir al topic `transactions.created`.
4. Verificar mensajes producidos por `api-transaction`.

## 4) Validar consumo del listener antifraud

```bash
docker compose logs listenet-antifraud --tail=100
```

Debes ver logs tipo:
- `Listening topic transactions.created on kafka:29092`
- `Transaction ... evaluated as flagged` (monto alto)
- `Transaction ... evaluated as approved` (monto bajo)

## 5) Variables de entorno

Se encuentran en `.env`:
- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_HOST_PORT`
- `KAFKA_HOST_PORT`, `KAFKA_TOPIC`, `KAFKA_CONSUMER_GROUP`
- `API_TRANSACTION_PORT`, `KAFKA_UI_PORT`

## 6) Compilar y actualizar contenedores

### `api-transaction` (TransactionService)

**1. Validar build localmente** (antes de reconstruir la imagen):
```bash
cd src/TransactionService
dotnet build -c Release
```

**2. Generar migración de BD** (solo si se modificó el modelo de datos):
```bash
cd src/TransactionService
dotnet ef migrations add <NombreMigracion> \
  --project Api.Transaction.Infrastructure \
  --startup-project Api.Transaction
# Las migraciones se aplican automáticamente al iniciar el contenedor (MigrateAsync)
```

**3. Reconstruir imagen y reiniciar contenedor**:
```bash
docker compose up -d --build api-transaction
```

**4. Verificar logs de startup**:
```bash
docker compose logs api-transaction --tail=50
# Buscar: "Database migrations applied successfully."
```

**5. Verificar Kafka desde el API**:
```bash
curl -s http://localhost:${API_TRANSACTION_PORT:-8080}/health/kafka
# HTTP 200 → Kafka up, lista de brokers
# HTTP 503 → Kafka unreachable, mensaje de error
```

**6. Seguir logs en tiempo real**:
```bash
docker compose logs -f api-transaction
```

---

### `listenet-antifraud` (AntiFraudService)

**1. Validar build localmente**:
```bash
cd src/AntiFraudService
dotnet build -c Release
```

**2. Reconstruir imagen y reiniciar contenedor**:
```bash
docker compose up -d --build listenet-antifraud
```

**3. Verificar logs de startup**:
```bash
docker compose logs listenet-antifraud --tail=50
# Buscar: "Anti-fraud worker started"
```

**4. Seguir logs en tiempo real**:
```bash
docker compose logs -f listenet-antifraud
```

---

### Reconstruir ambos servicios a la vez

```bash
docker compose up -d --build api-transaction listenet-antifraud
```

**Seguir logs de ambos simultáneamente**:
```bash
docker compose logs -f api-transaction listenet-antifraud
```

**Ver estado de todos los contenedores**:
```bash
docker compose ps
```

---

## 7) Consultar estado de una transacción

Luego de crear la transacción, puedes consultar su estado actualizado:

```bash
curl -s http://localhost:${API_TRANSACTION_PORT:-8080}/transactions/<transactionExternalId>
```

Response:
```json
{
  "transactionExternalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "rejected",
  "createdAt": "2026-03-03T20:00:00Z",
  "updatedAt": "2026-03-03T20:00:01Z"
}
```

---

## 8) Reinicio rápido de toda la infraestructura

```bash
docker compose up -d --build
```

## 9) Apagar stack

```bash
docker compose down
```
