namespace Dorbit.Framework.Models;

public struct ArrayWrapper<T>
{
    public ArrayWrapper(T[] values) 
        => Values = values;

    public T[] Values { get; }
}