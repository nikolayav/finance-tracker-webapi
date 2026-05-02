\# FinancePlanner API



A production-grade personal finance management REST API built with \*\*.NET 8\*\*, \*\*Azure SQL Database\*\*, and \*\*Azure\*\* cloud services. Designed to demonstrate layered architecture, JWT authentication, structured logging, distributed telemetry, and cloud-native deployment.



\---



\## Architecture



The solution follows a \*\*layered architecture\*\* with clear separation of concerns across four projects:



```

HTTP Request

&#x20; → Controller (FinancePlanner.API)

&#x20;   → Service (FinancePlanner.API/Services)

&#x20;     → Repository (FinancePlanner.Infrastructure)

&#x20;       → AppDbContext (EF Core)

&#x20;         → Azure SQL Database

```



\### Projects



| Project | Responsibility |

|---|---|

| `FinancePlanner.API` | Controllers, Services, JWT, middleware, DI registration |

| `FinancePlanner.Infrastructure` | EF Core DbContext, Entities, Repositories, Migrations |

| `FinancePlanner.Common` | Shared DTOs, Enums used across layers |

| `FinancePlanner.UnitTests` | xUnit + Moq unit tests for service layer |



\---



\## Tech Stack



| Concern | Technology |

|---|---|

| Framework | .NET 8 Web API |

| Database | Azure SQL Database (Basic tier) |

| ORM | Entity Framework Core 8 |

| Authentication | JWT Bearer tokens (15 min expiry) |

| Password hashing | BCrypt.Net |

| Structured logging | Serilog |

| Telemetry | Azure Application Insights |

| Secrets (production) | Azure Key Vault |

| Secrets (local) | Environment variables |

| Unit testing | xUnit + Moq |

| Hosting | Azure App Service (B2 Linux) |



\---



\## Project Structure



```

FinancePlanner/

├── FinancePlanner.API/

│   ├── Controllers/

│   │   ├── BaseController.cs           ← shared GetUserId() from JWT claims

│   │   ├── AuthController.cs           ← POST register, POST login

│   │   ├── AccountsController.cs       ← GET, POST, DELETE accounts

│   │   ├── TransactionsController.cs   ← GET, POST, DELETE transactions

│   │   └── BudgetsController.cs        ← GET, POST, DELETE budgets

│   ├── Services/

│   │   ├── Interfaces/

│   │   │   ├── IAuthService.cs

│   │   │   ├── IAccountService.cs

│   │   │   ├── ITransactionService.cs

│   │   │   ├── IBudgetService.cs

│   │   │   └── IJwtTokenService.cs

│   │   ├── AuthService.cs              ← register, login, BCrypt hashing

│   │   ├── AccountService.cs           ← account management

│   │   ├── TransactionService.cs       ← transactions + real-time balance updates

│   │   ├── BudgetService.cs            ← budget management

│   │   └── JwtTokenService.cs          ← JWT token generation

│   ├── appsettings.json                ← non-sensitive config only

│   └── Program.cs                      ← DI, middleware, auth pipeline

│

├── FinancePlanner.Infrastructure/

│   ├── Data/

│   │   └── AppDbContext.cs             ← EF Core context + generic audit logging

│   ├── Entities/

│   │   ├── User.cs

│   │   ├── Account.cs

│   │   ├── Transaction.cs

│   │   ├── Budget.cs

│   │   └── AuditLog.cs                 ← captures all entity changes automatically

│   ├── Repositories/

│   │   ├── Interfaces/

│   │   │   ├── IUserRepository.cs

│   │   │   ├── IAccountRepository.cs

│   │   │   ├── ITransactionRepository.cs

│   │   │   └── IBudgetRepository.cs

│   │   ├── UserRepository.cs

│   │   ├── AccountRepository.cs

│   │   ├── TransactionRepository.cs

│   │   └── BudgetRepository.cs

│   └── Migrations/

│

├── FinancePlanner.Common/

│   ├── DTOs/

│   │   ├── Auth/         ← RegisterRequest, LoginRequest, AuthResponse

│   │   ├── Account/      ← CreateAccountRequest, AccountResponse

│   │   ├── Transaction/  ← CreateTransactionRequest, TransactionResponse

│   │   └── Budget/       ← CreateBudgetRequest, BudgetResponse

│   └── Enums/

│       ├── AccountType.cs       ← Checking, Savings, CreditCard, Cash

│       └── TransactionType.cs   ← Income, Expense

│

└── FinancePlanner.UnitTests/

&#x20;   └── Services/

&#x20;       ├── AuthServiceTests.cs

&#x20;       ├── TransactionServiceTests.cs

&#x20;       └── BudgetServiceTests.cs

```



\---



\## Security Design



\*\*Authentication\*\* uses short-lived JWT tokens (15 minutes by default). The user ID is extracted from the JWT claims server-side — it is never trusted from the request body, preventing users from accessing each other's data.



\*\*Secrets\*\* are never stored in source code or configuration files:



| Environment | Secret storage |

|---|---|

| Local development | PowerShell environment variables |

| Production (Azure) | Azure Key Vault via managed identity |



\*\*Passwords\*\* are hashed with BCrypt before storage — plain text passwords are never persisted.



\---



\## Audit Logging



Every entity change (create, update, delete) is automatically captured in the `AuditLogs` table via a generic `SaveChangesAsync` override in `AppDbContext`. Each audit entry records the entity name, entity ID, action type, old values, new values, and timestamp — without requiring any changes to individual repositories or services.



This is a pattern that ensures a full history of all data modifications is retained.



\---



\## Observability



Structured logging is implemented with \*\*Serilog\*\* and shipped to \*\*Azure Application Insights\*\*. Every service method logs:



\- Operation start with contextual parameters (user ID, entity ID, filters)

\- Operation success with result counts or created entity IDs

\- Warnings for business rule violations (unauthorized access attempts, duplicate registrations)



This produces end-to-end traces visible in the Application Insights transaction search and live metrics dashboard.



\---



\## Local Development Setup



\*\*Prerequisites:\*\*

\- .NET 8 SDK

\- Access to an Azure SQL Database instance



\*\*Required environment variables:\*\*



```powershell

$env:ConnectionStrings\_\_DefaultConnection  = "Server=...;Database=...;User Id=...;Password=...;Encrypt=True;"

$env:Jwt\_\_SecretKey                        = "your-secret-key-minimum-32-characters"

$env:ApplicationInsights\_\_ConnectionString = "InstrumentationKey=...;"

```



\*\*Run the API:\*\*



```powershell

dotnet run --project FinancePlanner.API

```



\*\*Apply database migrations:\*\*



```powershell

dotnet ef database update `

&#x20; --project FinancePlanner.Infrastructure `

&#x20; --startup-project FinancePlanner.API

```



\*\*Run unit tests:\*\*



```powershell

dotnet test FinancePlanner.UnitTests

```



\---



\## Azure Infrastructure



All resources live under the resource group `rg-financedashboard`:



| Resource | Name | Tier |

|---|---|---|

| App Service Plan | `asp-financedashboard` | B2 Linux |

| Web App | `app-financedashboard` | — |

| SQL Server | `sql-financedashboard` | — |

| SQL Database | `financedashboard` | Basic |

| Key Vault | `kv-financedashboard` | Standard |

| Application Insights | `ai-financedashboard` | Pay-as-you-go |

| Log Analytics Workspace | `law-financedashboard` | PerGB2018 |

| Container Registry | `acrfinancedashboard` | Basic |

| SignalR Service | `sigr-financedashboard` | Free F1 |



\---



\## Design Decisions



\*\*Layered architecture\*\* — for a financial management API of this scope, layered architecture provides a clear and maintainable structure.



\*\*Generic audit logging\*\* — implemented at the `DbContext` level rather than per-repository, ensuring all entity changes are captured consistently without repetitive code across services.



\*\*Environment-based secret resolution\*\* — the .NET configuration system merges environment variables over `appsettings.json`, so the same codebase runs locally against environment variables and in Azure against Key Vault with zero code changes.



\*\*decimal(18,4) for all monetary values\*\* — financial amounts are stored with four decimal places using SQL Server's `decimal` type. Floating-point types (`float`, `double`) are explicitly avoided to prevent silent precision loss on monetary calculations.



\*\*BCrypt for password hashing\*\* — BCrypt is deliberately slow by design, making brute-force attacks computationally expensive. The work factor can be increased over time as hardware improves without requiring password resets.

