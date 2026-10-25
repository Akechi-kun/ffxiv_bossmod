namespace BossMod.Autorotation;

public sealed class ClassGNBUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Camouflage = SharedTrack.Count, Nebula, Aurora, Superbolide, HeartOfLight, HeartOfCorundum, Trajectory }
    public enum HoCOption { None, HeartOfStone, HeartOfCorundum }
    public enum AuroraStrategy { None, Force, ForceHold1 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(GNB.AID.GunmetalSoul);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(GNB.AID.RoyalGuard);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(GNB.AID.ReleaseRoyalGuard);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: GNB", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.GNB), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.Camouflage, "Camouflage", "Camouflage", 500, GNB.AID.Camouflage, 20);
        DefineSimpleConfig(res, Track.Nebula, "Nebula", "Nebula", 550, GNB.AID.Nebula, 15);

        res.Define(Track.Aurora).As<AuroraStrategy>("Aurora", "Aurora", 150)
            .AddOption(AuroraStrategy.None, NoUse)
            .AddOption(AuroraStrategy.Force, "Use Aurora", 60, 18, ActionTargets.Self | ActionTargets.Party, 45)
            .AddOption(AuroraStrategy.ForceHold1, "Use Aurora; keep 1 charge", 60, 18, ActionTargets.Self | ActionTargets.Party, 84)
            .AddAssociatedActions(GNB.AID.Aurora);

        DefineSimpleConfig(res, Track.Superbolide, "Superbolide", "Superbolide", 600, GNB.AID.Superbolide, 10);
        DefineSimpleConfig(res, Track.HeartOfLight, "HeartOfLight", "Heart of Light", 245, GNB.AID.HeartOfLight, 15);

        res.Define(Track.HeartOfCorundum).As<HoCOption>("HeartOfCorundum", "Heart of Corundum", 350)
            .AddOption(HoCOption.None, NoUse)
            .AddOption(HoCOption.HeartOfStone, "Use Heart of Stone (co-tank by default, otherwise self)", 25, 7, ActionTargets.Self | ActionTargets.Party, 68, 81)
            .AddOption(HoCOption.HeartOfCorundum, "Use Heart of Corundum (co-tank by default, otherwise self)", 25, 4, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(GNB.AID.HeartOfStone, GNB.AID.HeartOfCorundum);

        DefineDash(res, Track.Trajectory, "Trajectory", "Trajectory", GNB.AID.Trajectory, ActionTargets.Hostile, hold1MinLevel: 84);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)GNB.SID.RoyalGuard, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Camouflage), GNB.AID.Camouflage, Player);
        ExecuteSimple(strategy.Option(Track.Nebula), GNB.AID.Nebula, Player);
        ExecuteSimple(strategy.Option(Track.Superbolide), GNB.AID.Superbolide, Player);
        ExecuteSimple(strategy.Option(Track.HeartOfLight), GNB.AID.HeartOfLight, Player);
        ExecuteAurora(strategy.Option(Track.Aurora));
        ExecuteHeart(strategy.Option(Track.HeartOfCorundum), primaryTarget);
        ExecuteDash(strategy.Option(Track.Trajectory), GNB.AID.Trajectory, primaryTarget);
    }

    private void ExecuteAurora(in StrategyValues.OptionRef aurora)
    {
        var option = aurora.As<AuroraStrategy>();
        var target = ResolveTarget(aurora.Value) ?? Player;
        var condition = option switch
        {
            AuroraStrategy.Force => true,
            AuroraStrategy.ForceHold1 => HasCharges(GNB.AID.Aurora, 1),
            _ => false
        };
        if (condition && !HasStatus(target, GNB.SID.Aurora))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(GNB.AID.Aurora), target, aurora.Priority(), aurora.Value.ExpireIn);
    }
    private void ExecuteHeart(in StrategyValues.OptionRef hoc, Actor? primaryTarget)
    {
        var action = hoc.As<HoCOption>() switch
        {
            HoCOption.HeartOfStone => GNB.AID.HeartOfStone,
            HoCOption.HeartOfCorundum => GNB.AID.HeartOfCorundum,
            _ => default
        };
        if (action != default)
            ExecuteAction(hoc, action, PartnerTarget(primaryTarget) ?? Player);
    }
}
