using VoxPopuli.Client.Models;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Service d'analyse de phrases politiques pour déterminer l'orientation.
/// Utilise ML.NET si disponible, sinon fallback sur mots-clés.
/// </summary>
public class PoliticalPhraseAnalyzer
{
    private readonly MLNetInferenceService _mlNetService;
    private const bool UseMLNet = true; // Toggle pour activer/désactiver ML.NET

    public PoliticalPhraseAnalyzer(MLNetInferenceService mlNetService)
    {
        _mlNetService = mlNetService;
    }

    // Expressions fortes de GAUCHE (poids 2.0) - FALLBACK uniquement
    private static readonly string[] LeftStrongPhrases = new[]
    {
        "justice sociale", "services publics", "augmenter impôts", "taxer les riches",
        "redistribuer les richesses", "accueillir les réfugiés", "aide sociale",
        "égalité des chances", "transition écologique", "lutte contre la pauvreté",
        "sécurité sociale", "droit au logement", "salaire minimum", "pas de laissés pour compte",
        "personne ne doit être laissé", "peu importe son origine", "réguler le marché",
        "transition environnementale", "réguler les entreprises"
    };

    // Expressions fortes de DROITE (poids 2.0)
    private static readonly string[] RightStrongPhrases = new[]
    {
        "baisse impôts", "réduire taxes", "liberté d'entreprendre", "marché libre",
        "propriété privée", "contrôler immigration", "réguler l'immigration", "immigration contrôlée",
        "fermer les frontières", "valeur travail", "ordre public", "sécurité nationale",
        "défense nationale", "initiative individuelle", "réduire l'état", "secteur privé",
        "mérite individuel", "récompenser le travail", "lutter contre l'assistanat"
    };

    // Mots-clés simples de GAUCHE (poids 1.0)
    private static readonly string[] LeftKeywords = new[]
    {
        "taxer", "redistribuer", "égalité", "social", "public", 
        "solidarité", "pauvreté", "progressiste", "environnement",
        "réfugiés", "droits", "collectif", "nationalisation",
        "gratuit", "universel", "inclusion", "diversité", "minorités",
        "écologie", "climat", "renouvelable", "partage", "commun",
        "syndicat", "gauche", "socialiste", "communiste"
    };

    // Mots-clés simples de DROITE (poids 1.0)
    private static readonly string[] RightKeywords = new[]
    {
        "liberté", "entreprise", "privé", "marché", "économie",
        "sécurité", "ordre", "tradition", "conservateur", "individuel", 
        "propriété", "défense", "armée", "frontière", "mérite",
        "compétition", "croissance", "investissement", "profit",
        "capitalisme", "droite", "libéral", "patrimoine", "héritage",
        "travail", "effort", "responsabilité", "autorité", "discipline"
    };

    // Seuil pour considérer une phrase comme neutre
    private const float NeutralThreshold = 0.15f;

    /// <summary>
    /// Analyse une phrase politique et retourne un score :
    /// Score négatif = phrase de gauche
    /// Score positif = phrase de droite
    /// Score proche de 0 = neutre
    /// </summary>
    public float AnalyzePhrase(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return 0f;

        // Essayer d'utiliser ML.NET si disponible
        if (UseMLNet)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🧠 Analyse via ML.NET...");
                float mlScore = _mlNetService.AnalyzePhrase(phrase);
                System.Diagnostics.Debug.WriteLine($"   → Score ML.NET: {mlScore:F2}");
                return mlScore;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erreur ML.NET, fallback sur mots-clés: {ex.Message}");
                // Continuer avec le fallback
            }
        }

        // FALLBACK : Analyse par mots-clés (code existant)
        System.Diagnostics.Debug.WriteLine($"📝 Analyse via mots-clés (fallback)...");
        return AnalyzePhraseWithKeywords(phrase);
    }

    /// <summary>
    /// Analyse par mots-clés (méthode fallback).
    /// </summary>
    private static float AnalyzePhraseWithKeywords(string phrase)
    {
        var lowerPhrase = phrase.ToLowerInvariant();
        float leftScore = 0f;
        float rightScore = 0f;

        // 1. Détecter les expressions fortes (poids 2.0)
        foreach (var strongPhrase in LeftStrongPhrases)
        {
            if (lowerPhrase.Contains(strongPhrase))
            {
                leftScore += 2f;
                System.Diagnostics.Debug.WriteLine($"   [GAUCHE] Expression forte détectée: '{strongPhrase}' (+2.0)");
            }
        }

        foreach (var strongPhrase in RightStrongPhrases)
        {
            if (lowerPhrase.Contains(strongPhrase))
            {
                rightScore += 2f;
                System.Diagnostics.Debug.WriteLine($"   [DROITE] Expression forte détectée: '{strongPhrase}' (+2.0)");
            }
        }

        // 2. Détecter les mots-clés simples (poids 1.0)
        foreach (var keyword in LeftKeywords)
        {
            if (lowerPhrase.Contains(keyword))
            {
                leftScore += 1f;
                System.Diagnostics.Debug.WriteLine($"   [GAUCHE] Mot-clé détecté: '{keyword}' (+1.0)");
            }
        }

        foreach (var keyword in RightKeywords)
        {
            if (lowerPhrase.Contains(keyword))
            {
                rightScore += 1f;
                System.Diagnostics.Debug.WriteLine($"   [DROITE] Mot-clé détecté: '{keyword}' (+1.0)");
            }
        }

        // 3. Calculer le score final
        float totalScore = leftScore + rightScore;

        if (totalScore == 0) 
        {
            System.Diagnostics.Debug.WriteLine($"   → Aucun mot-clé détecté (phrase neutre)");
            return 0f;
        }

        float finalScore = (rightScore - leftScore) / totalScore;
        System.Diagnostics.Debug.WriteLine($"   → Score brut: Gauche={leftScore}, Droite={rightScore}");
        System.Diagnostics.Debug.WriteLine($"   → Score final normalisé: {finalScore:F2}");

        return finalScore;
    }

    /// <summary>
    /// Détermine si un agent est content selon son orientation et la phrase.
    /// Gestion améliorée des phrases neutres.
    /// </summary>
    public bool IsAgentHappy(PoliticalOrientation orientation, string phrase)
    {
        float phraseScore = AnalyzePhrase(phrase);

        // Si la phrase est neutre (score proche de 0), garder l'état actuel (content par défaut)
        if (Math.Abs(phraseScore) < NeutralThreshold)
        {
            System.Diagnostics.Debug.WriteLine($"   [NEUTRE] Score {phraseScore:F2} < seuil {NeutralThreshold} → Agents restent contents");
            return true; // Phrase neutre = tout le monde reste content
        }

        // Si la phrase est de gauche (score négatif) et l'agent est de gauche → content
        // Si la phrase est de droite (score positif) et l'agent est de droite → content
        if (orientation == PoliticalOrientation.Left)
        {
            return phraseScore < -NeutralThreshold;
        }
        else
        {
            return phraseScore > NeutralThreshold;
        }
    }

    /// <summary>
    /// Analyse un discours complet en le découpant en phrases individuelles.
    /// Retourne un résultat agrégé avec l'orientation globale.
    /// </summary>
    public SpeechAnalysisResult AnalyzeSpeech(string speechText)
    {
        var result = new SpeechAnalysisResult();

        var sentences = SplitIntoSentences(speechText);
        if (sentences.Count == 0)
            return result;

        float orientedScoreSum = 0f;
        int orientedSentenceCount = 0;

        foreach (var sentence in sentences)
        {
            float score = AnalyzePhrase(sentence);

            string orientation;
            if (score < -NeutralThreshold)
            {
                orientation = "Gauche";
                result.LeftSentences++;
                orientedScoreSum += score;
                orientedSentenceCount++;
            }
            else if (score > NeutralThreshold)
            {
                orientation = "Droite";
                result.RightSentences++;
                orientedScoreSum += score;
                orientedSentenceCount++;
            }
            else
            {
                orientation = "Neutre";
                result.NeutralSentences++;
            }

            result.SentenceDetails.Add((sentence, score, orientation));
            System.Diagnostics.Debug.WriteLine($"   [{orientation}] {sentence.Substring(0, Math.Min(50, sentence.Length))}... → {score:F2}");
        }

        result.TotalSentences = sentences.Count;
        result.AverageScore = orientedSentenceCount > 0 ? orientedScoreSum / orientedSentenceCount : 0f;

        if (result.AverageScore <= -NeutralThreshold)
        {
            result.GlobalOrientation = "Gauche";
        }
        else if (result.AverageScore >= NeutralThreshold)
        {
            result.GlobalOrientation = "Droite";
        }
        else if (result.LeftSentences > result.RightSentences)
        {
            result.GlobalOrientation = "Gauche";
        }
        else if (result.RightSentences > result.LeftSentences)
        {
            result.GlobalOrientation = "Droite";
        }
        else
        {
            result.GlobalOrientation = "Neutre";
        }

        System.Diagnostics.Debug.WriteLine($"📊 Discours analysé : {result.TotalSentences} phrases, {orientedSentenceCount} orientées, Score moyen={result.AverageScore:F2}, Orientation={result.GlobalOrientation}");
        System.Diagnostics.Debug.WriteLine($"   Gauche={result.LeftSentences}, Droite={result.RightSentences}, Neutre={result.NeutralSentences}");

        return result;
    }

    /// <summary>
    /// Découpe un texte en phrases individuelles.
    /// </summary>
    private static List<string> SplitIntoSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var sentences = text.Split(new[] { '.', '!', '?', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => s.Length > 8) // Ignorer les fragments trop courts
                            .ToList();

        return sentences;
    }
}
