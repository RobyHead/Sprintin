using System.Collections.Generic;

public class ChartData
{
    public string name;
    public float jumpBpm = 120f;
    public float barBpm = 0f;
    public int offset = 0;

    public List<BpmData> jumpBpms = new List<BpmData>();
    public List<BpmData> barBpms = new List<BpmData>();
    public List<TapData> taps = new List<TapData>();
    public List<HoldData> holds = new List<HoldData>();
    public List<GroundData> grounds = new List<GroundData>();
    public List<EffectData> effects = new List<EffectData>();
    public List<SpeedData> speeds = new List<SpeedData>();
    public List<TextEffectData> textEffects = new List<TextEffectData>();
}