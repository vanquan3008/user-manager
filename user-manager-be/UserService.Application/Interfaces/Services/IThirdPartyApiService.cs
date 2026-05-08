namespace UserService.Application.Interfaces.Services;

public interface IThirdPartyApiService
{
    Task<ThirdPartyApiResponse> PostFormAsync(
        string operationName,
        string requestUri,
        IReadOnlyDictionary<string, string> formValues,
        CancellationToken cancellationToken = default);

    Task<ThirdPartyApiResponse<TResponse>> PostFormAsync<TResponse>(
        string operationName,
        string requestUri,
        IReadOnlyDictionary<string, string> formValues,
        CancellationToken cancellationToken = default);
}