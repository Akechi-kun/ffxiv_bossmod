﻿namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count }
    public enum DashStrategy { Automatic, Force, GapClose }
    public float CDleft => World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining;
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range

    public const float DashMinCD = 0.8f; //Triple-weaving dash is not a good idea, since it might delay gcd for longer than normal anim lock

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", "Planner support for utility actions", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.WingedGlide).As<DashStrategy>("Winged Glide", "Dash", 20)
            .AddOption(DashStrategy.Automatic, "Automatic", "No use.")
            .AddOption(DashStrategy.Force, "Force", "Use ASAP", 60, 0, ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 60, 0, ActionTargets.Hostile, 45)
            .AddAssociatedActions(DRG.AID.WingedGlide);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var dash = strategy.Option(Track.WingedGlide);
        var dashStrategy = strategy.Option(Track.WingedGlide).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, null))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.WingedGlide), null, dash.Priority());
    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.Automatic => false,
        DashStrategy.Force => CDleft >= DashMinCD && !InMeleeRange(primaryTarget),
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };
}
