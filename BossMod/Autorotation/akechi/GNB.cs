using BossMod.GNB;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using System.Reflection;

namespace BossMod.Autorotation.akechi;

public sealed class GNB(RotationModuleManager manager, Actor player) : xbase<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs, Partner }
    public enum PartnerStrategy { Automatic, Manual }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("GNB", "Gunbreaker", "Akechi-kun", RotationModuleQuality.Basic, BitMask.Build(Class.GNB), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.NoMercy);

        return def;
    }

    public int Ammo; // 0 to 3 (cartridges)
    public int GunComboStep; // 0 to 2
    public bool NoMercy; // 0s if buff not up, max 20s
    public bool ReadyToRip; // 0s if buff not up, max 10s
    public bool ReadyToTear; // 0s if buff not up, max 10s
    public bool ReadyToGouge; // 0s if buff not up, max 10s
    public bool ReadyToBlast; // 0s if buff not up, max 10s
    public bool ReadyToRaze; // 0s if buff not up, max 10s
    public bool ReadyToBreak; // 0s if buff not up, max 30s
    public bool ReadyToReign; // 0s if buff not up, max 30s
    public float NoMercyLeft; // 0s if buff not up, max 20s
    public float AuroraLeft; // 0s if mit not up, max 20s
    public float ReadyToRazeLeft; // 0 if buff not up, max 10
    public float ReadyToBreakLeft; // 0 if buff not up, max 30
    public float ReadyToReignLeft; // 0 if buff not up, max 30
    public int NumTargetsHitByAOE;
    public int MaxCartridges;
    public int NumAOETargets;

    // upgrade paths
    public AID BestZone => Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
    public AID BestHeart => Unlocked(AID.HeartOfCorundum) ? AID.HeartOfCorundum : AID.HeartOfStone;
    public AID BestContinuation => ReadyToRip ? AID.JugularRip : ReadyToTear ? AID.AbdomenTear : ReadyToGouge ? AID.EyeGouge : ReadyToBlast ? AID.Hypervelocity : AID.Continuation;
    public AID BestGnash => GunComboStep == 1 ? AID.SavageClaw : GunComboStep == 2 ? AID.WickedTalon : AID.GnashingFang;
    public AID ComboLastMove => 

    public bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    public bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);


    private int GetSTComboLength(AID comboLastMove) => comboLastMove switch
    {
        AID.BrutalShell => 1,
        AID.KeenEdge => 2,
        _ => 3
    };

    private AID GetNextBrutalShellComboAction(AID comboLastMove)
    {
        if (comboLastMove == AID.DemonSlice)
        {
            return AID.DemonSlaughter;
        }
        else if (comboLastMove == AID.BrutalShell)
        {
            return AID.SolidBarrel;
        }
        else if (comboLastMove == AID.KeenEdge)
        {
            return AID.BrutalShell;
        }

        return AID.KeenEdge;
    }

    private AID GetNextAOEComboAction(AID comboLastMove)
    {
        if (comboLastMove == AID.DemonSlice)
        {
            return AID.DemonSlaughter;
        }
        else if (comboLastMove == AID.BrutalShell)
        {
            return AID.SolidBarrel;
        }
        else if (comboLastMove == AID.KeenEdge)
        {
            return AID.BrutalShell;
        }

        return AID.DemonSlice;
    }

    private const float NMLateWeave = 0.6f;

    protected override float GetCastTime(AID aid) => 0;

    private bool HaveTarget => NumAOETargets > 1 || _state.TargetingEnemy;

    private void GetNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        // prepull
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            PushGCD(AID.None, Player);

        if (_state.RangeToTarget > 3)
            PushGCD(AID.LightningShot, Player);

        if (_state.CD(AID.NoMercy) is <= 60 or >= 40)
        {
            // Lv100 NM window
            if (Unlocked(AID.ReignOfBeasts))
            {
                if (Unlocked(AID.SonicBreak) && ShouldUseSonic(strategy))
                {
                    if ((ReadyToBreakLeft > 17.5 && Unlocked(AID.SonicBreak) && Ammo == 0
                        && _state.CD(AID.Bloodfest) > 50 && !ReadyToRip
                        && ComboLastMove == AID.GnashingFang || _state.CD(AID.GnashingFang) < 28) // SB 1min 2cart
                        || (ReadyToBreakLeft >= 28 && Unlocked(AID.SonicBreak) && Ammo == 3
                        && _state.CD(AID.Bloodfest) > 50) // SB 1min 3cart
                        || (ReadyToBreakLeft >= 28 && Unlocked(AID.SonicBreak) && Ammo == 2
                        && _state.CD(AID.Bloodfest) < 30))
                        PushGCD(AID.SonicBreak, Player);
                }

                if (Unlocked(AID.DoubleDown))
                {
                    // DD 1min 2cart
                    if ((_state.CD(AID.DoubleDown) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Ammo == 3 && _state.RangeToTarget <= 5
                        && ComboLastMove == AID.SolidBarrel && NoMercyLeft < 17.5) // DD 1min 2cart
                        || (_state.CD(AID.DoubleDown) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Ammo == 3 && _state.RangeToTarget <= 5
                        && ComboLastMove == AID.SonicBreak && NoMercyLeft < 17.5 && ReadyToBreakLeft == 0) // DD 1min 3cart
                        || (_state.CD(AID.DoubleDown) < 0.6f && _state.CD(AID.Bloodfest) < 10
                        && Ammo >= 2 && _state.RangeToTarget <= 5
                        && ComboLastMove == AID.SonicBreak && NoMercyLeft < 17.5 && ReadyToBreakLeft == 0)) // DD 2min 2cart
                        PushGCD(AID.DoubleDown, Player);
                }

                if (Unlocked(AID.GnashingFang))
                {
                    if ((_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5) // GF 1min
                        || (_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) < 10
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5)) // GF 2min
                        PushGCD(AID.DoubleDown, Player);
                }

                if (ReadyToReignLeft > 30 && _state.CD(AID.DoubleDown) >= 40 && _state.CD(AID.GnashingFang) >= 17 && GunComboStep == 0)
                {
                    // Reign
                    if (Unlocked(AID.ReignOfBeasts) && GunComboStep == 0 && NoMercyLeft < 7.5 && _state.CD(AID.Bloodfest) >= 100 && _state.CD(AID.GnashingFang) > 17 && _state.CD(AID.DoubleDown) >= 40)
                        PushGCD(AID.ReignOfBeasts, Player);
                    if (Unlocked(AID.NobleBlood) && GunComboStep == 0 && ComboLastMove == AID.ReignOfBeasts && NoMercyLeft < 5 && _state.CD(AID.Bloodfest) >= 100 && _state.CD(AID.GnashingFang) > 15 && _state.CD(AID.DoubleDown) >= 40)
                        PushGCD(AID.DoubleDown, Player);
                    if (Unlocked(AID.LionHeart) && GunComboStep == 0 && ComboLastMove == AID.NobleBlood && NoMercyLeft < 2.5 && _state.CD(AID.Bloodfest) >= 100 && _state.CD(AID.GnashingFang) > 15 && _state.CD(AID.DoubleDown) >= 40)
                        PushGCD(AID.DoubleDown, Player);
                }
            }

            // Lv90 NM window
            if (!Unlocked(AID.ReignOfBeasts) && Unlocked(AID.DoubleDown))
            {
                if (Unlocked(AID.SonicBreak) && ShouldUseSonic(strategy))
                {
                    if ((ReadyToBreakLeft > 17.5 && Unlocked(AID.SonicBreak) && Ammo == 0
                        && _state.CD(AID.Bloodfest) > 50 && !ReadyToRip
                        && ComboLastMove == AID.GnashingFang || _state.CD(AID.GnashingFang) < 28) // SB 1min 2cart
                        || (ReadyToBreakLeft >= 28 && Unlocked(AID.SonicBreak) && Ammo == 3
                        && _state.CD(AID.Bloodfest) > 50) // SB 1min 3cart
                        || (ReadyToBreakLeft >= 28 && Unlocked(AID.SonicBreak) && Ammo == 2
                        && _state.CD(AID.Bloodfest) < 30)) // SB 2min 2cart
                        PushGCD(AID.SonicBreak, Player);
                }

                if (Unlocked(AID.DoubleDown) && ShouldUseDoubleDown(strategy))
                {
                    if ((_state.CD(AID.DoubleDown) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Unlocked(AID.DoubleDown) && Ammo == 3 && _state.RangeToTarget <= 5
                        && ComboLastMove == AID.SolidBarrel && NoMercyLeft < 17.5) // DD 1min 2cart
                        || (_state.CD(AID.DoubleDown) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Unlocked(AID.DoubleDown) && Ammo == 3 && _state.RangeToTarget <= 5
                        && ComboLastMove == AID.SonicBreak && NoMercyLeft < 17.5 && ReadyToBreakLeft == 0) // DD 1min 3cart
                        || (_state.CD(AID.DoubleDown) < 0.6f && _state.CD(AID.Bloodfest) < 10
                        && Unlocked(AID.DoubleDown) && Ammo >= 2 && _state.RangeToTarget <= 5
                        && ComboLastMove == AID.SonicBreak && NoMercyLeft < 17.5 && ReadyToBreakLeft == 0)) // DD 2min 2cart
                        PushGCD(AID.DoubleDown, Player);
                }

                if (Unlocked(AID.GnashingFang) && ShouldUseGnashingFang(strategy))
                {
                    // GF 1min
                    if (_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5)
                        return AID.GnashingFang;
                    // GF 2min
                    if (_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) < 10
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5)
                        return AID.GnashingFang;
                }
            }
        }

        // ST Logic 80 & below
        if (!aoe)
        {
            if (Ammo >= 2 && !Unlocked(AID.DoubleDown) &&
                !Unlocked(AID.Bloodfest) && !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) &&
                !Unlocked(AID.SonicBreak))
            {
                return AID.BurstStrike;
            }
        }
        // AOE Logic 80 & below
        else if (aoe)
        {
            if (Ammo >= 2)
            {
                if (Ammo >= 2)
                {
                    if (Unlocked(AID.GnashingFang) && _state.CD(AID.GnashingFang) == 0)
                    {
                        return AID.GnashingFang; // Lv60+ AOE GF
                    }
                    if (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown))
                    {
                        return AID.BurstStrike; // Lv30-72 AOE BS
                    }
                }
                if (Ammo >= 2 && !Unlocked(AID.DoubleDown) &&
                    !Unlocked(AID.Bloodfest) && !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) && !Unlocked(AID.SonicBreak))
                {
                    return AID.BurstStrike; // Lv30-53 AOE BS
                }
                if (Ammo >= 2 && Unlocked(AID.SonicBreak) && Unlocked(AID.GnashingFang) &&
                    !Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown))
                {
                    return AID.GnashingFang; // Lv60 AOE GF fix
                }
                else if (Ammo >= 2 && Unlocked(AID.SonicBreak) && Unlocked(AID.GnashingFang) && (_state.CD(AID.GnashingFang) > _state.AnimationLock && !Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown)))
                {
                    return AID.BurstStrike; // Lv60 AOE BS 
                }
            }
            if (Ammo >= 2 && _state.GunComboStep == 0)
            {
                if (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    Unlocked(AID.Continuation))
                {
                    return AID.BurstStrike; // Lv70 AOE BS
                }
                if (_state.CD(AID.GnashingFang) > _state.GCD && _state.CD(AID.DoubleDown) > _state.GCD &&
                    ReadyToBreakLeft >= 0 && Unlocked(AID.DoubleDown))
                {
                    return AID.FatedCircle; // Lv80 AOE
                }
                if (_state.CD(AID.GnashingFang) > _state.GCD && Unlocked(AID.FatedCircle) &&
                    !Unlocked(AID.DoubleDown) && !Unlocked(AID.SonicBreak))
                {
                    return AID.FatedCircle; // Lv80 AOE
                }
                if (Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) &&
                    !Unlocked(AID.SonicBreak) && !Unlocked(AID.GnashingFang))
                {
                    return AID.FatedCircle; // Lv80 AOE
                }
            }
        }

        if (ReadyToBlast)
            return _state.BestContinuation;
        if (ReadyToGouge)
            return _state.BestContinuation;
        if (ReadyToTear)
            return _state.BestContinuation;
        if (ReadyToRip)
            return _state.BestContinuation;

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.PenultimateComboThenSpend && ComboLastMove != AID.BrutalShell && ComboLastMove != AID.DemonSlice && (ComboLastMove != AID.BrutalShell || Ammo == _state.MaxCartridges) && _state.GunComboStep == 0)
            return aoe ? AID.DemonSlice : ComboLastMove == AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;

        if (Ammo >= _state.MaxCartridges && ComboLastMove == AID.BrutalShell)
            return GetNextAmmoAction(strategy, aoe);

        if (Ammo >= _state.MaxCartridges && ComboLastMove == AID.DemonSlice)
            return GetNextAmmoAction(strategy, aoe);

        if (NoMercyLeft > _state.AnimationLock)
            return GetNextAmmoAction(strategy, aoe);

        // GF
        if (_state.CD(AID.GnashingFang) < 0.6f)
            return GetNextAmmoAction(strategy, aoe);

        // GF2&3
        if (_state.GunComboStep > 0)
        {
            if (_state.GunComboStep == 2)
                return AID.WickedTalon;
            if (_state.GunComboStep == 1)
                return AID.SavageClaw;
        }

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.Spend)
            return GetNextAmmoAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.Hold)
            return GetNextUnlockedComboAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.ForceST)
            return GetNextUnlockedComboAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.ForceAOE)
            return GetNextUnlockedComboAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.ForceGF)
            return GetNextAmmoAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.ForceReign)
            return GetNextAmmoAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.MaxGaugeBeforeDowntime && NoMercyLeft < _state.AnimationLock)
            return ChooseRotationBasedOnGauge(strategy, aoe);

        return GetNextUnlockedComboAction(strategy, aoe);
    }


    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining is > 2 and < 10 && NextStep == 0 && PelotonLeft == 0 && Unlocked(AID.Peloton))
                PushOGCD(AID.Peloton, Player);

            return;
        }

        if (IsDancing)
            return;

        if (TechFinishLeft > _state.GCD && Unlocked(AID.Devilment) && _state.CanWeave(AID.Devilment, 0.6f, deadline))
            PushOGCD(AID.Devilment, Player);

        if (_state.CD(AID.Devilment) > 55 && _state.CanWeave(AID.Flourish, 0.6f, deadline))
            PushOGCD(AID.Flourish, Player);

        if ((TechFinishLeft == 0 || _state.CD(AID.Devilment) > 0) && ThreefoldLeft > _state.AnimationLock && NumRangedAOETargets > 0 && _state.CanWeave(AID.FanDanceIII, 0.6f, deadline))
            PushOGCD(AID.FanDanceIII, BestRangedAOETarget);

        var canF1 = ShouldSpendFeathers(strategy);
        var f1ToUse = NumAOETargets > 1 && Unlocked(AID.FanDanceII) ? AID.FanDanceII : AID.FanDance;

        if (Feathers == 4 && canF1)
            PushOGCD(f1ToUse, primaryTarget);

        if (_state.CD(AID.Devilment) > 0 && FourfoldLeft > _state.AnimationLock && NumFan4Targets > 0)
            PushOGCD(AID.FanDanceIV, BestFan4Target);

        if (canF1)
            PushOGCD(f1ToUse, primaryTarget);
    }


    //      //      //      //      //

    // NM plan
    private bool ShouldUseNoMercy(StrategyValues strategy)
    {
        if (!Unlocked(AID.NoMercy))
        {
            return false;
        }
        else if (_state.CD(AID.NoMercy) < 0.6f && Unlocked(AID.NoMercy))
        {
            return true;
        }
        else
        {
            bool justusewhenever = !Unlocked(AID.BurstStrike) && _state.TargetingEnemy && _state.RangeToTarget < 5;

            bool shouldUseNoMercy = (Player.InCombat && _state.TargetingEnemy && Unlocked(AID.NoMercy));

            if ((CombatTimer > 30 && ComboLastMove == AID.BrutalShell && Ammo == 0) // opener
                || (_state.CD(AID.NoMercy) <= 2.4 && Ammo >= 2) //1min
                || ((_state.CD(AID.NoMercy) <= 2.4 && Ammo == 2) && _state.CD(AID.Bloodfest) < 10)) //2min
                return shouldUseNoMercy || justusewhenever;
        }
    }

    // Sonic plan
    private bool ShouldUseSonic(StrategyValues strategy)
    {
        if (!Unlocked(AID.SonicBreak) || !NoMercy && !ReadyToBreak)
        {
            return false;
        }
        else if (Unlocked(AID.SonicBreak) && ReadyToBreak && NoMercy)
        {
            return true;
        }
        else
        {
            bool shouldUseSonicBreak = Player.InCombat && _state.TargetingEnemy && ((ReadyToBreak && _state.CD(AID.DoubleDown) < 5 && Ammo >= 3) && _state.CD(AID.Bloodfest) >= 40) //2cart
                || (ComboLastMove == AID.GnashingFang && _state.CD(AID.DoubleDown) <= 60 && Ammo == 0 && _state.CD(AID.Bloodfest) >= 40) // 2min 2cart
                || ((ReadyToBreakLeft > 27.5) && (_state.CD(AID.DoubleDown) < 5) && Ammo == 2
                && (_state.CD(AID.Bloodfest) >= 40) && (ComboLastMove == AID.BurstStrike) && (!ReadyToBlast));

            return shouldUseSonicBreak;
        }
    }

    // GF plan
    private bool ShouldUseGnashingFang(StrategyValues strategy)
    {
        if (!Unlocked(AID.GnashingFang))
        {
            return false;
        }
        else if (Unlocked(AID.GnashingFang) && Ammo >= 1 && (NoMercy || _state.CD(AID.GnashingFang) > 17))
        {
            return true;
        }
        return Player.InCombat && _state.TargetingEnemy && Unlocked(AID.GnashingFang) && _state.CD(AID.GnashingFang) < 0.6f && Ammo >= 1;
    }

    // Zone plan
    private bool ShouldUseZone(StrategyValues strategy) => strategy.Option(Track.Zone).As<OffensiveStrategy>() switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && Unlocked(AID.SonicBreak) && (NoMercyLeft < 17.5 || _state.CD(AID.NoMercy) > 17)
    };

    // Bloodfest plan
    private bool ShouldUseBloodfest(StrategyValues strategy) => strategy.As<OffensiveStrategy>() switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && (ComboLastMove == AID.DoubleDown) && (_state.CD(AID.Bloodfest) is < 10 or 0)
    };

    // BowShock plan
    private bool ShouldUseBowShock(StrategyValues strategy) => strategy.Option(Track.Bow).As<OffensiveStrategy>() switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && Unlocked(AID.BowShock) && (ComboLastMove == AID.SonicBreak) && _state.CD(AID.NoMercy) > 40
    };

    //      //      //      //      //
    private bool ShouldTechStep(StrategyValues strategy)
    {
        if (!Unlocked(AID.TechnicalStep) || _state.CD(AID.TechnicalStep) > _state.GCD || strategy.Option(Track.Buffs).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return false;

        const float TechStepDuration = 5.5f;
        const float TechFinishDuration = 20f;

        // standard finish must last for the whole burst window now, since tillana doesn't refresh it
        return NumDanceTargets > 0 && StandardFinishLeft > _state.GCD + TechStepDuration + TechFinishDuration;
    }

    private bool CanFlow(out AID action)
    {
        var act = NumAOETargets > 1 ? AID.Bloodshower : AID.Fountainfall;
        if (Unlocked(act) && FlowLeft > _state.GCD && HaveTarget)
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private bool CanSymmetry(out AID action)
    {
        var act = NumAOETargets > 1 ? AID.RisingWindmill : AID.ReverseCascade;
        if (Unlocked(act) && SymmetryLeft > _state.GCD && HaveTarget)
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private bool ShouldFinishDance(float danceTimeLeft)
    {
        if (NextStep != 0)
            return false;
        if (danceTimeLeft is > 0 and < FinishDanceWindow)
            return true;

        return danceTimeLeft > _state.GCD && NumDanceTargets > 0;
    }

    private bool ShouldSaberDance(StrategyValues strategy, int minimumEsprit)
    {
        if (Esprit < 50 || !Unlocked(AID.SaberDance))
            return false;

        return Esprit >= minimumEsprit && NumRangedAOETargets > 0;
    }

    private bool ShouldSpendFeathers(StrategyValues strategy)
    {
        if (Feathers == 0)
            return false;

        if (Feathers == 4 || !Unlocked(AID.TechnicalStep))
            return true;

        return TechFinishLeft > _state.AnimationLock;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);
        _state.HaveTankStance = Player.FindStatus(SID.RoyalGuard) != null;
        //if (ComboLastMove == AID.SolidBarrel)
        //    _state.ComboTimeLeft = 0;

        var gauge = GetGauge<GunbreakerGauge>();
        Ammo = gauge.Ammo;
        GunComboStep = gauge.AmmoComboStep;
        MaxCartridges = Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;

        NoMercyLeft = _state.StatusDetails(Player, SID.NoMercy, Player.InstanceID).Left;
        NoMercy = Player.FindStatus(SID.NoMercy) != null;
        ReadyToRip = Player.FindStatus(SID.ReadyToRip) != null;
        ReadyToTear = Player.FindStatus(SID.ReadyToTear) != null;
        ReadyToGouge = Player.FindStatus(SID.ReadyToGouge) != null;
        ReadyToBreak = Player.FindStatus(SID.ReadyToBreak) != null;
        ReadyToBreakLeft = _state.StatusDetails(Player, SID.ReadyToBreak, Player.InstanceID).Left;
        ReadyToReign = Player.FindStatus(SID.ReadyToReign) != null;
        ReadyToReignLeft = _state.StatusDetails(Player, SID.ReadyToReign, Player.InstanceID).Left;
        ReadyToBlast = Player.FindStatus(SID.ReadyToBlast) != null;
        ReadyToRaze = Player.FindStatus(SID.ReadyToRaze) != null;
        ReadyToRazeLeft = _state.StatusDetails(Player, SID.ReadyToRaze, Player.InstanceID).Left;
        AuroraLeft = _state.StatusDetails(Player, SID.Aurora, Player.InstanceID).Left;

        AID GCD = GetNextBestGCD(strategy, aoe);
        PushResult(GCD, primaryTarget);

        ActionID oGCD = default;
        var deadline = _state.GCD > 0 && GCD != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first oGCD slot
            oGCD = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength, aoe);
        if (!oGCD && _state.CanWeave(deadline)) // second/only oGCD slot
            oGCD = GetNextBestOGCD(strategy, deadline, aoe);
        PushResult(oGCD, primaryTarget);
    }
}
