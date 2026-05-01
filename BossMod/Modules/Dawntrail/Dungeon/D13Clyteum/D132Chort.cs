namespace BossMod.Dawntrail.Dungeon.D13Clyteum.D132Chort;

public enum OID : uint
{
    Boss = 0x4C3F,
    Helper = 0x233C,
}

class D132ChortStates : StateMachineBuilder
{
    public D132ChortStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1011, NameID = 14734)]
public class D132Chort(WorldState ws, Actor primary) : BossModule(ws, primary, new(660, -141), new ArenaBoundsCircle(15));
