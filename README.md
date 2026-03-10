# OasisWords

A vocabulary learning platform built with **Clean Architecture**, **CQRS**, and **.NET 8**.

---

## Solution Structure

```
OasisWords/
├── OasisWords.sln
└── src/
    ├── Core/                               # Reusable core packages
    │   ├── OasisWords.Core.Application     # MediatR pipelines, PageRequest, ValidationTool
    │   ├── OasisWords.Core.CrossCuttingConcerns  # ExceptionMiddleware, Serilog, logging
    │   ├── OasisWords.Core.Mailing         # MailKit email service
    │   ├── OasisWords.Core.Persistence     # EF generic repository, paging, dynamic query
    │   └── OasisWords.Core.Security        # JWT, hashing, OTP/email auth, entities
    │
    ├── OasisWords.Domain                   # Business entities & enums
    ├── OasisWords.Application              # CQRS features, validators, AutoMapper profiles
    ├── OasisWords.Persistence              # EF Core DbContext, entity configs, repositories
    ├── OasisWords.Infrastructure           # External service adapters (AI, translation, etc.)
    └── OasisWords.WebAPI                   # ASP.NET Core Web API, controllers, Program.cs
```

---

## Architecture

| Pattern | Implementation |
|---|---|
| Clean Architecture | Domain → Application → Persistence/Infrastructure → WebAPI |
| CQRS | MediatR `IRequest<T>` commands & queries per feature folder |
| Mediator | MediatR with ordered pipeline behaviors |
| Repository | Generic `EfRepositoryBase<TEntity, TId, TContext>` |
| Pipeline Behaviors | Authorization → Logging → Validation → Caching → CacheRemoving |
| Middleware | `ExceptionMiddleware` maps exceptions to RFC 7807 ProblemDetails |
| Dependency Injection | Microsoft DI with `AddApplicationServices` / `AddPersistenceServices` etc. |
| Options Pattern | `TokenOptions`, `MailSettings`, `CacheSettings` bound from appsettings |

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL 15+
- Redis (optional – falls back to in-memory cache in development)

### 1. Configure `appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "OasisWordsDB": "Host=localhost;Port=5432;Database=OasisWordsDb_Dev;Username=postgres;Password=yourpassword"
  },
  "TokenOptions": {
    "SecurityKey": "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG"
  }
}
```

### 2. Apply EF Core Migrations
```bash
cd src/OasisWords.WebAPI
dotnet ef migrations add InitialCreate --project ../OasisWords.Persistence
dotnet ef database update
```

### 3. Run
```bash
dotnet run --project src/OasisWords.WebAPI
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

---

## Adding a New Feature

1. **Domain entity** → `OasisWords.Domain/Entities/`
2. **Entity config** → `OasisWords.Persistence/EntityConfigurations/`
3. **DbSet** → `OasisWordsDbContext`
4. **Repository interface** → `OasisWords.Application/Services/`
5. **Repository impl** → `OasisWords.Persistence/Repositories/`
6. **Feature folder** → `OasisWords.Application/Features/<FeatureName>/`
   - `Commands/` and `Queries/` (CQRS handlers)
   - `Validators/` (FluentValidation)
   - `Profiles/` (AutoMapper)
   - `Rules/` (business rule classes)
7. **Controller** → `OasisWords.WebAPI/Controllers/`

---

## Technologies

| Library | Version | Purpose |
|---|---|---|
| .NET | 8 LTS | Runtime |
| ASP.NET Core | 8 | Web API |
| Entity Framework Core | 8 | ORM |
| PostgreSQL / Npgsql | 8 | Database |
| MediatR | 12 | CQRS / Mediator |
| AutoMapper | 13 | Object mapping |
| FluentValidation | 11 | Input validation |
| Serilog | 8 | Structured logging |
| Redis | StackExchange | Distributed cache |
| MailKit / MimeKit | 4 | Email delivery |
| Otp.NET | 1.4 | TOTP 2FA |
| JWT | 7 | Authentication tokens |
| Swashbuckle | 6.5 | Swagger/OpenAPI |
