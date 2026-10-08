namespace DesignPatterns.Patterns.DependencyInjection;

// WHEN TO USE: a class needs collaborators but shouldn't create them itself.
// Inject them through the constructor; a container wires the whole object graph
// at startup. In ASP.NET Core the built-in container does this via
// services.AddScoped<IMessageSender, EmailSender>().
public interface IMessageSender
{
    void Send(string message);
}

public class EmailSender : IMessageSender
{
    public void Send(string message) => Console.WriteLine($"Email sent: {message}");
}

public class SmsSender : IMessageSender
{
    public void Send(string message) => Console.WriteLine($"SMS sent: {message}");
}

// Depends on the abstraction IMessageSender, never on a concrete sender.
public class NotificationManager
{
    private readonly IMessageSender _sender;

    public NotificationManager(IMessageSender sender)
    {
        _sender = sender;
    }

    public void Notify(string message) => _sender.Send(message);
}

// A tiny hand-rolled container: register abstractions once, resolve anywhere.
public class SimpleContainer
{
    private readonly Dictionary<Type, Func<object>> _registrations = new();

    public void Register<TService, TImpl>() where TImpl : TService, new()
        => _registrations[typeof(TService)] = () => new TImpl();

    public void Register<TService>(Func<TService> factory) where TService : notnull
        => _registrations[typeof(TService)] = () => factory()!;

    public TService Resolve<TService>()
    {
        if (_registrations.TryGetValue(typeof(TService), out var factory))
            return (TService)factory();
        throw new InvalidOperationException($"No registration for {typeof(TService).Name}");
    }
}

public static class DependencyInjectionDemo
{
    public static void Run()
    {
        var container = new SimpleContainer();

        // Swap EmailSender for SmsSender here — NotificationManager never changes.
        container.Register<IMessageSender, EmailSender>();
        container.Register(() => new NotificationManager(container.Resolve<IMessageSender>()));

        var manager = container.Resolve<NotificationManager>();
        manager.Notify("Your build succeeded.");
    }
}
