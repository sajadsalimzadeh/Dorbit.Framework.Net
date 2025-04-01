namespace Dorbit.Framework.Contracts.Commands;

public class CommandParameter<T>(string key, string message, T defaultValue)
{
    public string Key { get; set; } = key;
    public string Message { get; set; } = message;
    public T DefaultValue { get; set; } = defaultValue;

    public CommandParameter(string key, string message) : this(key, message, default(T))
    {
    }
}

public class CommandParameter : CommandParameter<object>
{
    public CommandParameter(string key) : base(key, key)
    {
    }

    public CommandParameter(string key, string message) : base(key, message)
    {
    }

    public CommandParameter(string key, string message, object defaultValue) : base(key, message, defaultValue)
    {
    }
}