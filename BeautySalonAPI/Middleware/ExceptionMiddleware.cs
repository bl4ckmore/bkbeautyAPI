using System.Net;
using System.Text.Json;

namespace BeautySalonAPI.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (InvalidOperationException ex)
        {
            await WriteError(ctx, HttpStatusCode.Conflict, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteError(ctx, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteError(ctx, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteError(HttpContext ctx, HttpStatusCode status, string message)
    {
        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = message });
        return ctx.Response.WriteAsync(body);
    }
}
