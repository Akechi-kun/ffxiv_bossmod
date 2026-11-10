namespace BossMod.Autorotation;

public sealed class ClassSMNUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { RadiantAegis = SharedTrack.Count, Rekindle }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SMN.AID.Teraflare);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SMN", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SMN), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.RadiantAegis).As<SimpleOption>("Radiant Aegis", "Radiant Aegis", 20)
            .AddOption(SimpleOption.None, NoUse)
            .AddOption(SimpleOption.Use, "Use Radiant Aegis if the barrier is not already up", 60, 30, ActionTargets.Self, 2)
            .AddAssociatedActions(SMN.AID.RadiantAegis);

        DefineSimple(res, Track.Rekindle, "Rekindle", "Rekindle", 150, SMN.AID.Rekindle, 30);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Rekindle), SMN.AID.Rekindle, AllyOrCoTank(primaryTarget) ?? Player);
        if (!HasStatus(Player, SMN.SID.RadiantAegis))
            ExecuteSimple(strategy.Option(Track.RadiantAegis), SMN.AID.RadiantAegis, Player);
    }
}
