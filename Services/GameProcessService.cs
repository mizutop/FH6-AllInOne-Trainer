using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FH6Mod.Services;

public sealed class GameProcessService : IDisposable
{
    public const string ProcessName = "ForzaHorizon6";

    // Known trainer process names to detect conflicts
    private static readonly HashSet<string> KnownTrainers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Forza-Mods-AIO", "ForzaModsAIO",
        "AutoshowUnlocker", "FH6AutoshowUnlocker",
        "flingtrainer", "WeMod", "infinitytrainer",
    };

    private readonly Timer _poll;
    private Process? _process;

    public event Action? StatusChanged;

    public bool IsAttached => _process is { HasExited: false };
    public int? Pid => IsAttached ? _process!.Id : null;
    public IntPtr BaseAddress
    {
        get
        {
            try { return IsAttached && _process!.MainModule is { } m ? m.BaseAddress : IntPtr.Zero; }
            catch { return IntPtr.Zero; }
        }
    }
    public long ModuleSize
    {
        get
        {
            try { return IsAttached && _process!.MainModule is { } m ? m.ModuleMemorySize : 0; }
            catch { return 0; }
        }
    }

    public GameProcessService()
    {
        _poll = new Timer(_ => Poll(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    private void Poll()
    {
        try
        {
            var was = IsAttached;
            if (_process is { HasExited: false })
                return;

            _process = Process.GetProcessesByName(ProcessName).FirstOrDefault();
            var nowAttached = IsAttached;
            if (was != nowAttached)
                StatusChanged?.Invoke();
        }
        catch { }
    }

    public List<string> DetectConflictingTrainers()
    {
        var conflicts = new List<string>();
        var ownPid = Environment.ProcessId;
        try
        {
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (proc.Id == ownPid) continue;
                    var name = proc.ProcessName;
                    if (KnownTrainers.Any(t => name.Contains(t, StringComparison.OrdinalIgnoreCase)))
                        conflicts.Add($"{name} (PID {proc.Id})");
                }
                catch { }
                finally { proc.Dispose(); }
            }
        }
        catch { }
        return conflicts;
    }

    public void Dispose() => _poll.Dispose();
}
