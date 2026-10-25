namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        DefineDash(res, Track.WingedGlide, "Winged Glide", "Winged Glide", DRG.AID.WingedGlide, ActionTargets.Hostile, "Use when outside melee range of target", 84);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteDash(strategy.Option(Track.WingedGlide), DRG.AID.WingedGlide, primaryTarget);
    }
}
