using BuildingBlocks.Exception;
using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Customers.Models;
using OrderManagement.Data;
using OrderManagement.Orders.Enums;
using OrderManagement.Orders.Models;

namespace YourNamespace.Features.Orders
{
    public class CreateOrderEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapPost(
                $"{EndpointConfig.BaseApiPath}/orders",
                async (CreateOrderRequestDto request, IMediator mediator) =>
                {
                    var result = await mediator.Send(new CreateOrderCommand(
                        request.CustomerId,
                        request.Items));

                    return Results.Created($"{EndpointConfig.BaseApiPath}/orders/{result.Id}", result);
                })
                .RequireAuthorization()
                .WithApiVersionSet(builder.NewApiVersionSet("Order").Build())
                .WithTags("Order")
                .WithName("CreateOrder")
                .WithOpenApi()
                .HasApiVersion(1.0);

            return builder;
        }
    }

    public record CreateOrderCommand(
        Guid CustomerId,
        List<OrderItemDto> Items) : IRequest<OrderResponseDto>;

    public record CreateOrderRequestDto(
        Guid CustomerId,
        List<OrderItemDto> Items);

    public record OrderItemDto(
        string Product,
        decimal UnitPrice,
        int Quantity);

    public record OrderResponseDto(
        Guid Id,
        Guid CustomerId,
        DateTime OrderDate,
        string Status,
        decimal TotalAmount,
        List<OrderItemResponseDto> Items);

    public record OrderItemResponseDto(
        Guid Id,
        string Product,
        decimal UnitPrice,
        int Quantity,
        decimal TotalPrice);

    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CreateOrderHandler(
            AppDbContext dbContext,
            ICurrentUserProvider currentUserProvider)
        {
            _dbContext = dbContext;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<OrderResponseDto> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            // Verify customer exists and belongs to current user (unless admin)
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found");
            }

            if (!_currentUserProvider.IsAdmin())
            {
                throw new ForbiddenException("Unauthorized to create order for this customer");
            }

            // Convert DTOs to domain models
            var orderItems = request.Items
                .Select(item => OrderItem.Create(
                    Guid.NewGuid(),
                    Guid.Empty, // Will be updated when order is created
                    item.Product,
                    item.UnitPrice,
                    item.Quantity))
                .ToList();

            // Create order
            var order = Order.Create(
                Guid.NewGuid(),
                request.CustomerId,
                orderItems);

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapToResponseDto(order);
        }

        private static OrderResponseDto MapToResponseDto(Order order)
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

    public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Customer ID is required");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Order must have at least one item");

            RuleForEach(x => x.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.Product)
                        .NotEmpty()
                        .WithMessage("Product name is required");

                    item.RuleFor(i => i.UnitPrice)
                        .GreaterThan(0)
                        .WithMessage("Unit price must be positive");

                    item.RuleFor(i => i.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Quantity must be positive");
                });
        }
    }
}
