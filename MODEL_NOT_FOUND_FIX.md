# 🔧 Dépannage : Modèle ML.NET Introuvable

## ❌ Problème Résolu

**Erreur :** `ML.NET: Fichier VoxPopuli.mlnet introuvable, mode DEMO activé`

**Cause :** Le chemin de chargement ne correspondait pas à l'emplacement réel du fichier.

## ✅ Solution Appliquée

### Avant
```csharp
// ❌ Cherchait à la racine des ressources
await FileSystem.AppPackageFileExistsAsync("VoxPopuli.mlnet")
```

### Après
```csharp
// ✅ Cherche dans le bon dossier
const string modelResourcePath = "Models/VoxPopuli.mlnet";
await FileSystem.AppPackageFileExistsAsync(modelResourcePath)
```

## 📁 Structure Requise

Votre fichier doit être ici :
```
VoxPopuli.Client/
├── Resources/
│   └── MLModels/
│       └── VoxPopuli.mlnet  ← ICI
```

## 🔍 Vérification

### 1. Vérifiez l'emplacement physique
```powershell
# Dans le terminal, à la racine du projet
Test-Path "VoxPopuli.Client\Resources\MLModels\VoxPopuli.mlnet"
# Doit retourner: True
```

### 2. Vérifiez le .csproj
Ouvrez `VoxPopuli.Client.csproj` et cherchez :
```xml
<ItemGroup>
  <MauiAsset Include="Resources\MLModels\VoxPopuli.mlnet" />
</ItemGroup>
```

### 3. Logs de débogage attendus
Après le correctif, vous devriez voir :
```
✅ ML.NET: Fichier Models/VoxPopuli.mlnet trouvé, chargement...
✅ ML.NET: Modèle d'analyse de PHRASES chargé!
   - Supporte texte: True
```

## 🐛 Si le problème persiste

### Option 1 : Vérifier les propriétés du fichier dans Visual Studio
1. Faites un clic droit sur `VoxPopuli.mlnet` dans l'Explorateur de solutions
2. Sélectionnez **Propriétés**
3. Vérifiez :
   - **Build Action** : `MauiAsset`
   - **Copy to Output Directory** : `Copy if newer` ou `Always`

### Option 2 : Nettoyer et rebuilder
```powershell
dotnet clean
dotnet build
```

### Option 3 : Chemin relatif alternatif
Si `Models/VoxPopuli.mlnet` ne fonctionne pas, essayez :
```csharp
// Dans MLNetInferenceService.cs
const string modelResourcePath = "MLModels/VoxPopuli.mlnet";
// OU
const string modelResourcePath = "VoxPopuli.mlnet";  // Si déplacé à la racine
```

## 📊 Chemins MAUI par Plateforme

Le système de fichiers MAUI gère automatiquement les chemins selon la plateforme :

| Plateforme | Chemin physique réel |
|------------|---------------------|
| **Windows** | `AppData/Local/Packages/.../LocalState/VoxPopuli.mlnet` |
| **Android** | `/data/user/0/com.companyname.voxpopuli/files/VoxPopuli.mlnet` |
| **iOS** | `~/Documents/VoxPopuli.mlnet` |

**Important :** Vous n'avez pas à gérer ces chemins manuellement ! `FileSystem.AppPackageFileAsync()` s'en charge.

## ✅ Checklist de Validation

- [ ] Le fichier `VoxPopuli.mlnet` existe dans `Resources/MLModels/`
- [ ] Le `.csproj` contient `<MauiAsset Include="Resources\MLModels\VoxPopuli.mlnet" />`
- [ ] Le code utilise `"Models/VoxPopuli.mlnet"` (sans `Resources/`)
- [ ] Build réussi sans erreurs
- [ ] Les logs montrent `✅ ML.NET: Fichier Models/VoxPopuli.mlnet trouvé`

## 🎯 Résultat Attendu

Après cette correction, le modèle sera chargé avec succès :

```
✅ ML.NET: Fichier Models/VoxPopuli.mlnet trouvé, chargement...
✅ ML.NET: Modèle d'analyse de PHRASES chargé!
✅ ML.NET: Modèle chargé avec succès!
   - Input Schema: DataViewSchema
   - Supporte texte: True

📢 Phrase politique analysée: 'il faut taxer les richesses'
🧠 Analyse via ML.NET...
🧠 ML.NET Prédiction:
   - Score: 0.15
   - Label: Left
   - Probabilité: 0.85
   → Score ML.NET: -0.70
   Score: -0.70 (Gauche)
   Résultat: 248 contents (verts), 252 pas contents (rouges)
```

---

**Le modèle devrait maintenant être détecté et chargé correctement ! 🚀**
