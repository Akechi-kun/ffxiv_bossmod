namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Shadowstride }
    public enum WallOption { None, ShadowWall, ShadowedVigil }
    public enum TBNStrategy { None, Use }
    public enum OblationStrategy { None, Use }
    public enum DashStrategy { None, GapClose, Five, Ten }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRK.AID.DarkForce);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(DRK.AID.Grit);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(DRK.AID.ReleaseGrit);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRK", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.DRK), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.DarkMind, "DarkMind", "DMind", 450, DRK.AID.DarkMind, 10);

        res.Define(Track.ShadowWall).As<WallOption>("ShadowWall", "Wall", 550)
            .AddOption(WallOption.None, NoUse)
            .AddOption(WallOption.ShadowWall, "Use Shadow Wall", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(WallOption.ShadowedVigil, "Use Shadowed Vigil", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(DRK.AID.ShadowWall, DRK.AID.ShadowedVigil);

        DefineSimpleConfig(res, Track.LivingDead, "LivingDead", "LD", 400, DRK.AID.LivingDead, 10);

        res.Define(Track.TheBlackestNight).As<TBNStrategy>("TheBlackestNight", "TBN", 550)
            .AddOption(TBNStrategy.None, NoUse)
            .AddOption(TBNStrategy.Use, "Use The Blackest Night", 15, 7, ActionTargets.Self | ActionTargets.Party, 70, internalNameOverride: "Force")
            .AddAssociatedActions(DRK.AID.TheBlackestNight);

        res.Define(Track.Oblation).As<OblationStrategy>("Oblation", "", 550)
            .AddOption(OblationStrategy.None, NoUse)
            .AddOption(OblationStrategy.Use, "Use Oblation", 60, 10, ActionTargets.Self | ActionTargets.Party, 82, internalNameOverride: "Force")
            .AddAssociatedActions(DRK.AID.Oblation);

        DefineSimpleConfig(res, Track.DarkMissionary, "DarkMissionary", "Mission", 220, DRK.AID.DarkMissionary, 15);

        res.Define(Track.Shadowstride).As<DashStrategy>("Shadowstride", "Dash", 20)
            .AddOption(DashStrategy.None, NoUse)
            .AddOption(DashStrategy.GapClose, "Use as a gap closer when outside melee range", 30, 0, ActionTargets.Hostile, 54)
            .AddOption(DashStrategy.Five, "Use when 5y or further from target", 30, 0, ActionTargets.Hostile, 54)
            .AddOption(DashStrategy.Ten, "Use when 10y or further from target", 30, 0, ActionTargets.Hostile, 54)
            .AddAssociatedActions(DRK.AID.Shadowstride);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)DRK.SID.Grit, primaryTarget);
        ExecuteSimple(strategy.Option(Track.DarkMind), DRK.AID.DarkMind, Player);
        ExecuteSimple(strategy.Option(Track.LivingDead), DRK.AID.LivingDead, Player);
        ExecuteSimple(strategy.Option(Track.DarkMissionary), DRK.AID.DarkMissionary, Player);
        ExecuteBlackestNight(strategy.Option(Track.TheBlackestNight));
        ExecuteOblation(strategy.Option(Track.Oblation));
        ExecuteShadowWall(strategy.Option(Track.ShadowWall));
        ExecuteShadowstride(strategy.Option(Track.Shadowstride), primaryTarget);
    }

    private void ExecuteBlackestNight(in StrategyValues.OptionRef tbn)
    {
        if (tbn.As<TBNStrategy>() == TBNStrategy.Use && Player.HPMP.CurMP >= 3000)
            ExecuteAction(tbn, DRK.AID.TheBlackestNight, Player);
    }
    private void ExecuteOblation(in StrategyValues.OptionRef oblation)
    {
        var target = ResolveTarget(oblation.Value) ?? Player;
        if (oblation.As<OblationStrategy>() == OblationStrategy.Use && target.FindStatus(DRK.SID.Oblation, DateTime.MaxValue) == null)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Oblation), target, oblation.Priority(), oblation.Value.ExpireIn);
    }
    private void ExecuteShadowWall(in StrategyValues.OptionRef wall)
    {
        var action = wall.As<WallOption>() switch
        {
            WallOption.ShadowWall => DRK.AID.ShadowWall,
            WallOption.ShadowedVigil => DRK.AID.ShadowedVigil,
            _ => default
        };
        if (action != default)
            ExecuteAction(wall, action, Player);
    }
    private void ExecuteShadowstride(in StrategyValues.OptionRef dash, Actor? primaryTarget)
    {
        var option = dash.As<DashStrategy>();
        var minDistance = option switch
        {
            DashStrategy.Five => 5,
            DashStrategy.Ten => 10,
            _ => 3
        };
        if (option != DashStrategy.None)
            ExecuteDash(dash, DRK.AID.Shadowstride, primaryTarget, minDistance);
    }
}
