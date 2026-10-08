using System.Collections.Generic;

public class ChartData
{
    public int offset = 0;
    public int end = 0;

    public List<BpmData> jumpBpms = new List<BpmData>();
    public List<BpmData> barBpms = new List<BpmData>();
    public List<TapData> taps = new List<TapData>();
    public List<HoldData> holds = new List<HoldData>();
    public List<GroundData> grounds = new List<GroundData>();
    public List<EffectData> effects = new List<EffectData>();
    public List<SpeedData> speeds = new List<SpeedData>();
    public List<StretchData> stretchs = new List<StretchData>();
    public List<TextEffectData> textEffects = new List<TextEffectData>();
}