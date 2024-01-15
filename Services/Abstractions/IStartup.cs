using System.Threading.Tasks;

namespace Dorbit.Framework.Services.Abstractions;

public interface IStartup
{
    Task RunAsync();
}