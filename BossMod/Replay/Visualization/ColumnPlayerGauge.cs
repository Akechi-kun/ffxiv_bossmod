using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.ReplayVisualization;

#region Base
public abstract class ColumnPlayerGauge : Timeline.ColumnGroup, IToggleableColumn
{
    public abstract bool Visible { get; set; }
    protected Replay Replay;
    protected Replay.Encounter Encounter;
    protected readonly ColorConfig _colors = Service.Config.Get<ColorConfig>();

    protected Color Good => _colors.PlannerWindow[7];
    protected Color Bad => new(0x8A0000FF);
    public static ColumnPlayerGauge? Create(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass) => playerClass switch
    {
        Class.PLD => new ColumnPlayerGaugePLD(timeline, tree, phaseBranches, replay, enc, player),
        Class.WAR => new ColumnPlayerGaugeWAR(timeline, tree, phaseBranches, replay, enc, player),
        Class.DRK => new ColumnPlayerGaugeDRK(timeline, tree, phaseBranches, replay, enc, player),
        Class.GNB => new ColumnPlayerGaugeGNB(timeline, tree, phaseBranches, replay, enc, player),
        Class.WHM => new ColumnPlayerGaugeWHM(timeline, tree, phaseBranches, replay, enc, player),
        Class.SCH => new ColumnPlayerGaugeSCH(timeline, tree, phaseBranches, replay, enc, player),
        Class.AST => new ColumnPlayerGaugeAST(timeline, tree, phaseBranches, replay, enc, player),
        Class.SGE => new ColumnPlayerGaugeSGE(timeline, tree, phaseBranches, replay, enc, player),
        Class.MNK => new ColumnPlayerGaugeMNK(timeline, tree, phaseBranches, replay, enc, player),
        Class.DRG => new ColumnPlayerGaugeDRG(timeline, tree, phaseBranches, replay, enc, player),
        Class.NIN => new ColumnPlayerGaugeNIN(timeline, tree, phaseBranches, replay, enc, player),
        Class.SAM => new ColumnPlayerGaugeSAM(timeline, tree, phaseBranches, replay, enc, player),
        Class.RPR => new ColumnPlayerGaugeRPR(timeline, tree, phaseBranches, replay, enc, player),
        Class.VPR => new ColumnPlayerGaugeVPR(timeline, tree, phaseBranches, replay, enc, player),
        Class.BRD => new ColumnPlayerGaugeBRD(timeline, tree, phaseBranches, replay, enc, player),
        Class.MCH => new ColumnPlayerGaugeMCH(timeline, tree, phaseBranches, replay, enc, player),
        //Class.DNC => new ColumnPlayerGaugeDNC(timeline, tree, phaseBranches, replay, enc, player),
        //Class.BLM => new ColumnPlayerGaugeBLM(timeline, tree, phaseBranches, replay, enc, player),
        //Class.SMN => new ColumnPlayerGaugeSMN(timeline, tree, phaseBranches, replay, enc, player),
        //Class.RDM => new ColumnPlayerGaugeRDM(timeline, tree, phaseBranches, replay, enc, player),
        //Class.PCT => new ColumnPlayerGaugePCT(timeline, tree, phaseBranches, replay, enc, player),
        _ => null
    };

    protected ColumnPlayerGauge(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline)
    {
        Name = "G.";
        Replay = replay;
        Encounter = enc;
    }

    protected DateTime MinTime() => Encounter.Time.Start.AddSeconds(Timeline.MinTime);

    protected IEnumerable<(DateTime time, T gauge)> EnumerateGauge<T>() where T : unmanaged
    {
        var minTime = MinTime();
        foreach (var frame in Replay.Ops
            .SkipWhile(op => op.Timestamp < minTime)
            .TakeWhile(op => op.Timestamp <= Encounter.Time.End)
            .OfType<WorldState.OpFrameStart>())
        {
            yield return (frame.Timestamp, ClientState.GetGauge<T>(frame.GaugePayload));
        }
    }

    protected void AddRange(
        DateTime from, DateTime to,
        ColumnGenericHistory cgh,
        int curValue, int maxValue,
        string label,
        Func<int, float> height,
        bool condition = true,
        Color? color = null
    )
    {
        if (condition && to > from)
        {
            cgh.AddHistoryEntryRange(Encounter.Time.Start, from, to, label, (curValue >= maxValue && maxValue != 1 ? Bad : (color ?? Good)).ABGR, height(curValue));
        }
    }

    protected void AddCountRange(DateTime from, DateTime to, ColumnGenericHistory cgh, int curCount, int maxCount, float increments, bool condition = true, string label = "Count", Color? color = null)
        => AddRange(from, to, cgh, curCount, maxCount, $"{label}: {curCount}", v => v * increments, condition, color);
    protected void AddGaugeRange(DateTime from, DateTime to, ColumnGenericHistory cgh, int curGauge, int maxGauge = 100, bool condition = true, string label = "Gauge", Color? color = null)
        => AddRange(from, to, cgh, curGauge, maxGauge, $"{label}: {curGauge}", v => v < 10 ? v * 0.02f : v * 0.008f, condition, color);
    protected void AddActiveRange(DateTime from, DateTime to, ColumnGenericHistory cgh, int curState, bool condition = true, string label = "Active", Color? color = null)
        => AddRange(from, to, cgh, curState > 0 ? 1 : 0, 1, $"{label}: {(curState > 0 ? "Active" : "Inactive")}", v => v * 0.9f, condition, color);
}
#endregion

#region PLD
public class ColumnPlayerGaugePLD : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _oath;

    public override bool Visible
    {
        get => _oath.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _oath.Width = width;
        }
    }

    public ColumnPlayerGaugePLD(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _oath = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevOath = 0;
        var prevOathTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<PaladinGauge>())
        {
            var oath = gauge.OathGauge;
            if (oath != prevOath)
            {
                AddOathRange(prevOathTime, time, prevOath);
                prevOath = oath;
                prevOathTime = time;
            }
        }

        AddOathRange(prevOathTime, enc.Time.End, prevOath);
    }

    private void AddOathRange(DateTime from, DateTime to, int oath)
        => AddGaugeRange(from, to, _oath, oath, label: "Oath Gauge", color: new(0xFFFFF6B0));
}
#endregion

#region WAR
public class ColumnPlayerGaugeWAR : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _beast;

    public override bool Visible
    {
        get => _beast.Width > 0;
        set => _beast.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeWAR(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _beast = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevBeast = 0;
        var prevTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<WarriorGauge>())
        {
            if (gauge.BeastGauge != prevBeast)
            {
                AddBeastRange(prevTime, time, prevBeast);
                prevBeast = gauge.BeastGauge;
                prevTime = time;
            }
        }

        AddBeastRange(prevTime, enc.Time.End, prevBeast);
    }

    private void AddBeastRange(DateTime from, DateTime to, int beast)
        => AddGaugeRange(from, to, _beast, beast, label: "Beast Gauge", color: new(0xFFFF7A45));
}
#endregion

#region DRK
public class ColumnPlayerGaugeDRK : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _blood;
    private readonly ColumnGenericHistory _da;
    private readonly ColumnGenericHistory _ds;
    private readonly ColumnGenericHistory _ls;

    public override bool Visible
    {
        get => _blood.Width > 0 || _da.Width > 0 || _ds.Width > 0 || _ls.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _blood.Width = width;
            _da.Width = width;
            _ds.Width = width;
            _ls.Width = width;
        }
    }

    public ColumnPlayerGaugeDRK(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _blood = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _ds = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _ls = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _da = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevBlood = 0;
        var prevBloodTime = MinTime();
        var prevDA = 0;
        var prevDATime = MinTime();
        var prevDSActive = 0;
        var prevDSTimer = 0;
        var prevDSTime = MinTime();
        var prevLSActive = 0;
        var prevLSTimer = 0;
        var prevLSTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<DarkKnightGauge>())
        {
            var blood = gauge.Blood;
            if (blood != prevBlood)
            {
                AddBloodRange(prevBloodTime, time, prevBlood);
                prevBlood = blood;
                prevBloodTime = time;
            }

            var DSTimer = gauge.DarksideTimer;
            var DSActive = DSTimer > 0 ? 1 : 0;
            var DSTimerSec = DSTimer / 1000;
            var prevDSTimerSec = prevDSTimer / 1000;
            if (DSActive != prevDSActive || (DSActive == 1 && DSTimerSec != prevDSTimerSec))
            {
                AddDarksideRange(prevDSTime, time, prevDSActive, prevDSTimer);
                prevDSActive = DSActive;
                prevDSTimer = DSTimer;
                prevDSTime = time;
            }

            var LSTimer = gauge.ShadowTimer;
            var LSActive = LSTimer > 0.0f ? 1 : 0;
            var LSTimerSecs = LSTimer / 1000;
            var prevLSTimerSecs = prevLSTimer / 1000;
            if (LSActive != prevLSActive || (LSActive == 1 && LSTimerSecs != prevLSTimerSecs))
            {
                AddLivingShadowRange(prevLSTime, time, prevLSActive, prevLSTimer);
                prevLSActive = LSActive;
                prevLSTimer = LSTimer;
                prevLSTime = time;
            }

            var da = gauge.DarkArtsState;
            if (da != prevDA)
            {
                AddDarkArtsRange(prevDATime, time, prevDA);
                prevDA = da;
                prevDATime = time;
            }
        }

        AddBloodRange(prevBloodTime, enc.Time.End, prevBlood);
        AddDarksideRange(prevDSTime, enc.Time.End, prevDSActive, prevDSTimer);
        AddLivingShadowRange(prevLSTime, enc.Time.End, prevLSActive, prevLSTimer);
        AddDarkArtsRange(prevDATime, enc.Time.End, prevDA);
    }

    private void AddBloodRange(DateTime from, DateTime to, int blood)
        => AddGaugeRange(from, to, _blood, blood, label: "Blood", color: new(0xAA00D7FF));
    private void AddDarkArtsRange(DateTime from, DateTime to, int da)
        => AddActiveRange(from, to, _da, da, label: "Dark Arts", color: new(0xFF000080));
    private void AddDarksideRange(DateTime from, DateTime to, int active, int timer)
        => AddActiveRange(from, to, _ds, active, label: active != 0 ? $"Darkside ({timer / 1000}s)" : "Darkside", color: new(0xFF800080));
    private void AddLivingShadowRange(DateTime from, DateTime to, int active, int timer)
        => AddActiveRange(from, to, _ls, active, label: active != 0 ? $"Living Shadow ({timer / 1000}s)" : "Living Shadow", color: new(0xAA800080));
}
#endregion

#region GNB
public class ColumnPlayerGaugeGNB : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _ammo;

    public override bool Visible
    {
        get => _ammo.Width > 0;
        set => _ammo.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeGNB(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _ammo = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevAmmo = 0;
        var prevTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<GunbreakerGauge>())
        {
            var ammo = gauge.Ammo;
            if (ammo != prevAmmo)
            {
                AddCartRange(prevTime, time, prevAmmo);
                prevAmmo = ammo;
                prevTime = time;
            }
        }

        AddCartRange(prevTime, enc.Time.End, prevAmmo);
    }

    private void AddCartRange(DateTime from, DateTime to, int ammo)
        => AddCountRange(from, to, _ammo, ammo, 3, 0.31f, label: "Cartridges");
}
#endregion

#region WHM
public class ColumnPlayerGaugeWHM : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _nLily;
    private readonly ColumnGenericHistory _bLily;

    public override bool Visible
    {
        get => _nLily.Width > 0 || _bLily.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _nLily.Width = width;
            _bLily.Width = width;
        }
    }

    public ColumnPlayerGaugeWHM(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _nLily = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _bLily = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevNormalLily = 0;
        var prevNormalTime = MinTime();
        var prevBloodLily = 0;
        var prevBloodTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<WhiteMageGauge>())
        {
            var normalLily = gauge.Lily;
            if (normalLily != prevNormalLily)
            {
                AddNormalLilyRange(prevNormalTime, time, normalLily);
                prevNormalLily = normalLily;
                prevNormalTime = time;
            }
            var bloodLily = gauge.Lily;
            if (bloodLily != prevBloodLily)
            {
                AddBloodLilyRange(prevBloodTime, time, bloodLily);
                prevBloodLily = bloodLily;
                prevBloodTime = time;
            }
        }

        AddNormalLilyRange(prevNormalTime, enc.Time.End, prevNormalLily);
        AddBloodLilyRange(prevBloodTime, enc.Time.End, prevBloodLily);
    }

    private void AddLilyRange(DateTime from, DateTime to, int lily, ColumnGenericHistory cgh, string label, Color? color)
        => AddCountRange(from, to, cgh, lily, 3, 0.31f, label: label, color: color);
    private void AddNormalLilyRange(DateTime from, DateTime to, int lily)
        => AddLilyRange(from, to, lily, _nLily, "Lilies", new(0xFFC7F464));
    private void AddBloodLilyRange(DateTime from, DateTime to, int lily)
        => AddLilyRange(from, to, lily, _bLily, "Blood Lilies", new(0xFF4B0082));
}
#endregion

#region SCH
public class ColumnPlayerGaugeSCH : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _aetherflow;
    private readonly ColumnGenericHistory _faerie;

    public override bool Visible
    {
        get => _aetherflow.Width > 0 || _faerie.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _aetherflow.Width = width;
            _faerie.Width = width;
        }
    }

    public ColumnPlayerGaugeSCH(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _aetherflow = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _faerie = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevAetherflow = 0;
        var prevAetherflowTime = MinTime();
        var prevFaerie = 0;
        var prevFaerieTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<ScholarGauge>())
        {
            var aetherflow = gauge.Aetherflow;
            if (aetherflow != prevAetherflow)
            {
                AddAetherflowRange(prevAetherflowTime, time, aetherflow);
                prevAetherflow = aetherflow;
                prevAetherflowTime = time;
            }
            var faerie = gauge.FairyGauge;
            if (faerie != prevFaerie)
            {
                AddFaerieRange(prevFaerieTime, time, faerie);
                prevFaerie = faerie;
                prevFaerieTime = time;
            }
        }

        AddAetherflowRange(prevAetherflowTime, enc.Time.End, prevAetherflow);
        AddFaerieRange(prevFaerieTime, enc.Time.End, prevFaerie);
    }

    private void AddAetherflowRange(DateTime from, DateTime to, int aetherflow)
        => AddCountRange(from, to, _aetherflow, aetherflow, 3, 0.31f, label: "Aetherflow", color: new(0xFFC7F464));
    private void AddFaerieRange(DateTime from, DateTime to, int faerie)
        => AddGaugeRange(from, to, _faerie, faerie, label: "Faerie Gauge", color: new(0xFF008080));
}
#endregion

#region AST
public class ColumnPlayerGaugeAST : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _cards;
    private readonly ColumnGenericHistory _currentArcana;
    private readonly ColumnGenericHistory _currentCards;
    private readonly ColumnGenericHistory _currentDraw;

    public override bool Visible
    {
        get => _currentArcana.Width > 0 || _currentCards.Width > 0 || _currentDraw.Width > 0 || _cards.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _currentArcana.Width = width;
            _currentCards.Width = width;
            _currentDraw.Width = width;
            _cards.Width = width;
        }
    }

    public ColumnPlayerGaugeAST(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _cards = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _currentArcana = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _currentCards = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _currentDraw = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevCards = 0;
        var prevCardsTime = MinTime();
        var prevCurArcana = default(AstrologianCard);
        var prevCurArcanaTime = MinTime();
        var prevCurCards = default(AstrologianCard[]);
        var prevCurCardsTime = MinTime();
        var prevCurDraw = default(AstrologianDraw);
        var prevCurDrawTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<AstrologianGauge>())
        {
            var cards = gauge.Cards;
            if (cards != prevCards)
            {
                AddCardsRange(prevCardsTime, time, cards);
                prevCards = cards;
                prevCardsTime = time;
            }

            var curArcana = gauge.CurrentArcana;
            if (curArcana != prevCurArcana)
            {
                AddCurArcanaRange(prevCurArcanaTime, time, curArcana);
                prevCurArcana = curArcana;
                prevCurArcanaTime = time;
            }

            var curCards = gauge.CurrentCards;
            if (curCards != prevCurCards)
            {
                AddCurCardsRange(prevCurCardsTime, time, curCards);
                prevCurCards = curCards;
                prevCurCardsTime = time;
            }

            var curDraw = gauge.CurrentDraw;
            if (curDraw != prevCurDraw)
            {
                AddCurDrawRange(prevCurDrawTime, time, curDraw);
                prevCurDraw = curDraw;
                prevCurDrawTime = time;
            }
        }

        AddCardsRange(prevCardsTime, enc.Time.End, prevCards);
        AddCurArcanaRange(prevCurArcanaTime, enc.Time.End, prevCurArcana);
        AddCurCardsRange(prevCurCardsTime, enc.Time.End, prevCurCards);
        AddCurDrawRange(prevCurDrawTime, enc.Time.End, prevCurDraw);
    }

    private void AddCardsRange(DateTime from, DateTime to, int cards)
        => AddCountRange(from, to, _cards, cards, 4, 0.24f, label: "Cards", color: new(0xFFA3FFA9));
    private void AddCurArcanaRange(DateTime from, DateTime to, AstrologianCard arcana)
    {
        if (to > from)
        {
            _currentArcana.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"Current Arcana: {arcana}", Good.ABGR, 0.99f);
        }
    }
    private void AddCurCardsRange(DateTime from, DateTime to, AstrologianCard[]? curCards)
    {
        if (to > from)
        {
            _currentCards.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"Current Cards: {curCards}", Good.ABGR, 0.99f);
        }
    }
    private void AddCurDrawRange(DateTime from, DateTime to, AstrologianDraw draw)
    {
        AstrologianCard card = default;

        var color = card switch
        {
            AstrologianCard.Arrow => Good.ABGR,
            AstrologianCard.Balance => Good.ABGR,
            AstrologianCard.Bole => Good.ABGR,
            AstrologianCard.Ewer => Good.ABGR,
            AstrologianCard.Lady => Good.ABGR,
            AstrologianCard.Lord => Good.ABGR,
            AstrologianCard.Spear => Good.ABGR,
            AstrologianCard.Spire => Good.ABGR,
            _ => Bad.ABGR,
        };
        if (to > from)
        {
            _currentDraw.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"Current Draw: {draw}", color, 0.99f);
        }
    }
}
#endregion

#region SGE
public class ColumnPlayerGaugeSGE : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _addersgall;
    private readonly ColumnGenericHistory _addersting;

    public override bool Visible
    {
        get => _addersgall.Width > 0 || _addersting.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _addersgall.Width = width;
            _addersting.Width = width;
        }
    }

    public ColumnPlayerGaugeSGE(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _addersgall = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _addersting = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevAddersgall = 0;
        var prevAddersgallTimer = 0;
        var prevAddersgallTime = MinTime();
        var prevAddersting = 0;
        var prevAdderstingTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<SageGauge>())
        {
            var addersgall = gauge.Addersgall;
            var addersgallTimer = gauge.AddersgallTimer / 1000;
            if (addersgall != prevAddersgall || addersgallTimer != prevAddersgallTimer)
            {
                AddAddersgallRange(prevAddersgallTime, time, addersgall);
                prevAddersgall = addersgall;
                prevAddersgallTime = time;
            }

            var addersting = gauge.Addersting;
            if (addersting != prevAddersting)
            {
                AddAdderstingRange(prevAdderstingTime, time, addersting);
                prevAddersting = addersting;
                prevAdderstingTime = time;
            }
        }

        AddAddersgallRange(prevAddersgallTime, enc.Time.End, prevAddersgall);
        AddAdderstingRange(prevAdderstingTime, enc.Time.End, prevAddersting);
    }

    private void AddAddersgallRange(DateTime from, DateTime to, int addersgall)
        => AddCountRange(from, to, _addersgall, addersgall, 3, 0.31f, label: "Addersgall", color: new(0xFFC7F464));
    private void AddAdderstingRange(DateTime from, DateTime to, int addersting)
        => AddCountRange(from, to, _addersting, addersting, 3, 0.31f, label: "Addersting", color: new(0xFF008080));
}
#endregion

#region MNK
//TODO: revise
public class ColumnPlayerGaugeMNK : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _chakras;
    private readonly ColumnGenericHistory _beast1;
    private readonly ColumnGenericHistory _beast2;
    private readonly ColumnGenericHistory _beast3;
    private readonly ColumnGenericHistory _nadi;

    public override bool Visible
    {
        get => _chakras.Width > 0 || _beast1.Width > 0 || _beast2.Width > 0 || _beast3.Width > 0 || _nadi.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _chakras.Width = width;
            _beast1.Width = width;
            _beast2.Width = width;
            _beast3.Width = width;
            _nadi.Width = width;
        }
    }
    public ColumnPlayerGaugeMNK(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _chakras = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _chakras.Name = "Chakras";
        _beast1 = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _beast1.Name = "Beast Chakra 1";
        _beast2 = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _beast2.Name = "Beast Chakra 2";
        _beast3 = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _beast3.Name = "Beast Chakra 3";
        _nadi = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _nadi.Name = "Nadi";
        var prevChakras = 0;
        var prevBeast1 = default(BeastChakraType);
        var prevBeast2 = default(BeastChakraType);
        var prevBeast3 = default(BeastChakraType);
        var prevNadi = default(NadiFlags);
        var prevChakraTime = MinTime();
        var prevBeastChakra1Time = MinTime();
        var prevBeastChakra2Time = MinTime();
        var prevBeastChakra3Time = MinTime();
        var prevNadiTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<MonkGauge>())
        {
            if (gauge.Chakra != prevChakras)
            {
                AddChakraRange(_chakras, prevChakraTime, time, prevChakras, _chakras.Name);
                prevChakras = gauge.Chakra;
                prevChakraTime = time;
            }
            if (gauge.BeastChakra1 != prevBeast1)
            {
                AddBeastChakraRange(_beast1, prevBeastChakra1Time, time, prevBeast1, _beast1.Name);
                prevBeast1 = gauge.BeastChakra1;
                prevBeastChakra1Time = time;
            }
            if (gauge.BeastChakra2 != prevBeast2)
            {
                AddBeastChakraRange(_beast2, prevBeastChakra2Time, time, prevBeast2, _beast2.Name);
                prevBeast2 = gauge.BeastChakra2;
                prevBeastChakra2Time = time;
            }
            if (gauge.BeastChakra3 != prevBeast3)
            {
                AddBeastChakraRange(_beast3, prevBeastChakra3Time, time, prevBeast3, _beast3.Name);
                prevBeast3 = gauge.BeastChakra3;
                prevBeastChakra3Time = time;
            }
            if (gauge.Nadi != prevNadi)
            {
                AddNadiRange(_nadi, prevNadiTime, time, prevNadi, _nadi.Name);
                prevNadi = gauge.Nadi;
                prevNadiTime = time;
            }
        }
        AddChakraRange(_chakras, prevChakraTime, enc.Time.End, prevChakras, _chakras.Name);
        AddBeastChakraRange(_beast1, prevBeastChakra1Time, enc.Time.End, prevBeast1, _beast1.Name);
        AddBeastChakraRange(_beast2, prevBeastChakra2Time, enc.Time.End, prevBeast2, _beast2.Name);
        AddBeastChakraRange(_beast3, prevBeastChakra3Time, enc.Time.End, prevBeast3, _beast3.Name);
        AddNadiRange(_nadi, prevNadiTime, enc.Time.End, prevNadi, _nadi.Name);
    }
    private void AddChakraRange(ColumnGenericHistory col, DateTime from, DateTime to, int gauge, string label)
    {
        if (to > from)
        {
            var color = gauge switch
            {
                5 => Bad,
                4 => new(0xFFE8FAFF),
                3 => new(0xFFBEEFFF),
                2 => new(0xFF90E0FF),
                1 => new(0xFFB0E0E6),
                _ => new(0x80808080)
            };
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {gauge}", color.ABGR, gauge * 0.2f);
        }
    }
    private void AddBeastChakraRange(ColumnGenericHistory col, DateTime from, DateTime to, BeastChakraType count, string label)
    {
        if (count != BeastChakraType.None && to > from)
        {
            var color = count == BeastChakraType.None ? Bad : new(0xFF0066FF);
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {count}", color.ABGR);
        }
    }
    private void AddNadiRange(ColumnGenericHistory col, DateTime from, DateTime to, NadiFlags nadi, string label)
    {
        if (to > from)
        {
            var color = nadi switch
            {
                NadiFlags.Solar => new(0xFF90E0FF),
                NadiFlags.Lunar => new(0xFFFFA500),
                0 => new(0x80808080),
                _ => Bad
            };
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {nadi}", color.ABGR, 100);
        }
    }
}
#endregion

#region DRG
public class ColumnPlayerGaugeDRG : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _focus;

    public override bool Visible
    {
        get => _focus.Width > 0;
        set => _focus.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeDRG(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _focus = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevGauge = 0;
        var prevTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<DragoonGauge>())
        {
            var count = gauge.FirstmindsFocusCount;
            if (count != prevGauge)
            {
                AddFocusRange(prevTime, time, prevGauge);
                prevGauge = count;
                prevTime = time;
            }
        }

        AddFocusRange(prevTime, enc.Time.End, prevGauge);
    }

    private void AddFocusRange(DateTime from, DateTime to, int focus)
        => AddCountRange(from, to, _focus, focus, 2, 0.49f, label: "Firstminds' Focus");
}
#endregion

#region NIN
public class ColumnPlayerGaugeNIN : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _ninki;
    private readonly ColumnGenericHistory _kazematoi;

    public override bool Visible
    {
        get => _ninki.Width > 0 || _kazematoi.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _ninki.Width = width;
            _kazematoi.Width = width;
        }
    }

    public ColumnPlayerGaugeNIN(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _ninki = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _kazematoi = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevKazematoi = 0;
        var prevKazematoiTime = MinTime();
        var prevNinki = 0;
        var prevNinkiTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<NinjaGauge>())
        {
            var kazematoi = gauge.Kazematoi;
            if (kazematoi != prevKazematoi)
            {
                AddKazematoiRange(prevKazematoiTime, time, kazematoi);
                prevKazematoi = kazematoi;
                prevKazematoiTime = time;
            }
            var ninki = gauge.Ninki;
            if (ninki != prevNinki)
            {
                AddNinkiRange(prevNinkiTime, time, ninki);
                prevNinki = ninki;
                prevNinkiTime = time;
            }
        }

        AddKazematoiRange(prevKazematoiTime, enc.Time.End, prevKazematoi);
        AddNinkiRange(prevNinkiTime, enc.Time.End, prevNinki);
    }

    private void AddKazematoiRange(DateTime from, DateTime to, int kazematoi)
        => AddCountRange(from, to, _kazematoi, kazematoi, 3, 0.18f, label: "Kazematoi", color: new(0xFFCC99FF));
    private void AddNinkiRange(DateTime from, DateTime to, int ninki)
        => AddGaugeRange(from, to, _ninki, ninki, label: "Ninki", color: new(0xFF007FFF));
}
#endregion

#region SAM
//TODO: revise
public class ColumnPlayerGaugeSAM : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _kenki;
    private readonly ColumnGenericHistory _sen;
    private readonly ColumnGenericHistory _meditation;

    public override bool Visible
    {
        get => _kenki.Width > 0 || _meditation.Width > 0 || _sen.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _kenki.Width = width;
            _sen.Width = width;
            _meditation.Width = width;
        }
    }
    public ColumnPlayerGaugeSAM(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _kenki = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _sen = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _meditation = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevKenki = 0;
        var prevSen = default(SenFlags);
        var prevKa = default(SenFlags);
        var prevGetsu = default(SenFlags);
        var prevSetsu = default(SenFlags);
        var prevMeditation = 0;
        var prevKenkiTime = MinTime();
        var prevSenTime = MinTime();
        var prevKaTime = MinTime();
        var prevGetsuTime = MinTime();
        var prevSetsuTime = MinTime();
        var prevMeditationTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<SamuraiGauge>())
        {
            if (gauge.Kenki != prevKenki)
            {
                AddKenkiRange(prevKenkiTime, time, prevKenki);
                prevKenki = gauge.Kenki;
                prevKenkiTime = time;
            }
            if (gauge.SenFlags != prevSen)
            {
                AddSenRange(prevSenTime, time, prevSen);
                prevSen = gauge.SenFlags;
                prevSenTime = time;
            }
            if (gauge.MeditationStacks != prevMeditation)
            {
                AddMeditationRange(prevMeditationTime, time, prevMeditation);
                prevMeditation = gauge.MeditationStacks;
                prevMeditationTime = time;
            }
        }
        AddKenkiRange(prevKenkiTime, enc.Time.End, prevKenki);
        AddSenRange(prevKaTime, enc.Time.End, prevKa);
        AddSenRange(prevGetsuTime, enc.Time.End, prevGetsu);
        AddSenRange(prevSetsuTime, enc.Time.End, prevSetsu);
        AddMeditationRange(prevMeditationTime, enc.Time.End, prevMeditation);
    }
    private void AddKenkiRange(DateTime from, DateTime to, int Kenki)
    {
        if (Kenki != 0 && to > from)
        {
            var color = Kenki == 100 ? Bad : new(0xFF90E0FF);
            _kenki.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{Kenki} Kenki", color.ABGR, Kenki < 10 ? Kenki * 0.02f : Kenki * 0.01f);
        }
    }
    private int GetSenCount(SenFlags sen)
    {
        var senCount = 0;
        if (sen.HasFlag(SenFlags.Setsu))
            senCount++;
        if (sen.HasFlag(SenFlags.Getsu))
            senCount++;
        if (sen.HasFlag(SenFlags.Ka))
            senCount++;

        return senCount;
    }
    private void AddSenRange(DateTime from, DateTime to, SenFlags sen)
    {
        if (sen != SenFlags.None && to > from)
        {
            var color = GetSenCount(sen) == 3 ? Bad : new(0xFFFFAACC);
            _sen.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{sen}", color.ABGR, GetSenCount(sen) == 3 ? 1f : GetSenCount(sen) == 2 ? 0.6f : GetSenCount(sen) == 1 ? 0.3f : 1f);
        }
    }
    private void AddMeditationRange(DateTime from, DateTime to, int mediStacks)
    {
        if (mediStacks != 0 && to > from)
        {
            var color = mediStacks == 3 ? Bad : new(0xFF8080FF);
            _meditation.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{mediStacks} Meditation stack{(mediStacks == 1 ? "" : "s")}", color.ABGR, mediStacks * 0.31f);
        }
    }
}
#endregion

#region RPR
public class ColumnPlayerGaugeRPR : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _soul;
    private readonly ColumnGenericHistory _shroud;
    private readonly ColumnGenericHistory _enshroud;

    public override bool Visible
    {
        get => _soul.Width > 0 || _shroud.Width > 0 || _enshroud.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _soul.Width = width;
            _shroud.Width = width;
            _enshroud.Width = width;
        }
    }

    public ColumnPlayerGaugeRPR(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _soul = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _shroud = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _enshroud = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevSoul = 0;
        var prevSoulTime = MinTime();
        var prevShroud = 0;
        var prevShroudTime = MinTime();
        var prevEnshroudActive = 0;
        var prevEnshroudTimeLeft = 0;
        var prevEnshroudTime = MinTime();
        var prevLemureShroud = 0;
        var prevVoidShroud = 0;

        foreach (var (time, gauge) in EnumerateGauge<ReaperGauge>())
        {
            var soul = gauge.Soul;
            if (soul != prevSoul)
            {
                AddSoulRange(prevSoulTime, time, prevSoul);
                prevSoul = soul;
                prevSoulTime = time;
            }

            var shroud = gauge.Shroud;
            if (soul != prevShroud)
            {
                AddShroudRange(prevShroudTime, time, prevShroud);
                prevShroud = soul;
                prevShroudTime = time;
            }

            var enshroudTimeLeft = gauge.EnshroudedTimeRemaining / 1000;
            var enshroudActive = enshroudTimeLeft > 0 ? 1 : 0;
            var lemures = gauge.LemureShroud;
            var voids = gauge.VoidShroud;
            if (enshroudActive != prevEnshroudActive ||
                (enshroudActive == 1 &&
                    (enshroudTimeLeft != prevEnshroudTimeLeft || lemures != prevLemureShroud || voids != prevVoidShroud)))
            {
                AddEnshroudRange(prevEnshroudTime, time, prevEnshroudActive, prevEnshroudTimeLeft, prevLemureShroud, prevVoidShroud);
                prevEnshroudActive = enshroudActive;
                prevEnshroudTimeLeft = enshroudTimeLeft;
                prevEnshroudTime = time;
            }
        }

        AddSoulRange(prevSoulTime, enc.Time.End, prevSoul);
        AddShroudRange(prevShroudTime, enc.Time.End, prevShroud);
        AddEnshroudRange(prevEnshroudTime, enc.Time.End, prevEnshroudActive, prevEnshroudTimeLeft, prevLemureShroud, prevVoidShroud);
    }

    private void AddSoulRange(DateTime from, DateTime to, int soul)
        => AddGaugeRange(from, to, _soul, soul, label: "Soul Gauge", color: new(0xAA00D7FF));
    private void AddShroudRange(DateTime from, DateTime to, int shroud)
        => AddGaugeRange(from, to, _shroud, shroud, label: "Shroud Gauge", color: new(0xAA00D7FF));
    private void AddEnshroudRange(DateTime from, DateTime to, int active, int timer, int lemures, int voids)
        => AddActiveRange(from, to, _enshroud, active, label: active != 0 ? $"Enshroud ({timer / 1000}s, {lemures}, {voids})" : "Enshroud", color: new(0xFF800080));
}
#endregion

#region VPR
public class ColumnPlayerGaugeVPR : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _rc;
    private readonly ColumnGenericHistory _so;

    public override bool Visible
    {
        get => _rc.Width > 0 || _so.Width > 0;
        set => _rc.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeVPR(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _rc = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _so = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevRC = 0;
        var prevSO = 0;
        var prevRCTime = MinTime();
        var prevSOTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<ViperGauge>())
        {
            var rc = gauge.RattlingCoilStacks;
            if (rc != prevRC)
            {
                AddRattlingCoilRange(prevRCTime, time, prevRC);
                prevRC = rc;
                prevRCTime = time;
            }

            var so = gauge.SerpentOffering;
            if (so != prevSO)
            {
                AddSerpentOfferingRange(prevSOTime, time, prevSO);
                prevSO = so;
                prevSOTime = time;
            }
        }

        AddRattlingCoilRange(prevRCTime, enc.Time.End, prevRC);
        AddSerpentOfferingRange(prevSOTime, enc.Time.End, prevSO);
    }

    private void AddRattlingCoilRange(DateTime from, DateTime to, int rc)
        => AddCountRange(from, to, _rc, rc, 3, 0.31f, color: new(0xFF3A7AC8), label: "Rattling Coils");
    private void AddSerpentOfferingRange(DateTime from, DateTime to, int so)
        => AddGaugeRange(from, to, _so, so, color: new(0xFFE8CFA8), label: "Serpent Offering");
}
#endregion

#region BRD
//TODO: revise
public class ColumnPlayerGaugeBRD : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _songs;
    private readonly ColumnGenericHistory _soul;

    public override bool Visible
    {
        get => _songs.Width > 0;
        set => _songs.Width = _soul.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeBRD(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _songs = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _soul = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevSong = (default(SongFlags), 0);
        var prevSoul = 0;
        var prevSongTime = MinTime();
        var prevSoulTime = prevSongTime;
        var prevSongStartTimer = 0.0f;
        foreach (var (time, gauge) in EnumerateGauge<BardGauge>())
        {
            if ((gauge.SongFlags, gauge.Repertoire) != prevSong)
            {
                AddSongRange(prevSongTime, time, prevSong.Item1, prevSong.Item2, prevSongStartTimer);
                prevSong = (gauge.SongFlags, gauge.Repertoire);
                prevSongTime = time;
                prevSongStartTimer = gauge.SongTimer * 0.001f;
            }
            if (gauge.SoulVoice != prevSoul)
            {
                AddSoulRange(prevSoulTime, time, prevSoul);
                prevSoul = gauge.SoulVoice;
                prevSoulTime = time;
            }
        }
        AddSongRange(prevSongTime, enc.Time.End, prevSong.Item1, prevSong.Item2, prevSongStartTimer);
        AddSoulRange(prevSoulTime, enc.Time.End, prevSoul);
    }

    private void AddSongRange(DateTime from, DateTime to, SongFlags song, int repertoire, float timer)
    {
        if (song != SongFlags.None && to > from)
        {
            var (color, scale) = (song & SongFlags.WanderersMinuet) switch
            {
                SongFlags.MagesBallad => (_colors.PlannerWindow[0], 1),
                SongFlags.ArmysPaeon => (_colors.PlannerWindow[1], 0.2f),
                SongFlags.WanderersMinuet => (Bad, 0.25f),
                _ => (new(0x80808080), 1),
            };
            var e = _songs.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{song}, {repertoire} rep, {timer:f3} at start", color.ABGR, (1 + repertoire) * scale);
            e.TooltipExtra = (res, t) =>
            {
                var delta = t - (from - Encounter.Time.Start).TotalSeconds;
                var remaining = timer - delta;
                res.Add($"- time since start: {45 - remaining:f3}");
                res.Add($"- remaining: {remaining:f3}");
            };
        }
    }

    private void AddSoulRange(DateTime from, DateTime to, int soulVoice)
    {
        if (soulVoice != 0 && to > from)
        {
            _soul.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{soulVoice} voice", 0x80808080, soulVoice * 0.01f);
        }
    }
}
#endregion

#region MCH
public class ColumnPlayerGaugeMCH : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _heat;
    private readonly ColumnGenericHistory _battery;

    public override bool Visible
    {
        get => _heat.Width > 0 || _battery.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _heat.Width = width;
            _battery.Width = width;
        }
    }

    public ColumnPlayerGaugeMCH(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _heat = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _battery = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevHeat = 0;
        var prevBattery = 0;
        var prevHeatTime = MinTime();
        var prevBatteryTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<MachinistGauge>())
        {
            var heat = gauge.Heat;
            if (heat != prevHeat)
            {
                AddHeatRange(prevHeatTime, time, prevHeat);
                prevHeat = heat;
                prevHeatTime = time;
            }

            var battery = gauge.Battery;
            if (battery != prevBattery)
            {
                AddHeatRange(prevHeatTime, time, prevHeat);
                prevBattery = battery;
                prevBatteryTime = time;
            }
        }
        AddHeatRange(prevHeatTime, enc.Time.End, prevHeat);
        AddBatteryRange(prevBatteryTime, enc.Time.End, prevBattery);
    }

    private void AddHeatRange(DateTime from, DateTime to, int heat)
        => AddGaugeRange(from, to, _heat, heat, label: "Heat", color: new(0xFF90E0FF));
    private void AddBatteryRange(DateTime from, DateTime to, int battery)
        => AddGaugeRange(from, to, _battery, battery, label: "Battery", color: new(0xFFFFA500));
}
#endregion

#region DNC
public class ColumnPlayerGaugeDNC : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _ff;
    private readonly ColumnGenericHistory _espirit;

    public override bool Visible
    {
        get => _ff.Width > 0 || _espirit.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _ff.Width = width;
            _espirit.Width = width;
        }
    }

    public ColumnPlayerGaugeDNC(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _ff = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _espirit = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevFF = 0;
        var prevEspirit = 0;
        var prevFFTime = MinTime();
        var prevEspiritTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<DancerGauge>())
        {
            var ff = gauge.Feathers;
            if (ff != prevFF)
            {
                AddFeatherRange(prevFFTime, time, prevFF);
                prevFF = ff;
                prevFFTime = time;
            }

            var espirit = gauge.Esprit;
            if (espirit != prevEspirit)
            {
                AddEspiritRange(prevEspiritTime, time, prevEspirit);
                prevEspirit = espirit;
                prevEspiritTime = time;
            }
        }

        AddFeatherRange(prevFFTime, enc.Time.End, prevFF);
        AddEspiritRange(prevEspiritTime, enc.Time.End, prevEspirit);
    }

    private void AddFeatherRange(DateTime from, DateTime to, int ff)
        => AddCountRange(from, to, _ff, ff, 4, 0.22f, color: new(0xFF80FFFF), label: "Feathers");
    private void AddEspiritRange(DateTime from, DateTime to, int espirit)
        => AddGaugeRange(from, to, _espirit, espirit, color: new(0xFF80C0FF), label: "Espirit");
}
#endregion

#region BLM
public class ColumnPlayerGaugeBLM : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _element;
    private readonly ColumnGenericHistory _polyglot;
    private readonly ColumnGenericHistory _paradox;
    private readonly ColumnGenericHistory _souls;

    public override bool Visible
    {
        get => _element.Width > 0 || _polyglot.Width > 0 || _paradox.Width > 0 || _souls.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _element.Width = width;
            _polyglot.Width = width;
            _paradox.Width = width;
            _souls.Width = width;
        }
    }

    public ColumnPlayerGaugeBLM(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _element = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _polyglot = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _paradox = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _souls = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevFire = 0;
        var prevIce = 0;
        var prevHearts = 0;
        var prevElementActive = 0;
        var prevElementTime = MinTime();

        var prevEnochianTimer = 0;
        var prevEnochianActive = 0;
        var prevPolyglot = 0;
        var prevPolyglotTime = MinTime();

        var prevParadox = 0;
        var prevParadoxTime = MinTime();

        var prevSouls = 0;
        var prevSoulsTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<BlackMageGauge>())
        {
            var enochianTimer = gauge.EnochianTimer / 1000;
            var enochianActive = gauge.EnochianActive ? 1 : 0;
            var polyglot = gauge.PolyglotStacks;
            if (polyglot != prevPolyglot ||
                enochianActive != prevEnochianActive ||
                (enochianActive == 1 && enochianTimer != prevEnochianTimer))
            {
                AddPolyglotRange(prevPolyglotTime, time, prevEnochianActive, prevEnochianTimer, prevPolyglot);
                prevEnochianActive = enochianActive;
                prevEnochianTimer = enochianTimer;
                prevPolyglotTime = time;
            }

            var element = gauge.ElementStance;
            var isFire = element is (1 or 2 or 3) and not (0 or -1 or -2 or -3);
            var isIce = element is (-1 or -2 or -3) and not (0 or 1 or 2 or 3);
            var elementActive = isIce ? 2 : isFire ? 1 : 0;
            var fire = gauge.AstralStacks;
            var ice = gauge.UmbralStacks;
            var hearts = gauge.UmbralHearts;
            if (elementActive != prevElementActive ||
                hearts != prevHearts ||
                (elementActive == 1 && fire != prevFire) ||
                (elementActive == 2 && ice != prevIce))
            {
                AddElementRange(prevElementTime, time, prevElementActive, prevFire, prevIce);
                prevElementActive = elementActive;
                prevFire = fire;
                prevIce = ice;
                prevElementTime = time;
            }

            var paradox = gauge.ParadoxActive ? 1 : 0;
            if (paradox != prevParadox)
            {
                AddParadoxRange(prevParadoxTime, enc.Time.End, prevParadox);
                prevParadox = paradox;
                prevParadoxTime = time;
            }

            var souls = gauge.AstralSoulStacks;
            if (souls != prevSouls)
            {
                AddSoulsRange(prevSoulsTime, time, prevSouls);
                prevSouls = souls;
                prevSoulsTime = time;
            }
        }

        AddElementRange(prevElementTime, enc.Time.End, prevElementActive, prevFire, prevIce);
        AddPolyglotRange(prevPolyglotTime, enc.Time.End, prevEnochianActive, prevEnochianTimer, prevPolyglot);
        AddParadoxRange(prevParadoxTime, enc.Time.End, prevParadox);
        AddSoulsRange(prevSoulsTime, enc.Time.End, prevSouls);
    }

    private void AddElementRange(DateTime from, DateTime to, int active, int fire, int ice)
        => AddActiveRange(from, to, _element, active,
            label: active == 2 ? $"Ice (UI{ice})" : active == 1 ? $"Fire (AF{fire})" : "Element",
            color: active == 2 ? new(0xFFFFE0A0) : active == 1 ? new(0xFF003CFF) : Bad);
    private void AddPolyglotRange(DateTime from, DateTime to, int active, int timer, int stacks)
        => AddActiveRange(from, to, _polyglot, active, label: active != 0 ? $"Polyglots: {stacks} - Enochian ({timer / 1000}s)" : $"Polyglots: {stacks} - Enochian", color: new(0xFF800080));
    private void AddParadoxRange(DateTime from, DateTime to, int active)
        => AddActiveRange(from, to, _paradox, active, label: "Paradox", color: new(0xFFE6B8E6));
    private void AddSoulsRange(DateTime from, DateTime to, int souls)
        => AddCountRange(from, to, _souls, souls, 6, 0.12f, label: "Astral Souls", color: new(0xFF003CFF));
}
#endregion

#region SMN
// TODO: add SMN gauge
#endregion

#region RDM
// TODO: add RDM gauge
#endregion

#region PCT
// TODO: add PCT gauge
#endregion
