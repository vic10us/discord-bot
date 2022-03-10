using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace bot.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseFluentValidationExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(x =>
        {
            x.Run(async context =>
            {
                var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = errorFeature.Error;
                if (exception is not ValidationException validationException) throw exception;
                var errors = validationException.Errors.Select(x => new
                {
                    x.PropertyName,
                    x.ErrorMessage,
                    x.ErrorCode,
                    x.Severity,
                }).Distinct();
                var errorText = JsonConvert.SerializeObject(errors);
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(errorText, Encoding.UTF8);
            });
        });
    }
}
