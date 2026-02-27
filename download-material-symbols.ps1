# Script de téléchargement de Material Symbols Outlined
# Ce script télécharge la police d'icônes Material Design 3

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Téléchargement de Material Symbols   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier si le dossier Fonts existe
$fontsDir = "VoxPopuli.Client\Resources\Fonts"
if (-not (Test-Path $fontsDir)) {
    Write-Host "❌ Erreur: Le dossier $fontsDir n'existe pas." -ForegroundColor Red
    exit 1
}

# URL de téléchargement de Material Symbols Outlined
$url = "https://github.com/google/material-design-icons/raw/master/variablefont/MaterialSymbolsOutlined%5BFILL%2CGRAD%2Copsz%2Cwght%5D.ttf"
$outputFile = Join-Path $fontsDir "MaterialSymbolsOutlined.ttf"

# Vérifier si le fichier existe déjà
if (Test-Path $outputFile) {
    Write-Host "⚠️  Le fichier MaterialSymbolsOutlined.ttf existe déjà." -ForegroundColor Yellow
    $response = Read-Host "Voulez-vous le remplacer? (O/N)"
    if ($response -ne "O" -and $response -ne "o") {
        Write-Host "❌ Téléchargement annulé." -ForegroundColor Yellow
        exit 0
    }
    Remove-Item $outputFile -Force
}

# Téléchargement
Write-Host "📥 Téléchargement en cours..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri $url -OutFile $outputFile -UseBasicParsing
    Write-Host "✅ Téléchargement réussi !" -ForegroundColor Green
    Write-Host ""
    Write-Host "📁 Fichier sauvegardé : $outputFile" -ForegroundColor Gray
    
    # Afficher la taille du fichier
    $fileSize = (Get-Item $outputFile).Length / 1MB
    Write-Host "📊 Taille du fichier : $([math]::Round($fileSize, 2)) MB" -ForegroundColor Gray
    
} catch {
    Write-Host "❌ Erreur lors du téléchargement : $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Prochaines étapes                    " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Enregistrer la police dans MauiProgram.cs :" -ForegroundColor Yellow
Write-Host '   fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialIcons");' -ForegroundColor White
Write-Host ""
Write-Host "2. Utiliser les icônes dans XAML :" -ForegroundColor Yellow
Write-Host '   <Label Text="&#xe88a;" FontFamily="MaterialIcons" />' -ForegroundColor White
Write-Host ""
Write-Host "3. Consulter le guide complet :" -ForegroundColor Yellow
Write-Host "   VoxPopuli.Client\Resources\Fonts\MATERIAL_SYMBOLS_GUIDE.md" -ForegroundColor White
Write-Host ""
Write-Host "✨ Configuration terminée avec succès !" -ForegroundColor Green
