using BossMod.GNB;

namespace BossMod.Autorotation;

public sealed class ClassGNBBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { NoMercy, Bloodfest, BloodfestEX, Potion }
    public RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: GNB", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.GNB), 100);
        DefineBuffs(res, Track.NoMercy, AID.NoMercy, "No Mercy", "N.Mercy", 500, 60, 20, ActionTargets.Self, 2);
        DefineBuffs(res, Track.Bloodfest, AID.Bloodfest, "Bloodfest", "B.fest", 500, 120, Player.Level < 100 ? 30 : 0, ActionTargets.Self, 100);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.NoMercy), AID.NoMercy, Player);
        ExecuteBuffs(strategy.Option(Track.Bloodfest), AID.Bloodfest, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
