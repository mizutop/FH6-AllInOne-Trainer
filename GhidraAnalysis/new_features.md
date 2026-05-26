# FH6 New Feature Opportunities

## NoClip (Broken — Needs Fix)

Current signature hooks a string/hash compare function, not the actual collision function.

### Collision-related strings found
- "Collision" — 1056 occurrences (it's a Data_Car column field)
- "CollisionMode" — at 0x063907D0
- "HavokCollisionTemplate" — at 0x069A9628
- "RaycastTireCollision" — at 0x0697ADD0
- "RaycastDistance" — at 0x063C734B
- "RaycastPadding" — at 0x064C1B36

### Havok Physics Engine
- "Havok" at 0x068905B4 — physics engine string
- "HavokCollisionTemplate" — collision geometry
- "HavokNavMesh" — navigation mesh
- Raycast-based collision system (not volume-based)

### Next Steps
1. Use Ghidra to decompile functions referencing "RaycastTireCollision" and "CollisionMode"
2. Find the player-specific collision check function (not car-to-car, but player-to-world)
3. The collision function likely does a raycast downward for ground detection
4. Look for functions that check is_grounded flag near position updates

## Potential New Features

### Speed Multiplier
Based on the existing Acceleration cheat, a speed multiplier could:
- Hook the velocity write function
- Multiply the velocity vector by a scalar
- Look for the same MOVSS/MOVUPS patterns used in Teleport

### Infinite Boost
- Find the boost/nitrous depletion function
- Hook to prevent decrement or always refill
- Search for boost-related strings in the binary

### Perfect Handling
- Hook the tire grip/suspension function
- Set friction/spring constants to ideal values
- Related to "Traction_Road" field in Data_Car (already modifiable via SQL)

## Money Functions

Two Money setter functions identified:
- `0x053176F0` — Credits setter (BB=0x04 = initial value parameter)
- `0x05318F70` — Credits setter variant (BB=0x0C = different offset)

Both use 16-bit XOR encryption inline. Both are 500+ byte functions with complex control flow. The trainer already hooks these via the Credits profile feature.
