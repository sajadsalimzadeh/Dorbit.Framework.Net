namespace Dorbit.Database.Abstractions
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();

    }
}
