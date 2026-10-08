public class StretchData
{
    public int ms;
    public int endms;
    public float multiplier;
    public string easing;

    public StretchData(int ms, int endms, float multiplier, string easing)
    {
        this.ms = ms;
        this.endms = endms;
        this.multiplier = multiplier;
        this.easing = easing;
    }
}