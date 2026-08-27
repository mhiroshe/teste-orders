using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Orders;

internal static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order) =>
        new(
            Id: order.Id,
            CustomerId: order.CustomerId,
            Status: order.Status,
            StatusName: order.Status.ToString(),
            CreatedAt: order.CreatedAt,
            TotalAmount: order.TotalAmount,
            Items: order.Items.Select(i => i.ToDto()).ToList()
        );

    public static OrderItemDto ToDto(this OrderItem item) =>
        new(
            Id: item.Id,
            ProductName: item.ProductName,
            Quantity: item.Quantity,
            UnitPrice: item.UnitPrice,
            Total: item.Total
        );
}
