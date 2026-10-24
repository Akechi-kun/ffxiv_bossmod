namespace BossMod.Autorotation;

public sealed class ClassASTUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Helios = SharedTrack.Count, Lightspeed, BeneficII, EssentialDignity, AspectedBenefic, AspectedHelios, Synastry, CollectiveUnconscious, CelestialOpposition, EarthlyStar, CelestialIntersection, Horoscope, NeutralSect, Exaltation, Macrocosmos, SunSign }
    public enum StarOption { None, Use, End }
    public enum HoroscopeOption { None, Use, End }
    public enum MacrocosmosOption { None, Use, End }
    public enum HeliosOption { None, Use, UseEx }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AST.AID.AstralStasis);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: AST", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.AST), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Helios, "Helios", "", 140, AST.AID.Helios);
        DefineSimpleConfig(res, Track.Lightspeed, "Lightspeed", "L.speed", 140, AST.AID.Lightspeed, 15);
        DefineSimpleConfig(res, Track.BeneficII, "BeneficII", "Bene2", 100, AST.AID.BeneficII);
        DefineSimpleConfig(res, Track.EssentialDignity, "EssentialDignity", "E.Dig", 140, AST.AID.EssentialDignity);
        DefineSimpleConfig(res, Track.AspectedBenefic, "AspectedBenefic", "A.Benefic", 100, AST.AID.AspectedBenefic, 15);

        res.Define(Track.AspectedHelios).As<HeliosOption>("AspectedHelios", "A.Helios", 130)
            .AddOption(HeliosOption.None, NoUse)
            .AddOption(HeliosOption.Use, "Use Aspected Helios", 2.5f, 15, ActionTargets.Self, 40, 95)
            .AddOption(HeliosOption.UseEx, "Use Helios Conjunction", 2.5f, 15, ActionTargets.Self, 96)
            .AddAssociatedActions(AST.AID.AspectedHelios, AST.AID.HeliosConjunction);

        DefineSimpleConfig(res, Track.Synastry, "Synastry", "", 200, AST.AID.Synastry, 20);
        DefineSimpleConfig(res, Track.CollectiveUnconscious, "CollectiveUnconscious", "C.Uncon", 100, AST.AID.CollectiveUnconscious, 5);
        DefineSimpleConfig(res, Track.CelestialOpposition, "CelestialOpposition", "C.Oppo", 100, AST.AID.CelestialOpposition, 15);

        res.Define(Track.EarthlyStar).As<StarOption>("EarthlyStar", "E.Star", 200)
            .AddOption(StarOption.None, NoUse)
            .AddOption(StarOption.Use, "Place Earthly Star", 60, 20, ActionTargets.Area, 62)
            .AddOption(StarOption.End, "Detonate the star early (Stellar Detonation)", 0, 1, ActionTargets.Self, 62)
            .AddAssociatedActions(AST.AID.EarthlyStar, AST.AID.StellarDetonation);

        DefineSimpleConfig(res, Track.CelestialIntersection, "CelestialIntersection", "C.Inter", 100, AST.AID.CelestialIntersection, 30);

        res.Define(Track.Horoscope).As<HoroscopeOption>("Horoscope", "Horo", 130)
            .AddOption(HoroscopeOption.None, NoUse)
            .AddOption(HoroscopeOption.Use, "Use Horoscope", 60, 10, ActionTargets.Self, 76)
            .AddOption(HoroscopeOption.End, "Trigger the Horoscope heal early", 0, 1, ActionTargets.Self, 76)
            .AddAssociatedActions(AST.AID.Horoscope, AST.AID.HoroscopeEnd);

        DefineSimpleConfig(res, Track.NeutralSect, "NeutralSect", "Sect", 250, AST.AID.NeutralSect, 20);
        DefineSimpleConfig(res, Track.Exaltation, "Exaltation", "Exalt", 100, AST.AID.Exaltation, 8);

        res.Define(Track.Macrocosmos).As<MacrocosmosOption>("Macrocosmos", "Macro", 300)
            .AddOption(MacrocosmosOption.None, NoUse)
            .AddOption(MacrocosmosOption.Use, "Use Macrocosmos", 180, 15, ActionTargets.Self, 90, defaultPriority: ActionQueue.Priority.ManualGCD - 1)
            .AddOption(MacrocosmosOption.End, "Trigger the heal early (Microcosmos)", 0, 1, ActionTargets.Self, 90)
            .AddAssociatedActions(AST.AID.Macrocosmos, AST.AID.MicrocosmosEnd);

        DefineSimpleConfig(res, Track.SunSign, "SunSign", "", 290, AST.AID.SunSign, 15);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var healTarget = FriendlyTarget(primaryTarget);
        ExecuteSimple(strategy.Option(Track.Helios), AST.AID.Helios, Player);
        ExecuteSimple(strategy.Option(Track.Lightspeed), AST.AID.Lightspeed, Player);
        ExecuteSimple(strategy.Option(Track.BeneficII), AST.AID.BeneficII, healTarget);
        ExecuteSimple(strategy.Option(Track.EssentialDignity), AST.AID.EssentialDignity, healTarget);
        ExecuteSimple(strategy.Option(Track.AspectedBenefic), AST.AID.AspectedBenefic, healTarget);
        ExecuteSimple(strategy.Option(Track.Synastry), AST.AID.Synastry, PartnerTarget(primaryTarget));
        ExecuteSimple(strategy.Option(Track.CollectiveUnconscious), AST.AID.CollectiveUnconscious, Player);
        ExecuteSimple(strategy.Option(Track.CelestialOpposition), AST.AID.CelestialOpposition, Player);
        ExecuteSimple(strategy.Option(Track.CelestialIntersection), AST.AID.CelestialIntersection, healTarget);
        ExecuteSimple(strategy.Option(Track.NeutralSect), AST.AID.NeutralSect, Player);
        ExecuteSimple(strategy.Option(Track.Exaltation), AST.AID.Exaltation, healTarget);
        ExecuteSimple(strategy.Option(Track.SunSign), AST.AID.SunSign, Player);
        ExecuteStar(strategy.Option(Track.EarthlyStar));
        ExecuteAspectedHelios(strategy.Option(Track.AspectedHelios));
        ExecuteHoroscope(strategy.Option(Track.Horoscope));
        ExecuteMacrocosmos(strategy.Option(Track.Macrocosmos));
    }

    private void ExecuteStar(in StrategyValues.OptionRef star)
    {
        var option = star.As<StarOption>();
        if (option == StarOption.Use)
            ExecuteArea(star, AST.AID.EarthlyStar);
        if (option == StarOption.End)
            ExecuteAction(star, AST.AID.StellarDetonation, Player);
    }
    private void ExecuteAspectedHelios(in StrategyValues.OptionRef helios)
    {
        var action = helios.As<HeliosOption>() switch
        {
            HeliosOption.Use => AST.AID.AspectedHelios,
            HeliosOption.UseEx => AST.AID.HeliosConjunction,
            _ => default
        };
        if (action != default && !HasStatus(Player, AST.SID.AspectedHelios) && !HasStatus(Player, AST.SID.HeliosConjunction))
            ExecuteAction(helios, action, Player);
    }
    private void ExecuteHoroscope(in StrategyValues.OptionRef horoscope)
    {
        var option = horoscope.As<HoroscopeOption>();
        var active = HasStatus(Player, AST.SID.Horoscope) || HasStatus(Player, AST.SID.HoroscopeHelios);
        if (option == HoroscopeOption.Use && !active)
            ExecuteAction(horoscope, AST.AID.Horoscope, Player);
        if (option == HoroscopeOption.End && active)
            ExecuteAction(horoscope, AST.AID.HoroscopeEnd, Player);
    }
    private void ExecuteMacrocosmos(in StrategyValues.OptionRef cosmos)
    {
        var option = cosmos.As<MacrocosmosOption>();
        var active = Player.FindStatus(AST.SID.Macrocosmos) != null;
        if (option == MacrocosmosOption.Use && !active)
            ExecuteAction(cosmos, AST.AID.Macrocosmos, Player);
        if (option == MacrocosmosOption.End && active)
            ExecuteAction(cosmos, AST.AID.MicrocosmosEnd, Player);
    }
}
