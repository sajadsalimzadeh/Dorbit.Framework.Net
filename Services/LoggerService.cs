using System.Diagnostics;
using System.Reflection;
using Dorbit.Attributes;
using Dorbit.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dorbit.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class LoggerService : ILoggerService
{
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;

    private IUserResolver _userResolver;
    private IUserResolver UserResolver => _userResolver ??= serviceProvider.GetService<IUserResolver>();

    private IRequestService _requestService;
    private IRequestService RequestService => _requestService ??= serviceProvider.GetService<IRequestService>();

    public LoggerService(IServiceProvider serviceProvider, ILogger logger = null)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    private object[] GetExtraInfoValues(object[] args)
    {
        var st = new StackTrace();
        StackFrame frame;
        MethodBase method;
        var i = 1;
        do
        {
            i++;
            frame = st.GetFrame(i);
            method = frame.GetMethod();
        } while (method?.DeclaringType == typeof(LoggerService));

        var user = UserResolver?.User;
        var list = new List<object>();
        foreach (var arg in args) list.Add(arg);
        list.Add(method?.DeclaringType.Assembly.FullName);
        list.Add(method?.DeclaringType.FullName);
        list.Add(method?.Name);
        list.Add(RequestService?.CorrelationId);
        list.Add(user != null
            ? new
            {
                Id = user.Id,
                Name = user.Name,
            }
            : null);
        return list.ToArray();
    }

    public void LogError(Exception ex, params object[] args)
    {
        logger?.Error(ex, ex.Message + ex.InnerException?.Message, GetExtraInfoValues(args));
    }

    public void LogError(string message, params object[] args)
    {
        logger?.Error(message, GetExtraInfoValues(args));
    }

    public void LogInformation(string message, params object[] args)
    {
        logger?.Information(message, GetExtraInfoValues(args));
    }

    public void LogTrace()
    {
        var trace = new StackTrace(0, true);
        var frames = trace.GetFrames().ToList().Where(x => !string.IsNullOrEmpty(x.GetFileName())).Select(x => new
        {
            File = x.GetFileName(),
            Line = x.GetFileLineNumber(),
            Column = x.GetFileColumnNumber(),
            Method = x.GetMethod()?.Name,
        });
        logger?.Information("{@Frames}", GetExtraInfoValues(new object[] { frames }));
    }

    public void ClearBefore(DateTime dateTime)
    {
        var compareDate = dateTime.ToString("yyyyMMdd");
        var dir = new DirectoryInfo("C:\\ProgramData\\Seq\\Logs");
        foreach (var file in dir.GetFiles())
        {
            var dateName = file.Name.Substring(4);
            if (dateName.CompareTo(compareDate) < 0)
            {
                File.Delete(file.FullName);
            }
        }
    }
}