namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count }
    public enum DashStrategy { None, GapClose, Five, Ten }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.WingedGlide).As<DashStrategy>("Winged Glide", "Dash", 20)
            .AddOption(DashStrategy.None, NoUse)
            .AddOption(DashStrategy.GapClose, "Use when outside melee range of target", 60, 0, ActionTargets.Party | ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.Five, "Use when 5y or more from target", 60, 0, ActionTargets.Party | ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.Ten, "Use when 10y or more from target", 60, 0, ActionTargets.Party | ActionTargets.Hostile, 45)
            .AddAssociatedActions(DRG.AID.WingedGlide);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteWingedGlide(strategy.Option(Track.WingedGlide), primaryTarget);
    }

    private void ExecuteWingedGlide(in StrategyValues.OptionRef dash, Actor? primaryTarget)
    {
        var option = dash.As<DashStrategy>();
        var minDistance = option switch
        {
            DashStrategy.Five => 5,
            DashStrategy.Ten => 10,
            _ => 3
        };
        if (option != DashStrategy.None)
            ExecuteDash(dash, DRG.AID.WingedGlide, primaryTarget, minDistance);
    }
}
