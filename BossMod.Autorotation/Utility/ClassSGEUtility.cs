using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;

public sealed class ClassSGEUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Kardia = SharedTrack.Count, Soteria, Physis, Eukrasia, Diagnosis, Prognosis, Druochole, Kerachole, Ixochole, Zoe, Pepsis, Taurochole, Haima, Rhizomata, Holos, Panhaima, Krasis, Pneuma, Philosophia, Icarus }
    public enum KardiaOption { None, LowestPartyWithSelf, LowestPartyNoSelf, LowestTank, SelfOnly, Kardia, Soteria }
    public enum DiagnosisOption { None, Use, UseED, Force }
    public enum PrognosisOption { None, Use, UseEP, UseEPEx, Force }
    public enum PhysisOption { None, Use, UseEx }
    public enum ZoeOption { None, Use, UseEx, Pneuma, Prognosis }
    public enum AddersgallOption { None, Use, OvercapParty, OvercapSelf }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SGE.AID.TechneMakre);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SGE", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SGE), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.Kardia).As<KardiaOption>("Kardia", "Kardia", 200)
            .AddOption(KardiaOption.None, NoUse)
            .AddOption(KardiaOption.LowestPartyWithSelf, "Use Kardia on the party member with lowest HP% (including self)", 5, 0, ActionTargets.Self | ActionTargets.Party, 4)
            .AddOption(KardiaOption.LowestPartyNoSelf, "Use Kardia on the party member with lowest HP% (excluding self)", 5, 0, ActionTargets.Party, 4)
            .AddOption(KardiaOption.LowestTank, "Use Kardia on the tank with lowest HP%", 5, 0, ActionTargets.Party, 4)
            .AddOption(KardiaOption.SelfOnly, "Use Kardia on self", 5, 0, ActionTargets.Self, 4)
            .AddOption(KardiaOption.Kardia, "Use Kardia on selected party member", 5, 0, ActionTargets.Self | ActionTargets.Party, 4)
            .AddOption(KardiaOption.Soteria, "Use Soteria", 60, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(SGE.AID.Kardia, SGE.AID.Soteria);

        DefineSimpleConfig(res, Track.Soteria, "Soteria", "Soteria", 205, SGE.AID.Soteria, 15);

        res.Define(Track.Physis).As<PhysisOption>("Physis", "Physis", 200)
            .AddOption(PhysisOption.None, NoUse)
            .AddOption(PhysisOption.Use, "Use Physis", 60, 15, ActionTargets.Self, 20, 59)
            .AddOption(PhysisOption.UseEx, "Use Physis II", 60, 15, ActionTargets.Self, 60)
            .AddAssociatedActions(SGE.AID.Physis, SGE.AID.PhysisII);

        DefineSimpleConfig(res, Track.Eukrasia, "Eukrasia", "Eukrasia", 110, SGE.AID.Eukrasia);

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

        res.Define(Track.Druochole).As<AddersgallOption>("Druochole", "Druochole", 150)
            .AddOption(AddersgallOption.None, NoUse)
            .AddOption(AddersgallOption.Use, "Use Druochole", 1, 0, ActionTargets.Self | ActionTargets.Party, 45, defaultPriority: 3000)
            .AddOption(AddersgallOption.OvercapParty, "Use Druochole on lowest HP% party member if about to gain a charge of Addersgall", 1, 0, ActionTargets.Self | ActionTargets.Party, 45, defaultPriority: 3000)
            .AddOption(AddersgallOption.OvercapSelf, "Use Druochole on self if about to gain a charge of Addersgall", 1, 0, ActionTargets.Self, 45, defaultPriority: 3000)
            .AddAssociatedActions(SGE.AID.Druochole);

        DefineSimpleConfig(res, Track.Kerachole, "Kerachole", "Kerachole", 180, SGE.AID.Kerachole, 15);
        DefineSimpleConfig(res, Track.Ixochole, "Ixochole", "Ixochole", 190, SGE.AID.Ixochole);

        res.Define(Track.Zoe).As<ZoeOption>("Zoe", "Zoe", 200)
            .AddOption(ZoeOption.None, NoUse)
            .AddOption(ZoeOption.Use, "Use Zoe (120s cooldown)", 120, 30, ActionTargets.Self, 56, 87)
            .AddOption(ZoeOption.UseEx, "Use Zoe (90s cooldown)", 90, 30, ActionTargets.Self, 88)
            .AddOption(ZoeOption.Pneuma, "Use Zoe -> Pneuma; will not use if Zoe is unavailable", 90, 30, ActionTargets.Hostile, 90)
            .AddOption(ZoeOption.Prognosis, "Use Zoe -> Eukrasian Prognosis; will not use if Zoe is unavailable", 90, 30, ActionTargets.Self, 56)
            .AddAssociatedActions(SGE.AID.Zoe);

        DefineSimpleConfig(res, Track.Pepsis, "Pepsis", "Pepsis", 170, SGE.AID.Pepsis);
        DefineSimpleConfig(res, Track.Taurochole, "Taurochole", "Taurochole", 200, SGE.AID.Taurochole, 15);
        DefineSimpleConfig(res, Track.Haima, "Haima", "Haima", 100, SGE.AID.Haima, 15);
        DefineSimpleConfig(res, Track.Rhizomata, "Rhizomata", "Rhizomata", 230, SGE.AID.Rhizomata);
        DefineSimpleConfig(res, Track.Holos, "Holos", "Holos", 240, SGE.AID.Holos, 20);
        DefineSimpleConfig(res, Track.Panhaima, "Panhaima", "Panhaima", 250, SGE.AID.Panhaima, 15);
        DefineSimpleConfig(res, Track.Krasis, "Krasis", "Krasis", 210, SGE.AID.Krasis, 10);
        DefineSimpleConfig(res, Track.Pneuma, "Pneuma", "Pneuma", 220, SGE.AID.Pneuma);
        DefineSimpleConfig(res, Track.Philosophia, "Philosophia", "Philosophia", 260, SGE.AID.Philosophia, 20);

        DefineDash(res, Track.Icarus, "Icarus", "Icarus", SGE.AID.Icarus, ActionTargets.Party | ActionTargets.Hostile, "Use when outside melee range of selected target or party member");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Gauge = World.Client.GetGauge<SageGauge>();
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var healTarget = FriendlyTarget(primaryTarget);
        var enemy = primaryTarget is { IsAlly: false } ? primaryTarget : null;

        ExecuteEukrasia(strategy.Option(Track.Eukrasia));
        ExecuteDruochole(strategy.Option(Track.Druochole), healTarget);
        ExecuteAddersgall(strategy.Option(Track.Kerachole), SGE.AID.Kerachole, Player);
        ExecuteAddersgall(strategy.Option(Track.Ixochole), SGE.AID.Ixochole, Player);
        ExecuteAddersgall(strategy.Option(Track.Taurochole), SGE.AID.Taurochole, healTarget);
        ExecuteSimple(strategy.Option(Track.Haima), SGE.AID.Haima, healTarget);
        ExecuteSimple(strategy.Option(Track.Holos), SGE.AID.Holos, Player);
        ExecuteSimple(strategy.Option(Track.Panhaima), SGE.AID.Panhaima, Player);
        ExecuteSimple(strategy.Option(Track.Krasis), SGE.AID.Krasis, healTarget);
        ExecuteSimple(strategy.Option(Track.Pneuma), SGE.AID.Pneuma, enemy);
        ExecuteSimple(strategy.Option(Track.Philosophia), SGE.AID.Philosophia, Player);
        ExecuteSoteria(strategy.Option(Track.Soteria));
        ExecuteRhizomata(strategy.Option(Track.Rhizomata));
        ExecutePepsis(strategy.Option(Track.Pepsis));
        ExecuteKardia(strategy.Option(Track.Kardia), primaryTarget);
        ExecuteDiagnosis(strategy.Option(Track.Diagnosis), healTarget);
        ExecutePrognosis(strategy.Option(Track.Prognosis));
        ExecutePhysis(strategy.Option(Track.Physis));
        ExecuteZoe(strategy.Option(Track.Zoe), enemy);
        ExecuteDash(strategy.Option(Track.Icarus), SGE.AID.Icarus, primaryTarget);
    }

    private SageGauge Gauge;
    private int Addersgall => Gauge.Addersgall;
    private float NextAddersgallIn => (20000 - Gauge.AddersgallTimer) * 0.001f;
    private bool AddersgallCapping => Addersgall >= 3 || Addersgall == 2 && NextAddersgallIn < 3f;
    private bool HasEukrasia => Gauge.EukrasiaActive;
    private bool HasZoe => Player.FindStatus(SGE.SID.Zoe) != null;
    private SGE.AID BestPrognosis => ActionUnlocked(SGE.AID.EukrasianPrognosisII) ? SGE.AID.EukrasianPrognosisII : SGE.AID.EukrasianPrognosis;
    private bool HasOwnShield(Actor target) => HasStatus(target, SGE.SID.EukrasianDiagnosis) || HasStatus(target, SGE.SID.EukrasianPrognosis);
    private static bool HasConflictingShield(Actor target)
        => target.FindStatus(SGE.SID.EukrasianDiagnosis) != null || target.FindStatus(SGE.SID.EukrasianPrognosis) != null || target.FindStatus(SCH.SID.Galvanize) != null;

    private void ExecuteEukrasia(in StrategyValues.OptionRef eukrasia)
    {
        if (!HasEukrasia)
            ExecuteSimple(eukrasia, SGE.AID.Eukrasia, Player);
    }
    private void ExecuteAddersgall(in StrategyValues.OptionRef opt, SGE.AID aid, Actor target)
    {
        if (Addersgall > 0)
            ExecuteSimple(opt, aid, target);
    }
    private void ExecuteDruochole(in StrategyValues.OptionRef druochole, Actor healTarget)
    {
        var option = druochole.As<AddersgallOption>();
        var overcapTarget = option == AddersgallOption.OvercapParty ? LowestHPPartyMember(true, false) ?? Player : Player;
        if (option == AddersgallOption.Use && Addersgall > 0)
            ExecuteAction(druochole, SGE.AID.Druochole, healTarget);
        if (option is AddersgallOption.OvercapParty or AddersgallOption.OvercapSelf && AddersgallCapping)
            ExecuteOn(druochole, SGE.AID.Druochole, overcapTarget);
    }
    private void ExecuteSoteria(in StrategyValues.OptionRef soteria)
    {
        if (HasStatus(Player, SGE.SID.Kardia))
            ExecuteSimple(soteria, SGE.AID.Soteria, Player);
    }
    private void ExecuteRhizomata(in StrategyValues.OptionRef rhizomata)
    {
        if (Addersgall < 2 || Addersgall == 2 && NextAddersgallIn >= 2.5f)
            ExecuteSimple(rhizomata, SGE.AID.Rhizomata, Player);
    }
    private void ExecutePepsis(in StrategyValues.OptionRef pepsis)
    {
        var shielded = false;
        foreach (var member in World.Party.WithoutSlot())
            shielded |= !member.IsDeadOrDestroyed && Player.DistanceToHitbox(member) <= 20 && HasOwnShield(member);
        if (shielded)
            ExecuteSimple(pepsis, SGE.AID.Pepsis, Player);
    }
    private bool KardiaEligible(Actor member, bool includeSelf, bool tanksOnly)
        => !member.IsDeadOrDestroyed && (includeSelf || member != Player) && (!tanksOnly || member.Role == Role.Tank) && Player.DistanceToHitbox(member) <= 30;
    private Actor? LowestHPPartyMember(bool includeSelf, bool tanksOnly)
    {
        Actor? lowest = null;
        foreach (var member in World.Party.WithoutSlot())
            if (KardiaEligible(member, includeSelf, tanksOnly) && (lowest == null || member.PendingHPRatio < lowest.PendingHPRatio))
                lowest = member;
        return lowest;
    }
    private Actor? KardiaTarget(bool includeSelf, bool tanksOnly)
    {
        var lowest = LowestHPPartyMember(includeSelf, tanksOnly);
        Actor? holder = null;
        foreach (var member in World.Party.WithoutSlot())
            if (KardiaEligible(member, includeSelf, tanksOnly) && HasStatus(member, SGE.SID.Kardion))
                holder = member;
        return holder != null && lowest != null && lowest.PendingHPRatio > holder.PendingHPRatio - 0.1f ? holder : lowest;
    }
    private void ExecuteKardia(in StrategyValues.OptionRef kardia, Actor? primaryTarget)
    {
        var option = kardia.As<KardiaOption>();
        if (option == KardiaOption.Soteria)
        {
            if (HasStatus(Player, SGE.SID.Kardia))
                ExecuteAction(kardia, SGE.AID.Soteria, Player);
            return;
        }

        var target = option switch
        {
            KardiaOption.LowestPartyWithSelf => KardiaTarget(true, false),
            KardiaOption.LowestPartyNoSelf => KardiaTarget(false, false),
            KardiaOption.LowestTank => KardiaTarget(true, true),
            KardiaOption.SelfOnly => Player,
            KardiaOption.Kardia => ResolveTarget(kardia.Value) ?? PartnerTarget(primaryTarget) ?? Player,
            _ => null
        };
        if (target != null && !HasStatus(target, SGE.SID.Kardion))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Kardia), target, kardia.Priority(), kardia.Value.ExpireIn);
    }
    private void ExecuteEukrasian(in StrategyValues.OptionRef opt, SGE.AID action, Actor? target)
    {
        var eukrasia = ActionID.MakeSpell(SGE.AID.Eukrasia);
        if (HasMP(ActionID.MakeSpell(action)))
            ExecuteOn(opt, HasEukrasia ? ActionID.MakeSpell(action) : eukrasia, HasEukrasia ? target : Player);
    }
    private void ExecuteDiagnosis(in StrategyValues.OptionRef diagnosis, Actor healTarget)
    {
        var option = diagnosis.As<DiagnosisOption>();
        var target = ResolveTarget(diagnosis.Value) ?? healTarget;
        if (option == DiagnosisOption.Use)
            ExecuteOn(diagnosis, SGE.AID.Diagnosis, target);
        if (option == DiagnosisOption.UseED && !HasConflictingShield(target) || option == DiagnosisOption.Force)
            ExecuteEukrasian(diagnosis, SGE.AID.EukrasianDiagnosis, target);
    }
    private void ExecutePrognosis(in StrategyValues.OptionRef prognosis)
    {
        var option = prognosis.As<PrognosisOption>();
        var action = option switch
        {
            PrognosisOption.UseEP => SGE.AID.EukrasianPrognosis,
            PrognosisOption.UseEPEx => SGE.AID.EukrasianPrognosisII,
            _ => BestPrognosis
        };
        if (option == PrognosisOption.Use)
            ExecuteOn(prognosis, SGE.AID.Prognosis, Player);
        if (option is PrognosisOption.UseEP or PrognosisOption.UseEPEx && !HasConflictingShield(Player) || option == PrognosisOption.Force)
            ExecuteEukrasian(prognosis, action, Player);
    }
    private void ExecutePhysis(in StrategyValues.OptionRef physis)
    {
        var action = physis.As<PhysisOption>() switch
        {
            PhysisOption.Use => SGE.AID.Physis,
            PhysisOption.UseEx => SGE.AID.PhysisII,
            _ => default
        };
        if (action != default)
            ExecuteAction(physis, action, Player);
    }
    private void ExecuteZoe(in StrategyValues.OptionRef zoe, Actor? enemy)
    {
        var option = zoe.As<ZoeOption>();
        var followUp = option switch
        {
            ZoeOption.Pneuma => SGE.AID.Pneuma,
            ZoeOption.Prognosis => BestPrognosis,
            _ => default
        };
        var pneumaTarget = ResolveTarget(zoe.Value) ?? enemy;
        var followUpReady = followUp != default && HasMP(ActionID.MakeSpell(followUp)) && (option != ZoeOption.Pneuma || IsEnemy(pneumaTarget));
        if (option is ZoeOption.Use or ZoeOption.UseEx || followUpReady && !HasZoe)
            ExecuteOn(zoe, SGE.AID.Zoe, Player);
        if (followUpReady && HasZoe && option == ZoeOption.Pneuma)
            ExecuteOn(zoe, SGE.AID.Pneuma, pneumaTarget);
        if (followUpReady && HasZoe && option == ZoeOption.Prognosis)
            ExecuteEukrasian(zoe, followUp, Player);
    }
}
