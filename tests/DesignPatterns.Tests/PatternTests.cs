using DesignPatterns.Patterns.Builder;
using DesignPatterns.Patterns.Decorator;
using DesignPatterns.Patterns.DependencyInjection;
using DesignPatterns.Patterns.FactoryMethod;
using DesignPatterns.Patterns.Mediator;
using DesignPatterns.Patterns.Observer;
using DesignPatterns.Patterns.RepositoryUnitOfWork;
using DesignPatterns.Patterns.Singleton;
using DesignPatterns.Patterns.Strategy;
using Xunit;
using DIEmailSender = DesignPatterns.Patterns.DependencyInjection.EmailSender;
using DISmsSender = DesignPatterns.Patterns.DependencyInjection.SmsSender;

// Console output is process-global, so tests must not run in parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DesignPatterns.Tests;

public static class ConsoleCapture
{
    public static string Capture(Action action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(original);
        }
        return writer.ToString();
    }
}

public class SingletonTests
{
    [Fact]
    public void Instance_AlwaysReturnsSameObject()
    {
        Assert.Same(Logger.Instance, Logger.Instance);
    }

    [Fact]
    public void Instance_IsThreadSafe()
    {
        Logger[] instances = new Logger[16];
        Parallel.For(0, instances.Length, i => instances[i] = Logger.Instance);
        Assert.All(instances, i => Assert.Same(instances[0], i));
    }
}

public class FactoryMethodTests
{
    [Fact]
    public void EmailService_SendsEmail()
    {
        var output = ConsoleCapture.Capture(() =>
            new EmailNotificationService().Notify("user@example.com", "Hi"));
        Assert.Contains("Email to user@example.com: Hi", output);
    }

    [Fact]
    public void SmsService_SendsSms()
    {
        var output = ConsoleCapture.Capture(() =>
            new SmsNotificationService().Notify("+201000000000", "Code"));
        Assert.Contains("SMS to +201000000000: Code", output);
    }
}

public class StrategyTests
{
    [Theory]
    [InlineData(200, 200)]   // no discount
    public void NoDiscount_ReturnsTotal(decimal total, decimal expected)
        => Assert.Equal(expected, new ShoppingCart().Checkout(total));

    [Fact]
    public void PercentageDiscount_AppliesCorrectly()
    {
        var cart = new ShoppingCart();
        cart.SetDiscountStrategy(new PercentageDiscount(10));
        Assert.Equal(180m, cart.Checkout(200m));
    }

    [Fact]
    public void FixedDiscount_AppliesCorrectly()
    {
        var cart = new ShoppingCart();
        cart.SetDiscountStrategy(new FixedDiscount(50));
        Assert.Equal(150m, cart.Checkout(200m));
    }

    [Fact]
    public void FixedDiscount_NeverGoesBelowZero()
    {
        var cart = new ShoppingCart();
        cart.SetDiscountStrategy(new FixedDiscount(500));
        Assert.Equal(0m, cart.Checkout(200m));
    }

    [Fact]
    public void Strategy_CanBeSwitchedAtRuntime()
    {
        var cart = new ShoppingCart();
        cart.SetDiscountStrategy(new PercentageDiscount(50));
        Assert.Equal(100m, cart.Checkout(200m));
        cart.SetDiscountStrategy(new NoDiscount());
        Assert.Equal(200m, cart.Checkout(200m));
    }
}

public class DecoratorTests
{
    [Fact]
    public void SecondCall_IsServedFromCache()
    {
        IOrderService service = new CachingOrderService(new OrderService());

        var first = ConsoleCapture.Capture(() => service.GetOrder(1));
        var second = ConsoleCapture.Capture(() => service.GetOrder(1));

        Assert.Contains("[CACHE] Miss", first);
        Assert.Contains("[CACHE] Hit", second);
    }

    [Fact]
    public void StackedDecorators_ReturnInnerResult()
    {
        IOrderService service = new CachingOrderService(
            new LoggingOrderService(new OrderService()));
        var output = ConsoleCapture.Capture(() =>
            Assert.Equal("Order #7: 2x Keyboard, 1x Mouse", service.GetOrder(7)));
        Assert.Contains("[LOG] Fetching order 7...", output);
    }
}

public class ObserverTests
{
    private sealed class SelfDetachingInvestor : IInvestor
    {
        private readonly Stock _stock;
        public int Notifications { get; private set; }

        public SelfDetachingInvestor(Stock stock) => _stock = stock;

        public void Update(string symbol, decimal price)
        {
            Notifications++;
            _stock.Detach(this); // unsubscribe from inside the callback
        }
    }

    [Fact]
    public void DetachingDuringNotify_DoesNotThrow()
    {
        var stock = new Stock("MSFT", 400m);
        var selfDetaching = new SelfDetachingInvestor(stock);
        var other = new Investor("Omar");
        stock.Attach(selfDetaching);
        stock.Attach(other);

        // Before the snapshot fix this threw InvalidOperationException
        // ("Collection was modified") when reaching the second observer.
        var exception = Record.Exception(() => stock.SetPrice(415m));

        Assert.Null(exception);
        Assert.Equal(1, selfDetaching.Notifications);
    }

    [Fact]
    public void DetachedObserver_StopsReceivingUpdates()
    {
        var stock = new Stock("MSFT", 400m);
        var mona = new Investor("Mona");
        stock.Attach(mona);
        stock.Detach(mona);

        var output = ConsoleCapture.Capture(() => stock.SetPrice(420m));
        Assert.DoesNotContain("Mona notified", output);
    }

    [Fact]
    public void AttachedObservers_AreNotified()
    {
        var stock = new Stock("MSFT", 400m);
        stock.Attach(new Investor("Mona"));

        var output = ConsoleCapture.Capture(() => stock.SetPrice(415.50m));
        Assert.Contains("Mona notified: MSFT is now", output);
    }
}

public class RepositoryUnitOfWorkTests
{
    [Fact]
    public void Add_ThenGetById_ReturnsEntity()
    {
        using var uow = new UnitOfWork();
        uow.Customers.Add(new Customer { Id = 1, Name = "Eslam" });

        var customer = uow.Customers.GetById(1);

        Assert.NotNull(customer);
        Assert.Equal("Eslam", customer.Name);
    }

    [Fact]
    public void Remove_DeletesEntity()
    {
        using var uow = new UnitOfWork();
        var customer = new Customer { Id = 1, Name = "Eslam" };
        uow.Customers.Add(customer);
        uow.Customers.Remove(customer);

        Assert.Null(uow.Customers.GetById(1));
    }

    [Fact]
    public void GetAll_ReturnsEveryAddedEntity()
    {
        using var uow = new UnitOfWork();
        uow.Customers.Add(new Customer { Id = 1, Name = "Eslam" });
        uow.Customers.Add(new Customer { Id = 2, Name = "Mona" });

        Assert.Equal(2, uow.Customers.GetAll().Count());
    }
}

public class DependencyInjectionTests
{
    [Fact]
    public void Resolve_ReturnsWiredGraph()
    {
        var container = new SimpleContainer();
        container.Register<IMessageSender, DIEmailSender>();
        container.Register(() => new NotificationManager(container.Resolve<IMessageSender>()));

        var output = ConsoleCapture.Capture(() =>
            container.Resolve<NotificationManager>().Notify("Hello"));

        Assert.Contains("Email sent: Hello", output);
    }

    [Fact]
    public void SwappingRegistration_SwapsBehaviorWithoutChangingConsumer()
    {
        var container = new SimpleContainer();
        container.Register<IMessageSender, DISmsSender>();
        container.Register(() => new NotificationManager(container.Resolve<IMessageSender>()));

        var output = ConsoleCapture.Capture(() =>
            container.Resolve<NotificationManager>().Notify("Hello"));

        Assert.Contains("SMS sent: Hello", output);
    }

    [Fact]
    public void Resolve_UnregisteredService_Throws()
    {
        var container = new SimpleContainer();
        Assert.Throws<InvalidOperationException>(() => container.Resolve<IMessageSender>());
    }
}

public class MediatorTests
{
    [Fact]
    public void Send_DeliversToNamedColleague()
    {
        IMediator chat = new ChatMediator();
        var layla = new User(chat, "Layla");
        var omar = new User(chat, "Omar");

        var output = ConsoleCapture.Capture(() => layla.Send("Hi Omar", "Omar"));

        Assert.Contains("Omar received from Layla: \"Hi Omar\"", output);
    }

    [Fact]
    public void Send_ToUnknownColleague_IsDroppedWithWarning()
    {
        IMediator chat = new ChatMediator();
        var omar = new User(chat, "Omar");

        var output = ConsoleCapture.Capture(() => omar.Send("Hello?", "Ghost"));

        Assert.Contains("no colleague named 'Ghost'", output);
    }
}

public class BuilderTests
{
    [Fact]
    public void FluentChain_BuildsOrderWithCorrectTotals()
    {
        var order = new OrderBuilder()
            .ForCustomer("Eslam")
            .AddItem("Keyboard", 75m)
            .AddItem("Mouse", 25m)
            .WithDiscount(10)
            .Build();

        Assert.Equal("Eslam", order.Customer);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(100m, order.Subtotal);
        Assert.Equal(90m, order.Total);
    }

    [Fact]
    public void Build_WithoutCustomer_Throws()
    {
        var builder = new OrderBuilder().AddItem("Keyboard", 75m);
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithoutItems_Throws()
    {
        var builder = new OrderBuilder().ForCustomer("Eslam");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void AddItem_WithNegativePrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OrderBuilder().AddItem("Keyboard", -1m));
    }

    [Fact]
    public void WithDiscount_Above100_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OrderBuilder().WithDiscount(101));
    }
}
