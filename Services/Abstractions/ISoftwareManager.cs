namespace Dorbit.Framework.Services.Abstractions;

public interface ISoftwareManager
{
    IEnumerable<ISoftwareService> GetAllSoftwares();
    ISoftwareService GetSoftwares(string identifier);
}