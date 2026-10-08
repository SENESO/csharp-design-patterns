namespace DesignPatterns.Patterns.RepositoryUnitOfWork;

// WHEN TO USE: business logic shouldn't depend on EF Core or SQL directly.
// The repository hides data access behind an interface; the Unit of Work groups
// several repository changes and commits them together (one transaction).
// (This demo is in-memory so it runs anywhere; swap the repository for an
// EF Core one in a real app and the calling code doesn't change.)
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IRepository<T> where T : class
{
    void Add(T entity);
    void Remove(T entity);
    T? GetById(int id);
    IEnumerable<T> GetAll();
}

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _store = new();
    private readonly Func<T, int> _getId;

    public InMemoryRepository(Func<T, int> getId) => _getId = getId;

    public void Add(T entity) => _store.Add(entity);
    public void Remove(T entity) => _store.Remove(entity);
    public T? GetById(int id) => _store.FirstOrDefault(e => _getId(e) == id);
    public IEnumerable<T> GetAll() => _store.AsReadOnly();
}

// Unit of Work: exposes the repositories and commits everything at once.
public interface IUnitOfWork : IDisposable
{
    IRepository<Customer> Customers { get; }
    int Save();
}

public class UnitOfWork : IUnitOfWork
{
    public IRepository<Customer> Customers { get; } =
        new InMemoryRepository<Customer>(c => c.Id);

    public int Save()
    {
        // In a real app this calls DbContext.SaveChanges() — one transaction
        // covering every repository change made since the last save.
        Console.WriteLine("Changes committed.");
        return 1;
    }

    public void Dispose() { }
}

public static class RepositoryUnitOfWorkDemo
{
    public static void Run()
    {
        using var uow = new UnitOfWork();

        uow.Customers.Add(new Customer { Id = 1, Name = "Eslam" });
        uow.Customers.Add(new Customer { Id = 2, Name = "Mona" });
        uow.Save();

        foreach (var customer in uow.Customers.GetAll())
            Console.WriteLine($"{customer.Id}: {customer.Name}");
    }
}
