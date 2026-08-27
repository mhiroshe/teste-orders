using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using OrderManagement.Application.Common.Behaviors;
using Xunit;

namespace OrderManagement.UnitTests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    public sealed record TestRequest(string Value);

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new TestRequest("anything"),
            () => { nextCalled = true; return Task.FromResult("response"); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("response");
    }

    [Fact]
    public async Task Handle_WhenAllValidatorsPass_CallsNext()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        var result = await behavior.Handle(
            new TestRequest("anything"),
            () => Task.FromResult("response"),
            CancellationToken.None);

        result.Should().Be("response");
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ThrowsValidationExceptionAndDoesNotCallNext()
    {
        var failure = new ValidationFailure("Value", "Value is required.");
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult([failure]));

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        var nextCalled = false;

        var act = async () => await behavior.Handle(
            new TestRequest(""),
            () => { nextCalled = true; return Task.FromResult("response"); },
            CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().ContainSingle(e => e.ErrorMessage == "Value is required.");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_AggregatesFailuresFromAll()
    {
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult([new ValidationFailure("Value", "Error from validator 1.")]));

        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult([new ValidationFailure("Value", "Error from validator 2.")]));

        var behavior = new ValidationBehavior<TestRequest, string>([validator1, validator2]);

        var act = async () => await behavior.Handle(
            new TestRequest(""),
            () => Task.FromResult("response"),
            CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().HaveCount(2);
    }
}
