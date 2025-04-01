namespace Dorbit.Framework.Contracts;

public struct ArrayWrapper<T>(T[] values)
{
    public T[] Values { get; } = values;
}