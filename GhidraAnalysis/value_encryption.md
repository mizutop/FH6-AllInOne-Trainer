# FH6 Value Encryption Analysis

## Key Finding: No Separate Encryption Function

The original search for a value encryption function with signature `48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 8B EA 48 8B F9 8B F1 48 8B 0D` returned **0 matches** in the live dump. Further analysis revealed:

- The subset `8B EA 48 8B F9 8B F1` also returned **0 matches**
- The relaxed pattern `8B EA 48 8B F9` returned **20 matches** but none were encryption functions

## Conclusion

Value encryption in FH6 is **inline XOR**, not a separate callable function. The encryption is performed within the getter/setter functions themselves using 16-bit XOR operations.

### Evidence from Money Functions

Money setter at `0x053176F0` and `0x05318F70` use inline encryption:
```asm
66 44 33 D8    ; XOR R11W, AX  (16-bit XOR)
66 44 33 D0    ; XOR R10W, AX  (16-bit XOR)
66 33 D8       ; XOR BX, AX    (16-bit XOR)
66 33 C8       ; XOR CX, AX    (16-bit XOR)
66 33 D0       ; XOR DX, AX    (16-bit XOR)
```

These XOR operations are applied to profile values as they're read/written. The "encryption key" is derived from other register values at the point of access.

### Implications for Trainer

1. **The ValueEncryption bypass (RET at function prologue) is unnecessary** — there's no separate encryption function to bypass
2. **Profile value cheats work via hook-based detours** — they intercept the setter functions directly, not through a decryption layer
3. **The XOR patterns in Money functions are for game-internal obfuscation**, not anti-cheat — the trainer's hook approach sidesteps them entirely

## Search Statistics

| Approach | Matches | Result |
|----------|---------|--------|
| Full original signature | 0 | Function doesn't exist |
| Subset `8B EA 48 8B F9 8B F1` | 0 | Pattern absent |
| Relaxed `8B EA 48 8B F9` | 20 | Unrelated functions |
| XOR + ROL within 64 bytes | 303 | Crypto/hash functions, not value encryption |
| SEH-like (Denuvo wrappers) | 226 | Denuvo stubs |
| XOR EAX, imm32 near prologue | 16533 | Mostly string hashing |
