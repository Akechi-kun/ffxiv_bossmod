namespace BossMod.Autorotation;

public abstract class RoleCasterUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Addle, Sleep, LucidDreaming, Swiftcast, Surecast, Count }
    public enum AddleOption { None, Use, UseEx }
    public enum SwiftcastOption { None, Use, UseEx }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 10, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Area, 8)
            .AddAssociatedActions(ClassShared.AID.Skyshard, ClassShared.AID.Starstorm)
            .AddAssociatedAction(lb3);

        def.Define(SharedTrack.Addle).As<AddleOption>("Addle", "", 250)
            .AddOption(AddleOption.None, NoUse)
            .AddOption(AddleOption.Use, "Use Addle (10s)", 90, 10, ActionTargets.Hostile, 8, 97)
            .AddOption(AddleOption.UseEx, "Use Addle (15s)", 90, 15, ActionTargets.Hostile, 98)
            .AddAssociatedActions(ClassShared.AID.Addle);

        DefineSimpleConfig(def, SharedTrack.Sleep, "Sleep", "", -10, ClassShared.AID.Sleep, 30);
        DefineSimpleConfig(def, SharedTrack.LucidDreaming, "LucidDreaming", "Lucid D.", 30, ClassShared.AID.LucidDreaming, 21);

        def.Define(SharedTrack.Swiftcast).As<SwiftcastOption>("Swiftcast", "Swiftcast", 20)
            .AddOption(SwiftcastOption.None, NoUse)
            .AddOption(SwiftcastOption.Use, "Use Swiftcast (60s cooldown)", 60, 10, ActionTargets.Self, 18, 93)
            .AddOption(SwiftcastOption.UseEx, "Use Swiftcast (40s cooldown)", 40, 10, ActionTargets.Self, 94)
            .AddAssociatedActions(ClassShared.AID.Swiftcast);

        DefineSimpleConfig(def, SharedTrack.Surecast, "Surecast", "", 10, ClassShared.AID.Surecast, 6);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Sleep), ClassShared.AID.Sleep, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.LucidDreaming), ClassShared.AID.LucidDreaming, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Surecast), ClassShared.AID.Surecast, Player);
        ExecuteAddle(strategy.Option(SharedTrack.Addle), primaryTarget);
        ExecuteSwiftcast(strategy.Option(SharedTrack.Swiftcast));
        ExecuteLimitBreak(strategy.Option(SharedTrack.LB), ClassShared.AID.Skyshard, ClassShared.AID.Starstorm, lb3, null, true);
    }

    private void ExecuteAddle(in StrategyValues.OptionRef addle, Actor? primaryTarget)
    {
        if (addle.As<AddleOption>() != AddleOption.None)
            ExecuteAction(addle, ClassShared.AID.Addle, primaryTarget);
    }
    private void ExecuteSwiftcast(in StrategyValues.OptionRef swift)
    {
        if (swift.As<SwiftcastOption>() != SwiftcastOption.None && !HasSwiftcast)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, swift.Priority(), swift.Value.ExpireIn);
    }
}
