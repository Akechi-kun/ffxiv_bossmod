namespace BossMod.Autorotation;

public sealed class ClassPLDUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Sheltron = SharedTrack.Count, Sentinel, Cover, Bulwark, DivineVeil, PassageOfArms, HallowedGround, ShieldBash, Clemency }
    public enum ShelOption { None, Sheltron, HolySheltron, Intervention }
    public enum SentOption { None, Sentinel, Guardian }
    public enum ArmsDirection { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PLD.AID.LastBastion);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(PLD.AID.IronWill);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(PLD.AID.ReleaseIronWill);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PLD", UtilityDescription, UtilityCategory, "veyn, Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.PLD), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        res.Define(Track.Sheltron).As<ShelOption>("Sheltron", "Shel", 350)
            .AddOption(ShelOption.None, NoUse)
            .AddOption(ShelOption.Sheltron, "Use Sheltron", 5, 4, ActionTargets.Self, 35, 81)
            .AddOption(ShelOption.HolySheltron, "Use Holy Sheltron", 5, 4, ActionTargets.Self, 82)
            .AddOption(ShelOption.Intervention, "Use Intervention on a party member (co-tank by default)", 10, 4, ActionTargets.Party, 62)
            .AddAssociatedActions(PLD.AID.Sheltron, PLD.AID.HolySheltron, PLD.AID.Intervention);

        res.Define(Track.Sentinel).As<SentOption>("Sentinel", "Sent", 550)
            .AddOption(SentOption.None, NoUse)
            .AddOption(SentOption.Sentinel, "Use Sentinel", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(SentOption.Guardian, "Use Guardian", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(PLD.AID.Sentinel, PLD.AID.Guardian);

        DefineSimpleConfig(res, Track.Cover, "Cover", "", 320, PLD.AID.Cover, 12);
        DefineSimpleConfig(res, Track.Bulwark, "Bulwark", "Bul", 450, PLD.AID.Bulwark, 10);
        DefineSimpleConfig(res, Track.DivineVeil, "DivineVeil", "Veil", 220, PLD.AID.DivineVeil, 30);

        res.Define(Track.PassageOfArms).As<ArmsDirection>("PassageOfArms", "PoA", 400)
            .AddOption(ArmsDirection.None, NoUse)
            .AddOption(ArmsDirection.CharacterForward, "Face forward relative to the character", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CharacterBackward, "Face backward relative to the character", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CameraForward, "Face forward relative to the camera", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CameraBackward, "Face backward relative to the camera", 120, 18, ActionTargets.Self, 70)
            .AddAssociatedActions(PLD.AID.PassageOfArms);

        DefineSimpleConfig(res, Track.HallowedGround, "HallowedGround", "Inv", 400, PLD.AID.HallowedGround, 10);
        DefineSimpleConfig(res, Track.ShieldBash, "ShieldBash", "ShieldBash", 340, PLD.AID.ShieldBash, 6, ActionQueue.Priority.VeryHigh);
        DefineSimpleConfig(res, Track.Clemency, "Clemency", "", 110, PLD.AID.Clemency, 0, ActionQueue.Priority.VeryHigh);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)PLD.SID.IronWill, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Cover), PLD.AID.Cover, PartnerTarget(primaryTarget));
        ExecuteSimple(strategy.Option(Track.Bulwark), PLD.AID.Bulwark, Player);
        ExecuteSimple(strategy.Option(Track.DivineVeil), PLD.AID.DivineVeil, Player);
        ExecuteSimple(strategy.Option(Track.HallowedGround), PLD.AID.HallowedGround, Player);
        ExecuteSimple(strategy.Option(Track.Clemency), PLD.AID.Clemency, FriendlyTarget(primaryTarget));
        ExecuteSheltron(strategy.Option(Track.Sheltron), primaryTarget);
        ExecuteSentinel(strategy.Option(Track.Sentinel));
        ExecutePassage(strategy.Option(Track.PassageOfArms));
        ExecuteShieldBash(strategy.Option(Track.ShieldBash), primaryTarget);
    }

    private void ExecuteSheltron(in StrategyValues.OptionRef shel, Actor? primaryTarget)
    {
        var action = shel.As<ShelOption>() switch
        {
            ShelOption.Sheltron => PLD.AID.Sheltron,
            ShelOption.HolySheltron => PLD.AID.HolySheltron,
            ShelOption.Intervention => PLD.AID.Intervention,
            _ => default
        };
        if (action != default)
            ExecuteAction(shel, action, action == PLD.AID.Intervention ? PartnerTarget(primaryTarget) : Player);
    }
    private void ExecuteSentinel(in StrategyValues.OptionRef sent)
    {
        var action = sent.As<SentOption>() switch
        {
            SentOption.Sentinel => PLD.AID.Sentinel,
            SentOption.Guardian => PLD.AID.Guardian,
            _ => default
        };
        if (action != default)
            ExecuteAction(sent, action, Player);
    }
    private void ExecutePassage(in StrategyValues.OptionRef poa)
    {
        var direction = poa.As<ArmsDirection>();
        var angle = direction switch
        {
            ArmsDirection.CharacterBackward => Player.Rotation + 180.Degrees(),
            ArmsDirection.CameraForward => World.Client.CameraAzimuth + 180.Degrees(),
            ArmsDirection.CameraBackward => World.Client.CameraAzimuth,
            _ => Player.Rotation
        };
        if (direction != ArmsDirection.None)
            ExecuteAction(poa, PLD.AID.PassageOfArms, Player, angle);
    }
    private void ExecuteShieldBash(in StrategyValues.OptionRef bash, Actor? primaryTarget)
    {
        var target = ResolveTarget(bash.Value) ?? primaryTarget;
        if (bash.As<SimpleOption>() == SimpleOption.Use && target?.FindStatus(WAR.SID.Stun) == null)
            ExecuteAction(bash, PLD.AID.ShieldBash, target);
    }
}
