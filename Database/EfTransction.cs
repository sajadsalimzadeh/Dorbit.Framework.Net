using Dorbit.Database.Abstractions;
using Dorbit.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dorbit.Database;

internal class EfPrimaryTransction : ITransaction
{
    private readonly IDbContextTransaction _transaction;
    private readonly DbContext _dbContext;
    private readonly EfTransactionContext _transactionContext;

    internal EfPrimaryTransction(DbContext dbContext, EfTransactionContext transactionContext)
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
internal class EfSecondaryTransction : ITransaction
{
    private readonly EfTransactionContext _transactionContext;

    internal EfSecondaryTransction(EfTransactionContext transactionContext)
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