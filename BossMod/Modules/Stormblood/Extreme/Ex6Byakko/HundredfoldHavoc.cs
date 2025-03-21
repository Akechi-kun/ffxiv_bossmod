﻿namespace BossMod.Stormblood.Extreme.Ex6Byakko;

class HundredfoldHavoc(BossModule module) : Components.Exaflare(module, 5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HundredfoldHavocFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 5 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 4, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HundredfoldHavocFirst or (uint)AID.HundredfoldHavocRest)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}
