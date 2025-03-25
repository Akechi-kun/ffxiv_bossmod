using BossMod.MNK;

namespace BossMod.Autorotation;

public sealed class ClassMNKBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { RiddleOfFire, Brotherhood, RiddleOfWind, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: MNK", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.PGL, Class.MNK), 100);
        DefineBuffs(res, Track.RiddleOfFire, AID.RiddleOfFire, "RiddleOfFire", "Riddle", 500, 60, 20, ActionTargets.Self, 68);
        DefineBuffs(res, Track.Brotherhood, AID.Brotherhood, "Brotherhood", "Brotherhood", 501, 120, 20, ActionTargets.Self, 70);
        DefineBuffs(res, Track.RiddleOfWind, AID.RiddleOfWind, "RiddleOfWind", "Riddle", 500, 90, 20, ActionTargets.Self, 72);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.RiddleOfFire), AID.RiddleOfFire, Player);
        ExecuteBuffs(strategy.Option(Track.Brotherhood), AID.Brotherhood, Player);
        ExecuteBuffs(strategy.Option(Track.RiddleOfWind), AID.RiddleOfWind, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
