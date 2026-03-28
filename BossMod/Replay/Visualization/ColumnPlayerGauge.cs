using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.ReplayVisualization;

#region Base
public abstract class ColumnPlayerGauge : Timeline.ColumnGroup, IToggleableColumn
{
    public abstract bool Visible { get; set; }
    protected Replay Replay;
    protected Replay.Encounter Encounter;
    protected readonly ColorConfig _colors = Service.Config.Get<ColorConfig>();
    protected Color Yellow = new(0xFF6AB0B8); //yellow as default color for any gauge present
    protected Color Red = new(0xFF202080); //red as default color for any gauge full

    public static ColumnPlayerGauge? Create(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass) => playerClass switch
    {
        Class.PLD => new ColumnPlayerGaugePLD(timeline, tree, phaseBranches, replay, enc, player),
        Class.WAR => new ColumnPlayerGaugeWAR(timeline, tree, phaseBranches, replay, enc, player),
        Class.DRK => new ColumnPlayerGaugeDRK(timeline, tree, phaseBranches, replay, enc, player),
        Class.GNB => new ColumnPlayerGaugeGNB(timeline, tree, phaseBranches, replay, enc, player),
        Class.WHM => new ColumnPlayerGaugeWHM(timeline, tree, phaseBranches, replay, enc, player),
        Class.SCH => new ColumnPlayerGaugeSCH(timeline, tree, phaseBranches, replay, enc, player),
        //Class.AST => new ColumnPlayerGaugeAST(timeline, tree, phaseBranches, replay, enc, player),
        //Class.SGE => new ColumnPlayerGaugeSGE(timeline, tree, phaseBranches, replay, enc, player),
        Class.MNK => new ColumnPlayerGaugeMNK(timeline, tree, phaseBranches, replay, enc, player),
        Class.DRG => new ColumnPlayerGaugeDRG(timeline, tree, phaseBranches, replay, enc, player),
        //Class.NIN => new ColumnPlayerGaugeNIN(timeline, tree, phaseBranches, replay, enc, player),
        Class.SAM => new ColumnPlayerGaugeSAM(timeline, tree, phaseBranches, replay, enc, player),
        //Class.RPR => new ColumnPlayerGaugeRPR(timeline, tree, phaseBranches, replay, enc, player),
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
        Name = "G";
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
        Color? curColor = null, Color? fullColor = null)
    {
        var color = curValue >= maxValue ? (fullColor ?? Red) : (curColor ?? Yellow);
        if (condition && to > from)
        {
            cgh.AddHistoryEntryRange(Encounter.Time.Start, from, to, label, color.ABGR, height(curValue));
        }
    }

    protected void AddCountRange(DateTime from, DateTime to, ColumnGenericHistory cgh, int curCount, int maxCount, float increments, bool condition = true, string label = "Count", Color? curColor = null, Color? maxColor = null)
        => AddRange(from, to, cgh, curCount, maxCount, $"{label}: {curCount}", v => v * increments, condition, curColor, maxColor);
    protected void AddGaugeRange(DateTime from, DateTime to, ColumnGenericHistory cgh, int curGauge, int maxGauge = 100, bool condition = true, string label = "Gauge", Color? curColor = null, Color? maxColor = null)
        => AddRange(from, to, cgh, curGauge, maxGauge, $"{label}: {curGauge}", v => v < 10 ? v * 0.02f : v * 0.01f, condition, curColor, maxColor);
    protected void AddTimerRange(DateTime from, DateTime to, ColumnGenericHistory cgh, int curTimer, int maxTimer = 60, bool condition = true, string label = "Gauge Timer", Color? color = null)
        => AddRange(from, to, cgh, (int)(curTimer * 0.001f), (int)(maxTimer * 0.001f), $"{label} Active - Current Duration: {curTimer}", v => v * 0.99f, condition, color);
    protected void AddActiveRange(DateTime from, DateTime to, ColumnGenericHistory cgh, int curState, int activeState = 1, bool condition = true, string label = "Active", Color? color = null)
        => AddRange(from, to, cgh, curState, activeState, $"{label} Active", v => v * 0.99f, condition, color);
    protected void AddCardRange(DateTime from, DateTime to, ColumnGenericHistory cgh, AstrologianCard curCard, bool condition = true, string label = "Card", Color? curColor = null)
    {
        if (condition && to > from)
        {
            cgh.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {curCard}", (color ?? Yellow).ABGR, curCard != default ? 0.99f : 0);
        }
    }
}
#endregion

#region PLD
public class ColumnPlayerGaugePLD : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _oath;
    private readonly ColumnGenericHistory _ironwill;

    public override bool Visible
    {
        get => _oath.Width > 0 || _ironwill.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _oath.Width = width;
            _ironwill.Width = width;
        }
    }

    public ColumnPlayerGaugePLD(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _oath = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _ironwill = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

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
        => AddGaugeRange(from, to, _oath, oath, label: "Oath Gauge", curColor: new(0xFFFFF6B0));
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
        => AddGaugeRange(from, to, _beast, beast, label: "Beast Gauge", curColor: new(0xFFFF7A45));
}
#endregion

#region DRK
public class ColumnPlayerGaugeDRK : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _blood;
    private readonly ColumnGenericHistory _darkarts;
    private readonly ColumnGenericHistory _darkside;
    private readonly ColumnGenericHistory _livingshadow;

    public override bool Visible
    {
        get => _blood.Width > 0 || _darkarts.Width > 0 || _darkside.Width > 0 || _livingshadow.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _blood.Width = width;
            _darkarts.Width = width;
            _darkside.Width = width;
            _livingshadow.Width = width;
        }
    }

    public ColumnPlayerGaugeDRK(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _blood = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _darkarts = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _darkside = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _livingshadow = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevBlood = 0;
        var prevBloodTime = MinTime();
        var prevDA = 0;
        var prevDATime = MinTime();
        var prevDS = 0;
        var prevDSTime = MinTime();
        var prevLS = 0;
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

            var da = gauge.DarkArtsState;
            if (da != prevDA)
            {
                AddDarkArtsRange(prevDATime, time, prevDA);
                prevDA = da;
                prevDATime = time;
            }

            var ds = gauge.DarksideTimer * 0.001f;
            if (ds != prevDS)
            {
                AddDarksideRange(prevDSTime, time, prevDS);
                prevDS = (int)ds;
                prevDSTime = time;
            }

            var ls = gauge.ShadowTimer * 0.001f;
            if (ls != prevLS)
            {
                AddLivingShadowRange(prevLSTime, time, prevLS);
                prevLS = (int)ls;
                prevLSTime = time;
            }
        }

        AddBloodRange(prevBloodTime, enc.Time.End, prevBlood);
        AddDarkArtsRange(prevDATime, enc.Time.End, prevDA);
        AddDarksideRange(prevDSTime, enc.Time.End, prevDS);
        AddLivingShadowRange(prevLSTime, enc.Time.End, prevLS);
    }

    private void AddBloodRange(DateTime from, DateTime to, int blood)
        => AddGaugeRange(from, to, _blood, blood, label: "Blood", curColor: new(0xFFFFF6B0));
    private void AddDarkArtsRange(DateTime from, DateTime to, int da)
        => AddActiveRange(from, to, _darkarts, da, label: "Dark Arts", color: new(0xFF331F66));
    private void AddDarksideRange(DateTime from, DateTime to, int ds)
        => AddTimerRange(from, to, _darkside, ds, label: "Darkside", color: new(0xFF7F4DFF));
    private void AddLivingShadowRange(DateTime from, DateTime to, int ls)
        => AddTimerRange(from, to, _livingshadow, ls, label: "Living Shadow", color: new(0xFF66445E));
}
#endregion

#region GNB
public class ColumnPlayerGaugeGNB : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _carts;

    public override bool Visible
    {
        get => _carts.Width > 0;
        set => _carts.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeGNB(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _carts = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevGauge = 0;
        var prevTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<GunbreakerGauge>())
        {
            var ammo = gauge.Ammo;
            if (ammo != prevGauge)
            {
                AddCartRange(prevTime, time, prevGauge);
                prevGauge = ammo;
                prevTime = time;
            }
        }

        AddCartRange(prevTime, enc.Time.End, prevGauge);
    }

    private void AddCartRange(DateTime from, DateTime to, int carts)
        => AddCountRange(from, to, _carts, carts, 3, 0.31f, label: "Cartridges");
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

    private void AddLilyRange(DateTime from, DateTime to, int lily, ColumnGenericHistory cgh, string label, Color? curColor)
        => AddCountRange(from, to, cgh, lily, 3, 0.31f, label: label, curColor: curColor);
    private void AddNormalLilyRange(DateTime from, DateTime to, int lily)
        => AddLilyRange(from, to, lily, _nLily, "Lilies", new(0xFF87E7FF));
    private void AddBloodLilyRange(DateTime from, DateTime to, int lily)
        => AddLilyRange(from, to, lily, _bLily, "Blood Lilies", new(0xFFFF637D));
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
        => AddCountRange(from, to, _aetherflow, aetherflow, 3, 0.31f, label: "Aetherflow", curColor: new(0xFFA3FFA9));
    private void AddFaerieRange(DateTime from, DateTime to, int faerie)
        => AddGaugeRange(from, to, _faerie, faerie, label: "Faerie Gauge", curColor: new(0xFFFFFD94));
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

        var prevCard = 0;
        var prevCardTime = MinTime();
        var prevCurArcana = default(AstrologianCard);
        var prevCurArcanaTime = MinTime();
        var prevCurCards = default(AstrologianCard);
        var prevCurCardsTime = MinTime();
        var prevCurDraw = default(AstrologianCard);
        var prevCurDrawTime = MinTime();

        foreach (var (time, gauge) in EnumerateGauge<AstrologianGauge>())
        {
            var cards = gauge.Cards;
            if (cards != prevCard)
            {
                AddAetherflowRange(prevCardTime, time, cards);
                prevCard = cards;
                prevCardTime = time;
            }
            var curArcana = gauge.CurrentArcana;
            if (curArcana != prevCurArcana)
            {
                AddArcanaRange(prevCurArcanaTime, time, curArcana);
                prevCurArcana = curArcana;
                prevCurArcanaTime = time;
            }
            var curCards = gauge.Cards;
            if (curCards != prevCurCards)
            {
                AddArcanaRange(prevCurArcanaTime, time, curCards);
                prevCurArcana = curCards;
                prevCurArcanaTime = time;
            }
        }

        AddAetherflowRange(prevCardTime, enc.Time.End, prevCard);
        AddArcanaRange(prevCurArcanaTime, enc.Time.End, prevCurArcana);
        AddCardsRange(prevCurCardsTime, enc.Time.End, prevCurCards);
        AddDrawRange(prevCurDrawTime, enc.Time.End, prevCurDraw);
    }

    private void AddAetherflowRange(DateTime from, DateTime to, int aetherflow)
        => AddCountRange(from, to, _cards, aetherflow, 3, 0.31f, label: "Aetherflow", curColor: new(0xFFA3FFA9));
    private void AddArcanaRange(DateTime from, DateTime to, AstrologianCard arcana)
        => AddCardRange(from, to, _currentArcana, arcana, label: "Current Arcana", curColor: new(0xFFFFFD94));
    private void AddCardsRange(DateTime from, DateTime to, AstrologianCard card)
        => AddCountRange(from, to, _currentArcana, card, label: "Current Card", curColor: new(0xFFFFFD94));
    private void AddDrawRange(DateTime from, DateTime to, AstrologianCard draw)
        => AddCardRange(from, to, _currentArcana, draw, label: "Current Draw", curColor: new(0xFFFFFD94));
}
#endregion

#region SGE
// TODO: add SGE gauge
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
                5 => Red,
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
            var color = count == BeastChakraType.None ? Red : new(0xFF0066FF);
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
                _ => Red
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

    private void AddFocusRange(DateTime from, DateTime to, int focus) => AddCountRange(from, to, _focus, focus, 2, 0.49f, label: "Firstminds' Focus");
}
#endregion

#region NIN
// TODO: add NIN gauge
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
            var color = Kenki == 100 ? Red : new(0xFF90E0FF);
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
            var color = GetSenCount(sen) == 3 ? Red : new(0xFFFFAACC);
            _sen.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{sen}", color.ABGR, GetSenCount(sen) == 3 ? 1f : GetSenCount(sen) == 2 ? 0.6f : GetSenCount(sen) == 1 ? 0.3f : 1f);
        }
    }
    private void AddMeditationRange(DateTime from, DateTime to, int mediStacks)
    {
        if (mediStacks != 0 && to > from)
        {
            var color = mediStacks == 3 ? Red : new(0xFF8080FF);
            _meditation.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{mediStacks} Meditation stack{(mediStacks == 1 ? "" : "s")}", color.ABGR, mediStacks * 0.31f);
        }
    }
}
#endregion

#region RPR
// TODO: add RPR gauge
#endregion

#region VPR
public class ColumnPlayerGaugeVPR : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _coils;
    private readonly ColumnGenericHistory _offerings;

    public override bool Visible
    {
        get => _coils.Width > 0 || _offerings.Width > 0;
        set => _coils.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeVPR(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _coils = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _offerings = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevCoil = 0;
        var prevOffering = 0;
        var prevCoilTime = MinTime();
        var prevOfferingTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<ViperGauge>())
        {
            var coil = gauge.RattlingCoilStacks;
            if (coil != prevCoil)
            {
                AddCoilRange(prevCoilTime, time, prevCoil);
                prevCoil = coil;
                prevCoilTime = time;
            }

            var offering = gauge.SerpentOffering;
            if (offering != prevOffering)
            {
                AddOfferingsRange(prevOfferingTime, time, prevOffering);
                prevOffering = offering;
                prevOfferingTime = time;
            }
        }

        AddCoilRange(prevCoilTime, enc.Time.End, prevCoil);
        AddOfferingsRange(prevOfferingTime, enc.Time.End, prevOffering);
    }

    private void AddCoilRange(DateTime from, DateTime to, int coils)
        => AddCountRange(from, to, _coils, coils, 3, 0.31f, curColor: new(0xFF3A7AC8), label: "Rattling Coils");
    private void AddOfferingsRange(DateTime from, DateTime to, int offerings)
        => AddGaugeRange(from, to, _offerings, offerings, curColor: new(0xFFE8CFA8), label: "Serpent Offering");
}
#endregion

#region BRD
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
                SongFlags.WanderersMinuet => (Red, 0.25f),
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
        _heat.Name = "Heat";
        _battery = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _battery.Name = "Battery";

        var prevHeat = 0;
        var prevBattery = 0;
        var prevHeatTime = MinTime();
        var prevBatteryTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<MachinistGauge>())
        {
            if (gauge.Heat != prevHeat)
            {
                AddGaugeRange(_heat, prevHeatTime, time, prevHeat, _heat.Name);
                prevHeat = gauge.Heat;
                prevHeatTime = time;
            }
            if (gauge.Battery != prevBattery)
            {
                AddGaugeRange(_battery, prevBatteryTime, time, prevBattery, _battery.Name);
                prevBattery = gauge.Battery;
                prevBatteryTime = time;
            }
        }
        AddGaugeRange(_heat, prevHeatTime, enc.Time.End, prevHeat, _heat.Name);
        AddGaugeRange(_battery, prevBatteryTime, enc.Time.End, prevBattery, _battery.Name);
    }

    private void AddGaugeRange(ColumnGenericHistory col, DateTime from, DateTime to, int gauge, string label)
    {
        if (to > from)
        {
            var color = (gauge == 100) ? Red : label switch
            {
                "Heat" => new(0xFF90E0FF),
                "Battery" => new(0xFFFFA500),
                _ => new(0x08080808)
            };
            var width = gauge < 10 ? gauge * 0.02f : gauge * 0.01f; //if Heat = 5, it will not show on Visualizer, instead showing as 0.. so here's a small hack-around to make it visible
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {gauge}", color.ABGR, width);
        }
    }
}
#endregion

#region DNC
// TODO: add DNC gauge
#endregion

#region BLM
// TODO: add BLM gauge
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
