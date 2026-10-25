namespace BossMod.Autorotation;

public sealed class ClassNINUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ShadeShift = SharedTrack.Count, Shukuchi }
    public enum DashStrategy { None, GapClose, Five, Ten, Force, Rear, Flank, GapCloseHold1 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(NIN.AID.Chimatsuri);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: NIN", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.NIN), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ShadeShift, "Shade", "Shade Shift", 400, NIN.AID.ShadeShift, 20);

        res.Define(Track.Shukuchi).As<DashStrategy>("Shukuchi", "Shukuchi", 20)
            .AddOption(DashStrategy.None, NoUse)
            .AddOption(DashStrategy.GapClose, "Dash to the selected spot when 3y or further", 60, 0, ActionTargets.Area, 40)
            .AddOption(DashStrategy.Five, "Dash to the selected spot when 5y or further", 60, 0, ActionTargets.Area, 40)
            .AddOption(DashStrategy.Ten, "Dash to the selected spot when 10y or further", 60, 0, ActionTargets.Area, 40)
            .AddOption(DashStrategy.Force, "Dash to the selected spot, unless already there", 60, 0, ActionTargets.Area, 40)
            .AddOption(DashStrategy.Rear, "Dash to the target's rear, unless already there", 60, 0, ActionTargets.Hostile, 40)
            .AddOption(DashStrategy.Flank, "Dash to the target's nearest flank, unless already there", 60, 0, ActionTargets.Hostile, 40)
            .AddOption(DashStrategy.GapCloseHold1, "Dash to the selected spot when 3y or further; conserve one charge", 60, 0, ActionTargets.Area, 74)
            .AddAssociatedActions(NIN.AID.Shukuchi);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ShadeShift), NIN.AID.ShadeShift, Player);
        ExecuteShukuchi(strategy.Option(Track.Shukuchi), primaryTarget);
    }

    private void ExecuteShukuchi(in StrategyValues.OptionRef dash, Actor? primaryTarget)
    {
        var option = dash.As<DashStrategy>();
        if (option is DashStrategy.Rear or DashStrategy.Flank)
        {
            ExecuteShukuchiToPositional(dash, option == DashStrategy.Rear ? Positional.Rear : Positional.Flank, ResolveTarget(dash.Value) ?? primaryTarget);
            return;
        }

        var distance = Player.DistanceToPoint(ResolveTargetLocation(dash.Value));
        var condition = option switch
        {
            DashStrategy.GapClose => distance is > 3 and <= 20,
            DashStrategy.Force => distance is > 1 and <= 20,
            DashStrategy.Five => distance is > 5 and <= 20,
            DashStrategy.Ten => distance is > 10 and <= 20,
            DashStrategy.GapCloseHold1 => distance is > 3 and <= 20 && HasCharges(NIN.AID.Shukuchi, 1),
            _ => false
        };
        if (condition)
            ExecuteArea(dash, NIN.AID.Shukuchi);
    }

    private void ExecuteShukuchiToPositional(in StrategyValues.OptionRef dash, Positional positional, Actor? target)
    {
        if (target == null || !IsEnemy(target) || target.Omnidirectional || InPositional(target, positional))
            return;

        var destination = PositionalSpot(target, positional);
        if (Player.DistanceToPoint(destination) <= 20)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(NIN.AID.Shukuchi), null, dash.Priority(), dash.Value.ExpireIn, targetPos: destination.ToVec3(target.PosRot.Y));
    }

    private bool InPositional(Actor target, Positional positional)
    {
        var dot = target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized());
        return positional == Positional.Rear ? dot < -0.7071068f : MathF.Abs(dot) < 0.7071067f;
    }

    private WPos PositionalSpot(Actor target, Positional positional)
    {
        var facing = target.Rotation.ToDirection();
        var nearFlank = (Player.Position - target.Position).Dot(facing.OrthoL()) >= 0 ? facing.OrthoL() : facing.OrthoR();
        var direction = positional == Positional.Rear ? -facing : nearFlank;
        return target.Position + direction * (target.HitboxRadius + 1);
    }
}
