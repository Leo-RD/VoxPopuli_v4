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
                    Content = $"Tu es un citoyen avec l'orientation politique suivante : {orientationPolitique}. Argumente strictement selon cette idéologie."
                },
                new GitHubModelsChatMessage
                {
                    Role = "user",
                    Content = $"Discours/phrase: \"{discours}\". Donne un argumentaire."
                }
            ]
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var chatResponse = await JsonSerializer.DeserializeAsync<GitHubModelsChatResponse>(stream, JsonOptions, cancellationToken);
        return chatResponse?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
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

        return apiKey;
    }
}
