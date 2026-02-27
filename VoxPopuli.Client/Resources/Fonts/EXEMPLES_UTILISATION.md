# Exemples d'utilisation des icônes et polices

## Utilisation des icônes depuis le dictionnaire de ressources

Les icônes sont définies dans `Resources/Styles/Icons.xaml` et sont automatiquement disponibles dans toute l'application.

### Exemple 1 : Icône simple

```xaml
<!-- Icône de lecture -->
<Label Text="{StaticResource IconPlayArrow}" 
       FontSize="24" 
       TextColor="#3498DB" />
```

### Exemple 2 : Icône avec texte (Button)

```xaml
<Button BackgroundColor="#27AE60" TextColor="White" FontFamily="AvantGardeDemi">
    <Button.FormattedText>
        <FormattedString>
            <Span Text="{StaticResource IconPlayArrow}" />
            <Span Text=" Démarrer" FontFamily="AvantGardeDemi"/>
        </FormattedString>
    </Button.FormattedText>
</Button>
```

### Exemple 3 : Icône avec texte (Label)

```xaml
<Label FontFamily="AvantGardeBook" TextColor="#2C3E50">
    <Label.FormattedText>
        <FormattedString>
            <Span Text="{StaticResource IconSettings}" FontSize="18"/>
            <Span Text=" " />
            <Span Text="Configuration" FontFamily="AvantGardeDemi"/>
        </FormattedString>
    </Label.FormattedText>
</Label>
```

### Exemple 4 : Icône dans un HorizontalStackLayout

```xaml
<HorizontalStackLayout Spacing="8">
    <Label Text="{StaticResource IconWarning}" 
           FontSize="20" 
           TextColor="#F39C12" 
           VerticalOptions="Center"/>
    <Label Text="Attention : Avertissement important" 
           FontFamily="AvantGardeBook"
           FontSize="14" 
           TextColor="#856404"
           VerticalOptions="Center"/>
</HorizontalStackLayout>
```

### Exemple 5 : Icône dans un bouton avec couleur de fond

```xaml
<Button BackgroundColor="#9B59B6" TextColor="White" FontFamily="AvantGardeDemi">
    <Button.FormattedText>
        <FormattedString>
            <Span Text="{StaticResource IconBrain}" FontSize="16"/>
            <Span Text=" Exécuter IA" FontFamily="AvantGardeDemi"/>
        </FormattedString>
    </Button.FormattedText>
</Button>
```

## Utilisation des polices ITC Avant Garde Gothic

### Exemple 1 : Utilisation directe

```xaml
<!-- Titre principal -->
<Label Text="VoxPopuli 4.0" 
       FontFamily="AvantGardeBold" 
       FontSize="48" 
       TextColor="White" />

<!-- Sous-titre -->
<Label Text="Simulateur d'Opinion Publique" 
       FontFamily="AvantGardeBook" 
       FontSize="20" 
       TextColor="#BDC3C7" />
```

### Exemple 2 : Utilisation des styles prédéfinis

```xaml
<!-- Titre avec style -->
<Label Text="Mon Titre" Style="{StaticResource AvantGardeTitle}" />

<!-- Sous-titre avec style -->
<Label Text="Mon Sous-titre" Style="{StaticResource AvantGardeHeading}" />

<!-- Corps de texte avec style -->
<Label Text="Mon texte" Style="{StaticResource AvantGardeBody}" />

<!-- Texte en gras avec style -->
<Label Text="Texte important" Style="{StaticResource AvantGardeBodyBold}" />
```

### Exemple 3 : Mélange de polices dans FormattedString

```xaml
<Label>
    <Label.FormattedText>
        <FormattedString>
            <Span Text="Agents: " FontFamily="AvantGardeBook" FontSize="14"/>
            <Span Text="500" FontFamily="AvantGardeBold" FontSize="16" TextColor="#3498DB"/>
        </FormattedString>
    </Label.FormattedText>
</Label>
```

## Palette de couleurs recommandée (Material Design 3)

```xaml
<!-- Couleurs primaires -->
<Color x:Key="Primary">#3498DB</Color>        <!-- Bleu -->
<Color x:Key="Secondary">#9B59B6</Color>      <!-- Violet -->
<Color x:Key="Success">#27AE60</Color>        <!-- Vert -->
<Color x:Key="Warning">#F39C12</Color>        <!-- Orange -->
<Color x:Key="Error">#E74C3C</Color>          <!-- Rouge -->

<!-- Couleurs neutres -->
<Color x:Key="Dark">#2C3E50</Color>           <!-- Bleu foncé -->
<Color x:Key="Light">#ECF0F1</Color>          <!-- Gris clair -->
<Color x:Key="Gray">#95A5A6</Color>           <!-- Gris -->

<!-- Couleurs de texte -->
<Color x:Key="TextPrimary">#2C3E50</Color>
<Color x:Key="TextSecondary">#7F8C8D</Color>
<Color x:Key="TextLight">#BDC3C7</Color>
```

## Icônes disponibles

| Catégorie | Icônes |
|-----------|--------|
| **Navigation** | IconPlayArrow, IconPause, IconStop, IconRefresh, IconClose |
| **Interface** | IconSettings, IconMenu, IconHome, IconInfo, IconHelp |
| **Zoom** | IconZoomIn, IconZoomOut, IconZoomReset, IconFullscreen |
| **Personnes** | IconPeople, IconGroup, IconPerson, IconAccount |
| **Statuts** | IconWarning, IconError, IconSuccess, IconBolt |
| **IA & Tech** | IconBrain, IconAI, IconStar, IconSparkles |
| **Communication** | IconBroadcast, IconMessage, IconNotification, IconSend |
| **Édition** | IconAdd, IconRemove, IconEdit, IconDelete |
| **Média** | IconPlay, IconSkipNext, IconSkipPrevious, IconVolumeUp |
| **Stats** | IconChart, IconTrending, IconAnalytics |
| **Fichiers** | IconFolder, IconFile, IconDownload, IconUpload |
| **Temps** | IconClock, IconCalendar, IconTimer |
| **Flèches** | IconArrowUp, IconArrowDown, IconArrowLeft, IconArrowRight |
| **Toggle** | IconCheck, IconCheckCircle, IconRadioOn, IconRadioOff |

## Exemples de layouts complets

### Card avec icône et titre

```xaml
<Border BackgroundColor="White" 
        Padding="15" 
        StrokeThickness="1" 
        Stroke="#E0E0E0">
    <Border.StrokeShape>
        <RoundRectangle CornerRadius="8"/>
    </Border.StrokeShape>
    <VerticalStackLayout Spacing="10">
        <Label Text="{StaticResource IconChart}" 
               FontSize="32" 
               TextColor="#3498DB"
               HorizontalOptions="Center"/>
        <Label Text="Statistiques" 
               FontFamily="AvantGardeDemi"
               FontSize="18" 
               TextColor="#2C3E50"
               HorizontalOptions="Center"/>
        <Label Text="Visualisez vos données en temps réel" 
               FontFamily="AvantGardeBook"
               FontSize="12" 
               TextColor="#7F8C8D"
               HorizontalOptions="Center"
               HorizontalTextAlignment="Center"/>
    </VerticalStackLayout>
</Border>
```

### Barre d'outils

```xaml
<HorizontalStackLayout Spacing="10" Padding="10" BackgroundColor="#F8F9FA">
    <Button Text="{StaticResource IconPlayArrow}" 
            BackgroundColor="#27AE60" 
            TextColor="White"
            Padding="12,8"
            FontSize="16"/>
    <Button Text="{StaticResource IconPause}" 
            BackgroundColor="#E74C3C" 
            TextColor="White"
            Padding="12,8"
            FontSize="16"/>
    <Button Text="{StaticResource IconRefresh}" 
            BackgroundColor="#3498DB" 
            TextColor="White"
            Padding="12,8"
            FontSize="16"/>
    <Button Text="{StaticResource IconSettings}" 
            BackgroundColor="#95A5A6" 
            TextColor="White"
            Padding="12,8"
            FontSize="16"/>
</HorizontalStackLayout>
```
