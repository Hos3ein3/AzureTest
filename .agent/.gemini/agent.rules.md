---
name: dotnet-vsa-ddd-feature
description: Create or change a feature in our .NET monolith (Clean Architecture, DDD, EDD, Vertical Slice Application layer, MVC). Use when adding a Domain entity or NoSQL Document, enum, domain rule values, Command, Query, mapping, Controller action, persistence configuration, or filter class.
---

# Purpose
Generate code that follows our architecture exactly. Do not introduce new patterns or libraries.
Never use MediatR or AutoMapper (or any mapping library). We use our own `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`, and manual mapping.

# Architecture rules
- Style: monolith, Clean Architecture, DDD + Event-Driven Design (EDD).
- Dependency direction: Mvc -> Application -> Domain. Persistence -> Application/Domain. Domain depends on nothing.
- Application layer uses Vertical Slice Architecture (VSA): one feature = one folder/file set.
- Contracts layer holds all DTOs. Never return domain entities from handlers or controllers.
- All Commands and Queries return `Result` or `Result<T>`. Do not throw for expected business errors.

# Layers and what goes where

## 1. Domain layer
- A SQL model is an **Entity**. It inherits `BaseEntity` and `AggregateRoot` (when it is an aggregate root).
- A NoSQL model is a **Document**. It follows the same base types.
- Constructors are private/protected. Create objects only through **static factory methods** on the class (e.g. `Order.Create(...)`).
- Put business behavior in methods on the class (e.g. `order.Cancel(reason)`). No public setters.
- Raise domain events inside the aggregate (EDD) when state changes in a meaningful way.
- Each Domain/Document has its own repository interfaces:
  - SQL: `IOrderRepository : IGenericRepository<Order>`
  - NoSQL: `IOrderDocumentRepository : INoSqlRepository<OrderDocument>`
- Two units of work exist: `IUnitOfWork` (SQL) and `INoSqlUnitOfWork` (NoSQL). Use the one that matches the store.

### Enums
- All enums live in the Domain layer, in an `Enums` folder of the matching Domain (e.g. `Domain/Orders/Enums/OrderStatus.cs`).
- One enum per file. Use explicit integer values for enums stored in the database.
- Contracts, Application, Persistence and MVC reference these enums from Domain. Do not duplicate enums elsewhere.
- In persistence configuration, set how the enum is stored (e.g. `HasConversion<int>()` or `HasConversion<string>()`).

### Rule values (no magic numbers)
- Every value that is a data rule (max length, min length, min/max number, precision, scale, regex pattern, allowed range, default value, etc.) is defined ONCE as a static value in the Domain class.
- Put them in a nested `public static class Rules` inside the Domain/Document class (e.g. `Order.Rules.NumberMaxLength`).
- Use `const` for compile-time types (`int`, `string`, ...). Use `static readonly` for other types (`decimal`, `DateTime`, `Regex`, ...).
- Name pattern: `{Property}{Rule}`, e.g. `NumberMaxLength`, `NumberMinLength`, `AmountMin`, `AmountPrecision`, `AmountScale`, `CodePattern`.
- The same values are used in ALL of these places:
  - Persistence Fluent API (`HasMaxLength(Order.Rules.NumberMaxLength)`).
  - FluentValidation validators (`MaximumLength(Order.Rules.NumberMaxLength)`).
  - Domain factory/behavior checks.
- Never write a literal like `50` or `100` for these rules in Persistence, Application or MVC.
- When a rule changes, it changes in one place only.

## 2. Contracts layer
- Request/response DTOs, filter-result DTOs, paged-result DTOs.
- Plain classes or records. No logic. No reference to Domain classes (enums and rule values from Domain are allowed).

## 3. Persistence layer
- One configuration class per Domain: `OrderConfiguration : IEntityTypeConfiguration<Order>`.
- In that class, always set: table name, schema, keys, property rules, relations, indexes (Fluent API only, no data annotations).
- All rule values (max length, precision, scale, etc.) come from `{Domain}.Rules`. No hard-coded values.
- Audit: an EF Core `SaveChangesInterceptor` fills audit fields. Do not set audit fields by hand in handlers.
- Soft delete: a global query filter (`HasQueryFilter(x => !x.IsDeleted)`). Delete = set the soft-delete flag through the aggregate/interceptor. Use `IgnoreQueryFilters()` only when the user asks for deleted data.
- Implement the repositories here. Do not call `SaveChanges` inside repositories; the unit of work does it.

## 4. Application layer (VSA)
Folder layout per Domain:

```
Application/
  Orders/
    Commands/
      CreateOrder/CreateOrderCommand.cs      (command + mapping + handler + validator in ONE file)
      CancelOrder/CancelOrderCommand.cs
    Queries/
      GetOrders/GetOrdersQuery.cs            (query + mapping + handler + validator in ONE file)
      GetOrderById/GetOrderByIdQuery.cs
    IOrderCommandUnit.cs
    OrderCommandUnit.cs
    IOrderQueryUnit.cs
    OrderQueryUnit.cs
```

Rules:
- One file per use case. The file contains: the `Command`/`Query` type, its mapping class, its `FluentValidation` validator (`AbstractValidator<T>`), and the handler.
- Command implements `ICommand` or `ICommand<T>`. Handler implements `ICommandHandler<TCommand, ...>`.
- Query implements `IQuery<T>`. Handler implements `IQueryHandler<TQuery, T>`.
- Handler flow for commands: load aggregate via repository -> call domain method / static factory (using mapped values) -> add/update in repository -> `unitOfWork.SaveChangesAsync()` -> return `Result`.
- Query handlers are read-only. Use projection to DTOs and `AsNoTracking()`.
- Handlers do NOT validate input. Validation runs in the controller through `IValidatorService` before the handler is called.
- Validators take every limit (max/min length, ranges, patterns) from `{Domain}.Rules`. No hard-coded values.
- **Command Unit**: `I{Domain}CommandUnit` / `{Domain}CommandUnit` exposes one method per command of that Domain and forwards to the matching command handler.
- **Query Unit**: `I{Domain}QueryUnit` / `{Domain}QueryUnit` does the same for all queries of that Domain.
- Register units and handlers in DI (one extension method per Domain, e.g. `AddOrderModule()`).

### Mapping
- We have no AutoMapper or other mapping library. Write all mapping by hand.
- The mapping class lives inside the Command class (same file), as a nested `static class Mapping`.
- Mapping methods are static, small and pure. Typical methods: command -> domain input / domain object, domain -> DTO.
- Queries use the same pattern: a nested `Mapping` class (or an `Expression<Func<Entity, Dto>>` for EF projection) in the Query class.
- Do not create shared global mappers or reflection-based mapping.
- Mapping does not contain business rules. Business rules stay in the Domain class.

## 5. MVC layer
- Controller injects: `IValidatorService`, `I{Domain}CommandUnit`, `I{Domain}QueryUnit`. Never inject handlers directly.
- Action flow: validate with `IValidatorService` -> call unit -> return with `ToView` or `ToForm`.
- Keep actions thin. No business logic.
- **Index action**: create a filter class (e.g. `OrderFilter`) that holds all filter, sorting and pagination properties (`Page`, `PageSize`, `SortBy`, `SortDirection`, search fields). The action takes this one object. The Query takes the same filter object, so the handler is reusable and the action stays clean.

### Controller metadata
- Every controller class and every action has `[DisplayName("...")]` and `[Description("...")]` (`System.ComponentModel`).
- `DisplayName` is a short, human-readable name (2-4 words). `Description` is one sentence about what the action does.
- Apply this to all actions (Index, Details, Create, Edit, Delete, custom). Never leave a public action without both.

### Action return values
- Do not build `IActionResult` by hand from a `Result`. Use the `Result` extension methods:
  - `ToView(...)` for page actions (Index, Details, Edit GET). Success renders the view with the value. Failure maps the error (NotFound, Forbidden, error page).
  - `ToForm(...)` for form actions (Create/Edit POST, Delete). Success redirects (or shows a message). Failure returns to the form with errors in `ModelState`.
- Validation errors from `IValidatorService` also go back through `ToForm`.
- Do not write `if (result.IsSuccess) ... else ...` in controllers.
- If a needed `ToView`/`ToForm` overload does not exist, say so and propose a new overload. Do not inline a workaround.

# Result pattern
- Success: `Result.Success()` / `Result<T>.Success(value)`.
- Failure: `Result.Failure(Error)`; use typed errors (NotFound, Validation, Conflict, Unexpected).
- Never return `null` for "not found". Return a failure Result.

# Naming
- Commands: `{Verb}{Domain}Command`. Queries: `Get{Domain}...Query`.
- Handlers: `{Command|Query}Handler`. Validators: `{Command|Query}Validator`.
- Config: `{Domain}Configuration`. Repository: `I{Domain}Repository`.
- Mapping: nested `Mapping` class in the command/query.
- Rule values: nested `Rules` class in the Domain class, members named `{Property}{Rule}`.

# Code templates
Adjust base type and method names to the real codebase.

## Domain (with Rules)
```csharp
public sealed class Order : BaseEntity, IAggregateRoot
{
    public static class Rules
    {
        public const int NumberMinLength = 3;
        public const int NumberMaxLength = 50;
        public const int NotesMaxLength = 500;
        public const int AmountPrecision = 18;
        public const int AmountScale = 2;
        public static readonly decimal AmountMin = 0.01m;
    }

    private Order() { }

    public string Number { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public OrderStatus Status { get; private set; }

    public static Result<Order> Create(string number, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(number) || number.Length > Rules.NumberMaxLength)
            return Result<Order>.Failure(OrderErrors.NumberInvalid);

        var order = new Order { Number = number, Amount = amount, Status = OrderStatus.New };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id));
        return Result<Order>.Success(order);
    }
}
```

## Enum (Domain layer)
```csharp
// Domain/Orders/Enums/OrderStatus.cs
public enum OrderStatus
{
    New = 1,
    Paid = 2,
    Cancelled = 3
}
```

## Persistence configuration (uses Rules)
```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders", "sales");
        b.HasKey(x => x.Id);

        b.Property(x => x.Number)
            .HasMaxLength(Order.Rules.NumberMaxLength)
            .IsRequired();

        b.Property(x => x.Amount)
            .HasPrecision(Order.Rules.AmountPrecision, Order.Rules.AmountScale);

        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => x.Number).IsUnique();
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
```

## Command + mapping + validator + handler (one file)
```csharp
public sealed record CreateOrderCommand(string Number, decimal Amount) : ICommand<Guid>
{
    public static class Mapping
    {
        public static Result<Order> ToDomain(CreateOrderCommand cmd) =>
            Order.Create(cmd.Number.Trim(), cmd.Amount);
    }
}

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty()
            .MinimumLength(Order.Rules.NumberMinLength)
            .MaximumLength(Order.Rules.NumberMaxLength);

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(Order.Rules.AmountMin)
            .PrecisionScale(Order.Rules.AmountPrecision, Order.Rules.AmountScale, false);
    }
}

public sealed class CreateOrderCommandHandler(IOrderRepository repo, IUnitOfWork uow)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateOrderCommand cmd, CancellationToken ct)
    {
        var created = CreateOrderCommand.Mapping.ToDomain(cmd);
        if (created.IsFailure) return Result<Guid>.Failure(created.Error);

        await repo.AddAsync(created.Value, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(created.Value.Id);
    }
}
```

## Filter class
```csharp
public sealed class OrderFilter
{
    public string? Search { get; set; }
    public OrderStatus? Status { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

## Query + mapping using the filter
```csharp
public sealed record GetOrdersQuery(OrderFilter Filter) : IQuery<PagedResult<OrderDto>>
{
    public static class Mapping
    {
        public static readonly Expression<Func<Order, OrderDto>> ToDto =
            o => new OrderDto(o.Id, o.Number, o.Amount, o.Status);
    }
}
```

## Command unit
```csharp
public interface IOrderCommandUnit
{
    Task<Result<Guid>> CreateAsync(CreateOrderCommand command, CancellationToken ct);
}

public sealed class OrderCommandUnit(ICommandHandler<CreateOrderCommand, Guid> create) : IOrderCommandUnit
{
    public Task<Result<Guid>> CreateAsync(CreateOrderCommand c, CancellationToken ct) => create.HandleAsync(c, ct);
}
```

## Controller
```csharp
[DisplayName("Orders")]
[Description("Manage customer orders.")]
public class OrdersController(
    IValidatorService validator,
    IOrderCommandUnit commands,
    IOrderQueryUnit queries) : Controller
{
    [DisplayName("Order list")]
    [Description("Show a filtered, sorted and paged list of orders.")]
    public async Task<IActionResult> Index([FromQuery] OrderFilter filter, CancellationToken ct)
    {
        var result = await queries.GetOrdersAsync(new GetOrdersQuery(filter), ct);
        return result.ToView(this);
    }

    [HttpPost]
    [DisplayName("Create order")]
    [Description("Validate and create a new order.")]
    public async Task<IActionResult> Create(CreateOrderCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return validation.ToResult().ToForm(this);

        var result = await commands.CreateAsync(command, ct);
        return result.ToForm(this, successAction: nameof(Index));
    }
}
```

# Workflow when asked to add a feature
1. Ask which store is used (SQL entity or NoSQL document) if it is not clear.
2. Domain: class + nested `Rules` (all limits) + static factory + behavior + events + enums (in `Domain/{Domain}/Enums`) + repository interface.
3. Contracts: DTOs.
4. Persistence: configuration class (table, schema, Fluent API using `Rules`, enum conversion), repository implementation, DI.
5. Application: command/query files (type + Mapping + validator using `Rules` + handler in one file), command unit, query unit, DI.
6. MVC: controller with `DisplayName` + `Description` on class and actions, filter class for Index, return with `ToView` / `ToForm`.
7. Before output, scan the code for literal numbers/strings used as limits. Move each one into `Rules`.
8. Output files grouped by layer with their full paths.
9. Bump AppVersion in `src/AzureTest.MVC/appsettings*.json` (run `python3 scripts/bump_version.py`) after making source code changes.

# Versioning
- The application version (e.g. `V3.1`) is stored in `appsettings.json`, `appsettings.Development.json`, and `appsettings.ProductionSimulateLocal.json` under the key `"AppVersion"`.
- It is displayed directly below `TrackerPro` in the sidebar (`_Sidebar.cshtml`).
- Every time source code changes are implemented, increment `AppVersion` using `python3 scripts/bump_version.py`.

# Do not
- Do not use MediatR, AutoMapper, Mapster or any mapping library.
- Do not use data annotations for EF mapping (use Fluent API).
- Do not hard-code max/min length, precision, scale, range or pattern values in Persistence, validators or MVC. Use `{Domain}.Rules`.
- Do not define the same rule value in two places.
- Do not put validators, mapping or handlers in separate files from their command/query.
- Do not inject handlers or repositories into controllers.
- Do not expose domain entities outside the Application layer.
- Do not define enums outside the Domain layer.
- Do not set audit or soft-delete fields manually.
- Do not call `SaveChanges` in repositories or controllers.
- Do not add business logic to controllers, units or mapping classes.
- Do not write a controller or action without `[DisplayName]` and `[Description]`.
- Do not map `Result` to `IActionResult` manually; use `ToView` / `ToForm`.
