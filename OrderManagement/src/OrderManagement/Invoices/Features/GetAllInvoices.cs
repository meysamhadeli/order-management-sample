using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Data;
using OrderManagement.Invoices.Dtos;

namespace OrderManagement.Invoices.Features
{
    public static class GetAllInvoices
    {
        public class GetAllInvoicesEndpoint : IMinimalEndpoint
        {
            public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
            {
                builder.MapGet(
                    $"{EndpointConfig.BaseApiPath}/invoices",
                    async (IMediator mediator, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new GetAllInvoicesQuery(
                            parameters.PageNumber,
                            parameters.PageSize);

                        var result = await mediator.Send(query);
                        return Results.Ok(result);
                    })
                    .RequireAuthorization()
                    .WithApiVersionSet(builder.NewApiVersionSet("Invoice").Build())
                    .WithTags("Invoices")
                    .WithName("GetAllInvoices")
                    .WithOpenApi()
                    .HasApiVersion(1.0);

                return builder;
            }
        }

        public record GetAllInvoicesQuery(
            int PageNumber = 1,
            int PageSize = 20) : IRequest<PagedResult<InvoiceDto>>;

        public record QueryParameters(
            int PageNumber = 1,
            int PageSize = 20);

        public class GetAllInvoicesHandler : IRequestHandler<GetAllInvoicesQuery, PagedResult<InvoiceDto>>
        {
            private readonly AppDbContext _context;
            private readonly ICurrentUserProvider _currentUserProvider;

            public GetAllInvoicesHandler(
                AppDbContext context,
                ICurrentUserProvider currentUserProvider)
            {
                _context = context;
                _currentUserProvider = currentUserProvider;
            }

            public async Task<PagedResult<InvoiceDto>> Handle(
                GetAllInvoicesQuery query,
                CancellationToken cancellationToken)
            {
                var baseQuery = _context.Invoices
                    .Include(i => i.Order)
                    .ThenInclude(o => o.Customer)
                    .Include(i => i.Order)
                    .ThenInclude(o => o.OrderItems)
                    .AsQueryable();

                // Apply customer filter if not admin
                if (!_currentUserProvider.IsAdmin())
                {
                    var customerId = await _context.Customers
                        .Where(c => c.UserId == _currentUserProvider.GetCurrentUserId())
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (customerId == Guid.Empty)
                    {
                        return new PagedResult<InvoiceDto>([], 0, query.PageNumber, query.PageSize);
                    }

                    baseQuery = baseQuery.Where(i => i.Order.CustomerId == customerId);
                }

                var totalCount = await baseQuery.CountAsync(cancellationToken);

                var invoices = await baseQuery
                    .OrderByDescending(i => i.Order.OrderDate)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync(cancellationToken);

                var items = invoices.Select(InvoiceMappings.MapToInvoiceDto).ToList();

                return new PagedResult<InvoiceDto>(
                    items,
                    totalCount,
                    query.PageNumber,
                    query.PageSize);
            }
        }

        public class GetAllInvoicesQueryValidator : AbstractValidator<GetAllInvoicesQuery>
        {
            public GetAllInvoicesQueryValidator()
            {
                RuleFor(x => x.PageNumber)
                    .GreaterThan(0)
                    .WithMessage("Page number must be greater than 0");

                RuleFor(x => x.PageSize)
                    .GreaterThan(0)
                    .LessThanOrEqualTo(100)
                    .WithMessage("Page size must be between 1 and 100");
            }
        }
    }
}
