using Microsoft.ML;
using Microsoft.ML.Data;
using VoxPopuli.Client.Models;
using Vox_populi_test_model;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Service d'inférence pour le modèle ML.NET de prédiction d'opinions politiques.
/// Supporte à la fois l'analyse de vecteurs d'opinion ET de phrases textuelles.
/// </summary>
public class MLNetInferenceService : IDisposable
{
    private MLContext? _mlContext;
    private ITransformer? _model;
    private PredictionEngine<OpinionInput, OpinionOutput>? _opinionPredictionEngine;
    private PredictionEngine<Vox_populi_test_model.VoxPopuli.ModelInput, MinimalPhraseOutput>? _phrasePredictionEngine;
    private bool _isInitialized;
    private bool _useDemoMode = true;
    private bool _supportsTextInput = false;

    /// <summary>
    /// Classe de sortie minimale pour le moteur de prédiction de phrases.
    /// N'inclut que les colonnes dont on a besoin, évitant le conflit de type
    /// sur la colonne 'Text' (String vs float[]) du schema du modèle.
    /// </summary>
    private sealed class MinimalPhraseOutput
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = string.Empty;

        [ColumnName("Score")]
        public float[] Score { get; set; } = [];
    }

    public MLNetInferenceService()
    {
        // Pas besoin de chemin local, on charge depuis le stream directement
    }

    /// <summary>
    /// Initialise le modèle ML.NET (à appeler au démarrage).
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // === CODE DE DIAGNOSTIC TEMPORAIRE ===
            string[] pathsToTry = new[]
            {
                "Models/VoxPopuli.mlnet",
                "MLModels/VoxPopuli.mlnet",
                "Resources/MLModels/VoxPopuli.mlnet",
                "VoxPopuli.mlnet"
            };

            System.Diagnostics.Debug.WriteLine("🔍 TEST DE TOUS LES CHEMINS POSSIBLES:");
            foreach (var path in pathsToTry)
            {
                bool exists = await FileSystem.AppPackageFileExistsAsync(path);
                System.Diagnostics.Debug.WriteLine($"   {(exists ? "✅ TROUVÉ" : "❌ ABSENT")} : {path}");
            }
            System.Diagnostics.Debug.WriteLine("");
            // === FIN DU CODE DE DIAGNOSTIC ===

            // Vérifier si le fichier existe dans les ressources (chemin complet avec dossier)
            const string modelResourcePath = "Models/VoxPopuli.mlnet";

            if (await FileSystem.AppPackageFileExistsAsync(modelResourcePath))
            {
                System.Diagnostics.Debug.WriteLine($"✅ ML.NET: Fichier {modelResourcePath} trouvé, chargement...");

                // Initialiser ML.NET
                _mlContext = new MLContext(seed: 0);

                // Charger le modèle DIRECTEMENT depuis le stream (sans copier sur disque)
                using var stream = await FileSystem.OpenAppPackageFileAsync(modelResourcePath);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset pour la lecture

                // Charger depuis le MemoryStream
                _model = _mlContext.Model.Load(memoryStream, out var modelInputSchema);

                // Essayer de créer un prediction engine pour les phrases (texte)
                try
                {
                    _phrasePredictionEngine = _mlContext.Model.CreatePredictionEngine<Vox_populi_test_model.VoxPopuli.ModelInput, MinimalPhraseOutput>(_model);
                    _supportsTextInput = true;
                    System.Diagnostics.Debug.WriteLine($"✅ ML.NET: Modèle d'analyse de PHRASES chargé!");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ML.NET: Le modèle ne supporte pas l'analyse de phrases (texte)");
                    System.Diagnostics.Debug.WriteLine($"   Erreur: {ex.Message}");
                    _supportsTextInput = false;
                }

                _useDemoMode = false;

                System.Diagnostics.Debug.WriteLine($"✅ ML.NET: Modèle chargé avec succès!");
                System.Diagnostics.Debug.WriteLine($"   - Input Schema: {modelInputSchema}");
                System.Diagnostics.Debug.WriteLine($"   - Supporte texte: {_supportsTextInput}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ ML.NET: Fichier {modelResourcePath} introuvable, mode DEMO activé");
                System.Diagnostics.Debug.WriteLine($"   Assurez-vous que le fichier est dans Resources/MLModels/ et configuré comme MauiAsset");
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
    /// Analyse une phrase politique et retourne un score (0 = gauche, 1 = droite).
    /// </summary>
    public float AnalyzePhrase(string phrase)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Le service doit être initialisé avec InitializeAsync()");

        // MODE DEMO ou modèle ne supporte pas le texte : Fallback sur mots-clés
        if (_useDemoMode || !_supportsTextInput || _phrasePredictionEngine == null)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ ML.NET: Analyse de phrase non disponible, mode FALLBACK");
            return SimulatePhraseAnalysis(phrase);
        }

        try
        {
            // Utiliser la classe ModelInput générée par Model Builder
            var input = new Vox_populi_test_model.VoxPopuli.ModelInput 
            { 
                Text = phrase,
                Label = "" // Pas nécessaire pour l'inférence, mais requis par la classe
            };

            var prediction = _phrasePredictionEngine.Predict(input);

            System.Diagnostics.Debug.WriteLine($"🧠 ML.NET Prédiction:");
            System.Diagnostics.Debug.WriteLine($"   - Label prédit: {prediction.PredictedLabel}");
            System.Diagnostics.Debug.WriteLine($"   - Scores: [{string.Join(", ", prediction.Score.Select(s => s.ToString("F3")))}]");

            // Vérifier s'il s'agit de la classe "Autre" (sans poids politique)
            if (prediction.PredictedLabel.ToLowerInvariant().Contains("autre"))
            {
                System.Diagnostics.Debug.WriteLine($"   - Label 'Autre' détecté, score forcé à 0 (Neutre).");
                return 0.0f; // Neutre
            }

            // Pour un modèle de classification, on gère les probabilités
            // Il faut déterminer quelle classe correspond à quelle orientation
            float normalizedScore;

            if (prediction.Score.Length >= 2)
            {
                // Stratégie simple : on regarde quel index a le score le plus élevé
                // et on compare avec le label prédit pour comprendre l'ordre

                int maxScoreIndex = prediction.Score[0] > prediction.Score[1] ? 0 : 1;
                string predictedLabel = prediction.PredictedLabel.ToLowerInvariant();

                // Déterminer si Score[0] correspond à gauche ou droite
                bool score0IsLeft;

                if (predictedLabel.Contains("left") || predictedLabel.Contains("gauche") || predictedLabel == "0")
                {
                    // Le label prédit est "gauche" ou "0"
                    // Donc l'index avec le score max correspond à gauche
                    score0IsLeft = (maxScoreIndex == 0);
                }
                else if (predictedLabel.Contains("right") || predictedLabel.Contains("droite") || predictedLabel == "1")
                {
                    // Le label prédit est "droite" ou "1"
                    // Donc l'index avec le score max correspond à droite
                    score0IsLeft = (maxScoreIndex != 0);
                }
                else
                {
                    // Fallback : on assume que 0 = gauche
                    score0IsLeft = true;
                }

                // Normaliser entre -1 (gauche) et +1 (droite)
                if (score0IsLeft)
                {
                    // Score[0] = gauche, Score[1] = droite
                    normalizedScore = prediction.Score[1] - prediction.Score[0];
                }
                else
                {
                    // Score[0] = droite, Score[1] = gauche
                    normalizedScore = prediction.Score[0] - prediction.Score[1];
                }

                System.Diagnostics.Debug.WriteLine($"   - Interprétation: Score[0]={prediction.Score[0]:F3} {(score0IsLeft ? "gauche" : "droite")}, Score[1]={prediction.Score[1]:F3} {(score0IsLeft ? "droite" : "gauche")}");
                System.Diagnostics.Debug.WriteLine($"   - Score normalisé: {normalizedScore:F3} ({(normalizedScore < -0.15f ? "GAUCHE" : normalizedScore > 0.15f ? "DROITE" : "NEUTRE")})");
            }
            else
            {
                // Fallback si structure inattendue
                System.Diagnostics.Debug.WriteLine($"⚠️ Structure de Score inattendue, fallback sur label");
                normalizedScore = prediction.PredictedLabel switch
                {
                    "1" => 1.0f,
                    "0" => -1.0f,
                    var label when label.ToLowerInvariant().Contains("right") || label.ToLowerInvariant().Contains("droite") => 1.0f,
                    var label when label.ToLowerInvariant().Contains("left") || label.ToLowerInvariant().Contains("gauche") => -1.0f,
                    _ => 0.0f
                };
            }

            return normalizedScore;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur analyse phrase ML.NET: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
            return SimulatePhraseAnalysis(phrase);
        }
    }

    /// <summary>
    /// Simulation d'analyse de phrase (fallback si modèle non disponible).
    /// </summary>
    private float SimulatePhraseAnalysis(string phrase)
    {
        // Fallback simple basé sur des mots-clés
        var lowerPhrase = phrase.ToLowerInvariant();
        int leftCount = 0;
        int rightCount = 0;

        string[] leftWords = { "taxer", "redistribuer", "social", "public", "solidarité" };
        string[] rightWords = { "liberté", "entreprise", "marché", "privé", "sécurité" };

        foreach (var word in leftWords)
            if (lowerPhrase.Contains(word)) leftCount++;

        foreach (var word in rightWords)
            if (lowerPhrase.Contains(word)) rightCount++;

        if (leftCount + rightCount == 0) return 0;

        // Normaliser entre -1 et +1
        return (float)(rightCount - leftCount) / (leftCount + rightCount);
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
            var prediction = _opinionPredictionEngine!.Predict(input);

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
        _opinionPredictionEngine?.Dispose();
        _opinionPredictionEngine = null;
        _phrasePredictionEngine?.Dispose();
        _phrasePredictionEngine = null;
        _model = null;
        _mlContext = null;
        _isInitialized = false;
    }
}
