namespace Dorbit.Framework.Contracts;

public interface IAppSecurity
{
    byte[] GetKey();
    byte[] Encrypt(string value);
    string Decrypt(byte[] value);
}