using FluentAssertions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public sealed class OrderItemTests
{
    [Fact]
    public void Create_WithValidData_ReturnsOrderItem()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Widget", 3, 9.99m);

        item.ProductName.Should().Be("Widget");
        item.Quantity.Should().Be(3);
        item.UnitPrice.Should().Be(9.99m);
        item.Total.Should().Be(29.97m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidQuantity_ThrowsDomainException(int quantity)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "Widget", quantity, 10m);

        act.Should().Throw<DomainException>()
            .WithMessage("*Quantity*greater than zero*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Create_WithInvalidUnitPrice_ThrowsDomainException(decimal unitPrice)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "Widget", 1, unitPrice);

        act.Should().Throw<DomainException>()
            .WithMessage("*UnitPrice*greater than zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyProductName_ThrowsDomainException(string productName)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), productName, 1, 10m);

        act.Should().Throw<DomainException>()
            .WithMessage("*ProductName*");
    }
}
