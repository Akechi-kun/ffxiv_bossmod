namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum SimpleOption { None, Use }
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 }
    public enum SwiftcastOption { None, Use, UseEx }
    public enum DashOption { None, Opener, GapClose, Five, Ten }

    protected const string UtilityDescription = "Cooldown Planner support for utility actions.\nThis is not a rotation preset; utility modules are meant for cooldown planning only.";
    protected const string UtilityCategory = "Utility for planner";
    protected const string NoUse = "Do not use automatically";

    protected static void DefineSimple<Index, AID>(RotationModuleDefinition def, Index track, string internalName, string displayName, int uiPriority, AID aid, float effect = 0, float defaultPriority = 3000)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        var data = ActionDefinitions.Instance[action]!;
        def.Define(track).As<SimpleOption>(internalName, displayName, uiPriority)
            .AddOption(SimpleOption.None, NoUse)
            .AddOption(SimpleOption.Use, $"Use {action.Name()}", data.Cooldown, effect, data.AllowedTargets, ActionDefinitions.Instance.ActionMinLevel(action), defaultPriority: defaultPriority)
            .AddAssociatedActions(aid);
    }

    protected static RotationModuleDefinition.ConfigRef<LBOption> DefineLimitBreak<Index>(RotationModuleDefinition def, Index track, ActionTargets allowedTargets, float effectLB1 = 0, float effectLB2 = 0, float effectLB3 = 0) where Index : Enum
    {
        return def.Define(track).As<LBOption>("LB", "Limit Break")
            .AddOption(LBOption.None, NoUse)
            .AddOption(LBOption.LB3, "Use LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2, "Use LB2 or LB3 if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1, "Use any LB if available", 0, effectLB3, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB2Only, "Use LB2 if available, but not LB3", 0, effectLB2, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB1Only, "Use LB1 if available, but not LB2 or LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh)
            .AddOption(LBOption.LB12, "Use LB1 or LB2 if available, but not LB3", 0, effectLB1, allowedTargets, defaultPriority: ActionQueue.Priority.VeryHigh);
    }

    protected static void DefineSwiftcast<Index>(RotationModuleDefinition def, Index track, string displayName) where Index : Enum
    {
        def.Define(track).As<SwiftcastOption>("Swiftcast", displayName, 20)
            .AddOption(SwiftcastOption.None, NoUse)
            .AddOption(SwiftcastOption.Use, "Use Swiftcast (60s cooldown)", 60, 10, ActionTargets.Self, 18, 93)
            .AddOption(SwiftcastOption.UseEx, "Use Swiftcast (40s cooldown)", 40, 10, ActionTargets.Self, 94)
            .AddAssociatedActions(ClassShared.AID.Swiftcast);
    }

    protected static void DefineDash<Index, AID>(RotationModuleDefinition def, Index track, string internalName, string displayName, AID aid, ActionTargets targets, string gapCloseDescription = "Use as a gap closer when outside melee range")
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        var cooldown = ActionDefinitions.Instance[action]!.Cooldown;
        var minLevel = ActionDefinitions.Instance.ActionMinLevel(action);
        def.Define(track).As<DashOption>(internalName, displayName, 20)
            .AddOption(DashOption.None, NoUse)
            .AddOption(DashOption.Opener, "Use as a gap closer at the start of combat", cooldown, 0, targets, minLevel)
            .AddOption(DashOption.GapClose, gapCloseDescription, cooldown, 0, targets, minLevel)
            .AddOption(DashOption.Five, "Use when 5y or further from target", cooldown, 0, targets, minLevel)
            .AddOption(DashOption.Ten, "Use when 10y or further from target", cooldown, 0, targets, minLevel)
            .AddAssociatedActions(aid);
    }

    protected Actor? CoTank() => ActionDefinitions.FindCoTank(World, Player);
    protected Actor? OtherAlly(Actor? primaryTarget) => primaryTarget is { IsAlly: true } && primaryTarget != Player ? primaryTarget : null;
    protected Actor AllyOrSelf(Actor? primaryTarget) => primaryTarget is { IsAlly: true } ? primaryTarget : Player;
    protected Actor? AllyOrCoTank(Actor? primaryTarget) => OtherAlly(primaryTarget) ?? CoTank();
    protected Actor? AllyOrCleansable(Actor? primaryTarget) => primaryTarget is { IsAlly: true } ? primaryTarget : ActionDefinitions.FindEsunaTarget(World);

    protected static bool HasStatus<SID>(Actor? actor, SID sid) where SID : Enum => actor?.FindStatus(sid, DateTime.MaxValue) != null;
    protected static bool IsEnemy(Actor? actor) => actor is { IsAlly: false, IsDeadOrDestroyed: false, IsTargetable: true };
    protected bool HasMP<AID>(AID aid) where AID : Enum => HasMP(ActionID.MakeSpell(aid));
    protected bool CanWeave<AID>(AID aid, float animLockDelay) where AID : Enum
        => World.Client.AnimationLock + ActionDefinitions.Instance.Spell(aid)!.TotalDuration + animLockDelay <= GCD;

    protected void ExecuteSimple<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? fallbackTarget) where AID : Enum
    {
        if (opt.Value.Option != 0)
            ExecuteAction(opt, aid, fallbackTarget);
    }

    protected void ExecuteVariant<AID>(in StrategyValues.OptionRef opt, Actor? fallbackTarget, params ReadOnlySpan<AID> actionPerOption) where AID : Enum
    {
        var index = opt.Value.Option - 1;
        if (index >= 0 && index < actionPerOption.Length)
            ExecuteAction(opt, actionPerOption[index], fallbackTarget);
    }

    protected void ExecuteAction<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? fallbackTarget, Angle? facing = null) where AID : Enum
        => ExecuteOn(opt, aid, ResolveTarget(opt.Value) ?? fallbackTarget, facing);

    protected void ExecuteOn<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? target, Angle? facing = null) where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        if (HasMP(action) && IsValidTarget(action, target))
            Hints.ActionsToExecute.Push(action, target, opt.Priority(), opt.Value.ExpireIn, castTime: CastTime(action), facingAngle: facing);
    }

    protected void ExecuteArea<AID>(in StrategyValues.OptionRef opt, AID aid) where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        if (opt.Value.Option != 0 && HasMP(action))
            Hints.ActionsToExecute.Push(action, null, opt.Priority(), opt.Value.ExpireIn, castTime: CastTime(action), targetPos: ResolveTargetLocation(opt.Value).ToVec3(Player.PosRot.Y));
    }

    protected void ExecuteDash<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? fallbackTarget) where AID : Enum
    {
        var option = opt.As<DashOption>();
        var minDistance = option switch
        {
            DashOption.None => -1,
            DashOption.Opener => IsPullStart ? 3 : -1,
            DashOption.Five => 5,
            DashOption.Ten => 10,
            _ => 3
        };
        if (minDistance >= 0)
            ExecuteDashBeyond(opt, aid, fallbackTarget, minDistance);
    }

    protected void ExecuteDashBeyond<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? fallbackTarget, float minDistance) where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        var target = ResolveTarget(opt.Value) ?? fallbackTarget;
        var distance = target == null || target == Player ? -1 : Player.DistanceToHitbox(target);
        if (distance > minDistance && distance <= ActionDefinitions.Instance[action]!.Range && IsValidTarget(action, target))
            Hints.ActionsToExecute.Push(action, target, opt.Priority(), opt.Value.ExpireIn);
    }

    protected void ExecuteSwiftcast(in StrategyValues.OptionRef opt)
    {
        if (opt.Value.Option != 0 && !HasStatus(Player, ClassShared.SID.Swiftcast))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, opt.Priority(), opt.Value.ExpireIn);
    }

    protected void ExecuteLimitBreak<AID>(in StrategyValues.OptionRef opt, AID lb1, AID lb2, ActionID lb3, Actor? fallbackTarget, bool area = false) where AID : Enum
    {
        var level = World.Party.LimitBreakLevel;
        var allowed = opt.As<LBOption>() switch
        {
            LBOption.LB3 => level == 3,
            LBOption.LB2 => level >= 2,
            LBOption.LB1 => level >= 1,
            LBOption.LB2Only => level == 2,
            LBOption.LB1Only => level == 1,
            LBOption.LB12 => level is 1 or 2,
            _ => false
        };
        var action = level == 3 ? lb3 : ActionID.MakeSpell(level == 2 ? lb2 : lb1);
        var castTime = ActionDefinitions.Instance[action]?.CastTime ?? 0;
        var target = area ? null : ResolveTarget(opt.Value) ?? fallbackTarget;
        if (allowed && area)
            Hints.ActionsToExecute.Push(action, null, opt.Priority(), opt.Value.ExpireIn, castTime: castTime, targetPos: ResolveTargetLocation(opt.Value).ToVec3(Player.PosRot.Y));
        if (allowed && !area && IsValidTarget(action, target))
            Hints.ActionsToExecute.Push(action, target, opt.Priority(), opt.Value.ExpireIn, castTime: castTime);
    }

    private const ActionTargets AllyTargets = ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly | ActionTargets.Dead;
    private static readonly Dictionary<uint, int> MPCosts = [];

    private bool IsPullStart => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    private bool HasMP(ActionID action) => IsFreeCast(action) || Player.HPMP.CurMP >= MPCost(action);
    private bool IsFreeCast(ActionID action) => HasStatus(Player, WHM.SID.ThinAir) || HasStatus(Player, SCH.SID.Recitation) && action.ID is (uint)SCH.AID.Adloquium or (uint)SCH.AID.Concitation;
    private int MPCost(ActionID action) => Player.Class == Class.PLD && Player.Level >= 64 ? BaseMPCost(action) / 2 : BaseMPCost(action);

    private static int BaseMPCost(ActionID action)
    {
        if (action.Type != ActionType.Spell)
            return 0;
        if (!MPCosts.TryGetValue(action.ID, out var cost))
            MPCosts[action.ID] = cost = Service.LuminaRow<Lumina.Excel.Sheets.Action>(action.ID) is { PrimaryCostType: 3 or 70 } row ? row.PrimaryCostValue * 100 : 0;
        return cost;
    }

    private static bool IsValidTarget(ActionID action, Actor? target)
    {
        var allowed = ActionDefinitions.Instance[action]?.AllowedTargets ?? ActionTargets.All;
        if (target == null)
            return allowed.HasFlag(ActionTargets.Self);
        return target.IsAlly ? (allowed & AllyTargets) != 0 : allowed.HasFlag(ActionTargets.Hostile) && IsEnemy(target);
    }

    private float CastTime(ActionID action)
    {
        var castTime = ActionDefinitions.Instance[action]?.CastTime ?? 0;
        if (castTime == 0 || HasStatus(Player, ClassShared.SID.Swiftcast) || HasStatus(Player, BLM.SID.Triplecast) || HasStatus(Player, RDM.SID.Dualcast))
            return 0;
        return HasStatus(Player, AST.SID.Lightspeed) ? Math.Max(castTime - 2.5f, 0) : castTime;
    }
}
