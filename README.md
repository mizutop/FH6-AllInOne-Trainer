# FH6 All-in-One Trainer

An all-in-one trainer for **Forza Horizon 6** — runtime hooks for player profile values + car/physics cheats + live SQL access to the game's in-memory database. Self-contained `.exe`, no .NET install needed.

> **Offline mode only.** This trainer modifies game memory. Online play (Rivals, Eventlab, Multiplayer, leaderboards) will not work and may result in a ban. Run FH6 in offline mode before using.

## Download

Latest release: **[GitHub Releases](../../releases)** — download the `.zip`, extract, and run `FH6AllInOneTrainer.exe` as Administrator.

## Features

### Profile Values (runtime hooks)
- **Credits** — set any amount (10K to 999M). Toggle on, then reopen the Credits tab to see the updated value.
- **Wheelspins** — set count (10–999). Toggle on, then spin once for it to lock.
- **Super Wheelspins** — set count (10–999). Same as Wheelspins — spin once to activate.
- **Skill Points** — set any amount. Toggle on, then spend a point for the change to take effect.
- **Sell Payout** — multiply car sell prices by any value.
- **Drift Score** — multiply drift score by any value (5x, 10x, 50x, or custom).
- **No Skill Break** — prevents skill chains from breaking on impacts.

Uses inline code cave hooks with toggle+value slots — based on the paris' club approach (CALL-resolution with string-compare verification).

### Quick Actions
- **Quick Start** — 999M Credits + Free Cars + Autoshow Unlock + Install Flags + All Cars
- **Max All** — max Credits, Wheelspins, Super Wheelspins, Skill Points

### SQL Database (in-memory SQLite)
- **Unlock Everything** — all SQL cheats in one click
- Free Cars (BaseCost=0), Autoshow Unlock, Install Flags — with persistent locks that re-apply every 10s
- Add All Cars (CarBuckets approach), Free Upgrades (47 tables), Free Wheels, Full Autoshow
- Unlock Upgrade Presets, Clear "NEW!" Tag

### Physics & Performance (SQL)
- Drift Score 10x, Max Traction, Torque 2x, Reduce Drag 0.5x

## Anti-Cheat Bypass

- CRC bypass with heartbeat timer + jitter (XXH check pointer replacement)
- 5/5 integrity check patches (MemCmp, PageHash, TextHash, CodeSection, Checksum)
- Deferred retry for Denuvo-encrypted pages — signatures that aren't available on first scan are retried during the CRC heartbeat
- Thread-safe patching with ExpectedOriginal sanity check
- Pre-resolution: all hook targets are scanned before any hooks are installed

## Known Limitations

- **Denuvo timing** — PageHash and TextHash code pages are encrypted by Denuvo in the static binary and only decrypted at runtime. The trainer retries missing patches during the CRC heartbeat, but there may be a brief window (first ~30s) where not all patches are active.
- **Value Encryption bypass signature not found** on latest builds. Some profile value changes may not persist between sessions.
- **XP / Level modding** is not yet supported. See [issue #19](../../issues/19) for discussion.
- Teleport, Freeze AI, timers, gravity, and other experimental cheats from earlier versions were removed — they used signatures that matched the wrong functions and didn't work reliably.

## Build from Source

Requires **.NET 10 SDK** on Windows:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Credits

| Who | Contribution |
|-----|-------------|
| **[paris' club](https://discord.gg/WSd3bRNJuJ)** | Core profile cheats (CALL-resolution approach), SQL features, CRC bypass |
| **[ForzaMods](https://github.com/ForzaMods/Forza-Mods-AIO)** | AOB signatures reference |
| **[matkhl](https://www.unknowncheats.me/forum/other-games/752793)** | Free Upgrades SQL (47 tables), CarBuckets approach, database dumper |
| **[Omkmakwana](https://github.com/Omkmakwana/FH6Trainer)** | Add All Cars reference |
| **[Chaarkor](https://github.com/Chaarkoor)** | Original Avalonia UI shell, MVVM architecture |
| **[changcheng967](https://github.com/changcheng967)** | All-in-one integration, physics SQL cheats, code cave detours, UI |

## License

GPL-3.0 — see [LICENSE](LICENSE).
