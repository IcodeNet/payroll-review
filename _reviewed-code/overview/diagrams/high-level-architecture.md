[⬅️ Back to Documentation](../../README.md)

# High Level Architecture

```mermaid
graph TB
    Client[Client Applications] --> API[Web API Layer]
    API --> Services[Service Layer]
    Services --> Domain[Domain Layer]
    Services --> Data[Data Access Layer]
    Domain --> Data
    Services --> Banking[Banking Integration]

    subgraph External
        Banking --> BankingAPI[Banking API]
    end

    Data --> DB[(SQL Database)]
```

## Components
- **Client Applications**: Web and Mobile clients
- **Web API Layer**: REST API endpoints
- **Service Layer**: Business logic and orchestration
- **Domain Layer**: Core business rules and entities
- **Data Access Layer**: Database operations
- **Banking Integration**: External payment processing