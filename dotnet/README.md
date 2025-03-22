# .NET integration snippet

This folder contains a minimal C# client you can drop into a .NET service or API layer to call the Python scoring API.

## Usage concept

1. Train the Python model
2. Run the FastAPI service
3. Call `/predict` from your .NET worker, Web API, or portal backend
4. Persist response into your operational database or trigger alerts
