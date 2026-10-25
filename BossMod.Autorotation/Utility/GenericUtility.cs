namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum SimpleOption { None, Use }
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 }
    public enum SwiftcastOption { None, Use, UseEx }
    public enum DashOption { None, Opener, GapClose, Five, Ten, GapCloseHold1 }

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
        return def.Define(expectedIndex).As<LBOption>("LB", "Limit Break")
            .AddOption(LBOption.None, NoUse)
            .AddOption(LBOption.LB3, "Use LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2, "Use LB2 or LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1, "Use any LB if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2Only, "Use LB2 if available, but not LB3", 0, effectLB2, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1Only, "Use LB1 if available, but not LB2 or LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB12, "Use LB1 or LB2 if available, but not LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh);
    }

    protected static void DefineSwiftcast<Index>(RotationModuleDefinition def, Index expectedIndex, string displayName) where Index : Enum
    {
        def.Define(expectedIndex).As<SwiftcastOption>("Swiftcast", displayName, 20)
            .AddOption(SwiftcastOption.None, NoUse)
            .AddOption(SwiftcastOption.Use, "Use Swiftcast (60s cooldown)", 60, 10, ActionTargets.Self, 18, 93)
            .AddOption(SwiftcastOption.UseEx, "Use Swiftcast (40s cooldown)", 40, 10, ActionTargets.Self, 94)
            .AddAssociatedActions(ClassShared.AID.Swiftcast);
    }

    protected static void DefineDash<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, AID aid, ActionTargets targets, string gapCloseDescription = "Use as a gap closer when outside melee range", int hold1MinLevel = 0)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var cooldown = adefs[action]!.Cooldown;
        var minLevel = adefs.ActionMinLevel(action);
        var dash = def.Define(expectedIndex).As<DashOption>(internalName, displayName, 20)
            .AddOption(DashOption.None, NoUse)
            .AddOption(DashOption.Opener, "Use as a gap closer at the start of combat", cooldown, 0, targets, minLevel)
            .AddOption(DashOption.GapClose, gapCloseDescription, cooldown, 0, targets, minLevel)
            .AddOption(DashOption.Five, "Use when 5y or further from target", cooldown, 0, targets, minLevel)
            .AddOption(DashOption.Ten, "Use when 10y or further from target", cooldown, 0, targets, minLevel)
            .AddAssociatedActions(aid);
        if (hold1MinLevel > 0)
            dash.AddOption(DashOption.GapCloseHold1, $"{gapCloseDescription}; conserve one charge", cooldown, 0, targets, hold1MinLevel);
    }

    protected Actor? CoTank() => ActionDefinitions.FindCoTank(World, Player);
    protected Actor FriendlyTarget(Actor? primaryTarget) => primaryTarget is { IsAlly: true } ? primaryTarget : Player;
    protected Actor? PartnerTarget(Actor? primaryTarget) => primaryTarget is { IsAlly: true } && primaryTarget != Player ? primaryTarget : CoTank();
    protected Actor? EsunaTarget(Actor? primaryTarget) => primaryTarget is { IsAlly: true } ? primaryTarget : ActionDefinitions.FindEsunaTarget(World);

    protected bool HasStatus<SID>(Actor? target, SID sid) where SID : Enum => target?.FindStatus(sid) != null;
    protected bool HasSwiftcast => HasStatus(Player, ClassShared.SID.Swiftcast);
    protected bool HasCharges<AID>(AID aid, int keep) where AID : Enum
    {
        var def = ActionDefinitions.Instance.Spell(aid)!;
        var spare = def.MaxChargesAtLevel(Player.Level) - keep - 1;
        return spare >= 0 && def.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) <= spare * def.Cooldown + 0.6f;
    }
    protected float CastTime(ActionID action)
    {
        var castTime = ActionDefinitions.Instance[action]?.CastTime ?? 0;
        if (castTime == 0 || HasSwiftcast || HasStatus(Player, BLM.SID.Triplecast) || HasStatus(Player, RDM.SID.Dualcast))
            return 0;
        return HasStatus(Player, AST.SID.Lightspeed) ? Math.Max(castTime - 2.5f, 0) : castTime;
    }

    private static readonly Dictionary<uint, int> MPCosts = [];
    internal static int MPCost(ActionID action)
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

    protected bool IsFirstGCD => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;
    protected bool InDashRange(Actor? target, float minDistance, float maxDistance)
    {
        if (target == null || target == Player)
            return false;
        var distance = Player.DistanceToHitbox(target);
        return distance > minDistance && distance <= maxDistance;
    }
    protected void ExecuteDash<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? target, float minDistance, int keepCharges = 0) where AID : Enum
    {
        var dashTarget = ResolveTarget(opt.Value) ?? target;
        var range = ActionDefinitions.Instance.Spell(aid)!.Range;
        if (InDashRange(dashTarget, minDistance, range) && ValidTarget(ActionID.MakeSpell(aid), dashTarget) && HasCharges(aid, keepCharges))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), dashTarget, opt.Priority(), opt.Value.ExpireIn);
    }
    protected void ExecuteDash<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? target) where AID : Enum
    {
        var option = opt.As<DashOption>();
        var minDistance = option switch
        {
            DashOption.Opener or DashOption.GapClose or DashOption.GapCloseHold1 => 3,
            DashOption.Five => 5,
            DashOption.Ten => 10,
            _ => -1
        };
        if (minDistance >= 0 && (option != DashOption.Opener || IsFirstGCD))
            ExecuteDash(opt, aid, target, minDistance, option == DashOption.GapCloseHold1 ? 1 : 0);
    }

    protected void ExecuteSwiftcast(in StrategyValues.OptionRef swift)
    {
        if (swift.As<SwiftcastOption>() != SwiftcastOption.None && !HasSwiftcast)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, swift.Priority(), swift.Value.ExpireIn);
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
