namespace BossMod.Stormblood.Savage.O8S1Kefka;

class FlagrantFire(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true)
{
    private enum Pattern
    {
        None,
        Stack,
        Spread
    }

    private Pattern _pattern;
    private Actor? _target;
    private bool _inverted;
    private bool _haveInversion;
    private bool _charged;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.JestersTruths:
                _inverted = false;
                _haveInversion = true;
                TryResolve();
                break;

            case SID.JestersAntics:
                _inverted = true;
                _haveInversion = true;
                TryResolve();
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
            case IconID.FireStack:
                _pattern = Pattern.Stack;
                _target = actor;
                break;

            case IconID.FireSpread:
                _pattern = Pattern.Spread;
                _target = null;
                break;

            default:
                return;
        }

        TryResolve();
    }

    private void TryResolve()
    {
        if (!_haveInversion || _pattern == Pattern.None || _charged)
            return;

        var resolved = _inverted
            ? (_pattern == Pattern.Stack ? Pattern.Spread : Pattern.Stack)
            : _pattern;

        switch (resolved)
        {
            case Pattern.Stack:
                if (_target != null)
                    Stacks.Add(new(_target, 6f, minSize: 8, activation: WorldState.FutureTime(5)));
                break;

            case Pattern.Spread:
                foreach (var p in Raid.WithoutSlot())
                    Spreads.Add(new(p, 6f, WorldState.FutureTime(5)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ManaRelease && _charged)
        {
            TryResolve();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlagrantFire1:
            case AID.FlagrantFire2:
            case AID.FlagrantFire3:
            case AID.FlagrantFire4:
                Stacks.Clear();
                Spreads.Clear();
                break;

            case AID.ManaRelease:
                _charged = false;
                TryResolve();
                break;
        }
    }
}
