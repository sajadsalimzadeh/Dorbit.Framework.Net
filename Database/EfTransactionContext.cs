﻿using Dorbit.Framework.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Database;

public enum TransactionState
{
    NotSet = 0,
    Commit = 1,
    Rollback = 2,
}
internal class EfTransactionContext
{
    internal readonly DbContext DbContext;
    internal List<ITransaction> Transactions = new List<ITransaction>();
    internal TransactionState State = TransactionState.NotSet;

    public EfTransactionContext(DbContext dbContext)
    {
        DbContext = dbContext;
    }

    public ITransaction BeginTransaction()
    {
        ITransaction transaction;
        if (Transactions.Count > 0)
        {
            transaction = new EfSecondaryTransction(this);
            Transactions.Add(transaction);
        }
        else
        {
            transaction = new EfPrimaryTransction(DbContext, this);
            Transactions.Add(transaction);
        }
        return transaction;
    }
}