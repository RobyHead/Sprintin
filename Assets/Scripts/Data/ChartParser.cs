using System.Globalization;
using UnityEngine;

public static class ChartParser
{
    public static ChartData Parse(string text)
    {
        var data = new ChartData();
        var lines = text.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            if (line.StartsWith("@"))
            {
                ParseMeta(line, data);
            }
            else if (line.StartsWith("("))
            {
                ParseBpm(line, data);
            }
            else if (line.StartsWith("["))
            {
                ParseNote(line, data);
            }
            else if (line.StartsWith("{"))
            {
                ParseEffect(line, data);
            }
        }

        return data;
    }

    private static void ParseMeta(string line, ChartData data)
    {
        line = line.TrimStart('@').TrimEnd(';');
        var eqIdx = line.IndexOf('=');
        if (eqIdx < 0)
            return;

        var key = line.Substring(0, eqIdx);
        var value = line.Substring(eqIdx + 1);

        if (key == "name")
            data.name = value;
        else if (key == "bpm")
        {
            data.jumpBpm = float.Parse(value, CultureInfo.InvariantCulture);
        }
        else if (key == "offset")
            data.offset = int.Parse(value, CultureInfo.InvariantCulture);
    }

    private static void ParseBpm(string line, ChartData data)
    {
        line = line.Trim('(', ')', ';');
        var parts = line.Split(',');

        if (parts.Length < 3)
            return;

        int ms = int.Parse(parts[0].Trim());
        float bpm = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
        float beatsPerBar = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);

        var entry = new BpmData(ms + data.offset, bpm, beatsPerBar);
        if (beatsPerBar == 0f)
            data.jumpBpms.Add(entry);
        else
            data.barBpms.Add(entry);
    }

    private static void ParseNote(string line, ChartData data)
    {
        line = line.Trim('[', ']', ';');
        var parts = line.Split(',');

        if (parts.Length < 2)
            return;

        int ms = int.Parse(parts[0].Trim());
        int key = int.Parse(parts[1].Trim());

        if (key == 0)
        {
            data.grounds.Add(new GroundData(ms + data.offset));
        }
        else if (parts.Length >= 3)
        {
            int endMs = int.Parse(parts[2].Trim());
            data.holds.Add(new HoldData(ms + data.offset, key, endMs + data.offset));
        }
        else
        {
            data.taps.Add(new TapData(ms + data.offset, key));
        }
    }

    private static void ParseEffect(string line, ChartData data)
    {
        line = line.Trim('{', '}', ';');
        var parts = line.Split(',');

        if (parts.Length < 2)
            return;

        int ms = int.Parse(parts[0].Trim());
        var effect = new EffectData(ms + data.offset);

        for (int i = 1; i < parts.Length; i++)
        {
            var pair = parts[i].Trim();
            var colonIdx = pair.IndexOf(':');
            if (colonIdx < 0)
                continue;

            var key = pair.Substring(0, colonIdx).Trim();
            var value = pair.Substring(colonIdx + 1).Trim();
            effect.properties[key] = value;
        }

        data.effects.Add(effect);
    }
}