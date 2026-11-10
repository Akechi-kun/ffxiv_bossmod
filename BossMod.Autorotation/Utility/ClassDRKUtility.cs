namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Shadowstride }
    public enum WallOption { None, ShadowWall, ShadowedVigil }
    public enum OblationStrategy { None, Use, Both }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRK.AID.DarkForce);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(DRK.AID.Grit);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(DRK.AID.ReleaseGrit);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRK", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.DRK), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimple(res, Track.DarkMind, "DarkMind", "Dark Mind", 450, DRK.AID.DarkMind, 10);

        res.Define(Track.ShadowWall).As<WallOption>("ShadowWall", "Shadow Wall", 550)
            .AddOption(WallOption.None, NoUse)
            .AddOption(WallOption.ShadowWall, "Use Shadow Wall", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(WallOption.ShadowedVigil, "Use Shadowed Vigil", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(DRK.AID.ShadowWall, DRK.AID.ShadowedVigil);

        DefineSimple(res, Track.LivingDead, "LivingDead", "Living Dead", 400, DRK.AID.LivingDead, 10);

        res.Define(Track.TheBlackestNight).As<SimpleOption>("TheBlackestNight", "The Blackest Night", 550)
            .AddOption(SimpleOption.None, NoUse)
            .AddOption(SimpleOption.Use, "Use The Blackest Night", 15, 7, ActionTargets.Self | ActionTargets.Party, 70, internalNameOverride: "Force")
            .AddAssociatedActions(DRK.AID.TheBlackestNight);

        res.Define(Track.Oblation).As<OblationStrategy>("Oblation", "Oblation", 550)
            .AddOption(OblationStrategy.None, NoUse)
            .AddOption(OblationStrategy.Use, "Use Oblation", 60, 10, ActionTargets.Self | ActionTargets.Party, 82, internalNameOverride: "Force")
            .AddOption(OblationStrategy.Both, "Use Oblation on self and co-tank", 60, 10, ActionTargets.Self, 82)
            .AddAssociatedActions(DRK.AID.Oblation);

        DefineSimple(res, Track.DarkMissionary, "DarkMissionary", "Dark Missionary", 220, DRK.AID.DarkMissionary, 15);

        DefineDash(res, Track.Shadowstride, "Shadowstride", "Shadowstride", DRK.AID.Shadowstride, ActionTargets.Hostile);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)DRK.SID.Grit, primaryTarget);
        ExecuteSimple(strategy.Option(Track.DarkMind), DRK.AID.DarkMind, Player);
        ExecuteSimple(strategy.Option(Track.LivingDead), DRK.AID.LivingDead, Player);
        ExecuteSimple(strategy.Option(Track.DarkMissionary), DRK.AID.DarkMissionary, Player);
        ExecuteSimple(strategy.Option(Track.TheBlackestNight), DRK.AID.TheBlackestNight, Player);
        ExecuteVariant(strategy.Option(Track.ShadowWall), Player, DRK.AID.ShadowWall, DRK.AID.ShadowedVigil);
        ExecuteOblation(strategy.Option(Track.Oblation));
        ExecuteDash(strategy.Option(Track.Shadowstride), DRK.AID.Shadowstride, primaryTarget);
    }

    private void ExecuteOblation(in StrategyValues.OptionRef oblation)
    {
        var option = oblation.As<OblationStrategy>();
        var target = option == OblationStrategy.Both ? Player : ResolveTarget(oblation.Value) ?? Player;
        if (option != OblationStrategy.None && !HasStatus(target, DRK.SID.Oblation))
            ExecuteOn(oblation, DRK.AID.Oblation, target);
        if (option == OblationStrategy.Both && CoTank() is { } coTank && !HasStatus(coTank, DRK.SID.Oblation))
            ExecuteOn(oblation, DRK.AID.Oblation, coTank);
    }
}
