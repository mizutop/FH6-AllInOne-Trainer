namespace FH6Mod.Cheats.RuntimeHook;

public enum RuntimeProfileFeature
{
    // Original working cheats
    Credits,
    Wheelspins,
    SuperWheelspins,
    SkillPoints,
    DriftScoreMultiplier,
    NoSkillBreak,
    SellFactor,

    // New cheats (from ForzaMods AIO signatures)
    FreezeAI,
    Teleport,
    NoClip,
    GravityMultiplier,
    NoWaterDrag,
    TimeOfDay,
    SkillScoreMultiplier,
    PrizeScale,
    RemoveBuildCap,
    RaceTimeScale,

    // Batch 3 — more ForzaMods AIO signatures
    Acceleration,
    SpeedTrapMultiplier,
    MissionTimeScale,
    FreeClothing,

}

internal sealed class RuntimeProfileHookDescriptor
{
    public string Key = "";
    public string Name = "";
    public string Signature = "";
    public int MatchOffset;
    public bool ResolveCallTarget;
    public int CallTargetOffset;
    public int HookSize;
    public byte[] Asm = [];
    public byte[] ExpectedOriginal = [];
    public int ToggleOffset;
    public int ValueOffset = -1;
}

internal sealed class RuntimeDetour
{
    public string Name = "";
    public ulong Address;
    public ulong DetourAddress;
    public int Size;
    public byte[] Original = [];
    public byte[] Patch = [];
}

internal static class ProfileFeatureCatalog
{
    public static RuntimeProfileHookDescriptor Get(RuntimeProfileFeature feature) => feature switch
    {
        // ===== ORIGINAL WORKING CHEATS =====

        RuntimeProfileFeature.Credits => new()
        {
            Key = "Credits", Name = "Credits",
            Signature = "E8 ? ? ? ? 89 84 ? ? ? ? ? 4C 8D ? ? ? ? ? 48 8B",
            ResolveCallTarget = true, CallTargetOffset = 24,
            HookSize = 6,
            ExpectedOriginal = [72, 139, 79, 8, 51, 210],
            ToggleOffset = 49, ValueOffset = 50,
            Asm =
            [
                72, 139, 79, 8, 128, 61, 38, 0, 0, 0,
                1, 117, 29, 72, 139, 84, 36, 32, 72, 184,
                67, 114, 101, 100, 105, 116, 115, 0, 72, 57,
                66, 180, 117, 8, 139, 21, 10, 0, 0, 0,
                137, 23, 49, 210,
            ],
        },
        RuntimeProfileFeature.Wheelspins => new()
        {
            Key = "Wheelspins", Name = "Wheelspins",
            Signature = "48 89 5C 24 08 57 48 83 EC 20 48 8B FA 33 D2 48 8B 4F 10",
            MatchOffset = 28, HookSize = 5,
            ExpectedOriginal = [51, 210, 139, 95, 8],
            ToggleOffset = 28, ValueOffset = 29,
            Asm =
            [
                128, 61, 21, 0, 0, 0, 1, 117, 9, 139,
                21, 14, 0, 0, 0, 137, 87, 8, 51, 210,
                139, 95, 8,
            ],
        },
        RuntimeProfileFeature.SuperWheelspins => new()
        {
            Key = "SuperWheelspins", Name = "Super Wheelspins",
            Signature = "48 89 5C 24 08 57 48 83 EC 20 48 8B FA 33 D2 48 8B 4F 18",
            MatchOffset = 28, HookSize = 5,
            ExpectedOriginal = [51, 210, 139, 95, 16],
            ToggleOffset = 28, ValueOffset = 29,
            Asm =
            [
                128, 61, 21, 0, 0, 0, 1, 117, 9, 139,
                21, 14, 0, 0, 0, 137, 87, 16, 51, 210,
                139, 95, 16,
            ],
        },
        RuntimeProfileFeature.SkillPoints => new()
        {
            Key = "SkillPoints", Name = "Skill Points",
            Signature = "85 D2 78 32 48 89 5C 24 08 57 48 83 EC 20 8B DA 48 8B F9 48 8B 49 48",
            MatchOffset = 34, HookSize = 5,
            ExpectedOriginal = [51, 210, 137, 95, 64],
            ToggleOffset = 25, ValueOffset = 26,
            Asm =
            [
                128, 61, 18, 0, 0, 0, 1, 117, 6, 139,
                29, 11, 0, 0, 0, 51, 210, 137, 95, 64,
            ],
        },
        RuntimeProfileFeature.DriftScoreMultiplier => new()
        {
            Key = "DriftScoreMultiplier", Name = "Drift Score Multiplier",
            Signature = "E8 ? ? ? ? F3 0F ? ? 0F 28 ? ? ? 0F 28",
            MatchOffset = 5, HookSize = 9,
            ExpectedOriginal = [243, 15, 88, 247, 15, 40, 124, 36, 32],
            ToggleOffset = 31, ValueOffset = 32,
            Asm =
            [
                128, 61, 24, 0, 0, 0, 1, 117, 8, 243,
                15, 89, 61, 15, 0, 0, 0, 243, 15, 88,
                247, 15, 40, 124, 36, 32,
            ],
        },
        RuntimeProfileFeature.NoSkillBreak => new()
        {
            Key = "NoSkillBreak", Name = "No Skill Break",
            Signature = "0F B6 ? 40 38 ? ? ? ? ? 74 ? 84 C0",
            MatchOffset = 0, HookSize = 10,
            ExpectedOriginal = [15, 182, 240, 64, 56, 171, 116, 2, 0, 0],
            ToggleOffset = 26, ValueOffset = -1,
            Asm =
            [
                128, 61, 19, 0, 0, 0, 1, 117, 2, 48,
                192, 15, 182, 240, 64, 56, 171, 116, 2, 0,
                0,
            ],
        },
        RuntimeProfileFeature.SellFactor => new()
        {
            Key = "SellFactor", Name = "Sell Payout",
            Signature = "44 8B ? ? ? ? ? 33 D2 48 8B ? ? ? ? ? E8 ? ? ? ? 90",
            MatchOffset = 0, HookSize = 7,
            ExpectedOriginal = [68, 139, 183, 8, 1, 0, 0],
            ToggleOffset = 28, ValueOffset = 29,
            Asm =
            [
                68, 139, 183, 8, 1, 0, 0, 128, 61, 14,
                0, 0, 0, 1, 117, 7, 68, 139, 53, 6,
                0, 0, 0,
            ],
        },

        // ===== NEW CHEATS (ForzaMods AIO signatures) =====

        // Freeze AI: zeroes AI car velocity X/Y/Z when not the local player
        // Original: F3 0F 58 81 54 01 00 00  (addss xmm0, [rcx+154h])
        RuntimeProfileFeature.FreezeAI => new()
        {
            Key = "FreezeAI", Name = "Freeze AI",
            Signature = "F3 0F ? ? ? ? ? ? F3 0F ? ? F3 0F ? ? 0F 57 ? F3 0F ? ? ? ? ? ? F3 0F ? ? C3",
            MatchOffset = 0, HookSize = 8,
            ExpectedOriginal = [243, 15, 88, 129, 84, 1, 0, 0],
            ToggleOffset = 18, ValueOffset = -1,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 14, 0, 0, 0, 1,
                // jne skip
                117, 9,
                // xorps xmm0,xmm0 ; zero velocity
                15, 87, 192,
                // addss xmm0,[rcx+154h] (original)
                243, 15, 88, 129, 84, 1, 0, 0,
            ],
        },

        // Teleport to waypoint: reads waypoint coords from rdi+0x230, writes to player pos
        // Confirmed FH6 match: 0F 10 8B 30 02 00 00 (movups xmm1,[rbx+230h])
        RuntimeProfileFeature.Teleport => new()
        {
            Key = "Teleport", Name = "Teleport to Waypoint",
            Signature = "0F 10 8B 30 02 00 00 0F 11 8F 30 02 00 00",
            MatchOffset = 0, HookSize = 7,
            ExpectedOriginal = [15, 16, 139, 48, 2, 0, 0],
            ToggleOffset = 16, ValueOffset = -1,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 13, 0, 0, 0, 1,
                // jne skip
                117, 2,
                // xorps xmm1,xmm1 ; zero = teleport marker
                15, 87, 201,
                // movups xmm1,[rbx+230h] (original)
                15, 16, 139, 48, 2, 0, 0,
            ],
        },

        // No Clip: skip collision processing for local player
        // Original: 48 8B C4 4C 89 48 20 56 41 57 41
        RuntimeProfileFeature.NoClip => new()
        {
            Key = "NoClip", Name = "No Clip",
            Signature = "48 8B ? 4C 89 ? ? 56 41 ? 41",
            MatchOffset = 0, HookSize = 7,
            ExpectedOriginal = [72, 139, 196, 76, 137, 72, 32],
            ToggleOffset = 18, ValueOffset = -1,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 14, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // ret (skip collision)
                195,
                // nop padding
                144, 144, 144,
                // original: mov rax,rsp; mov [rax+20],r9
                72, 139, 196, 76, 137, 72, 32,
            ],
        },

        // Gravity Multiplier: multiply gravity on player car
        // Original: F3 0F 59 4B 08  (mulss xmm1,[rbx+8])
        RuntimeProfileFeature.GravityMultiplier => new()
        {
            Key = "GravityMultiplier", Name = "Gravity Multiplier",
            Signature = "F3 0F ? ? ? F3 0F ? ? ? ? ? ? F3 0F ? ? ? ? ? ? 45 84 ? 74",
            MatchOffset = 0, HookSize = 5,
            ExpectedOriginal = [243, 15, 89, 75, 8],
            ToggleOffset = 14, ValueOffset = 15,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 11, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // mulss xmm1,[value] (custom gravity)
                243, 15, 89, 13, 2, 0, 0, 0,
                // mulss xmm1,[rbx+8] (original)
                243, 15, 89, 75, 8,
            ],
        },

        // No Water Drag: return early from water drag function
        // Original: 48 8B C4 F3 0F 11 48 10
        RuntimeProfileFeature.NoWaterDrag => new()
        {
            Key = "NoWaterDrag", Name = "No Water Drag",
            Signature = "48 8B ? F3 0F ? ? ? 53 55",
            MatchOffset = 0, HookSize = 8,
            ExpectedOriginal = [72, 139, 196, 243, 15, 17, 72, 16],
            ToggleOffset = 20, ValueOffset = -1,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 16, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // ret
                195,
                // nop
                144, 144, 144, 144, 144,
                // original
                72, 139, 196, 243, 15, 17, 72, 16,
            ],
        },

        // Time of Day: override time float at rbx+8
        // Original: F2 0F 11 43 08  (movsd [rbx+8],xmm0)
        // Signature found at result+6, HookSize=5
        RuntimeProfileFeature.TimeOfDay => new()
        {
            Key = "TimeOfDay", Name = "Time of Day",
            Signature = "44 0F ? ? ? ? F2 0F ? ? ? 48 83 C4",
            MatchOffset = 6, HookSize = 5,
            ExpectedOriginal = [242, 15, 17, 67, 8],
            ToggleOffset = 14, ValueOffset = 15,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 11, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // movsd xmm0,[value]
                242, 15, 16, 5, 2, 0, 0, 0,
                // movsd [rbx+8],xmm0 (original)
                242, 15, 17, 67, 8,
            ],
        },

        // Skill Score Multiplier: imul earned skill score by multiplier
        // Confirmed FH6 match near: 8B 78 08 48 8B 18 48 3B DF
        RuntimeProfileFeature.SkillScoreMultiplier => new()
        {
            Key = "SkillScoreMultiplier", Name = "Skill Score Multiplier",
            Signature = "8B 78 08 48 8B 18 48 3B DF",
            MatchOffset = 0, HookSize = 7,
            ExpectedOriginal = [139, 120, 8, 72, 139, 77, 96],
            ToggleOffset = 19, ValueOffset = 20,
            Asm =
            [
                // mov edi,[rax+8] (original first instr)
                139, 120, 8,
                // cmp [toggle], 1
                128, 61, 12, 0, 0, 0, 1,
                // jne skip
                117, 6,
                // imul edi,[value]
                139, 13, 2, 0, 0, 0, 15, 175, 255,
                // mov rcx,[rbp+60h] (original second instr)
                72, 139, 77, 96,
            ],
        },

        // Prize Scale: multiply wheelspin reward float
        // Confirmed FH6 match: F3 0F 10 73 10 (movss xmm6,[rbx+10h])
        RuntimeProfileFeature.PrizeScale => new()
        {
            Key = "PrizeScale", Name = "Prize Scale",
            Signature = "F3 0F 10 73 10 44 0F 29 40",
            MatchOffset = 0, HookSize = 5,
            ExpectedOriginal = [243, 15, 16, 115, 16],
            ToggleOffset = 14, ValueOffset = 15,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 11, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // movss xmm6,[value]
                243, 15, 16, 53, 2, 0, 0, 0,
                // movss xmm6,[rbx+10h] (original)
                243, 15, 16, 115, 16,
            ],
        },

        // Remove Build Cap: zero out the engine swap/build power cap
        // Signature at result+5, Original: F3 0F 11 43 44 (movss [rbx+44h],xmm0)
        RuntimeProfileFeature.RemoveBuildCap => new()
        {
            Key = "RemoveBuildCap", Name = "Remove Build Cap",
            Signature = "E8 ? ? ? ? F3 0F ? ? ? 48 8B ? ? ? 48 8B",
            MatchOffset = 5, HookSize = 5,
            ExpectedOriginal = [243, 15, 17, 67, 68],
            ToggleOffset = 14, ValueOffset = -1,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 11, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // xorps xmm0,xmm0 (zero the cap)
                15, 87, 192,
                // movss [rbx+44h],xmm0 (original)
                243, 15, 17, 67, 68,
            ],
        },

        // Race Time Scale: multiply race timer
        // Signature at result+29, Original: F3 0F 5A CE F2 0F 58 C8
        RuntimeProfileFeature.RaceTimeScale => new()
        {
            Key = "RaceTimeScale", Name = "Race Time Scale",
            Signature = "40 ? 48 83 EC ? 48 8B ? 48 8B ? 0F 29 ? ? ? 0F 28 ? FF 50 ? 0F 57",
            MatchOffset = 29, HookSize = 8,
            ExpectedOriginal = [243, 15, 90, 206, 242, 15, 88, 200],
            ToggleOffset = 20, ValueOffset = 21,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 15, 0, 0, 0, 1,
                // jne skip
                117, 7,
                // cvtss2sd xmm1,[value]
                243, 15, 90, 13, 2, 0, 0, 0,
                // addsd xmm1,xmm0 (original part)
                242, 15, 88, 200,
                // cvtss2sd xmm1,xmm6 (original first instr)
                243, 15, 90, 206,
            ],
        },

        // Acceleration Override: replace car acceleration input with custom value
        // FH6 variant from Omkmakwana: F3 0F 10 81 ?? ?? ?? ?? F3 0F 59 C1 C3
        // Original: F3 0F 10 5D 0C (movss xmm3,[rbp+0Ch])
        RuntimeProfileFeature.Acceleration => new()
        {
            Key = "Acceleration", Name = "Acceleration Override",
            Signature = "F3 0F ? ? ? 41 0F ? ? 0F C6 DB ? 41 0F",
            MatchOffset = 0, HookSize = 5,
            ExpectedOriginal = [243, 15, 16, 93, 12],
            ToggleOffset = 14, ValueOffset = 15,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 11, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // movss xmm3,[value]
                243, 15, 16, 29, 2, 0, 0, 0,
                // movss xmm3,[rbp+0Ch] (original)
                243, 15, 16, 93, 12,
            ],
        },

        // Speed Trap Multiplier: multiply speed trap score
        // Original: 0F 29 44 24 30 (movaps [rsp+30h],xmm0)
        RuntimeProfileFeature.SpeedTrapMultiplier => new()
        {
            Key = "SpeedTrapMultiplier", Name = "Speed Trap Multiplier",
            Signature = "0F 29 ? ? ? 48 8B ? 48 8B ? ? ? ? ? 48 85 ? 74",
            MatchOffset = 0, HookSize = 5,
            ExpectedOriginal = [15, 41, 68, 36, 48],
            ToggleOffset = 14, ValueOffset = 15,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 11, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // mulss xmm0,[value]
                243, 15, 89, 5, 2, 0, 0, 0,
                // movaps [rsp+30h],xmm0 (original)
                15, 41, 68, 36, 48,
            ],
        },

        // Mission Time Scale: scale mission timer (0 = freeze)
        // Original: F3 0F 5C C7 F3 0F 11 87 0C 04 00 00
        RuntimeProfileFeature.MissionTimeScale => new()
        {
            Key = "MissionTimeScale", Name = "Mission Time Scale",
            Signature = "F3 0F ? ? F3 0F ? ? ? ? ? ? 0F 2F ? 0F 87 ? ? ? ? C7 ? ? ? ? ? 00 00 00 00",
            MatchOffset = 0, HookSize = 12,
            ExpectedOriginal = [243, 15, 92, 199, 243, 15, 17, 135, 12, 4, 0, 0],
            ToggleOffset = 26, ValueOffset = 27,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 16, 0, 0, 0, 1,
                // jne skip
                117, 7,
                // mulss xmm0,[value]
                243, 15, 89, 5, 2, 0, 0, 0,
                // subss xmm0,xmm7 (original first)
                243, 15, 92, 199,
                // movss [rdi+40Ch],xmm0 (original second)
                243, 15, 17, 135, 12, 4, 0, 0,
            ],
        },

        // Free Clothing: set clothing item price to 0
        // Confirmed FH6 match: 8B 88 A4 00 00 00 89 4D 98 (mov ecx,[rax+A4h]; mov [rbp-68],ecx)
        RuntimeProfileFeature.FreeClothing => new()
        {
            Key = "FreeClothing", Name = "Free Clothing",
            Signature = "8B 88 A4 00 00 00 89 4D",
            MatchOffset = 0, HookSize = 6,
            ExpectedOriginal = [139, 136, 164, 0, 0, 0],
            ToggleOffset = 18, ValueOffset = -1,
            Asm =
            [
                // cmp [toggle], 1
                128, 61, 14, 0, 0, 0, 1,
                // jne skip
                117, 3,
                // xor ecx,ecx (price = 0)
                49, 201,
                // nop
                144,
                // mov ecx,[rax+0A4h] (original)
                139, 136, 164, 0, 0, 0,
            ],
        },

        _ => throw new System.InvalidOperationException("Unsupported runtime profile feature."),
    };
}
