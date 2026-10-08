using DesignPatterns.Patterns.Decorator;
using DesignPatterns.Patterns.DependencyInjection;
using DesignPatterns.Patterns.FactoryMethod;
using DesignPatterns.Patterns.Observer;
using DesignPatterns.Patterns.RepositoryUnitOfWork;
using DesignPatterns.Patterns.Singleton;
using DesignPatterns.Patterns.Strategy;

var demos = new Dictionary<string, (string Name, Action Run)>
{
    ["1"] = ("Singleton", SingletonDemo.Run),
    ["2"] = ("Factory Method", FactoryMethodDemo.Run),
    ["3"] = ("Strategy", StrategyDemo.Run),
    ["4"] = ("Observer", ObserverDemo.Run),
    ["5"] = ("Decorator", DecoratorDemo.Run),
    ["6"] = ("Repository + Unit of Work", RepositoryUnitOfWorkDemo.Run),
    ["7"] = ("Dependency Injection", DependencyInjectionDemo.Run),
};

while (true)
{
    Console.WriteLine("=== C# Design Patterns ===");
    foreach (var (key, demo) in demos)
        Console.WriteLine($"  {key}. {demo.Name}");
    Console.WriteLine("  0. Exit");
    Console.Write("Pick a pattern: ");

    var choice = Console.ReadLine()?.Trim();

    if (choice == "0" || string.IsNullOrEmpty(choice))
        break;

    if (demos.TryGetValue(choice, out var selected))
    {
        Console.WriteLine($"\n--- {selected.Name} ---");
        selected.Run();
    }
    else
    {
        Console.WriteLine("Unknown option, try again.");
    }

    Console.WriteLine();
}
