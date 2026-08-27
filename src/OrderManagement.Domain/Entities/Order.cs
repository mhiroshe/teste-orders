using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.Total);

    private Order() { }

    public static Order Create(
        Guid customerId,
        IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> itemData)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (productName, quantity, unitPrice) in itemData)
            order._items.Add(OrderItem.Create(order.Id, productName, quantity, unitPrice));

        if (order._items.Count == 0)
            throw new DomainException("An order must have at least one item.");

        return order;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only orders with status Pending can be cancelled.");

        Status = OrderStatus.Cancelled;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only orders with status Pending can be confirmed.");

        Status = OrderStatus.Confirmed;
    }
}
