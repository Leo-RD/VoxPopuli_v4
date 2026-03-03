using Microsoft.ML.Data;

namespace VoxPopuli.Client.Models;

/// <summary>
/// Classe de sortie pour l'analyse de phrases politiques avec ML.NET.
/// Doit correspondre EXACTEMENT au schéma du modèle entraîné.
/// </summary>
public class PhraseOutput
{
    /// <summary>
    /// Label prédit par le modèle de classification.
    /// Exemples: "Left", "Right", "Gauche", "Droite", "0", "1"
    /// </summary>
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;

    /// <summary>
    /// Tableau de scores/probabilités pour chaque classe.
    /// Pour un modèle binaire : [probGauche, probDroite]
    /// </summary>
    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();
}
