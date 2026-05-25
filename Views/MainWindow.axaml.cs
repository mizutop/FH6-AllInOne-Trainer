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
    private TrayIcon? _trayIcon;

    public static bool AllowExit;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => HookGameStatus();
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
            catch { }
        }
        catch { }
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
    }
}
