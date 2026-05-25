using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace FH6Mod.Cheats.RuntimeHook;

/// <summary>
/// Resolves profile struct by capturing the pointer from the Credits setter function.
///
/// The Credits setter (found via AOB) is: mov [rbx+0x1D4], eax; ret
/// When called, rbx = profile struct base. We install a code cave that:
///   1. Saves the struct base (rbx) to a shared slot
///   2. If toggle is on: writes desired value to [rbx+0x1D4], skips original
///   3. If toggle is off: executes original instruction normally
///
/// Once the struct base is captured, we can write to any profile field directly.
/// Profile field offsets (confirmed via Ghidra):
///   Credits@0x1D4, SkillPoints@0x1DC, Wheelspins@0x1F8, SuperWheelspins@0x1FC
/// </summary>
internal sealed class FnvProfileResolver : IDisposable
{
    private readonly RuntimeHookEngine _engine;
    private readonly Dictionary<RuntimeProfileFeature, ResolvedField> _fields = new();
    private ulong _structBase;
    private bool _structBaseResolved;
    private Timer? _lockTimer;
    private int _timerRunning;
    private static readonly Random _jitter = new();
    private bool _disposed;

    // Profile field offsets (confirmed via Ghidra decompilation)
    private const int CreditsOffset = 0x1D4;
    private const int SkillPointsOffset = 0x1DC;
    private const int WheelspinsOffset = 0x1F8;
    private const int SuperWheelspinsOffset = 0x1FC;

    internal sealed class ResolvedField
    {
        public int StructOffset;
        public int DesiredValue;
        public bool Active;
    }

    public FnvProfileResolver(RuntimeHookEngine engine)
    {
        _engine = engine;
    }

    public ulong StructBase => _structBase;
    public bool IsResolved => _structBaseResolved;

    /// <summary>
    /// Captures the profile struct base by reading the rbx value that the Credits
    /// setter function uses. The NOP-sled hook already intercepts this function,
    /// and the original instruction is `mov [rbx+0x1D4], eax`. We read rbx from
    /// the thread context when the function is about to be called.
    ///
    /// Simpler approach: just scan for the profile struct by looking for a known
    /// pattern of the current profile values at the known offsets.
    /// </summary>
    public bool ResolveStructBase(byte[] moduleBytes)
    {
        if (_structBaseResolved) return true;

        _engine.LogPublic("FNV: Starting struct base resolution...");

        RegisterField(RuntimeProfileFeature.Credits, CreditsOffset);
        RegisterField(RuntimeProfileFeature.SkillPoints, SkillPointsOffset);
        RegisterField(RuntimeProfileFeature.Wheelspins, WheelspinsOffset);
        RegisterField(RuntimeProfileFeature.SuperWheelspins, SuperWheelspinsOffset);

        // Approach: scan heap for any struct where offsets 0x1D4, 0x1DC, 0x1F8, 0x1FC
        // all contain reasonable int32 values (>0, <2B) and at least 2 are non-zero.
        // This is much simpler than marker scanning and works across all builds.
        var matches = ScanForProfileStruct();
        if (matches.Count == 0)
        {
            _engine.LogPublic("FNV: No profile struct candidates found");
            return false;
        }

        _engine.LogPublic($"FNV: Found {matches.Count} profile struct candidate(s):");
        for (var i = 0; i < Math.Min(matches.Count, 10); i++)
        {
            var m = matches[i];
            _engine.LogPublic($"FNV:   [{i}] 0x{m.Ptr:X} C={m.Credits} SP={m.SP} WS={m.WS} SWS={m.SWS}");
        }

        // Pick the struct with the most non-zero fields
        var best = matches[0];
        var bestScore = 0;
        foreach (var m in matches)
        {
            var score = (m.Credits > 0 ? 1 : 0) + (m.SP > 0 ? 1 : 0) + (m.WS > 0 ? 1 : 0) + (m.SWS > 0 ? 1 : 0);
            if (score > bestScore) { bestScore = score; best = m; }
        }

        _structBase = best.Ptr;
        _structBaseResolved = true;
        _engine.LogPublic($"FNV: Selected 0x{best.Ptr:X} (C={best.Credits}, SP={best.SP}, WS={best.WS}, SWS={best.SWS})");
        return true;
    }

    /// <summary>
    /// Scans committed heap for structs matching the profile field layout.
    /// Valid candidate: all 4 fields are reasonable int32 (>0 or small negative, <2B)
    /// and at least 2 are non-zero.
    /// </summary>
    private List<(ulong Ptr, int Credits, int SP, int WS, int SWS)> ScanForProfileStruct()
    {
        var handle = _engine.HandlePublic;
        if (handle == IntPtr.Zero) return [];

        var matches = new List<(ulong Ptr, int Credits, int SP, int WS, int SWS)>();
        var mbiSize = (UIntPtr)(ulong)Marshal.SizeOf<Native.MemoryBasicInformation64>();
        ulong addr = 0x1_0000_0000UL;
        int regionsScanned = 0;

        const int minStructSize = SuperWheelspinsOffset + 4;

        while (true)
        {
            if (Native.VirtualQueryEx(handle, (UIntPtr)addr, out var mbi, mbiSize) == UIntPtr.Zero)
                break;
            addr = mbi.BaseAddress + mbi.RegionSize;

            if (mbi.State != Native.MEM_COMMIT) continue;
            if (!Native.IsReadable(mbi.Protect)) continue;
            if ((mbi.Protect & Native.PAGE_GUARD) != 0) continue;
            if ((mbi.Protect & (Native.PAGE_EXECUTE_READ | Native.PAGE_EXECUTE_READWRITE)) != 0) continue;

            var regionSize = (long)mbi.RegionSize;
            if (regionSize < 1024 || regionSize > 512 * 1024 * 1024) continue;

            regionsScanned++;
            const int chunkSize = 4 * 1024 * 1024;
            var regionBase = mbi.BaseAddress;
            var regionRemaining = regionSize;

            while (regionRemaining > 0)
            {
                var toRead = (int)Math.Min(chunkSize, regionRemaining);
                var buf = _engine.ReadBytesPublic(regionBase, toRead);
                if (buf.Length < toRead) break;

                var scanLimit = buf.Length - minStructSize;
                if (scanLimit <= 0) { regionBase += (ulong)toRead; regionRemaining -= toRead; continue; }

                for (var i = 0; i < scanLimit; i += 8)
                {
                    var credits = BitConverter.ToInt32(buf, i + CreditsOffset);
                    var sp = BitConverter.ToInt32(buf, i + SkillPointsOffset);
                    var ws = BitConverter.ToInt32(buf, i + WheelspinsOffset);
                    var sws = BitConverter.ToInt32(buf, i + SuperWheelspinsOffset);

                    // All must be reasonable int32 values (0 to 999M)
                    if (!IsReasonable(credits) || !IsReasonable(sp) ||
                        !IsReasonable(ws) || !IsReasonable(sws))
                        continue;

                    // At least 2 non-zero fields (active profile)
                    var nonZero = (credits > 0 ? 1 : 0) + (sp > 0 ? 1 : 0) +
                                  (ws > 0 ? 1 : 0) + (sws > 0 ? 1 : 0);
                    if (nonZero < 2) continue;

                    // Additional validation: check that the struct has a valid vtable pointer at offset 0
                    var vtable = BitConverter.ToUInt64(buf, i);
                    if (vtable < 0x7FF0_0000_0000_0000UL || vtable > 0x7FFF_FFFF_FFFF_FFFF)
                        continue;

                    matches.Add((regionBase + (ulong)i, credits, sp, ws, sws));

                    // Cap matches to avoid excessive memory
                    if (matches.Count >= 50) goto done;
                }

                regionBase += (ulong)toRead;
                regionRemaining -= toRead;
            }
        }
        done:

        _engine.LogPublic($"FNV: Heap scan: {matches.Count} candidate(s), {regionsScanned} regions");
        return matches;
    }

    private static bool IsReasonable(int val) => val >= 0 && val <= 999_999_999;

    // ===== Field resolution =====

    private void RegisterField(RuntimeProfileFeature feature, int offset)
    {
        if (!_fields.ContainsKey(feature))
            _fields[feature] = new ResolvedField { StructOffset = offset };
    }

    public bool ResolveField(RuntimeProfileFeature feature, byte[] moduleBytes)
    {
        if (_fields.ContainsKey(feature)) return true;

        var offset = feature switch
        {
            RuntimeProfileFeature.Credits => CreditsOffset,
            RuntimeProfileFeature.SkillPoints => SkillPointsOffset,
            RuntimeProfileFeature.Wheelspins => WheelspinsOffset,
            RuntimeProfileFeature.SuperWheelspins => SuperWheelspinsOffset,
            _ => -1
        };

        if (offset < 0) return false;
        RegisterField(feature, offset);
        return true;
    }

    public bool TryResolve(RuntimeProfileFeature feature, byte[] moduleBytes)
    {
        if (!_structBaseResolved && !ResolveStructBase(moduleBytes)) return false;
        return ResolveField(feature, moduleBytes);
    }

    // ===== Read/Write =====

    public int ReadValue(RuntimeProfileFeature feature)
    {
        if (!_fields.TryGetValue(feature, out var field) || !_structBaseResolved)
            throw new InvalidOperationException($"{feature} not resolved");
        return _engine.ReadInt32Public(_structBase + (ulong)field.StructOffset);
    }

    public void WriteValue(RuntimeProfileFeature feature, int value)
    {
        if (!_fields.TryGetValue(feature, out var field) || !_structBaseResolved)
            throw new InvalidOperationException($"{feature} not resolved");
        _engine.WriteInt32Public(_structBase + (ulong)field.StructOffset, value);
    }

    // ===== Value Lock =====

    public void StartLock(RuntimeProfileFeature feature, int value, int periodMs = 5000)
    {
        if (!_fields.TryGetValue(feature, out var field)) return;
        field.DesiredValue = value;
        field.Active = true;

        try
        {
            var addr = _structBase + (ulong)field.StructOffset;
            _engine.WriteInt32Public(addr, value);
            _engine.LogPublic($"FNV: {feature} locked at {value} (0x{addr:X})");
        }
        catch (Exception ex)
        {
            _engine.LogPublic($"FNV: {feature} write failed: {ex.Message}");
        }

        EnsureTimerStarted(periodMs);
    }

    public void StopLock(RuntimeProfileFeature feature)
    {
        if (_fields.TryGetValue(feature, out var field))
            field.Active = false;
        foreach (var f in _fields.Values)
            if (f.Active) return;
        StopTimer();
    }

    public bool IsFieldActive(RuntimeProfileFeature feature)
        => _fields.TryGetValue(feature, out var f) && f.Active;

    // ===== Timer =====

    private void EnsureTimerStarted(int periodMs)
    {
        if (_lockTimer != null) return;
        _lockTimer = new Timer(LockTimerTick, null, 1000, Timeout.Infinite);
    }

    private void StopTimer()
    {
        var t = _lockTimer;
        _lockTimer = null;
        try { t?.Dispose(); } catch { }
    }

    private void LockTimerTick(object? _)
    {
        if (Interlocked.Exchange(ref _timerRunning, 1) == 1) return;
        try
        {
            if (!_engine.IsAttached || !_structBaseResolved) return;

            foreach (var kvp in _fields)
            {
                if (!kvp.Value.Active) continue;
                try
                {
                    var addr = _structBase + (ulong)kvp.Value.StructOffset;
                    var current = _engine.ReadInt32Public(addr);
                    if (current != kvp.Value.DesiredValue)
                        _engine.WriteInt32Public(addr, kvp.Value.DesiredValue);
                }
                catch { }
            }
        }
        catch { }
        finally
        {
            Interlocked.Exchange(ref _timerRunning, 0);
            if (!_disposed)
            {
                try
                {
                    var nextMs = 5000 + _jitter.Next(-1000, 1001);
                    _lockTimer?.Change(nextMs, Timeout.Infinite);
                }
                catch { }
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        StopTimer();
    }
}
