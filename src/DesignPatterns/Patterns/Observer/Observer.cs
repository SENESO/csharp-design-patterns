namespace DesignPatterns.Patterns.Observer;

// WHEN TO USE: one object's state change must notify many dependents
// (event systems, pub/sub, UI data binding) without the subject knowing
// who they are. Observers subscribe and unsubscribe independently.
public interface IInvestor
{
    void Update(string symbol, decimal price);
}

public class Investor : IInvestor
{
    private readonly string _name;

    public Investor(string name) => _name = name;

    public void Update(string symbol, decimal price)
        => Console.WriteLine($"{_name} notified: {symbol} is now {price:C}");
}

// The subject. It only knows the IInvestor interface, not the concrete classes.
public class Stock
{
    private readonly List<IInvestor> _investors = new();
    private decimal _price;

    public Stock(string symbol, decimal price)
    {
        Symbol = symbol;
        _price = price;
    }

    public string Symbol { get; }

    public void Attach(IInvestor investor) => _investors.Add(investor);
    public void Detach(IInvestor investor) => _investors.Remove(investor);

    public void SetPrice(decimal price)
    {
        _price = price;
        Notify();
    }

    private void Notify()
    {
        foreach (var investor in _investors)
            investor.Update(Symbol, _price);
    }
}

public static class ObserverDemo
{
    public static void Run()
    {
        var stock = new Stock("MSFT", 400m);

        var mona = new Investor("Mona");
        var omar = new Investor("Omar");
        stock.Attach(mona);
        stock.Attach(omar);

        stock.SetPrice(415.50m);

        stock.Detach(mona);
        Console.WriteLine("-- Mona unsubscribed --");
        stock.SetPrice(420m);
    }
}
