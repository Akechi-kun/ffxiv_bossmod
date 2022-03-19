﻿using System;

namespace BossMod.Endwalker.ARanks.Storsie
{
    public enum OID : uint
    {
        Boss = 0x35DE,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        AspectEarth = 27354,
        AspectWind = 27355,
        AspectLightning = 27356,
        Whorlstorm = 27358,
        Defibrillate = 27359,
        EarthenAugur = 27360,
        FangsEnd = 27361,
        AspectEarthApply = 27870,
        AspectWindApply = 27871,
        AspectLightningApply = 27872,
    }

    public class Mechanics : BossModule.Component
    {
        private enum AspectType { None, Earth, Wind, Lightning }

        private BossModule _module;
        private AspectType _imminentAspect;
        private AOEShapeCone _earthenAugur = new(30, 3 * MathF.PI / 4);
        private AOEShapeDonut _whorlstorm = new(10, 40); // TODO: verify inner radius
        private AOEShapeCircle _defibrillate = new(22);

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (ActiveAOE()?.Check(actor.Position, _module.PrimaryActor) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule.GlobalHints hints)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.FangsEnd => "Tankbuster",
                AID.AspectEarth or AID.AspectWind or AID.AspectLightning => "Select AOE shape",
                AID.EarthenAugur or AID.Whorlstorm or AID.Defibrillate => "Avoidable AOE",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            ActiveAOE()?.Draw(arena, _module.PrimaryActor);
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor != _module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.AspectEarth: _imminentAspect = AspectType.Earth; break;
                case AID.AspectWind: _imminentAspect = AspectType.Wind; break;
                case AID.AspectLightning: _imminentAspect = AspectType.Lightning; break;
            }
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor != _module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.Whorlstorm:
                case AID.Defibrillate:
                case AID.EarthenAugur:
                    _imminentAspect = AspectType.None;
                    break;
            }
        }

        private AOEShape? ActiveAOE()
        {
            return _imminentAspect switch
            {
                AspectType.Earth => _earthenAugur,
                AspectType.Wind => _whorlstorm,
                AspectType.Lightning => _defibrillate,
                _ => null
            };
        }
    }

    public class Storsie : SimpleBossModule
    {
        public Storsie(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}