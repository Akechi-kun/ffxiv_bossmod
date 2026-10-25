namespace BossMod.Autorotation;

public sealed class ClassRPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ArcaneCrest = SharedTrack.Count, Hellsgate }
    public enum HellsgateOption { None, Ingress, Egress }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RPR.AID.TheEnd);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RPR", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.RPR), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ArcaneCrest, "Crest", "Arcane Crest", 600, RPR.AID.ArcaneCrest, 5);

        res.Define(Track.Hellsgate).As<HellsgateOption>("Hellsgate", "Hell's Ingress/Egress", 20)
            .AddOption(HellsgateOption.None, NoUse)
            .AddOption(HellsgateOption.Ingress, "Use Hell's Ingress (dash 15y forward)", 20, 0, ActionTargets.Self, 20)
            .AddOption(HellsgateOption.Egress, "Use Hell's Egress (dash 15y backward)", 20, 0, ActionTargets.Self, 20)
            .AddAssociatedActions(RPR.AID.HellsIngress, RPR.AID.HellsEgress);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ArcaneCrest), RPR.AID.ArcaneCrest, Player);
        ExecuteHellsgate(strategy.Option(Track.Hellsgate));
    }

    private void ExecuteHellsgate(in StrategyValues.OptionRef gate)
    {
        var action = gate.As<HellsgateOption>() switch
        {
            HellsgateOption.Ingress => RPR.AID.HellsIngress,
            HellsgateOption.Egress => RPR.AID.HellsEgress,
            _ => default
        };
        if (action != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(action), Player, gate.Priority(), gate.Value.ExpireIn);
    }
}
