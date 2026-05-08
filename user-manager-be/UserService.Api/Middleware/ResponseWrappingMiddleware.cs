using System.Text.Json;

namespace UserService.Api.Middleware;

public class ResponseWrappingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseWrappingMiddleware> _logger;

    public ResponseWrappingMiddleware(RequestDelegate next, ILogger<ResponseWrappingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        await using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            context.Response.Body = originalBodyStream;
            var isJson = context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 400 && isJson)
            {
                var trimmed = (bodyText ?? string.Empty).TrimStart();
                if (!string.IsNullOrEmpty(trimmed) && trimmed.StartsWith("{") &&
                    (trimmed.IndexOf("\"success\"", StringComparison.OrdinalIgnoreCase) >= 0 || trimmed.IndexOf("Success", StringComparison.Ordinal) >= 0))
                {
                    await originalBodyStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(bodyText ?? ""));
                    return;
                }

                object? data = null;
                if (!string.IsNullOrWhiteSpace(bodyText))
                {
                    try
                    {
                        data = JsonSerializer.Deserialize<JsonElement>(bodyText);
                    }
                    catch
                    {
                        data = bodyText;
                    }
                }

                var api = new ApiResponse<object>(true, data, "OK");
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(api);
                return;
            }
            if (!string.IsNullOrEmpty(bodyText))
            {
                await originalBodyStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(bodyText));
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
