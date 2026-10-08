# csharp-design-patterns

Seven classic design patterns in C#, each as a small self-contained console demo. Pick a pattern from the menu and watch it run.

```bash
dotnet run --project src/DesignPatterns
```

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

Each file starts with a `WHEN TO USE` comment explaining the problem the pattern solves. The Repository demo is in-memory so it runs without a database — swap in an EF Core repository in a real app and the calling code doesn't change.

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).
