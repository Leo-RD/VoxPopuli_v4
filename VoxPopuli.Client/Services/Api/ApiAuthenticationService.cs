using System.Net.Http.Json;
using System.Text.Json;

namespace VoxPopuli.Client.Services.Api;

public class ApiAuthenticationService
{
    private readonly HttpClient _httpClient;

    public ApiAuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        string? existingToken = await SecureStorage.Default.GetAsync(VoxPopuliApiOptions.JwtStorageKey);
        if (!string.IsNullOrWhiteSpace(existingToken))
        {
            return existingToken;
        }

        bool loggedIn = await LoginAsync(
            VoxPopuliApiOptions.DefaultUsername,
            VoxPopuliApiOptions.DefaultPassword,
            cancellationToken);

        if (!loggedIn)
        {
            return null;
        }

        return await SecureStorage.Default.GetAsync(VoxPopuliApiOptions.JwtStorageKey);
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var payload = new { username, password };
        using var response = await _httpClient.PostAsJsonAsync("login", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            System.Diagnostics.Debug.WriteLine($"❌ API Login failed: {(int)response.StatusCode} {response.ReasonPhrase}");
            return false;
        }

        string rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        string? token = ExtractTokenFromResponse(rawJson);

        if (string.IsNullOrWhiteSpace(token))
        {
            System.Diagnostics.Debug.WriteLine("❌ API Login succeeded but no JWT token was found in the response payload.");
            return false;
        }

        await SecureStorage.Default.SetAsync(VoxPopuliApiOptions.JwtStorageKey, token);
        System.Diagnostics.Debug.WriteLine("✅ API Login OK, JWT stored in secure storage.");
        return true;
    }

    private static string? ExtractTokenFromResponse(string rawJson)
    {
        using JsonDocument document = JsonDocument.Parse(rawJson);
        JsonElement root = document.RootElement;

        if (root.ValueKind == JsonValueKind.String)
        {
            return root.GetString();
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        string[] possibleTokenProperties = ["token", "accessToken", "jwt", "jwtToken"];

        foreach (string propertyName in possibleTokenProperties)
        {
            if (root.TryGetProperty(propertyName, out JsonElement tokenElement) && tokenElement.ValueKind == JsonValueKind.String)
            {
                return tokenElement.GetString();
            }
        }

        return null;
    }
}
