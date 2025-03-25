using BossMod.DNC;

namespace BossMod.Autorotation;

public sealed class ClassDNCBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { Devilment, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: DNC", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.DNC), 100);
        DefineBuffs(res, Track.Devilment, AID.Devilment, "Devilment", "Devilment", 500, 120, 20, ActionTargets.Self, 62);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionDex, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.Devilment), AID.Devilment, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
