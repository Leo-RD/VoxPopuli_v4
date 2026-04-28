using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
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
        System.Diagnostics.Debug.WriteLine($"🌐 [API] CreateSimulationAsync() - endpoint={_httpClient.BaseAddress}api/Simulations");
        System.Diagnostics.Debug.WriteLine($"🌐 [API] NbAgent payload: NbAgent={request.NbAgent}");

        string payloadJson = JsonSerializer.Serialize(request);
        System.Diagnostics.Debug.WriteLine($"🌐 [API] Payload JSON: {payloadJson}");

        string? token = await _authenticationService.GetTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            System.Diagnostics.Debug.WriteLine("❌ [API] JWT unavailable, simulation will not be sent.");
            return false;
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/Simulations")
        {
            Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ [API] 401 reçu, tentative de rafraîchissement JWT et nouvel envoi...");

            string? refreshedToken = await _authenticationService.RefreshTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(refreshedToken))
            {
                using var retryRequest = new HttpRequestMessage(HttpMethod.Post, "api/Simulations")
                {
                    Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
                };
                retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshedToken);

                using var retryResponse = await _httpClient.SendAsync(retryRequest, cancellationToken);
                if (retryResponse.IsSuccessStatusCode)
                {
                    string retrySuccessBody = await retryResponse.Content.ReadAsStringAsync(cancellationToken);
                    string? retryCreatedId = TryExtractId(retrySuccessBody) ?? TryExtractIdFromLocation(retryResponse.Headers.Location);

                    if (!string.IsNullOrWhiteSpace(retryCreatedId))
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ [API] Simulation successfully sent after JWT refresh. Id={retryCreatedId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("✅ [API] Simulation successfully sent after JWT refresh.");
                    }

                    return true;
                }

                string retryBody = await retryResponse.Content.ReadAsStringAsync(cancellationToken);
                System.Diagnostics.Debug.WriteLine($"❌ [API] Retry failed ({(int)retryResponse.StatusCode}) {retryBody}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("❌ [API] Impossible de rafraîchir le JWT après 401.");
            return false;
        }

        if (response.IsSuccessStatusCode)
        {
            string successBody = await response.Content.ReadAsStringAsync(cancellationToken);
            string? createdId = TryExtractId(successBody) ?? TryExtractIdFromLocation(response.Headers.Location);

            if (!string.IsNullOrWhiteSpace(createdId))
            {
                System.Diagnostics.Debug.WriteLine($"✅ [API] Simulation successfully sent. Id={createdId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✅ [API] Simulation successfully sent.");
            }

            return true;
        }

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        System.Diagnostics.Debug.WriteLine($"❌ [API] Failed to send simulation ({(int)response.StatusCode}) {responseBody}");

        return false;
    }

    private static string? TryExtractId(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            JsonElement root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (JsonProperty prop in root.EnumerateObject())
            {
                string name = prop.Name.ToLowerInvariant();
                if (name is "id" or "simulationid")
                {
                    return prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.ToString(),
                        _ => null
                    };
                }
            }
        }
        catch
        {
            // Ignore non-JSON response bodies.
        }

        return null;
    }

    private static string? TryExtractIdFromLocation(Uri? location)
    {
        if (location is null)
        {
            return null;
        }

        string lastSegment = location.Segments.LastOrDefault()?.Trim('/') ?? string.Empty;
        return string.IsNullOrWhiteSpace(lastSegment) ? null : lastSegment;
    }
}
