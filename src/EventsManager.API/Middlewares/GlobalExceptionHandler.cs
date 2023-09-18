using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace EventsManager.API.Middlewares;

public static class GlobalExceptionHandler
{
    public static void ConfigureGlobalHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                IExceptionHandlerFeature exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature is not null)
                {
                    Exception baseException = exceptionHandlerFeature.Error;
                    logger.LogError(baseException, "Something went wrong");

                    var response = new
                    {
                        code = (int)HttpStatusCode.InternalServerError,
                        message = "Something bad happened, try again later"
                    };

                    context.Response.ContentLength = JsonConvert.SerializeObject(response).Length;
                    await context.Response.WriteAsJsonAsync(response);
                }
            });
        });
    }
}