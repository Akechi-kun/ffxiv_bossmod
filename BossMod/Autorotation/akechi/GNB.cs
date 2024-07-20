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
    public AID BestContinuation => ReadyToRip ? AID.JugularRip : ReadyToTear ? AID.AbdomenTear : ReadyToGouge ? AID.EyeGouge : ReadyToBlast ? AID.Hypervelocity : AID.Continuation;
    public AID ComboLastMove => (AID)World.Client.ComboState.Action;

    protected override float GetCastTime(AID aid) => 0;

    private void GetNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        // prepull
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            PushGCD(AID.None, Player);

        if (_state.RangeToTarget > 3)
            PushGCD(AID.LightningShot, primaryTarget);

        if (_state.CD(AID.NoMercy) is <= 60 or >= 40)
        {
            // GnashingFang
            if (Unlocked(AID.GnashingFang) && ShouldUseGnashingFang(strategy))
            {
                if (_state.CD(AID.GnashingFang) < 0.6f &&
                    (_state.CD(AID.NoMercy) > 57.5 ||
                    _state.CD(AID.NoMercy) > 17))
                    PushGCD(AID.GnashingFang, primaryTarget);
            }

            // DoubleDown
            if (Unlocked(AID.DoubleDown) && ShouldUseDoubleDown(strategy))
            {
                if (_state.CD(AID.DoubleDown) < 0.6f &&
                    _state.CD(AID.NoMercy) < 58 && _state.CD(AID.GnashingFang) > 15)
                    PushGCD(AID.DoubleDown, primaryTarget);
            }

            // Reign
            if (ReadyToReign && _state.CD(AID.DoubleDown) >= 40 && _state.CD(AID.GnashingFang) >= 17 && GunComboStep == 0)
            {
                if (Unlocked(AID.ReignOfBeasts) && GunComboStep == 0 && (ComboLastMove == AID.WickedTalon || ComboLastMove == AID.EyeGouge) && _state.CD(AID.Bloodfest) >= 90 && _state.CD(AID.DoubleDown) >= 40)
                    PushGCD(AID.ReignOfBeasts, primaryTarget);
                if (Unlocked(AID.NobleBlood) && GunComboStep == 0 && ComboLastMove == AID.ReignOfBeasts && _state.CD(AID.Bloodfest) >= 100)
                    PushGCD(AID.NobleBlood, primaryTarget);
                if (Unlocked(AID.LionHeart) && GunComboStep == 0 && ComboLastMove == AID.NobleBlood && _state.CD(AID.Bloodfest) >= 100)
                    PushGCD(AID.LionHeart, primaryTarget);
            }

            // BurstStrike
            if (Unlocked(AID.BurstStrike) && ShouldUseBurstStrike(strategy))
            {
                if (_state.CD(AID.DoubleDown) > 40 && _state.CD(AID.GnashingFang) > 10 && !ReadyToReign)
                    PushGCD(AID.BurstStrike, primaryTarget);
            }

            // SonicBreak
            if (Unlocked(AID.SonicBreak) && ShouldUseSonic(strategy))
            {
                if (ReadyToBreak && _state.CD(AID.NoMercy) <= 42)
                    PushGCD(AID.SonicBreak, primaryTarget);
            }

            /* Lv100 NM window
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
                        PushGCD(AID.SonicBreak, primaryTarget);
                }

                if (Unlocked(AID.DoubleDown) && ShouldUseDoubleDown(strategy))
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
                        PushGCD(AID.DoubleDown, primaryTarget);
                }

                if (Unlocked(AID.GnashingFang) && ShouldUseGnashingFang(strategy))
                {
                    if ((_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5) // GF 1min
                        || (_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) < 10
                        || (_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) < 10
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5))) // GF 2min
                        PushGCD(AID.DoubleDown, primaryTarget);
                }

                if (ReadyToReignLeft > 30 && _state.CD(AID.DoubleDown) >= 40 && _state.CD(AID.GnashingFang) >= 17 && GunComboStep == 0)
                {
                    // Reign
                    if (Unlocked(AID.ReignOfBeasts) && GunComboStep == 0 && NoMercyLeft < 7.5 && _state.CD(AID.Bloodfest) >= 100 && _state.CD(AID.GnashingFang) > 17 && _state.CD(AID.DoubleDown) >= 40)
                        PushGCD(AID.ReignOfBeasts, primaryTarget);
                    if (Unlocked(AID.NobleBlood) && GunComboStep == 0 && ComboLastMove == AID.ReignOfBeasts && NoMercyLeft < 5 && _state.CD(AID.Bloodfest) >= 100 && _state.CD(AID.GnashingFang) > 15 && _state.CD(AID.DoubleDown) >= 40)
                        PushGCD(AID.DoubleDown, primaryTarget);
                    if (Unlocked(AID.LionHeart) && GunComboStep == 0 && ComboLastMove == AID.NobleBlood && NoMercyLeft < 2.5 && _state.CD(AID.Bloodfest) >= 100 && _state.CD(AID.GnashingFang) > 15 && _state.CD(AID.DoubleDown) >= 40)
                        PushGCD(AID.DoubleDown, primaryTarget);
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
                        PushGCD(AID.SonicBreak, primaryTarget);
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
                        PushGCD(AID.DoubleDown, primaryTarget);
                }

                if (Unlocked(AID.GnashingFang) && ShouldUseGnashingFang(strategy))
                {
                    // GF 1min
                    if ((_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) > 50
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5)
                        || (_state.CD(AID.GnashingFang) < 0.6f && _state.CD(AID.Bloodfest) < 10
                        && Unlocked(AID.ReignOfBeasts) && Ammo == 1 && ReadyToBreakLeft == 0
                        && ComboLastMove == AID.DoubleDown && NoMercyLeft <= 12.5 && _state.CD(AID.DoubleDown) >= 57.5))
                        PushGCD(AID.GnashingFang, primaryTarget);
                }
            } */
        }

        // ST Logic 80 & below
        if (NumAOETargets == 1)
        {
            if (Ammo >= 1 && !Unlocked(AID.DoubleDown) &&
                !Unlocked(AID.Bloodfest) && !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) &&
                !Unlocked(AID.SonicBreak))
            {
                PushGCD(AID.BurstStrike, primaryTarget);
            }
        }
        // AOE Logic 80 & below
        else if (NumAOETargets >= 2)
        {
            if (Ammo >= 2)
            {
                if (Ammo >= 2)
                {
                    if (Unlocked(AID.GnashingFang) && _state.CD(AID.GnashingFang) == 0)
                    {
                        PushGCD(AID.GnashingFang, primaryTarget); // Lv60+ AOE GF
                    }
                    if (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown))
                    {
                        PushGCD(AID.BurstStrike, primaryTarget); // Lv30-72 AOE BS
                    }
                }
                if (Ammo >= 2 && !Unlocked(AID.DoubleDown) &&
                    !Unlocked(AID.Bloodfest) && !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) && !Unlocked(AID.SonicBreak))
                {
                    PushGCD(AID.BurstStrike, primaryTarget); // Lv30-53 AOE BS
                }
                if (Ammo >= 2 && Unlocked(AID.SonicBreak) && Unlocked(AID.GnashingFang) &&
                    !Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown))
                {
                    PushGCD(AID.GnashingFang, primaryTarget); // Lv60 AOE GF fix
                }
                else if (Ammo >= 2 && Unlocked(AID.SonicBreak) && Unlocked(AID.GnashingFang) && (_state.CD(AID.GnashingFang) > _state.AnimationLock && !Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown)))
                {
                    PushGCD(AID.BurstStrike, primaryTarget); // Lv60 AOE BS 
                }
            }
            if (Ammo >= 2 && GunComboStep == 0)
            {
                if (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    Unlocked(AID.Continuation))
                {
                    PushGCD(AID.BurstStrike, primaryTarget); // Lv70 AOE BS
                }
                if (_state.CD(AID.GnashingFang) > _state.GCD && _state.CD(AID.DoubleDown) > _state.GCD &&
                    ReadyToBreakLeft >= 0 && Unlocked(AID.DoubleDown))
                {
                    PushGCD(AID.FatedCircle, primaryTarget); // Lv80 AOE
                }
                if (_state.CD(AID.GnashingFang) > _state.GCD && Unlocked(AID.FatedCircle) &&
                    !Unlocked(AID.DoubleDown) && !Unlocked(AID.SonicBreak))
                {
                    PushGCD(AID.FatedCircle, primaryTarget); // Lv80 AOE
                }
                if (Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) &&
                    !Unlocked(AID.SonicBreak) && !Unlocked(AID.GnashingFang))
                {
                    PushGCD(AID.FatedCircle, primaryTarget); // Lv80 AOE
                }
            }
        }

        if (Unlocked(AID.Continuation) && (ReadyToBlast || ReadyToRaze || ReadyToGouge || ReadyToTear || ReadyToRip))
            PushOGCD(BestContinuation, primaryTarget);

        // GF2&3
        if (GunComboStep > 0)
        {
            if (GunComboStep == 2)
                PushGCD(AID.WickedTalon, primaryTarget);
            if (GunComboStep == 1)
                PushGCD(AID.SavageClaw, primaryTarget);
        }

        // 123/12
        if (ComboLastMove == AID.DemonSlice)
        {
            if (Ammo == MaxCartridges)
                PushGCD(AID.FatedCircle, primaryTarget);
            if (Ammo != MaxCartridges)
                PushGCD(AID.DemonSlaughter, primaryTarget);
        }

        if (ComboLastMove == AID.BrutalShell)
        {
            if (Ammo == MaxCartridges)
                PushGCD(AID.BurstStrike, primaryTarget);
            if (Ammo != MaxCartridges)
                PushGCD(AID.SolidBarrel, primaryTarget);
        }
        else if (ComboLastMove == AID.KeenEdge)
        {
            PushGCD(AID.BrutalShell, primaryTarget);
        }

        PushGCD(AID.KeenEdge, primaryTarget);
    }

    private void GetNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        /*if (ShouldUsePotion(strategy) && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
            return ActionDefinitions.IDPotionStr; */

        // No Mercy
        if (Unlocked(AID.NoMercy) && ShouldUseNoMercy(strategy, deadline))
        {
            if (_state.GCD < 0.8f)
            {
                if ((Unlocked(AID.Bloodfest) && ComboLastMove == AID.SolidBarrel && Ammo == 1 && CombatTimer < 30) // Opener conditions
                    || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) && !Unlocked(AID.SonicBreak) &&
                    Ammo == MaxCartridges) // subLv53
                    || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    !Unlocked(AID.Continuation) && Unlocked(AID.GnashingFang) && Unlocked(AID.SonicBreak) &&
                    Ammo == MaxCartridges) // Lv60
                    || (!Unlocked(AID.DoubleDown) && !Unlocked(AID.FatedCircle) && !Unlocked(AID.Bloodfest) &&
                    Unlocked(AID.Continuation) &&
                    Ammo == MaxCartridges) // Lv70
                    || (!Unlocked(AID.DoubleDown) && Unlocked(AID.FatedCircle) && Unlocked(AID.Bloodfest) && _state.CD(AID.GnashingFang) < _state.AnimationLock &&
                    Ammo == MaxCartridges) // Lv80
                    || (Unlocked(AID.DoubleDown) && Ammo == MaxCartridges)) // Lv90+
                    PushOGCD(AID.NoMercy, primaryTarget);
            }
        }

        // Bloodfest
        if (Unlocked(AID.Bloodfest) && ShouldUseBloodfest(strategy, deadline))
        {
            if (_state.CanWeave(AID.Bloodfest, 0.6f, deadline) && Ammo == 0 && ComboLastMove == AID.DoubleDown)
                PushOGCD(AID.Bloodfest, primaryTarget);
        }

        // Zone
        if (Unlocked(AID.DangerZone) && ShouldUseZone(strategy, deadline))
        {
            if (_state.CanWeave(AID.DangerZone, 0.6f, deadline) && (_state.CD(AID.NoMercy) <= 57 || _state.CD(AID.GnashingFang) > 17))
                PushOGCD(BestZone, primaryTarget);
        }

        // Bow Shock
        if (Unlocked(AID.BowShock) && ShouldUseBowShock(strategy, deadline))
        {
            if (_state.CanWeave(AID.BowShock, 0.6f, deadline) && _state.CD(AID.NoMercy) <= 57)
                PushOGCD(AID.BowShock, primaryTarget);
        }

        // Continuation
        if ((Unlocked(AID.Continuation) || Unlocked(AID.Hypervelocity)) && (ReadyToBlast || ReadyToRaze || ReadyToGouge || ReadyToTear || ReadyToRip))
            PushOGCD(BestContinuation, primaryTarget);

        // Aurora
        if (_state.CanWeave(_state.CD(AID.Aurora) - 60, 0.6f, deadline) && Unlocked(AID.Aurora) &&
            (AuroraLeft < _state.GCD && _state.CD(AID.NoMercy) > 1 && _state.CD(AID.GnashingFang) > 1 && _state.CD(AID.SonicBreak) > 1 && _state.CD(AID.DoubleDown) > 1) ||
            (!Unlocked(AID.DoubleDown) && AuroraLeft < _state.GCD && _state.CD(AID.NoMercy) > 1 && _state.CD(AID.GnashingFang) > 1 && _state.CD(AID.SonicBreak) > 1) ||
            (!Unlocked(AID.DoubleDown) && !Unlocked(AID.SonicBreak) && AuroraLeft < _state.GCD && _state.CD(AID.NoMercy) > 1 && _state.CD(AID.GnashingFang) > 1) ||
            (!Unlocked(AID.DoubleDown) && !Unlocked(AID.SonicBreak) && !Unlocked(AID.GnashingFang) && AuroraLeft < _state.GCD && _state.CD(AID.NoMercy) > 1))
            PushOGCD(AID.Aurora, primaryTarget);
    }

    // NM plan
    private bool ShouldUseNoMercy(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.NoMercy))
        {
            return false;
        }
        else
        {
            return ((Unlocked(AID.Bloodfest) && ComboLastMove == AID.BrutalShell && CombatTimer < 30) // Opener conditions
                    || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) && !Unlocked(AID.SonicBreak) &&
                    Ammo == MaxCartridges) // subLv53
                    || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    !Unlocked(AID.Continuation) && Unlocked(AID.GnashingFang) && Unlocked(AID.SonicBreak) &&
                    Ammo == MaxCartridges) // Lv60
                    || (!Unlocked(AID.DoubleDown) && !Unlocked(AID.FatedCircle) && !Unlocked(AID.Bloodfest) &&
                    Unlocked(AID.Continuation) &&
                    Ammo == MaxCartridges) // Lv70
                    || (!Unlocked(AID.DoubleDown) && Unlocked(AID.FatedCircle) && Unlocked(AID.Bloodfest) &&
                    Ammo == MaxCartridges) // Lv80
                    || (Unlocked(AID.DoubleDown) && Ammo == MaxCartridges)); // Lv90+
        }
    }

    // Bloodfest plan
    private bool ShouldUseBloodfest(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.Bloodfest))
        {
            return false;
        }

        return _state.TargetingEnemy && Unlocked(AID.Bloodfest) && Ammo == 0 && ComboLastMove == AID.DoubleDown;
    }

    // Sonic plan
    private bool ShouldUseSonic(StrategyValues strategy)
    {
        if (!Unlocked(AID.SonicBreak) || !NoMercy && !ReadyToBreak)
        {
            return false;
        }
        else
        {
            bool shouldUseSonicBreak = _state.TargetingEnemy && ((ReadyToBreak && _state.CD(AID.DoubleDown) < 5 && Ammo >= 3) && _state.CD(AID.Bloodfest) >= 40) //2cart
                || (ComboLastMove == AID.GnashingFang && _state.CD(AID.DoubleDown) <= 60 && Ammo == 0 && _state.CD(AID.Bloodfest) >= 40) // 2min 2cart
                || ((ReadyToBreakLeft > 27.5) && (_state.CD(AID.DoubleDown) < 5) && Ammo == 2
                && (_state.CD(AID.Bloodfest) >= 40) && (ComboLastMove == AID.BurstStrike) && (!ReadyToBlast));

            return shouldUseSonicBreak;
        }
    }

    // DD plan
    private bool ShouldUseDoubleDown(StrategyValues strategy)
    {
        if (!Unlocked(AID.DoubleDown))
        {
            return false;
        }

        return _state.TargetingEnemy && Unlocked(AID.DoubleDown) && Ammo >= 2 && NoMercy;
    }

    // BS plan
    private bool ShouldUseBurstStrike(StrategyValues strategy)
    {
        if (!Unlocked(AID.BurstStrike))
        {
            return false;
        }

        return _state.TargetingEnemy && Unlocked(AID.BurstStrike) && ((Ammo >= 1 && NoMercy) || (Ammo == MaxCartridges && ComboLastMove == AID.BrutalShell));
    }

    // GF plan
    private bool ShouldUseGnashingFang(StrategyValues strategy)
    {
        if (!Unlocked(AID.GnashingFang))
        {
            return false;
        }

        return _state.TargetingEnemy && Unlocked(AID.GnashingFang) && _state.CD(AID.GnashingFang) < 0.6f && Ammo >= 1 && (NoMercy || _state.CD(AID.NoMercy) > 17);
    }

    // Zone plan
    private bool ShouldUseZone(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.BlastingZone))
        {
            return false;
        }

        return _state.TargetingEnemy && Unlocked(AID.BlastingZone) && (_state.CD(AID.NoMercy) < 57 || _state.CD(AID.NoMercy) > 17);
    }

    // BowShock plan
    private bool ShouldUseBowShock(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.BowShock))
        {
            return false;
        }

        return _state.TargetingEnemy && Unlocked(AID.BowShock) && _state.CD(AID.NoMercy) < 57;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();
        SelectPrimaryTarget(targeting, ref primaryTarget, range: 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);
        _state.HaveTankStance = Player.FindStatus(SID.RoyalGuard) != null;

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

        NumAOETargets = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AOE => NumMeleeAOETargets(),
            _ => 0
        };

        GetNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => GetNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
