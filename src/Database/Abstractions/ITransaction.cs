using System;

namespace Devor.Framework.Database.Abstractions
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();

    }
}
