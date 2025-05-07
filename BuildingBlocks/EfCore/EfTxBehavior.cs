using System.Text.Json;
using BuildingBlocks.EfCore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.EFCore;


public class EfTxBehavior<TRequest, TResponse>(
    ILogger<EfTxBehavior<TRequest, TResponse>> logger,
    IDbContext dbContextBase
)
    : IPipelineBehavior<TRequest, TResponse>
where TRequest : notnull, IRequest<TResponse>
where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{Prefix} Handled command {MediatrRequest}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);

        logger.LogDebug(
            "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        logger.LogInformation(
            "{Prefix} Open the transaction for {MediatrRequest}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);

        await dbContextBase.BeginTransactionAsync(cancellationToken);

        var response = await next();

        logger.LogInformation(
            "{Prefix} Executed the {MediatrRequest} request",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);

        await dbContextBase.CommitTransactionAsync(cancellationToken);

        return response;

    }
}
