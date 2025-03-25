namespace BossMod.Autorotation;

public sealed class ClassSMNUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { RadiantAegis = SharedTrack.Count }
    public enum AegisStrategy { None, Use }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SMN.AID.Teraflare);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SMN", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Utility", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.SMN), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleOGCD(res, Track.RadiantAegis, "Radiant Aegis", "Aegis", 600, SMN.AID.RadiantAegis, 30);

        //TODO: Rekindle here or inside own module?

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        if (Player.FindStatus(SMN.SID.RadiantAegis) == null)
        {
            ExecuteSimple(strategy.Option(Track.RadiantAegis), SMN.AID.RadiantAegis, Player);
        }
    }
}
