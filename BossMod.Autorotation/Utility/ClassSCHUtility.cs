using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;

public sealed class ClassSCHUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { WhisperingDawn = SharedTrack.Count, Adloquium, Succor, FeyIllumination, Lustrate, SacredSoil, Indomitability, DeploymentTactics, EmergencyTactics, Dissipation, Excogitation, Aetherpact, Recitation, FeyBlessing, Consolation, Protraction, Expedient, Seraphism, Seraph }
    public enum SuccorOption { None, Succor, Concitation }
    public enum SacredSoilOption { None, Use, UseEx }
    public enum DeployOption { None, Use, UseEx }
    public enum AetherpactOption { None, Use, End }
    public enum RecitationOption { None, Use, UseEx }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SCH.AID.AngelFeathers);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SCH", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimple(res, Track.WhisperingDawn, "Whispering Dawn", "Whispering Dawn", 140, SCH.AID.WhisperingDawn, 21);
        DefineSimple(res, Track.Adloquium, "Adloquium", "Adloquium", 100, SCH.AID.Adloquium, 30);

        res.Define(Track.Succor).As<SuccorOption>("Succor", "Succor", 200)
            .AddOption(SuccorOption.None, NoUse)
            .AddOption(SuccorOption.Succor, "Use Succor", 2.5f, 30, ActionTargets.Self, 35, 95, ActionQueue.Priority.VeryHigh)
            .AddOption(SuccorOption.Concitation, "Use Concitation", 2.5f, 30, ActionTargets.Self, 96, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddAssociatedActions(SCH.AID.Succor, SCH.AID.Concitation);

        DefineSimple(res, Track.FeyIllumination, "Fey Illumination", "Fey Illumination", 240, SCH.AID.FeyIllumination, 20);
        DefineSimple(res, Track.Lustrate, "Lustrate", "Lustrate", 150, SCH.AID.Lustrate);

        res.Define(Track.SacredSoil).As<SacredSoilOption>("Sacred Soil", "Sacred Soil", 200)
            .AddOption(SacredSoilOption.None, NoUse)
            .AddOption(SacredSoilOption.Use, "Place Sacred Soil", 30, 15, ActionTargets.Area, 50, 77)
            .AddOption(SacredSoilOption.UseEx, "Place Sacred Soil (with regen)", 30, 15, ActionTargets.Area, 78)
            .AddAssociatedActions(SCH.AID.SacredSoil);

        DefineSimple(res, Track.Indomitability, "Indomitability", "Indomitability", 90, SCH.AID.Indomitability);

        res.Define(Track.DeploymentTactics).As<DeployOption>("DeploymentTactics", "Deployment Tactics", 150)
            .AddOption(DeployOption.None, NoUse)
            .AddOption(DeployOption.Use, "Use Deployment Tactics (120s cooldown)", 120, 0, ActionTargets.Self | ActionTargets.Party, 56, 87)
            .AddOption(DeployOption.UseEx, "Use Deployment Tactics (90s cooldown)", 90, 0, ActionTargets.Self | ActionTargets.Party, 88)
            .AddAssociatedActions(SCH.AID.DeploymentTactics);

        DefineSimple(res, Track.EmergencyTactics, "EmergencyTactics", "Emergency Tactics", 100, SCH.AID.EmergencyTactics, 15);
        DefineSimple(res, Track.Dissipation, "Dissipation", "Dissipation", 290, SCH.AID.Dissipation, 30);
        DefineSimple(res, Track.Excogitation, "Excogitation", "Excogitation", 100, SCH.AID.Excogitation, 45);

        res.Define(Track.Aetherpact).As<AetherpactOption>("Aetherpact", "Aetherpact", 300)
            .AddOption(AetherpactOption.None, NoUse)
            .AddOption(AetherpactOption.Use, "Start Fey Union", 3, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddOption(AetherpactOption.End, "End Fey Union", 1, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(SCH.AID.Aetherpact, SCH.AID.DissolveUnion);

        res.Define(Track.Recitation).As<RecitationOption>("Recitation", "Recitation", 130)
            .AddOption(RecitationOption.None, NoUse)
            .AddOption(RecitationOption.Use, "Use Recitation (90s cooldown)", 90, 15, ActionTargets.Self, 74, 97)
            .AddOption(RecitationOption.UseEx, "Use Recitation (60s cooldown)", 60, 15, ActionTargets.Self, 98)
            .AddAssociatedActions(SCH.AID.Recitation);

        DefineSimple(res, Track.FeyBlessing, "FeyBlessing", "Fey Blessing", 120, SCH.AID.FeyBlessing);
        DefineSimple(res, Track.Consolation, "Consolation", "Consolation", 80, SCH.AID.Consolation, 30);
        DefineSimple(res, Track.Protraction, "Protraction", "Protraction", 110, SCH.AID.Protraction, 10);
        DefineSimple(res, Track.Expedient, "Expedient", "Expedient", 200, SCH.AID.Expedient, 20);
        DefineSimple(res, Track.Seraphism, "Seraphism", "Seraphism", 300, SCH.AID.Seraphism, 20);
        DefineSimple(res, Track.Seraph, "Seraph", "Summon Seraph", 300, SCH.AID.SummonSeraph, 22);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Gauge = World.Client.GetGauge<ScholarGauge>();
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var healTarget = AllyOrSelf(primaryTarget);
        ExecuteSimple(strategy.Option(Track.Adloquium), SCH.AID.Adloquium, healTarget);
        ExecuteSimple(strategy.Option(Track.Indomitability), SCH.AID.Indomitability, Player);
        ExecuteSimple(strategy.Option(Track.EmergencyTactics), SCH.AID.EmergencyTactics, Player);
        ExecuteSimple(strategy.Option(Track.Excogitation), SCH.AID.Excogitation, healTarget);
        ExecuteSimple(strategy.Option(Track.Protraction), SCH.AID.Protraction, healTarget);
        ExecuteSimple(strategy.Option(Track.Expedient), SCH.AID.Expedient, Player);
        ExecuteSimple(strategy.Option(Track.Lustrate), SCH.AID.Lustrate, healTarget);
        ExecuteSimple(strategy.Option(Track.Recitation), SCH.AID.Recitation, Player);
        ExecuteArea(strategy.Option(Track.SacredSoil), SCH.AID.SacredSoil);
        if (!HasStatus(Player, SCH.SID.Galvanize))
            ExecuteVariant(strategy.Option(Track.Succor), Player, SCH.AID.Succor, SCH.AID.Concitation);
        if (HasPet)
        {
            ExecuteSimple(strategy.Option(Track.WhisperingDawn), SCH.AID.WhisperingDawn, Player);
            ExecuteSimple(strategy.Option(Track.FeyIllumination), SCH.AID.FeyIllumination, Player);
            ExecuteSimple(strategy.Option(Track.FeyBlessing), SCH.AID.FeyBlessing, Player);
            ExecuteSimple(strategy.Option(Track.Seraph), SCH.AID.SummonSeraph, Player);
            if (Player.InCombat)
                ExecuteSimple(strategy.Option(Track.Seraphism), SCH.AID.Seraphism, Player);
            if (Player.InCombat && Gauge.SeraphTimer == 0)
                ExecuteSimple(strategy.Option(Track.Dissipation), SCH.AID.Dissipation, Player);
        }
        if (Gauge.SeraphTimer > 0)
            ExecuteSimple(strategy.Option(Track.Consolation), SCH.AID.Consolation, Player);
        ExecuteDeployment(strategy.Option(Track.DeploymentTactics), healTarget);
        ExecuteAetherpact(strategy.Option(Track.Aetherpact), healTarget);
    }

    private ScholarGauge Gauge;
    private bool HasPet => World.Client.ActivePet.InstanceID != 0xE0000000;

    private void ExecuteDeployment(in StrategyValues.OptionRef deploy, Actor healTarget)
    {
        if (HasStatus(ResolveTarget(deploy.Value) ?? healTarget, SCH.SID.Galvanize))
            ExecuteSimple(deploy, SCH.AID.DeploymentTactics, healTarget);
    }
    private void ExecuteAetherpact(in StrategyValues.OptionRef pact, Actor healTarget)
    {
        var option = pact.As<AetherpactOption>();
        var target = ResolveTarget(pact.Value) ?? healTarget;
        var bonded = HasStatus(target, SCH.SID.FeyUnion);
        if (option == AetherpactOption.Use && !bonded && HasPet && Gauge.FairyGauge >= 10)
            ExecuteOn(pact, SCH.AID.Aetherpact, target);
        if (option == AetherpactOption.End && bonded)
            ExecuteOn(pact, SCH.AID.DissolveUnion, Player);
    }
}
