namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

class Ex8EnuoStates : StateMachineBuilder
{
    readonly Ex8Enuo _module;

    public Ex8EnuoStates(Ex8Enuo module) : base(module)
    {
        _module = module;

        SimplePhase(0, id =>
        {
            P1(id);
            Intermission(id + 0x10000);
        }, "P1 + intermission")
            .Raw.Update = () => module.FindComponent<ArenaSwitcher>()?.IntermissionOver == true || module.PrimaryActor.IsDeadOrDestroyed;
        DeathPhase(1, P2).SetHint(StateMachine.PhaseHint.StartWithDowntime);
    }

    void P1(uint id)
    {
        Meteorain(id, 9.2f);
        NaughtGrows(id + 0x100, 7.5f);
        Cast(id + 0x150, AID._Weaponskill_NaughtWakes, 5.2f, 2);
        Meltdown(id + 0x200, 2.2f);
        Emptiness(id + 0x300, 1.8f);
        NaughtGrows(id + 0x400, 2.5f);
        GazeOfTheVoid(id + 0x500, 8.7f);
        Vacuum(id + 0x600, 3.5f);
        Emptiness(id + 0x700, 0.1f);
        DeepFreeze(id + 0x800, 5.2f);
        Meteorain(id + 0x900, 3.1f);
        Cast(id + 0xA00, AID._Weaponskill_AllForNaught, 8.6f, 5);
        Targetable(id + 0xA10, false, 0.2f, "Boss disappears (intermission)")
            .DeactivateOnExit<DeepFreezeFreeze>();
    }

    void Meteorain(uint id, float delay)
    {
        Cast(id, AID._Spell_Meteorain, delay, 5, "Raidwide")
            .ActivateOnEnter<Meteorain>()
            .DeactivateOnExit<Meteorain>();
    }

    void NaughtGrows(uint id, float delay)
    {
        CastStartMulti(id, [AID._Weaponskill_NaughtGrows, AID._Weaponskill_NaughtGrows3], delay)
            .ActivateOnEnter<NaughtGrowsCounter>()
            .ActivateOnEnter<NaughtGrowsDonut>()
            .ActivateOnEnter<NaughtGrowsCircle>()
            .ActivateOnEnter<ReturnToNothing>();

        ComponentCondition<NaughtGrowsCounter>(id + 1, 8, n => n.NumCasts > 0, "In/out")
            .DeactivateOnExit<NaughtGrowsCounter>()
            .DeactivateOnExit<NaughtGrowsDonut>()
            .DeactivateOnExit<NaughtGrowsCircle>();

        ComponentCondition<ReturnToNothing>(id + 2, 0.8f, r => r.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<ReturnToNothing>();
    }

    void Meltdown(uint id, float delay)
    {
        Cast(id, AID._Spell_Meltdown, delay, 4)
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<MeltdownBaited>()
            .ActivateOnEnter<MeltdownSpread>();

        ComponentCondition<ChainsOfCondemnation>(id + 0x10, 1, c => c.Active, "Stop moving");
        ComponentCondition<MeltdownBaited>(id + 0x11, 4.5f, m => m.NumCasts > 0, "Puddles")
            .DeactivateOnExit<MeltdownBaited>();
        ComponentCondition<MeltdownSpread>(id + 0x12, 1, m => m.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<MeltdownSpread>()
            .DeactivateOnExit<ChainsOfCondemnation>();
    }

    void Emptiness(uint id, float delay)
    {
        CastMulti(id, [AID._Weaponskill_AiryEmptiness, AID._Weaponskill_DenseEmptiness], delay, 4)
            .ActivateOnEnter<Emptiness>();

        ComponentCondition<Emptiness>(id + 2, 1, e => e.NumCasts > 0, "Stacks")
            .DeactivateOnExit<Emptiness>();
    }

    // slow orbs move ~1.3 units per second and start from arena edge (20 units away), so ~15.4s from spawn to absorb by boss
    // accounting for extra time waiting for boss to cast, let's say orb phase is 17s
    void GazeOfTheVoid(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_GazeOfTheVoid, delay)
            .ActivateOnEnter<GazeOfTheVoid>()
            .ActivateOnEnter<Burst>();
        ComponentCondition<GazeOfTheVoid>(id + 1, 7, v => v.NumCasts > 0, "Cones start");
        ComponentCondition<GazeOfTheVoid>(id + 2, 3.6f, v => v.NumCasts == 10, "Cones finish")
            .DeactivateOnExit<GazeOfTheVoid>();

        Timeout(id + 0x10, 16, "Orb deadline")
            .DeactivateOnExit<Burst>();
    }

    void Vacuum(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_Vacuum, delay, 2)
            .ActivateOnEnter<SilentTorrentSmall>()
            .ActivateOnEnter<SilentTorrentMedium>()
            .ActivateOnEnter<SilentTorrentLarge>()
            .ActivateOnEnter<Vacuum>();

        ComponentCondition<SilentTorrentMedium>(id + 0x10, 6.1f, s => s.NumCasts > 0, "Lines")
            .DeactivateOnExit<SilentTorrentSmall>()
            .DeactivateOnExit<SilentTorrentMedium>()
            .DeactivateOnExit<SilentTorrentLarge>();
        ComponentCondition<Vacuum>(id + 0x20, 2.1f, v => v.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Vacuum>();
    }

    void DeepFreeze(uint id, float delay)
    {
        Cast(id, AID._Spell_DeepFreeze, delay, 5, "Raidwide + move")
            .ActivateOnEnter<DeepFreezeRaidwide>()
            .ActivateOnEnter<DeepFreezeFreeze>()
            .ActivateOnEnter<DeepFreezeFlare>()
            .DeactivateOnExit<DeepFreezeRaidwide>()
            .DeactivateOnExit<DeepFreezeFlare>();
    }

    // apeiron cast happens at +278.9s or so, if gauge reaches 100 (110.4s after first director update)
    void Intermission(uint id)
    {
        ActorCastStart(id, _module.LoomingShadow, AID._Weaponskill_LoomingEmptiness, 14.2f)
            .ActivateOnEnter<LoomingEmptiness>()
            .ActivateOnEnter<LoomingEmptinessKB>()
            .ActivateOnEnter<LoomingShadow>()
            .ActivateOnEnter<ArenaSwitcher>()
            .ActivateOnEnter<DemonEye>()
            .ActivateOnEnter<WeightOfNothing>()
            .ActivateOnEnter<Beacon>()
            .ActivateOnEnter<Nothingness>();

        ActorTargetable(id + 0x10, _module.LoomingShadow, true, 6.1f, "Shadow appears")
            .DeactivateOnExit<LoomingEmptiness>()
            .DeactivateOnExit<LoomingEmptinessKB>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCastStart(id + 0x100, _module.LoomingShadow, AID._Weaponskill_VoidalTurbulence, 2.2f)
            .ActivateOnEnter<EmptyShadow>()
            .ActivateOnEnter<VoidalTurbulence>()
            .ActivateOnEnter<Shadows>()
            .ActivateOnEnter<Gauntlet>();

        ComponentCondition<VoidalTurbulence>(id + 0x110, 7.3f, v => v.NumCasts > 0, "Baits/towers 1")
            .DeactivateOnExit<EmptyShadow>()
            .DeactivateOnExit<VoidalTurbulence>();

        ComponentCondition<Shadows>(id + 0x120, 1.2f, s => s.ActiveActors.Any(), "Adds 1 appear");

        ActorCastStart(id + 0x200, _module.LoomingShadow, AID._Weaponskill_VoidalTurbulence, 16.1f)
            .ActivateOnEnter<EmptyShadow>()
            .ActivateOnEnter<VoidalTurbulence>();

        ComponentCondition<VoidalTurbulence>(id + 0x210, 7.3f, v => v.NumCasts > 0, "Baits/towers 2")
            .DeactivateOnExit<EmptyShadow>()
            .DeactivateOnExit<VoidalTurbulence>();

        ComponentCondition<Shadows>(id + 0x220, 1.2f, s => s.ActiveActors.Any(), "Adds 2 appear");

        Timeout(id + 0x300, 69, "Intermission enrage");
    }

    void P2(uint id)
    {
        Targetable(id, true, 8.3f, "Boss reappears");
        LightlessWorld(id + 0x10, 0.1f);

        Timeout(id + 0xFF0000, 10000, "???");
    }

    void LightlessWorld(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_LightlessWorld, delay, 10)
            .ActivateOnEnter<LightlessWorld>();
        ComponentCondition<LightlessWorld>(id + 0x2, 1, l => l.NumCasts > 0, "Raidwide 1");
        ComponentCondition<LightlessWorld>(id + 0x3, 2.5f, l => l.NumCasts == 6, "Raidwide 6")
            .DeactivateOnExit<LightlessWorld>();
    }
}
