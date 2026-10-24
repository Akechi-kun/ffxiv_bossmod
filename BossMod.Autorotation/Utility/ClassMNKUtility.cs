namespace BossMod.Autorotation;

public sealed class ClassMNKUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Mantra = SharedTrack.Count, RiddleOfEarth, Thunderclap, EarthsReply }
    public enum DashStrategy { None, GapClose, Five, Ten }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MNK.AID.FinalHeaven);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MNK", UtilityDescription, UtilityCategory, "xan", RotationModuleQuality.Ok, BitMask.Build((int)Class.MNK), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Mantra, "Mantra", "", 100, MNK.AID.Mantra, 15);
        DefineSimpleConfig(res, Track.RiddleOfEarth, "RiddleOfEarth", "RoE", 150, MNK.AID.RiddleOfEarth, 10);

        res.Define(Track.Thunderclap).As<DashStrategy>("Thunderclap", "Dash", 20)
            .AddOption(DashStrategy.None, NoUse)
            .AddOption(DashStrategy.GapClose, "Use as a gap closer when outside melee range; may use every charge", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddOption(DashStrategy.Five, "Use when 5y or more from target", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddOption(DashStrategy.Ten, "Use when 10y or more from target", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddAssociatedActions(MNK.AID.Thunderclap);

        DefineSimpleConfig(res, Track.EarthsReply, "EarthsReply", "Reply", 140, MNK.AID.EarthsReply);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Mantra), MNK.AID.Mantra, Player);
        ExecuteSimple(strategy.Option(Track.RiddleOfEarth), MNK.AID.RiddleOfEarth, Player);
        ExecuteEarthsReply(strategy.Option(Track.EarthsReply));
        ExecuteThunderclap(strategy.Option(Track.Thunderclap), primaryTarget);
    }

    private void ExecuteEarthsReply(in StrategyValues.OptionRef reply)
    {
        if (Player.FindStatus(MNK.SID.EarthsRumination) != null)
            ExecuteSimple(reply, MNK.AID.EarthsReply, Player);
    }
    private void ExecuteThunderclap(in StrategyValues.OptionRef dash, Actor? primaryTarget)
    {
        var option = dash.As<DashStrategy>();
        var minDistance = option switch
        {
            DashStrategy.Five => 5,
            DashStrategy.Ten => 10,
            _ => 3
        };
        if (option != DashStrategy.None)
            ExecuteDash(dash, MNK.AID.Thunderclap, primaryTarget, minDistance);
    }
}
