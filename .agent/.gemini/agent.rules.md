# AzureTest Architectural Rules for AI Agents

> **CRITICAL ARCHITECTURAL DIRECTIVES FOR ALL AI AGENTS AND DEVELOPERS**
> 
> This document defines mandatory, non-negotiable architectural rules, patterns, and design principles for the `AzureTest` solution. All future AI agents and contributors working in this codebase **MUST** follow these rules strictly.

---

## 1. Solution Overview & Clean Architecture Layers

The solution follows **Clean Architecture** and **Domain-Driven Design (DDD)** principles across the following layers:

```
src/
├── AzureTest.Domain          # Enterprise business rules (Entities, Documents, Enums, Primitives)
├── AzureTest.Contracts       # API contracts, DTOs, DTO Validators, Integration Events
├── AzureTest.Application     # Application business logic, CQRS (Commands, Queries, Handlers, Mappers, Command Validators, Units)
├── AzureTest.Persistence     # Relational (EF Core/PostgreSQL) & NoSQL (MongoDB) data access, Repositories, UnitOfWork
├── AzureTest.Infrastructure  # External services, Redis caching, Blob storage, OpenTelemetry, Serilog, Crawlers
├── AzureTest.MVC             # Web UI presentation layer (ASP.NET Core MVC, Razor views, ViewModels)
└── AzureTest.API             # Web API presentation layer (REST controllers)
```

### Dependency Rules:
- **`AzureTest.Domain`** has **NO dependencies** on other solution projects or external infrastructure libraries (EF Core, MongoDB, etc.).
- **`AzureTest.Contracts`** has **NO dependencies** on Application, Persistence, Infrastructure, or UI layers. Only depends on FluentValidation and Domain (if enums/primitives needed).
- **`AzureTest.Application`** depends ONLY on `AzureTest.Domain` and `AzureTest.Contracts`. It defines interfaces for persistence and infrastructure (`Interfaces/Data`, `Interfaces/Services`).
- **`AzureTest.Persistence`** implements persistence interfaces defined in `Application`.
- **`AzureTest.Infrastructure`** implements technical/external service interfaces defined in `Application`.
- **`AzureTest.MVC`** and **`AzureTest.API`** depend on `Application`, `Contracts`, and wire up DI via `Persistence` and `Infrastructure`.

---

## 2. CQRS & Messaging: NO MediatR

> **MANDATORY RULE**: **DO NOT USE MediatR.**
> The solution uses its own lightweight, strongly-typed messaging abstractions located in `AzureTest.Application.Messaging`.

### 2.1 Core Messaging Abstractions
All CQRS interfaces reside in `AzureTest.Application.Messaging`:
- `ICommand`: Marker for commands without return value.
- `ICommand<out TResponse>`: Marker for commands returning `TResponse` (e.g. `Guid`, `bool`, or `Result<T>`).
- `ICommandHandler<in TCommand>`: Handles `ICommand`.
- `ICommandHandler<in TCommand, TResponse>`: Handles `ICommand<TResponse>`.
- `IQuery<out TResponse>`: Marker for queries returning `TResponse`.
- `IQueryHandler<in TQuery, TResponse>`: Handles `IQuery<TResponse>`.

### 2.2 Base Handlers & Result Pattern
- Command handlers should inherit from `BaseCommandHandler<TCommand>` or `BaseCommandHandler<TCommand, TResponse>` (from `AzureTest.Application.Abstractions`). Base command handlers manage structured logging, error handling, and automatically call `UnitOfWork.SaveChangesAsync(cancellationToken)` upon successful execution.
- Query handlers should inherit from `BaseQueryHandler<TQuery, TResponse>`. Queries are read-only; no database changes or unit-of-work commits.
- Outcome values use `Result` or `Result<TValue>` from `AzureTest.Application.Abstractions`.

### 2.3 Handler Registration (Scrutor)
- Handlers are scanned and registered automatically into the DI container with scoped lifetime via Scrutor in `AzureTest.Application.ScrutorConfig.AddScrutorConfig()`.

### 2.4 Feature Command and Query Units
- Handlers are aggregated into Feature Units for clean dependency injection into Controllers:
  - Example: `ICompanyCommandUnit` & `CompanyCommandUnit`, `ICompanyQueryUnit` & `CompanyQueryUnit`.
  - Units are registered as scoped services in `AzureTest.Application/DependencyInjection.cs`.

---

## 3. CQRS Colocation Rule: Single File Mandate

> **MANDATORY RULE**:
> In `AzureTest.Application`, **Command, Handler, Mapper, and Validator MUST reside in the SAME file.**
> In `AzureTest.Application`, **Query, Handler, Mapper, and Validator MUST reside in the SAME file.**

Never split a command or query across separate files. Everything needed to understand, validate, map, and execute a command or query is self-contained in a single file.

### 3.1 Standard Command File Template

```csharp
// Path: src/AzureTest.Application/Features/Commands/CreateItem/CreateItemCommand.cs
namespace AzureTest.Application.Features.Commands.CreateItem;

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Features;
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

// ── 1. Command ────────────────────────────────────────────────────────────────
public sealed record CreateItemCommand(CreateItemDto Dto) : ICommand<Guid>;

// ── 2. Command Validator ──────────────────────────────────────────────────────
public sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Item payload is required.");
        RuleFor(x => x.Dto.Name).NotEmpty().WithMessage("Name is required.");
    }
}

// ── 3. Handler & Mappings ─────────────────────────────────────────────────────
public sealed class CreateItemCommandHandler : BaseCommandHandler<CreateItemCommand, Guid>
{
    public CreateItemCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateItemCommandHandler> logger)
        : base(unitOfWork, logger)
    {
    }

    protected override async Task<Guid> ExecuteAsync(CreateItemCommand command, CancellationToken cancellationToken)
    {
        var entity = MapToEntity(command.Dto);
        UnitOfWork.Items.Add(entity);
        return await Task.FromResult(entity.Id);
    }

    // ── Mappers: Both directions belong in the Handler file ────────────────────
    private static Item MapToEntity(CreateItemDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description
    };

    private static ItemDto MapToDto(Item entity) => new(
        entity.Id,
        entity.Name,
        entity.Description
    );
}
```

### 3.2 Standard Query File Template

```csharp
// Path: src/AzureTest.Application/Features/Queries/GetItems/GetItemsQuery.cs
namespace AzureTest.Application.Features.Queries.GetItems;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Features;
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

// ── 1. Dedicated Filter Record ────────────────────────────────────────────────
public sealed record ItemFilter(
    string? SearchTerm = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 10
);

// ── 2. Query ──────────────────────────────────────────────────────────────────
public sealed record GetItemsQuery(ItemFilter Filters) : IQuery<PagedResult<ItemDto>>;

// ── 3. Query Validator (if needed) ────────────────────────────────────────────
public sealed class GetItemsQueryValidator : AbstractValidator<GetItemsQuery>
{
    public GetItemsQueryValidator()
    {
        RuleFor(x => x.Filters.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Filters.PageSize).InclusiveBetween(1, 100);
    }
}

// ── 4. Handler & Mappings ─────────────────────────────────────────────────────
public sealed class GetItemsQueryHandler : BaseQueryHandler<GetItemsQuery, PagedResult<ItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetItemsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetItemsQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<PagedResult<ItemDto>> ExecuteAsync(GetItemsQuery query, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Items.GetAllAsync();
        
        // Filtering
        var filtered = items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.Filters.SearchTerm))
        {
            filtered = filtered.Where(x => x.Name.Contains(query.Filters.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = filtered.Count();
        var dtos = filtered
            .Skip((query.Filters.PageNumber - 1) * query.Filters.PageSize)
            .Take(query.Filters.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<ItemDto>(dtos, totalCount, query.Filters.PageNumber, query.Filters.PageSize);
    }

    // ── Mappers ───────────────────────────────────────────────────────────────
    private static ItemDto MapToDto(Item entity) => new(
        entity.Id,
        entity.Name,
        entity.Description
    );
}
```

---

## 4. Validation Rules (FluentValidation)

> **MANDATORY RULE**: **Use `FluentValidation` for ALL validations across the entire solution.**

Validation is split into two distinct tiers:

| Validation Tier | Scope | Location | File Placement |
|---|---|---|---|
| **Command / Query Validation** | Validates internal command/query intent, parameters, and business invariants | `AzureTest.Application` | **Inside the same file as the Handler** |
| **DTO Validation** | Validates external contract payloads, input format, string lengths, regex, required fields | `AzureTest.Contracts` | **Dedicated file under `AzureTest.Contracts/Validators/<Feature>/<DtoName>Validator.cs`** |

### Registration:
- Both assemblies are registered in `AzureTest.Application/DependencyInjection.cs`:
  - `services.AddValidatorsFromAssemblyContaining<CreateResumeDtoValidator>();` (Contracts)
  - `services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());` (Application)

---

## 5. Object Mapping Rules

> **MANDATORY RULE**:
> **Both mapping directions (`Dto => Entity` and `Entity => Dto`) MUST reside in the Handler file.**

- **No AutoMapper**: Do not introduce AutoMapper or external reflection-based mapping tools.
- **Explicit Mappings**: Write clean, explicit mapping methods (`MapToEntity`, `MapToDto`) as static or private methods directly inside the Handler class.
- Explicit mapping prevents hidden side effects, preserves type safety, supports custom domain constructor rules, and makes code instantly searchable and readable.

---

## 6. Query Filtering Rules: Dedicated Filter Class

> **MANDATORY RULE**:
> **Queries MUST accept a dedicated `Filter` class/record for filtering rather than a long list of primitive parameters.**

- Never write queries like: `GetItemsQuery(string name, bool? active, DateTime? from, int page, int pageSize)`.
- **Always** wrap filter parameters into a dedicated filter record (e.g. `ItemFilter`, `CompanyFilter`, `UserFilter`, `GetAllExpensesFilters`):
  ```csharp
  public sealed record CompanyFilter(
      string? Name = null,
      bool? IsINDMember = null,
      string? Location = null,
      int PageNumber = 1,
      int PageSize = 10
  );

  public sealed record GetAllCompaniesQuery(CompanyFilter Filters) : IQuery<PagedResult<CompanyDto>>;
  ```
- Always include pagination defaults (`PageNumber = 1`, `PageSize = 10` or `20`) on the Filter class.
- Return paged responses wrapped in `PagedResult<T>` from `AzureTest.Contracts.DTOs.Common`.

---

## 7. DTO Rules: Location & Immutability

> **MANDATORY RULE**:
> **ALL DTOs must be written in the `AzureTest.Contracts` project under `AzureTest.Contracts/DTOs/<Feature>/`.**

- DTOs represent external contracts and must never be defined inside `AzureTest.Application`, `AzureTest.Domain`, or presentation layers.
- Prefer `public sealed record` for immutable DTO definitions.
- Related DTOs for a feature must be grouped into their respective folder (e.g., `DTOs/Debts/`, `DTOs/Companies/`, `DTOs/Users/`).

---

## 8. Repositories & Unit of Work Rules

> **MANDATORY RULES**:
> 1. **Entities and Documents must ALWAYS generate Repositories.**
> 2. **SQL repositories inherit from `GenericRepository<TEntity>` and their own interface.**
> 3. **Document repositories inherit from `DocumentRepository<TDocument>` and their own interface.**
> 4. **Repositories must ALWAYS be registered in their respective UnitOfWork: `IUnitOfWork` (for SQL) and `INoSqlUnitOfWork` (for NoSQL).**

### 8.1 Relational (SQL / EF Core / PostgreSQL) Repositories
1. **Interface** in `AzureTest.Application.Interfaces.Data/I<Entity>Repository.cs`:
   ```csharp
   public interface IDebtRepository : IGenericRepository<Debt>
   {
       // Custom queries if needed
   }
   ```
2. **Implementation** in `AzureTest.Persistence.Repositories/<Entity>Repository.cs`:
   ```csharp
   public class DebtRepository : GenericRepository<Debt>, IDebtRepository
   {
       public DebtRepository(AzureTestDbContext dbContext) : base(dbContext)
       {
       }
   }
   ```
3. **Register in `IUnitOfWork`** (`AzureTest.Application.Interfaces.Data/IUnitOfWork.cs`):
   ```csharp
   public interface IUnitOfWork : IDisposable
   {
       IDebtRepository Debts { get; }
       Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
   }
   ```
4. **Implement in `UnitOfWork`** (`AzureTest.Persistence.Repositories/UnitOfWork.cs`).
5. **Register in DI** (`AzureTest.Persistence/DependencyInjection.cs`).

### 8.2 NoSQL (MongoDB) Repositories
1. **Interface** in `AzureTest.Application.Interfaces.Data.NoSql/I<Document>Repository.cs`:
   ```csharp
   public interface IDebtPersonRepository : IDocumentRepository<DebtPerson>
   {
       Task<IReadOnlyList<DebtPerson>> SearchByNameAsync(string term);
   }
   ```
2. **Implementation** in `AzureTest.Persistence.Repositories.NoSql/<Document>Repository.cs`:
   ```csharp
   public class DebtPersonRepository : DocumentRepository<DebtPerson>, IDebtPersonRepository
   {
       public DebtPersonRepository(IMongoDatabase database) : base(database, "DebtPersons")
       {
       }
   }
   ```
3. **Register BSON Class Map** in `AzureTest.Persistence.Configurations.NoSql/MongoDbClassMaps.cs`:
   - Map `Id` property to MongoDB `_id` field.
   - Use `GuidRepresentation.Standard`.
   - Call `SetIgnoreExtraElements(true)`.
4. **Register in `INoSqlUnitOfWork`** (`AzureTest.Application.Interfaces.Data.NoSql/INoSqlUnitOfWork.cs`).
5. **Implement in `NoSqlUnitOfWork`** (`AzureTest.Persistence.Repositories.NoSql/NoSqlUnitOfWork.cs`).
6. **Register in DI** (`AzureTest.Persistence/DependencyInjection.cs`).

---

## 9. Domain & Entity Guidelines (DDD)

- **Entities**: Placed in `src/AzureTest.Domain/Entities/`.
  - Must inherit from `BaseEntity` or `AggregateRoot`.
  - Every entity has a UUIDv7 identifier: `public Guid Id { get; protected set; } = Guid.CreateVersion7();`.
  - Implement `IAudit` (`CreatedOn`, `CreatedBy`, `ModifiedOn`, `ModifiedBy`) for audit-tracked entities.
  - Implement `ISoftDelete` (`IsDeleted`, `DeletedOn`) for soft-deletable entities.
  - EF Core interceptors (`AuditInterceptor`, `DomainEventsInterceptor`) automatically populate audit metadata and dispatch domain events upon `SaveChangesAsync()`.
- **Documents**: Placed in `src/AzureTest.Domain/Documents/`.
  - MongoDB documents use `Guid Id { get; init; } = Guid.CreateVersion7();`.
- **Domain Events**: Placed in `src/AzureTest.Domain/Primitives/`. Inherit from `IDomainEvent` or `DomainEvent`. Handled via `AggregateRoot.AddDomainEvent(...)`.

---

## 10. Persistence vs. Infrastructure Layer Separation

Keep a clean boundary between storage implementations and technical services:

| Concern | Target Layer | Examples |
|---|---|---|
| **Data Storage & Repositories** | `AzureTest.Persistence` | `AzureTestDbContext`, EF Core migrations, GenericRepository, DocumentRepository, UnitOfWork, NoSqlUnitOfWork, DB interceptors, DB seeders |
| **External & Cross-Cutting Services** | `AzureTest.Infrastructure` | Azure Blob Storage, Redis cache, Identity (`UserManagerService`, `RoleManagerService`), Web Crawlers, OpenTelemetry, Serilog, Health Checks |

---

## 11. Presentation Layer (MVC & API)

- **Thin Controllers**: Controllers must never contain business or data access logic.
  - Controllers receive HTTP requests, bind to DTOs/Commands, execute FluentValidation, invoke the Command/Query handler (or Feature Unit), and return views or JSON.
  - In MVC: Inject Feature Units (`I<Feature>CommandUnit`, `I<Feature>QueryUnit`) or specific handlers, and `IValidator<T>`.
- **ViewModels**: Placed in `AzureTest.MVC/Models` or `AzureTest.MVC/ViewModels`. Used purely for UI rendering.

---

## 12. Code Standards & Conventions

1. **Target Framework**: .NET 10 (`net10.0`).
2. **C# Language Features**:
   - File-scoped namespaces (`namespace AzureTest.Application...;`).
   - Nullable reference types enabled (`<Nullable>enable</Nullable>`).
   - Primary constructors for classes and records where appropriate.
   - UUIDv7 generation via `Guid.CreateVersion7()`.
3. **Structured Logging**: Always inject `ILogger<T>` and use semantic logging templates (e.g. `_logger.LogInformation("Processing {CommandName} for user {UserId}", ...)`).

---

## 13. AI Agent Step-by-Step Implementation Checklist

When tasked with adding a new feature or entity, follow these steps in order:

### Adding a new SQL Entity Feature:
1. **Domain**: Create Entity in `src/AzureTest.Domain/Entities/<Entity>.cs` inheriting `BaseEntity` or `AggregateRoot`.
2. **Contracts**: Create DTOs in `src/AzureTest.Contracts/DTOs/<Feature>/` and DTO validators in `src/AzureTest.Contracts/Validators/<Feature>/`.
3. **Application Interfaces**: Create `I<Entity>Repository.cs` in `src/AzureTest.Application/Interfaces/Data/`. Add property to `IUnitOfWork`.
4. **Persistence**:
   - Create `<Entity>Repository.cs` in `src/AzureTest.Persistence/Repositories/`.
   - Add property to `UnitOfWork.cs`.
   - Add `DbSet<<Entity>>` to `AzureTestDbContext.cs` and add EF Core configuration in `Configurations/`.
   - Register in `AzureTest.Persistence/DependencyInjection.cs`.
   - Add EF Core migration.
5. **Application CQRS**:
   - Create Commands and Queries in `src/AzureTest.Application/<Feature>/`.
   - Ensure Command + Handler + Mapper + Command Validator are colocated in the SAME file.
   - Ensure Query + Filter + Handler + Mapper + Query Validator are colocated in the SAME file.
   - Create `I<Feature>CommandUnit` / `I<Feature>QueryUnit` if grouping commands/queries for controller consumption.
   - Register units in `AzureTest.Application/DependencyInjection.cs`.
6. **Presentation**: Add or update MVC/API controller and views.

### Adding a new NoSQL Document Feature:
1. **Domain**: Create Document in `src/AzureTest.Domain/Documents/<Doc>.cs`.
2. **Contracts**: Create DTOs and DTO validators in `AzureTest.Contracts`.
3. **Persistence Configuration**: Register BSON class map in `MongoDbClassMaps.cs`.
4. **Application Interfaces**: Create `I<Doc>Repository.cs` in `src/AzureTest.Application/Interfaces/Data/NoSql/`. Add property to `INoSqlUnitOfWork`.
5. **Persistence**:
   - Create `<Doc>Repository.cs` in `src/AzureTest.Persistence/Repositories/NoSql/`.
   - Add property to `NoSqlUnitOfWork.cs`.
   - Register repository in `AzureTest.Persistence/DependencyInjection.cs`.
6. **Application CQRS**: Colocate Command/Query, Handler, Mapper, and Validator in single files in `AzureTest.Application`.
