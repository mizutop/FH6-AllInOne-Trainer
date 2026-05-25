using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FH6Mod.Services;
using FH6Mod.Cheats.RuntimeHook;
using FH6Mod.Cheats.Sql;
using Material.Icons;

namespace FH6Mod.ViewModels.Pages;

public partial class AccentItemVm : ObservableObject
{
    public Accent Inner { get; }
    public string Name => Inner.Name;
    public string Base => Inner.Base;
    [ObservableProperty] private bool _isSelected;
    public AccentItemVm(Accent a, bool selected) { Inner = a; IsSelected = selected; }
}

public partial class SettingsViewModel : PageViewModelBase
{
    private readonly CheatService? _cheat;
    private readonly GameProcessService? _game;
    private readonly ProfileService? _profiles;

    public override string PageTitle => "Settings";
    public override string PageSubtitle => "Animations, diagnostics, profiles, about & credits.";
    public override MaterialIconKind PageIcon => MaterialIconKind.CogOutline;

    public string VersionText => $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?.?.?"}";

    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private bool _mouseGlowEnabled;
    [ObservableProperty] private string _selectedAccentName = AppSettings.Current.AccentName;

    // Scan results
    [ObservableProperty] private string _scanStatus = "";
    [ObservableProperty] private bool _isScanning;

    // Conflict detection
    [ObservableProperty] private string _conflictStatus = "";
    [ObservableProperty] private bool _hasConflicts;

    // Profile management
    [ObservableProperty] private string _profileName = "";
    public ObservableCollection<string> SavedProfiles { get; } = [];
    [ObservableProperty] private string? _selectedProfile;
    [ObservableProperty] private string _profileStatus = "";

    public IReadOnlyList<AccentItemVm> AccentOptions { get; }

    public SettingsViewModel() : this(null, null, null) { }

    public SettingsViewModel(CheatService? cheat, GameProcessService? game, ProfileService? profiles)
    {
        _cheat = cheat;
        _game = game;
        _profiles = profiles;
        AnimationsEnabled = AppSettings.Current.AnimationsEnabled;
        MouseGlowEnabled  = AppSettings.Current.MouseGlowEnabled;
        AccentOptions = AccentPalette.All
            .Select(a => new AccentItemVm(a, a.Name == SelectedAccentName))
            .ToList();
        RefreshProfiles();
    }

    partial void OnAnimationsEnabledChanged(bool value)
    {
        AppSettings.Current.AnimationsEnabled = value;
        AppSettings.Current.NotifyChanged();
    }

    partial void OnMouseGlowEnabledChanged(bool value)
    {
        AppSettings.Current.MouseGlowEnabled = value;
        AppSettings.Current.NotifyChanged();
    }

    public void SelectAccent(AccentItemVm? item)
    {
        if (item is null || item.IsSelected) return;
        foreach (var x in AccentOptions) x.IsSelected = (x == item);
        SelectedAccentName = item.Name;
        AppSettings.Current.AccentName = item.Name;
        AppSettings.Current.NotifyChanged();
        App.ApplyAccent(item.Inner);
    }

    public string SettingsPath => AppSettings.SettingsPath;

    public override IReadOnlyList<FeatureRow> Features { get; } = [];

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        try
        {
            Directory.CreateDirectory(AppSettings.SettingsDir);
            Process.Start(new ProcessStartInfo
            {
                FileName        = "explorer.exe",
                Arguments       = $"\"{AppSettings.SettingsDir}\"",
                UseShellExecute = true,
            });
        }
        catch { }
    }

    // === Signature Scan ===

    [RelayCommand]
    private void ScanSignatures()
    {
        if (_cheat is null) { ScanStatus = "Service not available."; return; }
        IsScanning = true;
        ScanStatus = "Scanning...";

        System.Threading.Tasks.Task.Run(() =>
        {
            var results = _cheat.ScanAllSignatures();
            var found = results.Count(r => r.Found);
            var lines = results.Select(r =>
                $"{(r.Found ? "OK" : "MISS")}  {r.Feature}  —  {r.Detail}");
            return $"Found {found}/{results.Count} signatures.\n\n{string.Join('\n', lines)}";
        }).ContinueWith(t =>
        {
            IsScanning = false;
            ScanStatus = t.IsFaulted ? $"Error: {t.Exception?.InnerException?.Message}" : t.Result;
        });
    }

    // === Conflict Detection ===

    [RelayCommand]
    private void DetectConflicts()
    {
        if (_game is null) { ConflictStatus = "Service not available."; return; }
        var conflicts = _game.DetectConflictingTrainers();
        HasConflicts = conflicts.Count > 0;
        ConflictStatus = conflicts.Count > 0
            ? $"WARNING — Conflicting trainers detected:\n{string.Join('\n', conflicts)}"
            : "No conflicting trainers detected.";
    }

    // === Profile Management ===

    [RelayCommand]
    private void SaveProfile()
    {
        if (_profiles is null) { ProfileStatus = "Service not available."; return; }
        var name = ProfileName.Trim();
        if (string.IsNullOrWhiteSpace(name)) { ProfileStatus = "Enter a profile name first."; return; }

        var profile = new CheatProfile();
        // Capture current cheat states from CheatService if available
        if (_cheat is not null)
        {
            foreach (RuntimeProfileFeature f in Enum.GetValues<RuntimeProfileFeature>())
            {
                if (_cheat.IsActive(f))
                    profile.Features[f] = new CheatState { Enabled = true };
            }
            foreach (SqlFeature f in Enum.GetValues<SqlFeature>())
            {
                if (_cheat.IsSqlLockActive(f))
                    profile.SqlLocks[f] = true;
            }
        }

        _profiles.Save(name, profile);
        ProfileStatus = $"Profile \"{name}\" saved.";
        ProfileName = "";
        RefreshProfiles();
    }

    [RelayCommand]
    private void LoadProfile()
    {
        if (_profiles is null || SelectedProfile is null) { ProfileStatus = "Select a profile first."; return; }
        var profile = _profiles.Load(SelectedProfile);
        if (profile is null) { ProfileStatus = "Failed to load profile."; return; }
        ProfileStatus = $"Profile \"{SelectedProfile}\" loaded. Apply cheats from the Unlocks/Database pages.";
    }

    [RelayCommand]
    private void DeleteProfile()
    {
        if (_profiles is null || SelectedProfile is null) { ProfileStatus = "Select a profile first."; return; }
        _profiles.Delete(SelectedProfile);
        ProfileStatus = $"Profile \"{SelectedProfile}\" deleted.";
        RefreshProfiles();
    }

    [RelayCommand]
    private void OpenProfilesFolder()
    {
        try
        {
            Directory.CreateDirectory(ProfileService.ProfilesDir);
            Process.Start(new ProcessStartInfo
            {
                FileName  = "explorer.exe",
                Arguments = $"\"{ProfileService.ProfilesDir}\"",
                UseShellExecute = true,
            });
        }
        catch { }
    }

    private void RefreshProfiles()
    {
        SavedProfiles.Clear();
        if (_profiles is null) return;
        foreach (var p in _profiles.ListProfiles())
            SavedProfiles.Add(p);
    }
}
