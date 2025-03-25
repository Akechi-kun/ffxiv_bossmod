using BossMod.DRK;

namespace BossMod.Autorotation;

public sealed class ClassDRKBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { BloodWeapon, Delirium, LivingShadow, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: DRK", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.DRK), 100);
        DefineBuffs(res, Track.BloodWeapon, AID.BloodWeapon, "BloodWeapon", "B.Wpn", 500, 60, 15, ActionTargets.Self, 35, 67);
        DefineBuffs(res, Track.Delirium, AID.Delirium, "Delirium", "Del.", 500, 60, 15, ActionTargets.Self, 68);
        DefineBuffs(res, Track.LivingShadow, AID.LivingShadow, "LivingShadow", "L.Shadow", 500, 120, 20, ActionTargets.Self, 80);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.BloodWeapon), AID.BloodWeapon, Player);
        ExecuteBuffs(strategy.Option(Track.Delirium), AID.Delirium, Player);
        ExecuteBuffs(strategy.Option(Track.LivingShadow), AID.LivingShadow, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
