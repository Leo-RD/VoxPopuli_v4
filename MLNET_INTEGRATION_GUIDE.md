# 🧠 Intégration ML.NET pour l'Analyse de Phrases

## ✅ Implémentation Terminée

Le système utilise maintenant **ML.NET** pour analyser les phrases politiques au lieu du système de mots-clés basique !

## 🏗️ Architecture Mise à Jour

```
┌─────────────────────────────────────────┐
│  SimulationViewModel                    │
│  AnalyzePoliticalPhrase(phrase)         │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│  PoliticalPhraseAnalyzer                │
│  (Injecté via DI)                       │
│  ✅ Utilise ML.NET si disponible        │
│  📝 Fallback sur mots-clés sinon        │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│  MLNetInferenceService                  │
│  AnalyzePhrase(string phrase)           │
│  → float score (-1 à +1)                │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│  VoxPopuli.mlnet                        │
│  Input: PhraseInput (Text: string)      │
│  Output: PhraseOutput (Score: float)    │
└─────────────────────────────────────────┘
```

## 📁 Nouveaux Fichiers Créés

### 1. **PhraseInput.cs**
```csharp
public class PhraseInput
{
    [ColumnName("Text")]
    public string Text { get; set; }
}
```
**Utilité** : Entrée pour le modèle ML.NET (texte de la phrase)

### 2. **PhraseOutput.cs**
```csharp
public class PhraseOutput
{
    [ColumnName("Score")]
    public float Score { get; set; }  // 0 = gauche, 1 = droite
    
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; }  // "Left", "Right"
    
    [ColumnName("Probability")]
    public float Probability { get; set; }  // Confiance
}
```
**Utilité** : Sortie du modèle ML.NET (classification gauche/droite)

## 📝 Fichiers Modifiés

### 1. **MLNetInferenceService.cs**
**Ajouts :**
- Nouvelle méthode `AnalyzePhrase(string phrase)` → float
- Détection automatique du type de modèle (texte vs vecteur)
- Deux `PredictionEngine` :
  - `_phrasePredictionEngine` pour les phrases
  - `_opinionPredictionEngine` pour les vecteurs d'opinion
- Fallback sur mots-clés si modèle non disponible

### 2. **PoliticalPhraseAnalyzer.cs**
**Changements :**
- N'est plus `static` → Devient un service injectable
- Reçoit `MLNetInferenceService` par injection de dépendances
- Méthode `AnalyzePhrase()` :
  1. Essaye d'utiliser ML.NET
  2. Si échec, fallback sur mots-clés (code existant)
- `UseMLNet = true` → Toggle pour activer/désactiver ML.NET

### 3. **SimulationViewModel.cs**
**Changements :**
- Reçoit `PoliticalPhraseAnalyzer` injecté via constructeur
- Utilise `_phraseAnalyzer.AnalyzePhrase()` au lieu de la méthode statique

### 4. **MauiProgram.cs**
**Ajout :**
```csharp
builder.Services.AddSingleton<PoliticalPhraseAnalyzer>();
```

## 🔄 Flux d'Exécution

### Scénario 1 : Modèle ML.NET Disponible ✅

```
Utilisateur saisit : "Il faut taxer les richesses"
         ↓
SimulationViewModel.AnalyzePoliticalPhrase()
         ↓
PoliticalPhraseAnalyzer.AnalyzePhrase()
         ↓
MLNetInferenceService.AnalyzePhrase()
         ↓
PredictionEngine<PhraseInput, PhraseOutput>
         ↓
Modèle VoxPopuli.mlnet
         ↓
Score: 0.15 (converti en -0.70)
         ↓
Gauche → Contents (verts), Droite → Pas contents (rouges)
```

### Scénario 2 : Modèle ML.NET Absent ⚠️

```
Utilisateur saisit : "Il faut taxer les richesses"
         ↓
PoliticalPhraseAnalyzer.AnalyzePhrase()
         ↓
MLNetInferenceService non disponible
         ↓
AnalyzePhraseWithKeywords() (fallback)
         ↓
Détection mots-clés : "taxer" (+2.0 gauche)
         ↓
Score: -0.67 (gauche)
         ↓
Gauche → Contents, Droite → Pas contents
```

## 📊 Format du Modèle ML.NET

Votre modèle `VoxPopuli.mlnet` doit avoir :

### Input Schema
```
Text: string (texte de la phrase)
```

### Output Schema
```
Score: float (0.0 = gauche, 1.0 = droite)
PredictedLabel: string (optionnel, ex: "Left", "Right")
Probability: float (optionnel, confiance 0-1)
```

### Conversion du Score

Le service convertit automatiquement :
- **Score ML.NET** : 0.0 → 1.0
- **Score interne** : -1.0 → +1.0

```csharp
float normalizedScore = (mlScore * 2) - 1;
// ML: 0.0 → Interne: -1.0 (gauche)
// ML: 0.5 → Interne:  0.0 (neutre)
// ML: 1.0 → Interne: +1.0 (droite)
```

## 🧪 Tester l'Intégration

### 1. **Avec le modèle ML.NET**

1. Placez `VoxPopuli.mlnet` dans `Resources/MLModels/`
2. Lancez l'application
3. Consultez les logs :

```
✅ ML.NET: Fichier VoxPopuli.mlnet trouvé, chargement...
✅ ML.NET: Modèle d'analyse de PHRASES chargé!
   - Supporte texte: True

📢 Phrase politique analysée: 'il faut taxer les richesses'
🧠 Analyse via ML.NET...
🧠 ML.NET Prédiction:
   - Score: 0.15
   - Label: Left
   - Probabilité: 0.85
   → Score ML.NET: -0.70
```

### 2. **Sans le modèle (mode DEMO)**

```
⚠️ ML.NET: Fichier VoxPopuli.mlnet introuvable, mode DEMO activé

📢 Phrase politique analysée: 'il faut taxer les richesses'
⚠️ ML.NET: Analyse de phrase non disponible, mode FALLBACK
📝 Analyse via mots-clés (fallback)...
   [GAUCHE] Expression forte détectée: 'taxer les riches' (+2.0)
   → Score final normalisé: -0.67
```

## 🎛️ Configuration

### Activer/Désactiver ML.NET

Dans `PoliticalPhraseAnalyzer.cs` :
```csharp
private const bool UseMLNet = true;  // false = force le fallback mots-clés
```

### Ajuster le Seuil de Neutralité

Dans `PoliticalPhraseAnalyzer.cs` :
```csharp
private const float NeutralThreshold = 0.15f;  // ±15%
```

## 🔍 Diagnostic

### Vérifier quel système est utilisé

**Logs à chercher :**
- `🧠 Analyse via ML.NET...` → ML.NET utilisé
- `📝 Analyse via mots-clés (fallback)...` → Fallback actif

### Si ML.NET ne fonctionne pas

1. **Vérifiez le fichier** : `Resources/MLModels/VoxPopuli.mlnet` existe ?
2. **Vérifiez les logs** : Erreur de chargement ?
3. **Vérifiez le schéma** : Le modèle attend-il bien `Text` en input ?
4. **Mode DEMO** : Le fallback fonctionne toujours !

## 🎯 Avantages de cette Architecture

✅ **Flexible** : Fonctionne avec ou sans ML.NET  
✅ **Robuste** : Fallback automatique si problème  
✅ **Testable** : Mode DEMO pour tests sans modèle  
✅ **Injectable** : Utilise l'injection de dépendances  
✅ **Configurable** : Toggle `UseMLNet` pour forcer le fallback

## 🚀 Prochaines Étapes

1. **Entraînez votre modèle ML.NET** avec des phrases françaises
2. **Placez-le** dans `Resources/MLModels/VoxPopuli.mlnet`
3. **Testez** avec des phrases réelles
4. **Comparez** les résultats ML.NET vs mots-clés
5. **Ajustez** le seuil de neutralité si nécessaire

---

**Le système est maintenant 100% compatible avec votre modèle ML.NET ! 🎉**

Si le modèle n'est pas disponible, le fallback sur mots-clés assure le fonctionnement.
