
# Error Handling v1.0
## Principles
- No PHI in logs
- Friendly UI messages
## API Envelope
```
{
  "success": false,
  "error": { "code": "...", "message": "..." },
  "correlationId": "..."
}
```
## Error Categories
- Validation errors
- Mapping errors
- System errors
