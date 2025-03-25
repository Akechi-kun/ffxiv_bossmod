using BossMod.PCT;

namespace BossMod.Autorotation;

public sealed class ClassPCTBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { StarryMuse, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: PCT", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planPCTg usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.LNC, Class.PCT), 100);
        DefineBuffs(res, Track.StarryMuse, AID.StarryMuse, "Starry Muse", "S.Muse", 500, 120, 20, ActionTargets.Self, 70);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.StarryMuse), AID.StarryMuse, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
