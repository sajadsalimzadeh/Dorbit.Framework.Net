using System.Collections.Generic;

namespace Devor.Framework.Services.Abstractions
{
    public interface ISoftwareManager
    {
        IEnumerable<ISoftwareService> GetAllSoftwares();
        ISoftwareService GetSoftwares(string identifier);
    }
}
