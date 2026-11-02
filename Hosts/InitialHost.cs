using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceSingletone]
public class InitialHost(IServiceProvider serviceProvider) : BaseHost(serviceProvider)
{
    protected override Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var settingService = serviceProvider.GetService<SettingService>();
        return settingService.InitAsync();
    }
}