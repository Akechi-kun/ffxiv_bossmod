using Dalamud.Bindings.ImGui;

namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

class Meteorain(BossModule module) : Components.RaidwideCast(module, AID._Spell_Meteorain);
class NaughtGrowsCounter(BossModule module) : Components.CastCounterMulti(module, [AID._Weaponskill_NaughtGrows1, AID._Weaponskill_NaughtGrows2]);
class NaughtGrowsDonut(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_NaughtGrows2, new AOEShapeDonut(40, 60));
class NaughtGrowsCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_NaughtGrows1, 40);

// 21.34 -> 30.46
class ReturnToNothing(BossModule module) : Components.UntelegraphedBait(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_z5fd_loc01_t0a1:
                CurrentBaits.Add(new(default, BitMask.Build(Raid.FindSlot(targetID)), new AOEShapeRect(15, 3), WorldState.FutureTime(9.1f), stackSize: 4, count: 2));
                break;
            case IconID._Gen_Icon_z5fd_loc02_t0a1:
                CurrentBaits.Add(new(default, BitMask.Build(Raid.FindSlot(targetID)), new AOEShapeRect(15, 3), WorldState.FutureTime(9.1f), stackSize: 8, count: 1));
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // prevent any autorot dashes during this mechanic so we don't fuck up wild charge, i REALLY don't feel like writing proper hints
        if (CurrentBaits.Count > 0 && actor.Class.GetRole2() == Role2.Tank)
            hints.AddForbiddenZone(_ => true, DateTime.MaxValue);
    }

    public override void Update()
    {
        var remove = new BitMask();
        for (var i = 0; i < CurrentBaits.Count; i++)
        {
            var (_, target) = Raid.WithSlot(includeDead: true).IncludedInMask(CurrentBaits[i].Targets).First();
            if (target.IsDead)
            {
                remove.Set(i);
                continue;
            }
            var angle = Module.PrimaryActor.AngleTo(target);
            var src = target.Position + angle.ToDirection() * 7;
            var gap = (target.Position - Module.PrimaryActor.Position).Length();
            ref var bait = ref CurrentBaits.Ref(i);
            bait.Origin = src;
            bait.Shape = new AOEShapeRect(7 + gap, 3);
        }
        foreach (var bit in remove.SetBits().Reverse())
            CurrentBaits.RemoveAt(bit);

        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_GreatReturnToNothing or AID._Weaponskill_ReturnToNothing)
        {
            CurrentBaits.Clear();
            NumCasts++;
        }
    }
}

class ChainsOfCondemnation(BossModule module) : Components.StayMove(module)
{
    public bool Active { get; private set; }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
        {
            Active = true;
            SetState(slot, new(Requirement.NoMove, WorldState.CurrentTime));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}
class MeltdownBaited(BossModule module) : Components.StandardAOEs(module, AID._Spell_Meltdown1, 5);
class MeltdownSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_Meltdown2, 5);

class Emptiness(BossModule module) : Components.UntelegraphedBait(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_AiryEmptiness)
            CurrentBaits.Add(new(Module.PrimaryActor.Position, Raid.WithSlot().OrderByDescending(d => d.Item2.Class.IsSupport()).Take(4).Mask(), new AOEShapeCone(60, 30.Degrees()), Module.CastFinishAt(spell, 1), count: 4, stackSize: 2));

        if ((AID)spell.Action.ID == AID._Weaponskill_DenseEmptiness)
            CurrentBaits.Add(new(Module.PrimaryActor.Position, Raid.WithSlot().OrderByDescending(d => d.Item2.Role == Role.Healer).Take(2).Mask(), new AOEShapeCone(60, 50.Degrees()), Module.CastFinishAt(spell, 1), count: 2, stackSize: 4));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_AiryEmptiness1:
            case AID._Weaponskill_DenseEmptiness1:
                NumCasts++;
                CurrentBaits.Clear();
                break;
        }
    }
}

class GazeOfTheVoid(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GazeOfTheVoid1, new AOEShapeCone(40, 22.5f.Degrees()), maxCasts: 7, highlightImminent: true);

class Burst(BossModule module) : Components.CastCounterMulti(module, [AID._Weaponskill_Burst, AID._Weaponskill_ViolentBurst])
{
    readonly int[] _clockOrderPerPlayer = Utils.MakeArray(PartyState.MaxPartySize, -1);
    readonly DateTime[] _vuln = new DateTime[PartyState.MaxPartySize];

    record struct VoidBall(Actor Actor, bool Imminent, bool Tank, int ClockOrder)
    {
        public readonly float Radius => Tank ? 6 : 5;
    }

    readonly List<VoidBall> Balls = [];
    bool _ordered;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID._Gen_Tether_chn_z5fd16_0a1:
                Balls.Add(new(source, false, source.OID == (uint)OID.VoidGazeBig, -1));
                Organize();
                break;
            case TetherID._Gen_Tether_chn_z5fd17_0a1:
                Balls.Add(new(source, true, source.OID == (uint)OID.VoidGazeBig, -1));
                Organize();
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in Balls)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(b.Actor.Position, b.Radius, 0xFF000000, _clockOrderPerPlayer[pcSlot] == b.ClockOrder ? 3 : 2);

            var forbidden = _clockOrderPerPlayer[pcSlot] >= 0 && b.ClockOrder != _clockOrderPerPlayer[pcSlot] || !b.Imminent || _vuln[pcSlot] >= WorldState.CurrentTime;

            Arena.AddCircle(b.Actor.Position, b.Radius, forbidden ? ArenaColor.Danger : ArenaColor.Safe, _clockOrderPerPlayer[pcSlot] == b.ClockOrder ? 2 : 1);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var numSoaked = 0;
        var numAllowed = 0;

        foreach (var b in Balls)
        {
            var forbidden = _clockOrderPerPlayer[slot] >= 0 && b.ClockOrder != _clockOrderPerPlayer[slot] || !b.Imminent || _vuln[slot] >= WorldState.CurrentTime;
            if (!forbidden)
                numAllowed++;

            if (actor.Position.InCircle(b.Actor.Position, b.Radius))
            {
                if (forbidden)
                    hints.Add("GTFO from orb!");
                else
                    numSoaked++;
            }
        }

        if (numAllowed > 0 && numSoaked == 0)
        {
            if (numAllowed > 1)
                hints.Add("Soak an orb!");
            else
                hints.Add("Soak the orb!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {

        foreach (var b in Balls)
        {
            var isMine = b.ClockOrder == _clockOrderPerPlayer[slot] && b.Imminent && _vuln[slot] < WorldState.CurrentTime;

            if (isMine)
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(b.Actor.Position, b.Radius), DateTime.MaxValue);
            else
                hints.AddForbiddenZone(ShapeContains.Circle(b.Actor.Position, b.Radius), DateTime.MaxValue);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_MagicVulnerabilityUp && Raid.TryFindSlot(actor, out var slot))
            _vuln[slot] = status.ExpireAt;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            var b = Balls.FindIndex(b => b.Actor == caster);
            if (b >= 0)
            {
                for (var i = 0; i < Balls.Count; i++)
                    if (Balls[i].ClockOrder == Balls[b].ClockOrder)
                        Balls.Ref(i).Imminent = true;
                Balls.RemoveAt(b);
            }
        }
    }

    void Organize()
    {
        if (_ordered || Balls.Count != 8)
            return;

        var balls = Balls.ToList();
        Balls.Clear();

        var i = 0;
        foreach (var ball in balls.Where(b => !b.Imminent).ClockOrderWith(b => b.Actor, balls.First(s => !s.Imminent && s.Tank).Actor, Arena.Center))
            Balls.Add(ball with { ClockOrder = i++ });
        i = 0;
        foreach (var ball in balls.Where(b => b.Imminent).ClockOrderWith(b => b.Actor, balls.First(s => s.Imminent && s.Tank).Actor, Arena.Center))
            Balls.Add(ball with { ClockOrder = i++ });

        foreach (var (slot, group) in Service.Config.Get<Ex8EnuoConfig>().VoidGaze.Resolve(Raid))
            _clockOrderPerPlayer[slot] = group / 2;

        _ordered = true;
    }
}

class SilentTorrentSmall(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SilentTorrent5, new AOEShapeDonutSector(17, 19, 10.Degrees()));
class SilentTorrentMedium(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SilentTorrent4, new AOEShapeDonutSector(17, 19, 20.Degrees()));
class SilentTorrentLarge(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SilentTorrent3, new AOEShapeDonutSector(17, 19, 30.Degrees()));
class Vacuum(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Vacuum1)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_SilentTorrent or AID._Weaponskill_SilentTorrent1 or AID._Weaponskill_SilentTorrent2)
            _predicted.Add(new(new AOEShapeCircle(7), spell.LocXZ, default, Module.CastFinishAt(spell, 2.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class DeepFreezeRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Spell_DeepFreeze);
class DeepFreezeFlare(BossModule module) : Components.BaitAwayCast(module, AID._Spell_DeepFreeze1, new AOEShapeCircle(13), true);
class DeepFreezeFreeze(BossModule module) : Components.StayMove(module, maxTimeToShowHint: 3)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_DeepFreeze)
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell)));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_FreezingUp && Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new(Requirement.Move, WorldState.CurrentTime));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_FreezingUp && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}

class LoomingEmptinessKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_LoomingEmptiness2, 20, stopAtWall: true); // doesn't actually stop at wall, but good luck getting knocked out of the arena here
class LoomingEmptiness(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LoomingEmptiness2, 8);

class ArenaSwitcher : BossComponent
{
    public Actor? Beacon;

    public ArenaSwitcher(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public bool IntermissionOver => Beacon is { IsDead: true };

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BeaconInTheDark)
            Beacon = actor;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0 && state == 0x00020001)
            // intermission arena is actually a 500x500 square
            Arena.Bounds = new ArenaBoundsSquare(30);

        if (index == 0 && state == 0x00080004)
            Arena.Bounds = new ArenaBoundsCircle(20);
    }
}

class EmptyShadow(BossModule module) : Components.CastTowers(module, AID._Weaponskill_EmptyShadow, 6)
{
    BitMask _baits;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_com_trg07_0a1 && Raid.TryFindSlot(actor, out var slot))
        {
            _baits.Set(slot);
            UpdateBaits();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            UpdateBaits();
    }

    void UpdateBaits()
    {
        for (var i = 0; i < Towers.Count; i++)
            Towers.Ref(i).ForbiddenSoakers |= _baits;

        if (Towers.Count == 4)
        {
            Towers.SortByReverse(t => (t.Position - Arena.Center).ToAngle().Deg);

            var assignments = Service.Config.Get<Ex8EnuoConfig>().IntermissionTower.Resolve(Raid).ToList();
            var it = 0;
            foreach (var (slot, _) in assignments.OrderBy(a => a.group))
            {
                if (_baits[slot])
                    continue;

                if (it >= Towers.Count)
                    break;
                Towers.Ref(it).ForbiddenSoakers = ~BitMask.Build(slot);
                it++;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _baits.Reset();
    }
}
class VoidalTurbulence(BossModule module) : Components.GenericBaitAway(module, AID._Weaponskill_VoidalTurbulence1)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_com_trg07_0a1 && Module.Enemies(OID.LoomingShadow).FirstOrDefault() is { } shadow)
            CurrentBaits.Add(new(shadow, actor, new AOEShapeCone(60, 30.Degrees()), WorldState.FutureTime(7.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class LoomingShadow(BossModule module) : Components.Adds(module, (uint)OID.LoomingShadow);
class Shadows(BossModule module) : Components.AddsMulti(module, [OID.ProtectiveShadow, OID.AggressiveShadow, OID.SoothingShadow], 1);
class Beacon(BossModule module) : Components.Adds(module, (uint)OID.BeaconInTheDark, 1, true);
class Gauntlet(BossModule module) : Components.GenericInvincible(module)
{
    readonly int[] _playerOrder = Utils.MakeArray(PartyState.MaxPartySize, -1);
    readonly Actor?[] _addsOrdered = new Actor?[PartyState.MaxPartySize];

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var order = _playerOrder.BoundSafeAt(slot, -1);

        for (var i = 0; i < _addsOrdered.Length; i++)
            if (i != order && _addsOrdered[i] is { } add)
                yield return add;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is >= 5357 and <= 5364 && Raid.TryFindSlot(actor, out var slot))
            _playerOrder[slot] = (int)status.ID - (int)SID.GauntletTaken1;

        if (status.ID is >= 5365 and <= 5372)
            _addsOrdered[status.ID - (uint)SID.GauntletThrown1] = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is >= 5357 and <= 5364 && Raid.TryFindSlot(actor, out var slot))
            _playerOrder[slot] = -1;

        if (status.ID is >= 5365 and <= 5372)
        {
            var ix = Array.IndexOf(_addsOrdered, actor);
            if (ix >= 0)
                _addsOrdered[ix] = null;
        }
    }
}

class DemonEye(BossModule module) : Components.CastGaze(module, AID._Weaponskill_DemonEye1, range: 20);

class WeightOfNothing(BossModule module) : Components.BaitAwayCast(module, AID._Weaponskill_WeightOfNothing, new AOEShapeRect(100, 4));
class Nothingness(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Nothingness, new AOEShapeRect(100, 2));

class LightlessWorld(BossModule module) : Components.RaidwideInstant(module, AID._Weaponskill_LightlessWorld2, 1)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_LightlessWorld)
            Activation = Module.CastFinishAt(spell, Delay);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID._Weaponskill_LightlessWorld1)
            NumCasts++;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1116, NameID = 14749, DevOnly = true)]
public class Ex8Enuo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public Actor? LoomingShadow() => Enemies(OID.LoomingShadow).FirstOrDefault();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.LoomingShadow), ArenaColor.Enemy);
    }
}

