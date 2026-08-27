using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<OrderDto>>;
