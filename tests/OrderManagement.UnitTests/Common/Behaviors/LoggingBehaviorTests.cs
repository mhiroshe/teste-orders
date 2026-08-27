using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OrderManagement.Application.Common.Behaviors;
using Xunit;

namespace OrderManagement.UnitTests.Common.Behaviors;

public sealed class LoggingBehaviorTests
{
    public sealed record TestRequest(string Value);

    private readonly LoggingBehavior<TestRequest, string> _behavior =
        new(NullLogger<LoggingBehavior<TestRequest, string>>.Instance);

    [Fact]
    public async Task Handle_WhenNextSucceeds_ReturnsResponseFromNext()
    {
        var result = await _behavior.Handle(
            new TestRequest("anything"),
            () => Task.FromResult("response"),
            CancellationToken.None);

        result.Should().Be("response");
    }

    [Fact]
    public async Task Handle_WhenNextThrows_RethrowsSameException()
    {
        var thrown = new InvalidOperationException("boom");

        var act = async () => await _behavior.Handle(
            new TestRequest("anything"),
            () => throw thrown,
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(thrown);
    }
}
