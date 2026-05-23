# FH6 All-in-One Trainer

An all-in-one trainer for **Forza Horizon 6** — runtime hooks for player profile values + car/physics cheats + live SQL access to the game's in-memory database. Self-contained `.exe`, no .NET install needed.

> **Offline mode only.** This trainer modifies game memory. Online play (Rivals, Eventlab, Multiplayer, leaderboards) will not work and may result in a ban. Run FH6 in offline mode before using.

## Download

Latest release: **[GitHub Releases](../../releases)** — download the `.zip`, extract, and run `FH6AllInOneTrainer.exe` as Administrator.

## Community-Tested Status

Based on community reports (Steam version). **Your results may vary by game version.**

| Feature | Status | Notes |
|---------|--------|-------|
| SQL: Free Cars, Free Upgrades, Autoshow | Working | Database writes, reliable |
| SQL: Drift 10x, Torque 2x, Grip, Drag | Untested | New in v4.7.0 |
| Credits, Wheelspins, Super Wheelspins | Working | Signatures may not match all game builds |
| Skill Points | Working | Signatures may not match all game builds |
| Freeze AI | Working | Signature verified across builds |
| Drift Score Multiplier | Working | Community confirmed |
| Teleport, Gravity, Time of Day | Working | Signatures match across builds |
| Sell Payout, Skill Score, Prize Scale | Working | Signatures match across builds |
| Acceleration, Race/Mission Time | Working | Signatures match across builds |
| Speed Trap, Remove Build Cap, Free Clothing | Working | Signatures match across builds |
| NoClip | Broken | Hooks wrong function |
| No Skill Break | Version-dependent | Signature may not match all builds |
| XP Override | Broken | No known working approach |
| Add All Cars | Partial | Only adds DLC cars (not all 721) |

If a cheat says "signature not found", your game build may differ. Use the **Signature Scan** in Settings to check compatibility.

Use the **Signature Scan** in Settings to check which cheats match your game build.

## Features

### Quick Actions
- **Quick Start** — Free Cars + Autoshow Unlock + Install Flags + All Cars, one click
- **Max All** — max Credits, Wheelspins, Super Wheelspins, Skill Points

### Profile Values (memory hooks)
- Credits, Wheelspins, Super Wheelspins, Skill Points, Sell Payout

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

### Physics & Performance (SQL)
- Drift Score 10x, Max Traction, Torque 2x, Reduce Drag

## Anti-Cheat Bypass

- CRC bypass with 5s heartbeat + jitter
- Value Encryption bypass (RET at encryption prologue)
- 5 integrity check patches
- Thread-safe patching with ExpectedOriginal sanity check

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
| **[changcheng967](https://github.com/changcheng967)** | All-in-one improvements, physics SQL cheats, value encryption bypass |

## License

GPL-3.0 — see [LICENSE](LICENSE).
