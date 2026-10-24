namespace BossMod.Autorotation;

public abstract class RoleMeleeUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, SecondWind, LegSweep, Bloodbath, Feint, ArmsLength, Count }
    public enum FeintOption { None, Use, UseEx }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Hostile)
            .AddAssociatedActions(ClassShared.AID.Braver, ClassShared.AID.Bladedance)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.SecondWind, "S.Wind", "Second Wind", 20, ClassShared.AID.SecondWind);
        DefineSimpleConfig(def, SharedTrack.LegSweep, "Stun", "Leg Sweep", -150, ClassShared.AID.LegSweep, 3);
        DefineSimpleConfig(def, SharedTrack.Bloodbath, "Bloodbath", "", -50, ClassShared.AID.Bloodbath, 20);

        def.Define(SharedTrack.Feint).As<FeintOption>("Feint", "", 250)
            .AddOption(FeintOption.None, NoUse)
            .AddOption(FeintOption.Use, "Use Feint (10s)", 90, 10, ActionTargets.Hostile, 22, 97)
            .AddOption(FeintOption.UseEx, "Use Feint (15s)", 90, 15, ActionTargets.Hostile, 98)
            .AddAssociatedActions(ClassShared.AID.Feint);

        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsL", "Arm's Length", 300, ClassShared.AID.ArmsLength, 6);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.SecondWind), ClassShared.AID.SecondWind, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LegSweep), ClassShared.AID.LegSweep, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Bloodbath), ClassShared.AID.Bloodbath, Player);
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);
        ExecuteFeint(strategy.Option(SharedTrack.Feint), primaryTarget);
        ExecuteLimitBreak(strategy.Option(SharedTrack.LB), ClassShared.AID.Braver, ClassShared.AID.Bladedance, lb3, primaryTarget);
    }

    private void ExecuteFeint(in StrategyValues.OptionRef feint, Actor? primaryTarget)
    {
        if (feint.As<FeintOption>() != FeintOption.None)
            ExecuteAction(feint, ClassShared.AID.Feint, primaryTarget);
    }
}
