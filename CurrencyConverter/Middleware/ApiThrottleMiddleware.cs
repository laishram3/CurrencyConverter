namespace CurrencyConverter.Middleware
{
    public class ApiThrottleMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly int _limit = 5; // max requests
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

        public ApiThrottleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var key = $"Throttle_{ip}";
            if (!_cache.TryGetValue(key, out int count))
            {
                count = 0;
            }

            if (count >= _limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Try again later.");
                return;
            }

            _cache.Set(key, count + 1, DateTimeOffset.UtcNow.Add(_period));

            await _next(context);
        }
    }

}
