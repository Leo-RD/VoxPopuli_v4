using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoxPopuli.Client.ViewModels;

/// <summary>
/// Classe de base pour tous les ViewModels de l'application.
/// Hérite de ObservableObject pour la gestion automatique de INotifyPropertyChanged.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    // Le générateur de source va créer : public bool IsBusy { get; set; } 
    // et gérer l'événement PropertyChanged automatiquement.
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    public bool IsNotBusy => !IsBusy;
}