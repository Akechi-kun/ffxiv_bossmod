namespace BossMod.Autorotation;

public sealed class ClassBRDUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { WardensPaean = SharedTrack.Count, Troubadour, NaturesMinne }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BRD.AID.SagittariusArrow);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BRD", UtilityDescription, UtilityCategory, "veyn", RotationModuleQuality.Ok, BitMask.Build((int)Class.BRD), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.WardensPaean, "WardensPaean", "The Warden's Paean", -10, BRD.AID.WardensPaean, 30);

        DefineMitigation(res, Track.Troubadour, "Troubadour", "Troubadour", 500, BRD.AID.Troubadour, 62);

        DefineSimpleConfig(res, Track.NaturesMinne, "NaturesMinne", "Nature's Minne", 400, BRD.AID.NaturesMinne, 15);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteWardensPaean(strategy.Option(Track.WardensPaean), primaryTarget);
        ExecuteSimple(strategy.Option(Track.NaturesMinne), BRD.AID.NaturesMinne, Player);
        ExecuteMitigation(strategy.Option(Track.Troubadour), BRD.AID.Troubadour);
    }

    private void ExecuteWardensPaean(in StrategyValues.OptionRef paean, Actor? primaryTarget)
    {
        if (paean.As<SimpleOption>() == SimpleOption.Use)
            ExecuteAction(paean, BRD.AID.WardensPaean, EsunaTarget(primaryTarget));
    }
}
