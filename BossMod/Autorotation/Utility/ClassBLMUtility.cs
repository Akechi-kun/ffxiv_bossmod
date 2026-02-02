using BossMod.BLM;

namespace BossMod.Autorotation;

public sealed class ClassBLMUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { Manaward = SharedTrack.Count, AetherialManipulation }
    public enum DashStrategy { None, Force }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AID.Meteor);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLM", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.BLM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Manaward, "Manaward", "", 600, AID.Manaward, 20);

        res.Define(Track.AetherialManipulation).As<DashStrategy>("Dash", "", 20)
            .AddOption(DashStrategy.None, "Do not use automatically")
            .AddOption(DashStrategy.Force, "Use ASAP", 10, 0, ActionTargets.Party, 50)
            .AddAssociatedActions(AID.AetherialManipulation);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Manaward), AID.Manaward, Player);

        var dash = strategy.Option(Track.AetherialManipulation);
        var dashStrategy = strategy.Option(Track.AetherialManipulation).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value);
        if (dashTarget != null &&
            dashStrategy == DashStrategy.Force &&
            Player.DistanceToHitbox(dashTarget) <= 25 &&
            ReadyIn(AID.AetherialManipulation) < 0.6f)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.AetherialManipulation), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
