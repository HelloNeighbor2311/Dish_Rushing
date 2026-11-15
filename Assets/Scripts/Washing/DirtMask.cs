using UnityEngine;

/// <summary>
/// Dirt on one bowl: a Size x Size grid of dirt amounts (0 = clean, 255 = fully dirty).
/// Plain C# so it can be tested without Play Mode. Coordinates are in grid pixels.
/// </summary>
public sealed class DirtMask
{
    public const int Size = 128;
    // Dirt only appears inside this radius, so it always sits on the plate.
    public const float PlateRadius = Size * 0.36f;
    // Below this share of the starting dirt the bowl counts as fully clean.
    private const float CleanSnap = 0.02f;

    private readonly byte[] values = new byte[Size * Size];
    private long initialDirt;
    private long remainingDirt;
    private Vector2 lastPoint;
    private float distanceCarry;

    public byte[] Values => values;
    public float CleanRatio => initialDirt == 0 ? 1f : 1f - (float)remainingDirt / initialDirt;
    public bool IsClean => remainingDirt == 0;

    public static DirtMask Generate(System.Random random, int spots)
    {
        var mask = new DirtMask();
        Vector2 center = new Vector2(Size / 2f, Size / 2f);
        for (int i = 0; i < spots; i++)
        {
            float radius = 9f + (float)random.NextDouble() * 9f;
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float distance = Mathf.Sqrt((float)random.NextDouble()) * (PlateRadius - radius);
            Vector2 spot = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            mask.Stamp(spot, radius, 255);
        }
        mask.initialDirt = mask.remainingDirt;
        return mask;
    }

    /// <summary>Starts a new stroke at this point (mouse button just went down).</summary>
    public bool BeginStroke(Vector2 point, float radius, int strength)
    {
        lastPoint = point;
        distanceCarry = 0f;
        bool changed = Stamp(point, radius, -strength);
        SnapIfNearlyClean();
        return changed;
    }

    /// <summary>
    /// Continues the stroke to this point. Erases once per `spacing` pixels travelled, so a still
    /// mouse cleans nothing and the result does not depend on frame rate.
    /// </summary>
    public bool ContinueStroke(Vector2 point, float radius, int strength, float spacing)
    {
        Vector2 delta = point - lastPoint;
        float length = delta.magnitude;
        bool changed = false;
        float travelled = spacing - distanceCarry;
        while (travelled <= length)
        {
            changed |= Stamp(lastPoint + delta * (travelled / length), radius, -strength);
            travelled += spacing;
        }
        distanceCarry = length - (travelled - spacing);
        lastPoint = point;
        SnapIfNearlyClean();
        return changed;
    }

    // Tiny leftovers are invisible and tedious to find, so wipe them.
    private void SnapIfNearlyClean()
    {
        if (remainingDirt == 0 || remainingDirt >= initialDirt * CleanSnap) return;
        System.Array.Clear(values, 0, values.Length);
        remainingDirt = 0;
    }

    // Adds (amount > 0) or removes (amount < 0) dirt in a circle. Returns true if anything changed.
    private bool Stamp(Vector2 center, float radius, int amount)
    {
        bool changed = false;
        int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius));
        int maxX = Mathf.Min(Size - 1, Mathf.CeilToInt(center.x + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius));
        int maxY = Mathf.Min(Size - 1, Mathf.CeilToInt(center.y + radius));
        float radiusSquared = radius * radius;
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                if (dx * dx + dy * dy > radiusSquared) continue;

                int index = y * Size + x;
                int before = values[index];
                int after = Mathf.Clamp(before + amount, 0, 255);
                if (after == before) continue;

                values[index] = (byte)after;
                remainingDirt += after - before;
                changed = true;
            }
        }
        return changed;
    }
}
