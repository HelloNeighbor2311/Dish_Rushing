/// <summary>Kinds of owner remarks. A lower number is more important.</summary>
public enum BarkKind { Broken = 0, Dirty = 1, PileUp = 2, TimeLow = 3, CleanStreak = 4 }

/// <summary>
/// Decides when the owner may speak: at most one remark per global cooldown, the same kind at
/// most once per its own cooldown, and the most important pending request wins. Plain C#.
/// </summary>
public sealed class BarkScheduler
{
    private const int KindCount = 5;

    private readonly float globalCooldown;
    private readonly float perKindCooldown;
    private readonly bool[] pending = new bool[KindCount];
    private readonly float[] lastSpoken = new float[KindCount];
    private float lastAny = float.NegativeInfinity;

    public BarkScheduler(float globalCooldown, float perKindCooldown)
    {
        this.globalCooldown = globalCooldown;
        this.perKindCooldown = perKindCooldown;
        for (int i = 0; i < KindCount; i++) lastSpoken[i] = float.NegativeInfinity;
    }

    public void Request(BarkKind kind)
    {
        pending[(int)kind] = true;
    }

    /// <summary>
    /// Returns the remark to say now, if any. Taking one clears all other pending requests so
    /// stale remarks are not said long after the event.
    /// </summary>
    public bool TryTake(float now, out BarkKind kind)
    {
        kind = default;
        if (now - lastAny < globalCooldown) return false;

        for (int i = 0; i < KindCount; i++)
        {
            if (!pending[i] || now - lastSpoken[i] < perKindCooldown) continue;

            kind = (BarkKind)i;
            lastSpoken[i] = now;
            lastAny = now;
            System.Array.Clear(pending, 0, KindCount);
            return true;
        }
        return false;
    }
}
