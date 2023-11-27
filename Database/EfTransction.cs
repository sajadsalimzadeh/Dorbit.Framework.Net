using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dorbit.Framework.Database;

internal class EfPrimaryTransction : ITransaction
{
    private readonly IDbContextTransaction transaction;
    private readonly DbContext dbContext;
    private readonly EfTransactionContext transactionContext;

    internal EfPrimaryTransction(DbContext dbContext, EfTransactionContext transactionContext)
    {
        this.dbContext = dbContext;
        this.transactionContext = transactionContext;
        transaction = dbContext.Database.BeginTransaction();
    }

    public void Commit()
    {
        dbContext.SaveChanges();
        transaction.Commit();
    }

    public void Rollback()
    {
        transaction.Rollback();
    }

    public void Dispose()
    {
        transactionContext.Transactions.Remove(this);
        transaction.Dispose();
    }
}
internal class EfSecondaryTransction : ITransaction
{
    private readonly EfTransactionContext transactionContext;

    internal EfSecondaryTransction(EfTransactionContext transactionContext)
    {
        this.transactionContext = transactionContext;
    }

    public void Commit()
    {
        transactionContext.dbContext.SaveChanges();
    }

    public void Rollback()
    {
        throw new OperationException(Errors.TransactionRollback);
    }

    public void Dispose()
    {
        transactionContext.Transactions.Remove(this);
    }
}