using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace VoxPopuli.Client.Models;

/// <summary>
/// Représente un "Jumeau Numérique" dans la simulation.
/// Structure de données validée par l'équipe (Contrat Phase 1.1).
/// </summary>
public class AgentModel
{
    /// <summary>
    /// Identifiant unique (UUID v4) pour la traçabilité API/Base de Données.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Vecteur normalisé représentant l'adhésion aux thèmes politiques.
    /// Format optimisé pour l'entrée du modèle ONNX (TensorFloat).
    /// </summary>
    public float[] OpinionVector { get; set; }

    /// <summary>
    /// État psychologique courant affectant le comportement de mouvement.
    /// </summary>
    public EmotionalState CurrentEmotion { get; set; }

    /// <summary>
    /// Coordonnée X sur la carte virtuelle (ex: 0.0 à 1000.0).
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Coordonnée Y sur la carte virtuelle.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Couleur pré-calculée pour le rendu.
    /// Optimisation : Évite de recalculer la couleur dans la boucle de rendu.
    /// </summary>
    public SKColor RenderColor { get; set; } = SKColors.Gray;

    // Constructeur par défaut
    public AgentModel()
    {
        OpinionVector = new float[1]; // 5 Dimensions d'opinion par défaut
        CurrentEmotion = EmotionalState.Neutral;
    }
}

public enum EmotionalState
{
    Neutral,    // Calme
    Agitated,   // En mouvement erratique
    Happy,      // Adhésion positive (Vert)
    Angry,      // Rejet (Rouge)
    Fearful     // Fuite
}