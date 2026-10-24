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

        DefineSimpleConfig(res, Track.WhisperingDawn, "Whispering Dawn", "W.Dawn", 140, SCH.AID.WhisperingDawn, 21);
        DefineSimpleConfig(res, Track.Adloquium, "Adloquium", "Adlo.", 100, SCH.AID.Adloquium, 30);

        res.Define(Track.Succor).As<SuccorOption>("Succor", "Succor", 200)
            .AddOption(SuccorOption.None, NoUse)
            .AddOption(SuccorOption.Succor, "Use Succor", 2.5f, 30, ActionTargets.Self, 35, 95, ActionQueue.Priority.VeryHigh)
            .AddOption(SuccorOption.Concitation, "Use Concitation", 2.5f, 30, ActionTargets.Self, 96, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddAssociatedActions(SCH.AID.Succor, SCH.AID.Concitation);

        DefineSimpleConfig(res, Track.FeyIllumination, "Fey Illumination", "F.Illum.", 240, SCH.AID.FeyIllumination, 20);
        DefineSimpleConfig(res, Track.Lustrate, "Lustrate", "Lustrate", 150, SCH.AID.Lustrate);

        res.Define(Track.SacredSoil).As<SacredSoilOption>("Sacred Soil", "S.Soil", 200)
            .AddOption(SacredSoilOption.None, NoUse)
            .AddOption(SacredSoilOption.Use, "Place Sacred Soil", 30, 15, ActionTargets.Area, 50, 77)
            .AddOption(SacredSoilOption.UseEx, "Place Sacred Soil (with regen)", 30, 15, ActionTargets.Area, 78)
            .AddAssociatedActions(SCH.AID.SacredSoil);

        DefineSimpleConfig(res, Track.Indomitability, "Indomitability", "Indom.", 90, SCH.AID.Indomitability);

        res.Define(Track.DeploymentTactics).As<DeployOption>("DeploymentTactics", "Deploy.", 150)
            .AddOption(DeployOption.None, NoUse)
            .AddOption(DeployOption.Use, "Use Deployment Tactics (120s cooldown)", 120, 0, ActionTargets.Self | ActionTargets.Party, 56, 87)
            .AddOption(DeployOption.UseEx, "Use Deployment Tactics (90s cooldown)", 90, 0, ActionTargets.Self | ActionTargets.Party, 88)
            .AddAssociatedActions(SCH.AID.DeploymentTactics);

        DefineSimpleConfig(res, Track.EmergencyTactics, "EmergencyTactics", "Emerg.", 100, SCH.AID.EmergencyTactics, 15);
        DefineSimpleConfig(res, Track.Dissipation, "Dissipation", "Dissi.", 290, SCH.AID.Dissipation, 30);
        DefineSimpleConfig(res, Track.Excogitation, "Excogitation", "Excog.", 100, SCH.AID.Excogitation, 45);

        res.Define(Track.Aetherpact).As<AetherpactOption>("Aetherpact", "A.pact", 300)
            .AddOption(AetherpactOption.None, NoUse)
            .AddOption(AetherpactOption.Use, "Start Fey Union", 3, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddOption(AetherpactOption.End, "End Fey Union", 1, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(SCH.AID.Aetherpact, SCH.AID.DissolveUnion);

        res.Define(Track.Recitation).As<RecitationOption>("Recitation", "Recit.", 130)
            .AddOption(RecitationOption.None, NoUse)
            .AddOption(RecitationOption.Use, "Use Recitation (90s cooldown)", 90, 15, ActionTargets.Self, 74, 97)
            .AddOption(RecitationOption.UseEx, "Use Recitation (60s cooldown)", 60, 15, ActionTargets.Self, 98)
            .AddAssociatedActions(SCH.AID.Recitation);

        DefineSimpleConfig(res, Track.FeyBlessing, "FeyBlessing", "F.Blessing", 120, SCH.AID.FeyBlessing);
        DefineSimpleConfig(res, Track.Consolation, "Consolation", "Consol.", 80, SCH.AID.Consolation, 30);
        DefineSimpleConfig(res, Track.Protraction, "Protraction", "Prot.", 110, SCH.AID.Protraction, 10);
        DefineSimpleConfig(res, Track.Expedient, "Expedient", "Exped.", 200, SCH.AID.Expedient, 20);
        DefineSimpleConfig(res, Track.Seraphism, "Seraphism", "Seraphism", 300, SCH.AID.Seraphism, 20);
        DefineSimpleConfig(res, Track.Seraph, "Seraph", "Seraph", 300, SCH.AID.SummonSeraph, 22);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var healTarget = FriendlyTarget(primaryTarget);
        ExecuteSimple(strategy.Option(Track.WhisperingDawn), SCH.AID.WhisperingDawn, Player);
        ExecuteSimple(strategy.Option(Track.Adloquium), SCH.AID.Adloquium, healTarget);
        ExecuteSimple(strategy.Option(Track.FeyIllumination), SCH.AID.FeyIllumination, Player);
        ExecuteSimple(strategy.Option(Track.Lustrate), SCH.AID.Lustrate, healTarget);
        ExecuteSimple(strategy.Option(Track.Indomitability), SCH.AID.Indomitability, Player);
        ExecuteSimple(strategy.Option(Track.EmergencyTactics), SCH.AID.EmergencyTactics, Player);
        ExecuteSimple(strategy.Option(Track.Dissipation), SCH.AID.Dissipation, Player);
        ExecuteSimple(strategy.Option(Track.Excogitation), SCH.AID.Excogitation, healTarget);
        ExecuteSimple(strategy.Option(Track.FeyBlessing), SCH.AID.FeyBlessing, Player);
        ExecuteSimple(strategy.Option(Track.Consolation), SCH.AID.Consolation, Player);
        ExecuteSimple(strategy.Option(Track.Protraction), SCH.AID.Protraction, healTarget);
        ExecuteSimple(strategy.Option(Track.Expedient), SCH.AID.Expedient, Player);
        ExecuteSimple(strategy.Option(Track.Seraphism), SCH.AID.Seraphism, Player);
        ExecuteSimple(strategy.Option(Track.Seraph), SCH.AID.SummonSeraph, Player);
        ExecuteSuccor(strategy.Option(Track.Succor));
        ExecuteSoil(strategy.Option(Track.SacredSoil));
        ExecuteDeployment(strategy.Option(Track.DeploymentTactics), healTarget);
        ExecuteAetherpact(strategy.Option(Track.Aetherpact), healTarget);
        ExecuteRecitation(strategy.Option(Track.Recitation));
    }

    private void ExecuteSuccor(in StrategyValues.OptionRef succor)
    {
        var action = succor.As<SuccorOption>() switch
        {
            SuccorOption.Succor => SCH.AID.Succor,
            SuccorOption.Concitation => SCH.AID.Concitation,
            _ => default
        };
        if (action != default && !HasStatus(Player, SCH.SID.Galvanize))
            ExecuteAction(succor, action, Player);
    }
    private void ExecuteSoil(in StrategyValues.OptionRef soil)
    {
        if (soil.As<SacredSoilOption>() != SacredSoilOption.None)
            ExecuteArea(soil, SCH.AID.SacredSoil);
    }
    private void ExecuteDeployment(in StrategyValues.OptionRef deploy, Actor healTarget)
    {
        if (deploy.As<DeployOption>() != DeployOption.None)
            ExecuteAction(deploy, SCH.AID.DeploymentTactics, healTarget);
    }
    private void ExecuteAetherpact(in StrategyValues.OptionRef pact, Actor healTarget)
    {
        var option = pact.As<AetherpactOption>();
        var target = ResolveTarget(pact.Value) ?? healTarget;
        var bonded = target.FindStatus(SCH.SID.FeyUnion) != null;
        if (option == AetherpactOption.Use && !bonded)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Aetherpact), target, pact.Priority(), pact.Value.ExpireIn);
        if (option == AetherpactOption.End && bonded)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.DissolveUnion), Player, pact.Priority(), pact.Value.ExpireIn);
    }
    private void ExecuteRecitation(in StrategyValues.OptionRef recitation)
    {
        if (recitation.As<RecitationOption>() != RecitationOption.None)
            ExecuteAction(recitation, SCH.AID.Recitation, Player);
    }
}
