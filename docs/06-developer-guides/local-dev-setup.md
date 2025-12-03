# Local Development Setup
Synapxe FHIR Studio — Wave 4

## 1. System Requirements
- Node.js 20+
- .NET 8 SDK
- Docker Desktop
- PostgreSQL + pgvector
- Redis (optional)
- Azure CLI (for deployment)

## 2. Clone Repository
```
git clone <repo>
cd synapxe-fhir-studio
```

## 3. Start Local Infrastructure
```
docker compose up -d
```

Includes:
- Postgres
- Vector DB
- API gateway mock
- Local microservices

## 4. Run Frontend
```
npm install
npm run dev
```

## 5. Run Backend Microservices
```
cd backend/services/<service-name>
dotnet run
```

## 6. Testing
```
npm test
dotnet test
```

## 7. Troubleshooting
### 7.1 Docker Memory
Increase to 4–6GB if services restart.

### 7.2 Postgres Connection Issues
Reset using:
```
docker compose down -v
docker compose up -d
```

### 7.3 SSL issues on macOS
Disable strict SSL for local dev only:
```
DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
```
