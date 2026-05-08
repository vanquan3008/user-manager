using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;

namespace UserService.Infrastructure.Services;

public class ThirdPartyApiService : IThirdPartyApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<ThirdPartyApiService> _logger;

    public ThirdPartyApiService(HttpClient httpClient, ILogger<ThirdPartyApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ThirdPartyApiResponse> PostFormAsync(
        string operationName,
        string requestUri,
        IReadOnlyDictionary<string, string> formValues,
        CancellationToken cancellationToken = default)
    {
        var response = await PostFormAsync<string>(operationName, requestUri, formValues, cancellationToken);
        return new ThirdPartyApiResponse(
            response.Success,
            response.StatusCode,
            response.RawResponse,
            response.ErrorMessage);
    }

    public async Task<ThirdPartyApiResponse<TResponse>> PostFormAsync<TResponse>(
        string operationName,
        string requestUri,
        IReadOnlyDictionary<string, string> formValues,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestUri))
        {
            _logger.LogWarning("Third-party API {Operation} skipped because request URI is empty", operationName);
            return new ThirdPartyApiResponse<TResponse>(false, HttpStatusCode.BadRequest, default, null, "Request URI is required.");
        }

        try
        {
            using var content = new FormUrlEncodedContent(formValues);
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content
            };

            _logger.LogInformation(
                "Calling third-party API {Operation} {Method} {Uri} with form keys [{FormKeys}]",
                operationName,
                request.Method,
                requestUri,
                string.Join(", ", formValues.Keys));

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var rawResponse = response.Content is null
                ? null
                : await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Third-party API {Operation} failed with status {StatusCode}. Body: {Body}",
                    operationName,
                    response.StatusCode,
                    rawResponse);

                return new ThirdPartyApiResponse<TResponse>(
                    false,
                    response.StatusCode,
                    default,
                    rawResponse,
                    $"Third-party API returned {response.StatusCode}.");
            }

            TResponse? data = default;

            if (typeof(TResponse) == typeof(string))
            {
                data = (TResponse)(object)(rawResponse ?? string.Empty);
            }
            else if (!string.IsNullOrWhiteSpace(rawResponse))
            {
                try
                {
                    data = JsonSerializer.Deserialize<TResponse>(rawResponse, JsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Third-party API {Operation} returned a response that could not be deserialized to {ResponseType}",
                        operationName,
                        typeof(TResponse).Name);

                    return new ThirdPartyApiResponse<TResponse>(
                        false,
                        response.StatusCode,
                        default,
                        rawResponse,
                        $"Failed to deserialize third-party response to {typeof(TResponse).Name}.");
                }
            }

            _logger.LogInformation(
                "Third-party API {Operation} succeeded with status {StatusCode}",
                operationName,
                response.StatusCode);

            return new ThirdPartyApiResponse<TResponse>(true, response.StatusCode, data, rawResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling third-party API {Operation} at {Uri}", operationName, requestUri);
            return new ThirdPartyApiResponse<TResponse>(
                false,
                HttpStatusCode.ServiceUnavailable,
                default,
                null,
                ex.Message);
        }
    }
}