using BossMod.RDM;

namespace BossMod.Autorotation;

public sealed class ClassRDMBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { Acceleration, Embolden, Manafication, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: RDM", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.RDM), 100);
        DefineBuffs(res, Track.Acceleration, AID.Acceleration, "Acceleration", "Accel.", 500, 55, 20, ActionTargets.Self, 50);
        DefineBuffs(res, Track.Embolden, AID.Embolden, "Embolden", "Embolden", 500, 120, 20, ActionTargets.Self, 58);
        DefineBuffs(res, Track.Manafication, AID.Manafication, "Manafication", "Manafic.", 500, 120, 20, ActionTargets.Self, 60);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionInt, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.Acceleration), AID.Acceleration, Player);
        ExecuteBuffs(strategy.Option(Track.Embolden), AID.Embolden, Player);
        ExecuteBuffs(strategy.Option(Track.Manafication), AID.Manafication, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
