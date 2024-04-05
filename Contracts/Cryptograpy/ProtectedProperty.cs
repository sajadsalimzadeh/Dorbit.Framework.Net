using System.Security.Cryptography;

namespace Dorbit.Framework.Contracts.Cryptograpy;

public class ProtectedProperty
{
    public int Iterations { get; set; } = 1000;
    public string Algorithm { get; set; } = "SHA1";
    public string Value { get; set; }

    public HashAlgorithmName GetHashAlgorithmName()
    {
        return new HashAlgorithmName(Algorithm);
    }

    public ProtectedProperty()
    {
    }

    public ProtectedProperty(string value)
    {
        Value = value;
    }
}