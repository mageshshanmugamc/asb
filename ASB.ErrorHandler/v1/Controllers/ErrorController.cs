using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ASB.ErrorHandler.v1.Exceptions;

namespace ASB.ErrorHandler.v1.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;
    private readonly IHostEnvironment _env;

    public ErrorController(ILogger<ErrorController> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    [Route("/error")]
    public IActionResult HandleError()
    {
         var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var ex = exceptionHandlerPathFeature?.Error;

            // Use switch expression to map exception types
            var (errorCode, statusCode, title) = ex switch
            {
                ArgumentException => ((int)ErrorCodes.ErrorCodes.BadRequest, 400, "Invalid argument"),
                BadRequestException => ((int)ErrorCodes.ErrorCodes.BadRequest, 400, "Bad request"),
                UnAuthorizedException => ((int)ErrorCodes.ErrorCodes.Unauthorized, 401, "Unauthorized access"),
                PaymentRequiredException => ((int)ErrorCodes.ErrorCodes.PaymentRequired, 402, "Payment required"),
                ForbiddenAccessException => ((int)ErrorCodes.ErrorCodes.Forbidden, 403, "Forbidden access"),
                KeyNotFoundException => ((int)ErrorCodes.ErrorCodes.NotFound, 404, "Resource not found"),
                ConflictException => ((int)ErrorCodes.ErrorCodes.Conflict, 409, "Conflict occurred"),
                UnSupportedMediaTypeException => ((int)ErrorCodes.ErrorCodes.UnsupportedMediaType, 415, "Unsupported media type"),
                UnprocessableEntityException => ((int)ErrorCodes.ErrorCodes.UnprocessableEntity, 422, "Unprocessable entity"),
                TooManyRequestException => ((int)ErrorCodes.ErrorCodes.TooManyRequests, 429, "Too many requests"),
                NotImplementedException => ((int)ErrorCodes.ErrorCodes.NotImplemented, 501, "Not implemented"),
               
                _ => ((int)ErrorCodes.ErrorCodes.InternalServerError, 500, "Unexpected error")
            };

        _logger.LogError(ex, "Unhandled exception: {Message}", ex?.Message);

         return Problem(
                detail: ex?.Message,
                title: title,
                statusCode: statusCode,
                instance: exceptionHandlerPathFeature?.Path,
                extensions: new Dictionary<string, object>
                {
                    { "errorCode", errorCode }
                }
            );
    }

     private IActionResult Problem(string? detail, string title, int statusCode, string? instance = null, IDictionary<string, object>? extensions = null)
        {
            return new ObjectResult(new
            {
                Title = title,
                Details = detail,
                StatusCode = statusCode,
                Instance = instance,
                Extensions = extensions
            })
            {
                StatusCode = statusCode
            };
        }
    
}
