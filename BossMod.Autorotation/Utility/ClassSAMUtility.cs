namespace BossMod.Autorotation;

public sealed class ClassSAMUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ThirdEye = SharedTrack.Count }
    public enum EyeOption { None, ThirdEye, Tengentsu }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SAM.AID.DoomOfTheLiving);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SAM", UtilityDescription, UtilityCategory, "xan, Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SAM), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.ThirdEye).As<EyeOption>("ThirdEye", "Third Eye", 600)
            .AddOption(EyeOption.None, NoUse)
            .AddOption(EyeOption.ThirdEye, "Use Third Eye", 15, 4, ActionTargets.Self, 6, 81)
            .AddOption(EyeOption.Tengentsu, "Use Tengentsu", 15, 4, ActionTargets.Self, 82)
            .AddAssociatedActions(SAM.AID.ThirdEye, SAM.AID.Tengentsu);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteVariant(strategy.Option(Track.ThirdEye), Player, SAM.AID.ThirdEye, SAM.AID.Tengentsu);
    }
}
