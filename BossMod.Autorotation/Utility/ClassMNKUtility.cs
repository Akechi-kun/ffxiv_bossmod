namespace BossMod.Autorotation;

public sealed class ClassMNKUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Mantra = SharedTrack.Count, RiddleOfEarth, Thunderclap, EarthsReply }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MNK.AID.FinalHeaven);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MNK", UtilityDescription, UtilityCategory, "xan", RotationModuleQuality.Ok, BitMask.Build((int)Class.MNK), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Mantra, "Mantra", "Mantra", 100, MNK.AID.Mantra, 15);
        DefineSimpleConfig(res, Track.RiddleOfEarth, "RiddleOfEarth", "Riddle of Earth", 150, MNK.AID.RiddleOfEarth, 10);

        DefineDash(res, Track.Thunderclap, "Thunderclap", "Thunderclap", MNK.AID.Thunderclap, ActionTargets.Party | ActionTargets.Hostile, "Use as a gap closer when outside melee range; may use every charge");

        DefineSimpleConfig(res, Track.EarthsReply, "EarthsReply", "Earth's Reply", 140, MNK.AID.EarthsReply);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Mantra), MNK.AID.Mantra, Player);
        ExecuteSimple(strategy.Option(Track.RiddleOfEarth), MNK.AID.RiddleOfEarth, Player);
        ExecuteEarthsReply(strategy.Option(Track.EarthsReply));
        ExecuteDash(strategy.Option(Track.Thunderclap), MNK.AID.Thunderclap, primaryTarget);
    }

    private void ExecuteEarthsReply(in StrategyValues.OptionRef reply)
    {
        if (Player.FindStatus(MNK.SID.EarthsRumination) != null)
            ExecuteSimple(reply, MNK.AID.EarthsReply, Player);
    }
}
