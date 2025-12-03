# Schema Parser Service

A .NET 8 microservice that parses legacy schemas (CSV, XML, JSON, XSD) into normalized SchemaDefinition objects.

## Features

- ✅ Parse CSV schemas (header-based)
- ✅ Parse JSON schemas with type inference
- ✅ Parse XML schemas
- ✅ Parse XSD schemas
- ✅ FluentValidation for request validation
- ✅ Correlation ID tracking
- ✅ Structured logging with Serilog
- ✅ Global exception handling
- ✅ Swagger/OpenAPI documentation
- ✅ Health check endpoint

## API Endpoints

### POST `/v1/transform/schema/parse`

Parses a legacy schema and returns a normalized SchemaDefinition.

**Headers:**
- `X-Correlation-Id` (required): Unique identifier for request tracking

**Request Body:**
```json
{
  "sourceType": "csv",
  "name": "PutEvent",
  "content": "EVENTDATE,EVENTTYPE,EVENTID,DESCRIPTION"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
  "data": {
    "name": "PutEvent",
    "sourceType": "csv",
    "fields": [
      {
        "name": "EVENTDATE",
        "dataType": "string",
        "isRequired": false,
        "isArray": false
      },
      ...
    ],
    "metadata": {
      "originalFormat": "csv",
      "fieldCount": 4,
      "parsedAt": "2025-12-02T..."
    }
  },
  "warnings": [],
  "correlationId": "abc-123"
}
```

**Response (Error - 400/500):**
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": ["SourceType is required"],
    "target": "parseSchema",
    "traceId": "0HN..."
  },
  "correlationId": "abc-123"
}
```

### GET `/health`

Returns service health status.

## Running Locally

### Prerequisites
- .NET 8 SDK
- Docker (optional)

### Running with .NET CLI

```bash
cd backend/services/schema-parser-service/src
dotnet restore
dotnet run
```

The service will start on `http://localhost:5000` (or as configured).

### Running with Docker

```bash
# From repository root
docker build -f backend/services/schema-parser-service/Dockerfile -t schema-parser-service .
docker run -p 8080:80 schema-parser-service
```

## Configuration

Configuration is managed through `appsettings.json` and `appsettings.Development.json`.

### Logging

Serilog is configured to log to console with structured logging. Correlation IDs are automatically included in log context.

## Project Structure

```
src/
├── Controllers/
│   └── SchemaParserController.cs    # API controller
├── Models/
│   ├── ParseSchemaRequest.cs        # Request DTO
│   ├── ResponseModels.cs            # Success/Error envelopes
│   └── SchemaDefinition.cs          # Domain model
├── Services/
│   ├── ISchemaParserService.cs      # Service interface
│   └── SchemaParserService.cs       # Core parsing logic
├── Validators/
│   └── ParseSchemaRequestValidator.cs # FluentValidation rules
├── Middleware/
│   ├── CorrelationIdMiddleware.cs   # Correlation tracking
│   └── ExceptionHandlingMiddleware.cs # Global error handling
├── Program.cs                        # Application entry point
├── appsettings.json                 # Configuration
└── SchemaParserService.csproj       # Project file
```

## Testing

```bash
cd backend/services/schema-parser-service/tests
dotnet test
```

## Supported Source Types

- `csv` - Comma-separated values with header row
- `json` - JSON objects with type inference
- `xml` - XML documents
- `xsd` - XML Schema Definition

## Development

### Adding New Source Types

1. Add validation rule in `ParseSchemaRequestValidator`
2. Add parser method in `SchemaParserService`
3. Update switch statement in `ParseSchemaAsync`
4. Write tests for the new type

### Extending Field Metadata

Modify the `SchemaField` class in `Models/SchemaDefinition.cs` to add additional properties.

## License

Copyright © 2025 Synapxe
