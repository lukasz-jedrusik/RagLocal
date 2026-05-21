using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Rag.Services.Backend.Domain.Exceptions;

namespace Rag.Services.Backend.Api.Middleware
{
    public class ErrorHandlerMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlerMiddleware> logger,
        IWebHostEnvironment hostingEnvironment
        )
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger = logger;
        private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                var traceId = Activity.Current?.Id ?? context?.TraceIdentifier;

                // If response has already started (e.g., streaming), we can't modify headers
                if (response.HasStarted)
                {
                    // TaskCanceledException is expected when client disconnects during streaming
                    if (error is TaskCanceledException || error is OperationCanceledException)
                    {
                        _logger.LogInformation("{traceId}|Request was canceled (client likely disconnected)", traceId);
                        return; // Don't re-throw, this is normal behavior
                    }

                    _logger.LogError("{traceId}|Response already started. Cannot handle error: {error}", traceId, error.ToString());
                    throw; // Re-throw since we can't handle it properly
                }

                response.ContentType = "application/json";

                var problemDetails = new ProblemDetails()
                {
                    Detail = error?.Message
                };

                problemDetails.Extensions.Add("traceId", traceId);

                if (_hostingEnvironment.IsDevelopment()
                    || _hostingEnvironment.EnvironmentName.ToUpper().Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
                {
                    problemDetails.Extensions.Add("errors", new { message = error?.Message, stackTrace = error?.StackTrace });
                }

                switch (error)
                {
                    case BackendException:
                        {
                            // Other Rag.Services.Backend errors and argument null
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            problemDetails.Title = "Bad Request";
                            problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
                            problemDetails.Status = (int)HttpStatusCode.BadRequest;
                            break;
                        }
                    default:
                        {
                            // Unhandled errors
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            problemDetails.Title = "An error occured while processing your request";
                            problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
                            problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                            break;
                        }
                }

                _logger.LogError("{traceId}|{error}", traceId, error.ToString());
                var result = JsonSerializer.Serialize(problemDetails);
                await response.WriteAsync(result);
            }
        }
    }
}