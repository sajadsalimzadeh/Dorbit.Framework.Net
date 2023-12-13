namespace Dorbit.Framework.Models.Commands;

public class CommandParameter<T>
{
    public string Key { get; set; }
    public string Message { get; set; }
    public T DefaultValue { get; set; }

    public CommandParameter(string key, string message)
    {
        Key = key;
        Message = message;
    }

    public CommandParameter(string key, string message, T defaultValue)
    {
        Key = key;
        Message = message;
        DefaultValue = defaultValue;
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