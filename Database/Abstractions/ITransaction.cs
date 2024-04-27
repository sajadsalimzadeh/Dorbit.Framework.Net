using System;
using System.Threading.Tasks;

namespace Dorbit.Framework.Database.Abstractions;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}