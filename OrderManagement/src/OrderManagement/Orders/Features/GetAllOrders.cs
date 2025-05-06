using BuildingBlocks.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Data;
using OrderManagement.Orders.Models;

namespace YourNamespace.Features.Orders
{
    public class GetAllOrdersEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet(
                $"{EndpointConfig.BaseApiPath}/orders",
                async (IMediator mediator) =>
                {
                    var result = await mediator.Send(new GetAllOrdersQuery());
                    return Results.Ok(result);
                })
                .RequireAuthorization()
                .WithApiVersionSet(builder.NewApiVersionSet("Order").Build())
                .WithTags("Order")
                .WithName("GetAllOrders")
                .WithOpenApi()
                .HasApiVersion(1.0);

            return builder;
        }
    }

    public record GetAllOrdersQuery : IRequest<List<OrderResponseDto>>;

    public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, List<OrderResponseDto>>
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserProvider _currentUserProvider;

        public GetAllOrdersHandler(
            AppDbContext dbContext,
            ICurrentUserProvider currentUserProvider)
        {
            _dbContext = dbContext;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<List<OrderResponseDto>> Handle(
            GetAllOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .AsNoTracking();

            if (!_currentUserProvider.IsAdmin())
            {
                var userId = _currentUserProvider.GetCurrentUserId();

                // Get only the most recent order for the current user
                var mostRecentOrder = await query
                    .Where(o => o.Customer.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync(cancellationToken);

                return mostRecentOrder != null
                    ? new List<OrderResponseDto> { MapToOrderResponseDto(mostRecentOrder) }
                    : new List<OrderResponseDto>();
            }

            // Admin gets all orders
            var orders = await query.ToListAsync(cancellationToken);
            return orders.Select(MapToOrderResponseDto).ToList();
        }

        private static OrderResponseDto MapToOrderResponseDto(Order order)
        {
            return new OrderResponseDto(
                order.Id,
                order.CustomerId,
                order.OrderDate,
                order.Status.ToString(),
                order.TotalAmount,
                order.OrderItems.Select(item => new OrderItemResponseDto(
                    item.Id,
                    item.Product,
                    item.UnitPrice,
                    item.Quantity,
                    item.TotalPrice)).ToList());
        }
    }
}
