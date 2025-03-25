using BossMod.NIN;

namespace BossMod.Autorotation;

public sealed class ClassNINBuffs(RotationModuleManager manager, Actor player) : GenericBuffs(manager, player)
{
    public enum Track { Mug, TrickAttack, Dokumori, TenChiJin, KunaisBane, Potion }
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Buffs: NIN", "Cooldown Planner support for Buffs only\nNOTE: This is NOT a rotation preset! All Buffs modules are STRICTLY for cooldown-planning usage.", "Cooldown Planner|Buffs", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.ROG, Class.NIN), 100);
        DefineBuffs(res, Track.Mug, AID.Mug, "Mug", "Mug", 500, 120, 20, ActionTargets.Hostile, 15, 65);
        DefineBuffs(res, Track.TrickAttack, AID.TrickAttack, "Trick Attack", "T.Attack", 500, 60, 20, ActionTargets.Hostile, 6);
        DefineBuffs(res, Track.Dokumori, AID.Dokumori, "Dokumori", "Dokumori", 500, 120, 20, ActionTargets.Hostile, 66);
        DefineBuffs(res, Track.TenChiJin, AID.TenChiJin, "TenChiJin", "T.C.Jin", 502, 120, 20, ActionTargets.Self, 70);
        DefineBuffs(res, Track.KunaisBane, AID.KunaisBane, "Kunai's Bane", "K.Bane", 500, 120, 20, ActionTargets.Hostile, 52);
        DefinePots(res, Track.Potion, ActionDefinitions.IDPotionDex, "Potion", "Potion", 500, 270, 30, ActionTargets.Self, 70);
        return res;
    }
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        //TODO: add prevention for when casting mudra
        ExecuteBuffs(strategy.Option(Track.Mug), AID.Mug, primaryTarget);
        ExecuteBuffs(strategy.Option(Track.TrickAttack), AID.TrickAttack, primaryTarget);
        ExecuteBuffs(strategy.Option(Track.Dokumori), AID.Dokumori, primaryTarget);
        ExecuteBuffs(strategy.Option(Track.TenChiJin), AID.TenChiJin, primaryTarget);
        ExecuteBuffs(strategy.Option(Track.KunaisBane), AID.KunaisBane, primaryTarget);
        ExecutePots(strategy.Option(Track.Potion));
    }
}
