using System.Threading.Tasks;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dorbit.Framework.Database;

internal class EfPrimaryTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;
    private readonly DbContext _dbContext;
    private readonly EfTransactionContext _transactionContext;

    internal EfPrimaryTransaction(DbContext dbContext, EfTransactionContext transactionContext)
    {
        _dbContext = dbContext;
        _transactionContext = transactionContext;
        _transaction = dbContext.Database.BeginTransaction();
    }

    public async Task CommitAsync()
    {
        await _dbContext.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    public Task RollbackAsync()
    {
        return _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transactionContext.Transactions.Remove(this);
        _transaction.Dispose();
    }
}

internal class EfSecondaryTransaction : ITransaction
{
    private readonly EfTransactionContext _transactionContext;

    internal EfSecondaryTransaction(EfTransactionContext transactionContext)
    {
        _transactionContext = transactionContext;
    }

    public Task CommitAsync()
    {
        return _transactionContext.DbContext.SaveChangesAsync();
    }

    public Task RollbackAsync()
    {
        throw new OperationException(Errors.TransactionRollback);
    }

    public void Dispose()
    {
        _transactionContext.Transactions.Remove(this);
    }
}

internal class InMemoryTransaction : ITransaction
{
    private readonly DbContext _dbContext;

    public InMemoryTransaction(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task CommitAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    public Task RollbackAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}