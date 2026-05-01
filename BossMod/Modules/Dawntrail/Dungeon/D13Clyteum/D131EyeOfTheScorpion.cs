namespace BossMod.Dawntrail.Dungeon.D13Clyteum.D131EyeOfTheScorpion;

public enum OID : uint
{
    Boss = 0x4C2C, // R6.000, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    MotionScanner = 0x4C2D, // R1.000, x2
}

public enum AID : uint
{
    _AutoAttack_ = 50170, // 4EB6/4DD2->4C09/4C0A, no cast, single-target
    _AutoAttack_1 = 50110, // Boss->player, no cast, single-target
    _Weaponskill_EyesOnMe = 48896, // Boss->self, 5.0s cast, range 35 circle
    _Weaponskill_PetrifyingBeam = 50176, // Boss->self, 8.0+0.5s cast, single-target
    _Weaponskill_PetrifyingBeam1 = 50178, // Helper->self, 8.5s cast, range 70 100-degree cone
    _Weaponskill_PetrifyingBeam2 = 50175, // Boss->self, 8.0+0.5s cast, single-target
    _Weaponskill_PetrifyingBeam3 = 50177, // Helper->self, 8.5s cast, range 70 100-degree cone
    _Weaponskill_MotionScanner = 48893, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Launch = 48895, // Helper->player, no cast, single-target
    _Weaponskill_BallisticMissile = 48897, // Boss->self, no cast, single-target
    _Weaponskill_PenetratorMissile = 48901, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_SurfaceMissile = 48898, // Helper->location, 3.0s cast, range 5 circle
    _Weaponskill_AntiPersonnelMissile = 48899, // Helper->player, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_DirectionalDisregard = 3808, // none->Boss, extra=0x0
    _Gen_MotionTracker = 5191, // none->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
}

public enum IconID : uint
{
    _Gen_Icon_m0489trg_a0c = 136, // player->self
    _Gen_Icon_com_share0c = 62, // player->self
    _Gen_Icon_target_ae_s5f = 139, // player->self
}

class EyesOnMe(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_EyesOnMe);
class PetrifyingBeam(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_PetrifyingBeam1, AID._Weaponskill_PetrifyingBeam3], new AOEShapeCone(70, 50.Degrees()));
class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SurfaceMissile, 5);
class PenetratorMissile(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_PenetratorMissile, 5);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_AntiPersonnelMissile, 6);
class MotionScanner(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_MotionTracker && Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new(Requirement.Stay, WorldState.CurrentTime));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_MotionTracker && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}

class D131EyeOfTheScorpionStates : StateMachineBuilder
{
    public D131EyeOfTheScorpionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyesOnMe>()
            .ActivateOnEnter<PetrifyingBeam>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<PenetratorMissile>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<MotionScanner>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1011, NameID = 14716)]
public class D131EyeOfTheScorpion(WorldState ws, Actor primary) : BossModule(ws, primary, new(-615, 575), new ArenaBoundsSquare(20));
