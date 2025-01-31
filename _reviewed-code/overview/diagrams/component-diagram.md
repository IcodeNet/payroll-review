[⬅️ Back to Documentation](../../README.md)

# Component Level Architecture

```mermaid
graph TB
    subgraph API[API Layer]
        Controllers --> Middleware
        Middleware --> Auth[Authentication]
        Middleware --> Validation
    end

    subgraph Services[Service Layer]
        EmployeeService --> PayrollService
        PayrollService --> NotificationService
        PayrollService --> BankingService
    end

    subgraph Domain[Domain Layer]
        Employee --> Department
        Employee --> PaymentHistory
        Employee --> LeaveRequest
    end

    subgraph Data[Data Access]
        Repositories --> DbContext
        DbContext --> Database[(Database)]
    end

    Controllers --> Services
    Services --> Domain
    Services --> Repositories
```

## Key Components
- **API Layer**: Controllers, Middleware, Authentication
- **Service Layer**: Business logic orchestration
- **Domain Layer**: Core business entities and rules
- **Data Access**: Repository implementations