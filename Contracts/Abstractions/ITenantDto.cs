namespace Dorbit.Framework.Contracts.Abstractions;

public interface ITenantDto
{
    object GetId();
    string GetName();
}