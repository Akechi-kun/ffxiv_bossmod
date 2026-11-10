namespace BossMod.Autorotation;

public sealed class ClassRPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ArcaneCrest = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RPR.AID.TheEnd);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RPR", UtilityDescription, UtilityCategory, "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.RPR), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimple(res, Track.ArcaneCrest, "Crest", "Arcane Crest", 600, RPR.AID.ArcaneCrest, 5);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ArcaneCrest), RPR.AID.ArcaneCrest, Player);
    }
}
