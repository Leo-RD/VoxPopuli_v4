# 📁 Dossier des Modèles ML.NET

## 📍 Placement du modèle

Placez votre fichier **`VoxPopuli.mlnet`** dans ce dossier.

## ✅ Configuration

Le modèle est automatiquement chargé au démarrage de l'application via le service `MLNetInferenceService`.

### Chemin du modèle :
```
VoxPopuli.Client/Resources/MLModels/VoxPopuli.mlnet
```

### Chemin de chargement dans le code :
```csharp
// Le code charge avec ce chemin relatif (sans "Resources/")
const string modelResourcePath = "Models/VoxPopuli.mlnet";
```

### Format d'entrée attendu (pour l'analyse de phrases) :
- **Text** : `string` - La phrase politique à analyser

### Format de sortie :
- **Score** : `float` (0.0 = gauche, 1.0 = droite)
- **PredictedLabel** : `string` (optionnel, ex: "Left", "Right")
- **Probability** : `float` (optionnel, confiance 0-1)

## 🧪 Mode DEMO

Si le fichier `VoxPopuli.mlnet` n'est pas trouvé, le service fonctionne en mode DEMO avec :
- Fallback automatique sur analyse par mots-clés
- Logs indiquant : `⚠️ ML.NET: Fichier Models/VoxPopuli.mlnet introuvable, mode DEMO activé`

## 🔍 Vérification

### Logs de succès :
```
✅ ML.NET: Fichier Models/VoxPopuli.mlnet trouvé, chargement...
✅ ML.NET: Modèle d'analyse de PHRASES chargé!
   - Supporte texte: True
```

### Si le modèle n'est pas trouvé :
1. Vérifiez que le fichier existe bien ici : `Resources/MLModels/VoxPopuli.mlnet`
2. Vérifiez le `.csproj` : `<MauiAsset Include="Resources\MLModels\VoxPopuli.mlnet" />`
3. Consultez `MODEL_NOT_FOUND_FIX.md` pour le dépannage complet
