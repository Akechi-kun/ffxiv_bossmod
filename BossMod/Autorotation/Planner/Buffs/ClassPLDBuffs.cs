using BossMod.PLD;

namespace BossMod.Autorotation;

public sealed class ClassPLDBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { FightOrFlight, Requiescat, Imperator, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: PLD", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.GLA, Class.PLD), 100);
        DefineBuffs(res, Track.FightOrFlight, AID.FightOrFlight, "Fight or Flight", "F.Flight", 500, 60, 20, ActionTargets.Self, 2);
        DefineBuffs(res, Track.Requiescat, AID.Requiescat, "Requiescat", "Req.", 500, 60, 20, ActionTargets.Hostile, 66, 95);
        DefineBuffs(res, Track.Imperator, AID.Imperator, "Imperator", "Imp.", 500, 60, 20, ActionTargets.Hostile, 96);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionStr, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.FightOrFlight), AID.FightOrFlight, Player);
        ExecuteBuffs(strategy.Option(Track.Requiescat), AID.Requiescat, primaryTarget);
        ExecuteBuffs(strategy.Option(Track.Imperator), AID.Imperator, primaryTarget);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
