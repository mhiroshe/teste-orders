using MediatR;

namespace OrderManagement.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest;
