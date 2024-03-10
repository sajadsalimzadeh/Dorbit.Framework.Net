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

    public void Commit()
    {
        _dbContext.SaveChanges();
        _transaction.Commit();
    }

    public void Rollback()
    {
        _transaction.Rollback();
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

    public void Commit()
    {
        _transactionContext.DbContext.SaveChanges();
    }

    public void Rollback()
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

    public void Commit()
    {
        _dbContext.SaveChanges();
    }

    public void Rollback()
    {
    }

    public void Dispose()
    {
    }
}