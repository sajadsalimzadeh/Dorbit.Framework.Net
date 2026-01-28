using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceSingletone]
public class TranslationLoadHost(IServiceProvider serviceProvider, bool isConcurrent = false) : BaseHost(serviceProvider, isConcurrent)
{
    protected override Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var translationService = serviceProvider.GetService<TranslationService>();
        return translationService.LoadAllLocaleAsync();
    }
}