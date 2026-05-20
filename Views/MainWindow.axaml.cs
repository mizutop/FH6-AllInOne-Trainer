using System;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using FH6Mod.Services;
using FH6Mod.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FH6Mod.Views;

public partial class MainWindow : Window
{
    private bool _updateDialogShown;
    private RadialGradientBrush? _glowBrush;
    private Border? _glowBorder;
    private TrayIcon? _trayIcon;

    public static bool AllowExit;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => HookGameStatus();
        Opened += OnWindowOpened;
        InitGlow();
        InitTray();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!AllowExit && _trayIcon is not null)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        _trayIcon?.Dispose();
        base.OnClosing(e);
    }

    private void InitTray()
    {
        try
        {
            var showCmd = new RelayCommand(() => Dispatcher.UIThread.Post(() =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }));
            var exitCmd = new RelayCommand(() =>
            {
                AllowExit = true;
                Dispatcher.UIThread.Post(() => Close());
            });

            _trayIcon = new TrayIcon
            {
                ToolTipText = "FH6 All-in-One Trainer",
                Menu = CreateTrayMenu(showCmd, exitCmd),
                IsVisible = true,
            };
            try { _trayIcon.Icon = new WindowIcon("/Assets/logo.png"); }
            catch { /* icon format not supported — tray still works without icon */ }
        }
        catch { /* tray not available — window still works normally */ }
    }

    private NativeMenu CreateTrayMenu(ICommand show, ICommand exit)
    {
        var menu = new NativeMenu();
        menu.Add(new NativeMenuItem("Show") { Command = show });
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(new NativeMenuItem("Exit") { Command = exit });
        return menu;
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action _action;
        public RelayCommand(Action a) => _action = a;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? p) => true;
        public void Execute(object? p) => _action();
    }

    private void OnWindowOpened(object? sender, System.EventArgs e)
    {
        var updater = App.Services.GetRequiredService<UpdateCheckService>();
        updater.StateChanged += TryShowUpdateDialog;
        // In case the check already completed before the window opened
        TryShowUpdateDialog();

        // Logo fade-in animation
        var logo = this.FindControl<Border>("LogoBox");
        if (logo is not null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                logo.Opacity = 1.0;
                logo.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Parse("scale(1)");
            }, DispatcherPriority.Background);
        }
    }

    private void TryShowUpdateDialog()
    {
        if (_updateDialogShown) return;
        var updater = App.Services.GetRequiredService<UpdateCheckService>();
        if (!updater.IsUpdateAvailable || updater.LatestTag is null) return;

        _updateDialogShown = true;
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var dlg = new UpdateDialog(
                    updater.LatestTag!,
                    updater.CurrentVersion.ToString(3),
                    UpdateCheckService.ReleasesUrl);
                await dlg.ShowDialog(this);
            }
            catch { /* never break the main UI for an update prompt */ }
        });
    }

    private void HookGameStatus()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.PropertyChanged += OnVmPropertyChanged;
        UpdateStatusDot(vm.IsGameAttached);
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainWindowViewModel.IsGameAttached)) return;
        if (DataContext is MainWindowViewModel vm)
            Dispatcher.UIThread.Post(() => UpdateStatusDot(vm.IsGameAttached));
    }

    private void UpdateStatusDot(bool attached)
    {
        var dot = this.FindControl<Ellipse>("StatusDot");
        if (dot is null) return;
        var key = attached ? "StatusOk" : "StatusErr";
        if (Application.Current?.Resources[key] is IBrush brush)
            dot.Fill = brush;
        // Toggle 'alive' class to trigger the pulse animation when attached
        dot.Classes.Set("alive", attached);
    }

    // ============================================================
    //  Ambient accent glow — a large, soft radial gradient that is
    //  *always* painted in the content area. The cursor only nudges
    //  the gradient origin, so moving the mouse subtly redistributes
    //  the wash rather than acting like a flashlight.
    // ============================================================

    private void InitGlow()
    {
        _glowBorder = this.FindControl<Border>("MouseGlow");
        if (_glowBorder is null) return;

        _glowBrush = new RadialGradientBrush
        {
            // Geometric center of the gradient ellipse stays anchored to the middle
            // of the content area — that's what gives us the "always visible" wash.
            Center         = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            // Gradient origin starts at the same spot; pointer moves shift only this,
            // creating an asymmetric "bulge" of accent colour towards the cursor.
            GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            // Big and soft so the falloff is gentle — no hard spotlight edges.
            RadiusX        = new RelativeScalar(0.8, RelativeUnit.Relative),
            RadiusY        = new RelativeScalar(1.0, RelativeUnit.Relative),
            GradientStops  =
            {
                new GradientStop(GetAccentGlowColor(), 0),
                new GradientStop(Colors.Transparent,    1),
            },
        };
        _glowBorder.Background = _glowBrush;

        ApplyGlowVisibility();
        PointerMoved += OnPointerMovedForGlow;
        App.AccentChanged += OnAccentChangedForGlow;
        AppSettings.Current.Changed += ApplyGlowVisibility;
    }

    private static Color GetAccentGlowColor()
    {
        // Very soft tint — ~7% alpha. Reads as ambient warmth on the background,
        // never a flashlight; cards (opaque) keep their own look unaffected.
        var c = Color.FromRgb(0xFF, 0x6A, 0x1F);
        if (Application.Current?.Resources["AccentBrush"] is ISolidColorBrush sb)
            c = sb.Color;
        return Color.FromArgb(18, c.R, c.G, c.B);
    }

    private void OnAccentChangedForGlow(Accent _)
    {
        if (_glowBrush is null || _glowBrush.GradientStops.Count == 0) return;
        _glowBrush.GradientStops[0] = new GradientStop(GetAccentGlowColor(), 0);
    }

    private void OnPointerMovedForGlow(object? sender, PointerEventArgs e)
    {
        if (_glowBrush is null || _glowBorder is null) return;
        var pos = e.GetPosition(_glowBorder);

        // Only nudge the gradient origin (not the center). Result: the gradient
        // ellipse stays put while its bright spot leans toward the cursor, like
        // light catching the surface from a different angle.
        _glowBrush.GradientOrigin = new RelativePoint(pos.X, pos.Y, RelativeUnit.Absolute);
    }

    private void ApplyGlowVisibility()
    {
        if (_glowBorder is null) return;
        _glowBorder.IsVisible = AppSettings.Current.MouseGlowEnabled;
    }
}
