using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class ChartParser
{
    public static ChartData Parse(string text)
    {
        var data = new ChartData();
        var lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            int lineNum = i + 1;

            try
            {
                if (line.StartsWith("@"))
                    ParseMeta(line, lineNum, data);
                else if (line.StartsWith("("))
                    ParseBpm(line, lineNum, data);
                else if (line.StartsWith("["))
                    ParseNote(line, lineNum, data);
                else if (line.StartsWith("{"))
                    ParseEffect(line, lineNum, data);
                else
                    LogError(lineNum, $"unrecognized statement: '{line}'");
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                LogError(lineNum, $"numeric parse failed: {ex.Message}");
            }
        }

        MarkDualNotes(data);

        return data;
    }

    private static void MarkDualNotes(ChartData data)
    {
        var msCount = new Dictionary<int, int>();

        foreach (var t in data.taps)
        {
            if (!msCount.ContainsKey(t.ms))
                msCount[t.ms] = 0;
            msCount[t.ms]++;
        }

        foreach (var h in data.holds)
        {
            if (!msCount.ContainsKey(h.ms))
                msCount[h.ms] = 0;
            msCount[h.ms]++;
        }

        foreach (var t in data.taps)
        {
            if (msCount.TryGetValue(t.ms, out int count) && count >= 2)
                t.isDual = true;
        }

        foreach (var h in data.holds)
        {
            if (msCount.TryGetValue(h.ms, out int count) && count >= 2)
                h.isDual = true;
        }
    }

    private static void ParseMeta(string line, int lineNum, ChartData data)
    {
        line = line.TrimStart('@').TrimEnd(';');
        var eqIdx = line.IndexOf('=');
        if (eqIdx < 0) { LogError(lineNum, "missing '='"); return; }

        var key = line.Substring(0, eqIdx);
        var value = line.Substring(eqIdx + 1);

        if (key == "offset")
            data.offset = int.Parse(value, CultureInfo.InvariantCulture);
        else if (key == "end")
            data.end = int.Parse(value, CultureInfo.InvariantCulture);
        else
            LogError(lineNum, $"unknown meta key: '{key}'");
    }

    private static void ParseBpm(string line, int lineNum, ChartData data)
    {
        var parts = Tokenize(line.Trim('(', ')', ';'));
        if (parts.Count != 3) { LogError(lineNum, $"bpm needs 3 values, got {parts.Count}"); return; }

        int ms = int.Parse(parts[0]);
        float bpm = float.Parse(parts[1], CultureInfo.InvariantCulture);
        float beatsPerBar = float.Parse(parts[2], CultureInfo.InvariantCulture);

        var entry = new BpmData(ms, bpm, beatsPerBar);
        if (beatsPerBar == 0f)
            data.jumpBpms.Add(entry);
        else
            data.barBpms.Add(entry);
    }

    private static void ParseNote(string line, int lineNum, ChartData data)
    {
        var parts = Tokenize(line.Trim('[', ']', ';'));
        if (parts.Count != 2 && parts.Count != 3) { LogError(lineNum, $"note needs 2 or 3 values, got {parts.Count}"); return; }

        int ms = int.Parse(parts[0]);
        int key = int.Parse(parts[1]);

        if (key == 0)
        {
            data.grounds.Add(new GroundData(ms));
        }
        else if (key >= 1 && key <= 4)
        {
            if (parts.Count == 3)
                data.holds.Add(new HoldData(ms, key, int.Parse(parts[2])));
            else
                data.taps.Add(new TapData(ms, key));
        }
        else
        {
            LogError(lineNum, $"note key out of range (0-4): {key}");
        }
    }

    private static void ParseEffect(string line, int lineNum, ChartData data)
    {
        var parts = Tokenize(line.Trim('{', '}', ';'));
        if (parts.Count < 2) { LogError(lineNum, $"effect needs 2+ values, got {parts.Count}"); return; }

        int ms = int.Parse(parts[0]);
        string effectName = parts[1];

        if (effectName == "speed")
        {
            if (parts.Count != 3) { LogError(lineNum, $"speed effect needs 3 values, got {parts.Count}"); return; }
            data.speeds.Add(new SpeedData(ms, float.Parse(parts[2], CultureInfo.InvariantCulture)));
        }
        else if (effectName == "text")
        {
            if (parts.Count != 6) { LogError(lineNum, $"text effect needs 6 values, got {parts.Count}"); return; }
            data.textEffects.Add(new TextEffectData
            {
                ms = ms,
                fadeInMs = int.Parse(parts[2]),
                holdMs = int.Parse(parts[3]),
                fadeOutMs = int.Parse(parts[4]),
                content = parts[5]
            });
        }
        else
        {
            LogError(lineNum, $"unknown effect name: '{effectName}'");
        }
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                tokens.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString().Trim());

        return tokens;
    }

    private static void LogError(int lineNum, string message)
    {
        Debug.LogError($"ChartParser [line {lineNum}]: {message}");
    }
}