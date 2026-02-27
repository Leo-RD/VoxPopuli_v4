# Guide d'intégration de Material Symbols (Icônes Material Design 3)

## Option 1 : Téléchargement manuel

1. Visitez [Google Fonts - Material Symbols](https://fonts.google.com/icons)
2. Cliquez sur "Download all" ou sélectionnez les icônes souhaitées
3. Téléchargez le fichier de police (préférablement **Material Symbols Outlined**)
4. Renommez le fichier en `MaterialSymbolsOutlined.ttf`
5. Placez-le dans `VoxPopuli.Client\Resources\Fonts\`

## Option 2 : Téléchargement via PowerShell

Exécutez cette commande dans le terminal PowerShell depuis le répertoire racine du projet :

```powershell
# Télécharger Material Symbols Outlined
$url = "https://github.com/google/material-design-icons/raw/master/variablefont/MaterialSymbolsOutlined%5BFILL%2CGRAD%2Copsz%2Cwght%5D.ttf"
$output = "VoxPopuli.Client\Resources\Fonts\MaterialSymbolsOutlined.ttf"
Invoke-WebRequest -Uri $url -OutFile $output
Write-Host "✓ Material Symbols Outlined téléchargé avec succès !" -ForegroundColor Green
```

## Étape 2 : Enregistrer la police dans MauiProgram.cs

Ajoutez cette ligne dans la section `.ConfigureFonts()` de `MauiProgram.cs` :

```csharp
fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialIcons");
```

## Étape 3 : Utiliser les icônes dans XAML

### Méthode 1 : Code Unicode

Chaque icône Material a un code Unicode. Exemple pour l'icône "home" :

```xaml
<Label Text="&#xe88a;" FontFamily="MaterialIcons" FontSize="24" />
```

### Méthode 2 : Créer des ressources réutilisables

Dans `Resources/Styles/Styles.xaml`, ajoutez :

```xaml
<!-- Icônes Material Design -->
<x:String x:Key="IconHome">&#xe88a;</x:String>
<x:String x:Key="IconSettings">&#xe8b8;</x:String>
<x:String x:Key="IconPerson">&#xe7fd;</x:String>
<x:String x:Key="IconPlay">&#xe037;</x:String>
<x:String x:Key="IconPause">&#xe034;</x:String>
<x:String x:Key="IconRefresh">&#xe5d5;</x:String>
<x:String x:Key="IconZoomIn">&#xe8ff;</x:String>
<x:String x:Key="IconZoomOut">&#xe900;</x:String>
<x:String x:Key="IconBrain">&#xe8fd;</x:String>
<x:String x:Key="IconBroadcast">&#xe9be;</x:String>
```

Utilisation :

```xaml
<Label Text="{StaticResource IconHome}" FontFamily="MaterialIcons" FontSize="24" />
```

## Étape 4 : Remplacer les symboles actuels

Remplacez les symboles Unicode actuels par les codes Material Symbols :

| Ancien | Nouveau Material Symbol | Code Unicode |
|--------|------------------------|--------------|
| ▶ | play_arrow | &#xe037; |
| ⏸ | pause | &#xe034; |
| ⚙ | settings | &#xe8b8; |
| 👤 | person | &#xe7fd; |
| ⚡ | bolt | &#xe3be; |
| ⚠ | warning | &#xe002; |
| ✨ | auto_awesome | &#xe65f; |
| 📢 | campaign | &#xef42; |
| ↺ | refresh | &#xe5d5; |
| + | add | &#xe145; |
| − | remove | &#xe15b; |

## Références

- [Material Symbols Guide](https://developers.google.com/fonts/docs/material_symbols)
- [Material Icons Search](https://fonts.google.com/icons)
- [Codepoints List](https://github.com/google/material-design-icons/blob/master/font/MaterialIcons-Regular.codepoints)

## Exemple complet d'utilisation

### StartPage.xaml
```xaml
<Button FontFamily="MaterialIcons">
    <Button.FormattedText>
        <FormattedString>
            <Span Text="&#xe037; " FontFamily="MaterialIcons" FontSize="18"/>
            <Span Text="Démarrer" FontFamily="AvantGardeDemi"/>
        </FormattedString>
    </Button.FormattedText>
</Button>
```

### SimulationPage.xaml
```xaml
<HorizontalStackLayout Spacing="8">
    <Label Text="&#xe037;" 
           FontFamily="MaterialIcons"
           TextColor="#27AE60" 
           FontSize="16" 
           VerticalOptions="Center"/>
    <Label Text="Simulation en Cours" 
           FontFamily="AvantGardeDemi"
           FontSize="16" 
           TextColor="#2C3E50"
           VerticalOptions="Center"/>
</HorizontalStackLayout>
```
