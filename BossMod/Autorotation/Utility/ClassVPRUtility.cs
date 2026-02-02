using BossMod.VPR;

namespace BossMod.Autorotation;

public sealed class ClassVPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Slither = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AID.WorldSwallower);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: VPR", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.VPR), 100);
        DefineShared(res, IDLimitBreak3);
        DefineDashConfig(res, Track.Slither, "Slither", 100, AID.Slither);
        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteDash(strategy.Option(Track.Slither), AID.Slither, primaryTarget);
    }
}
