﻿namespace BossMod.Autorotation;

// the configuration part of the rotation module
// importantly, it defines constraints (supported classes and level ranges) and strategy configs (with their sets of possible options) used by the module to make its decisions
public sealed record class RotationModuleDefinition(string DisplayName, string Description, BitMask Classes, int MaxLevel, int MinLevel = 1)
{
    public readonly BitMask Classes = Classes;
    public readonly List<StrategyConfig> Configs = [];

    public StrategyConfig AddConfig<Index>(Index expectedIndex, StrategyConfig config) where Index : Enum
    {
        if (Configs.Count != (int)(object)expectedIndex)
            throw new ArgumentException($"Unexpected index for {config.InternalName}: expected {expectedIndex} ({(int)(object)expectedIndex}), got {Configs.Count}");
        Configs.Add(config);
        return config;
    }
}

// base class for rotation modules
// each rotation module should contain a `public static RotationModuleDefinition Definition()` function
// TODO: i don't think it should know about manager, rework this...
public abstract class RotationModule(RotationModuleManager manager, Actor player)
{
    public readonly RotationModuleManager Manager = manager;
    public readonly Actor Player = player;
    public BossModuleManager Bossmods => Manager.Bossmods;
    public WorldState World => Manager.Bossmods.WorldState;
    public AIHints Hints => Manager.Hints;

    // the main entry point of the module - given a set of strategy values, fill the queue with a set of actions to execute
    public abstract void Execute(ReadOnlySpan<StrategyValue> strategy, Actor? primaryTarget);

    public virtual string DescribeState() => "";

    // utility to resolve the target overrides; returns null on failure - in this case module is expected to run smart-targeting logic
    // expected usage is `ResolveTargetOverride(strategy) ?? CustomSmartTargetingLogic(...)`
    protected Actor? ResolveTargetOverride(in StrategyValue strategy) => Manager.ResolveTargetOverride(strategy);
}
