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
    private bool _useDemoMode = true; // Mode DEMO par défaut

    public OnnxInferenceService()
    {
        // CORRECTION : Utiliser le bon nom de fichier
        _modelPath = Path.Combine(FileSystem.AppDataDirectory, "test_model.onnx");
    }

    /// <summary>
    /// Initialise la session ONNX (à appeler au démarrage).
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Vérifier si le fichier existe dans les ressources
            if (await FileSystem.AppPackageFileExistsAsync("test_model.onnx"))
            {
                System.Diagnostics.Debug.WriteLine("✅ ONNX: Fichier test_model.onnx trouvé, chargement...");

                // Copier le fichier et s'assurer que les streams sont fermés
                using (var stream = await FileSystem.OpenAppPackageFileAsync("test_model.onnx"))
                using (var fileStream = File.Create(_modelPath))
                {
                    await stream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync(); // Forcer l'écriture
                } // Les streams sont maintenant fermés

                // Maintenant on peut charger le modèle
                _session = new InferenceSession(_modelPath);
                _useDemoMode = false;

                System.Diagnostics.Debug.WriteLine($"✅ ONNX: Modèle chargé avec succès!");
                System.Diagnostics.Debug.WriteLine($"   - Inputs: {string.Join(", ", _session.InputMetadata.Keys)}");
                System.Diagnostics.Debug.WriteLine($"   - Outputs: {string.Join(", ", _session.OutputMetadata.Keys)}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ ONNX: Fichier test_model.onnx introuvable, mode DEMO activé");
                _useDemoMode = true;
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ONNX: Erreur lors de l'initialisation - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Mode DEMO activé");
            _useDemoMode = true;
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Convertit un vecteur d'opinion en description textuelle pour l'analyse de sentiment.
    /// </summary>
    private string OpinionVectorToText(float[] opinionVector)
    {
        var themes = new[] { "économie", "immigration", "environnement", "sécurité", "éducation" };
        var opinions = new List<string>();

        for (int i = 0; i < Math.Min(opinionVector.Length, themes.Length); i++)
        {
            if (opinionVector[i] > 0.6f)
                opinions.Add($"fortement favorable à {themes[i]}");
            else if (opinionVector[i] < 0.4f)
                opinions.Add($"opposé à {themes[i]}");
        }

        return opinions.Any() 
            ? $"Opinion politique: {string.Join(", ", opinions)}"
            : "Opinion politique neutre";
    }

    /// <summary>
    /// Exécute une prédiction de sentiment sur du texte.
    /// </summary>
    public float PredictSentiment(string text)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Le service doit être initialisé avec InitializeAsync()");

        // MODE DEMO : Simulation
        if (_useDemoMode)
        {
            return SimulateSentimentPredict(text);
        }

        try
        {
            // Créer les inputs pour le modèle ML.NET
            var sentimentTextTensor = new DenseTensor<string>(new[] { text }, new[] { 1 });
            var labelTensor = new DenseTensor<bool>(new[] { false }, new[] { 1 }); // Label vide pour inférence

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("SentimentText", sentimentTextTensor),
                NamedOnnxValue.CreateFromTensor("Label", labelTensor)
            };

            // Exécuter l'inférence
            using var results = _session!.Run(inputs);

            // Récupérer le score de sentiment (Score.output)
            var scoreOutput = results.FirstOrDefault(r => r.Name == "Score.output");
            if (scoreOutput != null)
            {
                var scores = scoreOutput.AsEnumerable<float>().ToArray();
                return scores.Length > 1 ? scores[1] : scores[0]; // Probabilité de sentiment positif
            }

            return 0.5f; // Neutre par défaut
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur prédiction sentiment : {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
            return SimulateSentimentPredict(text);
        }
    }

    /// <summary>
    /// Exécute une prédiction sur un vecteur d'opinion (version legacy).
    /// </summary>
    public float[] Predict(float[] inputVector)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Le service doit être initialisé avec InitializeAsync()");

        // MODE DEMO : Simulation uniquement pour cette version
        return SimulatePredict(inputVector);
    }

    /// <summary>
    /// Simulation d'une prédiction de sentiment pour le mode DEMO
    /// </summary>
    private float SimulateSentimentPredict(string text)
    {
        var random = new Random(text.GetHashCode()); // Reproductible
        return (float)random.NextDouble();
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
        _session?.Dispose();
        _session = null;
        _isInitialized = false;
    }
}