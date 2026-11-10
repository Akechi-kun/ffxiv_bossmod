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

        res.Define(Track.Sheltron).As<ShelOption>("Sheltron", "Sheltron", 350)
            .AddOption(ShelOption.None, NoUse)
            .AddOption(ShelOption.Sheltron, "Use Sheltron", 5, 4, ActionTargets.Self, 35, 81)
            .AddOption(ShelOption.HolySheltron, "Use Holy Sheltron", 5, 4, ActionTargets.Self, 82)
            .AddOption(ShelOption.Intervention, "Use Intervention on a party member (co-tank by default)", 10, 4, ActionTargets.Party, 62)
            .AddAssociatedActions(PLD.AID.Sheltron, PLD.AID.HolySheltron, PLD.AID.Intervention);

        res.Define(Track.Sentinel).As<SentOption>("Sentinel", "Sentinel", 550)
            .AddOption(SentOption.None, NoUse)
            .AddOption(SentOption.Sentinel, "Use Sentinel", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(SentOption.Guardian, "Use Guardian", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(PLD.AID.Sentinel, PLD.AID.Guardian);

        DefineSimple(res, Track.Cover, "Cover", "Cover", 320, PLD.AID.Cover, 12);
        DefineSimple(res, Track.Bulwark, "Bulwark", "Bulwark", 450, PLD.AID.Bulwark, 10);
        DefineSimple(res, Track.DivineVeil, "DivineVeil", "Divine Veil", 220, PLD.AID.DivineVeil, 30);

        res.Define(Track.PassageOfArms).As<ArmsDirection>("PassageOfArms", "Passage of Arms", 400)
            .AddOption(ArmsDirection.None, NoUse)
            .AddOption(ArmsDirection.CharacterForward, "Face forward relative to the character", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CharacterBackward, "Face backward relative to the character", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CameraForward, "Face forward relative to the camera", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CameraBackward, "Face backward relative to the camera", 120, 18, ActionTargets.Self, 70)
            .AddAssociatedActions(PLD.AID.PassageOfArms);

        DefineSimple(res, Track.HallowedGround, "HallowedGround", "Hallowed Ground", 400, PLD.AID.HallowedGround, 10);
        DefineSimple(res, Track.ShieldBash, "ShieldBash", "Shield Bash", 340, PLD.AID.ShieldBash, 6, ActionQueue.Priority.VeryHigh);
        DefineSimple(res, Track.Clemency, "Clemency", "Clemency", 110, PLD.AID.Clemency, 0, ActionQueue.Priority.VeryHigh);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)PLD.SID.IronWill, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Cover), PLD.AID.Cover, AllyOrCoTank(primaryTarget));
        ExecuteSimple(strategy.Option(Track.Bulwark), PLD.AID.Bulwark, Player);
        ExecuteSimple(strategy.Option(Track.DivineVeil), PLD.AID.DivineVeil, Player);
        ExecuteSimple(strategy.Option(Track.HallowedGround), PLD.AID.HallowedGround, Player);
        ExecuteSimple(strategy.Option(Track.Clemency), PLD.AID.Clemency, AllyOrSelf(primaryTarget));
        var sheltron = strategy.Option(Track.Sheltron);
        ExecuteVariant(sheltron, sheltron.As<ShelOption>() == ShelOption.Intervention ? AllyOrCoTank(primaryTarget) : Player, PLD.AID.Sheltron, PLD.AID.HolySheltron, PLD.AID.Intervention);
        ExecuteVariant(strategy.Option(Track.Sentinel), Player, PLD.AID.Sentinel, PLD.AID.Guardian);
        ExecutePassage(strategy.Option(Track.PassageOfArms));
        ExecuteShieldBash(strategy.Option(Track.ShieldBash), primaryTarget);
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
        if (!HasStatus(ResolveTarget(bash.Value) ?? primaryTarget, WAR.SID.Stun))
            ExecuteSimple(bash, PLD.AID.ShieldBash, primaryTarget);
    }
}
