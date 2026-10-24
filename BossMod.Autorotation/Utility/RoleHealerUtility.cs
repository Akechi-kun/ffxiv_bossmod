namespace BossMod.Autorotation;

public abstract class RoleHealerUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Repose, Esuna, LucidDreaming, Swiftcast, Surecast, Rescue, Count }
    public enum SwiftcastOption { None, Use, UseEx }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 10, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self, 8)
            .AddAssociatedActions(ClassShared.AID.HealingWind, ClassShared.AID.BreathOfTheEarth)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.Repose, "Repose", "", -100, ClassShared.AID.Repose, 30);
        DefineSimpleConfig(def, SharedTrack.Esuna, "Esuna", "", 40, ClassShared.AID.Esuna);
        DefineSimpleConfig(def, SharedTrack.LucidDreaming, "LucidDreaming", "Lucid", 30, ClassShared.AID.LucidDreaming, 21);

        def.Define(SharedTrack.Swiftcast).As<SwiftcastOption>("Swiftcast", "Swift", 20)
            .AddOption(SwiftcastOption.None, NoUse)
            .AddOption(SwiftcastOption.Use, "Use Swiftcast (60s cooldown)", 60, 10, ActionTargets.Self, 18, 93)
            .AddOption(SwiftcastOption.UseEx, "Use Swiftcast (40s cooldown)", 40, 10, ActionTargets.Self, 94)
            .AddAssociatedActions(ClassShared.AID.Swiftcast);

        DefineSimpleConfig(def, SharedTrack.Surecast, "Surecast", "", 10, ClassShared.AID.Surecast, 6);
        DefineSimpleConfig(def, SharedTrack.Rescue, "Rescue", "", 50, ClassShared.AID.Rescue);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Repose), ClassShared.AID.Repose, primaryTarget is { IsAlly: false } ? primaryTarget : null);
        ExecuteEsuna(strategy.Option(SharedTrack.Esuna), primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.LucidDreaming), ClassShared.AID.LucidDreaming, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Surecast), ClassShared.AID.Surecast, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Rescue), ClassShared.AID.Rescue, primaryTarget is { IsAlly: true } && primaryTarget != Player ? primaryTarget : null);
        ExecuteSwiftcast(strategy.Option(SharedTrack.Swiftcast));
        ExecuteLimitBreak(strategy.Option(SharedTrack.LB), ClassShared.AID.HealingWind, ClassShared.AID.BreathOfTheEarth, lb3, Player);
    }

    private void ExecuteEsuna(in StrategyValues.OptionRef esuna, Actor? primaryTarget)
    {
        if (esuna.As<SimpleOption>() == SimpleOption.Use)
            ExecuteAction(esuna, ClassShared.AID.Esuna, EsunaTarget(primaryTarget));
    }
    private void ExecuteSwiftcast(in StrategyValues.OptionRef swift)
    {
        if (swift.As<SwiftcastOption>() != SwiftcastOption.None && !HasSwiftcast)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, swift.Priority(), swift.Value.ExpireIn);
    }
}
