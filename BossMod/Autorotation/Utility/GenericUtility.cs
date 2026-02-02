using System.Globalization;

namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Simple
    public enum SimpleOption { None, Use }
    protected static void DefineSimpleConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid, float effect = 0, float defaultPriority = 3000)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        def.Define(expectedIndex).As<SimpleOption>(internalName, displayName, uiPriority)
            .AddOption(SimpleOption.None, "Do not use automatically")
            .AddOption(SimpleOption.Use, $"Use {action.Name()}", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action), defaultPriority: defaultPriority)
            .AddAssociatedActions(aid);
    }
    protected void ExecuteSimple<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? defaultTarget, float castTime = 0) where AID : Enum
    {
        if (opt.As<SimpleOption>() == SimpleOption.Use)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(opt.Value) ?? defaultTarget, opt.Priority(), opt.Value.ExpireIn, castTime: castTime);
    }
    #endregion

    #region LB
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 }
    protected static RotationModuleDefinition.ConfigRef<LBOption> DefineLimitBreak<Index>(RotationModuleDefinition def, Index expectedIndex, ActionTargets allowedTargets, float effectLB1 = 0, float effectLB2 = 0, float effectLB3 = 0) where Index : Enum
    {
        // note: it assumes that effect durations are either 0's or correspond to tank LB (so lb2 > lb1 > lb3)
        return def.Define(expectedIndex).As<LBOption>("LB")
            .AddOption(LBOption.None, "Do not use automatically")
            .AddOption(LBOption.LB3, "Use LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2, "Use LB2/3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1, "Use any LB if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2Only, "Use LB2 if available, but not LB3", 0, effectLB2, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1Only, "Use LB1 if available, but not LB2+", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB12, "Use LB1/2 if available, but not LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh);
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
    #endregion

    #region Dash
    public enum DashOption { None, Any, Melee, Y5, Y10, Y15 }
    protected static void DefineDashConfig<Index, AID>(RotationModuleDefinition res, Index track, string displayName, int uiPriority, AID action, float effect = 0, float defaultPriority = 3000, string internalName = "Dash")
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var aid = ActionID.MakeSpell(action);
        var adata = adefs[aid]!;
        res.Define(track).As<DashOption>(internalName, displayName, uiPriority)
            .AddOption(DashOption.None, $"Do not use automatically")
            .AddOption(DashOption.Any, $"Use {aid.Name()} ASAP, regardless of range", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(aid), defaultPriority: defaultPriority)
            .AddOption(DashOption.Melee, $"Use {aid.Name()} only if outside melee range of selected target", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(aid), defaultPriority: defaultPriority)
            .AddOption(DashOption.Y5, $"Use {aid.Name()} only if 5 or more yalms away from selected target", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(aid), defaultPriority: defaultPriority)
            .AddOption(DashOption.Y10, $"Use {aid.Name()} only if 10 or more yalms away from selected target", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(aid), defaultPriority: defaultPriority)
            .AddOption(DashOption.Y15, $"Use {aid.Name()} only if 15 or more yalms away from selected target", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(aid), defaultPriority: defaultPriority)
            .AddAssociatedActions(action);
    }

    protected void ExecuteDash<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? defaultTarget, float castTime = 0) where AID : Enum
    {
        var target = ResolveTargetOverride(opt.Value) ?? defaultTarget;
        var distance = Player.DistanceToHitbox(target);
        if (ReadyIn(aid) <= 0.3f && opt.As<DashOption>() switch
        {
            DashOption.Any => distance is <= 20,
            DashOption.Melee => distance is > 3.5f and <= 20,
            DashOption.Y5 => distance is >= 5f and <= 20,
            DashOption.Y10 => distance is >= 10f and <= 20,
            DashOption.Y15 => distance is >= 15f and <= 20,
            _ => false
        })
            Hints.ActionsToExecute.Push
                (
                    ActionID.MakeSpell(aid),
                    ResolveTargetOverride(opt.Value) ?? defaultTarget,
                    opt.Priority(),
                    opt.Value.ExpireIn,
                    castTime: castTime
                );
    }

    #endregion

    protected bool Unlocked<AID>(AID action) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(action));
    protected float ReadyIn<AID>(AID action) where AID : Enum => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
}
