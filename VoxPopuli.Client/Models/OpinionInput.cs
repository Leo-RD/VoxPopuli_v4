using Microsoft.ML.Data;

namespace VoxPopuli.Client.Models;

/// <summary>
/// Classe d'entrée pour le modèle ML.NET de prédiction d'opinions.
/// </summary>
public class OpinionInput
{
    /// <summary>
    /// Vecteur d'opinion normalisé (5 dimensions).
    /// </summary>
    [VectorType(5)]
    [ColumnName("Features")]
    public float[] OpinionVector { get; set; } = new float[5];
}
