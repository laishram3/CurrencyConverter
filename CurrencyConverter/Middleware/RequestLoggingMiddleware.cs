using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CurrencyConverter.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var httpMethod = context.Request.Method;
            var endpoint = context.Request.Path;
            var statusCode = context.Response.StatusCode;

            string clientId = "N/A";
           
            if (context.User?.Identity?.IsAuthenticated == true)
            {                
                clientId = context.User.FindFirst("name")?.Value
                        ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
                        ?? context.User.FindFirst(ClaimTypes.Name)?.Value
                        ?? "N/A";
            }          

            _logger.LogInformation("Request info: {@RequestDetails}", new
            {
                ClientIp = clientIp,
                ClientId = clientId,
                HttpMethod = httpMethod,
                Endpoint = endpoint,
                ResponseCode = statusCode,
                ResponseTimeMs = sw.ElapsedMilliseconds
            });
        }
    }
}
