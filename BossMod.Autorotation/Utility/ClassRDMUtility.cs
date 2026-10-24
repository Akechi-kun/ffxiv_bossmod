namespace BossMod.Autorotation;

public sealed class ClassRDMUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { MagickBarrier = SharedTrack.Count, Vercure }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RDM.AID.VermilionScourge);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RDM", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.RDM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.MagickBarrier, "Magick Barrier", "Barrier", 600, RDM.AID.MagickBarrier, 10);
        DefineSimpleConfig(res, Track.Vercure, "Vercure", "", 100, RDM.AID.Vercure, 0, ActionQueue.Priority.VeryHigh);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.MagickBarrier), RDM.AID.MagickBarrier, Player);
        ExecuteSimple(strategy.Option(Track.Vercure), RDM.AID.Vercure, FriendlyTarget(primaryTarget));
    }
}
