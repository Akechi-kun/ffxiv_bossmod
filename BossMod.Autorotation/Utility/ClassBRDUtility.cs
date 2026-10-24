namespace BossMod.Autorotation;

public sealed class ClassBRDUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { WardensPaean = SharedTrack.Count, Troubadour, NaturesMinne }
    public enum TroubOption { None, Use87, Use87IfNotActive, Use88, Use88IfNotActive }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BRD.AID.SagittariusArrow);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BRD", UtilityDescription, UtilityCategory, "veyn", RotationModuleQuality.Ok, BitMask.Build((int)Class.BRD), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.WardensPaean, "WardensPaean", "Dispel", -10, BRD.AID.WardensPaean, 30);

        res.Define(Track.Troubadour).As<TroubOption>("Troubadour", "Troub", 500)
            .AddOption(TroubOption.None, NoUse)
            .AddOption(TroubOption.Use87, "Use Troubadour (120s cooldown), even if another ranged mitigation is active", 120, 15, ActionTargets.Self, 62, 87)
            .AddOption(TroubOption.Use87IfNotActive, "Use Troubadour (120s cooldown), unless another ranged mitigation is active", 120, 15, ActionTargets.Self, 62, 87)
            .AddOption(TroubOption.Use88, "Use Troubadour (90s cooldown), even if another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddOption(TroubOption.Use88IfNotActive, "Use Troubadour (90s cooldown), unless another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddAssociatedActions(BRD.AID.Troubadour);

        DefineSimpleConfig(res, Track.NaturesMinne, "NaturesMinne", "Minne", 400, BRD.AID.NaturesMinne, 15);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteWardensPaean(strategy.Option(Track.WardensPaean), primaryTarget);
        ExecuteSimple(strategy.Option(Track.NaturesMinne), BRD.AID.NaturesMinne, Player);
        ExecuteTroubadour(strategy.Option(Track.Troubadour));
    }

    private void ExecuteWardensPaean(in StrategyValues.OptionRef paean, Actor? primaryTarget)
    {
        if (paean.As<SimpleOption>() == SimpleOption.Use)
            ExecuteAction(paean, BRD.AID.WardensPaean, EsunaTarget(primaryTarget));
    }
    private void ExecuteTroubadour(in StrategyValues.OptionRef troub)
    {
        var condition = troub.As<TroubOption>() switch
        {
            TroubOption.Use87 or TroubOption.Use88 => true,
            TroubOption.Use87IfNotActive or TroubOption.Use88IfNotActive => !HasRangedMitigation,
            _ => false
        };
        if (condition)
            ExecuteAction(troub, BRD.AID.Troubadour, Player);
    }
}
