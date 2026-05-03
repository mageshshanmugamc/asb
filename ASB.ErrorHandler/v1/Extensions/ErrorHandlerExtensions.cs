using Microsoft.AspNetCore.Builder;

namespace ASB.ErrorHandler.v1.Extensions;

public static class ErrorHandlerExtensions
{
    public static IApplicationBuilder UseAppErrorHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler("/error");
        return app;
    }
}
