namespace BossMod.Autorotation;

public sealed class ClassWHMUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { PresenceOfMind = SharedTrack.Count, Regen, Cure, Medica, AfflatusSolace, AfflatusRapture, Benediction, Asylum, ThinAir, Tetragrammaton, DivineBenison, PlenaryIndulgence, Temperance, Aquaveil, LiturgyOfTheBell, DivineCaress }
    public enum CureOption { None, Cure, CureII, CureIII }
    public enum MedicaOption { None, MedicaII, MedicaIII }
    public enum LiturgyOption { None, Use, End }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(WHM.AID.PulseOfLife);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WHM", UtilityDescription, UtilityCategory, "TrueP", RotationModuleQuality.Ok, BitMask.Build((int)Class.WHM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimple(res, Track.PresenceOfMind, "PresenceOfMind", "Presence of Mind", 220, WHM.AID.PresenceOfMind, 15);
        DefineSimple(res, Track.Regen, "Regen", "Regen", 110, WHM.AID.Regen, 18, ActionQueue.Priority.VeryHigh);

        res.Define(Track.Cure).As<CureOption>("Cure", "Cure", 100)
            .AddOption(CureOption.None, NoUse)
            .AddOption(CureOption.Cure, "Use Cure", 2.5f, 0, ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly, 2, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(CureOption.CureII, "Use Cure II", 2.5f, 0, ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly, 30, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(CureOption.CureIII, "Use Cure III", 2.5f, 0, ActionTargets.Self | ActionTargets.Party, 40, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddAssociatedActions(WHM.AID.Cure, WHM.AID.CureII, WHM.AID.CureIII);

        res.Define(Track.Medica).As<MedicaOption>("Medica", "Medica", 130)
            .AddOption(MedicaOption.None, NoUse)
            .AddOption(MedicaOption.MedicaII, "Use Medica II", 2.5f, 15, ActionTargets.Self, 50, 95, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(MedicaOption.MedicaIII, "Use Medica III", 2.5f, 15, ActionTargets.Self, 96, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddAssociatedActions(WHM.AID.MedicaII, WHM.AID.MedicaIII);

        DefineSimple(res, Track.AfflatusSolace, "AfflatusSolace", "Afflatus Solace", 105, WHM.AID.AfflatusSolace, defaultPriority: ActionQueue.Priority.VeryHigh);
        DefineSimple(res, Track.AfflatusRapture, "AfflatusRapture", "Afflatus Rapture", 125, WHM.AID.AfflatusRapture, defaultPriority: ActionQueue.Priority.VeryHigh);
        DefineSimple(res, Track.Benediction, "Benediction", "Benediction", 300, WHM.AID.Benediction);

        res.Define(Track.Asylum).As<SimpleOption>("Asylum", "Asylum", 230)
            .AddOption(SimpleOption.None, NoUse)
            .AddOption(SimpleOption.Use, "Use Asylum", 90, 24, ActionTargets.Area, 52)
            .AddAssociatedActions(WHM.AID.Asylum);

        DefineSimple(res, Track.ThinAir, "ThinAir", "Thin Air", 200, WHM.AID.ThinAir, 12);
        DefineSimple(res, Track.Tetragrammaton, "Tetragrammaton", "Tetragrammaton", 150, WHM.AID.Tetragrammaton);
        DefineSimple(res, Track.DivineBenison, "DivineBenison", "Divine Benison", 140, WHM.AID.DivineBenison, 15);
        DefineSimple(res, Track.PlenaryIndulgence, "PlenaryIndulgence", "Plenary Indulgence", 160, WHM.AID.PlenaryIndulgence, 10);
        DefineSimple(res, Track.Temperance, "Temperance", "Temperance", 290, WHM.AID.Temperance, 20);
        DefineSimple(res, Track.Aquaveil, "Aquaveil", "Aquaveil", 170, WHM.AID.Aquaveil, 8);

        res.Define(Track.LiturgyOfTheBell).As<LiturgyOption>("LiturgyOfTheBell", "Liturgy of the Bell", 310)
            .AddOption(LiturgyOption.None, NoUse)
            .AddOption(LiturgyOption.Use, "Place Liturgy of the Bell", 180, 20, ActionTargets.Area, 90)
            .AddOption(LiturgyOption.End, "Detonate Liturgy of the Bell early", 0, 1, ActionTargets.Self, 90)
            .AddAssociatedActions(WHM.AID.LiturgyOfTheBell, WHM.AID.LiturgyOfTheBellEnd);

        DefineSimple(res, Track.DivineCaress, "DivineCaress", "Divine Caress", 280, WHM.AID.DivineCaress, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var healTarget = AllyOrSelf(primaryTarget);
        ExecuteSimple(strategy.Option(Track.PresenceOfMind), WHM.AID.PresenceOfMind, Player);
        ExecuteSimple(strategy.Option(Track.AfflatusSolace), WHM.AID.AfflatusSolace, healTarget);
        ExecuteSimple(strategy.Option(Track.AfflatusRapture), WHM.AID.AfflatusRapture, Player);
        ExecuteSimple(strategy.Option(Track.Benediction), WHM.AID.Benediction, healTarget);
        ExecuteSimple(strategy.Option(Track.Tetragrammaton), WHM.AID.Tetragrammaton, healTarget);
        ExecuteSimple(strategy.Option(Track.DivineBenison), WHM.AID.DivineBenison, healTarget);
        ExecuteSimple(strategy.Option(Track.PlenaryIndulgence), WHM.AID.PlenaryIndulgence, Player);
        ExecuteSimple(strategy.Option(Track.Temperance), WHM.AID.Temperance, Player);
        ExecuteSimple(strategy.Option(Track.Aquaveil), WHM.AID.Aquaveil, healTarget);
        ExecuteArea(strategy.Option(Track.Asylum), WHM.AID.Asylum);
        ExecuteVariant(strategy.Option(Track.Cure), healTarget, WHM.AID.Cure, WHM.AID.CureII, WHM.AID.CureIII);
        if (!HasStatus(Player, WHM.SID.ThinAir))
            ExecuteSimple(strategy.Option(Track.ThinAir), WHM.AID.ThinAir, Player);
        if (!HasStatus(Player, WHM.SID.MedicaII) && !HasStatus(Player, WHM.SID.MedicaIII))
            ExecuteVariant(strategy.Option(Track.Medica), Player, WHM.AID.MedicaII, WHM.AID.MedicaIII);
        if (HasStatus(Player, WHM.SID.DivineGrace))
            ExecuteSimple(strategy.Option(Track.DivineCaress), WHM.AID.DivineCaress, Player);
        ExecuteRegen(strategy.Option(Track.Regen), healTarget);
        ExecuteLiturgy(strategy.Option(Track.LiturgyOfTheBell));
    }

    private void ExecuteRegen(in StrategyValues.OptionRef regen, Actor healTarget)
    {
        if (!HasStatus(ResolveTarget(regen.Value) ?? healTarget, WHM.SID.Regen))
            ExecuteSimple(regen, WHM.AID.Regen, healTarget);
    }
    private void ExecuteLiturgy(in StrategyValues.OptionRef liturgy)
    {
        var option = liturgy.As<LiturgyOption>();
        var placed = HasStatus(Player, WHM.SID.LiturgyOfTheBell);
        if (option == LiturgyOption.Use && !placed)
            ExecuteArea(liturgy, WHM.AID.LiturgyOfTheBell);
        if (option == LiturgyOption.End && placed)
            ExecuteOn(liturgy, WHM.AID.LiturgyOfTheBellEnd, Player);
    }
}
