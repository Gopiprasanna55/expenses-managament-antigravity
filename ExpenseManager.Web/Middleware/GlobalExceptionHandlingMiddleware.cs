using System.Net;

namespace ExpenseManager.Web.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}
