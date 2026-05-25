# FH6 All-in-One Trainer — Project Status & How to Get Help

Welcome to the FH6 All-in-One Trainer.

**Current version:** v5.1.0

## What works

- Credits, Wheelspins, Super Wheelspins, Skill Points (Steam offline mode only)
- Sell Payout, Drift Score Multiplier, No Skill Break, Freeze AI
- SQL features: Free Cars, Add All Cars, Free Upgrades, Autoshow Unlock, Clear NEW Tag, and more
- Physics: Max Traction, Torque Scale, Drag Scale

## Known issues

- **Game crashes after ~5-15 minutes** — Fixed in v5.1.0. Root cause was an off-by-one error in the PageHash and TextHash patch offsets, plus Denuvo code encryption delaying signature availability. The trainer now retries missing bypasses during the CRC heartbeat.
- **Xbox/Windows Store version** — CRC bypass fails (VirtualProtectEx errors). Signatures may differ. Only Steam is tested.
- **Super Wheelspins** — "does not accept a value" bug. Will be fixed.
- **XP/Level** — not yet implemented. Research in progress.
- **Online mode (Forza Live)** — does not work and likely never will. Use offline mode only.

## Before reporting a bug

1. Make sure you're in **offline mode** (not Forza Live)
2. Attach the trainer **log file** (found in the trainer directory)
3. Include your FH6 version (Steam or Xbox) and trainer version

## Want to help?

This project is open source. If you have reverse engineering experience (x64dbg, IDA, Ghidra) and want to help find missing anti-cheat signatures or new cheat features, PRs and issue discussions are welcome.
