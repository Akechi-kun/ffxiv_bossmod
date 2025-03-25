﻿namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum GCDOption { None, Use }
    public enum OGCDOption { None, Use, AnyWeave, EarlyWeave, LateWeave }
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 }

    protected static void DefineSimpleGCD<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid, float effect = 0)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        def.Define(expectedIndex).As<GCDOption>(internalName, displayName, uiPriority)
            .AddOption(GCDOption.None, "None", "Do not use automatically")
            .AddOption(GCDOption.Use, "Use", $"Use {action.Name()}", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddAssociatedActions(aid);
    }

    protected static void DefineSimpleOGCD<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid, float effect = 0)
    where Index : Enum
    where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        def.Define(expectedIndex).As<OGCDOption>(internalName, displayName, uiPriority)
            .AddOption(OGCDOption.None, "None", "Do not use automatically")
            .AddOption(OGCDOption.Use, "Use", $"Use {action.Name()}, regardless of weave window", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddOption(OGCDOption.AnyWeave, "AnyWeave", $"Use {action.Name()} in any next weave window", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddOption(OGCDOption.EarlyWeave, "EarlyWeave", $"Use {action.Name()} asap in any next early-weave window", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddOption(OGCDOption.LateWeave, "LateWeave", $"Use {action.Name()} asap in any next late-weave window", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddAssociatedActions(aid);
    }

    protected static RotationModuleDefinition.ConfigRef<LBOption> DefineLimitBreak<Index>(RotationModuleDefinition def, Index expectedIndex, ActionTargets allowedTargets, float effectLB1 = 0, float effectLB2 = 0, float effectLB3 = 0) where Index : Enum
    {
        // note: it assumes that effect durations are either 0's or correspond to tank LB (so lb2 > lb1 > lb3)
        return def.Define(expectedIndex).As<LBOption>("LB")
            .AddOption(LBOption.None, "None", "Do not use automatically")
            .AddOption(LBOption.LB3, "LB3", "Use LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2, "LB2", "Use LB2/3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1, "LB1", "Use any LB if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2Only, "LB2Only", "Use LB2 if available, but not LB3", 0, effectLB2, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1Only, "LB1Only", "Use LB1 if available, but not LB2+", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB12, "LB12", "Use LB1/2 if available, but not LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh);
    }

    protected void ExecuteSimple<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? defaultTarget, float castTime = 0) where AID : Enum
    {
        if (opt.As<GCDOption>() == GCDOption.Use)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(opt.Value) ?? defaultTarget, opt.Priority(), opt.Value.ExpireIn, castTime: castTime);
        if (opt.As<OGCDOption>() == OGCDOption.Use ||
            opt.As<OGCDOption>() == OGCDOption.AnyWeave && GCD is < 2.5f and >= 0.6f ||
            opt.As<OGCDOption>() == OGCDOption.EarlyWeave && GCD is < 2.5f and >= 1.26f ||
            opt.As<OGCDOption>() == OGCDOption.LateWeave && GCD is <= 1.25f and >= 0.6f)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(opt.Value) ?? defaultTarget, opt.Priority(), opt.Value.ExpireIn, castTime: castTime);
    }

    // returns 0 if not needed, or current LB level
    protected int LBLevelToExecute(LBOption option)
    {
        // note: limit break's animation lock is very long, so we're guaranteed to delay next gcd at least a bit => priority has to be higher than gcd
        // note: while we could arguably delay that until right after next gcd, it's too risky, let the user deal with it by planning carefully...
        if (option == LBOption.None)
            return 0;
        var curLevel = World.Party.LimitBreakLevel;
        return option switch
        {
            LBOption.LB3 => curLevel == 3,
            LBOption.LB2 => curLevel >= 2,
            LBOption.LB1 => curLevel >= 1,
            LBOption.LB2Only => curLevel == 2,
            LBOption.LB1Only => curLevel == 1,
            LBOption.LB12 => curLevel is 1 or 2,
            _ => false
        } ? curLevel : 0;
    }
}
