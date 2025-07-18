﻿using BossMod.RPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class RPR(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player, PotionType.Strength)
{
    public enum Track { Harpe = SharedTrack.Count, Crest }

    public enum HarpeStrategy
    {
        Automatic,
        Forbid,
        Ranged,
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan RPR", "Reaper", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.RPR), 100);

        def.DefineShared().AddAssociatedActions(AID.ArcaneCircle);
        def.Define(Track.Harpe).As<HarpeStrategy>("Harpe")
            .AddOption(HarpeStrategy.Automatic, "Use out of melee range if Enhanced Harpe is active")
            .AddOption(HarpeStrategy.Forbid, "Don't use")
            .AddOption(HarpeStrategy.Ranged, "Use out of melee range");

        def.AbilityTrack(Track.Crest, "Crest", "Arcane Crest").AddAssociatedActions(AID.ArcaneCrest);

        return def;
    }

    public int RedGauge;
    public int BlueGauge;
    public bool Soulsow;
    public float EnshroudLeft;
    public int BlueSouls;
    public int PurpleSouls;
    public float SoulReaver;
    public float EnhancedGallows;
    public float EnhancedGibbet;
    public (float Left, int Stacks) ImmortalSacrifice;
    public float BloodsownCircle;
    public float IdealHost;
    public float EnhancedVoidReaping;
    public float EnhancedCrossReaping;
    public float EnhancedHarpe;
    public float Oblatio;
    public float Executioner;
    public float PerfectioParata;

    public float TargetDDLeft;
    public float ShortestNearbyDDLeft;

    public int NumAOETargets; // melee
    public int NumRangedAOETargets; // gluttony, communio
    public int NumConeTargets; // grim swathe, guillotine
    public int NumLineTargets; // plentiful harvest

    private Enemy? BestRangedAOETarget;
    private Enemy? BestConeTarget;
    private Enemy? BestLineTarget;

    public enum GCDPriority
    {
        None = 0,
        Soulsow = 1,
        EnhancedHarpe = 100,
        HarvestMoon = 150,
        Filler = 200,
        FillerAOE = 201,
        SoulSlice = 300,
        DDExtend = 600,
        Harvest = 650,
        Reaver = 700,
        Lemure = 700,
        Communio = 750,
        DDExpiring = 900,
    }

    private bool Enshrouded => BlueSouls > 0;

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<ReaperGauge>();

        RedGauge = gauge.Soul;
        BlueGauge = gauge.Shroud;
        EnshroudLeft = gauge.EnshroudedTimeRemaining * 0.001f;
        BlueSouls = gauge.LemureShroud;
        PurpleSouls = gauge.VoidShroud;

        Soulsow = Player.FindStatus(SID.Soulsow) != null;
        SoulReaver = StatusLeft(SID.SoulReaver);
        EnhancedGallows = StatusLeft(SID.EnhancedGallows);
        EnhancedGibbet = StatusLeft(SID.EnhancedGibbet);
        ImmortalSacrifice = Status(SID.ImmortalSacrifice);
        BloodsownCircle = StatusLeft(SID.BloodsownCircle);
        IdealHost = StatusLeft(SID.IdealHost);
        EnhancedVoidReaping = StatusLeft(SID.EnhancedVoidReaping);
        EnhancedCrossReaping = StatusLeft(SID.EnhancedCrossReaping);
        EnhancedHarpe = StatusLeft(SID.EnhancedHarpe);
        Oblatio = StatusLeft(SID.Oblatio);
        Executioner = StatusLeft(SID.Executioner);
        PerfectioParata = StatusLeft(SID.PerfectioParata);

        TargetDDLeft = DDLeft(primaryTarget);
        ShortestNearbyDDLeft = float.MaxValue;

        switch (strategy.AOE())
        {
            case AOEStrategy.AOE:
            case AOEStrategy.ForceAOE:
                var nearbyDD = Hints.PriorityTargets.Where(x => Hints.TargetInAOECircle(x.Actor, Player.Position, 5)).Select(DDLeft);
                var minNeeded = strategy.AOE() == AOEStrategy.ForceAOE ? 1 : 3;
                if (MinIfEnoughElements(nearbyDD.Where(x => x < 30), minNeeded) is float m)
                    ShortestNearbyDDLeft = m;
                break;
        }

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 8, (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 90.Degrees()));
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);

        var pos = GetNextPositional(primaryTarget?.Actor);
        UpdatePositionals(primaryTarget, ref pos);

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.Harpe))
                PushGCD(AID.Harpe, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.SpinningScythe, 3, maximumActionRange: 25);

        if (SoulReaver > GCD || Executioner > GCD)
        {
            var gib = Executioner > GCD ? AID.ExecutionersGibbet : AID.Gibbet;
            var gal = Executioner > GCD ? AID.ExecutionersGallows : AID.Gallows;
            var gui = Executioner > GCD ? AID.ExecutionersGuillotine : AID.Guillotine;

            if (NumConeTargets > 2)
                PushGCD(gui, BestConeTarget, GCDPriority.Reaver);

            if (primaryTarget != null)
            {
                if (EnhancedGallows > GCD)
                    PushGCD(gal, primaryTarget, GCDPriority.Reaver);
                else if (EnhancedGibbet > GCD)
                    PushGCD(gib, primaryTarget, GCDPriority.Reaver);
                else if (GetCurrentPositional(primaryTarget.Actor) == Positional.Rear)
                    PushGCD(gal, primaryTarget, GCDPriority.Reaver);
                else
                    PushGCD(gib, primaryTarget, GCDPriority.Reaver);
            }

            return; // every other GCD breaks soul reaver
        }

        if (!Player.InCombat)
        {
            if (!Soulsow)
                PushGCD(AID.SoulSow, Player, GCDPriority.Soulsow);

            // if we exit combat while casting, cancel it so we get instant cast instead
            if (Player.CastInfo?.Action.ID == (uint)AID.SoulSow)
                Hints.ForceCancelCast = true;
        }

        switch (strategy.Option(Track.Harpe).As<HarpeStrategy>())
        {
            case HarpeStrategy.Automatic:
                if (EnhancedHarpe > GCD)
                    PushGCD(AID.Harpe, primaryTarget, GCDPriority.EnhancedHarpe);
                break;
            case HarpeStrategy.Ranged:
                PushOGCD(AID.Harpe, primaryTarget, 50);
                break;
        }

        if (Soulsow)
            PushGCD(AID.HarvestMoon, BestRangedAOETarget, GCDPriority.HarvestMoon);

        DDRefresh(primaryTarget);

        if (PerfectioParata > GCD)
            PushGCD(AID.Perfectio, BestRangedAOETarget, GCDPriority.Communio);

        EnshroudGCDs(strategy, primaryTarget);

        if (ImmortalSacrifice.Stacks > 0 && BloodsownCircle == 0)
            PushGCD(AID.PlentifulHarvest, BestLineTarget, GCDPriority.Harvest);

        // other GCDs are all disabled during enshroud
        if (Enshrouded)
            return;

        // manually check CD since queue will be delayed in certain circumstances otherwise
        if (RedGauge <= 50 && ReadyIn(AID.SoulSlice) <= GCD)
        {
            if (NumConeTargets > 2)
                PushGCD(AID.SoulScythe, BestConeTarget, GCDPriority.SoulSlice);

            PushGCD(AID.SoulSlice, primaryTarget, GCDPriority.SoulSlice);
        }

        if (NumAOETargets > 2)
        {
            if (ComboLastMove == AID.SpinningScythe)
                PushGCD(AID.NightmareScythe, Player, GCDPriority.FillerAOE);

            PushGCD(AID.SpinningScythe, Player, GCDPriority.FillerAOE);
        }

        if (ComboLastMove == AID.WaxingSlice)
            PushGCD(AID.InfernalSlice, primaryTarget, GCDPriority.Filler);

        if (ComboLastMove == AID.Slice)
            PushGCD(AID.WaxingSlice, primaryTarget, GCDPriority.Filler);

        PushGCD(AID.Slice, primaryTarget, GCDPriority.Filler);
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        if (strategy.BuffsOk())
        {
            // wait for soul slice in opener
            if (OnCooldown(AID.SoulSlice) || CombatTimer > 60)
                PushOGCD(AID.ArcaneCircle, Player, delay: GCD - 1.6f);
        }

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, delay: GCD - 0.8f);

        if (SoulReaver > 0 || Executioner > 0)
            return;

        if (Oblatio > 0 && (RaidBuffsLeft > 0 || ReadyIn(AID.ArcaneCircle) > EnshroudLeft))
            PushOGCD(AID.Sacrificium, BestRangedAOETarget);

        if (PurpleSouls > 1)
        {
            if (NumConeTargets > 2)
                PushOGCD(AID.LemuresScythe, BestConeTarget);

            PushOGCD(AID.LemuresSlice, primaryTarget);
        }

        if (ShouldEnshroud(strategy))
            PushOGCD(AID.Enshroud, Player);

        UseSoul(strategy, primaryTarget);

        if (strategy.Enabled(Track.Crest) && Hints.PredictedDamage.Any(p => p.Players[0] && p.Activation < World.FutureTime(5)))
            PushOGCD(AID.ArcaneCrest, Player);
    }

    private void DDRefresh(Enemy? primaryTarget)
    {
        void Extend(float timer, AID action, Enemy? target)
        {
            if (!CanFitGCD(timer, CanWeave(AID.Gluttony) ? 2 : 1))
                PushGCD(action, target, GCDPriority.DDExpiring);

            if (timer < 30 && CanWeave(AID.ArcaneCircle, 1) && OnCooldown(AID.SoulSlice))
                PushGCD(action, target, GCDPriority.DDExtend);
        }

        Extend(ShortestNearbyDDLeft, AID.WhorlofDeath, null);
        Extend(TargetDDLeft, AID.ShadowofDeath, primaryTarget);
    }

    private bool ShouldEnshroud(StrategyValues strategy)
    {
        if (Enshrouded)
            return false;

        if (IdealHost > 0)
            return true;

        if (BlueGauge < 50)
            return false;

        if (RaidBuffsLeft > GCD + 6)
            return true;

        if (CanWeave(AID.ArcaneCircle, 2, extraFixedDelay: 1.5f)) // stupid fixed recast timer
            return true;

        // TODO tweak deadline, i need a simulator or something
        return ReadyIn(AID.ArcaneCircle) > 65;
    }

    private void UseSoul(StrategyValues strategy, Enemy? primaryTarget)
    {
        // can't
        if (RedGauge < 50 || Enshrouded)
            return;

        // don't, it would delay Plentiful Harvest
        if (ImmortalSacrifice.Stacks > 0 && CanWeave(BloodsownCircle, 0.6f, 1))
            return;

        // don't, we would immediately overwrite Soul Reaver with Perfectio
        if (CanFitGCD(PerfectioParata))
            return;

        var debuffLeft = Math.Min(TargetDDLeft, ShortestNearbyDDLeft);

        if (CanFitGCD(debuffLeft, 2))
            PushOGCD(AID.Gluttony, BestRangedAOETarget);

        // can't, we need to refresh debuff first and using sod removes Soul Reaver
        if (!CanFitGCD(debuffLeft, 1))
            return;

        // use in raidbuffs
        // use to get blue gauge, we need 50 before each 2min window
        var spendEarly = RaidBuffsLeft > 0 || BlueGauge < 50;
        var gluttonySoon = CanWeave(AID.Gluttony, 5);

        if (RedGauge == 100 || spendEarly && !gluttonySoon)
        {
            if (NumConeTargets > 2)
                PushOGCD(AID.GrimSwathe, BestConeTarget);

            if (primaryTarget?.Priority >= 0)
                PushOGCD(AID.BloodStalk, primaryTarget);
        }
    }

    private void EnshroudGCDs(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (BlueSouls == 0)
            return;

        if (BlueSouls == 1)
            PushGCD(AID.Communio, BestRangedAOETarget, GCDPriority.Communio);

        var prio = GCDPriority.Lemure;

        if (CanWeave(AID.ArcaneCircle, 2, extraFixedDelay: 1.5f) && BlueSouls < 5)
            prio = GCDPriority.None;

        if (RaidBuffsLeft > 0)
            prio = GCDPriority.Lemure;

        if (NumConeTargets > 2 && prio > 0)
            PushGCD(AID.GrimReaping, BestConeTarget, prio + 1);

        PushGCD(EnhancedCrossReaping > GCD ? AID.CrossReaping : AID.VoidReaping, primaryTarget, prio);
    }

    protected override float GetCastTime(AID aid)
    {
        if (aid == AID.Harpe && EnhancedHarpe > GCD)
            return 0;

        if (aid == AID.SoulSow && !Player.InCombat)
            return 0;

        return base.GetCastTime(aid);
    }

    private (Positional, bool) GetNextPositional(Actor? primaryTarget)
    {
        if (primaryTarget == null || !Unlocked(AID.Gibbet) || NumConeTargets > 2)
            return (Positional.Any, false);

        Positional nextPos;

        if (EnhancedGallows > 0)
            nextPos = Positional.Rear;
        else if (EnhancedGibbet > 0)
            nextPos = Positional.Flank;
        else
        {
            var closest = GetCurrentPositional(primaryTarget);
            nextPos = closest == Positional.Front ? Positional.Flank : closest;
        }

        return (nextPos, SoulReaver > GCD || Executioner > GCD);
    }

    private float DDLeft(Enemy? target)
        => (target?.ForbidDOTs ?? false)
            ? float.MaxValue
            : StatusDetails(target?.Actor, SID.DeathsDesign, Player.InstanceID, 30).Left;

    private float? MinIfEnoughElements(IEnumerable<float> collection, int minElements)
    {
        float min = float.MaxValue;
        var elements = 0;
        foreach (var flt in collection)
        {
            elements++;
            min = MathF.Min(flt, min);
        }

        return elements >= minElements ? min : null;
    }
}
