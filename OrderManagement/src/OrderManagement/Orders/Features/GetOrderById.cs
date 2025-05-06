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
using OrderManagement.Orders.Exceptions;

namespace OrderManagement.Orders.Features
{
    public class GetOrderByIdEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet(
                $"{EndpointConfig.BaseApiPath}/orders/{{id:guid}}",
                async (Guid id, IMediator mediator) =>
                {
                    var result = await mediator.Send(new GetOrderByIdQuery(id));
                    return Results.Ok(result);
                })
                .RequireAuthorization()
                .WithApiVersionSet(builder.NewApiVersionSet("Order").Build())
                .WithTags("Order")
                .WithName("GetOrderById")
                .WithOpenApi()
                .HasApiVersion(1.0);

            return builder;
        }
    }

    public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;

    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserProvider _currentUserProvider;

        public GetOrderByIdHandler(
            AppDbContext dbContext,
            ICurrentUserProvider currentUserProvider)
        {
            _dbContext = dbContext;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<OrderDto> Handle(
            GetOrderByIdQuery request,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                throw new OrderNotFoundException(request.OrderId);
            }

            // Authorization check
            if (!_currentUserProvider.IsAdmin())
            {
                throw new ForbiddenException("Unauthorized to view this order");
            }

            return OrderMappings.MapToOrderDto(order);
        }
    }

    public class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("Order ID is required");
        }
    }
}
