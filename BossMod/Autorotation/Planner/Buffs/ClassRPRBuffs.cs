using BossMod.RPR;

namespace BossMod.Autorotation;

public sealed class ClassRPRBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { ArcaneCircle, Soulsow, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: RPR", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.RPR), 100);
        DefineBuffs(res, Track.ArcaneCircle, AID.ArcaneCircle, "ArcaneCircle", "A.Circle", 500, 120, 20, ActionTargets.Self, 72);
        DefineBuffs(res, Track.Soulsow, AID.SoulSow, "Soulsow", "Soulsow", 500, 0, 0, ActionTargets.Self, 82);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionInt, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.ArcaneCircle), AID.ArcaneCircle, Player);
        ExecuteBuffs(strategy.Option(Track.Soulsow), AID.SoulSow, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
