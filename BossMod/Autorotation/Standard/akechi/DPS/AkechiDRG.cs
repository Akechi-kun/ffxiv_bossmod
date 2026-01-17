using BossMod.DRG;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiDRG(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Dives, ElusiveJump, LanceCharge, BattleLitany, LifeSurge, MirageDive, WyrmwindThrust, PiercingTalon, TrueNorth, DragonfireDive, Geirskogul, Stardiver, Jump, Nastrond, RiseOfTheDragon, Starcross }
    public enum AOEStrategy { AutoFinish, ForceSTFinish, ForceNormalFinish, ForceBuffsFinish, ForceAOEFinish, AutoBreak, ForceSTBreak, ForceNormalBreak, ForceBuffsBreak, ForceAOEBreak }
    public enum DivesStrategy { Allow3, Allow5, Allow1, Allow, Forbid }
    public enum ElusiveDirection { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }
    public enum BuffsStrategy { Automatic, Together, RaidBuffsOnly, Force, ForceWeave, Delay }
    public enum SurgeStrategy { Automatic, WhenBuffed, Force, ForceWeave, ForceNextOpti, ForceNextOptiWeave, Delay }
    public enum FlexStrategy { ASAP, Late, LateOrBuffed, Delay }
    public enum PiercingTalonStrategy { AllowEX, Allow, Force, ForceEX, Forbid }
    public enum TrueNorthStrategy { Automatic, ASAP, Rear, Flank, Force, Delay }
    public enum CommonStrategy { Automatic, Force, ForceEX, ForceWeave, ForceWeaveEX, Delay }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRG", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Excellent, BitMask.Build(Class.LNC, Class.DRG), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.AutoFinish, "Automatically use best rotation based on targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceSTFinish, "Force full single-target combo loop regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceNormalFinish, "Force damage combo loop (TrueThrust->VorpalThrust->FullThrust->FangAndClaw->Drakesbane) regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceBuffsFinish, "Force buffs combo loop (TrueThrust->Disembowel->ChaosThrust->WheelingThrust->Drakesbane) regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceAOEFinish, "Force AoE rotation (DoomSpike->SonicThrust->CoerthanTorment) regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.AutoBreak, "Automatically use best rotation depending on targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceSTBreak, "Force full single-target combo loop regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceNormalBreak, "Force damage combo loop (TrueThrust->VorpalThrust->FullThrust->FangAndClaw->Drakesbane) regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceBuffsBreak, "Force buffs combo loop (TrueThrust->Disembowel->ChaosThrust->WheelingThrust->Drakesbane) regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceAOEBreak, "Force AoE rotation (DoomSpike->SonicThrust->CoerthanTorment) regardless of targets nearby - will break current combo loop if in one")
            .AddAssociatedActions(
                AID.TrueThrust, AID.RaidenThrust, AID.DoomSpike, AID.DraconianFury, //1
                AID.VorpalThrust, AID.LanceBarrage, AID.Disembowel, AID.SpiralBlow, AID.SonicThrust, //2
                AID.FullThrust, AID.HeavensThrust, AID.ChaosThrust, AID.ChaoticSpring, AID.CoerthanTorment, //3
                AID.WheelingThrust, AID.FangAndClaw, //4
                AID.Drakesbane); //5

        res.Define(Track.Dives).As<DivesStrategy>("Dives", "Allow/Forbid Dive Actions", 199)
            .AddOption(DivesStrategy.Allow3, "Allow use of Stardiver & Dragonfire Dive only within 3 yalms of target")
            .AddOption(DivesStrategy.Allow5, "Allow use of Stardiver & Dragonfire Dive only within 5 yalms yalms of target")
            .AddOption(DivesStrategy.Allow1, "Allow use of Stardiver & Dragonfire Dive only within 1 yalm of target")
            .AddOption(DivesStrategy.Allow, "Allow use of Stardiver & Dragonfire Dive at any range")
            .AddOption(DivesStrategy.Forbid, "Forbid use of Stardiver & Dragonfire Dive")
            .AddAssociatedActions(AID.DragonfireDive, AID.Stardiver);

        res.Define(Track.ElusiveJump).As<ElusiveDirection>("E.Jump", "Elusive Jump", -1)
            .AddOption(ElusiveDirection.None, "Do not use Elusive Jump")
            .AddOption(ElusiveDirection.CharacterForward, "Jump into the direction forward of the character", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CharacterBackward, "Jump into the direction backward of the character (default)", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CameraForward, "Jump into the direction forward of the camera", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CameraBackward, "Jump into the direction backward of the camera", 30, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(AID.ElusiveJump);

        res.Define(Track.LanceCharge).As<BuffsStrategy>("L.Charge", "Lance Charge", 198)
            .AddOption(BuffsStrategy.Automatic, "Use Lance Charge optimally")
            .AddOption(BuffsStrategy.Together, "Use Lance Charge only with Battle Litany; will delay in attempt to align itself with Battle Litany (up to 30s)", 60, 20, ActionTargets.Self, 30)
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Use Lance Charge only when in alignment with other raid buffs or when raid buffs are active", 60, 20, ActionTargets.Self, 30)
            .AddOption(BuffsStrategy.Force, "Force use of Lance Charge ASAP", 60, 20, ActionTargets.Self, 30)
            .AddOption(BuffsStrategy.ForceWeave, "Force use of Lance Charge inside the next possible weave window", 60, 20, ActionTargets.Self, 30)
            .AddOption(BuffsStrategy.Delay, "Delay use of Lance Charge", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.LanceCharge);

        res.Define(Track.BattleLitany).As<BuffsStrategy>("B.Litany", "Battle Litany", 198)
            .AddOption(BuffsStrategy.Automatic, "Use Battle Litany optimally")
            .AddOption(BuffsStrategy.Together, "Use Battle Litany only with Lance Charge; will delay in attempt to align itself with Lance Charge")
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Use Battle Litany only when in alignment with other raid buffs or when other raid buffs are active", 60, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.Force, "Force use of Battle Litany ASAP", 120, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.ForceWeave, "Force use of Battle Litany inside the next possible weave window", 120, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.Delay, "Delay use of Battle Litany", 0, 0, ActionTargets.None, 52)
            .AddAssociatedActions(AID.BattleLitany);

        res.Define(Track.LifeSurge).As<SurgeStrategy>("L.Surge", "Life Surge", 197)
            .AddOption(SurgeStrategy.Automatic, "Use Life Surge optimally")
            .AddOption(SurgeStrategy.WhenBuffed, "Attempts to use Life Surge when under any buffs - this may be wonky to use generally; mainly for rushing use when under any raidbuff(s)", 0, 0, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.Force, "Force use of Life Surge ASAP", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceWeave, "Force use of Life Surge inside the next possible weave window", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceNextOpti, "Force use of Life Surge in next possible optimal window", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceNextOptiWeave, "Force use of Life Surge optimally inside the next possible weave window", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.Delay, "Delay use of Life Surge", 0, 0, ActionTargets.None, 6)
            .AddAssociatedActions(AID.LifeSurge);

        res.Define(Track.MirageDive).As<FlexStrategy>("M.Dive", "Mirage Dive", 192)
            .AddOption(FlexStrategy.ASAP, "Use Mirage Dive ASAP when under Dive Ready buff", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(FlexStrategy.Late, "Use Mirage Dive when Dive Ready buff is about to end", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(FlexStrategy.LateOrBuffed, "Use Mirage Dive when Dive Ready buff is about to end or when under any buffs", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(FlexStrategy.Delay, "Delay use of Mirage Dive", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.MirageDive);

        res.Define(Track.WyrmwindThrust).As<FlexStrategy>("W.Thrust", "Wyrmwind Thrust", 192)
            .AddOption(FlexStrategy.ASAP, "Use Wyrmwind Thrust ASAP when Firstminds' Focus is full", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(FlexStrategy.Late, "Use Wyrmwind Thrust as late as possible", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(FlexStrategy.LateOrBuffed, "Use Wyrmwind Thrust as late as possible or when under any buffs", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(FlexStrategy.Delay, "Delay use of Wyrmwind Thrust", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(AID.WyrmwindThrust);

        res.Define(Track.PiercingTalon).As<PiercingTalonStrategy>("P.Talon", "Piercing Talon", 100)
            .AddOption(PiercingTalonStrategy.AllowEX, "Allow use of Piercing Talon if already in combat, outside melee range, & is Enhanced")
            .AddOption(PiercingTalonStrategy.Allow, "Allow use of Piercing Talon if already in combat & outside melee range")
            .AddOption(PiercingTalonStrategy.Force, "Force Piercing Talon ASAP (even in melee range)")
            .AddOption(PiercingTalonStrategy.ForceEX, "Force Piercing Talon ASAP when Enhanced (even in melee range)")
            .AddOption(PiercingTalonStrategy.Forbid, "Forbid use of Piercing Talon")
            .AddAssociatedActions(AID.PiercingTalon);

        res.Define(Track.TrueNorth).As<TrueNorthStrategy>("T.North", "True North", 95)
            .AddOption(TrueNorthStrategy.Automatic, "Late-weaves True North when out of positional")
            .AddOption(TrueNorthStrategy.ASAP, "Use True North as soon as possible when out of positional", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Rear, "Use True North for saving rear positionals only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Flank, "Use True North for saving flank positionals only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Force, "Force use of True North ASAP", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Delay, "Delay use of True North", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(ClassShared.AID.TrueNorth);

        res.Define(Track.DragonfireDive).As<CommonStrategy>("D.Dive", "Dragonfire Dive", 195)
            .AddOption(CommonStrategy.Automatic, "Use Dragonfire Dive optimally")
            .AddOption(CommonStrategy.Force, "Force Dragonfire Dive", 120, 0, ActionTargets.Hostile, 50, 91)
            .AddOption(CommonStrategy.ForceEX, "Force Dragonfire Dive (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(CommonStrategy.ForceWeave, "Force Dragonfire Dive inside the next possible weave window", 120, 0, ActionTargets.Hostile, 50, 91)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Dragonfire Dive inside the next possible weave window (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(CommonStrategy.Delay, "Delay Dragonfire Dive", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.DragonfireDive);

        res.Define(Track.Geirskogul).As<CommonStrategy>("Gsk.", "Geirskogul", 196)
            .AddOption(CommonStrategy.Automatic, "Use Geirskogul optimally")
            .AddOption(CommonStrategy.Force, "Force Geirskogul", 60, 0, ActionTargets.Hostile, 60, 69)
            .AddOption(CommonStrategy.ForceEX, "Force Geirskogul (Grants Life of the Dragon & Nastrond Ready)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(CommonStrategy.ForceWeave, "Force Geirskogul inside the next possible weave window", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Geirskogul inside the next possible weave window (Grants Life of the Dragon & Nastrond Ready)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(CommonStrategy.Delay, "Delay Geirskogul", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Geirskogul);

        res.Define(Track.Stardiver).As<CommonStrategy>("S.diver", "Stardiver", 194)
            .AddOption(CommonStrategy.Automatic, "Use Stardiver optimally")
            .AddOption(CommonStrategy.Force, "Force Stardiver", 30, 0, ActionTargets.Hostile, 80, 99)
            .AddOption(CommonStrategy.ForceEX, "Force Stardiver (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(CommonStrategy.ForceWeave, "Force Stardiver inside the next possible weave window", 30, 0, ActionTargets.Hostile, 80, 99)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Stardiver inside the next possible weave window (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(CommonStrategy.Delay, "Delay Stardiver", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Stardiver);

        res.Define(Track.Jump).As<CommonStrategy>("Jump", uiPriority: 193)
            .AddOption(CommonStrategy.Automatic, "Use Jump optimally")
            .AddOption(CommonStrategy.Force, "Force Jump", 30, 0, ActionTargets.Hostile, 30, 67)
            .AddOption(CommonStrategy.ForceEX, "Force Jump (Grants Dive Ready buff)", 30, 15, ActionTargets.Hostile, 68)
            .AddOption(CommonStrategy.ForceWeave, "Force Jump inside the next possible weave window", 30, 0, ActionTargets.Hostile, 30, 67)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Jump inside the next possible weave window (Grants Dive Ready buff)", 30, 15, ActionTargets.Hostile, 68)
            .AddOption(CommonStrategy.Delay, "Delay Jump", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.Jump, AID.HighJump);

        res.DefineOGCD(Track.Nastrond, AID.Nastrond, "Nast.", "Nastrond", 196, 0, 0, ActionTargets.Hostile, 70);
        res.DefineOGCD(Track.RiseOfTheDragon, AID.RiseOfTheDragon, "RotD", "Rise Of The Dragon", 195, 0, 0, ActionTargets.Hostile, 92);
        res.DefineOGCD(Track.Starcross, AID.Starcross, "S.cross", "Starcross", 194, 0, 0, ActionTargets.Hostile, 100);

        return res;
    }

    private bool WantAOE;
    private bool AutoTarget;
    private float ChaosLeft;
    private int NumAOETargets;
    private Enemy? BestAOETarget;
    private Enemy? BestAOETargets;
    private Enemy? BestDOTTarget;
    private Enemy? BestDOTTargets;

    private DragoonGauge Gauge => World.Client.GetGauge<DragoonGauge>();
    private bool NeedPower => PowerLeft <= SkSGCDLength * 2;
    private bool HasPower => PowerLeft > 0;
    private bool HasLOTD => Gauge.LotdTimer > 0;
    private bool HasLC => LCcd is >= 40 and <= 60;
    private bool HasBL => BLcd is >= 100 and <= 120;
    private float BLcd => Cooldown(AID.BattleLitany);
    private float LCcd => Cooldown(AID.LanceCharge);
    private float PowerLeft => StatusRemaining(Player, SID.PowerSurge, 30);
    private bool WantDOT => ComboLastMove is AID.Disembowel or AID.SpiralBlow &&
                            Unlocked(AID.ChaosThrust) && In3y(BestDOTTarget?.Actor) &&
                            Hints.NumPriorityTargetsInAOECircle(Player.Position, 3) == 2;

    private AID BestTrue => (Unlocked(AID.RaidenThrust) && HasEffect(SID.DraconianFire)) ? AID.RaidenThrust : AID.TrueThrust;
    private AID BestDisembowel => Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : Unlocked(AID.Disembowel) ? AID.Disembowel : AID.TrueThrust;
    private AID BestChaos => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust;
    private AID BestWheeling => Unlocked(AID.WheelingThrust) ? AID.WheelingThrust : AID.TrueThrust;
    private AID BestVorpal => Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust;
    private AID BestHeavens => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust;
    private AID BestFang => Unlocked(AID.FangAndClaw) ? AID.FangAndClaw : AID.TrueThrust;
    private AID BestDrakesbane => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust;
    private AID BestSonic => Unlocked(AID.SonicThrust) ? AID.SonicThrust : NeedPower ? LowLevelAOE : AID.DoomSpike;
    private AID BestCoerthan => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : AID.DoomSpike;


    private AID FullSTFinish => ComboLastMove switch
    {
        //finish AOE combo loop if possible
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestTrue,
        AID.DraconianFury => AID.SonicThrust,
        AID.DoomSpike or AID.DraconianFury => Unlocked(AID.SonicThrust) ? AID.SonicThrust : BestTrue,

        AID.WheelingThrust or AID.FangAndClaw => BestDrakesbane,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.Disembowel) && (Unlocked(AID.ChaosThrust) ? (PowerLeft <= SkSGCDLength * 6 || ChaosLeft <= SkSGCDLength * 4) : (Unlocked(AID.FullThrust) ? PowerLeft <= SkSGCDLength * 3 : NeedPower)) ? BestDisembowel : BestVorpal,
        _ => BestTrue,
    };
    private AID BuffsSTFinish => ComboLastMove switch
    {
        //finish AOE combo loop if possible
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestTrue,
        AID.DraconianFury => AID.SonicThrust,
        AID.DoomSpike or AID.DraconianFury => Unlocked(AID.SonicThrust) ? AID.SonicThrust : BestTrue,

        //finish other combo loop if possible
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.FangAndClaw or

        AID.WheelingThrust => BestDrakesbane,
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        AID.TrueThrust or AID.RaidenThrust => BestDisembowel,
        _ => BestTrue,
    };
    private AID NormalSTFinish => ComboLastMove switch
    {
        //finish AOE combo loop if possible
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestTrue,
        AID.DraconianFury => AID.SonicThrust,
        AID.DoomSpike => Unlocked(AID.SonicThrust) ? AID.SonicThrust : BestTrue,

        //finish other combo loop if possible
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        AID.WheelingThrust or

        AID.FangAndClaw => BestDrakesbane,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.TrueThrust or AID.RaidenThrust => BestVorpal,
        _ => BestTrue
    };
    private AID FullAOEFinish => ComboLastMove switch
    {
        //finish ST combo loop if possible
        AID.WheelingThrust or AID.FangAndClaw => BestDrakesbane,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.Disembowel) && (Unlocked(AID.ChaosThrust) ? (PowerLeft <= SkSGCDLength * 6 || ChaosLeft <= SkSGCDLength * 4) : (Unlocked(AID.FullThrust) ? PowerLeft <= SkSGCDLength * 3 : NeedPower)) ? BestDisembowel : BestVorpal,

        AID.SonicThrust => BestCoerthan,
        AID.DoomSpike or AID.DraconianFury => BestSonic,
        _ => Unlocked(AID.SonicThrust) ? (HasEffect(SID.DraconianFire) ? AID.DraconianFury : AID.DoomSpike) : LowLevelAOE,
    };

    private AID FullSTBreak => ComboLastMove switch
    {
        AID.WheelingThrust or AID.FangAndClaw => BestDrakesbane,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.Disembowel) && (Unlocked(AID.ChaosThrust) ? (PowerLeft <= SkSGCDLength * 6 || ChaosLeft <= SkSGCDLength * 4) : (Unlocked(AID.FullThrust) ? PowerLeft <= SkSGCDLength * 3 : NeedPower)) ? BestDisembowel : BestVorpal,
        _ => BestTrue,
    };
    private AID BuffsSTBreak => ComboLastMove switch
    {
        AID.WheelingThrust => BestDrakesbane,
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        AID.TrueThrust or AID.RaidenThrust => BestDisembowel,
        _ => BestTrue,
    };
    private AID NormalSTBreak => ComboLastMove switch
    {
        AID.FangAndClaw => BestDrakesbane,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.TrueThrust or AID.RaidenThrust => BestVorpal,
        _ => BestTrue
    };
    private AID FullAOEBreak => ComboLastMove switch
    {
        AID.SonicThrust => BestCoerthan,
        AID.DoomSpike or AID.DraconianFury => BestSonic,
        _ => Unlocked(AID.SonicThrust) ? (HasEffect(SID.DraconianFire) ? AID.DraconianFury : AID.DoomSpike) : LowLevelAOE,
    };
    private AID AutoFinish => WantAOE ? FullAOEFinish : (AutoTarget && WantDOT) ? BuffsSTFinish : FullSTFinish;
    private AID AutoBreak => WantAOE ? FullAOEBreak : (AutoTarget && WantDOT) ? BuffsSTBreak : FullSTBreak;

    //until we unlock Sonic Thrust, we cannot generate Power Surge from AOE
    //so we use True Thrust -> Disembowel for the buff then immediately switch to Doom Spike spam
    private AID LowLevelAOE => ComboLastMove switch
    {
        AID.DoomSpike => NeedPower ? AID.TrueThrust : AID.DoomSpike,
        AID.Disembowel => AID.DoomSpike,
        AID.TrueThrust => AID.Disembowel,
        _ => NeedPower ? AID.TrueThrust : AID.DoomSpike,
    };

    private static SID[] GetDotStatus() => [SID.ChaosThrust, SID.ChaoticSpring];
    private float ChaosRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 6);

    private (Positional, bool) GetBestPositional(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (NumAOETargets > 2 && Unlocked(AID.DoomSpike) || !Unlocked(AID.ChaosThrust) || primaryTarget == null)
            return (Positional.Any, false);

        if (!Unlocked(AID.FangAndClaw))
            return (Positional.Rear, ComboLastMove == AID.Disembowel);

        (Positional, bool) PredictNextPositional(int StepsBeforeReloop)
        {
            var buffed = CanFitSkSGCD(ChaosLeft, StepsBeforeReloop + 3) && CanFitSkSGCD(PowerLeft, StepsBeforeReloop + 2);
            return (buffed ? Positional.Flank : Positional.Rear, false);
        }

        return ComboLastMove switch
        {
            AID.ChaosThrust => Unlocked(AID.WheelingThrust) ? (Positional.Rear, true) : PredictNextPositional(0),
            AID.Disembowel or AID.SpiralBlow or AID.ChaoticSpring => (Positional.Rear, true),
            AID.TrueThrust or AID.RaidenThrust => PredictNextPositional(-1),
            AID.VorpalThrust or AID.LanceBarrage => (Positional.Flank, false),
            AID.HeavensThrust or AID.FullThrust => (Positional.Flank, true),
            AID.WheelingThrust or AID.FangAndClaw => PredictNextPositional(Unlocked(AID.Drakesbane) ? 1 : 0),
            _ => PredictNextPositional(0)
        };
    }

    private bool ShouldUse(Actor? target) => InCombat(target) && HasPower;
    private bool ShouldDive(Actor? target) => ShouldUse(target) && In20y(target);
    private bool ShouldSpear(Actor? target) => ShouldUse(target) && In15y(target);
    private (bool, OGCDPriority) ShouldBuffUp(BuffsStrategy strategy, Actor? target, bool ready, bool together)
    {
        var minimal = ready && HasPower && Player.InCombat && target != null && In3y(target);
        return strategy switch
        {
            BuffsStrategy.Automatic => (minimal, OGCDPriority.Severe),
            BuffsStrategy.Together => (minimal && together, OGCDPriority.Severe),
            BuffsStrategy.RaidBuffsOnly => (minimal && (RaidBuffsLeft > 0 || RaidBuffsIn <= GCD), OGCDPriority.Severe),
            BuffsStrategy.Force => (true, OGCDPriority.Severe + 2000),
            _ => (false, OGCDPriority.None)
        };
    }
    private (bool, OGCDPriority) ShouldUseCommons(CommonStrategy strategy, Actor? target, bool ready, bool optimal)
    {
        var minimal = ready && target != null;
        return strategy switch
        {
            CommonStrategy.Automatic => (minimal && optimal && CanWeaveIn, OGCDPriority.High),
            CommonStrategy.Force or CommonStrategy.ForceEX => (minimal, OGCDPriority.High + 2000),
            CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX => (minimal && CanWeaveIn, OGCDPriority.High),
            _ => (false, OGCDPriority.None)
        };
    }

    private OGCDPriority LatePrio()
    {
        if (GCD < 0.5f)
            return OGCDPriority.ToGCDPriority + 500;

        var i = Math.Max(0, (int)((SkSGCDLength - GCD) / 0.5f));
        var a = i * 300;
        return OGCDPriority.ExtremelyLow + a;
    }

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {

        AutoTarget = strategy.AutoTarget();
        var manualTargets = Hints.NumPriorityTargetsInAOERect(Player.Position, Player.Rotation.ToDirection(), 10, 2);
        var aoe = strategy.Option(Track.AOE);
        var aoeStrat = aoe.As<AOEStrategy>();
        var forceAOE = aoeStrat is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak;
        WantAOE = Unlocked(AID.DoomSpike) && (AutoTarget ? (NumAOETargets > 2 || forceAOE) : (manualTargets > 2 || forceAOE));
        (BestAOETargets, NumAOETargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        BestAOETarget = WantAOE ? BestAOETargets : AutoTarget ? BestDOTTarget : primaryTarget;

        var mainTarget = primaryTarget?.Actor;
        ChaosLeft = MathF.Max(
            StatusDetails(mainTarget, SID.ChaosThrust, Player.InstanceID).Left,
            StatusDetails(mainTarget, SID.ChaoticSpring, Player.InstanceID).Left);

        (BestDOTTargets, ChaosLeft) = GetDOTTarget(primaryTarget, ChaosRemaining, 2, 3);
        BestDOTTarget = (AutoTarget && WantDOT) ? BestDOTTargets : primaryTarget;

        var (BestSpearTargets, NumSpearTargets) = GetBestTarget(primaryTarget, 15, LineTargetCheck(15));
        var BestSpearTarget = (AutoTarget && Unlocked(AID.Geirskogul) && NumSpearTargets > 1) ? BestSpearTargets : primaryTarget;

        var (BestDiveTargets, NumDiveTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        var BestDiveTarget = (AutoTarget && Unlocked(AID.DragonfireDive) && NumDiveTargets > 1) ? BestDiveTargets : primaryTarget;

        if (strategy.HoldEverything())
            return;

        //abilities
        if (!strategy.HoldAbilities())
        {
            //cooldowns
            if (!strategy.HoldCDs())
            {
                //buffs
                if (!strategy.HoldBuffs())
                {
                    //Lance Charge
                    var lcStrat = strategy.Option(Track.LanceCharge).As<BuffsStrategy>();
                    var (lcCondition, lcPrio) = ShouldBuffUp(lcStrat, mainTarget, ActionReady(AID.LanceCharge), !Unlocked(AID.BattleLitany) || BLcd is > 30 or < 1);
                    if (lcCondition)
                        QueueOGCD(AID.LanceCharge, Player, lcPrio + 3);

                    //Battle Litany
                    var blStrat = strategy.Option(Track.BattleLitany).As<BuffsStrategy>();
                    var (blCondition, blPrio) = ShouldBuffUp(blStrat, mainTarget, ActionReady(AID.BattleLitany), HasLC);
                    if (blCondition)
                        QueueOGCD(AID.BattleLitany, Player, blPrio + 2);

                    //Life Surge
                    if (Unlocked(AID.LifeSurge))
                    {
                        var ls = strategy.Option(Track.LifeSurge);
                        var lsStrat = ls.As<SurgeStrategy>();
                        var lsMinimum = Unlocked(AID.LifeSurge) && !HasEffect(SID.LifeSurge) && (Unlocked(TraitID.EnhancedLifeSurge) ? Cooldown(AID.LifeSurge) < 40.6f : ReadyIn(AID.LifeSurge) < 0.6f);
                        var lv6to17 = ComboLastMove is AID.TrueThrust;
                        var lv18to25 = !Unlocked(AID.FullThrust) && (Unlocked(AID.Disembowel) ? (lv6to17 && !NeedPower) : lv6to17);
                        var lv26to88 = (Unlocked(AID.FullThrust) && ComboLastMove is AID.VorpalThrust or AID.LanceBarrage) || (Unlocked(AID.Drakesbane) && ComboLastMove is AID.WheelingThrust or AID.FangAndClaw);
                        var lv88plus = HasLC && (Cooldown(AID.LifeSurge) < 40 || Cooldown(AID.BattleLitany) > 50) && lv26to88;
                        var st = Unlocked(TraitID.EnhancedLifeSurge) ? lv88plus : (lv26to88 || lv18to25);
                        var tt = (Unlocked(AID.LanceCharge) ? HasLC : lsMinimum) && (Unlocked(AID.ChaosThrust) ? (Unlocked(TraitID.EnhancedLifeSurge) ? ComboLastMove is AID.FangAndClaw or AID.WheelingThrust or AID.Drakesbane : ComboLastMove is AID.FangAndClaw or AID.WheelingThrust) : lv26to88);
                        var aoes = Unlocked(AID.CoerthanTorment) ? ComboLastMove is AID.SonicThrust : Unlocked(AID.SonicThrust) ? ComboLastMove is AID.DoomSpike : Unlocked(AID.DoomSpike) && !NeedPower;
                        var minimal = lsMinimum && InCombat(mainTarget) && HasPower && (WantAOE ? In10y(BestAOETarget?.Actor) : In3y(BestDOTTarget?.Actor));
                        var buffed = ((HasLC && HasLOTD) || HasBL) && (WantAOE ? aoes : lv26to88);
                        var (lsCondition, lsPrio) = lsStrat switch
                        {
                            SurgeStrategy.Automatic => (minimal && !LastActionUsed(AID.Stardiver) && CanWeaveIn && (WantAOE ? aoes : WantDOT ? tt : st), OGCDPriority.High),
                            SurgeStrategy.WhenBuffed => (minimal && buffed, LatePrio()),
                            SurgeStrategy.Force => (lsMinimum, OGCDPriority.Severe + 2000),
                            SurgeStrategy.ForceWeave => (lsMinimum && CanWeaveIn, OGCDPriority.Severe),
                            SurgeStrategy.ForceNextOpti => (lsMinimum && lv26to88, OGCDPriority.Severe + 2000),
                            SurgeStrategy.ForceNextOptiWeave => (lsMinimum && lv26to88 && CanWeaveIn, OGCDPriority.Severe),
                            _ => (false, OGCDPriority.None),
                        };
                        if (lsCondition)
                            QueueOGCD(AID.LifeSurge, Player, lsPrio + 1);
                    }
                }

                //dives
                if (strategy.Option(Track.Dives).As<DivesStrategy>() switch
                {
                    DivesStrategy.Allow3 => In3y(BestDiveTarget?.Actor),
                    DivesStrategy.Allow1 => DistanceFrom(BestDiveTarget?.Actor, 1f),
                    DivesStrategy.Allow5 => In5y(BestDiveTarget?.Actor),
                    DivesStrategy.Allow => In20y(BestDiveTarget?.Actor),
                    _ => false,
                })
                {
                    //Dragonfire Dive
                    var dd = strategy.Option(Track.DragonfireDive);
                    var ddStrat = dd.As<CommonStrategy>();
                    var ddTarget = AOETargetChoice(mainTarget, BestDiveTarget?.Actor, dd, strategy);
                    var (ddCondition, ddPrio) = ShouldUseCommons(ddStrat, ddTarget, ActionReady(AID.DragonfireDive), ShouldDive(ddTarget) && (Unlocked(AID.Geirskogul) ? (HasLC && HasBL && HasLOTD) : Unlocked(AID.BattleLitany) ? (HasLC && HasBL) : HasLC));
                    if (ddCondition)
                        QueueOGCD(AID.DragonfireDive, ddTarget, ddPrio + 3);

                    //Stardiver
                    var sd = strategy.Option(Track.Stardiver);
                    var sdStrat = sd.As<CommonStrategy>();
                    var sdTarget = AOETargetChoice(mainTarget, BestDiveTarget?.Actor, sd, strategy);
                    var (sdCondition, sdPrio) = ShouldUseCommons(sdStrat, sdTarget, ActionReady(AID.Stardiver) && HasLOTD, ShouldDive(sdTarget) && CanEarlyWeaveIn && Cooldown(Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump) > 3 && Cooldown(AID.DragonfireDive) > 3 && Cooldown(AID.Geirskogul) > 3);
                    if (sdCondition)
                        QueueOGCD(AID.Stardiver, sdTarget, sdPrio);
                }

                //Jump
                var jump = strategy.Option(Track.Jump);
                var jumpStrat = jump.As<CommonStrategy>();
                var jumpTarget = SingleTargetChoice(mainTarget, jump);
                var (jumpCondition, jumpPrio) = ShouldUseCommons(jumpStrat, jumpTarget, ActionReady(AID.Jump), ShouldDive(jumpTarget) && ((Unlocked(AID.Geirskogul) ? (HasLC && HasLOTD) : HasLC) || LCcd is < 35 and > 13));
                if (jumpCondition)
                    QueueOGCD(Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump, jumpTarget, jumpPrio + 5);

                //Geirskogul
                var gsk = strategy.Option(Track.Geirskogul);
                var gskStrat = gsk.As<CommonStrategy>();
                var gskTarget = AOETargetChoice(mainTarget, BestSpearTarget?.Actor, gsk, strategy);
                var (gskCondition, gskPrio) = ShouldUseCommons(gskStrat, gskTarget, ActionReady(AID.Geirskogul), ShouldSpear(gskTarget) && HasLC);
                if (gskCondition)
                    QueueOGCD(AID.Geirskogul, gskTarget, gskPrio + 7);

                //Starcross
                var sc = strategy.Option(Track.Starcross);
                var scStrat = sc.As<OGCDStrategy>();
                var scTarget = AOETargetChoice(mainTarget, BestDiveTarget?.Actor, sc, strategy);
                var scCondition = ShouldUseOGCD(scStrat, scTarget, Unlocked(AID.Starcross) && HasEffect(SID.StarcrossReady), ShouldDive(scTarget) && CanWeaveIn);
                if (scCondition)
                    QueueOGCD(AID.Starcross, scTarget, OGCDPrio(scStrat, OGCDPriority.BelowAverage + 4));

                //Nastrond
                var nast = strategy.Option(Track.Nastrond);
                var nastStrat = nast.As<OGCDStrategy>();
                var nastTarget = AOETargetChoice(mainTarget, BestSpearTarget?.Actor, nast, strategy);
                var nastCondition = ShouldUseOGCD(nastStrat, nastTarget, Unlocked(AID.Nastrond) && HasEffect(SID.NastrondReady), ShouldSpear(nastTarget) && CanWeaveIn);
                if (nastCondition)
                    QueueOGCD(AID.Nastrond, nastTarget, OGCDPrio(nastStrat, OGCDPriority.Average + 3));

                //Rise of the Dragon
                var rotd = strategy.Option(Track.RiseOfTheDragon);
                var rotdStrat = rotd.As<OGCDStrategy>();
                var rotdTarget = AOETargetChoice(mainTarget, BestDiveTarget?.Actor, rotd, strategy);
                var rotdCondition = ShouldUseOGCD(rotdStrat, rotdTarget, Unlocked(AID.RiseOfTheDragon) && HasEffect(SID.DragonsFlight), ShouldDive(rotdTarget) && CanWeaveIn);
                if (rotdCondition)
                    QueueOGCD(AID.RiseOfTheDragon, rotdTarget, OGCDPrio(rotdStrat, OGCDPriority.BelowAverage + 2));

                //Mirage Dive
                var md = strategy.Option(Track.MirageDive);
                var mdStrat = md.As<FlexStrategy>();
                var mdTarget = SingleTargetChoice(mainTarget, md);
                var mdMinimum = InCombat(mdTarget) && In20y(mdTarget) && HasEffect(SID.DiveReady);
                var (mdCondition, mdPrio) = mdStrat switch
                {
                    FlexStrategy.ASAP => (mdMinimum && CanWeaveIn, OGCDPriority.Average),
                    FlexStrategy.Late => (mdMinimum && StatusRemaining(Player, SID.DiveReady) <= 2.5f, OGCDPriority.ExtremelyHigh),
                    FlexStrategy.LateOrBuffed => (mdMinimum && (StatusRemaining(Player, SID.DiveReady) <= 2.5f || RaidBuffsLeft > 0 || (HasLC && HasLOTD)), OGCDPriority.ExtremelyHigh),
                    _ => (false, OGCDPriority.None),
                };
                if (mdCondition && CanWeaveIn)
                    QueueOGCD(AID.MirageDive, mdTarget, mdPrio);

                //Elusive Jump
                if (ActionReady(AID.ElusiveJump))
                {
                    var elusive = strategy.Option(Track.ElusiveJump).As<ElusiveDirection>();
                    var angle = elusive switch
                    {
                        ElusiveDirection.CharacterForward => Player.Rotation,
                        ElusiveDirection.CameraForward => World.Client.CameraAzimuth,
                        ElusiveDirection.CameraBackward => World.Client.CameraAzimuth + 180.Degrees(),
                        _ => Player.Rotation + 180.Degrees()
                    };
                    if (elusive != ElusiveDirection.None)
                        Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.ElusiveJump), Player, ActionQueue.Priority.Medium, facingAngle: angle);
                }

                //True North
                if (CanTrueNorth)
                {
                    var tn = strategy.Option(Track.TrueNorth);
                    var tnStrat = tn.As<TrueNorthStrategy>();
                    var condition = InCombat(mainTarget) && !WantAOE && In3y(mainTarget) && NextPositionalImminent && !NextPositionalCorrect;
                    var needRear = !IsOnRear(mainTarget!) && ((Unlocked(AID.ChaosThrust) && ComboLastMove is AID.Disembowel or AID.SpiralBlow) || (Unlocked(AID.WheelingThrust) && ComboLastMove is AID.ChaosThrust or AID.ChaoticSpring));
                    var needFlank = !IsOnFlank(mainTarget!) && Unlocked(AID.FangAndClaw) && ComboLastMove is AID.HeavensThrust or AID.FullThrust;
                    var (tnCondition, tnPrio) = tnStrat switch
                    {
                        TrueNorthStrategy.Automatic => (condition && CanLateWeaveIn, OGCDPriority.AboveAverage + 1000),
                        TrueNorthStrategy.ASAP => (condition, OGCDPriority.AboveAverage + 2000),
                        TrueNorthStrategy.Flank => (condition && CanLateWeaveIn && needFlank, OGCDPriority.AboveAverage),
                        TrueNorthStrategy.Rear => (condition && CanLateWeaveIn && needRear, OGCDPriority.AboveAverage),
                        TrueNorthStrategy.Force => (!HasEffect(SID.TrueNorth), OGCDPriority.AboveAverage + 2000),
                        _ => (false, OGCDPriority.None)
                    };
                    if (tnCondition)
                        QueueOGCD(AID.TrueNorth, Player, tnPrio);
                }

                //pots
                if (strategy.Potion() switch
                {
                    PotionStrategy.AlignWithBuffs => Player.InCombat && LCcd <= 2 && BLcd <= 3,
                    PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 3 || RaidBuffsLeft > 0),
                    PotionStrategy.Immediate => true,
                    _ => false
                })
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.High);
            }

            //gauge
            if (!strategy.HoldGauge())
            {
                //Wyrmwind Thrust
                var wt = strategy.Option(Track.WyrmwindThrust);
                var wtStrat = wt.As<FlexStrategy>();
                var wtTarget = AOETargetChoice(mainTarget, BestSpearTarget?.Actor, wt, strategy);
                var wtMinimum = ShouldSpear(wtTarget) && Gauge.FirstmindsFocusCount == 2;
                var (wtCondition, wtPrio) = wtStrat switch
                {
                    FlexStrategy.ASAP => (wtMinimum && CanWeaveIn, OGCDPriority.Average + 1),
                    FlexStrategy.Late => (wtMinimum && HasEffect(SID.DraconianFire), LatePrio()),
                    FlexStrategy.LateOrBuffed => (wtMinimum && (HasEffect(SID.DraconianFire) || (HasEffect(SID.LanceCharge) && CanWeaveIn)), HasEffect(SID.LanceCharge) ? OGCDPriority.Average + 1 : LatePrio()),
                    _ => (false, OGCDPriority.None)
                };
                if (wtCondition)
                    QueueOGCD(AID.WyrmwindThrust, wtTarget, wtPrio);
            }
        }

        //Piercing Talon
        if (Unlocked(AID.PiercingTalon))
        {
            var pt = strategy.Option(Track.PiercingTalon);
            var ptStrat = pt.As<PiercingTalonStrategy>();
            var ptTarget = SingleTargetChoice(mainTarget, pt);
            var allow = InCombat(ptTarget) && !In3y(ptTarget);
            var (ptCondition, ptPrio) = ptStrat switch
            {
                PiercingTalonStrategy.AllowEX => (allow && HasEffect(SID.EnhancedPiercingTalon), GCDPriority.Low),
                PiercingTalonStrategy.Allow => (allow, GCDPriority.Low),
                PiercingTalonStrategy.ForceEX => (HasEffect(SID.EnhancedPiercingTalon), GCDPriority.Low + 1),
                PiercingTalonStrategy.Force => (true, GCDPriority.Low + 1),
                _ => (false, GCDPriority.None)
            };
            if (ptCondition)
                QueueGCD(AID.PiercingTalon, ptTarget, ptPrio);
        }

        //standard rotation
        var stTarget = SingleTargetChoice(mainTarget, aoe);
        var ttTarget = AOETargetChoice(mainTarget, BestDOTTarget?.Actor, aoe, strategy);
        var aoesTarget = AOETargetChoice(mainTarget, BestAOETarget?.Actor, aoe, strategy);
        var bestTarget = WantAOE ? aoesTarget : AutoTarget ? ttTarget : stTarget;
        var (aoeAction, aoeTarget) = aoeStrat switch
        {
            AOEStrategy.AutoFinish => (AutoFinish, bestTarget),
            AOEStrategy.ForceSTFinish => (FullSTFinish, ttTarget),
            AOEStrategy.ForceBuffsFinish => (BuffsSTFinish, ttTarget),
            AOEStrategy.ForceNormalFinish => (NormalSTFinish, stTarget),
            AOEStrategy.ForceAOEFinish => (FullAOEFinish, aoesTarget),
            AOEStrategy.AutoBreak => (AutoBreak, bestTarget),
            AOEStrategy.ForceSTBreak => (FullSTBreak, ttTarget),
            AOEStrategy.ForceBuffsBreak => (BuffsSTBreak, ttTarget),
            AOEStrategy.ForceNormalBreak => (NormalSTBreak, stTarget),
            AOEStrategy.ForceAOEBreak => (FullAOEBreak, aoesTarget),
            _ => (AID.None, null)
        };
        if (aoeTarget != null)
            QueueGCD(aoeAction, aoeTarget, GCDPriority.Low);

        //targeting
        if (AutoTarget)
        {
            if (Unlocked(AID.ChaosThrust) &&
                BestDOTTargets != null &&
                In3y(BestDOTTargets.Actor) &&
                ComboLastMove is AID.Disembowel or AID.SpiralBlow)
            {
                Hints.ForcedTarget = BestDOTTargets.Actor;
            }
            if (BestDOTTargets == null || (WantAOE ? !In10y(BestAOETarget?.Actor) : !In3y(BestDOTTarget?.Actor)))
            {
                GetNextTarget(strategy, ref primaryTarget, 3);
            }
        }

        //positionals
        var pos = GetBestPositional(strategy, primaryTarget);
        UpdatePositionals(primaryTarget, ref pos);

        //ai
        if (primaryTarget != null)
        {
            GoalZoneCombined(strategy, 3, Hints.GoalAOERect(primaryTarget.Actor, 10, 2), AID.DoomSpike, minAoe: 3, maximumActionRange: 20);
        }
    }
}
