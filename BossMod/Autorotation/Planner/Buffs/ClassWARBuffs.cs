using BossMod.WAR;

namespace BossMod.Autorotation;

public sealed class ClassWARBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { Berserk, InnerRelease, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: WAR", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.MRD, Class.WAR), 100);
        DefineBuffs(res, Track.Berserk, AID.Berserk, "Berserk", "Berserk", 500, 60, 15, ActionTargets.Self, 6, 69);
        DefineBuffs(res, Track.InnerRelease, AID.InnerRelease, "Inner Release", "I.Release", 500, 60, 15, ActionTargets.Self, 70);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.InnerRelease), AID.InnerRelease, Player);
        ExecuteBuffs(strategy.Option(Track.Berserk), AID.Berserk, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
