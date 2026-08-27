using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Orders.Commands.CancelOrder;
using OrderManagement.Application.Orders.Commands.CreateOrder;
using OrderManagement.Application.Orders.Queries.GetOrderById;
using OrderManagement.Application.Orders.Queries.GetOrders;

namespace OrderManagement.API.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .RequireAuthorization()
            .WithTags("Orders");

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .Produces<CreateOrderResponse>(201)
            .Produces(400)
            .Produces(401);

        group.MapGet("/", GetOrders)
            .WithName("GetOrders")
            .WithSummary("List orders with pagination")
            .Produces<PagedResult<OrderDto>>()
            .Produces(401);

        group.MapGet("/{id:guid}", GetOrderById)
            .WithName("GetOrderById")
            .WithSummary("Get an order by ID")
            .Produces<OrderDto>()
            .Produces(401)
            .Produces(404);

        group.MapPatch("/{id:guid}/cancel", CancelOrder)
            .WithName("CancelOrder")
            .WithSummary("Cancel a pending order")
            .Produces(204)
            .Produces(401)
            .Produces(404)
            .Produces(422);

        return app;
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var orderId = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/orders/{orderId}", new CreateOrderResponse(orderId));
    }

    private sealed record CreateOrderResponse(Guid OrderId);

    private static async Task<IResult> GetOrders(
        ISender sender,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetOrdersQuery(page, pageSize), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetOrderById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        return Results.Ok(order);
    }

    private static async Task<IResult> CancelOrder(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new CancelOrderCommand(id), cancellationToken);
        return Results.NoContent();
    }
}
