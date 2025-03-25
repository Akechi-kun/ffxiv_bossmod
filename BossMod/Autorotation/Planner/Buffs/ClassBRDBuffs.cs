using BossMod.BRD;

namespace BossMod.Autorotation;

public sealed class ClassBRDBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { RagingStrikes, MagesBallad, Barrage, ArmysPaeon, BattleVoice, WanderersMinuet, RadiantFinale, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: BRD", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.ARC, Class.BRD), 100);
        DefineBuffs(res, Track.RagingStrikes, AID.RagingStrikes, "RagingStrikes", "R.Strikes", 500, 120, 20, ActionTargets.Self, 4);
        DefineBuffs(res, Track.MagesBallad, AID.MagesBallad, "MagesBallad", "M.Ballad", 500, 120, 45, ActionTargets.Self, 30);
        DefineBuffs(res, Track.Barrage, AID.Barrage, "Barrage", "Barrage", 500, 120, 10, ActionTargets.Self, 38);
        DefineBuffs(res, Track.ArmysPaeon, AID.ArmysPaeon, "ArmysPaeon", "A.Paeon", 500, 120, 45, ActionTargets.Self, 40);
        DefineBuffs(res, Track.BattleVoice, AID.BattleVoice, "BattleVoice", "B.Voice", 501, 120, 20, ActionTargets.Self, 50);
        DefineBuffs(res, Track.WanderersMinuet, AID.WanderersMinuet, "WanderersMinuet", "W.Minuet", 500, 120, 45, ActionTargets.Self, 52);
        DefineBuffs(res, Track.RadiantFinale, AID.RadiantFinale, "RadiantFinale", "R.Finale", 500, 110, 20, ActionTargets.Self, 90);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionDex, "Potion", "Potion", 500, 270, 0, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.RagingStrikes), AID.RagingStrikes, Player);
        ExecuteBuffs(strategy.Option(Track.MagesBallad), AID.MagesBallad, Player);
        ExecuteBuffs(strategy.Option(Track.Barrage), AID.Barrage, Player);
        ExecuteBuffs(strategy.Option(Track.ArmysPaeon), AID.ArmysPaeon, Player);
        ExecuteBuffs(strategy.Option(Track.BattleVoice), AID.BattleVoice, Player);
        ExecuteBuffs(strategy.Option(Track.WanderersMinuet), AID.WanderersMinuet, Player);
        ExecuteBuffs(strategy.Option(Track.RadiantFinale), AID.RadiantFinale, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
