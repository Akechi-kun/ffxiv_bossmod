namespace BossMod.Autorotation;

public abstract class RoleRangedUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, LegGraze, SecondWind, FootGraze, HeadGraze, ArmsLength, Count }
    public enum MitigationOption { None, Use87, Use87IfNotActive, Use88, Use88IfNotActive }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "Sprint", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Hostile)
            .AddAssociatedActions(ClassShared.AID.BigShot, ClassShared.AID.Desperado)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.LegGraze, "LegGraze", "Leg Graze", -100, ClassShared.AID.LegGraze, 10);
        DefineSimpleConfig(def, SharedTrack.SecondWind, "SecondWind", "Second Wind", 20, ClassShared.AID.SecondWind);
        DefineSimpleConfig(def, SharedTrack.FootGraze, "FootGraze", "Foot Graze", -150, ClassShared.AID.FootGraze, 10);
        DefineSimpleConfig(def, SharedTrack.HeadGraze, "HeadGraze", "Head Graze", -50, ClassShared.AID.HeadGraze);
        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsLength", "Arm's Length", 300, ClassShared.AID.ArmsLength, 6);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LegGraze), ClassShared.AID.LegGraze, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.SecondWind), ClassShared.AID.SecondWind, Player);
        ExecuteSimple(strategy.Option(SharedTrack.FootGraze), ClassShared.AID.FootGraze, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.HeadGraze), ClassShared.AID.HeadGraze, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);
        ExecuteLimitBreak(strategy.Option(SharedTrack.LB), ClassShared.AID.BigShot, ClassShared.AID.Desperado, lb3, primaryTarget);
    }

    protected static void DefineMitigation<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, float uiPriority, AID aid, int minLevel)
        where Index : Enum
        where AID : Enum
    {
        var name = ActionID.MakeSpell(aid).Name();
        def.Define(expectedIndex).As<MitigationOption>(internalName, displayName, uiPriority)
            .AddOption(MitigationOption.None, NoUse)
            .AddOption(MitigationOption.Use87, $"Use {name} (120s cooldown), even if another ranged mitigation is active", 120, 15, ActionTargets.Self, minLevel, 87)
            .AddOption(MitigationOption.Use87IfNotActive, $"Use {name} (120s cooldown), unless another ranged mitigation is active", 120, 15, ActionTargets.Self, minLevel, 87)
            .AddOption(MitigationOption.Use88, $"Use {name} (90s cooldown), even if another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddOption(MitigationOption.Use88IfNotActive, $"Use {name} (90s cooldown), unless another ranged mitigation is active", 90, 15, ActionTargets.Self, 88)
            .AddAssociatedActions(aid);
    }

    protected void ExecuteMitigation<AID>(in StrategyValues.OptionRef opt, AID aid) where AID : Enum
    {
        var condition = opt.As<MitigationOption>() switch
        {
            MitigationOption.Use87 or MitigationOption.Use88 => true,
            MitigationOption.Use87IfNotActive or MitigationOption.Use88IfNotActive => !HasRangedMitigation,
            _ => false
        };
        if (condition)
            ExecuteAction(opt, aid, Player);
    }

    protected bool HasRangedMitigation => Player.FindStatus(BRD.SID.Troubadour) != null || Player.FindStatus(MCH.SID.Tactician) != null || Player.FindStatus(DNC.SID.ShieldSamba) != null;
}
