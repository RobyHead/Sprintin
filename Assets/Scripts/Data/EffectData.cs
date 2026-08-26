using System.Collections.Generic;

public class EffectData
{
    public int ms;
    public Dictionary<string, string> properties = new Dictionary<string, string>();

    public EffectData(int ms)
    {
        this.ms = ms;
    }
}