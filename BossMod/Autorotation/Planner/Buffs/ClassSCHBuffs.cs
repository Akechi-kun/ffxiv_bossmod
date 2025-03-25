using BossMod.SCH;

namespace BossMod.Autorotation;

public sealed class ClassSCHBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { ChainStratagem, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: SCH", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);
        DefineBuffs(res, Track.ChainStratagem, AID.ChainStratagem, "Chain Stratagem", "C.Stratagem", 500, 120, 20, ActionTargets.Self, 66);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionMnd, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.ChainStratagem), AID.ChainStratagem, primaryTarget);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
