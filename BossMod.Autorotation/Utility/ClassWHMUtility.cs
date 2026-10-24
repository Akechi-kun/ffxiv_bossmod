namespace BossMod.Autorotation;

public sealed class ClassWHMUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { PresenceOfMind = SharedTrack.Count, Regen, Cure, Medica, AfflatusSolace, AfflatusRapture, Benediction, Asylum, ThinAir, Tetragrammaton, DivineBenison, PlenaryIndulgence, Temperance, Aquaveil, LiturgyOfTheBell, DivineCaress, AetherialShift }
    public enum CureOption { None, Cure, CureII, CureIII }
    public enum MedicaOption { None, MedicaII, MedicaIII }
    public enum LiturgyOption { None, Use, End }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(WHM.AID.PulseOfLife);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WHM", UtilityDescription, UtilityCategory, "TrueP", RotationModuleQuality.Ok, BitMask.Build((int)Class.WHM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.PresenceOfMind, "PresenceOfMind", "PoM", 220, WHM.AID.PresenceOfMind, 15);
        DefineSimpleConfig(res, Track.Regen, "Regen", "", 110, WHM.AID.Regen, 18, ActionQueue.Priority.VeryHigh);

        res.Define(Track.Cure).As<CureOption>("Cure", "", 100)
            .AddOption(CureOption.None, NoUse)
            .AddOption(CureOption.Cure, "Use Cure", 2.5f, 0, ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly, 2, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(CureOption.CureII, "Use Cure II", 2.5f, 0, ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly, 30, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(CureOption.CureIII, "Use Cure III", 2.5f, 0, ActionTargets.Self | ActionTargets.Party, 40, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddAssociatedActions(WHM.AID.Cure, WHM.AID.CureII, WHM.AID.CureIII);

        res.Define(Track.Medica).As<MedicaOption>("Medica", "", 130)
            .AddOption(MedicaOption.None, NoUse)
            .AddOption(MedicaOption.MedicaII, "Use Medica II", 2.5f, 15, ActionTargets.Self, 50, 95, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(MedicaOption.MedicaIII, "Use Medica III", 2.5f, 15, ActionTargets.Self, 96, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddAssociatedActions(WHM.AID.MedicaII, WHM.AID.MedicaIII);

        DefineSimpleConfig(res, Track.AfflatusSolace, "AfflatusSolace", "Solace", 105, WHM.AID.AfflatusSolace, defaultPriority: ActionQueue.Priority.VeryHigh);
        DefineSimpleConfig(res, Track.AfflatusRapture, "AfflatusRapture", "Rapture", 125, WHM.AID.AfflatusRapture, defaultPriority: ActionQueue.Priority.VeryHigh);
        DefineSimpleConfig(res, Track.Benediction, "Benediction", "Bene", 300, WHM.AID.Benediction);

        res.Define(Track.Asylum).As<SimpleOption>("Asylum", "", 230)
            .AddOption(SimpleOption.None, NoUse)
            .AddOption(SimpleOption.Use, "Use Asylum", 90, 24, ActionTargets.Area, 52)
            .AddAssociatedActions(WHM.AID.Asylum);

        DefineSimpleConfig(res, Track.ThinAir, "ThinAir", "", 200, WHM.AID.ThinAir, 12);
        DefineSimpleConfig(res, Track.Tetragrammaton, "Tetragrammaton", "Tetra", 150, WHM.AID.Tetragrammaton);
        DefineSimpleConfig(res, Track.DivineBenison, "DivineBenison", "Benison", 140, WHM.AID.DivineBenison, 15);
        DefineSimpleConfig(res, Track.PlenaryIndulgence, "PlenaryIndulgence", "Plenary", 160, WHM.AID.PlenaryIndulgence, 10);
        DefineSimpleConfig(res, Track.Temperance, "Temperance", "", 290, WHM.AID.Temperance, 20);
        DefineSimpleConfig(res, Track.Aquaveil, "Aquaveil", "", 170, WHM.AID.Aquaveil, 8);

        res.Define(Track.LiturgyOfTheBell).As<LiturgyOption>("LiturgyOfTheBell", "Liturgy", 310)
            .AddOption(LiturgyOption.None, NoUse)
            .AddOption(LiturgyOption.Use, "Place Liturgy of the Bell", 180, 20, ActionTargets.Area, 90)
            .AddOption(LiturgyOption.End, "Detonate Liturgy of the Bell early", 0, 1, ActionTargets.Self, 90)
            .AddAssociatedActions(WHM.AID.LiturgyOfTheBell, WHM.AID.LiturgyOfTheBellEnd);

        DefineSimpleConfig(res, Track.DivineCaress, "DivineCaress", "Caress", 280, WHM.AID.DivineCaress, 10);
        DefineSimpleConfig(res, Track.AetherialShift, "AetherialShift", "Dash", 20, WHM.AID.AetherialShift);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var healTarget = FriendlyTarget(primaryTarget);
        ExecuteSimple(strategy.Option(Track.PresenceOfMind), WHM.AID.PresenceOfMind, Player);
        ExecuteSimple(strategy.Option(Track.AfflatusSolace), WHM.AID.AfflatusSolace, healTarget);
        ExecuteSimple(strategy.Option(Track.AfflatusRapture), WHM.AID.AfflatusRapture, Player);
        ExecuteSimple(strategy.Option(Track.Benediction), WHM.AID.Benediction, healTarget);
        ExecuteSimple(strategy.Option(Track.Tetragrammaton), WHM.AID.Tetragrammaton, healTarget);
        ExecuteSimple(strategy.Option(Track.DivineBenison), WHM.AID.DivineBenison, healTarget);
        ExecuteSimple(strategy.Option(Track.PlenaryIndulgence), WHM.AID.PlenaryIndulgence, Player);
        ExecuteSimple(strategy.Option(Track.Temperance), WHM.AID.Temperance, Player);
        ExecuteSimple(strategy.Option(Track.Aquaveil), WHM.AID.Aquaveil, healTarget);
        ExecuteSimple(strategy.Option(Track.AetherialShift), WHM.AID.AetherialShift, Player);
        ExecuteThinAir(strategy.Option(Track.ThinAir));
        ExecuteCure(strategy.Option(Track.Cure), healTarget);
        ExecuteRegen(strategy.Option(Track.Regen), healTarget);
        ExecuteMedica(strategy.Option(Track.Medica));
        ExecuteAsylum(strategy.Option(Track.Asylum));
        ExecuteCaress(strategy.Option(Track.DivineCaress));
        ExecuteLiturgy(strategy.Option(Track.LiturgyOfTheBell));
    }

    private void ExecuteThinAir(in StrategyValues.OptionRef thinAir)
    {
        if (Player.FindStatus(WHM.SID.ThinAir, DateTime.MaxValue) == null)
            ExecuteSimple(thinAir, WHM.AID.ThinAir, Player);
    }
    private void ExecuteCure(in StrategyValues.OptionRef cure, Actor healTarget)
    {
        var action = cure.As<CureOption>() switch
        {
            CureOption.Cure => WHM.AID.Cure,
            CureOption.CureII => WHM.AID.CureII,
            CureOption.CureIII => WHM.AID.CureIII,
            _ => default
        };
        if (action != default)
            ExecuteAction(cure, action, healTarget);
    }
    private void ExecuteRegen(in StrategyValues.OptionRef regen, Actor healTarget)
    {
        var target = ResolveTarget(regen.Value) ?? healTarget;
        if (regen.As<SimpleOption>() == SimpleOption.Use && !HasStatus(target, WHM.SID.Regen))
            ExecuteOn(regen, WHM.AID.Regen, target);
    }
    private void ExecuteMedica(in StrategyValues.OptionRef medica)
    {
        var action = medica.As<MedicaOption>() switch
        {
            MedicaOption.MedicaII => WHM.AID.MedicaII,
            MedicaOption.MedicaIII => WHM.AID.MedicaIII,
            _ => default
        };
        if (action != default && !HasStatus(Player, WHM.SID.MedicaII) && !HasStatus(Player, WHM.SID.MedicaIII))
            ExecuteAction(medica, action, Player);
    }
    private void ExecuteAsylum(in StrategyValues.OptionRef asylum)
    {
        if (asylum.As<SimpleOption>() == SimpleOption.Use)
            ExecuteArea(asylum, WHM.AID.Asylum);
    }
    private void ExecuteCaress(in StrategyValues.OptionRef caress)
    {
        if (caress.As<SimpleOption>() == SimpleOption.Use && Player.FindStatus(WHM.SID.DivineGrace) != null)
            ExecuteAction(caress, WHM.AID.DivineCaress, Player);
    }
    private void ExecuteLiturgy(in StrategyValues.OptionRef liturgy)
    {
        var option = liturgy.As<LiturgyOption>();
        var hasLiturgy = Player.FindStatus(WHM.SID.LiturgyOfTheBell) != null;
        if (option == LiturgyOption.Use && !hasLiturgy)
            ExecuteArea(liturgy, WHM.AID.LiturgyOfTheBell);
        if (option == LiturgyOption.End && hasLiturgy)
            ExecuteAction(liturgy, WHM.AID.LiturgyOfTheBellEnd, Player);
    }
}
