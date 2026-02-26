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

    // ========== PROPRIÉTÉS DE POSITION ==========

    /// <summary>
    /// Coordonnée X sur la carte virtuelle (ex: 0.0 à 1000.0).
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Coordonnée Y sur la carte virtuelle.
    /// </summary>
    public float Y { get; set; }

    // ========== PROPRIÉTÉS DE MOUVEMENT ==========

    /// <summary>
    /// Vitesse de déplacement horizontale (pixels par frame).
    /// Utilisée pour le Random Walk.
    /// </summary>
    public float VelocityX { get; set; }

    /// <summary>
    /// Vitesse de déplacement verticale (pixels par frame).
    /// Utilisée pour le Random Walk.
    /// </summary>
    public float VelocityY { get; set; }

    /// <summary>
    /// Vitesse maximale autorisée (pour éviter les mouvements trop rapides).
    /// Valeur par défaut : 2.0f pixels/frame.
    /// </summary>
    public float MaxSpeed { get; set; } = 2.0f;

    /// <summary>
    /// Angle de direction en radians (0 = droite, π/2 = haut, π = gauche, 3π/2 = bas).
    /// </summary>
    public float Direction { get; set; }

    // ========== PROPRIÉTÉS D'APPARTENANCE ==========

    /// <summary>
    /// Groupe d'appartenance de l'agent (ex: "A", "B", "Neutre").
    /// Utilisé pour la coloration et les interactions.
    /// </summary>
    public string Group { get; set; } = "Neutre";

    /// <summary>
    /// Indique si l'agent a été influencé par un message.
    /// </summary>
    public bool IsInfluenced { get; set; } = false;

    /// <summary>
    /// Timestamp de la dernière influence (pour détecter les changements récents).
    /// </summary>
    public DateTime? LastInfluenceTime { get; set; }

    // ========== PROPRIÉTÉS DE RENDU ==========

    /// <summary>
    /// Couleur pré-calculée pour le rendu.
    /// Optimisation : Évite de recalculer la couleur dans la boucle de rendu.
    /// </summary>
    public SKColor RenderColor { get; set; } = SKColors.Gray;

    // Constructeur par défaut
    public AgentModel()
    {
        OpinionVector = new float[5]; // 5 Dimensions d'opinion par défaut
        CurrentEmotion = EmotionalState.Neutral;
        VelocityX = 0f;
        VelocityY = 0f;
        Direction = 0f;
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