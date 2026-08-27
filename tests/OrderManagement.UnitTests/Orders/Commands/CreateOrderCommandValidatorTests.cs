using FluentValidation.TestHelper;
using OrderManagement.Application.Orders.Commands.CreateOrder;
using Xunit;

namespace OrderManagement.UnitTests.Orders.Commands;

public sealed class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    private static CreateOrderCommand ValidCommand() =>
        new(
            CustomerId: Guid.NewGuid(),
            Items: [new CreateOrderItemDto("Widget", 1, 10.00m)]
        );

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_HasErrorForCustomerId()
    {
        var command = ValidCommand() with { CustomerId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_WithNoItems_HasErrorForItems()
    {
        var command = ValidCommand() with { Items = [] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Validate_WithEmptyProductName_HasErrorForProductName()
    {
        var command = ValidCommand() with { Items = [new CreateOrderItemDto("", 1, 10.00m)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].ProductName");
    }

    [Fact]
    public void Validate_WithProductNameTooLong_HasErrorForProductName()
    {
        var command = ValidCommand() with { Items = [new CreateOrderItemDto(new string('a', 201), 1, 10.00m)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].ProductName");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidQuantity_HasErrorForQuantity(int quantity)
    {
        var command = ValidCommand() with { Items = [new CreateOrderItemDto("Widget", quantity, 10.00m)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validate_WithInvalidUnitPrice_HasErrorForUnitPrice(decimal unitPrice)
    {
        var command = ValidCommand() with { Items = [new CreateOrderItemDto("Widget", 1, unitPrice)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }
}
