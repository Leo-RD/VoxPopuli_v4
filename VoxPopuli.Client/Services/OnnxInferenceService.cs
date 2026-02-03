using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Service d'inférence pour le modèle ONNX de prédiction d'opinions politiques.
/// </summary>
public class OnnxInferenceService : IDisposable
{
    private InferenceSession? _session;
    private readonly string _modelPath;
    private bool _isInitialized;

    public OnnxInferenceService()
    {
        _modelPath = Path.Combine(FileSystem.AppDataDirectory, "votre_modele.onnx");
    }

    /// <summary>
    /// Initialise la session ONNX (à appeler au démarrage).
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Copier le modèle depuis les ressources vers AppDataDirectory
            using var stream = await FileSystem.OpenAppPackageFileAsync("votre_modele.onnx");
            using var fileStream = File.Create(_modelPath);
            await stream.CopyToAsync(fileStream);

            // Créer la session d'inférence
            _session = new InferenceSession(_modelPath);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Échec de l'initialisation du modèle ONNX : {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exécute une prédiction sur un vecteur d'opinion.
    /// </summary>
    /// <param name="inputVector">Vecteur d'entrée (ex: 5 dimensions d'opinion)</param>
    /// <returns>Vecteur de sortie du modèle</returns>
    public float[] Predict(float[] inputVector)
    {
        if (!_isInitialized || _session == null)
            throw new InvalidOperationException("Le service doit être initialisé avec InitializeAsync()");

        try
        {
            // Créer un tensor d'entrée (ajustez les dimensions selon votre modèle)
            var inputTensor = new DenseTensor<float>(inputVector, new[] { 1, inputVector.Length });

            // Créer les inputs pour la session (ajustez le nom selon votre modèle)
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor) // Remplacez "input" par le nom réel
            };

            // Exécuter l'inférence
            using var results = _session.Run(inputs);

            // Récupérer la sortie (ajustez le nom selon votre modèle)
            var output = results.First().AsEnumerable<float>().ToArray();
            return output;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erreur lors de la prédiction : {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Traitement par batch pour optimiser les performances.
    /// </summary>
    public float[][] PredictBatch(float[][] inputVectors)
    {
        if (!_isInitialized || _session == null)
            throw new InvalidOperationException("Le service doit être initialisé avec InitializeAsync()");

        var results = new float[inputVectors.Length][];
        for (int i = 0; i < inputVectors.Length; i++)
        {
            results[i] = Predict(inputVectors[i]);
        }
        return results;
    }

    public void Dispose()
    {
        _session?.Dispose();
        _session = null;
        _isInitialized = false;
    }
}