using BossMod.AST;

namespace BossMod.Autorotation;

public sealed class ClassASTBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { Lightspeed, Divination, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: AST", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.AST), 100);
        DefineBuffs(res, Track.Lightspeed, AID.Lightspeed, "Lightspeed", "L.speed", 500, 60, 15, ActionTargets.Self, 6);
        DefineBuffs(res, Track.Divination, AID.Divination, "Divination", "Div.", 500, 120, 20, ActionTargets.Self, 50);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionMnd, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.Lightspeed), AID.Lightspeed, Player);
        ExecuteBuffs(strategy.Option(Track.Divination), AID.Divination, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
