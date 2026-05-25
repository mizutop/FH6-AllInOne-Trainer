# FH6 All-in-One Trainer

An all-in-one trainer for **Forza Horizon 6** — runtime hooks for player profile values + car/physics cheats + live SQL access to the game's in-memory database. Self-contained `.exe`, no .NET install needed.

> **Offline mode only.** This trainer modifies game memory. Online play (Rivals, Eventlab, Multiplayer, leaderboards) will not work and may result in a ban. Run FH6 in offline mode before using.

## Download

Latest release: **[GitHub Releases](../../releases)** — download the `.zip`, extract, and run `FH6AllInOneTrainer.exe` as Administrator.

## Features

### Quick Actions
- **Quick Start** — 999M Credits + Free Cars + Autoshow Unlock + Install Flags + All Cars
- **Max All** — max Credits, Wheelspins, Super Wheelspins, Skill Points

### Profile Values (code cave detours)
- Credits, Wheelspins, Super Wheelspins, Skill Points, Sell Payout
- Uses inline code cave hooks with toggle+value slots — the same approach used by ForzaMods AIO, Vynxly/FH6-Booster, and other trainers

### Racing & World
- Freeze AI, Teleport, Gravity, No Water Drag, Time of Day, Acceleration, Free Clothing

### Scoring & Rewards
- Drift Score x, Skill Score x, Prize Scale, Speed Trap x

### Timers
- Race Time Scale, Mission Time Scale, Remove Build Cap

### SQL Database (in-memory SQLite)
- **Unlock Everything** — all SQL cheats in one click
- Free Cars (LOCK), Autoshow (LOCK), Install Flags (LOCK)
- Add All Cars, Free Upgrades (47 tables), Free Wheels, Full Autoshow
- Unlock Upgrade Presets
- Clear "NEW!" Tag

### Physics & Performance (SQL)
- Drift Score 10x, Max Traction, Torque 2x, Reduce Drag

## Anti-Cheat Bypass

- CRC bypass with 5s heartbeat + jitter (XXH check pointer replacement)
- Value Encryption bypass (RET at encryption prologue)
- 5 integrity check patches
- Thread-safe patching with ExpectedOriginal sanity check
- Pre-resolution: all hook targets are scanned before any hooks are installed

## Signature System

- Primary + AltSignatures with progressive fallback (longest to shortest)
- Context-aware validation (permission check pattern must exist near match)
- Cross-cheat address deduplication
- Struct offset extraction and logging

## Build from Source

Requires **.NET 10 SDK** on Windows:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Credits

| Who | Contribution |
|-----|-------------|
| **[paris' club](https://discord.gg/WSd3bRNJuJ)** | Core cheats, SQL features, CRC bypass |
| **[ForzaMods](https://github.com/ForzaMods/Forza-Mods-AIO)** | AOB signatures for hook-based cheats |
| **[Omkmakwana](https://github.com/Omkmakwana/FH6Trainer)** | NOP-sled approach, Add All Cars |
| **[matkhl](https://www.unknowncheats.me/forum/other-games/752793)** | Free Upgrades SQL (47 tables), database dumper |
| **[Chaarkor](https://github.com/Chaarkoor)** | Original Avalonia UI shell, MVVM architecture |
| **[changcheng967](https://github.com/changcheng967)** | All-in-one improvements, physics SQL cheats, code cave detours |

## License

GPL-3.0 — see [LICENSE](LICENSE).
