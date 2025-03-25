using BossMod.SAM;

namespace BossMod.Autorotation;

public sealed class ClassSAMBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { MeikyoShisui, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: SAM", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SAM), 100);
        DefineBuffs(res, Track.MeikyoShisui, AID.MeikyoShisui, "Meikyo Shisui", "M.Shisui", 500, 55, 20, ActionTargets.Self, 50);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.MeikyoShisui), AID.MeikyoShisui, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
