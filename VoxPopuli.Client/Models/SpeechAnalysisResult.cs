namespace VoxPopuli.Client.Models;

/// <summary>
/// Résultat de l'analyse d'un discours complet découpé en phrases.
/// </summary>
public class SpeechAnalysisResult
{
    public int TotalSentences { get; set; }
    public int LeftSentences { get; set; }
    public int RightSentences { get; set; }
    public int NeutralSentences { get; set; }
    public float AverageScore { get; set; }
    public string GlobalOrientation { get; set; } = "Neutre";
    public List<(string Sentence, float Score, string Orientation)> SentenceDetails { get; set; } = new();
}
