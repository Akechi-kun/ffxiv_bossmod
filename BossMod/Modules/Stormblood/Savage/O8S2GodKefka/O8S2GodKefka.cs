using static BossMod.BossComponent;
using static BossMod.Components.GenericAOEs;

namespace BossMod.Shadowbringers.Raid.O8SKefka;

// ============================================================
// Additional enums for God Kefka (Phase 2)
// All values are placeholders — replace with real packet data
// ============================================================

public enum OID : uint
{
    GodKefka = 0x0010, // R-size boss actor for Phase 2
    KefkaClone = 0x0011, // Clone spawned during All Things Ending
    AngelHead = 0x0012, // Floating head that chases DPS during Forsaken
    TrineSmall = 0x0013, // Small triangle actor
    TrineLarge = 0x0014, // Large centre triangle actor
    PathOfLight = 0x0015, // Meteor circle actor (has rotating orbs indicating soaker count)
    PreyPuddle = 0x0016, // Persistent flame puddle left by Prey
    StatueGod = 0x0017, // Statue of the Gods that returns in Forsaken 3
}

public enum AID2 : uint
{
    // ── Raidwides ──────────────────────────────────────────
    Ultima = 0x2001, // Unavoidable raidwide
    Forsaken = 0x2002, // Mini-phase transition raidwide
    LightOfJudgement2 = 0x2003, // Heavy raidwide ending each mini-phase

    // ── HP reduction ───────────────────────────────────────
    HeartlessAngel = 0x2010, // Sets all HP to 1
    HeartlessArchangel = 0x2011, // Sets all HP to 1 + Incurable debuff

    // ── Tankbusters ────────────────────────────────────────
    HyperDriveGod = 0x2020, // Magical tankbuster + bleed, AOE splash
    UltimateEmbrace = 0x2021, // Shareable physical tankbuster

    // ── Celestriad (all three simultaneous) ────────────────
    CelestriadThunder = 0x2030, // Thrumming Thunder component of Celestriad
    CelestriadFire = 0x2031, // Flagrant Fire component (one H stack, DPS spread)
    CelestriadFireStack = 0x2032, // Stack resolve within Celestriad
    CelestriadFireSpread = 0x2033, // Spread resolve within Celestriad
    CelestriadBlizzard = 0x2034, // Blizzard Blitz point-blank + donut sequence

    // ── Wings of Destruction ───────────────────────────────
    WingsOfDestructionTwo = 0x2040, // Two wings: closest + farthest hit
    WingsOfDestructionOne = 0x2041, // One wing: that side of arena blasted

    // ── Trine ──────────────────────────────────────────────
    TrineSmallExplode = 0x2050, // Small triangle tip explosions
    TrineLargeExplode = 0x2051, // Large triangle tip explosions

    // ── Path of Light (meteor circles) ─────────────────────
    PathOfLightSoak = 0x2060, // Meteor detonation — must have correct soaker count

    // ── Angel Heads ────────────────────────────────────────
    AngelHeadCollision = 0x2070, // Damage + debuff when DPS collides with their head

    // ── Past / Future ──────────────────────────────────────
    PastFutureCharge = 0x2080, // Initial AoE charge at a random player
    PastsForgotten = 0x2081, // Backward AOE followup
    FuturesNumbered = 0x2082, // Frontal AOE followup

    // ── All Things Ending ──────────────────────────────────
    AllThingsEndingLeap = 0x2090, // Kefka + clones leap to T/H positions
    AllThingsEndingPast = 0x2091, // Backward cleave from each clone
    AllThingsEndingFuture = 0x2092, // Frontal cleave from each clone

    // ── Prey (Forsaken 2) ──────────────────────────────────
    PreyChase = 0x20A0, // Four consecutive AOE blasts on prey-marked DPS
    PreyPuddleSpawn = 0x20A1, // Persistent flame puddle left by each blast
    PreyMeteor = 0x20A2, // Meteors dropped on non-marked DPS
    ProximityBlast = 0x20A3, // Proximity marker at north circle

    // ── Graven Pulse Wave (Forsaken 3) ─────────────────────
    GravenPulseWave = 0x20B0, // Tether knockback into meteor circles

    // ── Sleep / Charm (Forsaken 3, returns from P1 GI5) ───
    // Status IDs used in SID2 below
    SleepTether = 0x20C0, // Tether to mid-right (purple) → Sleep
    CharmTether = 0x20C1, // Tether to top-left (yellow)  → Charm

    // ── Statue Orb (Forsaken 3) ────────────────────────────
    StatueOrbBlast = 0x20D0, // Half-arena blast from statue orb

    // ── Enrage ─────────────────────────────────────────────
    UltimaEnrage = 0x20FF, // Repeated Ultima spam with Damage Up stacks
}

public enum SID2 : uint
{
    BleedGod = 0x3001, // Bleed applied by HyperDrive in Phase 2
    Incurable = 0x3002, // Negates all healing (Heartless Archangel)
    Charm = 0x3003, // Charmed — player loses control, moves toward allies
    Sleep = 0x3004, // Asleep — cannot move or act
    PreyMark = 0x3005, // Prey marker on farthest two DPS
    DamageUp = 0x3006, // Enrage buff stacking on Kefka
}

public enum IconID2 : uint
{
    WingsTwoClosest = 0x0201, // Wings (two) — on the closest player
    WingsTwoFarthest = 0x0202, // Wings (two) — on the farthest player
    PreyMarker = 0x0203, // Prey — chased by four AOE blasts
    UltimateEmbrace = 0x0204, // Physical tankbuster share marker
}

// ============================================================
// Raidwides
// ============================================================

class Ultima(BossModule module) : Components.RaidwideCast(module, AID2.Ultima);

class Forsaken(BossModule module) : Components.RaidwideCast(module, AID2.Forsaken, "Forsaken — mini-phase incoming!");

class LightOfJudgement2(BossModule module) : Components.RaidwideCast(module, AID2.LightOfJudgement2, "Heavy raidwide — heavy mitigation!");

// ============================================================
// Heartless Angel — sets all HP to 1
// No mechanical dodge; just a global warning so healers
// know to top everyone up immediately afterward
// ============================================================

class HeartlessAngel(BossModule module) : Components.CastHint(module, AID2.HeartlessAngel,
    "Heartless Angel — all HP set to 1! Heal immediately after!");

// ============================================================
// Heartless Archangel — HP to 1 + Incurable debuff
// Players at full HP get 4s Incurable; players below full get 12s
// Healers MUST top everyone up before this cast
// ============================================================

class HeartlessArchangel(BossModule module) : Components.CastHint(module, AID2.HeartlessArchangel,
    "Heartless Archangel — be at FULL HP or Incurable lasts 12s!")
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // Signal to AI that all players will take damage — prioritise healing
        if (Casters.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(Casters[0].CastInfo));
    }
}

// ============================================================
// Hyperdrive (God Kefka) — magical tankbuster + bleed + AOE splash
// Same geometry as Phase 1 but now applies a bleed debuff
// ============================================================

class HyperDriveGod(BossModule module) : Components.BaitAwayCast(module, AID2.HyperDriveGod, new AOEShapeCircle(6f), centerAtTarget: true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaits.Any(b => b.Target == actor))
            hints.Add("Tankbuster — use cooldowns! Applies bleed!");
    }
}

// ============================================================
// Ultimate Embrace — shareable physical tankbuster
// Both tanks must share (or one uses invuln solo)
// Followed immediately by HyperDrive — tanks must separate after
// ============================================================

class UltimateEmbrace(BossModule module) : Components.GenericSharedTankbuster(module, AID2.UltimateEmbrace, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.UltimateEmbrace)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.UltimateEmbrace)
            Source = Target = null;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Active)
            hints.Add("Ultimate Embrace — both tanks share! Separate after for HyperDrive!");
    }
}

// ============================================================
// Celestriad — Thrumming Thunder + Flagrant Fire + Blizzard Blitz
// all firing simultaneously
// One healer gets a stack marker; DPS get spread circles;
// Thrumming Thunder creates a plus-sign (+) pattern
// ============================================================

// The Thunder component within Celestriad: plus-sign columns
class CelestriadThunder(BossModule module) : Components.GenericAOEs(module, AID2.CelestriadThunder)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect Column = new(30f, 4f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.CelestriadThunder)
            _aoes.Add(new(Column, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.CelestriadThunder)
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1f));
    }
}

// The Fire component within Celestriad: one H stack + DPS spread
class CelestriadFire(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID2)spell.Action.ID)
        {
            case AID2.CelestriadFireStack:
                Stacks.Clear();
                if (WorldState.Actors.Find(spell.TargetID) is { } t)
                    Stacks.Add(new Stack(t, 6f, minSize: 2, activation: Module.CastFinishAt(spell)));
                break;
            case AID2.CelestriadFireSpread:
                Spreads.Clear();
                if (WorldState.Actors.Find(spell.TargetID) is { } s)
                    Spreads.Add(new Spread(s, 6f, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID is AID2.CelestriadFireStack or AID2.CelestriadFireSpread)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Stacks.Count > 0 || Spreads.Count > 0)
            hints.Add("Celestriad Fire — one H stacks with tanks, all DPS spread!");
    }
}

// The Blizzard component within Celestriad: point-blank then donut
class CelestriadBlizzard(BossModule module) : Components.GenericAOEs(module, AID2.CelestriadBlizzard)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle PBAoE = new(10f);
    private static readonly AOEShapeDonut Donut = new(10f, 20f);
    private int _castsDone;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.CelestriadBlizzard)
            // First cast is point-blank, second is donut
            _aoes.Add(new(_castsDone == 0 ? PBAoE : Donut,
                Module.PrimaryActor.Position, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.CelestriadBlizzard)
        {
            _aoes.Clear();
            ++_castsDone;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count > 0)
            hints.Add(_castsDone == 0
                ? "Celestriad Blizzard — get away from Kefka!"
                : "Celestriad Blizzard — get INTO the donut ring!");
    }
}

// ============================================================
// Wings of Destruction — Two Wings
// Hits the CLOSEST and FARTHEST player simultaneously with large AOEs
// Main tank stays close; off-tank sprints far; everyone else mid-range
// ============================================================

class WingsOfDestructionTwo(BossModule module) : BossComponent(module)
{
    private bool _active;
    private DateTime _activation;

    // Track closest and farthest players by distance to boss
    private (Actor? actor, bool isFarthest) _closestTarget;
    private (Actor? actor, bool isFarthest) _farthestTarget;

    private static readonly AOEShapeCircle WingBlast = new(8f);

    public override void Update()
    {
        if (!_active)
            return;

        // Recalculate closest/farthest every frame as players move
        var sorted = Raid.WithoutSlot()
            .OrderBy(a => (a.Position - Module.PrimaryActor.Position).LengthSq())
            .ToList();

        _closestTarget = (sorted.FirstOrDefault(), false);
        _farthestTarget = (sorted.LastOrDefault(), true);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;

        bool isClosest = _closestTarget.actor == actor;
        bool isFarthest = _farthestTarget.actor == actor;

        if (actor.Role == Role.Tank)
        {
            if (!isClosest && !isFarthest)
                hints.Add("Wings (Two) — be the closest OR farthest tank!");
            else
                hints.Add(isClosest ? "Wings (Two) — stay close! Use cooldowns!" : "Wings (Two) — sprint far! Use cooldowns!", false);
        }
        else
        {
            // Non-tanks: stay at mid-distance, avoid being closest or farthest
            if (isClosest || isFarthest)
                hints.Add("Wings (Two) — move to mid-range distance from Kefka!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_active)
            return;

        var bossPos = Module.PrimaryActor.Position;

        if (actor.Role != Role.Tank)
        {
            // Non-tanks: forbidden within 5 units (too close) and beyond 18 units (too far)
            hints.AddForbiddenZone(ShapeContains.Circle(bossPos, 5f), _activation);
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(bossPos, 18f), _activation);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (!_active) return PlayerPriority.Irrelevant;
        return _closestTarget.actor == player || _farthestTarget.actor == player
            ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active) return;
        if (_closestTarget.actor != null)
            WingBlast.Outline(Arena, _closestTarget.actor.Position);
        if (_farthestTarget.actor != null)
            WingBlast.Outline(Arena, _farthestTarget.actor.Position);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.WingsOfDestructionTwo)
        {
            _active = true;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.WingsOfDestructionTwo)
        {
            _active = false;
            _closestTarget = default;
            _farthestTarget = default;
        }
    }
}

// ============================================================
// Wings of Destruction — One Wing
// The GLOWING side of the arena is blasted — move to opposite side
// ============================================================

class WingsOfDestructionOne(BossModule module) : Components.GenericAOEs(module, AID2.WingsOfDestructionOne, "Move to the safe side!")
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect HalfArena = new(20f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    // We detect which wing is glowing from the cast rotation / target position
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.WingsOfDestructionOne)
        {
            // Rotation indicates which side: east wing glowing → east blast
            // Use caster rotation to determine blast direction
            var blastDir = spell.Rotation;
            _aoes.Add(new(HalfArena, Module.Center, blastDir, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.WingsOfDestructionOne)
            _aoes.Clear();
    }
}

// ============================================================
// Trine — triangles rain down and explode at their tips
// Small version: several triangles, tips form AOE circles
// Large version: one giant central triangle + smaller ones
// ============================================================

class TrineSmall(BossModule module) : Components.GenericAOEs(module, AID2.TrineSmallExplode, "Dodge Trine tip explosions!")
{
    // Each triangle tip produces a circle AOE
    private readonly List<AOEInstance> _tipAOEs = [];
    private static readonly AOEShapeCircle TipBlast = new(4f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _tipAOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.TrineSmallExplode)
            _tipAOEs.Add(new(TipBlast, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.TrineSmallExplode)
            _tipAOEs.RemoveAll(a => a.Origin.AlmostEqual(spell.LocXZ, 1f));
    }
}

class TrineLarge(BossModule module) : Components.GenericAOEs(module, AID2.TrineLargeExplode, "Dodge Trine tip explosions! Center of large triangle may be safe!")
{
    private readonly List<AOEInstance> _tipAOEs = [];
    private static readonly AOEShapeCircle TipBlast = new(4f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _tipAOEs;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_tipAOEs.Count > 0)
            hints.Add("Large Trine — centre of large triangle is safe if no small trine overlaps it!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.TrineLargeExplode)
            _tipAOEs.Add(new(TipBlast, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.TrineLargeExplode)
            _tipAOEs.RemoveAll(a => a.Origin.AlmostEqual(spell.LocXZ, 1f));
    }
}

// ============================================================
// Path of Light — meteor circles that must be soaked
// Each circle shows rotating orbs = required soaker count
// First Forsaken: 2 per circle
// Later versions: 2-4 per circle in varying positions
// ============================================================

class PathOfLight(BossModule module) : Components.GenericTowers(module, AID2.PathOfLightSoak)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.PathOfLight)
        {
            // Default to 2 soakers; real soaker count should be read from
            // the actor's state/model data (rotating orb count) once IDs are confirmed
            Towers.Add(new Tower(actor.Position, 3f, minSoakers: 2, maxSoakers: 2,
                activation: WorldState.FutureTime(8f)));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.PathOfLight)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.PathOfLightSoak)
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.LocXZ, 1f));
    }
}

// ============================================================
// Angel Heads — tethered floating heads chase DPS during Forsaken
// DPS must keep distance until Incurable expires, then collide to remove
// ============================================================

class AngelHeads(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor head, Actor target)> _tethers = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.AngelHead)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tethers.Add((source, target));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.AngelHead)
            _tethers.RemoveAll(t => t.head.InstanceID == source.InstanceID);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var myHead = _tethers.FirstOrDefault(t => t.target == actor);
        if (myHead.head == null)
            return;

        // Check if Incurable is still active on this player
        bool incurable = actor.Statuses.Any(s => (SID2)s.ID == SID2.Incurable && s.ExpireAt > WorldState.CurrentTime);
        if (incurable)
            hints.Add("Angel Head chasing you — keep distance until Incurable fades!");
        else
            hints.Add("Incurable faded — collide with your Angel Head to remove it!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var myHead = _tethers.FirstOrDefault(t => t.target == actor);
        if (myHead.head == null)
            return;

        bool incurable = actor.Statuses.Any(s => (SID2)s.ID == SID2.Incurable && s.ExpireAt > WorldState.CurrentTime);
        if (incurable)
        {
            // Forbidden zone: don't get too close to the head
            hints.AddForbiddenZone(ShapeContains.Circle(myHead.head.Position, 3f));
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _tethers.Any(t => t.target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (head, target) in _tethers)
        {
            Arena.AddLine(head.Position, target.Position, ArenaColor.Danger);
            Arena.Actor(head, ArenaColor.Enemy);
        }
    }
}

// ============================================================
// Past / Future — Kefka charges at a player then
// Pasts Forgotten:  backward AOE
// Futures Numbered: frontal AOE
// ============================================================

class PastFuture(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone Cleave = new(15f, 90f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count > 0)
        {
            // We can infer which is coming from whichever AID triggered
            hints.Add("Past = Kefka cleaves BEHIND himself | Future = Kefka cleaves IN FRONT");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID2)spell.Action.ID)
        {
            // Past: cleave from behind Kefka (180° offset from facing)
            case AID2.PastsForgotten:
                _aoes.Add(new(Cleave, caster.Position, caster.Rotation + 180f.Degrees(), Module.CastFinishAt(spell)));
                break;
            // Future: cleave in front of Kefka (facing direction)
            case AID2.FuturesNumbered:
                _aoes.Add(new(Cleave, caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID is AID2.PastsForgotten or AID2.FuturesNumbered)
            _aoes.Clear();
    }
}

// ============================================================
// All Things Ending — Kefka + 3 clones leap to T/H positions
// then cleave either backward (Past) or forward (Future)
// Tanks and healers must bait clones to face away from the raid
// ============================================================

class AllThingsEnding(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone CloneCleave = new(15f, 90f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count > 0)
            hints.Add("All Things Ending — T/H bait clones, face them away from raid!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_aoes.Count > 0 && actor.Role is Role.Tank or Role.Healer)
            hints.Add("Bait clone — face it so cleave hits away from DPS!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID2)spell.Action.ID)
        {
            case AID2.AllThingsEndingPast:
                _aoes.Add(new(CloneCleave, caster.Position, caster.Rotation + 180f.Degrees(), Module.CastFinishAt(spell)));
                break;
            case AID2.AllThingsEndingFuture:
                _aoes.Add(new(CloneCleave, caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID is AID2.AllThingsEndingPast or AID2.AllThingsEndingFuture)
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1f));
    }
}

// ============================================================
// FORSAKEN 2 — Prey mechanic
// Farthest two DPS: chased by 4 consecutive AOE blasts leaving puddles
// Non-marked DPS: meteors dropped at their location
// ============================================================

class Prey(BossModule module) : BossComponent(module)
{
    private BitMask _preyTargets;
    private readonly List<AOEInstance> _chaseAOEs = [];
    private static readonly AOEShapeCircle ChaseBlast = new(6f);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID2)status.ID == SID2.PreyMark && Raid.TryFindSlot(actor, out var slot))
            _preyTargets.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID2)status.ID == SID2.PreyMark && Raid.TryFindSlot(actor, out var slot))
            _preyTargets.Clear(slot);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_preyTargets[slot])
            hints.Add("Prey! Drop puddles neatly in northern area — move to north!");
        else if (actor.Role is Role.Melee or Role.Ranged)
            hints.Add("Meteors target you — dodge and regroup north for 4-player circle!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // Prey targets must avoid non-prey players to not clip meteors/puddles
        foreach (var (j, other) in Raid.WithSlot().Where(r => !_preyTargets[r.Item1]))
            if (_preyTargets[slot])
                hints.AddForbiddenZone(ShapeContains.Circle(other.Position, ChaseBlast.Radius), WorldState.FutureTime(3f));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _preyTargets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
}

// Persistent flame puddles left by Prey chases
class PreyPuddles(BossModule module) : Components.PersistentVoidzone(
    module, 3f,
    m => m.Enemies(OID.PreyPuddle).Where(a => a.EventState != 7))
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Sources(Module).Any())
            hints.Add("GTFO from Prey puddles!");
    }
}

// Proximity blast at north circle — move away from north
class ProximityBlast(BossModule module) : BossComponent(module)
{
    private bool _active;
    private WPos _center;
    private DateTime _activation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_active)
            hints.Add("Proximity blast at north! Move away from the north circle!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_active)
            // Proximity: further is safer, so forbid being close to the blast center
            hints.AddForbiddenZone(ShapeContains.Circle(_center, 8f), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.ProximityBlast)
        {
            _active = true;
            _center = spell.LocXZ;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.ProximityBlast)
            _active = false;
    }
}

// ============================================================
// FORSAKEN 3 — Graven Pulse Wave
// Four players tethered to statue; knockback pushes them into
// their assigned meteor circle
// ============================================================

class GravenPulseWave(BossModule module) : Components.Knockback(module, AID2.GravenPulseWave, stopAtWall: false)
{
    private readonly List<(Actor player, Actor statue)> _tethers = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var statue = Module.Enemies(OID.StatueGod).FirstOrDefault();
        if (statue != null && _tethers.Any(t => t.player.InstanceID == actor.InstanceID))
            yield return new(statue.Position, 15f, WorldState.FutureTime(4f));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.Type == ActorType.Player)
        {
            var statue = Module.Enemies(OID.StatueGod).FirstOrDefault();
            if (statue != null)
                _tethers.Add((source, statue));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _tethers.RemoveAll(t => t.player.InstanceID == source.InstanceID);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tethers.Any(t => t.player.InstanceID == actor.InstanceID))
            hints.Add("Tethered to statue — aim knockback into your assigned meteor circle!");
        else
            base.AddHints(slot, actor, hints);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos);
}

// ============================================================
// FORSAKEN 3 — Sleep / Charm mechanic
// Top-left (yellow) tether → Charm (loses control, moves toward allies)
// Mid-right (purple) tether → Sleep (cannot move or act)
// Purple: stack in centre and go to sleep
// Yellow: spread to outer edges so you don't reach sleeping players
// ============================================================

class SleepCharm(BossModule module) : BossComponent(module)
{
    private BitMask _charmed;
    private BitMask _sleeping;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;
        switch ((SID2)status.ID)
        {
            case SID2.Charm: _charmed.Set(slot); break;
            case SID2.Sleep: _sleeping.Set(slot); break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;
        switch ((SID2)status.ID)
        {
            case SID2.Charm: _charmed.Clear(slot); break;
            case SID2.Sleep: _sleeping.Clear(slot); break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_charmed[slot])
            hints.Add("CHARMED — you are moving toward allies! Others must dodge you!");
        else if (_sleeping[slot])
            hints.Add("ASLEEP — stack in centre with other sleep targets!", false);
        else if (_charmed.Any())
        {
            // Warn players being approached by charmed allies
            foreach (var (j, charmedActor) in Raid.WithSlot().Where(r => _charmed[r.Item1]))
                if ((charmedActor.Position - actor.Position).LengthSq() < 100f)
                    hints.Add("Charmed player approaching — move away!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sleeping[slot])
        {
            // Sleep targets stack in centre
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Center, 5f));
        }
        else if (!_charmed[slot] && _charmed.Any())
        {
            // Non-charmed, non-sleeping: stay away from charmed players
            foreach (var (j, charmedActor) in Raid.WithSlot().Where(r => _charmed[r.Item1]))
                hints.AddForbiddenZone(ShapeContains.Circle(charmedActor.Position, 5f));
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_charmed[playerSlot])
        {
            customColor = ArenaColor.Danger;
            return PlayerPriority.Danger;
        }
        if (_sleeping[playerSlot])
            return PlayerPriority.Normal;
        return PlayerPriority.Irrelevant;
    }
}

// ============================================================
// FORSAKEN 3 — Statue Orb + Wings overlap
// Kefka should be faced north or south so there is always
// at least one safe quadrant when both blasts fire
// ============================================================

class GodStatueOrbBlast(BossModule module) : Components.GenericAOEs(module, AID2.StatueOrbBlast, "Half-arena blast — move to safe quadrant!")
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect HalfArena = new(20f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.StatueGod)
        {
            // Orb position on statue determines blast direction
            var blastDir = actor.Position.X < Module.Center.X ? -90f.Degrees() : 90f.Degrees();
            _aoes.Add(new(HalfArena, Module.Center, blastDir, WorldState.FutureTime(6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID2)spell.Action.ID == AID2.StatueOrbBlast)
            _aoes.Clear();
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count > 0)
            hints.Add("Position Kefka facing north/south to guarantee a safe quadrant!");
    }
}

// ============================================================
// Enrage — Ultima spam with Damage Up stacks
// ============================================================

class UltimaEnrage(BossModule module) : Components.CastHint(module, AID2.UltimaEnrage,
    "ENRAGE — defeat Kefka immediately!");

// ============================================================
// State Machine
// ============================================================

class O8SGodKefkaStates : StateMachineBuilder
{
    public O8SGodKefkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            // ── Core God Kefka abilities ───────────────────────
            .ActivateOnEnter<Ultima>()
            .ActivateOnEnter<Forsaken>()
            .ActivateOnEnter<LightOfJudgement2>()
            .ActivateOnEnter<HeartlessAngel>()
            .ActivateOnEnter<HeartlessArchangel>()
            .ActivateOnEnter<HyperDriveGod>()
            .ActivateOnEnter<UltimateEmbrace>()
            // ── Celestriad ─────────────────────────────────────
            .ActivateOnEnter<CelestriadThunder>()
            .ActivateOnEnter<CelestriadFire>()
            .ActivateOnEnter<CelestriadBlizzard>()
            // ── Wings of Destruction ──────────────────────────
            .ActivateOnEnter<WingsOfDestructionTwo>()
            .ActivateOnEnter<WingsOfDestructionOne>()
            // ── Trine ─────────────────────────────────────────
            .ActivateOnEnter<TrineSmall>()
            .ActivateOnEnter<TrineLarge>()
            // ── Past / Future ─────────────────────────────────
            .ActivateOnEnter<PastFuture>()
            .ActivateOnEnter<AllThingsEnding>()
            // ── Forsaken mini-phases ──────────────────────────
            .ActivateOnEnter<PathOfLight>()
            .ActivateOnEnter<AngelHeads>()
            .ActivateOnEnter<Prey>()
            .ActivateOnEnter<PreyPuddles>()
            .ActivateOnEnter<ProximityBlast>()
            .ActivateOnEnter<GravenPulseWave>()
            .ActivateOnEnter<SleepCharm>()
            .ActivateOnEnter<GodStatueOrbBlast>()
            // ── Enrage ────────────────────────────────────────
            .ActivateOnEnter<UltimaEnrage>();
    }
}

// ============================================================
// Module Registration
// ============================================================

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 0,       // TODO: replace with real CFC ID for Sigmascape V4.0 (Savage)
    NameID = 0)]       // TODO: replace with real NameID for God Kefka
public class O8SGodKefka(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.KefkaClone), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.AngelHead), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.StatueGod), ArenaColor.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GodKefka => 1,
                OID.AngelHead => 0, // Angel Heads are removed by collision, not damage
                _ => 0
            };
        }
    }
}
