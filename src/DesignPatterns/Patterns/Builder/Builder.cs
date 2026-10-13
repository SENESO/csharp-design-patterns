namespace DesignPatterns.Patterns.Builder;

// WHEN TO USE: building a complex object takes many steps or has lots of
// optional parts — a constructor with 8 parameters (or 5 overloads) becomes
// unreadable and error-prone. The builder lets the caller say exactly what
// they want, step by step, and validates everything once in Build().
// Compare with Factory Method: the factory picks the *type* to create,
// the builder assembles *one* complex object piece by piece.
public class Order
{
    public string Customer { get; }
    public IReadOnlyList<(string Name, decimal Price)> Items { get; }
    public decimal DiscountPercent { get; }

    public decimal Subtotal => Items.Sum(i => i.Price);
    public decimal Total => Subtotal * (1 - DiscountPercent / 100);

    internal Order(string customer, List<(string Name, decimal Price)> items, decimal discountPercent)
    {
        Customer = customer;
        Items = items.AsReadOnly();
        DiscountPercent = discountPercent;
    }

    public override string ToString() =>
        $"Order for {Customer}: {Items.Count} item(s), subtotal {Subtotal:C}, " +
        $"discount {DiscountPercent}%, total {Total:C}";
}

// Fluent builder: every step returns the builder so calls chain.
public class OrderBuilder
{
    private string? _customer;
    private readonly List<(string Name, decimal Price)> _items = new();
    private decimal _discountPercent;

    public OrderBuilder ForCustomer(string customer)
    {
        _customer = customer;
        return this;
    }

    public OrderBuilder AddItem(string name, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        _items.Add((name, price));
        return this;
    }

    public OrderBuilder WithDiscount(decimal percent)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(percent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, 100);
        _discountPercent = percent;
        return this;
    }

    // The single place where the object is validated and created:
    // an Order can never exist in a half-built, invalid state.
    public Order Build()
    {
        if (string.IsNullOrWhiteSpace(_customer))
            throw new InvalidOperationException("An order must have a customer — call ForCustomer() first.");
        if (_items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one item — call AddItem() first.");
        return new Order(_customer, _items, _discountPercent);
    }
}

public static class BuilderDemo
{
    public static void Run()
    {
        var order = new OrderBuilder()
            .ForCustomer("Eslam")
            .AddItem("Keyboard", 75m)
            .AddItem("Mouse", 25m)
            .WithDiscount(10)
            .Build();

        Console.WriteLine(order);

        // Skipping required steps fails fast instead of building a broken order.
        try
        {
            new OrderBuilder().AddItem("Keyboard", 75m).Build();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Build rejected: {ex.Message}");
        }
    }
}
