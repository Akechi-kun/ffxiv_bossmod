namespace BossMod.Autorotation;

public sealed class ClassDNCUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { CuringWaltz = SharedTrack.Count, ShieldSamba, Improvisation }
    public enum SambaOption { None, Use87, Use87IfNotActive, Use88, Use88IfNotActive }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DNC.AID.CrimsonLotus);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DNC", UtilityDescription, UtilityCategory, "xan", RotationModuleQuality.Ok, BitMask.Build((int)Class.DNC), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.CuringWaltz, "CuringWaltz", "C.Waltz", 400, DNC.AID.CuringWaltz);

        res.Define(Track.ShieldSamba).As<SambaOption>("Shield Samba", "S.Samba", 500)
            .AddOption(SambaOption.None, NoUse)
            .AddOption(SambaOption.Use87, "Use Shield Samba (120s cooldown), even if another ranged mitigation is active", 120, 15, ActionTargets.Self, 56, 87)
            .AddOption(SambaOption.Use87IfNotActive, "Use Shield Samba (120s cooldown), unless another ranged mitigation is active", 120, 15, ActionTargets.Self, 56, 87)
            .AddOption(SambaOption.Use88, "Use Shield Samba (90s cooldown), even if another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddOption(SambaOption.Use88IfNotActive, "Use Shield Samba (90s cooldown), unless another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddAssociatedActions(DNC.AID.ShieldSamba);

        DefineSimpleConfig(res, Track.Improvisation, "Improvisation", "Improv", 300, DNC.AID.Improvisation, 15);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.CuringWaltz), DNC.AID.CuringWaltz, Player);
        ExecuteSimple(strategy.Option(Track.Improvisation), DNC.AID.Improvisation, Player);
        ExecuteShieldSamba(strategy.Option(Track.ShieldSamba));
    }

    private void ExecuteShieldSamba(in StrategyValues.OptionRef samba)
    {
        var condition = samba.As<SambaOption>() switch
        {
            SambaOption.Use87 or SambaOption.Use88 => true,
            SambaOption.Use87IfNotActive or SambaOption.Use88IfNotActive => !HasRangedMitigation,
            _ => false
        };
        if (condition)
            ExecuteAction(samba, DNC.AID.ShieldSamba, Player);
    }
}
