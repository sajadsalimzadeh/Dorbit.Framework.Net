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
        _transactionContext.Transactions.Remove(this);
        await _dbContext.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        _transactionContext.Transactions.Remove(this);
        await _transaction.RollbackAsync();
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
        _transactionContext.Transactions.Remove(this);
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

internal class InMemoryTransaction(EfTransactionContext transactionContext) : ITransaction
{
    public Task CommitAsync()
    {
        transactionContext.Transactions.Remove(this);
        return transactionContext.DbContext.SaveChangesAsync();
    }

    public Task RollbackAsync()
    {
        transactionContext.Transactions.Remove(this);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        transactionContext.Transactions.Remove(this);
    }
}