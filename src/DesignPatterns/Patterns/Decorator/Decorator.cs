namespace DesignPatterns.Patterns.Decorator;

// WHEN TO USE: you need to add behavior to an object (logging, caching,
// validation) without modifying its class and without an explosion of
// subclasses for every behavior combination. Decorators stack.
public interface IOrderService
{
    string GetOrder(int id);
}

public class OrderService : IOrderService
{
    public string GetOrder(int id) => $"Order #{id}: 2x Keyboard, 1x Mouse";
}

// Base decorator: implements the same interface and forwards calls inward.
public abstract class OrderServiceDecorator : IOrderService
{
    protected readonly IOrderService _inner;

    protected OrderServiceDecorator(IOrderService inner) => _inner = inner;

    public virtual string GetOrder(int id) => _inner.GetOrder(id);
}

public class LoggingOrderService : OrderServiceDecorator
{
    public LoggingOrderService(IOrderService inner) : base(inner) { }

    public override string GetOrder(int id)
    {
        Console.WriteLine($"[LOG] Fetching order {id}...");
        var result = base.GetOrder(id);
        Console.WriteLine("[LOG] Done.");
        return result;
    }
}

public class CachingOrderService : OrderServiceDecorator
{
    private readonly Dictionary<int, string> _cache = new();

    public CachingOrderService(IOrderService inner) : base(inner) { }

    public override string GetOrder(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            Console.WriteLine($"[CACHE] Hit for order {id}");
            return cached;
        }

        Console.WriteLine($"[CACHE] Miss for order {id}");
        var result = base.GetOrder(id);
        _cache[id] = result;
        return result;
    }
}

public static class DecoratorDemo
{
    public static void Run()
    {
        // Stack them: caching on the outside, logging on the inside.
        IOrderService service = new CachingOrderService(
            new LoggingOrderService(
                new OrderService()));

        Console.WriteLine(service.GetOrder(1));
        Console.WriteLine(service.GetOrder(1)); // served from cache, no log lines
    }
}
