namespace DesignPatterns.Patterns.Mediator;

// WHEN TO USE: many objects need to talk to each other but you don't want
// them referencing each other directly — a mediator centralizes the
// interaction so colleagues stay decoupled. Think MediatR in ASP.NET Core:
// controllers send requests, handlers process them, neither knows the other.
public interface IMediator
{
    void Register(string name, Colleague colleague);
    void Send(string message, string from, string to);
}

public abstract class Colleague
{
    protected readonly IMediator Mediator;
    public string Name { get; }

    protected Colleague(IMediator mediator, string name)
    {
        Mediator = mediator;
        Name = name;
        mediator.Register(name, this);
    }

    public void Send(string message, string to) => Mediator.Send(message, Name, to);

    public virtual void Receive(string message, string from) =>
        Console.WriteLine($"{Name} received from {from}: \"{message}\"");
}

// The only class that knows about everyone — colleagues only know the mediator.
public class ChatMediator : IMediator
{
    private readonly Dictionary<string, Colleague> _colleagues = new();

    public void Register(string name, Colleague colleague) => _colleagues[name] = colleague;

    public void Send(string message, string from, string to)
    {
        if (_colleagues.TryGetValue(to, out var colleague))
            colleague.Receive(message, from);
        else
            Console.WriteLine($"[mediator] no colleague named '{to}' — message from {from} dropped.");
    }
}

public class User : Colleague
{
    public User(IMediator mediator, string name) : base(mediator, name) { }
}

public static class MediatorDemo
{
    public static void Run()
    {
        IMediator chat = new ChatMediator();
        var layla = new User(chat, "Layla");
        var omar = new User(chat, "Omar");

        layla.Send("Anyone up for a code review?", "Omar");
        omar.Send("Sure — send the PR link!", "Layla");
        omar.Send("Hello?", "Ghost"); // nobody by that name: dropped by the mediator
    }
}
