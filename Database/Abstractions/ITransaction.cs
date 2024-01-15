using System;

namespace Dorbit.Framework.Database.Abstractions;

public interface ITransaction : IDisposable
{
    void Commit();
    void Rollback();

}