using BossMod.VPR;

namespace BossMod.Autorotation;

public sealed class ClassVPRBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { SerpentsIre, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: VPR", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.VPR), 100);
        DefineBuffs(res, Track.SerpentsIre, AID.SerpentsIre, "Serpent's Ire", "S.Ire", 500, 120, 30, ActionTargets.Self, 86);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 0, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.SerpentsIre), AID.SerpentsIre, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
