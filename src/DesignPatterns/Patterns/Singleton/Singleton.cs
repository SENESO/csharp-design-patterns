namespace DesignPatterns.Patterns.Singleton;

// WHEN TO USE: you need exactly one instance of a class shared across the app
// (logging, configuration, in-memory caches). In ASP.NET Core, prefer registering
// the service as a singleton in DI instead of hand-rolling this.
public sealed class Logger
{
    // Lazy<T> makes initialization thread-safe with no explicit locking.
    private static readonly Lazy<Logger> _instance = new(() => new Logger());

    public static Logger Instance => _instance.Value;

    // Private constructor: nobody else can create an instance.
    private Logger() { }

    public void Log(string message)
        => Console.WriteLine($"[LOG {DateTime.Now:HH:mm:ss}] {message}");
}

public static class SingletonDemo
{
    public static void Run()
    {
        var first = Logger.Instance;
        var second = Logger.Instance;

        first.Log("Application started.");
        second.Log("Processing request.");

        Console.WriteLine($"Both references point to the same instance: {ReferenceEquals(first, second)}");
    }
}
