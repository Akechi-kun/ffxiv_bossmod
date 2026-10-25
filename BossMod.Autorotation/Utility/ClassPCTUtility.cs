namespace BossMod.Autorotation;

public sealed class ClassPCTUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { TemperaCoat = SharedTrack.Count, Smudge }
    public enum TemperaCoatOption { None, CoatOnly, CoatGrassaASAP, CoatGrassaWhenever }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PCT.AID.ChromaticFantasy);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PCT", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.PCT), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.TemperaCoat).As<TemperaCoatOption>("Tempera Coat", "Tempera Coat", 600)
            .AddOption(TemperaCoatOption.None, NoUse)
            .AddOption(TemperaCoatOption.CoatOnly, "Use Tempera Coat only, without Tempera Grassa", 120, 10, ActionTargets.Self, 10)
            .AddOption(TemperaCoatOption.CoatGrassaASAP, "Use Tempera Coat, then Tempera Grassa, as soon as possible", 120, 10, ActionTargets.Self, 88, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(TemperaCoatOption.CoatGrassaWhenever, "Use Tempera Coat, then Tempera Grassa, in a weave window", 120, 10, ActionTargets.Self, 88)
            .AddAssociatedActions(PCT.AID.TemperaCoat, PCT.AID.TemperaGrassa);

        DefineSimpleConfig(res, Track.Smudge, "Smudge", "Smudge", 20, PCT.AID.Smudge, 5);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Smudge), PCT.AID.Smudge, Player);
        ExecuteTempera(strategy.Option(Track.TemperaCoat));
    }

    private void ExecuteTempera(in StrategyValues.OptionRef tempera)
    {
        var option = tempera.As<TemperaCoatOption>();
        var hasCoat = HasStatus(Player, PCT.SID.TemperaCoat);
        var hasGrassa = HasStatus(Player, PCT.SID.TemperaGrassa);
        if (option != TemperaCoatOption.None && !hasCoat && !hasGrassa)
            ExecuteAction(tempera, PCT.AID.TemperaCoat, Player);
        if (option is TemperaCoatOption.CoatGrassaASAP or TemperaCoatOption.CoatGrassaWhenever && hasCoat)
            ExecuteAction(tempera, PCT.AID.TemperaGrassa, Player);
    }
}
