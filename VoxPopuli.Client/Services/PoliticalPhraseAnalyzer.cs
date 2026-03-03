using VoxPopuli.Client.Models;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Service d'analyse de phrases politiques pour déterminer l'orientation.
/// </summary>
public class PoliticalPhraseAnalyzer
{
    private static readonly string[] LeftKeywords = new[]
    {
        "taxer", "redistribuer", "égalité", "social", "public", 
        "solidarité", "pauvreté", "justice sociale", "progressiste",
        "environnement", "réfugiés", "immigration", "droits", "welfare",
        "collectif", "nationalis", "services publics", "augmenter impôts"
    };

    private static readonly string[] RightKeywords = new[]
    {
        "liberté", "entreprise", "privé", "marché", "économie",
        "baisse impôts", "réduire taxes", "sécurité", "ordre",
        "tradition", "conservateur", "individuel", "propriété",
        "défense", "armée", "frontières", "immigration contrôlée"
    };

    /// <summary>
    /// Analyse une phrase politique et retourne un score :
    /// Score négatif = phrase de gauche
    /// Score positif = phrase de droite
    /// Score proche de 0 = neutre
    /// </summary>
    public static float AnalyzePhrase(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return 0f;

        var lowerPhrase = phrase.ToLowerInvariant();
        float leftScore = 0f;
        float rightScore = 0f;

        // Compter les mots-clés de gauche
        foreach (var keyword in LeftKeywords)
        {
            if (lowerPhrase.Contains(keyword))
                leftScore += 1f;
        }

        // Compter les mots-clés de droite
        foreach (var keyword in RightKeywords)
        {
            if (lowerPhrase.Contains(keyword))
                rightScore += 1f;
        }

        // Retourner un score normalisé (-1 à +1)
        float totalScore = leftScore + rightScore;
        if (totalScore == 0) return 0f;

        return (rightScore - leftScore) / totalScore;
    }

    /// <summary>
    /// Détermine si un agent est content selon son orientation et la phrase.
    /// </summary>
    public static bool IsAgentHappy(PoliticalOrientation orientation, string phrase)
    {
        float phraseScore = AnalyzePhrase(phrase);

        // Si la phrase est de gauche (score négatif) et l'agent est de gauche → content
        // Si la phrase est de droite (score positif) et l'agent est de droite → content
        if (orientation == PoliticalOrientation.Left)
        {
            return phraseScore < 0; // Content si phrase de gauche
        }
        else
        {
            return phraseScore > 0; // Content si phrase de droite
        }
    }
}
