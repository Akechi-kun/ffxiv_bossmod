namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum SimpleOption { None, Use }
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 }

    protected const string UtilityDescription = "Cooldown Planner support for utility actions.\nThis is not a rotation preset; utility modules are meant for cooldown planning only.";
    protected const string UtilityCategory = "Utility for planner";
    protected const string NoUse = "Do not use automatically";

    protected static void DefineSimpleConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid, float effect = 0, float defaultPriority = 3000)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        def.Define(expectedIndex).As<SimpleOption>(internalName, displayName, uiPriority)
            .AddOption(SimpleOption.None, NoUse)
            .AddOption(SimpleOption.Use, $"Use {action.Name()}", adata.Cooldown, effect, adata.AllowedTargets, adefs.ActionMinLevel(action), defaultPriority: defaultPriority)
            .AddAssociatedActions(aid);
    }

    protected static RotationModuleDefinition.ConfigRef<LBOption> DefineLimitBreak<Index>(RotationModuleDefinition def, Index expectedIndex, ActionTargets allowedTargets, float effectLB1 = 0, float effectLB2 = 0, float effectLB3 = 0) where Index : Enum
    {
        return def.Define(expectedIndex).As<LBOption>("LB")
            .AddOption(LBOption.None, NoUse)
            .AddOption(LBOption.LB3, "Use LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2, "Use LB2 or LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1, "Use any LB if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2Only, "Use LB2 if available, but not LB3", 0, effectLB2, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1Only, "Use LB1 if available, but not LB2 or LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB12, "Use LB1 or LB2 if available, but not LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh);
    }

    protected Actor? CoTank() => ActionDefinitions.FindCoTank(World, Player);
    protected Actor FriendlyTarget(Actor? primaryTarget) => primaryTarget is { IsAlly: true } ? primaryTarget : Player;
    protected Actor? PartnerTarget(Actor? primaryTarget) => primaryTarget is { IsAlly: true } && primaryTarget != Player ? primaryTarget : CoTank();
    protected Actor? EsunaTarget(Actor? primaryTarget) => primaryTarget is { IsAlly: true } ? primaryTarget : ActionDefinitions.FindEsunaTarget(World) ?? null;

    protected bool HasStatus<SID>(Actor? target, SID sid) where SID : Enum => target?.FindStatus(sid) != null;
    protected bool HasSwiftcast => HasStatus(Player, ClassShared.SID.Swiftcast);
    protected bool HasCharges<AID>(AID aid, int keep) where AID : Enum
    {
        var def = ActionDefinitions.Instance.Spell(aid)!;
        var spare = def.MaxChargesAtLevel(Player.Level) - keep - 1;
        return spare >= 0 && def.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) <= spare * def.Cooldown + 0.6f;
    }
    protected float CastTime(ActionID action) => HasSwiftcast ? 0 : ActionDefinitions.Instance[action]?.CastTime ?? 0;

    private static readonly Dictionary<uint, int> MPCosts = [];
    protected static int MPCost(ActionID action)
    {
        if (action.Type != ActionType.Spell)
            return 0;
        if (!MPCosts.TryGetValue(action.ID, out var cost))
            MPCosts[action.ID] = cost = Service.LuminaRow<Lumina.Excel.Sheets.Action>(action.ID) is { PrimaryCostType: 3 } row ? row.PrimaryCostValue * 100 : 0;
        return cost;
    }
    protected bool HasMP(ActionID action) => Player.FindStatus(WHM.SID.ThinAir) != null || Player.HPMP.CurMP >= MPCost(action);
    protected static bool IsEnemy(Actor? target) => target is { IsAlly: false, IsDeadOrDestroyed: false, IsTargetable: true };
    protected static bool ValidTarget(ActionID action, Actor? target)
    {
        var allowed = ActionDefinitions.Instance[action]?.AllowedTargets ?? ActionTargets.All;
        var allyFlags = ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly | ActionTargets.Dead;
        return target == null ? allowed.HasFlag(ActionTargets.Self) : target.IsAlly ? (allowed & allyFlags) != 0 : allowed.HasFlag(ActionTargets.Hostile) && IsEnemy(target);
    }
    protected void ExecuteOn(in StrategyValues.OptionRef opt, ActionID action, Actor? target, Angle? facingAngle = null)
    {
        if (HasMP(action) && ValidTarget(action, target))
            Hints.ActionsToExecute.Push(action, target, opt.Priority(), opt.Value.ExpireIn, castTime: CastTime(action), facingAngle: facingAngle);
    }
    protected void ExecuteOn<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? target) where AID : Enum
        => ExecuteOn(opt, ActionID.MakeSpell(aid), target);
    protected void ExecuteAction(in StrategyValues.OptionRef opt, ActionID action, Actor? target, Angle? facingAngle = null)
        => ExecuteOn(opt, action, ResolveTarget(opt.Value) ?? target, facingAngle);
    protected void ExecuteAction<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? target, Angle? facingAngle = null) where AID : Enum
        => ExecuteAction(opt, ActionID.MakeSpell(aid), target, facingAngle);

    protected void ExecuteArea<AID>(in StrategyValues.OptionRef opt, AID aid) where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        if (HasMP(action))
            Hints.ActionsToExecute.Push(action, null, opt.Priority(), opt.Value.ExpireIn, castTime: CastTime(action), targetPos: ResolveTargetLocation(opt.Value).ToVec3(Player.PosRot.Y));
    }

    protected void ExecuteSimple<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? defaultTarget) where AID : Enum
    {
        if (opt.As<SimpleOption>() == SimpleOption.Use)
            ExecuteAction(opt, aid, defaultTarget);
    }

    protected bool InDashRange(Actor? target, float minDistance, float maxDistance)
    {
        var distance = target != null ? Player.DistanceToHitbox(target) : 0;
        return target != null && target != Player && distance > minDistance && distance <= maxDistance;
    }
    protected void ExecuteDash<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? target, float minDistance) where AID : Enum
    {
        var dashTarget = ResolveTarget(opt.Value) ?? target;
        var range = ActionDefinitions.Instance.Spell(aid)!.Range;
        if (InDashRange(dashTarget, minDistance, range) && HasCharges(aid, 0))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), dashTarget, opt.Priority(), opt.Value.ExpireIn);
    }

    protected int LBLevelToExecute(LBOption option)
    {
        var level = World.Party.LimitBreakLevel;
        var condition = option switch
        {
            LBOption.LB3 => level == 3,
            LBOption.LB2 => level >= 2,
            LBOption.LB1 => level >= 1,
            LBOption.LB2Only => level == 2,
            LBOption.LB1Only => level == 1,
            LBOption.LB12 => level is 1 or 2,
            _ => false
        };
        return condition ? level : 0;
    }
    protected void ExecuteLimitBreak<AID>(in StrategyValues.OptionRef lb, AID lb1, AID lb2, ActionID lb3, Actor? target, bool area = false) where AID : Enum
    {
        var level = LBLevelToExecute(lb.As<LBOption>());
        var action = level == 3 ? lb3 : ActionID.MakeSpell(level == 2 ? lb2 : lb1);
        var lbTarget = area ? null : ResolveTarget(lb.Value) ?? target;
        if (level > 0 && (area || ValidTarget(action, lbTarget)))
            Hints.ActionsToExecute.Push(action, lbTarget, lb.Priority(), lb.Value.ExpireIn, castTime: ActionDefinitions.Instance[action]?.CastTime ?? 0, targetPos: area ? ResolveTargetLocation(lb.Value).ToVec3(Player.PosRot.Y) : default);
    }
}
