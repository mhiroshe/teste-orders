using MediatR;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Orders;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        return order.ToDto();
    }
}
