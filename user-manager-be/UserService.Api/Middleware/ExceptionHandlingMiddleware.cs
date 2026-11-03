using System.Net;
using System.Text.Json;
using UserService.Api.Auth;
using UserService.Domain.Exceptions;
using DomainAuthException = UserService.Domain.Exceptions.AuthenticationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace UserService.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (await TryHandleAuthControllerSpecialCasesAsync(context, exception))
        {
            return;
        }

        var (statusCode, apiResponse) = CreateApiErrorResponse(exception, context);

        LogException(exception, context, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        await context.Response.WriteAsJsonAsync(apiResponse, options);
    }

    private async Task<bool> TryHandleAuthControllerSpecialCasesAsync(HttpContext context, Exception exception)
    {
        if (exception is VerificationEmailResentException resent)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(
                new ApiResponse<AuthResponse>(true, null, resent.Message),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return true;
        }

        // A dead refresh token must not linger in the browser; a superseded one (multi-tab race) is kept
        // because the browser already holds the newer cookie.
        if (context.Request.Path.StartsWithSegments("/api/auth/refresh") &&
            (exception is UnauthorizedAccessException or DomainAuthException or UnauthorizedException ||
             exception is AuthenticationFlowException { Code: not AuthErrorCodes.RefreshTokenSuperseded }))
        {
            foreach (var path in new[] { "/api/auth", "/" })
            {
                context.Response.Cookies.Delete(AuthCookieNames.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = context.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                    Path = path
                });
            }
        }

        return false;
    }

    private (int StatusCode, ApiResponse<object> Response) CreateApiErrorResponse(Exception exception, HttpContext context)
    {
        var (statusCode, title) = MapExceptionToStatusCode(exception);
        var message = GetSafeErrorMessage(exception);
        var requestId = context.Items["RequestId"]?.ToString() ?? context.TraceIdentifier;

        var code = exception switch
        {
            AuthenticationFlowException flow => flow.Code,
            OAuthProtocolException oauth => oauth.Error,
            BusinessRuleValidationException rule when rule.RuleName != "Unknown" => rule.RuleName,
            _ => exception.GetType().Name
        };
        var error = new ErrorDetail(code, message, GetValidationErrors(exception));

        var response = new ApiResponse<object>(false, null, title, error);

        return ((int)statusCode, response);
    }

    private static IDictionary<string, string[]>? GetValidationErrors(Exception exception)
    {
        if (exception is FluentValidationException fv)
        {
            return fv.Errors
                .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "_" : e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
        }

        return null;
    }

    private (HttpStatusCode StatusCode, string Title) MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            UserNotFoundException => (HttpStatusCode.NotFound, "User Not Found"),
            InvalidRoleException => (HttpStatusCode.BadRequest, "Invalid Role"),
            UserRoleOperationException => (HttpStatusCode.UnprocessableEntity, "Role Operation Failed"),
            FluentValidationException => (HttpStatusCode.BadRequest, "Validation Failed"),
            EntityNotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
            ApplicationNotFoundException => (HttpStatusCode.NotFound, "Application Not Found"),
            UserAlreadyInApplicationException => (HttpStatusCode.BadRequest, "User Already In Application"),
            RoleNotFoundException => (HttpStatusCode.BadRequest, "Role Not Found"),
            RoleAlreadyExistsException => (HttpStatusCode.BadRequest, "Role Already Exists"),
            InvalidDomainObjectException => (HttpStatusCode.BadRequest, "Invalid Domain Object"),
            DomainAuthException => (HttpStatusCode.Unauthorized, "Authentication Failed"),
            AuthenticationFlowException => (HttpStatusCode.Unauthorized, "Authentication Failed"),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            OAuthProtocolException { Error: OAuthErrors.InvalidClient } => (HttpStatusCode.Unauthorized, "Invalid Client"),
            OAuthProtocolException => (HttpStatusCode.BadRequest, "OAuth Error"),
            ConcurrencyConflictException => (HttpStatusCode.Conflict, "Conflict"),
            DuplicateUserException => (HttpStatusCode.Conflict, "User Already Exists"),
            BusinessRuleValidationException => (HttpStatusCode.UnprocessableEntity, "Business Rule Violation"),
            DomainOperationNotAllowedException => (HttpStatusCode.Forbidden, "Operation Not Allowed"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ArgumentNullException => (HttpStatusCode.InternalServerError, "Internal Server Error"),
            ArgumentException => (HttpStatusCode.InternalServerError, "Internal Server Error"),
            InvalidOperationException => (HttpStatusCode.InternalServerError, "Internal Server Error"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
            FileNotFoundException => (HttpStatusCode.InternalServerError, "Internal Server Error"),
            InvalidDataException => (HttpStatusCode.InternalServerError, "Internal Server Error"),
            TimeoutException => (HttpStatusCode.RequestTimeout, "Request Timeout"),
            TaskCanceledException => (HttpStatusCode.RequestTimeout, "Request Cancelled"),
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "Operation Cancelled"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Not Implemented"),
            NotSupportedException => (HttpStatusCode.NotImplemented, "Not Supported"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };
    }

    private string GetSafeErrorMessage(Exception exception)
    {
        if (exception is UserService.Domain.Exceptions.DomainException)
        {
            return exception.Message;
        }
        if (exception is KeyNotFoundException)
        {
            return exception.Message;
        }
        if (exception is FluentValidationException validationException)
        {
            return string.Join("; ", validationException.Errors.Select(error => error.ErrorMessage).Distinct());
        }
        if (!env.IsDevelopment())
        {
            return exception switch
            {
                UnauthorizedAccessException => "You are not authorized to access this resource.",
                TimeoutException or TaskCanceledException => "The request took too long to process.",
                _ => "An error occurred while processing your request."
            };
        }

        return exception.Message;
    }

    private void LogException(Exception exception, HttpContext context, int statusCode)
    {
        var logLevel = DetermineLogLevel(statusCode, exception);
        var requestId = context.Items["RequestId"]?.ToString() ?? context.TraceIdentifier;
        
        logger.Log(
            logLevel,
            exception,
            "[{RequestId}] HTTP {Method} {Path} resulted in {StatusCode}. Exception: {ExceptionType}, Message: {Message}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            statusCode,
            exception.GetType().Name,
            exception.Message);
    }

    private static LogLevel DetermineLogLevel(int statusCode, Exception exception)
    {
        if (exception is UserService.Domain.Exceptions.DomainException)
        {
            return statusCode >= 500 ? LogLevel.Warning : LogLevel.Information;
        }
        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
}
