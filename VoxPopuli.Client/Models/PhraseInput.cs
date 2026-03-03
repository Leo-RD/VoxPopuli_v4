using Microsoft.ML.Data;

namespace VoxPopuli.Client.Models;

/// <summary>
/// Classe d'entrée pour l'analyse de phrases politiques avec ML.NET.
/// Doit correspondre EXACTEMENT au schéma du modèle entraîné.
/// </summary>
public class PhraseInput
{
    /// <summary>
    /// Texte de la phrase politique à analyser.
    /// NE PAS utiliser LoadColumn pour un modèle déjà entraîné !
    /// </summary>
    [ColumnName("Text")]
    public string Text { get; set; } = string.Empty;
}
