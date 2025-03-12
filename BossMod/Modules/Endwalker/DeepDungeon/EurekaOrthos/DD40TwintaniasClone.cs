namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD40TwintaniasClone;

public enum OID : uint
{
    Boss = 0x3D1D, // R6.0
    Twister = 0x1E8910, // R0.5
    BitingWind = 0x3D1E, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    TwisterVisual = 31468, // Boss->self, 5.0s cast, single-target
    TwisterTouch = 31470, // Helper->player, no cast, single-target, player got hit by twister
    MeracydianCyclone = 31462, // Boss->self, 3.0s cast, single-target
    Gust = 31463, // Helper->location, 4.0s cast, range 5 circle
    MeracydianSquallVisual = 31465, // Boss->self, 3.0s cast, single-target
    MeracydianSquall = 31466, // Helper->location, 5.0s cast, range 5 circle
    BitingWind = 31464, // BitingWind->self, no cast, range 5 circle
    Turbine = 31467, // Boss->self, 6.0s cast, range 60 circle, knockback 15, away from source
    TwistingDive = 31471 // Boss->self, 5.0s cast, range 50 width 15 rect
}

class Twister(BossModule module) : Components.CastTwister(module, 1.5f, (uint)OID.Twister, ActionID.MakeSpell(AID.TwisterVisual), 0.4f, 0.25f);
class BitingWind(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5f, ActionID.MakeSpell(AID.Gust), GetVoidzones, 0.9f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.BitingWind);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class MeracydianSquall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MeracydianSquall), 5f);

class TwistersHint(BossModule module, AID aid) : Components.CastHint(module, ActionID.MakeSpell(aid), "Twisters soon, get moving!");
class Twisters1(BossModule module) : TwistersHint(module, AID.TwisterVisual);
class Twisters2(BossModule module) : TwistersHint(module, AID.TwistingDive);
class DiveTwister(BossModule module) : Components.CastTwister(module, 1.5f, (uint)OID.Twister, ActionID.MakeSpell(AID.TwistingDive), 0.4f, 0.25f);

class TwistingDive(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(50f, 7.5f);
    private bool preparing;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor == Module.PrimaryActor)
        {
            if (id == 0x1E3A)
                preparing = true;
            else if (preparing && id == 0x1E43)
                _aoe = new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(6.9d));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TwistingDive)
        {
            _aoe = null;
            preparing = false;
        }
    }
}

class Turbine(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Turbine), 15f)
{
    private readonly BitingWind _aoe = module.FindComponent<BitingWind>()!;
    private static readonly Angle a20 = 20f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Casters.Count != 0 ? Casters[0] : null;
        if (source != null)
        {
            var component = _aoe.Sources(Module).ToList();
            var forbidden = new List<Func<WPos, float>>
            {
                ShapeDistance.InvertedCircle(Arena.Center, 5f)
            };
            for (var i = 0; i < component.Count; ++i)
                forbidden.Add(ShapeDistance.Cone(Arena.Center, 20f, Angle.FromDirection(component[i].Position - Arena.Center), a20));
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), Module.CastFinishAt(source.CastInfo));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        foreach (var aoe in _aoe.ActiveAOEs(slot, actor))
        {
            if (aoe.Check(pos))
                return true;
        }
        return !Arena.InBounds(pos);
    }
}

class DD40TwintaniasCloneStates : StateMachineBuilder
{
    public DD40TwintaniasCloneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Twister>()
            .ActivateOnEnter<Twisters1>()
            .ActivateOnEnter<Twisters2>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<MeracydianSquall>()
            .ActivateOnEnter<Turbine>()
            .ActivateOnEnter<TwistingDive>()
            .ActivateOnEnter<DiveTwister>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 900, NameID = 12263)]
public class DD40TwintaniasClone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600f, -300f), new ArenaBoundsCircle(20f));
