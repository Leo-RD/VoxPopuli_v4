# 🎨 Design System - VoxPopuli 4.0

## Vue d'ensemble

VoxPopuli 4.0 utilise un design moderne basé sur **Material Design 3** avec la police **ITC Avant Garde Gothic** pour une identité visuelle professionnelle et élégante.

## 🔤 Polices

### ITC Avant Garde Gothic

Famille de polices principale utilisée dans toute l'application.

**Variantes disponibles :**
- Book (texte courant)
- Demi (semi-gras)
- Bold (gras)
- Medium (moyen)
- + variantes Oblique et Condensed

**Utilisation :**
```xaml
<Label Text="Mon texte" FontFamily="AvantGardeBook" />
<Label Text="Mon titre" FontFamily="AvantGardeBold" />
```

**Styles prédéfinis :**
- `AvantGardeTitle` - Titres principaux
- `AvantGardeHeading` - Sous-titres
- `AvantGardeBody` - Corps de texte
- `AvantGardeBodyBold` - Texte en gras

## 🎨 Icônes

### Bibliothèque d'icônes

Plus de 50 icônes Material Design disponibles via le dictionnaire de ressources `Icons.xaml`.

**Catégories :**
- 🎯 Navigation (Play, Pause, Stop, Refresh)
- ⚙️ Interface (Settings, Menu, Home, Info)
- 🔍 Zoom (ZoomIn, ZoomOut, Reset)
- 👥 Personnes (People, Group, Person)
- ⚡ Statuts (Warning, Error, Success)
- ✨ IA & Tech (Brain, AI, Sparkles)
- 📢 Communication (Broadcast, Message)
- 📊 Stats (Chart, Trending, Analytics)

**Utilisation :**
```xaml
<Label Text="{StaticResource IconPlayArrow}" FontSize="24" />
```

### Material Symbols (optionnel)

Pour une bibliothèque complète d'icônes Material Design 3 :

1. Télécharger la police :
   ```powershell
   .\download-material-symbols.ps1
   ```

2. Les icônes seront automatiquement disponibles avec le préfixe `MaterialIcons`

## 🎨 Palette de couleurs

### Couleurs principales

| Nom | Hex | Usage |
|-----|-----|-------|
| Primary | `#3498DB` | Actions principales, liens |
| Secondary | `#9B59B6` | Actions secondaires |
| Success | `#27AE60` | Succès, validations |
| Warning | `#F39C12` | Avertissements |
| Error | `#E74C3C` | Erreurs, suppressions |

### Couleurs neutres

| Nom | Hex | Usage |
|-----|-----|-------|
| Dark | `#2C3E50` | Texte principal |
| Gray | `#95A5A6` | Texte secondaire |
| Light | `#ECF0F1` | Arrière-plans |

## 📁 Structure des fichiers

```
VoxPopuli.Client/
├── Resources/
│   ├── Fonts/
│   │   ├── ITC Avant Garde Gothic Bold/
│   │   ├── ITC Avant Garde Gothic CE Book/
│   │   ├── ITC Avant Garde Gothic CE Demi/
│   │   ├── ... (autres variantes)
│   │   ├── README.md
│   │   ├── MATERIAL_SYMBOLS_GUIDE.md
│   │   └── EXEMPLES_UTILISATION.md
│   └── Styles/
│       ├── Colors.xaml
│       ├── Styles.xaml
│       └── Icons.xaml (nouveau)
├── MauiProgram.cs (modifié)
└── ...
```

## 🚀 Démarrage rapide

### 1. Les polices sont déjà configurées

Toutes les variantes ITC Avant Garde Gothic sont enregistrées automatiquement dans `MauiProgram.cs`.

### 2. Utiliser les icônes

```xaml
<!-- Bouton avec icône -->
<Button BackgroundColor="#27AE60" TextColor="White">
    <Button.FormattedText>
        <FormattedString>
            <Span Text="{StaticResource IconPlayArrow}" />
            <Span Text=" Démarrer" FontFamily="AvantGardeDemi"/>
        </FormattedString>
    </Button.FormattedText>
</Button>
```

### 3. Utiliser les styles

```xaml
<!-- Titre -->
<Label Text="Mon Titre" Style="{StaticResource AvantGardeTitle}" />

<!-- Texte courant -->
<Label Text="Mon texte" FontFamily="AvantGardeBook" />
```

## 📚 Documentation

- **[README.md](Resources/Fonts/README.md)** - Documentation complète des polices et icônes
- **[MATERIAL_SYMBOLS_GUIDE.md](Resources/Fonts/MATERIAL_SYMBOLS_GUIDE.md)** - Guide d'intégration Material Symbols
- **[EXEMPLES_UTILISATION.md](Resources/Fonts/EXEMPLES_UTILISATION.md)** - Exemples de code XAML
- **[CHANGEMENTS_VISUELS.md](../../../CHANGEMENTS_VISUELS.md)** - Récapitulatif des modifications

## 🛠️ Scripts utiles

### Télécharger Material Symbols

```powershell
.\download-material-symbols.ps1
```

Ce script télécharge automatiquement la police d'icônes Material Symbols Outlined.

## 💡 Exemples complets

Consultez le fichier [EXEMPLES_UTILISATION.md](Resources/Fonts/EXEMPLES_UTILISATION.md) pour des exemples détaillés de :
- Boutons avec icônes
- Cards avec icônes et titres
- Barres d'outils
- Layouts combinant icônes et texte
- Utilisation de FormattedString

## 🎯 Prochaines étapes

1. ✅ Polices ITC Avant Garde Gothic configurées
2. ✅ Icônes Material Design intégrées
3. ✅ Styles globaux définis
4. 🔲 Télécharger Material Symbols (optionnel)
5. 🔲 Personnaliser la palette de couleurs selon vos besoins

## 🤝 Contribution

Pour ajouter de nouvelles icônes ou styles :
1. Modifier `Resources/Styles/Icons.xaml` pour les icônes
2. Modifier `Resources/Styles/Styles.xaml` pour les styles
3. Documenter dans `EXEMPLES_UTILISATION.md`

## 📝 Notes

- Toutes les polices sont chargées au démarrage de l'application
- Les icônes sont disponibles globalement via StaticResource
- Les styles par défaut utilisent ITC Avant Garde Gothic
- Compatible avec toutes les plateformes .NET MAUI
