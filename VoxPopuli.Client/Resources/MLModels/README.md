# 📁 Dossier des Modèles ML.NET

## 📍 Placement du modèle

Placez votre fichier **`VoxPopuli.mlnet`** dans ce dossier.

## ✅ Configuration

Le modèle est automatiquement chargé au démarrage de l'application via le service `MLNetInferenceService`.

### Chemin du modèle :
```
VoxPopuli.Client/Resources/MLModels/VoxPopuli.mlnet
```

### Format d'entrée attendu :
- **OpinionVector** : `float[5]` - Vecteur d'opinion normalisé (5 dimensions)

### Format de sortie :
- **PredictedOpinionVector** : `float[5]` - Vecteur d'opinion prédit

## 🧪 Mode DEMO

Si le fichier `VoxPopuli.mlnet` n'est pas trouvé, le service fonctionne en mode DEMO avec des prédictions simulées.
