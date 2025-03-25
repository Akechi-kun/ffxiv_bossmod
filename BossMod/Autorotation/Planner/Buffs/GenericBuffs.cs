namespace BossMod.Autorotation;

public abstract class GenericBuffs(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum BuffsStrategy { None, ASAP, AnyWeave, EarlyWeave, LateWeave }

    protected static void DefineBuffs<Index, AID>(RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100, bool enhanced = true)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        res.Define(track).As<BuffsStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(BuffsStrategy.None, "None", $"Do not use {action.Name()}", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.ASAP, "ASAP", $"Use {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.AnyWeave, "AnyWeave", $"Use {action.Name()} in next possible weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.EarlyWeave, "EarlyWeave", $"Use {action.Name()} in next possible early-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.LateWeave, "LateWeave", $"Use {action.Name()} in next possible late-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }
    public void ExecuteBuffs<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? defaultTarget, float castTime = 0)
        where AID : Enum
    {
        if (opt.As<BuffsStrategy>() != BuffsStrategy.None)
        {
            var cd = ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);
            if ((opt.As<BuffsStrategy>() == BuffsStrategy.ASAP && cd < 0.6f) ||
                (opt.As<BuffsStrategy>() == BuffsStrategy.AnyWeave && GCD is < 2.5f and > 0.6f && cd < 0.6f) ||
                (opt.As<BuffsStrategy>() == BuffsStrategy.EarlyWeave && GCD is < 2.5f and > 1.25f && cd < 0.6f) ||
                (opt.As<BuffsStrategy>() == BuffsStrategy.LateWeave && GCD is < 1.25f and > 0.6f && cd < 0.6f))
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(opt.Value) ?? defaultTarget, opt.Priority(), opt.Value.ExpireIn, castTime: castTime);
        }
    }

    protected static void DefinePots<Index>(RotationModuleDefinition res, Index track, ActionID pot, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
    where Index : Enum
    {
        res.Define(track).As<BuffsStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(BuffsStrategy.None, "None", "Do not use any tinctures", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.ASAP, "ASAP", "Use best tincture ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.AnyWeave, "AnyWeave", "Use best tincture in next possible weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.EarlyWeave, "EarlyWeave", "Use best tincture in next possible early-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(BuffsStrategy.LateWeave, "LateWeave", "Use best tincture in next possible late-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddAssociatedAction(pot);
    }

    public void ExecutePots(in StrategyValues.OptionRef opt)
    {
        if (opt.As<BuffsStrategy>() != BuffsStrategy.None)
        {
            if ((opt.As<BuffsStrategy>() == BuffsStrategy.ASAP) ||
                (opt.As<BuffsStrategy>() == BuffsStrategy.AnyWeave && GCD is < 2.5f and > 0.6f) ||
                (opt.As<BuffsStrategy>() == BuffsStrategy.EarlyWeave && GCD is < 2.5f and > 1.25f) ||
                (opt.As<BuffsStrategy>() == BuffsStrategy.LateWeave && GCD is < 1.25f and > 0.6f))
            {
                if (Player.Class is Class.WHM or Class.SCH or Class.AST or Class.SGE)
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, opt.Priority(), opt.Value.ExpireIn);
                if (Player.Class is Class.PLD or Class.WAR or Class.DRK or Class.GNB or Class.MNK or Class.DRG or Class.SAM or Class.RPR)
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, opt.Priority(), opt.Value.ExpireIn);
                if (Player.Class is Class.NIN or Class.VPR or Class.BRD or Class.MCH or Class.DNC)
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, opt.Priority(), opt.Value.ExpireIn);
                if (Player.Class is Class.BLM or Class.SMN or Class.RDM or Class.PCT)
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionInt, Player, opt.Priority(), opt.Value.ExpireIn);
            }
        }
    }
}
