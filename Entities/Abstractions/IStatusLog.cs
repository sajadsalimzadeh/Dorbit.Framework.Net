using Dorbit.Gold.Contracts;

namespace Dorbit.Gold.Entities.Abstractions;

public interface IStatusLog<T> where T : Enum  
{
    public List<StatusLog<T>> StatusLogs { get; set; }
}