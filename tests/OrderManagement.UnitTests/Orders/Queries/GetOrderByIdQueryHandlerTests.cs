using FluentAssertions;
using NSubstitute;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.Orders.Queries.GetOrderById;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Orders.Queries;

public sealed class GetOrderByIdQueryHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _handler = new GetOrderByIdQueryHandler(_repository);
    }

    private static Order CreateOrder()
    {
        return Order.Create(Guid.NewGuid(), [("Product A", 2, 15.00m)]);
    }

    [Fact]
    public async Task Handle_WhenOrderExists_ReturnsOrderDto()
    {
        var order = CreateOrder();
        _repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
        result.TotalAmount.Should().Be(30.00m);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var act = async () =>
            await _handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
