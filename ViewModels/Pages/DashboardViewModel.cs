using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FH6Mod.Cheats.RuntimeHook;
using FH6Mod.Cheats.Sql;
using FH6Mod.Services;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace FH6Mod.ViewModels.Pages;

public sealed partial class DashboardViewModel : PageViewModelBase
{
    private readonly CheatService _cheats;

    public override string PageTitle => "Dashboard";
    public override string PageSubtitle => "Quick start, status, and cheat overview.";
    public override MaterialIconKind PageIcon => MaterialIconKind.ViewDashboardOutline;

    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private bool _statusIsError;

    public DashboardViewModel() : this(App.Services.GetRequiredService<CheatService>()) { }
    public DashboardViewModel(CheatService cheats) => _cheats = cheats;

    [RelayCommand]
    private void QuickMaxCreditsAndCars()
    {
        var cr = 999_999_999;
        var ok1 = _cheats.Apply(RuntimeProfileFeature.Credits, cr, true);
        var ok2 = _cheats.RunSql(SqlFeature.FreeCarPrices);
        var ok3 = _cheats.RunSql(SqlFeature.AutoshowUnlock);
        var ok4 = _cheats.RunSql(SqlFeature.InstallFlags);
        var ok5 = _cheats.RunSql(SqlFeature.AddAllCars);

        var allOk = ok1 && ok2 && ok3 && ok4 && ok5;
        StatusIsError = !allOk;
        StatusMessage = allOk
            ? "Quick Start applied — 999M credits, all cars free & unlocked, all visible in Autoshow."
            : "Partially applied. Check Unlocks and Database tabs for details.";
    }
}
