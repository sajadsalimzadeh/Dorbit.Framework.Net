using System;

namespace Dorbit.Framework;

public class AppSecurity
{
    private byte[] _key;
    public byte[] Key => _key ??= App.Current?.Key;
    
    public Func<string, string> Decrypt { get; internal set; }
    public Func<string, string> Encrypt { get; internal set; }

    internal void SetKey(byte[] value)
    {
        _key = value;
    }
}