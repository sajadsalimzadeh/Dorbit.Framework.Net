using Dorbit.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Dorbit.Installers;

public static class ErrorHandlerInstaller
{
    public static WebApplication UseGlobalErrorHandler(this WebApplication app)
    {
        app.UseMiddleware<GlobalErrorHandlerMiddleware>();
        return app;
    }
}