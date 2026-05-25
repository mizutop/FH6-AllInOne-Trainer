using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FH6Mod.Services;
using FH6Mod.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace FH6Mod.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly GameProcessService _gameProcess;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private int _selectedTab;

    [ObservableProperty]
    private string _gameStatusText = "FH6 disconnected";

    [ObservableProperty]
    private bool _isGameAttached;

    public string CurrentVersionText => $"v{App.Services.GetRequiredService<UpdateCheckService>().CurrentVersion.ToString(3)}";

    public MainWindowViewModel()
        : this(App.Services.GetRequiredService<GameProcessService>())
    {
    }

    public MainWindowViewModel(GameProcessService gameProcess)
    {
        _gameProcess = gameProcess;
        _gameProcess.StatusChanged += OnGameStatusChanged;
        OnGameStatusChanged();

        // Default to Cheats tab
        SelectedTab = 0;
    }

    partial void OnSelectedTabChanged(int value)
    {
        CurrentPage = value switch
        {
            0 => App.Services.GetRequiredService<UnlocksViewModel>(),
            1 => App.Services.GetRequiredService<DatabaseViewModel>(),
            2 => App.Services.GetRequiredService<SettingsViewModel>(),
            _ => CurrentPage
        };
    }

    [RelayCommand]
    private void SelectTab(string index) => SelectedTab = int.Parse(index);

    private void OnGameStatusChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsGameAttached = _gameProcess.IsAttached;
            GameStatusText = _gameProcess.IsAttached
                ? $"FH6 connected · PID {_gameProcess.Pid}"
                : "FH6 disconnected";
        });
    }
}
