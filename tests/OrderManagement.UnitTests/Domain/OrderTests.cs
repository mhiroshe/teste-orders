using FluentAssertions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public sealed class OrderTests
{
    private static IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> ValidItems() =>
    [
        ("Product A", 2, 10.00m),
        ("Product B", 1, 25.50m)
    ];

    [Fact]
    public void Create_WithValidData_ReturnsOrderWithPendingStatus()
    {
        var customerId = Guid.NewGuid();

        var order = Order.Create(customerId, ValidItems());

        order.Should().NotBeNull();
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().HaveCount(2);
        order.Id.Should().NotBeEmpty();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNoItems_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.NewGuid(), []);

        act.Should().Throw<DomainException>()
            .WithMessage("*at least one item*");
    }

    [Fact]
    public void TotalAmount_IsCalculatedCorrectlyInDomain()
    {
        var order = Order.Create(Guid.NewGuid(), ValidItems());

        order.TotalAmount.Should().Be(45.50m);
    }

    [Fact]
    public void Cancel_WhenPending_SetsStatusToCancelled()
    {
        var order = Order.Create(Guid.NewGuid(), ValidItems());

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidItems());
        order.Cancel();

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("*Pending*");
    }

    [Fact]
    public void Cancel_WhenConfirmed_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidItems());
        order.Confirm();

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("*Pending*");
    }

    [Fact]
    public void Confirm_WhenPending_SetsStatusToConfirmed()
    {
        var order = Order.Create(Guid.NewGuid(), ValidItems());

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }
}
