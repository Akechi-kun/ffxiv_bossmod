using BossMod.DRK;

namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Shadowstride }
    public enum WallOption { None, ShadowWall, ShadowedVigil }
    public enum TBNStrategy { None, Force }
    public enum OblationStrategy { None, Force }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AID.DarkForce);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(AID.Grit);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(AID.ReleaseGrit);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRK", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.DRK), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.DarkMind, "DarkMind", "DMind", 450, AID.DarkMind, 10);

        res.Define(Track.ShadowWall).As<WallOption>("ShadowWall", "Wall", 550)
            .AddOption(WallOption.None, "Do not use automatically")
            .AddOption(WallOption.ShadowWall, "Use Shadow Wall", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(WallOption.ShadowedVigil, "Use Shadowed Vigil", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(AID.ShadowWall, AID.ShadowedVigil);

        DefineSimpleConfig(res, Track.LivingDead, "LivingDead", "LD", 400, AID.LivingDead, 10);

        res.Define(Track.TheBlackestNight).As<TBNStrategy>("TheBlackestNight", "TBN", 550)
            .AddOption(TBNStrategy.None, "Do not use automatically")
            .AddOption(TBNStrategy.Force, "Use The Blackest Night", 15, 7, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(AID.TheBlackestNight);

        res.Define(Track.Oblation).As<OblationStrategy>("Oblation", "", 550)
            .AddOption(OblationStrategy.None, "Do not use automatically")
            .AddOption(OblationStrategy.Force, "Use Oblation", 60, 10, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(AID.Oblation);

        DefineSimpleConfig(res, Track.DarkMissionary, "DarkMissionary", "Mission", 220, AID.DarkMissionary, 15);
        DefineDashConfig(res, Track.Shadowstride, "Shadowstride", 100, AID.Shadowstride);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)SID.Grit, primaryTarget);
        ExecuteSimple(strategy.Option(Track.DarkMind), AID.DarkMind, Player);
        ExecuteSimple(strategy.Option(Track.LivingDead), AID.LivingDead, Player);
        ExecuteSimple(strategy.Option(Track.DarkMissionary), AID.DarkMissionary, Player);
        ExecuteDash(strategy.Option(Track.Shadowstride), AID.Shadowstride, primaryTarget);

        //TBN execution
        var tbn = strategy.Option(Track.TheBlackestNight);
        var tbnTarget = ResolveTargetOverride(tbn.Value) ?? CoTank() ?? primaryTarget ?? Player; //smart-target -> CoTank -> target (if current target is party member) -> self
        if (ActionUnlocked(ActionID.MakeSpell(AID.TheBlackestNight)) && Player.HPMP.CurMP >= 3000 && tbn.As<TBNStrategy>() == TBNStrategy.Force)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.TheBlackestNight), tbnTarget, tbn.Priority(), tbn.Value.ExpireIn);

        //Oblation execution
        var oblation = strategy.Option(Track.Oblation);
        var oblationTarget = ResolveTargetOverride(oblation.Value) ?? primaryTarget ?? Player; //smart-target -> target (if current target is party member) -> self
        if (ActionUnlocked(ActionID.MakeSpell(AID.Oblation)) &&
            World.Client.Cooldowns[ActionDefinitions.Instance.Spell(AID.Oblation)!.MainCooldownGroup].Remaining <= 60.5f &&
            oblationTarget?.FindStatus(SID.Oblation) == null && oblation.As<OblationStrategy>() == OblationStrategy.Force)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Oblation), oblationTarget, oblation.Priority(), oblation.Value.ExpireIn);

        //Shadow Wall / Vigil execution
        var wall = strategy.Option(Track.ShadowWall);
        var wallAction = wall.As<WallOption>() switch
        {
            WallOption.ShadowWall => AID.ShadowWall,
            WallOption.ShadowedVigil => AID.ShadowedVigil,
            _ => default
        };
        if (wallAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(wallAction), Player, wall.Priority(), wall.Value.ExpireIn);
    }
}
