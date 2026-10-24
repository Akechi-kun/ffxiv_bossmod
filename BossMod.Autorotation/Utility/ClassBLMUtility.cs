namespace BossMod.Autorotation;

public sealed class ClassBLMUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { Manaward = SharedTrack.Count, AetherialManipulation }
    public enum DashStrategy { None, Force, Five, Ten }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BLM.AID.Meteor);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLM", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.BLM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Manaward, "Manaward", "", 600, BLM.AID.Manaward, 20);

        res.Define(Track.AetherialManipulation).As<DashStrategy>("Dash", "", 20)
            .AddOption(DashStrategy.None, NoUse)
            .AddOption(DashStrategy.Force, "Dash to the selected party member (or your current ally target if no member selected)", 10, 0, ActionTargets.Party, 50)
            .AddOption(DashStrategy.Five, "Dash to the party member when 5y or more away", 10, 0, ActionTargets.Party, 50)
            .AddOption(DashStrategy.Ten, "Dash to the party member when 10y or more away", 10, 0, ActionTargets.Party, 50)
            .AddAssociatedActions(BLM.AID.AetherialManipulation);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Manaward), BLM.AID.Manaward, Player);
        ExecuteManipulation(strategy.Option(Track.AetherialManipulation), primaryTarget);
    }

    private void ExecuteManipulation(in StrategyValues.OptionRef dash, Actor? primaryTarget)
    {
        var option = dash.As<DashStrategy>();
        var minDistance = option switch
        {
            DashStrategy.Five => 5,
            DashStrategy.Ten => 10,
            _ => 0
        };
        if (option != DashStrategy.None)
            ExecuteDash(dash, BLM.AID.AetherialManipulation, primaryTarget is { IsAlly: true } ? primaryTarget : null, minDistance);
    }
}
