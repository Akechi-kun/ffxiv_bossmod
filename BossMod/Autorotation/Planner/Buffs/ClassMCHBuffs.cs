using BossMod.MCH;

namespace BossMod.Autorotation;

public sealed class ClassMCHBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { Wildfire, BarrelStabilizer, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: MCH", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.MCH), 100);
        DefineBuffs(res, Track.Wildfire, AID.Wildfire, "Wildfire", "W.fire", 500, 120, 10, ActionTargets.Self, 45);
        DefineBuffs(res, Track.BarrelStabilizer, AID.BarrelStabilizer, "Barrel Stabilizer", "Barrel S.", 500, 120, 30, ActionTargets.Self, 66);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionDex, "Potion", "Potion", 500, 270, 0, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.Wildfire), AID.Wildfire, Player);
        ExecuteBuffs(strategy.Option(Track.BarrelStabilizer), AID.BarrelStabilizer, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
