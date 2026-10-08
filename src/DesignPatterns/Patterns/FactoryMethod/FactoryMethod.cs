namespace DesignPatterns.Patterns.FactoryMethod;

// WHEN TO USE: client code must create objects but shouldn't know their concrete
// classes. The factory method lets subclasses decide which class to instantiate,
// so adding a new product type means adding a subclass, not editing existing code.
public interface INotificationSender
{
    void Send(string to, string message);
}

public class EmailSender : INotificationSender
{
    public void Send(string to, string message)
        => Console.WriteLine($"Email to {to}: {message}");
}

public class SmsSender : INotificationSender
{
    public void Send(string to, string message)
        => Console.WriteLine($"SMS to {to}: {message}");
}

// The creator declares the factory method; subclasses override it.
// Shared business logic stays here, object creation is delegated.
public abstract class NotificationService
{
    protected abstract INotificationSender CreateSender();

    public void Notify(string to, string message)
    {
        var sender = CreateSender();
        sender.Send(to, message);
    }
}

public class EmailNotificationService : NotificationService
{
    protected override INotificationSender CreateSender() => new EmailSender();
}

public class SmsNotificationService : NotificationService
{
    protected override INotificationSender CreateSender() => new SmsSender();
}

public static class FactoryMethodDemo
{
    public static void Run()
    {
        NotificationService email = new EmailNotificationService();
        email.Notify("user@example.com", "Your order has shipped.");

        NotificationService sms = new SmsNotificationService();
        sms.Notify("+201000000000", "Your verification code is 1234.");
    }
}
