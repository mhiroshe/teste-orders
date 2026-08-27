using MediatR;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            customerId: request.CustomerId,
            itemData: request.Items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice))
        );

        await orderRepository.AddAsync(order, cancellationToken);

        return order.Id;
    }
}
