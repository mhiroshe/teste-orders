using FluentValidation.TestHelper;
using OrderManagement.Application.Orders.Commands.CancelOrder;
using Xunit;

namespace OrderManagement.UnitTests.Orders.Commands;

public sealed class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidOrderId_HasNoErrors()
    {
        var result = _validator.TestValidate(new CancelOrderCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrderId_HasErrorForOrderId()
    {
        var result = _validator.TestValidate(new CancelOrderCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }
}
