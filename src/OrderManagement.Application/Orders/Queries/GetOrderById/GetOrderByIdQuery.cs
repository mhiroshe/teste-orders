using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
