using static BossMod.Autorotation.ClassASTUtility;

namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum SimpleOption { None, Use } //simple
    public enum EXOption { None, Use, UseEX } //for options that have upgrades
    public enum EndOption { None, Use, End } //for options that can be ended early
    public enum DashOption { None, Use, Melee, Five, Ten } //distances for dashes
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 } //Limit Breaks

    protected float CDleft<AID>(AID aid) where AID : Enum => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;
    protected bool IsUnlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool IsTraitUnlocked<TraitID>(TraitID tid) where TraitID : Enum => TraitUnlocked((uint)(object)tid); //trait checks
    protected bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus(sid) != null; //status checks
    protected bool ActionReady<AID>(AID aid, float remaining = 0f) where AID : Enum => IsUnlocked(aid) && CDleft(aid) < remaining; //simple condition check that can be used for just about every ability
    protected bool GCDReady<AID>(AID aid) where AID : Enum => ActionReady(aid, GCD); //simple GCD check
    protected bool OGCDReady<AID>(AID aid) where AID : Enum => ActionReady(aid, 2.5f); //simple OGCD check

    protected static void DefineSimpleConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid, float effect = 0)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        def.Define(expectedIndex).As<SimpleOption>(internalName, displayName, uiPriority)
            .AddOption(SimpleOption.None, "None", "Do not use automatically")
            .AddOption(SimpleOption.Use, "Use", $"Use {action.Name()}", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddAssociatedActions(aid);
    }
    protected static void DefineEXConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid1, AID aid2, int EXbreakpoint, float effect = 0)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action1 = ActionID.MakeSpell(aid1);
        var action2 = ActionID.MakeSpell(aid2);
        var adata1 = adefs[action1]!;
        var adata2 = adefs[action2]!;
        def.Define(expectedIndex).As<EXOption>(internalName, displayName, uiPriority)
            .AddOption(EXOption.None, "None", "Do not use automatically")
            .AddOption(EXOption.Use, "Use", $"Use {action1.Name()}", adata1.Cooldown, effect, adata1.AllowedTargets, adefs.ActionMinLevel(action1), EXbreakpoint)
            .AddOption(EXOption.UseEX, "UseEX", $"Use {action2.Name()}", adata2.Cooldown, effect, adata2.AllowedTargets, adefs.ActionMinLevel(action2))
            .AddAssociatedActions(aid1, aid2);
    }
    protected static void DefineEndConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID start, AID end, float effect = 0)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var startAID = ActionID.MakeSpell(start);
        var endAID = ActionID.MakeSpell(end);
        var adata1 = adefs[startAID]!;
        var adata2 = adefs[endAID]!;
        def.Define(expectedIndex).As<EndOption>(internalName, displayName, uiPriority)
            .AddOption(EndOption.None, "None", "Do not use automatically")
            .AddOption(EndOption.Use, "Use", $"Use {startAID.Name()}", adata1.Cooldown, effect, adata1.AllowedTargets, adefs.ActionMinLevel(startAID))
            .AddOption(EndOption.End, "End", $"Use {endAID.Name()}", adata2.Cooldown, effect, adata2.AllowedTargets, adefs.ActionMinLevel(endAID))
            .AddAssociatedActions(start, end);
    }
    protected static void DefineDashConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        def.Define(expectedIndex).As<DashOption>(internalName, displayName, uiPriority)
            .AddOption(DashOption.None, "None", "Do not use automatically")
            .AddOption(DashOption.Use, "Use", $"Use {action.Name()} at any distance", adata.Cooldown, 0, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddOption(DashOption.Melee, "Melee", $"Use {action.Name()} if further than max melee distance from target hitbox", adata.Cooldown, 0, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddOption(DashOption.Five, "Five", $"Use {action.Name()} if further than five yalms in distance from target hitbox", adata.Cooldown, 0, adata.AllowedTargets, adefs.ActionMinLevel(action))
            .AddOption(DashOption.Ten, "Ten+", $"Use {action.Name()} if further than ten yalms or more in distance from target hitbox", adata.Cooldown, 0, adata.AllowedTargets, adefs.ActionMinLevel(action))
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

    protected void ExecuteSimple<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? defaultTarget, float castTime = 0, Vector3 targetPos = default) where AID : Enum
    {
        if (opt.As<SimpleOption>() == SimpleOption.Use)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(opt.Value) ?? defaultTarget, opt.Priority(), opt.Value.ExpireIn, castTime: castTime, targetPos: targetPos);
    }
    protected void ExecuteEX<AID>(in StrategyValues.OptionRef opt, AID aid1, AID aid2, Actor? defaultTarget, float castTime = 0, Vector3 targetPos = default) where AID : Enum
    {
        var action = opt.As<EXOption>() switch
        {
            EXOption.Use => ActionID.MakeSpell(aid1),
            EXOption.UseEX => ActionID.MakeSpell(aid2),
            _ => default
        };
        if (action != default)
            Hints.ActionsToExecute.Push(action, ResolveTargetOverride(opt.Value) ?? defaultTarget, opt.Priority(), opt.Value.ExpireIn, castTime: castTime, targetPos: targetPos);
    }
    protected void ExecuteEnd<AID, SID>(in StrategyValues.OptionRef opt, AID startAID, AID endAID, SID startSID, SID endSID, Actor? defaultTarget = null, float castTime = 0, Vector3 targetPos = default)
        where AID : Enum
        where SID : Enum
    {
        var status = opt.As<EndOption>() switch
        {
            EndOption.Use => CDleft(startAID) <= 2f && Player.FindStatus(startSID) == null, //condition to start
            EndOption.End => Player.FindStatus(endSID) != null, //condition to end
            _ => false
        };
        var action = opt.As<EndOption>() switch
        {
            EndOption.Use => ActionID.MakeSpell(startAID), //action to start
            EndOption.End => ActionID.MakeSpell(endAID), //action to end
            _ => default
        };
        if (status && action != default)
            Hints.ActionsToExecute.Push(action, ResolveTargetOverride(opt.Value) ?? defaultTarget, opt.Priority(), opt.Value.ExpireIn, castTime: castTime, targetPos: targetPos);
    }
    protected void ExecuteDash<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? defaultTarget, Vector3 targetPos = default) where AID : Enum
    {
        var dashTarget = ResolveTargetOverride(opt.Value) ?? defaultTarget;
        var distance = opt.As<DashOption>() switch
        {
            DashOption.Use => Player.DistanceToHitbox(dashTarget) >= 0.0f, //use wherever
            DashOption.Melee => Player.DistanceToHitbox(dashTarget) >= 2.5f,
            DashOption.Five => Player.DistanceToHitbox(dashTarget) >= 5.0f,
            DashOption.Ten => Player.DistanceToHitbox(dashTarget) >= 10.0f,
            _ => default
        };
        if (distance && opt.As<DashOption>() != DashOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), dashTarget, opt.Priority(), opt.Value.ExpireIn, targetPos: targetPos);
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
