# Polices et Icônes - VoxPopuli 4.0

## Polices ITC Avant Garde Gothic

L'application utilise la famille de polices **ITC Avant Garde Gothic** comme police principale.

### Polices enregistrées

Les polices suivantes sont disponibles dans l'application via leurs aliases :

| Fichier | Alias | Usage |
|---------|-------|-------|
| ITC Avant Garde Gothic CE Book.otf | `AvantGardeBook` | Texte courant, corps de texte |
| ITC Avant Garde Gothic CE Book Oblique.otf | `AvantGardeBookOblique` | Texte courant en italique |
| ITC Avant Garde Gothic CE Demi.otf | `AvantGardeDemi` | Texte semi-gras, titres secondaires |
| ITC Avant Garde Gothic CE Demi Oblique.otf | `AvantGardeDemiOblique` | Texte semi-gras en italique |
| ITC Avant Garde Gothic Bold.otf | `AvantGardeBold` | Titres principaux, texte en gras |
| ITC Avant Garde Gothic Bold Oblique.otf | `AvantGardeBoldOblique` | Titres en gras et italique |
| ITC Avant Garde Gothic Bold Condensed.otf | `AvantGardeBoldCondensed` | Texte compact en gras |
| ITC Avant Garde Gothic Medium.otf | `AvantGardeMedium` | Texte medium |
| ITC Avant Garde Gothic Medium Oblique.otf | `AvantGardeMediumOblique` | Texte medium en italique |
| ITC Avant Garde Gothic Medium Condensed.otf | `AvantGardeMediumCondensed` | Texte medium compact |
| ITC Avant Garde Gothic Book Condensed.otf | `AvantGardeBookCondensed` | Texte courant compact |
| ITC Avant Garde Gothic Demi Condensed.otf | `AvantGardeDemiCondensed` | Texte semi-gras compact |

### Utilisation dans XAML

```xaml
<!-- Utilisation directe -->
<Label Text="Mon texte" FontFamily="AvantGardeBook" />

<!-- Utilisation avec un style -->
<Label Text="Mon titre" Style="{StaticResource AvantGardeTitle}" />
```

### Styles disponibles

Les styles suivants sont définis dans `Resources/Styles/Styles.xaml` :

- **AvantGardeTitle** : Titres principaux (Bold, 28pt)
- **AvantGardeHeading** : Sous-titres (Demi, 20pt)
- **AvantGardeBody** : Corps de texte (Book, 14pt)
- **AvantGardeBodyBold** : Corps de texte en gras (Demi, 14pt)

## Icônes Material Design

Les icônes utilisées dans l'application suivent les principes du Material Design 3.

### Symboles utilisés

| Symbole | Unicode | Usage |
|---------|---------|-------|
| ▶ | U+25B6 | Lecture, démarrer, diffuser |
| ⏸ | U+23F8 | Pause, arrêter |
| ⚙ | U+2699 | Paramètres, configuration |
| 👤 | U+1F464 | Utilisateur, agents |
| ⚡ | U+26A1 | Actions rapides, performance |
| ⚠ | U+26A0 | Avertissement, attention |
| ✨ | U+2728 | IA, inférence, intelligence |
| 📢 | U+1F4E2 | Diffusion, broadcast |
| ↺ | U+21BA | Réinitialiser, refresh |
| + | U+002B | Zoom avant, ajouter |
| − | U+2212 | Zoom arrière, réduire |

### Ajout de la police Material Symbols (optionnel)

Pour utiliser une police d'icônes Material Design complète :

1. Télécharger **Material Symbols** depuis [Google Fonts](https://fonts.google.com/icons)
2. Placer le fichier `.ttf` dans `Resources/Fonts/`
3. Enregistrer la police dans `MauiProgram.cs` :
   ```csharp
   fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialIcons");
   ```
4. Utiliser les icônes avec leur code Unicode :
   ```xaml
   <Label Text="&#xe8b6;" FontFamily="MaterialIcons" />
   ```

## Migration des emojis vers icônes

Les emojis suivants ont été remplacés par des symboles épurés :

| Ancien (emoji) | Nouveau (symbole) | Contexte |
|----------------|-------------------|----------|
| 🚀 | ▶ | Bouton "Démarrer la simulation" |
| 🧠 | ✨ | Bouton "Exécuter Inférence IA" |
| 🔄 | ↺ | Bouton "Réinitialiser Simulation" |
| 🔍+ | + | Bouton "Zoom +" |
| 🔍- | − | Bouton "Zoom -" |
| 🔍↺ | ↺ | Bouton "Reset Zoom" |

## Notes de développement

- Les polices sont chargées automatiquement au démarrage de l'application
- Les styles par défaut de `Label` et `Button` utilisent maintenant ITC Avant Garde Gothic
- Pour revenir à OpenSans, il suffit de modifier les styles dans `Styles.xaml`
- Les symboles Unicode sont compatibles avec toutes les plateformes (.NET MAUI)
