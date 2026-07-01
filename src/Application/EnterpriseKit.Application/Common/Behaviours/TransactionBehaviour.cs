namespace EnterpriseKit.Application.Common.Behaviours;

using EnterpriseKit.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Wraps every <see cref="ICommand"/> / <see cref="ICommand{TResult}"/>
/// in an EF Core database transaction. Read-only queries bypass this behaviour
/// entirely through the marker-interface check.
/// </summary>
public sealed class TransactionBehaviour<TRequest, TResponse>(
    DbContext dbContext,
    ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only wrap commands in transactions
        if (request is not ICommand && request is not ICommand<TResponse>)
            return await next();

        var requestName = typeof(TRequest).Name;

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        logger.LogDebug("Transaction started for {RequestName}", requestName);

        try
        {
            var response = await next();
            await transaction.CommitAsync(cancellationToken);
            logger.LogDebug("Transaction committed for {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Transaction rolled back for {RequestName}", requestName);
            throw;
        }
    }
}
