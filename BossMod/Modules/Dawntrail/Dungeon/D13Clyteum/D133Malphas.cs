namespace BossMod.Dawntrail.Dungeon.D13Clyteum.D133Malphas;

public enum OID : uint
{
    Boss = 0x4C28,
    Helper = 0x233C,
}

class D133MalphasStates : StateMachineBuilder
{
    public D133MalphasStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1011, NameID = 14758)]
public class D133Malphas(WorldState ws, Actor primary) : BossModule(ws, primary, new(760, -803), new ArenaBoundsCircle(20));
