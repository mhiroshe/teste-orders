using FluentAssertions;
using NSubstitute;
using OrderManagement.Application.Orders.Queries.GetOrders;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Orders.Queries;

public sealed class GetOrdersQueryHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _handler = new GetOrdersQueryHandler(_repository);
    }

    private static Order CreateOrder() =>
        Order.Create(Guid.NewGuid(), [("Product A", 1, 10.00m)]);

    [Fact]
    public async Task Handle_ReturnsPagedResultWithMappedItems()
    {
        var orders = new[] { CreateOrder(), CreateOrder() };
        _repository.GetPagedAsync(1, 10, Arg.Any<CancellationToken>())
            .Returns((orders, 2));

        var result = await _handler.Handle(new GetOrdersQuery(1, 10), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public async Task Handle_ClampsPageToAtLeastOne(int requestedPage, int expectedPage)
    {
        _repository.GetPagedAsync(expectedPage, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(([], 0));

        var result = await _handler.Handle(new GetOrdersQuery(requestedPage, 10), CancellationToken.None);

        result.Page.Should().Be(expectedPage);
        await _repository.Received(1).GetPagedAsync(expectedPage, Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(101, 100)]
    [InlineData(50, 50)]
    public async Task Handle_ClampsPageSizeBetweenOneAndOneHundred(int requestedPageSize, int expectedPageSize)
    {
        _repository.GetPagedAsync(Arg.Any<int>(), expectedPageSize, Arg.Any<CancellationToken>())
            .Returns(([], 0));

        var result = await _handler.Handle(new GetOrdersQuery(1, requestedPageSize), CancellationToken.None);

        result.PageSize.Should().Be(expectedPageSize);
        await _repository.Received(1).GetPagedAsync(Arg.Any<int>(), expectedPageSize, Arg.Any<CancellationToken>());
    }
}
