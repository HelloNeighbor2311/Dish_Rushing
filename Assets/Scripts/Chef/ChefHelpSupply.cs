using System;

/// <summary>One claim per scheduled window; collected drinks survive the window expiring.</summary>
public sealed class ChefHelpSupply
{
    private readonly float[] times;
    private readonly float duration;
    private int next;
    private float expiresAt;
    public bool Available { get; private set; }
    public int Drinks { get; private set; }

    public ChefHelpSupply(float[] offerTimes, float offerDuration)
    {
        times = offerTimes == null ? Array.Empty<float>() : (float[])offerTimes.Clone();
        Array.Sort(times);
        duration = Math.Max(0f, offerDuration);
    }

    public void Tick(float shiftSeconds)
    {
        if (Available && shiftSeconds >= expiresAt) Available = false;
        while (next < times.Length && shiftSeconds >= times[next])
        {
            expiresAt = times[next++] + duration;
            Available = shiftSeconds < expiresAt;
        }
    }

    public bool Claim()
    {
        if (!Available) return false;
        Available = false;
        Drinks++;
        return true;
    }

    public bool Consume()
    {
        if (Drinks <= 0) return false;
        Drinks--;
        return true;
    }
}
