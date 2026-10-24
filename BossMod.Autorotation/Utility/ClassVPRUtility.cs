namespace BossMod.Autorotation;

public sealed class ClassVPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Slither = SharedTrack.Count }
    public enum DashStrategy { None, Opener, GapClose, Five, Ten }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(VPR.AID.WorldSwallower);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: VPR", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.VPR), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.Slither).As<DashStrategy>("Slither", "", 20)
            .AddOption(DashStrategy.None, NoUse)
            .AddOption(DashStrategy.Opener, "Use as a gap closer at the start of combat", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 40)
            .AddOption(DashStrategy.GapClose, "Use when outside melee range of target", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 40)
            .AddOption(DashStrategy.Five, "Use when 5y or more from target", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 40)
            .AddOption(DashStrategy.Ten, "Use when 10y or more from target", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 40)
            .AddAssociatedActions(VPR.AID.Slither);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSlither(strategy.Option(Track.Slither), primaryTarget);
    }

    private bool IsFirstGCD => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    private void ExecuteSlither(in StrategyValues.OptionRef dash, Actor? primaryTarget)
    {
        var option = dash.As<DashStrategy>();
        var minDistance = option switch
        {
            DashStrategy.Five => 5,
            DashStrategy.Ten => 10,
            _ => 3
        };
        if (option != DashStrategy.None && (option != DashStrategy.Opener || IsFirstGCD))
            ExecuteDash(dash, VPR.AID.Slither, primaryTarget, minDistance);
    }
}
