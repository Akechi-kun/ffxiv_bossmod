using BossMod.DRG;

namespace BossMod.Autorotation;

public sealed class ClassDRGBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { LanceCharge, BattleLitany, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: DRG", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.LNC, Class.DRG), 100);
        DefineBuffs(res, Track.LanceCharge, AID.LanceCharge, "Lance Charge", "L.Charge", 500, 120, 20, ActionTargets.Self, 30);
        DefineBuffs(res, Track.BattleLitany, AID.BattleLitany, "Battle Litany", "B.Litany", 500, 120, 20, ActionTargets.Self, 52);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 0, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.LanceCharge), AID.LanceCharge, Player);
        ExecuteBuffs(strategy.Option(Track.BattleLitany), AID.BattleLitany, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
