using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;

public sealed class ClassSGEUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Kardia = SharedTrack.Count, Physis, Eukrasia, Diagnosis, Prognosis, Druochole, Kerachole, Ixochole, Zoe, Pepsis, Taurochole, Haima, Rhizomata, Holos, Panhaima, Krasis, Pneuma, Philosophia, Icarus }
    public enum KardiaOption { None, Kardia, Soteria }
    public enum DiagnosisOption { None, Use, UseED, Force }
    public enum PrognosisOption { None, Use, UseEP, UseEPEx, Force }
    public enum PhysisOption { None, Use, UseEx }
    public enum ZoeOption { None, Use, UseEx, Pneuma, Prognosis }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SGE.AID.TechneMakre);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SGE", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SGE), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.Kardia).As<KardiaOption>("Kardia", "Kardia", 200)
            .AddOption(KardiaOption.None, NoUse)
            .AddOption(KardiaOption.Kardia, "Use Kardia on the selected party member, otherwise the lowest HP tank, otherwise self", 5, 0, ActionTargets.Self | ActionTargets.Party, 4)
            .AddOption(KardiaOption.Soteria, "Use Soteria", 60, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(SGE.AID.Kardia, SGE.AID.Soteria);

        res.Define(Track.Physis).As<PhysisOption>("Physis", "Physis", 200)
            .AddOption(PhysisOption.None, NoUse)
            .AddOption(PhysisOption.Use, "Use Physis", 60, 15, ActionTargets.Self, 20, 59)
            .AddOption(PhysisOption.UseEx, "Use Physis II", 60, 15, ActionTargets.Self, 60)
            .AddAssociatedActions(SGE.AID.Physis, SGE.AID.PhysisII);

        DefineSimple(res, Track.Eukrasia, "Eukrasia", "Eukrasia", 110, SGE.AID.Eukrasia);

        res.Define(Track.Diagnosis).As<DiagnosisOption>("Diagnosis", "Diagnosis", 200)
            .AddOption(DiagnosisOption.None, NoUse)
            .AddOption(DiagnosisOption.Use, "Use Diagnosis", 2.5f, 0, ActionTargets.Self | ActionTargets.Party, 2)
            .AddOption(DiagnosisOption.UseED, "Use Eukrasian Diagnosis if selected party member has no conflicting shields", 1.5f, 30, ActionTargets.Self | ActionTargets.Party, 30)
            .AddOption(DiagnosisOption.Force, "Use Eukrasian Diagnosis on selected party member regardless of HP & shields", 1.5f, 30, ActionTargets.Self | ActionTargets.Party, 30)
            .AddAssociatedActions(SGE.AID.Diagnosis, SGE.AID.EukrasianDiagnosis);

        res.Define(Track.Prognosis).As<PrognosisOption>("Prognosis", "Prognosis", 200)
            .AddOption(PrognosisOption.None, NoUse)
            .AddOption(PrognosisOption.Use, "Use Prognosis", 2.5f, 0, ActionTargets.Self, 10)
            .AddOption(PrognosisOption.UseEP, "Use Eukrasian Prognosis if you have no conflicting shields on self", 1.5f, 30, ActionTargets.Self, 30, 95)
            .AddOption(PrognosisOption.UseEPEx, "Use Eukrasian Prognosis II if you have no conflicting shields on self", 1.5f, 30, ActionTargets.Self, 96)
            .AddOption(PrognosisOption.Force, "Use Eukrasian Prognosis regardless of HP & shields on self", 1.5f, 30, ActionTargets.Self, 30)
            .AddAssociatedActions(SGE.AID.Prognosis, SGE.AID.EukrasianPrognosis, SGE.AID.EukrasianPrognosisII);

        DefineSimple(res, Track.Druochole, "Druochole", "Druochole", 150, SGE.AID.Druochole);

        DefineSimple(res, Track.Kerachole, "Kerachole", "Kerachole", 180, SGE.AID.Kerachole, 15);
        DefineSimple(res, Track.Ixochole, "Ixochole", "Ixochole", 190, SGE.AID.Ixochole);

        res.Define(Track.Zoe).As<ZoeOption>("Zoe", "Zoe", 200)
            .AddOption(ZoeOption.None, NoUse)
            .AddOption(ZoeOption.Use, "Use Zoe (120s cooldown)", 120, 30, ActionTargets.Self, 56, 87)
            .AddOption(ZoeOption.UseEx, "Use Zoe (90s cooldown)", 90, 30, ActionTargets.Self, 88)
            .AddOption(ZoeOption.Pneuma, "Use Zoe -> Pneuma; will not use if Zoe is unavailable", 90, 30, ActionTargets.Hostile, 90)
            .AddOption(ZoeOption.Prognosis, "Use Zoe -> Eukrasian Prognosis; will not use if Zoe is unavailable", 90, 30, ActionTargets.Self, 56)
            .AddAssociatedActions(SGE.AID.Zoe);

        DefineSimple(res, Track.Pepsis, "Pepsis", "Pepsis", 170, SGE.AID.Pepsis);
        DefineSimple(res, Track.Taurochole, "Taurochole", "Taurochole", 200, SGE.AID.Taurochole, 15);
        DefineSimple(res, Track.Haima, "Haima", "Haima", 100, SGE.AID.Haima, 15);
        DefineSimple(res, Track.Rhizomata, "Rhizomata", "Rhizomata", 230, SGE.AID.Rhizomata);
        DefineSimple(res, Track.Holos, "Holos", "Holos", 240, SGE.AID.Holos, 20);
        DefineSimple(res, Track.Panhaima, "Panhaima", "Panhaima", 250, SGE.AID.Panhaima, 15);
        DefineSimple(res, Track.Krasis, "Krasis", "Krasis", 210, SGE.AID.Krasis, 10);
        DefineSimple(res, Track.Pneuma, "Pneuma", "Pneuma", 220, SGE.AID.Pneuma);
        DefineSimple(res, Track.Philosophia, "Philosophia", "Philosophia", 260, SGE.AID.Philosophia, 20);

        DefineDash(res, Track.Icarus, "Icarus", "Icarus", SGE.AID.Icarus, ActionTargets.Party | ActionTargets.Hostile, "Use when outside melee range of selected target or party member");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Gauge = World.Client.GetGauge<SageGauge>();
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var healTarget = AllyOrSelf(primaryTarget);
        var enemy = primaryTarget is { IsAlly: false } ? primaryTarget : null;
        ExecuteSimple(strategy.Option(Track.Haima), SGE.AID.Haima, healTarget);
        ExecuteSimple(strategy.Option(Track.Holos), SGE.AID.Holos, Player);
        ExecuteSimple(strategy.Option(Track.Panhaima), SGE.AID.Panhaima, Player);
        ExecuteSimple(strategy.Option(Track.Krasis), SGE.AID.Krasis, healTarget);
        ExecuteSimple(strategy.Option(Track.Pneuma), SGE.AID.Pneuma, enemy);
        ExecuteSimple(strategy.Option(Track.Philosophia), SGE.AID.Philosophia, Player);
        ExecuteVariant(strategy.Option(Track.Physis), Player, SGE.AID.Physis, SGE.AID.PhysisII);
        if (!Gauge.EukrasiaActive)
            ExecuteSimple(strategy.Option(Track.Eukrasia), SGE.AID.Eukrasia, Player);
        if (Gauge.Addersgall > 0)
        {
            ExecuteSimple(strategy.Option(Track.Druochole), SGE.AID.Druochole, healTarget);
            ExecuteSimple(strategy.Option(Track.Kerachole), SGE.AID.Kerachole, Player);
            ExecuteSimple(strategy.Option(Track.Ixochole), SGE.AID.Ixochole, Player);
            ExecuteSimple(strategy.Option(Track.Taurochole), SGE.AID.Taurochole, healTarget);
        }
        if (Gauge.Addersgall < 2 || Gauge.Addersgall == 2 && NextAddersgallIn >= 2.5f)
            ExecuteSimple(strategy.Option(Track.Rhizomata), SGE.AID.Rhizomata, Player);
        if (EukrasianShieldNearby)
            ExecuteSimple(strategy.Option(Track.Pepsis), SGE.AID.Pepsis, Player);
        ExecuteKardia(strategy.Option(Track.Kardia), primaryTarget);
        ExecuteDiagnosis(strategy.Option(Track.Diagnosis), healTarget);
        ExecutePrognosis(strategy.Option(Track.Prognosis));
        ExecuteZoe(strategy.Option(Track.Zoe), enemy);
        ExecuteDash(strategy.Option(Track.Icarus), SGE.AID.Icarus, primaryTarget);
    }

    private SageGauge Gauge;
    private float NextAddersgallIn => (20000 - Gauge.AddersgallTimer) * 0.001f;
    private SGE.AID BestEukrasianPrognosis => ActionUnlocked(SGE.AID.EukrasianPrognosisII) ? SGE.AID.EukrasianPrognosisII : SGE.AID.EukrasianPrognosis;
    private static bool HasEukrasianShield(Actor actor) => HasStatus(actor, SGE.SID.EukrasianDiagnosis) || HasStatus(actor, SGE.SID.EukrasianPrognosis);
    private static bool HasAnyShield(Actor actor) => HasEukrasianShield(actor) || HasStatus(actor, SCH.SID.Galvanize);
    private Actor? LowestHPTank => World.Party.WithoutSlot().Where(member => member.Role == Role.Tank && !member.IsDeadOrDestroyed && Player.DistanceToHitbox(member) <= 30).MinBy(member => member.PendingHPRatio);
    private bool EukrasianShieldNearby => World.Party.WithoutSlot().Any(member => !member.IsDeadOrDestroyed && Player.DistanceToHitbox(member) <= 20 && HasEukrasianShield(member));

    private void ExecuteKardia(in StrategyValues.OptionRef kardia, Actor? primaryTarget)
    {
        var option = kardia.As<KardiaOption>();
        var target = ResolveTarget(kardia.Value) ?? OtherAlly(primaryTarget) ?? LowestHPTank ?? Player;
        if (option == KardiaOption.Kardia && !HasStatus(target, SGE.SID.Kardion))
            ExecuteOn(kardia, SGE.AID.Kardia, target);
        if (option == KardiaOption.Soteria && HasStatus(Player, SGE.SID.Kardia))
            ExecuteOn(kardia, SGE.AID.Soteria, Player);
    }
    private void ExecuteEukrasian(in StrategyValues.OptionRef opt, SGE.AID action, Actor? target)
    {
        if (HasMP(action))
            ExecuteOn(opt, Gauge.EukrasiaActive ? action : SGE.AID.Eukrasia, Gauge.EukrasiaActive ? target : Player);
    }
    private void ExecuteDiagnosis(in StrategyValues.OptionRef diagnosis, Actor healTarget)
    {
        var option = diagnosis.As<DiagnosisOption>();
        var target = ResolveTarget(diagnosis.Value) ?? healTarget;
        if (option == DiagnosisOption.Use)
            ExecuteOn(diagnosis, SGE.AID.Diagnosis, target);
        if (option == DiagnosisOption.UseED && !HasAnyShield(target) || option == DiagnosisOption.Force)
            ExecuteEukrasian(diagnosis, SGE.AID.EukrasianDiagnosis, target);
    }
    private void ExecutePrognosis(in StrategyValues.OptionRef prognosis)
    {
        var option = prognosis.As<PrognosisOption>();
        var action = option switch
        {
            PrognosisOption.UseEP => SGE.AID.EukrasianPrognosis,
            PrognosisOption.UseEPEx => SGE.AID.EukrasianPrognosisII,
            _ => BestEukrasianPrognosis
        };
        if (option == PrognosisOption.Use)
            ExecuteOn(prognosis, SGE.AID.Prognosis, Player);
        if (option is PrognosisOption.UseEP or PrognosisOption.UseEPEx && !HasAnyShield(Player) || option == PrognosisOption.Force)
            ExecuteEukrasian(prognosis, action, Player);
    }
    private void ExecuteZoe(in StrategyValues.OptionRef zoe, Actor? enemy)
    {
        var option = zoe.As<ZoeOption>();
        var hasZoe = HasStatus(Player, SGE.SID.Zoe);
        var pneumaTarget = ResolveTarget(zoe.Value) ?? enemy;
        var followUp = option == ZoeOption.Pneuma ? SGE.AID.Pneuma : BestEukrasianPrognosis;
        var followUpReady = option is ZoeOption.Pneuma or ZoeOption.Prognosis && HasMP(followUp) && (option != ZoeOption.Pneuma || IsEnemy(pneumaTarget));
        if (option is ZoeOption.Use or ZoeOption.UseEx || followUpReady && !hasZoe)
            ExecuteOn(zoe, SGE.AID.Zoe, Player);
        if (followUpReady && hasZoe && option == ZoeOption.Pneuma)
            ExecuteOn(zoe, SGE.AID.Pneuma, pneumaTarget);
        if (followUpReady && hasZoe && option == ZoeOption.Prognosis)
            ExecuteEukrasian(zoe, followUp, Player);
    }
}
