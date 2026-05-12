namespace BossMod.Stormblood.Savage.O8S1Kefka;

#region Enums
public enum OID : uint
{
    Boss = 0x2150,
    Helper = 0x233C,
    StrikingDummy = 0x385, // R1.500, x?
    TinyMandragora = 0x76, // R0.300, x?
    GravenImage = 0x18D6, // R0.500, x?, mixed types
    Kefka1 = 0x2151, // R3.500, x?
}

public enum AID : uint
{
    AutoAttack = 10434, // 2150->player, no cast, single-target
    ManaCharge = 10449, // 2150->self, 2.0s cast, single-target
    FlagrantFire = 10446, // 2150->self, 4.2s cast, single-target
    FlagrantFire1 = 10448, // 18D6->player, no cast, range 6 circle
    Hyperdrive = 10472, // 2150->player, 4.0s cast, range 5 circle
    ManaRelease = 10450, // 2150->self, 4.0s cast, single-target
    FlagrantFire2 = 11059, // 18D6->player, no cast, range 6 circle
    ThrummingThunder = 10444, // 18D6->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunder1 = 10443, // 18D6->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunder2 = 10442, // 2150->self, 5.0s cast, single-target
    UltimaUpsurge = 10471, // 2150->self, 4.0s cast, range 100 circle
    GravenImage = 10455, // 2150->self, 5.0s cast, single-target
    InexorableWill = 10458, // 18D6->location, 3.0s cast, range 6 circle
    WaveCannon = 10460, // 18D6->self, no cast, range 100+R width 6 rect
    // = 11060, // 2150->self, 3.0s cast, single-target
    PulseWave = 10461, // 18D6->player, 5.0s cast, single-target
    IndomitableWill = 10457, // 18D6->player, 5.0s cast, range 6 circle
    TimelyTeleport = 10452, // 18D6->self, 4.0s cast, range 6 circle
    TimelyTeleport1 = 10451, // 2150->self, 4.0s cast, single-target
    RevoltingRuin = 10453, // 2150->self, no cast, range 100+R ?-degree cone
    LightOfJudgment = 10456, // 2150->location, 6.0s cast, range 100 circle
    BlizzardBlitz = 10441, // 18D6->self, 5.0s cast, range ?-40 donut
    BlizzardBlitz1 = 10439, // 2150->self, 5.0s cast, single-target
    Shockwave = 10459, // 18D6->self, 10.0s cast, range 100+R width 40 rect
    ThrummingThunder3 = 11056, // 18D6->self, 7.0s cast, range 40+R width 10 rect
    ThrummingThunder4 = 11055, // 18D6->self, 7.0s cast, range 40+R width 10 rect
    BlizzardBlitz2 = 11054, // 18D6->self, 7.0s cast, range ?-40 donut
    Vitrophyre = 10466, // 18D6->player, no cast, range 5 circle
    GravitationalWave = 10462, // 18D6->self, 5.0s cast, range 100+R ?-degree cone
    //1 = 11061, // 2150->self, no cast, single-target
    AeroAssault = 10454, // 2150->self, 5.0s cast, range 100 circle
    FlagrantFire3 = 10447, // 18D6->player, no cast, range 5 circle
    FlagrantFire4 = 11058, // 18D6->player, no cast, range 5 circle
    IndolentWill = 10468, // 18D6->self, 5.0s cast, range 100 circle
    IdyllicWill = 10470, // 18D6->player, 5.0s cast, single-target
    ThrummingThunder5 = 10445, // 18D6->self, 5.0s cast, range 40+R width 10 rect
    BlizzardBlitz3 = 10440, // 18D6->self, 5.0s cast, range 10 circle
    AveMaria = 10467, // 18D6->self, 5.0s cast, range 100 circle
    ThrummingThunder6 = 11057, // 18D6->self, 7.0s cast, range 40+R width 10 rect
    BlizzardBlitz4 = 11053, // 18D6->self, 7.0s cast, range 10 circle
    LightOfJudgment1 = 10833, // 2150->location, 4.0s cast, range 100 circle
}
public enum SID : uint
{
    ManaCharge = 1482, // Boss->GravenImage/Boss, extra=0x0
    JestersAntics = 1486, // none->GravenImage/Boss, extra=0x0
    FireCharged = 1483, // Boss->GravenImage/Boss, extra=0x0
    MagicVulnerabilityUp = 1138, // GravenImage->player, extra=0x0
    VulnerabilityUp = 444, // GravenImage->player, extra=0x0
    ThunderCharged = 1485, // Boss->GravenImage/Boss, extra=0x0
    JestersTruths = 1487, // none->GravenImage/Boss, extra=0x0
    BlizzardCharged = 1484, // Boss->GravenImage/Boss, extra=0x0
    Sleep = 1510, // GravenImage->player, extra=0x0
}
public enum IconID : uint
{
    Spread = 127, // player->self
    PartnerStack = 93, // player->self
    PartyStack = 128, // player->self
}
public enum TetherID : uint
{
    WaveCannonTether = 45, // GravenImage->player
}
#endregion

/*
// ============================================================
// Blizzard Blitz — point-blank AOE centred on Kefka
// Normal: stand outside the circle
// Trick:  stand inside the circle (do the opposite)
// ============================================================

class BlizzardBlitz(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private bool _isTrick;
    private static readonly AOEShapeCircle BlitzCircle = new(10f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var a in _aoes)
            yield return _isTrick ? a with { Inverted = true, Color = ArenaColor.SafeFromAOE } : a;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count > 0)
            hints.Add(_isTrick ? "Trick Blizzard — stand INSIDE the circle!" : "Blizzard Blitz — get away from Kefka!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BlizzardBlitzNormal:
                _isTrick = false;
                _aoes.Add(new(BlitzCircle, Module.PrimaryActor.Position, default, Module.CastFinishAt(spell)));
                break;
            case AID.BlizzardBlitzTrick:
                _isTrick = true;
                _aoes.Add(new(BlitzCircle, Module.PrimaryActor.Position, default, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BlizzardBlitzNormal or AID.BlizzardBlitzTrick)
        {
            _aoes.Clear();
            _isTrick = false;
        }
    }
}

// ============================================================
// Mana Charge — global hint reminding players to memorise the
// next AOE ability, since it will be replayed by Mana Release
// ============================================================

class ManaCharge(BossModule module) : Components.CastHint(module, AID.ManaCharge,
    "Mana Charge! Memorise the next AOE — it will be replayed!");

// ============================================================
// Aero Assault — knockback from Kefka's current position
// ============================================================

class AeroAssault(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AeroAssault, 20f, stopAtWall: false)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        foreach (var s in Sources(slot, actor))
            if (DestinationUnsafe(slot, actor, Components.Knockback.AwayFromSource(actor.Position, s.Origin, s.Distance)))
                hints.Add("Knockback — position to land safely!");
    }
}

// ============================================================
// Timely Teleport — Kefka teleports then fires a frontal cone
// ============================================================

class TimelyTeleport(BossModule module) : Components.GenericAOEs(module, AID.TimelyTeleportCone, "Get behind Kefka!")
{
    private readonly List<AOEInstance> _cones = [];
    private static readonly AOEShapeCone TeleportCone = new(30f, 90f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _cones;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TimelyTeleportCone)
            _cones.Add(new(TeleportCone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TimelyTeleportCone)
            _cones.Clear();
    }
}

// ============================================================
// GRAVEN IMAGE 1
// ── GI1 Line AOEs: hits first pair of DPS then second pair
// ── GI1 Floor AOEs: persistent circles under all players
// ── GI1 Tether Knockback: tethered players knocked back from statue
// ── GI1 Pair Stacks: each DPS shares with one T and one H
// ============================================================

// Straight line AOEs through DPS in two waves

// ============================================================
// GRAVEN IMAGE 2
// ── Orb knockback: all players knocked back from orb (W or E)
// ── Mana Release: Thrumming Thunder + Blizzard Blitz simultaneously
// ============================================================

class GI2OrbKnockback(BossModule module) : Components.Knockback(module, AID.GI2OrbKnockback, stopAtWall: false)
{
    private WPos _orbPosition;
    private bool _active;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_active)
            yield return new(_orbPosition, 20f, _activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ManaOrb)
        {
            _orbPosition = actor.Position;
            _active = true;
            _activation = WorldState.FutureTime(5f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GI2OrbKnockback)
            _active = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_active)
            hints.Add("Orb knockback — stay as close to the orb as possible!");
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos);
}

// ============================================================
// GRAVEN IMAGE 3
// ── Gravitas: shared AOE blast + leaves a bleed puddle
// ── Vitrophyre: personal spread AOE
// ── Half-arena blast (orb indicates which side)
// ── Aero Assault knockback (reused from main phase)
// ============================================================

class GI3Gravitas(BossModule module) : Components.UniformStackSpread(module, stackRadius: 5f, spreadRadius: 0f, minStackSize: 2, maxStackSize: 2)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.GravitasMarker)
            AddStack(actor, WorldState.FutureTime(5f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Gravitas)
            Stacks.Clear();
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Stacks.Count > 0)
            hints.Add("Gravitas — stack in pairs, then spread for Vitrophyre!");
    }
}

// Gravitas persistent bleed puddles — avoid after they drop
class GI3GravitasPuddles(BossModule module) : Components.PersistentVoidzone(
    module, 4f,
    m => m.Enemies(OID.Helper).Where(h => h.EventState != 7 && h.CastInfo == null))
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Sources(Module).Any())
            hints.Add("GTFO from Gravitas puddles — they apply bleed!");
    }
}

// Vitrophyre — personal spread, do not overlap
class GI3Vitrophyre(BossModule module) : Components.UniformStackSpread(module, stackRadius: 0f, spreadRadius: 6f, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.VitrophyreMarker)
            AddSpread(actor, WorldState.FutureTime(5f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Vitrophyre)
            Spreads.Clear();
    }
}

// Half-arena blast: orb appears on one side, players run to the other
class GI3HalfArenaBlast(BossModule module) : Components.GenericAOEs(module, AID.GI3HalfArenaBlast, "Move to safe side!")
{
    private readonly List<AOEInstance> _aoes = [];
    // Half-arena rect: covers one full half of the arena
    private static readonly AOEShapeRect HalfArena = new(20f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ManaOrb)
        {
            // Orb position indicates which half will be blasted
            // If orb spawns west (x < centre), blast the west half
            // If orb spawns east (x > centre), blast the east half
            var blastDir = actor.Position.X < Module.Center.X ? -90f.Degrees() : 90f.Degrees();
            _aoes.Add(new(HalfArena, Module.Center, blastDir, WorldState.FutureTime(6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GI3HalfArenaBlast)
            _aoes.Clear();
    }
}

// ============================================================
// GRAVEN IMAGE 4
// ── Half-arena orb blast (same pattern as GI3)
// ── Mana Release fires Flagrant Fire + Thrumming Thunder simultaneously
//    (those components are already active and handle the casts themselves)
// ============================================================

class GI4HalfArenaBlast(BossModule module) : Components.GenericAOEs(module, AID.GI4HalfArenaBlast, "Move to safe side!")
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect HalfArena = new(20f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ManaOrb)
        {
            var blastDir = actor.Position.X < Module.Center.X ? -90f.Degrees() : 90f.Degrees();
            _aoes.Add(new(HalfArena, Module.Center, blastDir, WorldState.FutureTime(6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GI4HalfArenaBlast)
            _aoes.Clear();
    }
}
class GI1OrbLines(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _firstWave = [];
    private readonly List<AOEInstance> _secondWave = [];
    private static readonly AOEShapeRect OrbLine = new(30f, 2f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // Show first wave while it is pending; once it clears, show second
        if (_firstWave.Count > 0)
            foreach (var a in _firstWave)
                yield return a with { Color = ArenaColor.Danger };
        else
            foreach (var a in _secondWave)
                yield return a with { Color = ArenaColor.AOE };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GI1OrbLineFirst:
                _firstWave.Add(new(OrbLine, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.GI1OrbLineSecond:
                _secondWave.Add(new(OrbLine, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GI1OrbLineFirst:
                _firstWave.Clear();
                break;
            case AID.GI1OrbLineSecond:
                _secondWave.Clear();
                break;
        }
    }
}

class GI1TetherKnockback(BossModule module) : Components.Knockback(module, AID.GI1TetherKB, stopAtWall: false)
{
    private readonly List<Actor> _tethered = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        // Knockback is from the statue (GravenImage actor) toward the tethered player
        var statue = Module.Enemies(OID.GravenImage).FirstOrDefault();
        if (statue != null && _tethered.Any(t => t.InstanceID == actor.InstanceID))
            yield return new(statue.Position, 15f, WorldState.FutureTime(3f));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((SID)tether.ID == SID.TetherStatue && source.Type == ActorType.Player)
            _tethered.Add(source);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((SID)tether.ID == SID.TetherStatue)
            _tethered.RemoveAll(a => a.InstanceID == source.InstanceID);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tethered.Any(t => t.InstanceID == actor.InstanceID))
            hints.Add("Tethered! Aim knockback toward your assigned pair position!");
        else
            base.AddHints(slot, actor, hints);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos);
}

// Pair stacks: each DPS must be paired with a tank or healer
class GI1PairStacks(BossModule module) : Components.UniformStackSpread(module, stackRadius: 3f, spreadRadius: 0f, minStackSize: 2, maxStackSize: 2)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.GI1PairStackMarker)
            AddStack(actor, WorldState.FutureTime(5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GI1PairStack)
            Stacks.Clear();
    }
}
*/

class UltimaUpsurge(BossModule module) : Components.RaidwideCast(module, AID.UltimaUpsurge);
class LightOfJudgement(BossModule module) : Components.RaidwideCast(module, AID.LightOfJudgment, "Heavy raidwide — heavy mitigation!");
class Hyperdrive(BossModule module) : Components.BaitAwayCast(module, AID.Hyperdrive, new AOEShapeCircle(6f), centerAtTarget: true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaits.Any(b => b.Target == actor))
            hints.Add("Tankbuster on you!");
    }
}
class BlizzardBlitz(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitz3, new AOEShapeCircle(10f))
{
    private bool _inverted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return
                _inverted ? aoe with { Inverted = true, Color = ArenaColor.SafeFromAOE } : aoe;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor != Module.PrimaryActor)
            return;

        switch ((SID)status.ID)
        {
            case SID.JestersTruths:
                _inverted = false;
                break;

            case SID.JestersAntics:
                _inverted = true;
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveAOEs(0, Module.PrimaryActor).Any())
            hints.Add(_inverted
                ? "Trick Blizzard — stand inside!"
                : "Blizzard Blitz — move out!");
    }
}
class FlagrantFire(BossModule module) : Components.GenericStackSpread(module)
{
    private enum Pattern
    {
        None,
        Stack,
        Spread
    }

    private Pattern _storedPattern;
    private bool _inverted;
    private bool _charged;
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor != Module.PrimaryActor)
            return;

        switch ((SID)status.ID)
        {
            case SID.JestersTruths:
                _inverted = false;
                break;
            case SID.JestersAntics:
                _inverted = true;
                break;
            case SID.FireCharged:
                _charged = true;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.PartyStack:
                {
                    if (_inverted)
                    {
                        Spreads.Add(new(actor, 6f, WorldState.FutureTime(5)));
                        _storedPattern = Pattern.Spread;
                    }
                    else
                    {
                        Stacks.Add(new(actor, 10f, minSize: 8, activation: WorldState.FutureTime(5)));
                        _storedPattern = Pattern.Stack;
                    }
                    break;
                }

            case IconID.Spread:
                {
                    if (_inverted)
                    {
                        Stacks.Add(new(actor, 10f, minSize: 8, activation: WorldState.FutureTime(5)));
                        _storedPattern = Pattern.Stack;
                    }
                    else
                    {
                        Spreads.Add(new(actor, 6f, WorldState.FutureTime(5)));
                        _storedPattern = Pattern.Spread;
                    }
                    break;
                }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ManaRelease:
                {
                    if (!_charged)
                        break;

                    _charged = false;

                    // replay stored pattern
                    if (_storedPattern == Pattern.Stack)
                    {
                        // convert to stack again
                        if (Stacks.Count > 0)
                            Stacks.AddRange(Stacks);
                    }
                    else if (_storedPattern == Pattern.Spread)
                    {
                        if (Spreads.Count > 0)
                            Spreads.AddRange(Spreads);
                    }

                    break;
                }

            case AID.FlagrantFire1:
            case AID.FlagrantFire2:
            case AID.FlagrantFire3:
            case AID.FlagrantFire4:
                Stacks.Clear();
                Spreads.Clear();
                break;
        }
    }
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Stacks.Count > 0)
            hints.Add(_inverted ? "FAKE: Spread!" : "Stack!");
        else if (Spreads.Count > 0)
            hints.Add(_inverted ? "FAKE: Stack!" : "Spread!");
    }
}
class ThrummingThunder(BossModule module) : Components.StandardAOEs(module, AID.ThrummingThunder, new AOEShapeRect(40.5f, 5f))
{
    private bool _inverted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return _inverted
                ? aoe with { Inverted = true, Color = ArenaColor.SafeFromAOE }
                : aoe;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor != Module.PrimaryActor)
            return;

        switch ((SID)status.ID)
        {
            case SID.JestersTruths:
                _inverted = true;
                break;

            case SID.JestersAntics:
                _inverted = false;
                break;
        }
    }
}
class InexorableWill(BossModule module) : Components.StandardAOEs(module, AID.InexorableWill, 6f);

class IndomitableWill(BossModule module)
    : Components.GenericBaitAway(module, AID.IndomitableWill)
{
    private static readonly AOEShapeRect Shape = new(100.5f, 3f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.IndomitableWill)
            return;

        var target = WorldState.Actors.Find(spell.TargetID);
        if (target == null)
            return;

        CurrentBaits.Add(new(caster, target, Shape, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IndomitableWill)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}
class O8S1KefkaStates : StateMachineBuilder
{
    public O8S1KefkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
        .ActivateOnEnter<UltimaUpsurge>()
        .ActivateOnEnter<LightOfJudgement>()
        .ActivateOnEnter<Hyperdrive>()
        .ActivateOnEnter<BlizzardBlitz>()
        .ActivateOnEnter<FlagrantFire>()
        .ActivateOnEnter<ThrummingThunder>()
        .ActivateOnEnter<InexorableWill>()
        .ActivateOnEnter<IndomitableWill>()
        ;
    }
}
/*
class PulseWave(BossModule module) : Components.SingleTargetCast(module, AID.PulseWave);
class TimelyTeleport(BossModule module) : Components.StandardAOEs(module, AID.TimelyTeleport, 6f);
class LightOfJudgment(BossModule module) : Components.RaidwideCast(module, AID.LightOfJudgment);
class BlizzardBlitz(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitz, new AOEShapeDonut(TODO, 40));
class Shockwave(BossModule module) : Components.StandardAOEs(module, AID.Shockwave, new AOEShapeRect(100.5f, 20f));
class ThrummingThunder3(BossModule module) : Components.StandardAOEs(module, AID.ThrummingThunder3, new AOEShapeRect(40.5f, 5f));
class ThrummingThunder4(BossModule module) : Components.StandardAOEs(module, AID.ThrummingThunder4, new AOEShapeRect(40.5f, 5f));
class BlizzardBlitz2(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitz2, new AOEShapeDonut(TODO, 40));
class GravitationalWave(BossModule module) : Components.StandardAOEs(module, AID.GravitationalWave, new AOEShapeCone(100.5f, TODO));
class AeroAssault(BossModule module) : Components.RaidwideCast(module, AID.AeroAssault);
class IndolentWill(BossModule module) : Components.RaidwideCast(module, AID.IndolentWill);
class IdyllicWill(BossModule module) : Components.SingleTargetCast(module, AID.IdyllicWill);
class ThrummingThunder5(BossModule module) : Components.StandardAOEs(module, AID.ThrummingThunder5, new AOEShapeRect(40.5f, 5f));
class BlizzardBlitz3(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitz3, 10f);
class AveMaria(BossModule module) : Components.RaidwideCast(module, AID.AveMaria);
class ThrummingThunder6(BossModule module) : Components.StandardAOEs(module, AID.ThrummingThunder6, new AOEShapeRect(40.5f, 5f));
class BlizzardBlitz4(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitz4, 10f);
class LightOfJudgment1(BossModule module) : Components.RaidwideCast(module, AID.LightOfJudgment1);
*/
[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "Akechi", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 295, NameID = 7131)]
public class O8S1Kefka(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
