# FH6 Anti-Cheat Integrity Map

## Overview

The game has 6 integrity verification mechanisms. Five were previously identified and bypassed. The sixth (TerminateGuard) was discovered through systematic analysis of the live game binary dump.

## Bypass Summary

| # | Name | Signature | Patch | Effect |
|---|------|-----------|-------|--------|
| 1 | MemCmp | `E8 ?? ?? ?? ?? 48 8B D8 48 8B C8 E8 ?? ?? ?? ?? 85 C0 75` | NOP JNZ | Memory comparison check always passes |
| 2 | PageHash | `48 83 EC 20 48 8B F1 BA 02 00 00 00 48 8B 89 50 02 00 00 E8 ?? ?? ?? ?? 48 8B F8 48 85 C0 75` | NOP JNZ | Page hash check always passes |
| 3 | TextHash | `48 8D 15 ?? ?? ?? ?? 48 8B C8 FF 15 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? BA 01 00 00 00 48 85 C0 0F 45` | NOP CMOVNZ | Text section hash check always passes |
| 4 | CodeSection | `48 8D 59 08 48 8B FA 48 8B CB BA 20 00 00 00 E8` | MOV EAX,1 | Code section verification always returns pass |
| 5 | Checksum | `48 8B D6 48 8B CF E8 ?? ?? ?? ?? 84 C0 74` | NOP JZ | Checksum comparison always passes |
| 6 | TerminateGuard | `FF 90 28 01 00 00 84 C0 74 0B B9 FF FF FF FF FF 15` | JZ→JMP | Skips TerminateProcess call — prevents 10-min shutdown |

## TerminateGuard (6th Check) — Detailed Analysis

### Discovery
Found by tracing from known TerminateProcess IAT slot (0x06324CC8) resolved to `0x00007FF9A7679220` in the live dump.

### Call Chain
1. **Callback function** at `0x025E4700` — called via function pointer (no direct callers in .text)
2. Calls **shutdown orchestrator** at `0x025DFCF0` (3485 bytes) from `0x025E499E`
3. Shutdown function performs integrity verification, logging, then conditionally calls TerminateProcess

### Conditional Termination (address 0x025E07E5)
```asm
FF 90 28 01 00 00    ; CALL [RAX+0x128] — vtable call to check "ok to terminate"
84 C0                ; TEST AL, AL
74 0B                ; JZ skip_terminate (+0xB)
B9 FF FF FF FF       ; MOV ECX, -1 (GetCurrentProcess pseudo-handle)
FF 15 CE 3A D4 03    ; CALL [IAT TerminateProcess]
; skip_terminate:
```

### Patch
- AOB: `FF 90 28 01 00 00 84 C0 74 0B B9 FF FF FF FF FF 15`
- Unique: exactly 1 match in entire .text section (99MB)
- Patch at offset 8: `74` → `EB` (JZ → JMP)
- Result: always skips TerminateProcess, regardless of integrity check result

### 10-Minute Timer
Five locations store 600000ms (0x927C0) timer values:
- `0x04BFAE6A` — timer object setup
- `0x04C00349` — timer object setup
- `0x04C00892` — timer object setup
- `0x04C216D6` — timer object setup
- `0x052758FF` — coroutine/task timeout

These are timer configurations, not the shutdown trigger itself. The TerminateGuard bypass prevents the shutdown regardless of timer state.

## CRC Heartbeat

Three CRC heartbeat functions at:
- `0x0536BA90` (main, 433 bytes)
- `0x05393160` (57 bytes)
- `0x0541A890` (129 bytes)

These run on a timer and verify code integrity by computing CRC/XXH hashes. The trainer bypasses these by replacing the CRC function pointer in a vtable with a RET stub.

## Hash Constants Found
- `0x9E3779B9` (Golden ratio / TEA): 57 occurrences — used in hash computations
- `0xEDB88320` (CRC32 IEEE): 4 occurrences
- `0x04C11DB7` (CRC32 normal): 2 occurrences
