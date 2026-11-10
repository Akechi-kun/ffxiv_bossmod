namespace BossMod.Autorotation;

public abstract class RoleHealerUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Repose, Esuna, LucidDreaming, Swiftcast, Surecast, Rescue, Count }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimple(def, SharedTrack.Sprint, "Sprint", "Sprint", 10, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self, 8)
            .AddAssociatedActions(ClassShared.AID.HealingWind, ClassShared.AID.BreathOfTheEarth)
            .AddAssociatedAction(lb3);

        DefineSimple(def, SharedTrack.Repose, "Repose", "Repose", -100, ClassShared.AID.Repose, 30);
        DefineSimple(def, SharedTrack.Esuna, "Esuna", "Esuna", 40, ClassShared.AID.Esuna);
        DefineSimple(def, SharedTrack.LucidDreaming, "LucidDreaming", "Lucid Dreaming", 30, ClassShared.AID.LucidDreaming, 21);

        DefineSwiftcast(def, SharedTrack.Swiftcast, "Swiftcast");

        DefineSimple(def, SharedTrack.Surecast, "Surecast", "Surecast", 10, ClassShared.AID.Surecast, 6);
        DefineSimple(def, SharedTrack.Rescue, "Rescue", "Rescue", 50, ClassShared.AID.Rescue);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Repose), ClassShared.AID.Repose, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Esuna), ClassShared.AID.Esuna, AllyOrCleansable(primaryTarget));
        ExecuteSimple(strategy.Option(SharedTrack.LucidDreaming), ClassShared.AID.LucidDreaming, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Surecast), ClassShared.AID.Surecast, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Rescue), ClassShared.AID.Rescue, OtherAlly(primaryTarget));
        ExecuteSwiftcast(strategy.Option(SharedTrack.Swiftcast));
        ExecuteLimitBreak(strategy.Option(SharedTrack.LB), ClassShared.AID.HealingWind, ClassShared.AID.BreathOfTheEarth, lb3, Player);
    }
}
