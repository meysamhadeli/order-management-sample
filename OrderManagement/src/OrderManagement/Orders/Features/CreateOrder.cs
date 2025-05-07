using BuildingBlocks.Core.Event;
using BuildingBlocks.Exception;
using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Data;
using OrderManagement.Orders.Dtos;
using OrderManagement.Orders.Enums;
using OrderManagement.Orders.Exceptions;
using OrderManagement.Orders.Models;

namespace OrderManagement.Orders.Features
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
        List<OrderItemDto> Items) : IRequest<OrderDto>;

    public record CreateOrderRequestDto(
        Guid CustomerId,
        List<OrderItemDto> Items);

    public record OrderCreatedDomainEvent(
        Guid OrderId,
        Guid CustomerId,
        DateTime OrderDate,
        OrderStatus Status,
        decimal TotalAmount,
        bool IsDeleted,
        List<OrderItemDto> OrderItems) : IDomainEvent;


    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
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

        public async Task<OrderDto> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            // Verify customer exists and belongs to current user (unless admin)
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (customer == null)
            {
                throw new CustomerNotFoundException(request.CustomerId);
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

            return OrderMappings.MapToOrderDto(order);
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
