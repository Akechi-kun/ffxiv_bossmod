namespace BossMod.Autorotation;

public sealed class ClassBLUUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { Bristle = SharedTrack.Count, WhiteWind, IceSpikes, OffGuard, Transfusion, Diamondback, MightyGuard, ToadOil, MoonFlute, PeculiarLight, VeilOfTheWhorl, EerieSoundwave, PomCure, Gobskin, Avail, FrogLegs, Whistle, Cactguard, AngelWhisper, Exuviation, ColdFog, Stotram, AngelsSnack, ChelonianGate, BasicInstinct, DragonForce, Schiltron, Rehydration, ForceField }
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BLU.AID.None);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLU", UtilityDescription + "\nBlue Mage has many utility actions; use the Column Visibility tab to hide the ones you do not need.", UtilityCategory, "Akechi", RotationModuleQuality.WIP, BitMask.Build((int)Class.BLU), 80);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Bristle, "Bristle", "Bristle", 100, BLU.AID.Bristle, 30);
        DefineSimpleConfig(res, Track.WhiteWind, "White Wind", "White Wind", 100, BLU.AID.WhiteWind);
        DefineSimpleConfig(res, Track.IceSpikes, "Ice Spikes", "Ice Spikes", 50, BLU.AID.IceSpikes, 15);
        DefineSimpleConfig(res, Track.OffGuard, "Off Guard", "Off-guard", 25, BLU.AID.OffGuard, 15);
        DefineSimpleConfig(res, Track.Transfusion, "Transfusion", "Transfusion", 10, BLU.AID.Transfusion);
        DefineSimpleConfig(res, Track.Diamondback, "Diamondback", "Diamondback", 100, BLU.AID.Diamondback, 10);
        DefineSimpleConfig(res, Track.MightyGuard, "Mighty Guard", "Mighty Guard", 10, BLU.AID.MightyGuard);
        DefineSimpleConfig(res, Track.ToadOil, "Toad Oil", "Toad Oil", 10, BLU.AID.ToadOil, 180);
        DefineSimpleConfig(res, Track.MoonFlute, "Moon Flute", "Moon Flute", 100, BLU.AID.MoonFlute, 15);
        DefineSimpleConfig(res, Track.PeculiarLight, "Peculiar Light", "Peculiar Light", 25, BLU.AID.PeculiarLight, 15);
        DefineSimpleConfig(res, Track.VeilOfTheWhorl, "Veil Of The Whorl", "Veil of the Whorl", 100, BLU.AID.VeilOfTheWhorl, 30);
        DefineSimpleConfig(res, Track.EerieSoundwave, "Eerie Soundwave", "Eerie Soundwave", 100, BLU.AID.EerieSoundwave);
        DefineSimpleConfig(res, Track.PomCure, "Pom Cure", "Pom Cure", 100, BLU.AID.PomCure);
        DefineSimpleConfig(res, Track.Gobskin, "Gobskin", "Gobskin", 100, BLU.AID.Gobskin, 30);
        DefineSimpleConfig(res, Track.Avail, "Avail", "Avail", 100, BLU.AID.Avail, 12);
        DefineSimpleConfig(res, Track.FrogLegs, "Frog Legs", "Frog Legs", 10, BLU.AID.FrogLegs);
        DefineSimpleConfig(res, Track.Whistle, "Whistle", "Whistle", 100, BLU.AID.Whistle, 30);
        DefineSimpleConfig(res, Track.Cactguard, "Cactguard", "Cactguard", 100, BLU.AID.Cactguard, 6);
        DefineSimpleConfig(res, Track.AngelWhisper, "Angel Whisper", "Angel Whisper", 100, BLU.AID.AngelWhisper);
        DefineSimpleConfig(res, Track.Exuviation, "Exuviation", "Exuviation", 100, BLU.AID.Exuviation);
        DefineSimpleConfig(res, Track.ColdFog, "Cold Fog", "Cold Fog", 50, BLU.AID.ColdFog, 5);
        DefineSimpleConfig(res, Track.Stotram, "Stotram (Heal)", "Stotram", 100, BLU.AID.StotramHeal);
        DefineSimpleConfig(res, Track.AngelsSnack, "Angels Snack", "Angel's Snack", 100, BLU.AID.AngelsSnack, 15);
        DefineSimpleConfig(res, Track.ChelonianGate, "Chelonian Gate", "Chelonian Gate", 100, BLU.AID.ChelonianGate, 10);
        DefineSimpleConfig(res, Track.BasicInstinct, "Basic Instinct", "Basic Instinct", 10, BLU.AID.BasicInstinct);
        DefineSimpleConfig(res, Track.DragonForce, "Dragon Force", "Dragon Force", 100, BLU.AID.DragonForce, 15);
        DefineSimpleConfig(res, Track.Schiltron, "Schiltron", "Schiltron", 100, BLU.AID.Schiltron, 15);
        DefineSimpleConfig(res, Track.Rehydration, "Rehydration", "Rehydration", 100, BLU.AID.Rehydration);
        DefineSimpleConfig(res, Track.ForceField, "Force Field", "Force Field", 100, BLU.AID.ForceField, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        var ally = primaryTarget is { IsAlly: true } && primaryTarget != Player ? primaryTarget : null;
        ExecuteSpell(strategy.Option(Track.Bristle), BLU.AID.Bristle, Player);
        ExecuteSpell(strategy.Option(Track.WhiteWind), BLU.AID.WhiteWind, Player);
        ExecuteSpell(strategy.Option(Track.IceSpikes), BLU.AID.IceSpikes, Player);
        ExecuteSpell(strategy.Option(Track.OffGuard), BLU.AID.OffGuard, primaryTarget);
        ExecuteSpell(strategy.Option(Track.Transfusion), BLU.AID.Transfusion, ally);
        ExecuteSpell(strategy.Option(Track.Diamondback), BLU.AID.Diamondback, Player);
        if (!HasStatus(Player, BLU.SID.MightyGuard))
            ExecuteSpell(strategy.Option(Track.MightyGuard), BLU.AID.MightyGuard, Player);
        ExecuteSpell(strategy.Option(Track.ToadOil), BLU.AID.ToadOil, Player);
        ExecuteSpell(strategy.Option(Track.MoonFlute), BLU.AID.MoonFlute, Player);
        ExecuteSpell(strategy.Option(Track.PeculiarLight), BLU.AID.PeculiarLight, Player);
        ExecuteSpell(strategy.Option(Track.VeilOfTheWhorl), BLU.AID.VeilOfTheWhorl, Player);
        ExecuteSpell(strategy.Option(Track.EerieSoundwave), BLU.AID.EerieSoundwave, Player);
        ExecuteSpell(strategy.Option(Track.PomCure), BLU.AID.PomCure, FriendlyTarget(primaryTarget));
        ExecuteSpell(strategy.Option(Track.Gobskin), BLU.AID.Gobskin, Player);
        ExecuteSpell(strategy.Option(Track.Avail), BLU.AID.Avail, PartnerTarget(primaryTarget));
        ExecuteSpell(strategy.Option(Track.FrogLegs), BLU.AID.FrogLegs, Player);
        ExecuteSpell(strategy.Option(Track.Whistle), BLU.AID.Whistle, Player);
        ExecuteSpell(strategy.Option(Track.Cactguard), BLU.AID.Cactguard, PartnerTarget(primaryTarget));
        ExecuteSpell(strategy.Option(Track.AngelWhisper), BLU.AID.AngelWhisper, ally);
        ExecuteSpell(strategy.Option(Track.Exuviation), BLU.AID.Exuviation, Player);
        ExecuteSpell(strategy.Option(Track.ColdFog), BLU.AID.ColdFog, Player);
        ExecuteSpell(strategy.Option(Track.Stotram), BLU.AID.StotramHeal, Player, BLU.AID.StotramDamage);
        ExecuteSpell(strategy.Option(Track.AngelsSnack), BLU.AID.AngelsSnack, Player);
        ExecuteSpell(strategy.Option(Track.ChelonianGate), BLU.AID.ChelonianGate, Player);
        if (!HasStatus(Player, BLU.SID.BasicInstinct))
            ExecuteSpell(strategy.Option(Track.BasicInstinct), BLU.AID.BasicInstinct, Player);
        ExecuteSpell(strategy.Option(Track.DragonForce), BLU.AID.DragonForce, Player);
        ExecuteSpell(strategy.Option(Track.Schiltron), BLU.AID.Schiltron, Player);
        ExecuteSpell(strategy.Option(Track.Rehydration), BLU.AID.Rehydration, Player);
        ExecuteSpell(strategy.Option(Track.ForceField), BLU.AID.ForceField, Player);
    }

    private void ExecuteSpell(in StrategyValues.OptionRef opt, BLU.AID aid, Actor? target, BLU.AID spellbookAid = BLU.AID.None)
    {
        var spell = spellbookAid == BLU.AID.None ? aid : spellbookAid;
        if (World.Client.BlueMageSpells.Contains((uint)spell))
            ExecuteSimple(opt, aid, target);
    }
}
