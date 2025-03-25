using BossMod.SMN;

namespace BossMod.Autorotation;

public sealed class ClassSMNBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { SearingLight, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: SMN", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SMN), 100);
        DefineBuffs(res, Track.SearingLight, AID.SearingLight, "Searing Light", "S.Light", 500, 120, 20, ActionTargets.Self, 66);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionInt, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.SearingLight), AID.SearingLight, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
