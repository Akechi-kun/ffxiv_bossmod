namespace BossMod.Autorotation;

public sealed class ClassSMNUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { RadiantAegis = SharedTrack.Count, Rekindle }
    public enum AegisStrategy { None, Use }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SMN.AID.Teraflare);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SMN", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SMN), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.RadiantAegis).As<AegisStrategy>("Radiant Aegis", "Aegis", 20)
            .AddOption(AegisStrategy.None, NoUse)
            .AddOption(AegisStrategy.Use, "Use Radiant Aegis if the barrier is not already up", 60, 30, ActionTargets.Self, 2)
            .AddAssociatedActions(SMN.AID.RadiantAegis);

        DefineSimpleConfig(res, Track.Rekindle, "Rekindle", "", 150, SMN.AID.Rekindle, 30);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Rekindle), SMN.AID.Rekindle, PartnerTarget(primaryTarget) ?? Player);
        ExecuteAegis(strategy.Option(Track.RadiantAegis));
    }

    private void ExecuteAegis(in StrategyValues.OptionRef aegis)
    {
        if (aegis.As<AegisStrategy>() == AegisStrategy.Use && !HasStatus(Player, SMN.SID.RadiantAegis))
            ExecuteAction(aegis, SMN.AID.RadiantAegis, Player);
    }
}
