using UnityEngine;

/// <summary>Where to draw an arrow that points at an off-screen target. Plain math, in viewport space.</summary>
public static class EdgePointerMath
{
    /// <summary>
    /// Returns true when the target is on screen (viewport inside [0,1]); the point is then unchanged.
    /// Otherwise the point is moved toward the screen centre until it lies inside
    /// [margin, 1 - margin], and angleDeg is the direction from the centre to the target
    /// (0 = right, 90 = up).
    /// </summary>
    public static bool Place(Vector2 viewport, float margin, out Vector2 clampedViewport, out float angleDeg)
    {
        Vector2 center = new Vector2(0.5f, 0.5f);
        Vector2 direction = viewport - center;
        angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bool onScreen = viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        if (onScreen)
        {
            clampedViewport = viewport;
            return true;
        }

        float half = 0.5f - margin;
        float scaleX = Mathf.Abs(direction.x) > 1e-6f ? half / Mathf.Abs(direction.x) : float.MaxValue;
        float scaleY = Mathf.Abs(direction.y) > 1e-6f ? half / Mathf.Abs(direction.y) : float.MaxValue;
        clampedViewport = center + direction * Mathf.Min(scaleX, scaleY);
        return false;
    }
}
