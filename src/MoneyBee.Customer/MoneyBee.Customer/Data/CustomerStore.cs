using System.Collections.Concurrent;
using MoneyBee.Customer.Entities;

namespace MoneyBee.Customer.Data;

public class CustomerStore
{
    private readonly ConcurrentDictionary<Guid, Entities.Customer> _customers = new();
    private readonly ConcurrentDictionary<string, Guid> _nationalIdIndex = new();

    public Entities.Customer? GetById(Guid id)
    {
        _customers.TryGetValue(id, out var customer);
        return customer;
    }

    public Entities.Customer? GetByNationalId(string nationalId)
    {
        if (_nationalIdIndex.TryGetValue(nationalId, out var id))
        {
            return GetById(id);
        }
        return null;
    }

    public Entities.Customer Add(Entities.Customer customer)
    {
        _customers[customer.Id] = customer;
        _nationalIdIndex[customer.NationalId] = customer.Id;
        return customer;
    }

    public Entities.Customer Update(Entities.Customer customer)
    {
        _customers[customer.Id] = customer;
        return customer;
    }

    public IEnumerable<Entities.Customer> GetAll() => _customers.Values;
}
