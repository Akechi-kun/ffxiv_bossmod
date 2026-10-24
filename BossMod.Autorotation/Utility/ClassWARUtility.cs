namespace BossMod.Autorotation;

public sealed class ClassWARUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Thrill = SharedTrack.Count, Vengeance, Holmgang, Bloodwhetting, Equilibrium, ShakeItOff }
    public enum BWOption { None, Bloodwhetting, RawIntuition, NascentFlash }
    public enum VengOption { None, Vengeance, Damnation }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(WAR.AID.LandWaker);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(WAR.AID.Defiance);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(WAR.AID.ReleaseDefiance);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WAR", UtilityDescription, UtilityCategory, "veyn", RotationModuleQuality.Ok, BitMask.Build((int)Class.WAR), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.Thrill, "ThrillOfBattle", "Thrill", 450, WAR.AID.ThrillOfBattle, 10);

        res.Define(Track.Vengeance).As<VengOption>("Vengeance", "Veng", 550)
            .AddOption(VengOption.None, NoUse)
            .AddOption(VengOption.Vengeance, "Use Vengeance", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(VengOption.Damnation, "Use Damnation", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(WAR.AID.Vengeance, WAR.AID.Damnation);

        DefineSimpleConfig(res, Track.Holmgang, "Holmgang", "", 400, WAR.AID.Holmgang, 10);

        res.Define(Track.Bloodwhetting).As<BWOption>("BW", uiPriority: 350)
            .AddOption(BWOption.None, NoUse)
            .AddOption(BWOption.Bloodwhetting, "Use Bloodwhetting", 25, 4, ActionTargets.Self, 82)
            .AddOption(BWOption.RawIntuition, "Use Raw Intuition", 25, 6, ActionTargets.Self, 56, 81)
            .AddOption(BWOption.NascentFlash, "Use Nascent Flash on a party member (co-tank by default)", 25, 4, ActionTargets.Party, 76)
            .AddAssociatedActions(WAR.AID.Bloodwhetting, WAR.AID.RawIntuition, WAR.AID.NascentFlash);

        DefineSimpleConfig(res, Track.Equilibrium, "Equilibrium", "Equi", 320, WAR.AID.Equilibrium, 15);
        DefineSimpleConfig(res, Track.ShakeItOff, "ShakeItOff", "SIO", 220, WAR.AID.ShakeItOff, 30);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)WAR.SID.Defiance, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Thrill), WAR.AID.ThrillOfBattle, Player);
        ExecuteSimple(strategy.Option(Track.Holmgang), WAR.AID.Holmgang, Player);
        ExecuteSimple(strategy.Option(Track.Equilibrium), WAR.AID.Equilibrium, Player);
        ExecuteSimple(strategy.Option(Track.ShakeItOff), WAR.AID.ShakeItOff, Player);
        ExecuteVengeance(strategy.Option(Track.Vengeance));
        ExecuteBloodwhetting(strategy.Option(Track.Bloodwhetting), primaryTarget);
    }

    private void ExecuteVengeance(in StrategyValues.OptionRef veng)
    {
        var action = veng.As<VengOption>() switch
        {
            VengOption.Vengeance => WAR.AID.Vengeance,
            VengOption.Damnation => WAR.AID.Damnation,
            _ => default
        };
        if (action != default)
            ExecuteAction(veng, action, Player);
    }
    private void ExecuteBloodwhetting(in StrategyValues.OptionRef bw, Actor? primaryTarget)
    {
        var action = bw.As<BWOption>() switch
        {
            BWOption.Bloodwhetting => WAR.AID.Bloodwhetting,
            BWOption.RawIntuition => WAR.AID.RawIntuition,
            BWOption.NascentFlash => WAR.AID.NascentFlash,
            _ => default
        };
        if (action != default)
            ExecuteAction(bw, action, action == WAR.AID.NascentFlash ? PartnerTarget(primaryTarget) : Player);
    }
}
