using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace AbbContentEditor.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log basic request info
            _logger.LogInformation("Incoming request: {method} {url}",
                context.Request.Method, context.Request.Path + context.Request.QueryString);

            // Optionally log headers
            foreach (var header in context.Request.Headers)
            {
                _logger.LogDebug("Header: {key}={value}", header.Key, header.Value);
            }

            // Optionally log body (only if small / text-based)
            if (context.Request.ContentLength > 0 &&
                context.Request.ContentType != null &&
                context.Request.ContentType.Contains("application/json"))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                _logger.LogDebug("Body: {body}", body);
            }

            // Continue to next middleware
            await _next(context);
        }
    }
}
