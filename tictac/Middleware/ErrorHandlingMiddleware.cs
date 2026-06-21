namespace tictac.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);

            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    error = ex.Message
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    error = "Internal server error"
                });
        }
    }
}
