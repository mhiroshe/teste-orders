using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace OrderManagement.IntegrationTests;

public sealed class OrderEndpointsTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private async Task AuthenticateAsync()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "dev@martech.com",
            password = "Senha@123"
        });

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_Returns201WithId()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = Guid.NewGuid(),
            items = new[]
            {
                new { productName = "Test Product", quantity = 2, unitPrice = 10.00 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateOrder_WithNoItems_Returns400()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = Guid.NewGuid(),
            items = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrders_Authenticated_ReturnsPagedResult()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/orders?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("items", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetOrderById_WhenNotFound_Returns404()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsPending_Returns204()
    {
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/orders", new
        {
            customerId = Guid.NewGuid(),
            items = new[] { new { productName = "Widget", quantity = 1, unitPrice = 5.00 } }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var orderId = location.Split('/').Last();

        var cancelResponse = await _client.PatchAsync($"/api/orders/{orderId}/cancel", null);

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
