using BossMod.BLM;

namespace BossMod.Autorotation;

public sealed class ClassBLMBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { Transpose, Swiftcast, Manafont, UmbralSoul, LeyLines, Triplecast, Amplifier, Potion }
    public RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: BLM", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.THM, Class.BLM), 100);
        DefineBuffs(res, Track.Transpose, AID.Transpose, "Transpose", "T.pose", uiPriority: 500, 5, 0, ActionTargets.Self, 4);
        DefineBuffs(res, Track.Swiftcast, AID.Swiftcast, "Swiftcast", "S.cast", uiPriority: 500, Player.Level < 93 ? 60 : 40, 0, ActionTargets.Self, 18);
        DefineBuffs(res, Track.Manafont, AID.Manafont, "Manafont", "M.font", uiPriority: 500, 180, 0, ActionTargets.Self, 30, 83);
        DefineBuffs(res, Track.UmbralSoul, AID.UmbralSoul, "Umbral Soul", "U.Soul", uiPriority: 500, 0, 0, ActionTargets.Self, 35);
        DefineBuffs(res, Track.LeyLines, AID.LeyLines, "Ley Lines", "L.Lines", uiPriority: 500, 120, 0, ActionTargets.Self, 52);
        DefineBuffs(res, Track.Triplecast, AID.Triplecast, "Triplecast", "T.cast", uiPriority: 500, 60, 0, ActionTargets.Self, 66);
        DefineBuffs(res, Track.Amplifier, AID.Amplifier, "Amplifier", "Amp.", uiPriority: 500, 120, 0, ActionTargets.Self, 86);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionInt, "Potion", "Potion", uiPriority: 500, 270, 0, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteBuffs(strategy.Option(Track.Transpose), AID.Transpose, Player);
        ExecuteBuffs(strategy.Option(Track.Swiftcast), AID.Swiftcast, Player);
        ExecuteBuffs(strategy.Option(Track.Manafont), AID.Manafont, Player);
        ExecuteBuffs(strategy.Option(Track.UmbralSoul), AID.UmbralSoul, Player);
        ExecuteBuffs(strategy.Option(Track.LeyLines), AID.LeyLines, Player);
        ExecuteBuffs(strategy.Option(Track.Triplecast), AID.Triplecast, Player);
        ExecuteBuffs(strategy.Option(Track.Amplifier), AID.Amplifier, Player);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
