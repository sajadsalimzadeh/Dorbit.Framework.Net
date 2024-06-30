namespace Dorbit.Framework.Contracts;

public interface IAppSecurity
{
    byte[] GetKey();
    string Encrypt(string value);
    string Decrypt(string value);
}