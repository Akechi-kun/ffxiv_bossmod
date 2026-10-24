namespace BossMod.Autorotation;

public sealed class ClassMCHUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { Tactician = SharedTrack.Count, Dismantle }
    public enum TactOption { None, Use87, Use87IfNotActive, Use88, Use88IfNotActive }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MCH.AID.SatelliteBeam);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MCH", UtilityDescription, UtilityCategory, "Aimsucks", RotationModuleQuality.Ok, BitMask.Build((int)Class.MCH), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.Tactician).As<TactOption>("Tactician", "Tact", 400)
            .AddOption(TactOption.None, NoUse)
            .AddOption(TactOption.Use87, "Use Tactician (120s cooldown), even if another ranged mitigation is active", 120, 15, ActionTargets.Self, 56, 87)
            .AddOption(TactOption.Use87IfNotActive, "Use Tactician (120s cooldown), unless another ranged mitigation is active", 120, 15, ActionTargets.Self, 56, 87)
            .AddOption(TactOption.Use88, "Use Tactician (90s cooldown), even if another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddOption(TactOption.Use88IfNotActive, "Use Tactician (90s cooldown), unless another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddAssociatedActions(MCH.AID.Tactician);

        DefineSimpleConfig(res, Track.Dismantle, "Dismantle", "Dism", 500, MCH.AID.Dismantle, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Dismantle), MCH.AID.Dismantle, primaryTarget);
        ExecuteTactician(strategy.Option(Track.Tactician));
    }

    private void ExecuteTactician(in StrategyValues.OptionRef tact)
    {
        var condition = tact.As<TactOption>() switch
        {
            TactOption.Use87 or TactOption.Use88 => true,
            TactOption.Use87IfNotActive or TactOption.Use88IfNotActive => !HasRangedMitigation,
            _ => false
        };
        if (condition)
            ExecuteAction(tact, MCH.AID.Tactician, Player);
    }
}
