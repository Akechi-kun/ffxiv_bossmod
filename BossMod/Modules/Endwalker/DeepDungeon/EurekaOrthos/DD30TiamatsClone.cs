namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD30TiamatsClone;

public enum OID : uint
{
    Boss = 0x3D9A, // R19.0
    DarkWanderer = 0x3D9B, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 32702, // Boss->player, no cast, single-target
    HeadAttack = 31842, // Helper->player, no cast, single-target

    CreatureOfDarkness = 31841, // Boss->self, 3.0s cast, single-target, summon Heads E<->W heading S
    DarkMegaflareVisual = 31849, // Boss->self, 3.0s cast, single-target
    DarkMegaflare = 31850, // Helper->location, 3.0s cast, range 6 circle
    DarkWyrmtailVisual = 31843, // Boss->self, 5.0s cast, single-target
    DarkWyrmtail = 31844, // Helper->self, 6.0s cast, range 40 width 16 rect, summon Heads Heading E/W from Middle Lane
    DarkWyrmwingVisual = 31845, // Boss->self, 5.0s cast, single-target
    DarkWyrmwing = 31846, // Helper->self, 6.0s cast, range 40 width 16 rect, summon Heads Heading E/W from E/W Walls
    WheiMornFirst = 31847, // Boss->location, 5.0s cast, range 6 circle
    WheiMornRest = 31848 // Boss->location, no cast, range 6 circle
}

public enum IconID : uint
{
    ChasingAOE = 197 // player
}

class WheiMorn(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6f), ActionID.MakeSpell(AID.WheiMornFirst), ActionID.MakeSpell(AID.WheiMornRest), 6f, 2f, 5, true, (uint)IconID.ChasingAOE);
class DarkMegaflare(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DarkMegaflare), 6f);

class DarkWyrm(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40f, 8f));
class DarkWyrmwing(BossModule module) : DarkWyrm(module, AID.DarkWyrmwing);
class DarkWyrmtail(BossModule module) : DarkWyrm(module, AID.DarkWyrmtail);
class CreatureOfDarkness(BossModule module) : Components.PersistentVoidzone(module, 2f, m => m.Enemies(OID.DarkWanderer).Where(x => x.ModelState.AnimState1 == 1), 6f);

class DD30TiamatsCloneStates : StateMachineBuilder
{
    public DD30TiamatsCloneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkWyrmwing>()
            .ActivateOnEnter<DarkWyrmtail>()
            .ActivateOnEnter<DarkMegaflare>()
            .ActivateOnEnter<WheiMorn>()
            .ActivateOnEnter<CreatureOfDarkness>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 899, NameID = 12242)]
public class DD30TiamatsClone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsSquare(19.5f));
