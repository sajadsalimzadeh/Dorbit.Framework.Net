using System;

namespace Devor.Framework.Services.Abstractions
{
    public interface ILoggerService
    {
        void ClearBefore(DateTime dateTime);
        void LogError(Exception ex, params object[] args);
        void LogError(string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogTrace();
    }
}
