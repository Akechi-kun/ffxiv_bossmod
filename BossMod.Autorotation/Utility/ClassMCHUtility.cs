namespace BossMod.Autorotation;

public sealed class ClassMCHUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { Tactician = SharedTrack.Count, Dismantle }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MCH.AID.SatelliteBeam);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MCH", UtilityDescription, UtilityCategory, "Aimsucks", RotationModuleQuality.Ok, BitMask.Build((int)Class.MCH), 100);
        DefineShared(res, IDLimitBreak3);

        DefineMitigation(res, Track.Tactician, "Tactician", "Tactician", 400, MCH.AID.Tactician, 56);

        DefineSimpleConfig(res, Track.Dismantle, "Dismantle", "Dismantle", 500, MCH.AID.Dismantle, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Dismantle), MCH.AID.Dismantle, primaryTarget);
        ExecuteMitigation(strategy.Option(Track.Tactician), MCH.AID.Tactician);
    }
}
