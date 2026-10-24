namespace BossMod.Autorotation;

public sealed class ClassNINUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ShadeShift = SharedTrack.Count, Shukuchi }
    public enum DashStrategy { None, GapClose, Five, Ten, Force }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(NIN.AID.Chimatsuri);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: NIN", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.NIN), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ShadeShift, "Shade", "", 400, NIN.AID.ShadeShift, 20);

        res.Define(Track.Shukuchi).As<DashStrategy>("Shukuchi", "Dash", 20)
            .AddOption(DashStrategy.None, NoUse)
            .AddOption(DashStrategy.GapClose, "Dash to the selected spot when 3y or further", 60, 0, ActionTargets.Area, 40)
            .AddOption(DashStrategy.Five, "Dash to the selected spot when 5y or further", 60, 0, ActionTargets.Area, 40)
            .AddOption(DashStrategy.Ten, "Dash to the selected spot when 10y or further", 60, 0, ActionTargets.Area, 40)
            .AddOption(DashStrategy.Force, "Dash to the selected spot regardless of distance", 60, 0, ActionTargets.Area, 40)
            .AddAssociatedActions(NIN.AID.Shukuchi);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ShadeShift), NIN.AID.ShadeShift, Player);
        ExecuteShukuchi(strategy.Option(Track.Shukuchi));
    }

    private void ExecuteShukuchi(in StrategyValues.OptionRef dash)
    {
        var option = dash.As<DashStrategy>();
        var distance = Player.DistanceToPoint(ResolveTargetLocation(dash.Value));
        var condition = option switch
        {
            DashStrategy.GapClose => distance is > 3 and <= 20,
            DashStrategy.Force => distance <= 20,
            DashStrategy.Five => distance is > 5 and <= 20,
            DashStrategy.Ten => distance is > 10 and <= 20,
            _ => false
        };
        if (condition)
            ExecuteArea(dash, NIN.AID.Shukuchi);
    }
}
