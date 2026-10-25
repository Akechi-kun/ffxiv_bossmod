namespace BossMod.Autorotation.Utility;

public sealed class RolePvPUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Elixir, Recuperate, Guard, GuardEnd, Purify, Sprint }
    public enum ElixirStrategy { Far, Close, Forbid }
    public enum ThresholdStrategy { Seventy, Fifty, Thirty, Forbid }
    public enum GuardStrategy { Auto, Two, Three, Four, Seventy, Fifty, Thirty, Forbid }
    public enum GuardEndStrategy { Normal, Point25, Point5, One }
    public enum DefensiveStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PvP", "Automatic use of the shared PvP actions: Guard, Recuperate, Purify, Sprint and Elixir.", "PvP", "Akechi", RotationModuleQuality.Ok, BitMask.Build(
                Class.PLD, Class.WAR, Class.DRK, Class.GNB,
                Class.WHM, Class.SCH, Class.AST, Class.SGE,
                Class.MNK, Class.DRG, Class.NIN, Class.SAM, Class.RPR, Class.VPR,
                Class.BRD, Class.MCH, Class.DNC,
                Class.BLM, Class.SMN, Class.RDM, Class.PCT), 100, 30, PvP: PvPCompatibility.PvPOnly);

        res.Define(Track.Elixir).As<ElixirStrategy>("Elixir", uiPriority: 150)
            .AddOption(ElixirStrategy.Far, "Use Elixir when HP or MP is missing and no enemy is within 50y")
            .AddOption(ElixirStrategy.Close, "Use Elixir when HP or MP is missing and no enemy is within 30y")
            .AddOption(ElixirStrategy.Forbid, "Never use Elixir")
            .AddAssociatedActions(ClassShared.AID.ElixirPvP);

        res.Define(Track.Recuperate).As<ThresholdStrategy>("Recuperate", uiPriority: 150)
            .AddOption(ThresholdStrategy.Seventy, "Use Recuperate below 70% HP")
            .AddOption(ThresholdStrategy.Fifty, "Use Recuperate below 50% HP")
            .AddOption(ThresholdStrategy.Thirty, "Use Recuperate below 30% HP")
            .AddOption(ThresholdStrategy.Forbid, "Never use Recuperate")
            .AddAssociatedActions(ClassShared.AID.RecuperatePvP);

        res.Define(Track.Guard).As<GuardStrategy>("Guard", uiPriority: 150)
            .AddOption(GuardStrategy.Auto, "Use Guard below 75% HP while two or more enemies target you, or below 33% HP")
            .AddOption(GuardStrategy.Two, "Use Guard when damaged and two or more enemies target you")
            .AddOption(GuardStrategy.Three, "Use Guard when damaged and three or more enemies target you")
            .AddOption(GuardStrategy.Four, "Use Guard when damaged and four or more enemies target you")
            .AddOption(GuardStrategy.Seventy, "Use Guard below 70% HP")
            .AddOption(GuardStrategy.Fifty, "Use Guard below 50% HP")
            .AddOption(GuardStrategy.Thirty, "Use Guard below 30% HP")
            .AddOption(GuardStrategy.Forbid, "Never use Guard")
            .AddAssociatedActions(ClassShared.AID.GuardPvP);

        res.Define(Track.GuardEnd).As<GuardEndStrategy>("Guard End", uiPriority: 150)
            .AddOption(GuardEndStrategy.Normal, "Let Guard run its full duration")
            .AddOption(GuardEndStrategy.Point25, "Resume actions 0.25s before Guard ends")
            .AddOption(GuardEndStrategy.Point5, "Resume actions 0.5s before Guard ends")
            .AddOption(GuardEndStrategy.One, "Resume actions 1s before Guard ends")
            .AddAssociatedActions(ClassShared.AID.GuardPvP);

        res.Define(Track.Purify).As<DefensiveStrategy>("Purify", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Use Purify when afflicted by a removable debuff")
            .AddOption(DefensiveStrategy.Forbid, "Never use Purify")
            .AddAssociatedActions(ClassShared.AID.PurifyPvP);

        res.Define(Track.Sprint).As<DefensiveStrategy>("Sprint", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Use Sprint when no enemy is within 30y")
            .AddOption(DefensiveStrategy.Forbid, "Never use Sprint")
            .AddAssociatedActions(ClassShared.AID.SprintPvP);

        return res;
    }

    private float HPRatio => Player.PendingHPRatio;
    private bool Damaged => HPRatio is > 0 and < 1;
    private bool Below(float ratio) => HPRatio > 0 && HPRatio < ratio;
    private bool IsReady(ClassShared.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining <= 0.2f;

    private int EnemiesTargetingPlayer()
    {
        var count = 0;
        foreach (var enemy in Hints.PotentialTargets)
            if (!enemy.Actor.IsDeadOrDestroyed && enemy.Actor.TargetID == Player.InstanceID)
                ++count;
        return count;
    }
    private bool EnemiesWithin(float range)
    {
        foreach (var enemy in Hints.PotentialTargets)
            if (!enemy.Actor.IsDeadOrDestroyed && enemy.Actor.DistanceToHitbox(Player) <= range)
                return true;
        return false;
    }
    private bool HasPurifiableDebuff => Player.FindStatus(ClassShared.SID.StunPvP) != null
        || Player.FindStatus(ClassShared.SID.HeavyPvP) != null
        || Player.FindStatus(ClassShared.SID.BindPvP) != null
        || Player.FindStatus(ClassShared.SID.SilencePvP) != null
        || Player.FindStatus(ClassShared.SID.DeepFreezePvP) != null
        || Player.FindStatus(WHM.SID.MiracleOfNaturePvP) != null;

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var guardLeft = SelfStatusLeft(ClassShared.SID.GuardPvP);
        var guardActive = strategy.Option(Track.GuardEnd).As<GuardEndStrategy>() switch
        {
            GuardEndStrategy.Point25 => guardLeft > 0.25f,
            GuardEndStrategy.Point5 => guardLeft > 0.5f,
            GuardEndStrategy.One => guardLeft > 1f,
            _ => Player.FindStatus(ClassShared.SID.GuardPvP) != null
        };
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || guardActive)
            return;

        ExecuteGuard(strategy.Option(Track.Guard).As<GuardStrategy>());
        ExecuteRecuperate(strategy.Option(Track.Recuperate).As<ThresholdStrategy>());
        ExecutePurify(strategy.Option(Track.Purify).As<DefensiveStrategy>());
        ExecuteSprint(strategy.Option(Track.Sprint).As<DefensiveStrategy>());
        ExecuteElixir(strategy.Option(Track.Elixir).As<ElixirStrategy>());
    }

    private void ExecuteGuard(GuardStrategy option)
    {
        var condition = option switch
        {
            GuardStrategy.Auto => (Below(0.75f) && EnemiesTargetingPlayer() >= 2) || Below(0.33f),
            GuardStrategy.Two => Damaged && EnemiesTargetingPlayer() >= 2,
            GuardStrategy.Three => Damaged && EnemiesTargetingPlayer() >= 3,
            GuardStrategy.Four => Damaged && EnemiesTargetingPlayer() >= 4,
            GuardStrategy.Seventy => Below(0.7f),
            GuardStrategy.Fifty => Below(0.5f),
            GuardStrategy.Thirty => Below(0.3f),
            _ => false
        };
        if (condition && IsReady(ClassShared.AID.GuardPvP))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.GuardPvP), Player, ActionQueue.Priority.VeryHigh + 2);
    }
    private void ExecuteRecuperate(ThresholdStrategy option)
    {
        var condition = option switch
        {
            ThresholdStrategy.Seventy => Below(0.7f),
            ThresholdStrategy.Fifty => Below(0.5f),
            ThresholdStrategy.Thirty => Below(0.3f),
            _ => false
        };
        if (condition && Player.HPMP.CurMP >= 2500)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.RecuperatePvP), Player, ActionQueue.Priority.VeryHigh + 1);
    }
    private void ExecutePurify(DefensiveStrategy option)
    {
        if (option == DefensiveStrategy.Allow && HasPurifiableDebuff && IsReady(ClassShared.AID.PurifyPvP))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.PurifyPvP), Player, ActionQueue.Priority.VeryHigh);
    }
    private void ExecuteSprint(DefensiveStrategy option)
    {
        if (option == DefensiveStrategy.Allow && Player.FindStatus(ClassShared.SID.SprintPvP) == null && IsReady(ClassShared.AID.SprintPvP) && !EnemiesWithin(32))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SprintPvP), Player, ActionQueue.Priority.High);
    }
    private void ExecuteElixir(ElixirStrategy option)
    {
        var missing = Player.HPMP.CurHP != Player.HPMP.MaxHP || Player.HPMP.CurMP != Player.HPMP.MaxMP;
        var condition = option switch
        {
            ElixirStrategy.Close => !EnemiesWithin(32),
            ElixirStrategy.Far => !EnemiesWithin(52),
            _ => false
        };
        if (condition && missing)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ElixirPvP), Player, ActionQueue.Priority.High);
    }
}
