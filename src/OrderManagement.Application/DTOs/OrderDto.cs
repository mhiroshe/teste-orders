using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    string StatusName,
    DateTime CreatedAt,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items
);
