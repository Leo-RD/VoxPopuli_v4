using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VoxPopuli.Client.Models.Ai;

namespace VoxPopuli.Client.Services.Ai;

public sealed class GitHubModelsChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly GitHubModelsOptions _options;

    public GitHubModelsChatService(HttpClient httpClient, GitHubModelsOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<string> GetArgumentAsync(string orientationPolitique, string discours, CancellationToken cancellationToken = default)
    {
        System.Diagnostics.Debug.WriteLine("[AI] Préparation requête GitHub Models...");
        System.Diagnostics.Debug.WriteLine($"[AI] Modèle: {_options.Model}, Endpoint: {_options.Endpoint}");
        var apiKey = await ReadApiKeyAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new GitHubModelsChatRequest
        {
            Model = _options.Model,
            Messages =
            [
                new GitHubModelsChatMessage
                {
                    Role = "system",
                    Content = $"Tu es un citoyen avec l'orientation politique suivante : {orientationPolitique}. " +
                              "Tu dois argumenter de façon cohérente avec cette idéologie, en français, de manière concise et claire."
                },
                new GitHubModelsChatMessage
                {
                    Role = "user",
                    Content = $"Texte (phrase ou discours) : \"{discours}\". Donne un argumentaire en fonction de l'orientation." 
                }
            ]
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        System.Diagnostics.Debug.WriteLine($"[AI] Payload prêt: {json.Length} caractères.");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        System.Diagnostics.Debug.WriteLine($"[AI] Statut réponse: {(int)response.StatusCode} {response.StatusCode}.");
        if (response.Headers.WwwAuthenticate is not null)
        {
            foreach (var header in response.Headers.WwwAuthenticate)
            {
                System.Diagnostics.Debug.WriteLine($"[AI] WWW-Authenticate: {header.Scheme} {header.Parameter}".TrimEnd());
            }
        }
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"[AI] Erreur payload: {errorBody}");
        }
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var chatResponse = await JsonSerializer.DeserializeAsync<GitHubModelsChatResponse>(stream, JsonOptions, cancellationToken);
        var content = chatResponse?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
        System.Diagnostics.Debug.WriteLine($"[AI] Réponse brute: {content.Length} caractères.");
        return content;
    }

    private async Task<string> ReadApiKeyAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKeyFilePath))
        {
            throw new InvalidOperationException("Le chemin du fichier de clé API GitHub Models est vide.");
        }

        var apiKey = await File.ReadAllTextAsync(_options.ApiKeyFilePath, cancellationToken);
        apiKey = apiKey.Trim();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Le fichier de clé API GitHub Models est vide.");
        }

        System.Diagnostics.Debug.WriteLine("[AI] Clé API chargée depuis fichier local.");
        return apiKey;
    }
}
