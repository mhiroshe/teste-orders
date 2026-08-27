using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Orders;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await orderRepository.GetPagedAsync(page, pageSize, cancellationToken);

        return new PagedResult<OrderDto>(
            Items: items.Select(o => o.ToDto()).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }
}
