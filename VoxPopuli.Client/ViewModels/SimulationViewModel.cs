using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using VoxPopuli.Client.Models;
using VoxPopuli.Client.Models.Api;
using VoxPopuli.Client.Services;
using VoxPopuli.Client.Services.Api;
using System.Linq;

namespace VoxPopuli.Client.ViewModels;

public partial class SimulationViewModel : BaseViewModel
{
    private readonly MLNetInferenceService _mlNetService;
    private readonly PoliticalPhraseAnalyzer _phraseAnalyzer;
    private readonly MqttAgentService _mqttService;
    private readonly SimulationsApiService _simulationsApiService;
    private readonly Random _random = new Random();

    // Pool persistant
    private readonly List<AgentModel> _agentPool = new();

    private static readonly string[] _firstNames =
    [
        "Alice", "Bruno", "Clara", "David", "Emma", "Félix", "Gaëlle", "Hugo",
        "Inès", "Julien", "Karine", "Lucas", "Marie", "Nathan", "Olivia",
        "Paul", "Quentin", "Rosa", "Samuel", "Théa", "Ugo", "Valère",
        "Wendy", "Xavier", "Yasmine", "Zoé",
        "Adam", "Béatrice", "Cédric", "Daphné", "Éloïse", "Fabien", "Grace",
        "Hamid", "Iris", "Jérémy", "Lena", "Maxime", "Noémie", "Oscar",
        "Paola", "Raphaël", "Sonia", "Tristan", "Ursula", "Victor", "Zara",
        "Amélie", "Baptiste", "Camille", "Diane", "Ethan", "Florence", "Gaëtan",
        "Hortense", "Ivan", "Jade", "Kevin", "Laura", "Mathieu", "Nina"
    ];

    private static readonly string[] _lastNames =
    [
        "Martin", "Bernard", "Dupont", "Moreau", "Lemaire", "Lefebvre",
        "Garcia", "Roux", "Fournier", "Girard", "Bonnet", "Lambert",
        "Fontaine", "Rousseau", "Vincent", "Leroy", "Chevalier", "Morin",
        "Simon", "Laurent", "Michel", "Blanc", "Guerin", "Boyer",
        "Petit", "Grand", "Renard", "Faure", "Marchand", "Picard",
        "Colin", "Perrin", "Gautier", "Clément", "Gauthier", "Noel",
        "Lacroix", "Masson", "Hubert", "Jacquet", "Aubert", "Barbier",
        "Sanchez", "Benoit", "Dumas", "Berger", "Ferry", "Renaud"
    ];

    // Agents dont la présence dans la simulation est garantie (Easter Eggs)
    private static readonly (string Name, PoliticalOrientation Orientation, string Group)[] _easterEggs =
    [
        ("Tiburce Gandji",  PoliticalOrientation.Left,  "A"),
        ("Hugo Boulicaut-Raffort", PoliticalOrientation.Right, "B")
    ];

    // Constantes pour la simulation
    private const float WorldWidth = 1000f;
    private const float WorldHeight = 1000f;
    private const float DirectionChangeChance = 0.05f; // 5% de chance de changer de direction par frame
    private const float HappyAgentSpeed = 1.5f; // Vitesse des agents contents (verts)
    private const float UnhappyAgentSpeed = 3.0f; // Vitesse des agents pas contents (rouges)

    // Tracking du FPS
    private DateTime _lastFrameTime = DateTime.Now;
    private int _frameCount = 0;
    private double _accumulatedTime = 0;

    // Chronomètre de simulation
    private TimeSpan _simulationElapsed = TimeSpan.Zero;

    [ObservableProperty]
    private int currentFps = 60;

    [ObservableProperty]
    private string simulationTime = "00:00";

    [ObservableProperty]
    private string currentPoliticalPhrase = "";

    public List<AgentModel> Population { get; private set; } = new();

    [ObservableProperty]
    private int agentCount;

    [ObservableProperty]
    private bool isRunning = true; // État de la simulation

    [ObservableProperty]
    private string simulationButtonText = "⏸ Arrêter";

    [ObservableProperty]
    private Color simulationButtonColor = Color.FromArgb("#E74C3C");

    [ObservableProperty]
    private float zoomLevel = 1.0f; // Niveau de zoom (1.0 = 100%)

    [ObservableProperty]
    private AgentModel? selectedAgent = null;

    [ObservableProperty]
    private bool isAgentSelected = false;

    [ObservableProperty]
    private string selectedAgentInfo = "";

    // ===== MODE DISCOURS =====
    [ObservableProperty]
    private bool isSpeechMode = false;

    [ObservableProperty]
    private string speechModeButtonText = "📄 Mode Discours";

    [ObservableProperty]
    private string speechText = "";

    [ObservableProperty]
    private string speechResultSummary = "";

    [ObservableProperty]
    private bool hasSpeechResult = false;

    /// <summary>Dernier message brut reçu depuis la Raspberry via MQTT.</summary>
    [ObservableProperty]
    private string lastRaspberryMessage = "";

    private string _apiSyncStatus = "API: en attente";
    public string ApiSyncStatus
    {
        get => _apiSyncStatus;
        set => SetProperty(ref _apiSyncStatus, value);
    }

    public SimulationViewModel(
        MLNetInferenceService mlNetService,
        PoliticalPhraseAnalyzer phraseAnalyzer,
        MqttAgentService mqttService,
        SimulationsApiService simulationsApiService)
    {
        System.Diagnostics.Debug.WriteLine("📊 SimulationViewModel: Initialisation...");
        _mlNetService = mlNetService;
        _phraseAnalyzer = phraseAnalyzer;
        _mqttService = mqttService;
        _simulationsApiService = simulationsApiService;

        // Abonnement aux messages entrants de la Raspberry
        _mqttService.MessageFromRaspberryReceived += OnRaspberryMessageReceived;

        Task.Run(async () => await _mlNetService.InitializeAsync());
        Task.Run(async () => await _mqttService.ConnectAsync());
        InitializePopulation(500);
        System.Diagnostics.Debug.WriteLine($"📊 SimulationViewModel: {Population.Count} agents créés");
    }

    /// <summary>
    /// Traite un message reçu depuis la Raspberry (topic vox/vers/app).
    /// </summary>
    private void OnRaspberryMessageReceived(string payload)
    {
        LastRaspberryMessage = payload;
        System.Diagnostics.Debug.WriteLine($"📩 Message Raspberry reçu : {payload}");
        // Étendre ici pour interpréter des commandes JSON envoyées par la Raspberry
    }

    /// <summary>
    /// Réinitialise la simulation avec un nombre d'agents et une répartition gauche/droite.
    /// </summary>
    public void ResetSimulation(int count, int leftPercentage = 50)
    {
        InitializePopulation(count, leftPercentage);
    }

    [RelayCommand]
    private void ResetSimulation(object parameter = null)
    {
        int count = 500;
        if (parameter is int paramCount)
            count = paramCount;
        InitializePopulation(count, 50);
    }

    /// <summary>
    /// Augmente le niveau de zoom
    /// </summary>
    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel + 0.1f, 3.0f); // Max 300%
        System.Diagnostics.Debug.WriteLine($"🔍 Zoom In: {ZoomLevel:P0}");
    }

    /// <summary>
    /// Diminue le niveau de zoom
    /// </summary>
    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel - 0.1f, 0.5f); // Min 50%
        System.Diagnostics.Debug.WriteLine($"🔍 Zoom Out: {ZoomLevel:P0}");
    }

    /// <summary>
    /// Réinitialise le zoom à 100%
    /// </summary>
    [RelayCommand]
    private void ResetZoom()
    {
        ZoomLevel = 1.0f;
        System.Diagnostics.Debug.WriteLine($"🔍 Zoom Reset: 100%");
    }

    /// <summary>
    /// Déclenche un événement (changement de couleur des agents)
    /// </summary>
    [RelayCommand]
    private void TriggerEvent()
    {
        // Exemple de phrase de gauche
        AnalyzePoliticalPhrase("Il faut taxer les richesses pour redistribuer les ressources");
    }

    /// <summary>
    /// Diffuse le Message B (exemple : couleur violette)
    /// </summary>
    [RelayCommand]
    private void BroadcastMessageB()
    {
        // Exemple de phrase de droite
        AnalyzePoliticalPhrase("Il faut baisser les impôts pour libérer l'entreprise");
    }

    /// <summary>
    /// Analyse une phrase politique et met à jour les agents en conséquence.
    /// </summary>
    /// <param name="phrase">La phrase politique à analyser</param>
    public void AnalyzePoliticalPhrase(string phrase)
    {
        CurrentPoliticalPhrase = phrase;
        float phraseScore = _phraseAnalyzer.AnalyzePhrase(phrase);

        System.Diagnostics.Debug.WriteLine($"📢 Phrase politique analysée: '{phrase}'");
        System.Diagnostics.Debug.WriteLine($"   Score: {phraseScore:F2} ({(phraseScore < 0 ? "Gauche" : phraseScore > 0 ? "Droite" : "Neutre")})");

        int happyCount = 0;
        int unhappyCount = 0;

        foreach (var agent in Population)
        {
            // Déterminer si l'agent est content
            bool isHappy = _phraseAnalyzer.IsAgentHappy(agent.PoliticalOrientation, phrase);
            agent.IsHappy = isHappy;

            if (isHappy)
            {
                // Agent content : vert, vitesse normale
                agent.RenderColor = SKColors.Green;
                agent.MaxSpeed = HappyAgentSpeed;
                agent.CurrentEmotion = EmotionalState.Happy;
                happyCount++;
            }
            else
            {
                // Agent pas content : rouge, vitesse augmentée
                agent.RenderColor = SKColors.Red;
                agent.MaxSpeed = UnhappyAgentSpeed;
                agent.CurrentEmotion = EmotionalState.Angry;
                unhappyCount++;
            }
        }

        System.Diagnostics.Debug.WriteLine($"   Résultat: {happyCount} contents (verts), {unhappyCount} pas contents (rouges)");
    }

    /// <summary>
    /// Bascule entre le mode phrase et le mode discours.
    /// </summary>
    [RelayCommand]
    private void ToggleSpeechMode()
    {
        IsSpeechMode = !IsSpeechMode;
        SpeechModeButtonText = IsSpeechMode ? "💬 Mode Phrase" : "📄 Mode Discours";
    }

    /// <summary>
    /// Analyse un discours complet et met à jour les agents selon l'orientation globale.
    /// </summary>
    [RelayCommand]
    private async Task AnalyzeSpeech()
    {
        System.Diagnostics.Debug.WriteLine("🌐 [API] AnalyzeSpeechCommand déclenchée.");

        if (string.IsNullOrWhiteSpace(SpeechText))
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Aucun discours saisi");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"📜 Début de l'analyse du discours ({SpeechText.Length} caractères)...");

        var result = _phraseAnalyzer.AnalyzeSpeech(SpeechText);

        if (result.TotalSentences == 0)
        {
            SpeechResultSummary = "⚠️ Aucune phrase détectée dans le discours.";
            HasSpeechResult = true;
            return;
        }

        // Appliquer le résultat global aux agents (comme une phrase analysée)
        CurrentPoliticalPhrase = $"[Discours] {result.GlobalOrientation} (moy. {result.AverageScore:+0.00;-0.00})";

        int happyCount = 0;
        int unhappyCount = 0;

        foreach (var agent in Population)
        {
            bool isHappy = result.GlobalOrientation switch
            {
                "Gauche"  => agent.PoliticalOrientation == PoliticalOrientation.Left,
                "Droite"  => agent.PoliticalOrientation == PoliticalOrientation.Right,
                _         => agent.PoliticalOrientation == PoliticalOrientation.Center // Neutre → centre content
            };

            agent.IsHappy = isHappy;

            if (isHappy)
            {
                agent.RenderColor = SKColors.Green;
                agent.MaxSpeed = HappyAgentSpeed;
                agent.CurrentEmotion = EmotionalState.Happy;
                happyCount++;
            }
            else
            {
                agent.RenderColor = SKColors.Red;
                agent.MaxSpeed = UnhappyAgentSpeed;
                agent.CurrentEmotion = EmotionalState.Angry;
                unhappyCount++;
            }
        }

        // Construire le résumé
        string orientationIcon = result.GlobalOrientation switch
        {
            "Gauche" => "🔴",
            "Droite" => "🔵",
            _        => "⚪"
        };

        SpeechResultSummary =
            $"{orientationIcon} Orientation : {result.GlobalOrientation}\n" +
            $"📊 {result.TotalSentences} phrases analysées\n" +
            $"🔴 Gauche : {result.LeftSentences}  |  🔵 Droite : {result.RightSentences}  |  ⚪ Neutre : {result.NeutralSentences}\n" +
            $"📈 Score moyen : {result.AverageScore:+0.00;-0.00}\n" +
            $"😊 Contents : {happyCount}  |  😠 Mécontents : {unhappyCount}";

        HasSpeechResult = true;

        await SaveSimulationToApiInternalAsync(result, happyCount, unhappyCount);

        System.Diagnostics.Debug.WriteLine($"✅ Discours analysé : {result.GlobalOrientation}, {happyCount} contents, {unhappyCount} pas contents");
    }

    [RelayCommand]
    private async Task SaveSimulationToApi()
    {
        System.Diagnostics.Debug.WriteLine("🌐 [API] SaveSimulationToApiCommand déclenchée manuellement.");
        ApiSyncStatus = "API: bouton cliqué, préparation...";

        int happyCount = Population.Count(a => a.IsHappy);
        int unhappyCount = Population.Count - happyCount;

        var fallbackResult = new SpeechAnalysisResult
        {
            TotalSentences = 0,
            LeftSentences = 0,
            RightSentences = 0,
            NeutralSentences = 0,
            AverageScore = 0f,
            GlobalOrientation = "Neutre"
        };

        await SaveSimulationToApiInternalAsync(fallbackResult, happyCount, unhappyCount);
    }

    /// <summary>
    /// Commande pour analyser la phrase saisie par l'utilisateur
    /// </summary>
    [RelayCommand]
    private void AnalyzePhrase()
    {
        if (!string.IsNullOrWhiteSpace(CurrentPoliticalPhrase))
        {
            AnalyzePoliticalPhrase(CurrentPoliticalPhrase);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Aucune phrase politique saisie");
        }
    }

    /// <summary>
    /// Navigation vers la page de paramètres (StartPage)
    /// </summary>
    [RelayCommand]
    private async Task NavigateToSettings()
    {
        await Shell.Current.GoToAsync("//StartPage");
    }

    /// <summary>
    /// Sélectionne un agent en fonction des coordonnées cliquées (coordonnées monde virtuel)
    /// </summary>
    /// <param name="worldX">Coordonnée X dans l'espace virtuel (0-1000)</param>
    /// <param name="worldY">Coordonnée Y dans l'espace virtuel (0-1000)</param>
    public void SelectAgentAt(float worldX, float worldY)
    {
        const float clickRadius = 15f; // Rayon de détection en pixels du monde virtuel
        AgentModel? closestAgent = null;
        float closestDistance = float.MaxValue;

        // Trouver l'agent le plus proche du clic
        foreach (var agent in Population)
        {
            float dx = agent.X - worldX;
            float dy = agent.Y - worldY;
            float distance = MathF.Sqrt(dx * dx + dy * dy);

            if (distance < clickRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestAgent = agent;
            }
        }

        if (closestAgent != null)
        {
            SelectedAgent = closestAgent;
            IsAgentSelected = true;
            UpdateSelectedAgentInfo();
            System.Diagnostics.Debug.WriteLine($"🎯 Agent sélectionné: {closestAgent.Name} ({closestAgent.Id})");
            _ = _mqttService.TryPublishAgentAsync(
                closestAgent,
                phrase: CurrentPoliticalPhrase,
                discoursSummary: IsSpeechMode ? SpeechResultSummary : string.Empty);
        }
        else
        {
            // Déselectionner si clic dans le vide
            SelectedAgent = null;
            IsAgentSelected = false;
            SelectedAgentInfo = "";
        }
    }

    /// <summary>
    /// Met à jour les informations affichées pour l'agent sélectionné
    /// </summary>
    private void UpdateSelectedAgentInfo()
    {
        if (SelectedAgent == null)
        {
            SelectedAgentInfo = "";
            return;
        }

        var orientation = SelectedAgent.PoliticalOrientation switch
        {
            PoliticalOrientation.Left => "Gauche 🔴",
            PoliticalOrientation.Right => "Droite 🔵",
            _ => "Centre ⚪"
        };
        var emotion = SelectedAgent.IsHappy ? "Content 😊" : "Pas content 😠";
        var speed = $"{SelectedAgent.MaxSpeed:F1} px/frame";
        var position = $"({SelectedAgent.X:F0}, {SelectedAgent.Y:F0})";

        SelectedAgentInfo = $@"🎯 {SelectedAgent.Name}

📍 Position: {position}
🏛️ Orientation: {orientation}
😊 État: {emotion}
⚡ Vitesse: {speed}
🎨 Groupe: {SelectedAgent.Group}";

        System.Diagnostics.Debug.WriteLine($"📊 Info agent mise à jour: {SelectedAgent.Name} | {orientation}, {emotion}");
    }

    /// <summary>
    /// Désélectionne l'agent actuellement sélectionné
    /// </summary>
    [RelayCommand]
    private void DeselectAgent()
    {
        SelectedAgent = null;
        IsAgentSelected = false;
        SelectedAgentInfo = "";
    }

    /// <summary>
    /// Arrête ou redémarre la simulation
    /// </summary>
    [RelayCommand]
    private void ToggleSimulation()
    {
        IsRunning = !IsRunning;

        // Mise à jour du texte et de la couleur du bouton
        if (IsRunning)
        {
            SimulationButtonText = "⏸ Arrêter";
            SimulationButtonColor = Color.FromArgb("#E74C3C"); // Rouge
        }
        else
        {
            SimulationButtonText = "▶ Reprendre";
            SimulationButtonColor = Color.FromArgb("#27AE60"); // Vert
        }

        System.Diagnostics.Debug.WriteLine($"🎮 Simulation: {(IsRunning ? "RUNNING" : "PAUSED")}");
    }

    /// <summary>
    /// Exécute l'inférence ML.NET sur tous les agents
    /// </summary>
    [RelayCommand]
    private async Task RunInferenceAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🧠 Démarrage de l'inférence ML.NET sur {Population.Count} agents...");

            // Traitement optimisé par batch
            var inputVectors = Population.Select(a => a.OpinionVector).ToArray();
            System.Diagnostics.Debug.WriteLine($"   - Vecteurs d'entrée préparés");

            var predictions = _mlNetService.PredictBatch(inputVectors);
            System.Diagnostics.Debug.WriteLine($"   - Prédictions reçues");

            // Mise à jour des agents avec les prédictions
            for (int i = 0; i < Population.Count; i++)
            {
                var agent = Population[i];
                var oldVector = agent.OpinionVector[0];
                agent.OpinionVector = predictions[i];
                agent.RenderColor = GetColorFromOpinion(predictions[i][0]);

                // Log des premiers agents pour vérification
                if (i < 3)
                {
                    System.Diagnostics.Debug.WriteLine($"   Agent {i}: {oldVector:F2} -> {predictions[i][0]:F2}, Couleur: {agent.RenderColor}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"✅ Inférence ML.NET terminée avec succès!");

            // Forcer la notification de changement
            OnPropertyChanged(nameof(Population));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur inférence ML.NET: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }

    private SKColor GetColorFromOpinion(float opinionScore)
    {
        // Gradient de couleur basé sur le score d'opinion
        if (opinionScore > 0.6f) return SKColors.Green;
        if (opinionScore < 0.4f) return SKColors.Red;
        return SKColors.Orange;
    }

    private static string ToApiOrientation(PoliticalOrientation orientation)
    {
        return orientation switch
        {
            PoliticalOrientation.Left => "Gauche",
            PoliticalOrientation.Right => "Droite",
            PoliticalOrientation.Center => "Centre",
            _ => "Centre"
        };
    }

    private static string ToApiEmotionalState(bool isHappy)
    {
        return isHappy ? "1" : "0";
    }

    private static string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return string.Empty;
        }

        string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    private static string ExtractLastName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return string.Empty;
        }

        string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;
    }

    private async Task SaveSimulationToApiInternalAsync(SpeechAnalysisResult result, int happyCount, int unhappyCount)
    {
        try
        {
            IsBusy = true;
            ApiSyncStatus = "API: envoi en cours...";
            System.Diagnostics.Debug.WriteLine("🌐 [API] Début préparation payload simulation.");

            int leftAgents = Population.Count(a => a.PoliticalOrientation == PoliticalOrientation.Left);
            int rightAgents = Population.Count(a => a.PoliticalOrientation == PoliticalOrientation.Right);
            int centerAgents = Population.Count(a => a.PoliticalOrientation == PoliticalOrientation.Center);

            var request = new SimulationCreateRequest
            {
                SimulationId = 0,
                Titre = $"Simulation {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Discours = string.IsNullOrWhiteSpace(SpeechText) ? CurrentPoliticalPhrase : SpeechText,
                TypeTest = IsSpeechMode ? "Discours" : "Phrase",
                NbAgent = Population.Count,
                DateSimulation = DateTime.UtcNow,
                Agents = Population.Select(agent => new AgentCreateRequest
                {
                    AgentId = 0,
                    Nom = ExtractLastName(agent.Name),
                    Prenom = ExtractFirstName(agent.Name),
                    Description = string.Empty,
                    Avatar = string.Empty,
                    OriantationPolitique = ToApiOrientation(agent.PoliticalOrientation),
                    SimulationId = 0,
                    NiveauEmotion = agent.IsHappy ? 2 : 0,
                    Predictions =
                    [
                        new PredictionCreateRequest
                        {
                            PredictionId = 0,
                            Contenu = ToApiEmotionalState(agent.IsHappy),
                            AgentId = 0
                        }
                    ]
                }).ToList()
            };

            foreach (var (agent, index) in request.Agents.Take(3).Select((agent, index) => (agent, index)))
            {
                var prediction = agent.Predictions.FirstOrDefault();
                System.Diagnostics.Debug.WriteLine($"🌐 [API] Agent[{index}] prediction: Contenu={prediction?.Contenu}");
            }

            System.Diagnostics.Debug.WriteLine($"🌐 [API] Payload prêt: Agents={request.NbAgent} (G={leftAgents}, D={rightAgents}, C={centerAgents}), Type={request.TypeTest}");

            bool sent = await _simulationsApiService.CreateSimulationAsync(request);
            ApiSyncStatus = sent
                ? "API: simulation enregistrée ✅"
                : "API: échec d'enregistrement ❌";

            System.Diagnostics.Debug.WriteLine($"🌐 [API] Fin envoi simulation. Succès={sent}");
        }
        catch (Exception ex)
        {
            ApiSyncStatus = "API: erreur d'envoi ❌";
            System.Diagnostics.Debug.WriteLine($"❌ [API] Erreur envoi simulation API: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ [API] Stack: {ex.StackTrace}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ========== MOTEUR DE DÉPLACEMENT (RANDOM WALK) ==========

    /// <summary>
    /// Met à jour la logique de simulation (appelé chaque frame).
    /// Implémente le Random Walk pour chaque agent.
    /// </summary>
    public void UpdateSimulationLogic()
    {
        if (!IsRunning) return;

        // Calcul du FPS
        var currentTime = DateTime.Now;
        var deltaTime = (currentTime - _lastFrameTime).TotalSeconds;
        _lastFrameTime = currentTime;

        _frameCount++;
        _accumulatedTime += deltaTime;

        // Mise à jour du FPS toutes les 30 frames (pour éviter les fluctuations)
        if (_frameCount >= 30)
        {
            CurrentFps = (int)Math.Round(_frameCount / _accumulatedTime);
            _frameCount = 0;
            _accumulatedTime = 0;
        }

        // Mise à jour du chronomètre (cap à 100ms pour absorber les spikes de reprise de pause)
        _simulationElapsed += TimeSpan.FromSeconds(Math.Min(deltaTime, 0.1));
        SimulationTime = _simulationElapsed.ToString(@"mm\:ss");

        // Parcourir tous les agents pour mettre à jour leur position
        for (int i = 0; i < Population.Count; i++)
        {
            var agent = Population[i];
            UpdateAgentMovement(agent);
        }

        }

    /// <summary>
    /// Met à jour le mouvement d'un agent selon le Random Walk et le regroupement par affinité.
    /// </summary>
    private void UpdateAgentMovement(AgentModel agent)
    {
        // 1. Changement aléatoire de direction (Random Walk)
        if (_random.NextDouble() < DirectionChangeChance)
        {
            // Nouvelle direction aléatoire (en radians)
            agent.Direction = (float)(_random.NextDouble() * Math.PI * 2);
        }

        // 2. Calculer les forces de regroupement par affinité politique
        var (cohesionX, cohesionY) = CalculateFlockingForces(agent);

        // 3. Utiliser directement la MaxSpeed de l'agent (déjà configurée selon son état émotionnel)
        float baseSpeed = agent.MaxSpeed;

        // 4. Combiner le mouvement aléatoire avec les forces de regroupement
        float randomWeight = 0.4f;  // 40% de mouvement aléatoire
        float flockingWeight = 0.6f; // 60% d'attraction vers le groupe (augmenté pour un regroupement plus fort)

        agent.VelocityX = (MathF.Cos(agent.Direction) * baseSpeed * randomWeight) + (cohesionX * flockingWeight);
        agent.VelocityY = (MathF.Sin(agent.Direction) * baseSpeed * randomWeight) + (cohesionY * flockingWeight);

        // 5. Limiter la vitesse maximale
        float currentSpeed = MathF.Sqrt(agent.VelocityX * agent.VelocityX + agent.VelocityY * agent.VelocityY);
        if (currentSpeed > agent.MaxSpeed)
        {
            agent.VelocityX = (agent.VelocityX / currentSpeed) * agent.MaxSpeed;
            agent.VelocityY = (agent.VelocityY / currentSpeed) * agent.MaxSpeed;
        }

        // 6. Mettre à jour la position
        agent.X += agent.VelocityX;
        agent.Y += agent.VelocityY;

        // 7. Gestion des rebonds sur les bords
        HandleBoundaryCollision(agent);
    }

    /// <summary>
    /// Calcule les forces de cohésion et de répulsion basées sur l'état émotionnel (content/pas content).
    /// </summary>
    private (float forceX, float forceY) CalculateFlockingForces(AgentModel agent)
    {
        const float perceptionRadius = 80f; // Distance de perception des autres agents
        const float separationRadius = 20f; // Distance minimale entre agents
        const float cohesionStrength = 1.7f; // Force d'attraction vers les alliés (même état émotionnel)
        const float repulsionStrength = 4.0f; // Force de répulsion des adversaires (état émotionnel opposé)
        const float separationStrength = 4.0f; // Force de séparation pour éviter les collisions

        float cohesionX = 0f;
        float cohesionY = 0f;
        float separationX = 0f;
        float separationY = 0f;
        int allyCount = 0;
        int enemyCount = 0;

        foreach (var other in Population)
        {
            if (other == agent) continue;

            float dx = other.X - agent.X;
            float dy = other.Y - agent.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);

            if (distance < 0.1f) continue; // Éviter la division par zéro

            // Séparation : éviter les collisions rapprochées (tous agents)
            if (distance < separationRadius)
            {
                float separationForce = separationStrength / distance;
                separationX -= (dx / distance) * separationForce;
                separationY -= (dy / distance) * separationForce;
            }

            // Cohésion/Répulsion : uniquement dans le rayon de perception
            if (distance < perceptionRadius)
            {
                // Regroupement basé sur l'état émotionnel (IsHappy) au lieu de l'orientation politique
                bool sameEmotionalState = agent.IsHappy == other.IsHappy;

                if (sameEmotionalState)
                {
                    // Attraction vers les agents du même état émotionnel (verts avec verts, rouges avec rouges)
                    cohesionX += dx / distance * cohesionStrength;
                    cohesionY += dy / distance * cohesionStrength;
                    allyCount++;
                }
                else
                {
                    // Répulsion légère des agents d'état émotionnel opposé
                    float repulsionForce = repulsionStrength / distance;
                    cohesionX -= (dx / distance) * repulsionForce;
                    cohesionY -= (dy / distance) * repulsionForce;
                    enemyCount++;
                }
            }
        }

        // Moyenner les forces de cohésion
        if (allyCount > 0)
        {
            cohesionX /= allyCount;
            cohesionY /= allyCount;
        }

        // Combiner cohésion et séparation
        float totalForceX = cohesionX + separationX;
        float totalForceY = cohesionY + separationY;

        return (totalForceX, totalForceY);
    }

    /// <summary>
    /// Gère les collisions avec les bords de la carte (rebonds).
    /// </summary>
    private void HandleBoundaryCollision(AgentModel agent)
    {
        bool hasCollided = false;

        // Rebond sur le bord gauche ou droit
        if (agent.X < 0)
        {
            agent.X = 0;
            agent.VelocityX = -agent.VelocityX; // Inversion de la direction horizontale
            hasCollided = true;
        }
        else if (agent.X > WorldWidth)
        {
            agent.X = WorldWidth;
            agent.VelocityX = -agent.VelocityX;
            hasCollided = true;
        }

        // Rebond sur le bord haut ou bas
        if (agent.Y < 0)
        {
            agent.Y = 0;
            agent.VelocityY = -agent.VelocityY; // Inversion de la direction verticale
            hasCollided = true;
        }
        else if (agent.Y > WorldHeight)
        {
            agent.Y = WorldHeight;
            agent.VelocityY = -agent.VelocityY;
            hasCollided = true;
        }

        // Recalculer la direction après un rebond
        if (hasCollided)
        {
            agent.Direction = MathF.Atan2(agent.VelocityY, agent.VelocityX);
        }
    }

    private void InitializePopulation(int count, int leftPercentage = 50)
    {
        var random = new Random();
        AgentCount = count;

        _simulationElapsed = TimeSpan.Zero;
        SimulationTime = "00:00";

        int leftCount = (int)Math.Round(count * leftPercentage / 100.0);

        // Injecter les Easter Eggs en tête de pool (une seule fois)
        foreach (var egg in _easterEggs)
        {
            if (!_agentPool.Any(a => a.Name == egg.Name))
                _agentPool.Insert(0, new AgentModel { Name = egg.Name });
        }

        // Compléter le pool si besoin (les agents existants conservent leur Nom et Id)
        while (_agentPool.Count < count)
        {
            _agentPool.Add(new AgentModel
            {
                Name = $"{_firstNames[random.Next(_firstNames.Length)]} {_lastNames[random.Next(_lastNames.Length)]}"
            });
        }

        // Prendre les N premiers agents du pool
        Population = _agentPool.Take(count).ToList();

        // Réinitialiser uniquement l'état transitoire (position, vitesse, émotion)
        // L'identité (Name, Id) est conservée
        for (int i = 0; i < count; i++)
        {
            var agent = Population[i];
            float initialDirection = (float)(random.NextDouble() * Math.PI * 2);

            agent.X = random.Next(0, 1000);
            agent.Y = random.Next(0, 1000);
            agent.VelocityX = 0f;
            agent.VelocityY = 0f;
            agent.Direction = initialDirection;
            agent.OpinionVector = new float[5]
            {
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble()
            };
            agent.CurrentEmotion = EmotionalState.Neutral;
            agent.IsHappy = true;
            agent.IsInfluenced = false;
            agent.LastInfluenceTime = null;
            agent.RenderColor = SKColors.Green;
            agent.MaxSpeed = HappyAgentSpeed;

            // Les Easter Eggs conservent leur orientation et groupe fixes
            var egg = Array.Find(_easterEggs, e => e.Name == agent.Name);
            if (egg != default)
            {
                agent.PoliticalOrientation = egg.Orientation;
                agent.Group = egg.Group;
            }
            else
            {
                agent.PoliticalOrientation = i < leftCount
                    ? PoliticalOrientation.Left
                    : i >= count - leftCount
                        ? PoliticalOrientation.Right
                        : PoliticalOrientation.Center;
                agent.Group = agent.PoliticalOrientation switch
                {
                    PoliticalOrientation.Left => "A",
                    PoliticalOrientation.Right => "B",
                    _ => "C"
                };
            }
        }

        // Mélanger pour éviter que tous les gauches soient d'un côté au départ
        Population = Population.OrderBy(_ => random.Next()).ToList();

        System.Diagnostics.Debug.WriteLine($"📊 Population initialisée : {count} agents ({_agentPool.Count} dans le pool)");
        System.Diagnostics.Debug.WriteLine($"   - Gauche: {leftCount} ({leftPercentage}%), Droite: {count - leftCount} ({100 - leftPercentage}%)");
    }
}