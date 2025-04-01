using System.Collections.Generic;
using Dorbit.Framework.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Database;

public enum TransactionState
{
    NotSet = 0,
    Commit = 1,
    Rollback = 2,
}

internal class EfTransactionContext(DbContext dbContext)
{
    internal readonly DbContext DbContext = dbContext;
    internal readonly List<ITransaction> Transactions = new();
    private TransactionState _state = TransactionState.NotSet;

    public ITransaction BeginTransaction()
    {
        ITransaction transaction;
        if (Transactions.Count > 0)
        {
            transaction = new EfSecondaryTransaction(this);
            Transactions.Add(transaction);
        }
        else
        {
            transaction = new EfPrimaryTransaction(DbContext, this);
            Transactions.Add(transaction);
        }

        return transaction;
    }
}