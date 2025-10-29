﻿namespace BossMod.Autorotation.MiscAI;
public sealed class FateUtils(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Handin, Collect }
    public enum Flag { Enabled, Disabled }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("FATE helper", "Utilities for completing FATEs", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, 1);

        res.Define(Track.Handin).As<Flag>("Hand-in", "Automatically hand in FATE items").AddOption(Flag.Enabled).AddOption(Flag.Disabled);
        res.Define(Track.Collect).As<Flag>("Collect", "Try to pick up FATE items").AddOption(Flag.Enabled).AddOption(Flag.Disabled);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Option(Track.Handin).As<Flag>() != Flag.Enabled)
            return;

        if (!Utils.IsPlayerSyncedToFate(World))
            return;

        var fateID = World.Client.ActiveFate.ID;

        var item = Utils.GetFateItem(fateID);
        if (item == 0)
            return;

        var itemsHeld = (int)World.Client.GetItemQuantity(item);
        var itemsTurnin = World.Client.ActiveFate.HandInCount;
        var itemsTotal = itemsTurnin + itemsHeld;

        if (itemsTurnin < 10)
        {
            if (itemsTotal >= 10)
                Hints.InteractWithTarget = World.Actors.Find(World.Client.ActiveFate.ObjectiveNpc);
            else if (strategy.Option(Track.Collect).As<Flag>() == Flag.Enabled && !Player.InCombat)
                Hints.InteractWithTarget = World.Actors.Where(a => a.FateID == fateID && a.IsTargetable && a.Type == ActorType.EventObj).MinBy(Player.DistanceToHitbox);
        }
    }
}
