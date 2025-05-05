using BossMod.BLM;

namespace BossMod.Autorotation;

public sealed class ClassBLMUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { Manaward = SharedTrack.Count, AetherialManipulation, LeyLines, BTL, Retrace, Triplecast, Amplifier }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AID.Meteor);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLM", "Cooldown Planner support for Utility and Buff actions.\nNOTE: This is NOT a rotation preset!", "Cooldown Planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.BLM), 100);
        DefineShared(res, IDLimitBreak3);
        DefineSimpleConfig(res, Track.Manaward, "Manaward", "", 600, AID.Manaward, 20);
        DefineDashConfig(res, Track.AetherialManipulation, "Aetherial Manipulation", "AM", 600, AID.AetherialManipulation);
        DefineSimpleConfig(res, Track.LeyLines, "Ley Lines", "", 600, AID.LeyLines, 20);
        DefineSimpleConfig(res, Track.BTL, "Between The Lines", "", 600, AID.BetweenTheLines);
        DefineSimpleConfig(res, Track.Retrace, "Retrace", "", 600, AID.Retrace);
        DefineSimpleConfig(res, Track.Triplecast, "Triplecast", "", 600, AID.Triplecast, 15);
        DefineSimpleConfig(res, Track.Amplifier, "Amplifier", "", 600, AID.Amplifier);
        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        if (CDleft(AID.Manaward) <= GCD)
            ExecuteSimple(strategy.Option(Track.Manaward), AID.Manaward, Player);
        if (CDleft(AID.Amplifier) <= GCD)
            ExecuteSimple(strategy.Option(Track.Amplifier), AID.Amplifier, Player);
        if (!HasEffect(SID.Triplecast))
            ExecuteSimple(strategy.Option(Track.Triplecast), AID.Triplecast, Player);
        if (CDleft(AID.AetherialManipulation) < GCD)
            ExecuteDash(strategy.Option(Track.AetherialManipulation), AID.AetherialManipulation, primaryTarget);
        var llCircle = World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID);
        if (llCircle == null && !HasEffect(SID.LeyLines))
            ExecuteSimple(strategy.Option(Track.LeyLines), AID.LeyLines, Player, targetPos: Player.PosRot.XYZ());
        if (llCircle != null && HasEffect(SID.LeyLines) && !HasEffect(SID.CircleOfPower))
        {
            ExecuteSimple(strategy.Option(Track.BTL), AID.BetweenTheLines, Player, targetPos: llCircle.PosRot.XYZ());
            if (CDleft(AID.Retrace) <= GCD)
                ExecuteSimple(strategy.Option(Track.Retrace), AID.Retrace, Player, targetPos: Player.PosRot.XYZ());
        }
    }
}
