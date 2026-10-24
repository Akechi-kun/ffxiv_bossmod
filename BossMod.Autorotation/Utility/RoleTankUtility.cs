namespace BossMod.Autorotation;

public abstract class RoleTankUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Rampart, LowBlow, Provoke, Interject, Reprisal, Shirk, ArmsLength, Stance, Count }
    public enum ReprisalOption { None, Use, UseEx }
    public enum StanceOption { None, Apply, Remove }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3, ActionID stanceApply, ActionID stanceRemove)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self, 10, 15, 8)
            .AddAssociatedActions(ClassShared.AID.ShieldWall, ClassShared.AID.Stronghold)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.Rampart, "Rampart", "", 500, ClassShared.AID.Rampart, 20);
        DefineSimpleConfig(def, SharedTrack.LowBlow, "LowBlow", "Stun", -100, ClassShared.AID.LowBlow, 5);
        DefineSimpleConfig(def, SharedTrack.Provoke, "Provoke", "", 200, ClassShared.AID.Provoke);
        DefineSimpleConfig(def, SharedTrack.Interject, "Interject", "Interrupt", -50, ClassShared.AID.Interject);

        def.Define(SharedTrack.Reprisal).As<ReprisalOption>("Reprisal", "", 250)
            .AddOption(ReprisalOption.None, NoUse)
            .AddOption(ReprisalOption.Use, "Use Reprisal (10s)", 60, 10, ActionTargets.Self, 22, 97)
            .AddOption(ReprisalOption.UseEx, "Use Reprisal (15s)", 60, 15, ActionTargets.Self, 98)
            .AddAssociatedActions(ClassShared.AID.Reprisal);

        DefineSimpleConfig(def, SharedTrack.Shirk, "Shirk", "", 150, ClassShared.AID.Shirk);
        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsL", "Arm's Length", 300, ClassShared.AID.ArmsLength, 6);

        def.Define(SharedTrack.Stance).As<StanceOption>("Stance", "", 5)
            .AddOption(StanceOption.None, "Do not touch stance")
            .AddOption(StanceOption.Apply, "Turn stance on if it is off", 2)
            .AddOption(StanceOption.Remove, "Turn stance off if it is on", 1)
            .AddAssociatedAction(stanceApply)
            .AddAssociatedAction(stanceRemove);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, ActionID stanceApply, ActionID stanceRemove, uint stanceStatus, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Rampart), ClassShared.AID.Rampart, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LowBlow), ClassShared.AID.LowBlow, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Provoke), ClassShared.AID.Provoke, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Interject), ClassShared.AID.Interject, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Shirk), ClassShared.AID.Shirk, PartnerTarget(primaryTarget));
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);
        ExecuteStance(strategy.Option(SharedTrack.Stance), stanceApply, stanceRemove, stanceStatus);
        ExecuteReprisal(strategy.Option(SharedTrack.Reprisal));
        ExecuteLimitBreak(strategy.Option(SharedTrack.LB), ClassShared.AID.ShieldWall, ClassShared.AID.Stronghold, lb3, Player);
    }

    private void ExecuteStance(in StrategyValues.OptionRef stance, ActionID stanceApply, ActionID stanceRemove, uint stanceStatus)
    {
        var option = stance.As<StanceOption>();
        var wantStance = option == StanceOption.Apply;
        var haveStance = Player.FindStatus(stanceStatus) != null;
        if (option != StanceOption.None && haveStance != wantStance)
            Hints.ActionsToExecute.Push(wantStance ? stanceApply : stanceRemove, Player, stance.Priority(), stance.Value.ExpireIn);
    }
    private void ExecuteReprisal(in StrategyValues.OptionRef reprisal)
    {
        if (reprisal.As<ReprisalOption>() != ReprisalOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Reprisal), Player, reprisal.Priority(), reprisal.Value.ExpireIn);
    }
}
