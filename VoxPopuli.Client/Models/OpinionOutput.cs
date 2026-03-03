using Microsoft.ML.Data;

namespace VoxPopuli.Client.Models;

/// <summary>
/// Classe de sortie pour le modèle ML.NET de prédiction d'opinions.
/// </summary>
public class OpinionOutput
{
    /// <summary>
    /// Vecteur d'opinion prédit (5 dimensions).
    /// </summary>
    [VectorType(5)]
    [ColumnName("Score")]
    public float[] PredictedOpinionVector { get; set; } = new float[5];
}
