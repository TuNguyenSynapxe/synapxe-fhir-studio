
# Infrastructure Architecture v1.0
## Overview
Describes compute layout, networking, scaling, load balancing, storage, FHIR package hosting, and Firely validator deployment.
## Compute
- Azure App Service for backend APIs
- Static Web Apps for frontend
- Container support in later phases
## Networking
- Private endpoints recommended
- API Gateway ingress
## Storage
- PostgreSQL for metadata
- Blob Storage for FHIR packages
