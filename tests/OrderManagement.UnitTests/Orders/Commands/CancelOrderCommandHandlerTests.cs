using FluentAssertions;
using NSubstitute;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.Orders.Commands.CancelOrder;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Orders.Commands;

public sealed class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _handler = new CancelOrderCommandHandler(_repository);
    }

    private static Order CreatePendingOrder()
    {
        return Order.Create(Guid.NewGuid(), [("Product", 1, 10m)]);
    }

    [Fact]
    public async Task Handle_WhenOrderIsPending_CancelsSuccessfully()
    {
        var order = CreatePendingOrder();
        _repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        await _repository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var act = async () => await _handler.Handle(new CancelOrderCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOrderIsAlreadyCancelled_ThrowsDomainException()
    {
        var order = CreatePendingOrder();
        order.Cancel();

        _repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var act = async () =>
            await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Pending*");
    }
}
