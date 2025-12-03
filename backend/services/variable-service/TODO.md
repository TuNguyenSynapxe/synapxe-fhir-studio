
# TODO – variable-service

## 1. Purpose
Implement Phase 1 functionality for **variable-service** using .NET 8 and OpenAPI contract.

## 2. Setup
- Ensure .NET 8 SDK installed
- Ensure `.env` loaded from repo root
- Add project reference to shared models if applicable

## 3. Implementation Tasks
- Implement endpoints exactly as defined in openapi/variable-service.openapi.yaml
- Generate Controllers, DTOs, and Services
- Add validation (FluentValidation or manual)
- Implement core logic (Copilot will scaffold)

## 4. Testing
- Create xUnit project
- Write unit tests
- Write integration tests using TestServer

## 5. Deployment
- Create Dockerfile
- Add service to docker-compose.yml

## 6. Copilot Prompts
Use inside VS Code:

**Prompt 1:**  
"Using openapi/variable-service.openapi.yaml, generate controller, models, and DI setup."

**Prompt 2:**  
"Create integration tests for all endpoints in variable-service."
