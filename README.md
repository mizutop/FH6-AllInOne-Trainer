# FH6 All-in-One Trainer

An all-in-one trainer for **Forza Horizon 6** — runtime hooks for player profile values + car/physics cheats + live SQL access to the game's in-memory database. Single-file `.exe`, no extra runtime needed.

> ⚠️ **Use at your own risk.** This trainer modifies game memory. Microsoft / Turn 10 can ban your account. **Solo / Free Roam only — never use online (Rivals, Eventlab, Multiplayer, leaderboards).**

## ⬇️ Download

Latest release: **[GitHub Releases](../../releases/latest)** — grab `FH6AllInOneTrainer.exe`. Run as administrator.

## ✅ Working Features

### Quick Actions (Unlocks Page)
- **Quick Start** — 999M Credits + Free Cars + Autoshow Unlock + All Cars in Garage, one click
- **Max All** — set Credits 999M, Wheelspins 999, Super Wheelspins 999, Skill Points 999K in one click

### Profile Values (Unlocks Page)
- **Credits (CR)** — custom value with presets (10K, 100K, 1M, 100M, 999M), locked
- **Wheelspins** — custom count with presets (10, 50, 100, 999)
- **Super Wheelspins** — custom count with presets (10, 50, 100, 999)
- **Skill Points** — custom value with presets (100, 1K, 10K, 999K), locked
- **Sell Payout x** — multiply car sell price by any factor

### Car & Physics (Unlocks Page)
- **Freeze AI** — stops all AI Drivatar cars during races (zeroes their velocity)
- **Teleport to Waypoint** — instantly teleport to any map waypoint
- **No Clip** — disable collision detection, drive through walls and terrain
- **Gravity Multiplier** — adjust gravity (low gravity, moon gravity, etc.)
- **No Water Drag** — remove water resistance when driving through lakes/rivers
- **Remove Build Cap** — remove engine swap / build power limit
- **Acceleration Override** — boost car acceleration with custom multiplier
- **Free Clothing** — set all clothing prices to 0

### World & Events (Unlocks Page)
- **Time of Day** — set any hour (6 = dawn, 12 = noon, 18 = dusk, 0 = midnight)
- **Skill Score Multiplier** — multiply skill chain score earned (5x, 10x, 100x)
- **Prize Scale** — multiply wheelspin reward value (5x, 10x, 50x)
- **Race Time Scale** — slow down or speed up race timer (0 = freeze timer)
- **Mission Time Scale** — slow down or speed up mission timer (0 = freeze timer)
- **Speed Zone Multiplier** — multiply speed zone score (5x, 10x, 100x)
- **Speed Trap Multiplier** — multiply speed trap score (5x, 10x, 100x)

### SQL Database (Database Page)
- **Unlock Everything** — applies all 9 SQL cheats at once (one click)
- **Free Cars (LOCK)** — BaseCost stays at 0 forever (re-applied every 10s)
- **Autoshow All Visible (LOCK)** — every car stays in showroom
- **Install Flags (LOCK)** — IsInstalled / IsPurchased / IsDrivable stay at 1
- **Clear NEW Tag** — remove persistent NEW! badges from garage
- **Add All Cars** — grant every car free (reopen game to claim)
- **Free Upgrades** — set price=0 on all 47 upgrade tables (engine, turbo, brakes, body kits, rims, etc.)
- **Free Wheels** — set price=1 (free) on all wheels
- **Unlock Upgrade Presets** — reveal hidden upgrade preset tunes
- **Full Autoshow** — recreate Drivable_Data_Car view + fill CarBuckets for complete autoshow

Each LOCK toggle re-applies its SQL every 10 seconds. Backup tables are created automatically — toggling OFF restores originals.

### Settings & Diagnostics
- **Accent color picker** — 8 palettes, applies instantly everywhere
- **UI animations** — toggle stagger entrance, hover lift, pulse, transitions
- **Mouse hover glow** — soft radial gradient that follows cursor (disable on weak GPUs)
- **Signature scan** — test all AOB patterns against FH6 binary without installing hooks
- **Conflict detection** — warns if another trainer (ForzaMods AIO, WeMod, etc.) is running
- **Profile system** — save/load named cheat configurations to JSON

## 🛡️ Safety Features

- **Toggle locks** — all cheat toggles are disabled when FH6 is not running; red warning banner shown
- **System tray** — closing the window minimizes to tray instead of quitting; right-click for Show/Exit
- **Conflict detection** — warns if another trainer (ForzaMods AIO, WeMod, etc.) is already running
- **Signature scan mode** — test all AOB patterns against current FH6 binary without installing hooks
- **Profile system** — save and load cheat configurations to named JSON profiles

## 🛡️ Stability & Anti-Detection

- **5 integrity check bypasses** — the game has 5 independent anti-cheat mechanisms (TextSection hash, PageHash, MemCmp, CodeSection verify, Checksum verify). The trainer patches all 5 to always return "pass" during the patched window
- **CRC bypass** auto-armed before any hook (vtable function pointer swap + 5s re-arm timer with random jitter)
- **Dedicated RET stub** — the CRC bypass allocates its own `RET` instruction in cave memory instead of scavenging a random byte from the game binary
- **Thread-safe patching** — all FH6 threads are suspended during both phases of the CRC heartbeat dance, preventing race conditions
- **Randomized heartbeat** — 5s base interval with ±1.5s jitter prevents the game's integrity check from syncing with our timer
- **Hook self-healing** — every cycle the engine restores originals for 2s (game checks pass), then re-applies patches
- **ExpectedOriginal sanity check** — refuses to inject if target bytes don't match (no crashes from outdated signatures)
- **Auto-detach** when the game exits or crashes — no writes to dead processes

## 🔧 Build from Source

Requires **.NET 10 SDK** on Windows:

```bash
dotnet publish -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

Output: `bin/Release/net10.0-windows/win-x64/publish/FH6AllInOneTrainer.exe`

## 🙏 Credits

| Who | Contribution |
|-----|-------------|
| **[paris' club](https://discord.gg/WSd3bRNJuJ)** | Core cheats: runtime hooks (Credits, Wheelspins, Skill Points, Sell Payout), SQL features (Free Cars, Autoshow, Install Flags, Add All Cars, Clear NEW Tag), CRC bypass, code caves, memory injection |
| **[ForzaMods](https://github.com/ForzaMods/Forza-Mods-AIO)** | AOB signatures for all 22 hook-based cheats — Freeze AI, Teleport, No Clip, Gravity, Acceleration, Speed Zone/Trap Multipliers, Mission Time Scale, Free Clothing, and more |
| **[matkhl](https://github.com/matkhl)** | Free Upgrades SQL (47 upgrade tables), Free Wheels, Upgrade Presets, CarBuckets autoshow — [FH6-DBDUMPER](https://github.com/matkhl/FH6-DBDUMPER) |
| **[Chaarkor](https://github.com/Chaarkoor)** | Original Avalonia UI shell, MVVM architecture, design system, pattern scanner — [Chaarkors-FH6-Trainer](https://github.com/Chaarkoor/Chaarkors-FH6-Trainer) |
| **[Reloaded.Memory](https://github.com/Reloaded-Project/Reloaded.Memory.Sigscan)** | SIMD-accelerated AOB scanner |
| **[changcheng967](https://github.com/changcheng967)** | All-in-one improvements: Quick Start, Max All, Unlock Everything, 22 runtime hooks, 9 SQL features, system tray, safety locks, profile system, UI redesign, rebrand |

## 📝 License

GPL-3.0 — source must remain open. See [LICENSE](LICENSE).

---

**FH6 All-in-One Trainer** · v4.3.0 · 2026 · GPL-3.0 · Solo / Free Roam only
