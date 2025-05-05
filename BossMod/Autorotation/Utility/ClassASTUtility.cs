using BossMod.AST;

namespace BossMod.Autorotation;

public sealed class ClassASTUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Helios = SharedTrack.Count, Lightspeed, BeneficII, EssentialDignity, AspectedBenefic, Synastry, CollectiveUnconscious, CelestialOpposition, CelestialIntersection, NeutralSect, Exaltation, SunSign, Divination, AspectedHelios, EarthlyStar, Horoscope, Macrocosmos }
    public enum OptionSelect { None, Use, End }
    public enum HeliosOption { None, Use, UseEX }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AID.AstralStasis);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: AST", "Cooldown Planner support for Utility and Buff actions.\nNOTE: This is NOT a rotation preset!", "Cooldown Planner", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.AST), 100);
        DefineShared(res, IDLimitBreak3);
        DefineSimpleConfig(res, Track.Helios, "Helios", "", 140, AID.Helios);
        DefineSimpleConfig(res, Track.Lightspeed, "Lightspeed", "L.speed", 140, AID.Lightspeed, 15);
        DefineSimpleConfig(res, Track.BeneficII, "BeneficII", "Bene2", 100, AID.BeneficII);
        DefineSimpleConfig(res, Track.EssentialDignity, "EssentialDignity", "E.Dig", 140, AID.EssentialDignity);
        DefineSimpleConfig(res, Track.AspectedBenefic, "AspectedBenefic", "A.Benefic", 100, AID.AspectedBenefic, 15);
        DefineSimpleConfig(res, Track.Synastry, "Synastry", "", 200, AID.Synastry, 20);
        DefineSimpleConfig(res, Track.CollectiveUnconscious, "CollectiveUnconscious", "C.Uncon", 100, AID.CollectiveUnconscious, 5);
        DefineSimpleConfig(res, Track.CelestialOpposition, "CelestialOpposition", "C.Oppo", 100, AID.CelestialOpposition, 15);
        DefineSimpleConfig(res, Track.CelestialIntersection, "CelestialIntersection", "C.Inter", 100, AID.CelestialIntersection, 30);
        DefineSimpleConfig(res, Track.NeutralSect, "NeutralSect", "Sect", 250, AID.NeutralSect, 30);
        DefineSimpleConfig(res, Track.Exaltation, "Exaltation", "Exalt", 100, AID.Exaltation, 8);
        DefineSimpleConfig(res, Track.SunSign, "SunSign", "", 290, AID.SunSign, 15);
        DefineSimpleConfig(res, Track.Divination, "Divination", "Div.", 290, AID.Divination, 20);
        DefineEXConfig(res, Track.AspectedHelios, "A.Helios", "", 140, AID.AspectedHelios, AID.HeliosConjunction, 95);
        DefineEndConfig(res, Track.EarthlyStar, "EarthlyStar", "", 100, AID.EarthlyStar, AID.StellarDetonation, 30);
        DefineEndConfig(res, Track.Horoscope, "Horoscope", "", 100, AID.Horoscope, AID.HoroscopeEnd, 30);
        DefineEndConfig(res, Track.Macrocosmos, "Macrocosmos", "", 100, AID.Macrocosmos, AID.MicrocosmosEnd, 30);
        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Helios), AID.Helios, primaryTarget, 1.5f);
        ExecuteSimple(strategy.Option(Track.Lightspeed), AID.Lightspeed, Player);
        ExecuteSimple(strategy.Option(Track.BeneficII), AID.BeneficII, Player, 1.5f);
        ExecuteSimple(strategy.Option(Track.EssentialDignity), AID.EssentialDignity, Player);
        ExecuteSimple(strategy.Option(Track.AspectedBenefic), AID.AspectedBenefic, Player);
        ExecuteSimple(strategy.Option(Track.Synastry), AID.Synastry, Player);
        ExecuteSimple(strategy.Option(Track.CollectiveUnconscious), AID.CollectiveUnconscious, Player);
        ExecuteSimple(strategy.Option(Track.CelestialOpposition), AID.CelestialOpposition, Player);
        ExecuteSimple(strategy.Option(Track.CelestialIntersection), AID.CelestialIntersection, Player);
        ExecuteSimple(strategy.Option(Track.NeutralSect), AID.NeutralSect, Player);
        ExecuteSimple(strategy.Option(Track.Exaltation), AID.Exaltation, Player);
        ExecuteSimple(strategy.Option(Track.SunSign), AID.SunSign, Player);
        ExecuteSimple(strategy.Option(Track.Divination), AID.Divination, Player);
        ExecuteEX(strategy.Option(Track.AspectedHelios), AID.AspectedHelios, AID.HeliosConjunction, Player, castTime: 1.5f);
        ExecuteEnd(strategy.Option(Track.EarthlyStar), AID.EarthlyStar, AID.StellarDetonation, SID.EarthlyDominance, SID.EarthlyDominance, null, targetPos: ResolveTargetLocation(strategy.Option(Track.EarthlyStar).Value).ToVec3(Player.PosRot.Y));
        ExecuteEnd(strategy.Option(Track.Horoscope), AID.Horoscope, AID.HoroscopeEnd, SID.Horoscope, SID.Horoscope, Player);
        ExecuteEnd(strategy.Option(Track.Macrocosmos), AID.Macrocosmos, AID.MicrocosmosEnd, SID.Macrocosmos, SID.Macrocosmos, Player);
    }
}
