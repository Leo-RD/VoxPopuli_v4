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
            System.Diagnostics.Debug.WriteLine("🌐 [API] JWT trouvé dans SecureStorage.");
            return existingToken;
        }

        return await RefreshTokenAsync(cancellationToken);
    }

    public async Task<string?> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        SecureStorage.Default.Remove(VoxPopuliApiOptions.JwtStorageKey);
        System.Diagnostics.Debug.WriteLine("🌐 [API] Nouveau login demandé pour rafraîchir le JWT...");

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
        try
        {
            var payload = new { username, password };
            System.Diagnostics.Debug.WriteLine($"🌐 [API] POST {_httpClient.BaseAddress}login");

            using var response = await _httpClient.PostAsJsonAsync("login", payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                System.Diagnostics.Debug.WriteLine($"❌ [API] Login failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"❌ [API] Login body: {errorBody}");
                return false;
            }

            string rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"🌐 [API] Login response raw: {rawJson}");

            string? token = ExtractTokenFromResponse(rawJson);

            if (string.IsNullOrWhiteSpace(token))
            {
                System.Diagnostics.Debug.WriteLine("❌ [API] Login succeeded but no JWT token was found in the response payload.");
                return false;
            }

            await SecureStorage.Default.SetAsync(VoxPopuliApiOptions.JwtStorageKey, token);
            System.Diagnostics.Debug.WriteLine("✅ [API] Login OK, JWT stored in secure storage.");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [API] Exception during login: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ [API] Stack: {ex.StackTrace}");
            return false;
        }
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
