using BuildingBlocks.Core.Event;
using OrderManagement.Data;
using OrderManagement.Invoices.Dtos;
using BuildingBlocks.Exception;
using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Invoices;
using OrderManagement.Invoices.Enums;
using OrderManagement.Invoices.Exceptions;

namespace OrderManagement.Features.Invoices
{
    public static class PayInvoice
    {
        public class PayInvoiceEndpoint : IMinimalEndpoint
        {
            public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
            {
                builder.MapPost(
                        $"{EndpointConfig.BaseApiPath}/invoices/{{id}}/pay",
                        async (Guid id, IMediator mediator) =>
                        {
                            var result = await mediator.Send(new PayInvoiceCommand(id));
                            return Results.Ok(result);
                        })
                    .RequireAuthorization()
                    .WithApiVersionSet(builder.NewApiVersionSet("Invoice").Build())
                    .WithTags("Invoices")
                    .WithName("PayInvoice")
                    .WithOpenApi()
                    .HasApiVersion(1.0);

                return builder;
            }
        }

        public record PayInvoiceCommand(Guid InvoiceId) : IRequest<InvoiceDto>;

        public record InvoicePaidDomainEvent(
            Guid InvoiceId,
            Guid OrderId,
            decimal AmountPaid,
            decimal CustomerNewBalance,
            InvoiceStatus NewStatus,
            bool IsDeleted) : IDomainEvent;

        public class PayInvoiceHandler : IRequestHandler<PayInvoiceCommand, InvoiceDto>
        {
            private readonly AppDbContext _appDbContext;
            private readonly ICurrentUserProvider _currentUserProvider;

            public PayInvoiceHandler(
                AppDbContext appDbContext,
                ICurrentUserProvider currentUserProvider
            )
            {
                _appDbContext = appDbContext;
                _currentUserProvider = currentUserProvider;
            }

            public async Task<InvoiceDto> Handle(
                PayInvoiceCommand command,
                CancellationToken cancellationToken
            )
            {
                await _appDbContext.BeginTransactionAsync(cancellationToken);

                var invoice = await _appDbContext.Invoices
                                  .Include(i => i.Order)
                                  .ThenInclude(o => o.Customer)
                                  .Include(i => i.Order)
                                  .ThenInclude(o => o.OrderItems)
                                  .FirstOrDefaultAsync(
                                      i => i.Id == command.InvoiceId,
                                      cancellationToken) ??
                              throw new InvoiceNotFoundException(command.InvoiceId);

                if (!await IsInvoiceOwner(invoice.Order.CustomerId, cancellationToken))
                {
                    throw new ForbiddenException("You can only pay your own invoices");
                }

                var customer = invoice.Order.Customer;

                // Detach the original invoice to prevent EF Core tracking conflicts
                _appDbContext.Entry(invoice).State = EntityState.Detached;

                // Detach the customer to avoid tracking conflict
                _appDbContext.Entry(customer).State = EntityState.Detached;

                // Process payment through domain model
                var proccedInvoice = invoice.ProcessPayment(customer);

                // Ensure the new Invoice entity (processed one) is correctly tracked
                _appDbContext.Invoices.Attach(proccedInvoice);
                _appDbContext.Entry(proccedInvoice).State = EntityState.Modified;

                // Update the customer and invoice
                _appDbContext.Customers.Update(proccedInvoice.Order.Customer); // No conflict here now

                _appDbContext.Orders.Update(proccedInvoice.Order); // No conflict here now

                await _appDbContext.CommitTransactionAsync(cancellationToken);

                return InvoiceMappings.MapToInvoiceDto(proccedInvoice);
            }

            private async Task<bool> IsInvoiceOwner(
                Guid customerId,
                CancellationToken cancellationToken
            )
            {
                var currentUserId = _currentUserProvider.GetCurrentUserId();

                var customer = await _appDbContext.Customers
                                   .FirstOrDefaultAsync(
                                       c => c.Id == customerId && c.UserId == currentUserId,
                                       cancellationToken);

                return customer != null || _currentUserProvider.IsAdmin();
            }
        }

        public class PayInvoiceCommandValidator : AbstractValidator<PayInvoiceCommand>
        {
            public PayInvoiceCommandValidator()
            {
                RuleFor(x => x.InvoiceId)
                    .NotEmpty()
                    .WithMessage("Invoice ID is required");
            }
        }
    }
}
