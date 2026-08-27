using FluentAssertions;
using NSubstitute;
using OrderManagement.Application.Orders.Commands.CreateOrder;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Orders.Commands;

public sealed class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsOrderId()
    {
        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            Items:
            [
                new CreateOrderItemDto("Product A", 2, 15.00m),
                new CreateOrderItemDto("Product B", 1, 30.00m)
            ]
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectOrderData()
    {
        var customerId = Guid.NewGuid();
        Order? capturedOrder = null;

        await _repository.AddAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        var command = new CreateOrderCommand(
            CustomerId: customerId,
            Items: [new CreateOrderItemDto("Widget", 3, 10.00m)]
        );

        await _handler.Handle(command, CancellationToken.None);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.CustomerId.Should().Be(customerId);
        capturedOrder.Items.Should().HaveCount(1);
        capturedOrder.TotalAmount.Should().Be(30.00m);
    }
}
