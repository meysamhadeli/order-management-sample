using BuildingBlocks.Exception;
using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Data;
using OrderManagement.Invoices.Dtos;
using OrderManagement.Invoices.Exceptions;

namespace OrderManagement.Invoices.Features
{
    public class GetInvoiceByIdEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet(
                    $"{EndpointConfig.BaseApiPath}/invoices/{{id}}",
                    async (Guid id, IMediator mediator) =>
                    {
                        var result = await mediator.Send(new GetInvoiceByIdQuery(id));
                        return Results.Ok(result);
                    })
                .RequireAuthorization()
                .WithApiVersionSet(builder.NewApiVersionSet("Invoice").Build())
                .WithTags("Invoices")
                .WithName("GetInvoiceById")
                .WithOpenApi()
                .HasApiVersion(1.0);

            return builder;
        }
    }

    public record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto>;

    public class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserProvider _currentUserProvider;

        public GetInvoiceByIdHandler(
            AppDbContext context,
            ICurrentUserProvider currentUserProvider)
        {
            _context = context;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.Customer)
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(i => i.Id == query.Id, cancellationToken);

            if (invoice == null)
            {
                throw new InvoiceNotFoundException(query.Id);
            }

            // Check if current user is admin or owns the invoice
            if (!_currentUserProvider.IsAdmin())
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c =>
                        c.UserId == _currentUserProvider.GetCurrentUserId() &&
                        c.Id == invoice.Order.CustomerId,
                        cancellationToken);

                if (customer == null)
                {
                    throw new ForbiddenException("You can only view your own invoices");
                }
            }

            return InvoiceMappings.MapToInvoiceDto(invoice);
        }
    }

    public class GetInvoiceByIdValidator : AbstractValidator<GetInvoiceByIdQuery>
    {
        public GetInvoiceByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Invoice ID is required");
        }
    }
}
