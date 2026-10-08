namespace DesignPatterns.Patterns.Strategy;

// WHEN TO USE: you have several interchangeable algorithms and want to switch
// between them at runtime — without if/else or switch chains scattered through
// the client code. Each algorithm lives in its own class behind one interface.
public interface IDiscountStrategy
{
    decimal Apply(decimal total);
}

public class NoDiscount : IDiscountStrategy
{
    public decimal Apply(decimal total) => total;
}

public class PercentageDiscount : IDiscountStrategy
{
    private readonly decimal _percent;

    public PercentageDiscount(decimal percent) => _percent = percent;

    public decimal Apply(decimal total) => total * (1 - _percent / 100);
}

public class FixedDiscount : IDiscountStrategy
{
    private readonly decimal _amount;

    public FixedDiscount(decimal amount) => _amount = amount;

    public decimal Apply(decimal total) => Math.Max(0, total - _amount);
}

// The context holds a reference to the current strategy and delegates to it.
public class ShoppingCart
{
    private IDiscountStrategy _discount = new NoDiscount();

    public void SetDiscountStrategy(IDiscountStrategy discount) => _discount = discount;

    public decimal Checkout(decimal total) => _discount.Apply(total);
}

public static class StrategyDemo
{
    public static void Run()
    {
        var cart = new ShoppingCart();

        cart.SetDiscountStrategy(new NoDiscount());
        Console.WriteLine($"No discount: {cart.Checkout(200):C}");

        cart.SetDiscountStrategy(new PercentageDiscount(10));
        Console.WriteLine($"10% off:     {cart.Checkout(200):C}");

        cart.SetDiscountStrategy(new FixedDiscount(50));
        Console.WriteLine($"$50 off:     {cart.Checkout(200):C}");
    }
}
