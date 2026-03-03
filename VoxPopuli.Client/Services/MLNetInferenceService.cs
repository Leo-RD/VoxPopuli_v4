using Microsoft.ML;
using VoxPopuli.Client.Models;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Service d'inférence pour le modèle ML.NET de prédiction d'opinions politiques.
/// </summary>
public class MLNetInferenceService : IDisposable
{
    private MLContext? _mlContext;
    private ITransformer? _model;
    private PredictionEngine<OpinionInput, OpinionOutput>? _predictionEngine;
    private readonly string _modelPath;
    private bool _isInitialized;
    private bool _useDemoMode = true;

    public MLNetInferenceService()
    {
        _modelPath = Path.Combine(FileSystem.AppDataDirectory, "VoxPopuli.mlnet");
    }

    /// <summary>
    /// Initialise le modèle ML.NET (à appeler au démarrage).
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Vérifier si le fichier existe dans les ressources
            if (await FileSystem.AppPackageFileExistsAsync("VoxPopuli.mlnet"))
            {
                System.Diagnostics.Debug.WriteLine("✅ ML.NET: Fichier VoxPopuli.mlnet trouvé, chargement...");

                // Copier le modèle depuis les ressources vers AppDataDirectory
                using var stream = await FileSystem.OpenAppPackageFileAsync("VoxPopuli.mlnet");
                using var fileStream = File.Create(_modelPath);
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();

                // Initialiser ML.NET
                _mlContext = new MLContext(seed: 0);
                
                // Charger le modèle
                _model = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
                
                // Créer le prediction engine
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<OpinionInput, OpinionOutput>(_model);
                
                _useDemoMode = false;

                System.Diagnostics.Debug.WriteLine($"✅ ML.NET: Modèle chargé avec succès!");
                System.Diagnostics.Debug.WriteLine($"   - Input Schema: {modelInputSchema}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ ML.NET: Fichier VoxPopuli.mlnet introuvable, mode DEMO activé");
                _useDemoMode = true;
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ML.NET: Erreur lors de l'initialisation - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"   Mode DEMO activé");
            _useDemoMode = true;
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Exécute une prédiction sur un vecteur d'opinion.
    /// </summary>
    public float[] Predict(float[] inputVector)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Le service doit être initialisé avec InitializeAsync()");

        // MODE DEMO : Simulation
        if (_useDemoMode)
        {
            return SimulatePredict(inputVector);
        }

        try
        {
            var input = new OpinionInput { OpinionVector = inputVector };
            var prediction = _predictionEngine!.Predict(input);
            
            return prediction.PredictedOpinionVector ?? inputVector;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur prédiction ML.NET: {ex.Message}");
            return SimulatePredict(inputVector);
        }
    }

    /// <summary>
    /// Simulation d'une prédiction pour le mode DEMO
    /// </summary>
    private float[] SimulatePredict(float[] inputVector)
    {
        var random = new Random();
        var output = new float[inputVector.Length];

        // Simulation : Perturbation des valeurs
        for (int i = 0; i < inputVector.Length; i++)
        {
            output[i] = Math.Clamp(
                inputVector[i] + (float)(random.NextDouble() - 0.5) * 0.4f,
                0f, 1f
            );
        }

        return output;
    }

    /// <summary>
    /// Traitement par batch pour optimiser les performances.
    /// </summary>
    public float[][] PredictBatch(float[][] inputVectors)
    {
        if (!_isInitialized)
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
        _predictionEngine?.Dispose();
        _predictionEngine = null;
        _model = null;
        _mlContext = null;
        _isInitialized = false;
    }
}
