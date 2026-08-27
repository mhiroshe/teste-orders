using MediatR;

namespace OrderManagement.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyList<CreateOrderItemDto> Items
) : IRequest<Guid>;

public sealed record CreateOrderItemDto(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
