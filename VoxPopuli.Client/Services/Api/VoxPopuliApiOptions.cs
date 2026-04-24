namespace VoxPopuli.Client.Services.Api;

public static class VoxPopuliApiOptions
{
    public const string HttpClientName = "VoxPopuliApi";

    // TODO: Remplacer par l'URL réelle de votre API ASP.NET
    public const string BaseUrl = "http://74.161.43.203:5000";

    // Identifiants fournis pour initialiser rapidement le flux JWT
    public const string DefaultUsername = "Vox_user";
    public const string DefaultPassword = "85KEOMuKgOmgaNj";

    public const string JwtStorageKey = "voxpopuli_api_jwt";
}
