using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Jobs;

namespace Dorbit.Framework.Services.Abstractions;

public interface IJobHub
{
    Task UpdateAllJobAsync();
    Task UpdateJobAsync(Job job);
}