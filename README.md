# csharp-design-patterns

Nine classic design patterns in C#, each as a small self-contained console demo with a `WHEN TO USE` comment explaining the problem the pattern solves. Pick a pattern from the menu and watch it run.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)

## Run the demos

```bash
dotnet run --project src/DesignPatterns
```

## Run the tests

```bash
dotnet test
```

27 xUnit tests cover every pattern's behavior — including a regression test for the Observer re-entrancy fix (see below).

## Patterns

| # | Pattern | Folder | Use it when... |
|---|---|---|---|
| 1 | Singleton | `src/DesignPatterns/Patterns/Singleton` | You need exactly one shared instance (logging, config, caches) |
| 2 | Factory Method | `src/DesignPatterns/Patterns/FactoryMethod` | Client code must create objects without knowing their concrete classes |
| 3 | Strategy | `src/DesignPatterns/Patterns/Strategy` | You have interchangeable algorithms to switch at runtime (discounts, sorting, pricing) |
| 4 | Observer | `src/DesignPatterns/Patterns/Observer` | One object's change must notify many dependents without tight coupling |
| 5 | Decorator | `src/DesignPatterns/Patterns/Decorator` | You need to add behavior (logging, caching) without subclassing everything |
| 6 | Repository + Unit of Work | `src/DesignPatterns/Patterns/RepositoryUnitOfWork` | Business logic shouldn't depend on EF Core/SQL directly; changes commit together |
| 7 | Dependency Injection | `src/DesignPatterns/Patterns/DependencyInjection` | A class needs collaborators but shouldn't create them itself |
| 8 | Mediator | `src/DesignPatterns/Patterns/Mediator` | Many objects must communicate without referencing each other directly (think MediatR) |
| 9 | Builder | `src/DesignPatterns/Patterns/Builder` | A complex object needs step-by-step construction; you want an invalid state to be unrepresentable |

The Repository demo is in-memory so it runs without a database — swap in an EF Core repository in a real app and the calling code doesn't change.

## Builder example

```csharp
var order = new OrderBuilder()
    .ForCustomer("Eslam")
    .AddItem("Keyboard", 75m)
    .AddItem("Mouse", 25m)
    .WithDiscount(10)
    .Build();

Console.WriteLine(order);
// Order for Eslam: 2 item(s), subtotal $100.00, discount 10%, total $90.00
```

Skipping a required step fails fast instead of building a broken object:

```csharp
new OrderBuilder().AddItem("Keyboard", 75m).Build();
// InvalidOperationException: An order must have a customer — call ForCustomer() first.
```

## Notes on the implementations

- **Observer** notifies over a snapshot of the subscriber list, so an observer may safely unsubscribe (or subscribe) from inside its own `Update()` callback — modifying the live list mid-notification would throw `InvalidOperationException`.
- **Decorator**'s cache uses `ConcurrentDictionary`, so stacked decorators stay correct under concurrent use.
- **DI** includes a tiny hand-rolled container (`SimpleContainer`) — in ASP.NET Core the built-in container does this via `services.AddScoped<IMessageSender, EmailSender>()`.

## NuGet

Install the demos as a package:

```bash
dotnet add package SENESO.csharp-design-patterns
```

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

## License

[MIT](LICENSE) — © 2026 Eslam Ashraf
