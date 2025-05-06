using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Data;
using OrderManagement.Invoices.Dtos;
using OrderManagement.Invoices.Exceptions;
using OrderManagement.Invoices.Models;
using OrderManagement.Orders.Models;

namespace OrderManagement.Invoices.Features;

public class GenerateInvoiceEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost(
                $"{EndpointConfig.BaseApiPath}/invoices/{{orderId}}",
                async (Guid orderId, [FromBody] GenerateInvoiceRequestDto request, IMediator mediator) =>
                {
                    var command = new GenerateInvoiceCommand(orderId, request.DueDate);
                    var result = await mediator.Send(command);

                    return Results.Created(
                        $"{EndpointConfig.BaseApiPath}/invoices/{result.Id}",
                        result);
                })
            .RequireAuthorization()
            .WithApiVersionSet(builder.NewApiVersionSet("Invoice").Build())
            .WithTags("Invoices")
            .WithName("GenerateInvoice")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }
}

public record GenerateInvoiceCommand(
    Guid OrderId,
    DateTime DueDate
) : IRequest<InvoiceDto>;

public record GenerateInvoiceRequestDto(
    DateTime DueDate
);

public class GenerateInvoiceHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
{
    private readonly AppDbContext _context;

    public GenerateInvoiceHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto> Handle(
        GenerateInvoiceCommand command,
        CancellationToken cancellationToken
    )
    {
        // 1. Locate the order
        var order = await GetOrder(command.OrderId, cancellationToken);

        // 2. Extract TotalAmount and generate invoice
        var invoice = Invoice.Create(
            id: Guid.NewGuid(),
            orderId: order.Id,
            amount: order.TotalAmount,
            dueDate: command.DueDate);

        // Save invoice
        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return InvoiceMappings.MapToInvoiceDto(invoice);
    }

    private async Task<Order> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        return order ?? throw new OrderNotFoundException(orderId);
    }
}

public class GenerateInvoiceValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("Due date is required")
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("Due date must be in the future");
    }
}
