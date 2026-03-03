# 🚀 Guide de Démarrage Rapide - VoxPopuli Analyse Politique

## 📦 Installation du modèle ML.NET

1. **Placez votre fichier `VoxPopuli.mlnet`** dans le dossier :
   ```
   VoxPopuli.Client/Resources/MLModels/VoxPopuli.mlnet
   ```

2. **Le fichier est déjà configuré** dans le `.csproj` :
   ```xml
   <MauiAsset Include="Resources\MLModels\VoxPopuli.mlnet" />
   ```

## ✅ Vérification

Le système fonctionne **avec ou sans modèle** :
- ✅ **Avec modèle** : Utilise ML.NET pour les prédictions
- ✅ **Sans modèle** : Mode DEMO avec simulation

## 🎯 Test de la fonctionnalité

### 1. Lancez l'application

```bash
dotnet build
dotnet run
```

### 2. Testez les phrases politiques

#### Exemple 1 : Phrase de GAUCHE
```
"Il faut taxer les richesses pour redistribuer les ressources"
```
**Résultat attendu :**
- ~50% des agents deviennent VERTS (gauche, alignés)
- ~50% des agents deviennent ROUGES (droite, opposition) et bougent 2x plus vite

#### Exemple 2 : Phrase de DROITE
```
"Il faut baisser les impôts pour libérer l'entreprise"
```
**Résultat attendu :**
- ~50% des agents deviennent VERTS (droite, alignés)
- ~50% des agents deviennent ROUGES (gauche, opposition) et bougent 2x plus vite

### 3. Observez les logs

Dans la console de débogage :
```
📊 Population initialisée : 500 agents
   - Gauche: 248, Droite: 252
📢 Phrase politique analysée: 'Il faut taxer les richesses...'
   Score: -0.67 (Gauche)
   Résultat: 248 contents (verts), 252 pas contents (rouges)
```

## 🎮 Contrôles disponibles

| Bouton | Action |
|--------|--------|
| **💬 Saisie + Analyser** | Analyse votre phrase personnalisée |
| **▶ Diffuser Message A** | Phrase de gauche prédéfinie |
| **▶ Diffuser Message B** | Phrase de droite prédéfinie |
| **↺ Réinitialiser** | Recrée 500 agents avec nouvelles orientations |

## 🐛 Dépannage

### Le modèle ne se charge pas
**Vérifiez :**
1. Le fichier `VoxPopuli.mlnet` existe bien dans `Resources/MLModels/`
2. Le fichier est marqué comme `MauiAsset` dans le `.csproj`
3. Consultez les logs : `⚠️ ML.NET: Fichier VoxPopuli.mlnet introuvable`

**Solution :** Le mode DEMO fonctionne sans modèle !

### Les agents ne changent pas de couleur
**Vérifiez :**
1. Vous avez cliqué sur "Analyser et Diffuser"
2. La phrase contient des mots-clés politiques
3. Consultez les logs pour voir le score de la phrase

### Tous les agents ont la même couleur
**Normal !** Si votre phrase est très orientée, tous les agents de l'orientation opposée seront rouges.

## 📊 Résultats attendus

Pour une phrase claire :
- **Répartition 50/50** : Verts (contents) vs Rouges (pas contents)
- **Vitesse différente** : Les rouges se déplacent 2x plus vite
- **Score affiché** : Dans les logs de débogage

## 🎨 Personnalisation

### Ajouter des mots-clés
Modifiez `PoliticalPhraseAnalyzer.cs` :

```csharp
private static readonly string[] LeftKeywords = new[]
{
    "taxer", "redistribuer", // ... ajoutez ici
};
```

### Changer les vitesses
Modifiez `SimulationViewModel.cs` :

```csharp
private const float HappyAgentSpeed = 1.5f;    // Agents verts
private const float UnhappyAgentSpeed = 3.0f;  // Agents rouges
```

### Changer les couleurs
Modifiez dans `AnalyzePoliticalPhrase()` :

```csharp
agent.RenderColor = SKColors.Green;  // Content
agent.RenderColor = SKColors.Red;    // Pas content
```

## 🔗 Prochaines étapes

1. **Tester avec votre modèle ML.NET réel**
2. **Ajouter plus de phrases de test**
3. **Analyser les statistiques de réaction**
4. **Créer des graphiques de visualisation**

---

**Bon test ! 🚀**
