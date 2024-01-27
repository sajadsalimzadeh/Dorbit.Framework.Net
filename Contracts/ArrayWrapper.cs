namespace Dorbit.Framework.Contracts;

public struct ArrayWrapper<T>
{
    public ArrayWrapper(T[] values) 
        => Values = values;

    public T[] Values { get; }
}