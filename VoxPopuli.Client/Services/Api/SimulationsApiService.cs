using System.Net.Http.Headers;
using System.Net.Http.Json;
using VoxPopuli.Client.Models.Api;

namespace VoxPopuli.Client.Services.Api;

public class SimulationsApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiAuthenticationService _authenticationService;

    public SimulationsApiService(HttpClient httpClient, ApiAuthenticationService authenticationService)
    {
        _httpClient = httpClient;
        _authenticationService = authenticationService;
    }

    public async Task<bool> CreateSimulationAsync(SimulationCreateRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _authenticationService.GetTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            System.Diagnostics.Debug.WriteLine("❌ API: JWT unavailable, simulation will not be sent.");
            return false;
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/Simulations")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            System.Diagnostics.Debug.WriteLine("✅ API: simulation successfully sent.");
            return true;
        }

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        System.Diagnostics.Debug.WriteLine($"❌ API: failed to send simulation ({(int)response.StatusCode}) {responseBody}");

        return false;
    }
}
