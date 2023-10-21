using System.Diagnostics;
using System.Reflection;
using Dorbit.Attributes;
using Dorbit.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dorbit.Services
{
    [ServiceRegisterar]
    internal class LoggerService : ILoggerService
    {
        class ExtraInfo
        {
            public object User { get; set; }
            public string Method { get; set; }
            public string Class { get; set; }
            public string Assembly { get; set; }
        }

        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        private IUserResolver _userResolver;
        private IUserResolver userResolver => _userResolver ??= serviceProvider.GetService<IUserResolver>();

        private IRequestService _requestService;
        private IRequestService requestService => _requestService ??= serviceProvider.GetService<IRequestService>();

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

            var user = userResolver?.GetUser();
            var list = new List<object>();
            foreach (var arg in args) list.Add(arg);
            list.Add(method?.DeclaringType.Assembly.FullName);
            list.Add(method?.DeclaringType.FullName);
            list.Add(method?.Name);
            list.Add(requestService?.CorrelationId);
            list.Add(user != null ? new
            {
                Id = user.Id,
                Name = user.Name,
            } : null);
            return list.ToArray();
        }

        private string GetExtraInfoMessage()
        {
            return " / {@Assembly} / {@Class} / {@Method} / {@CorrelationId} / {@User}";
        }

        public void LogError(Exception ex, params object[] args)
        {
            logger?.Error(ex, ex.Message + GetExtraInfoMessage(), GetExtraInfoValues(args));
        }

        public void LogError(string message, params object[] args)
        {
            logger?.Error(message + GetExtraInfoMessage(), GetExtraInfoValues(args));
        }

        public void LogInformation(string message, params object[] args)
        {
            logger?.Information(message + GetExtraInfoMessage(), GetExtraInfoValues(args));
        }

        public void LogTrace()
        {
            var trace = new StackTrace(0, true);
            var Frames = trace.GetFrames().ToList().Where(x => !string.IsNullOrEmpty(x.GetFileName())).Select(x => new
            {
                File = x.GetFileName(),
                Line = x.GetFileLineNumber(),
                Column = x.GetFileColumnNumber(),
                Method = x.GetMethod().Name,
            });
            logger?.Information("{@Frames}" + GetExtraInfoMessage(), GetExtraInfoValues(new object[] { Frames }));
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
}
