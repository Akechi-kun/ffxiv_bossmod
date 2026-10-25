namespace BossMod.Autorotation;

public sealed class ClassVPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Slither = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(VPR.AID.WorldSwallower);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: VPR", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.VPR), 100);
        DefineShared(res, IDLimitBreak3);

        DefineDash(res, Track.Slither, "Slither", "Slither", VPR.AID.Slither, ActionTargets.Party | ActionTargets.Hostile, "Use when outside melee range of target");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteDash(strategy.Option(Track.Slither), VPR.AID.Slither, primaryTarget);
    }
}
