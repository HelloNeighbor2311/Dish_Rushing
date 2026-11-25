using UnityEngine;

/// <summary>
/// Bouncing arrow above the tutorial's current target, plus an arrow on the screen edge
/// (from TutorialHud) when the target is off screen.
/// </summary>
public sealed class TutorialPointer : MonoBehaviour
{
    [SerializeField] private Transform worldArrow;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobPeriod = 1f;
    [SerializeField, Range(0f, 0.45f)] private float edgeMargin = 0.05f;

    private Transform target;
    private float height;
    private RectTransform edgeArrow;

    private void Awake()
    {
        worldArrow.gameObject.SetActive(false);
    }

    public void Point(Transform newTarget, float heightAboveTarget = 1f)
    {
        target = newTarget;
        height = heightAboveTarget;
        if (edgeArrow == null) edgeArrow = TutorialHud.Get().EdgeArrow;
    }

    public void Clear()
    {
        target = null;
        worldArrow.gameObject.SetActive(false);
        if (edgeArrow != null) edgeArrow.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        // A target destroyed elsewhere (e.g. a dish that broke) simply stops being pointed at.
        if (target == null)
        {
            if (worldArrow.gameObject.activeSelf) Clear();
            return;
        }

        float bob = Mathf.Sin(Time.time * Mathf.PI * 2f / Mathf.Max(0.1f, bobPeriod)) * bobAmplitude;
        worldArrow.position = target.position + new Vector3(0f, height + bob, 0f);
        worldArrow.gameObject.SetActive(true);

        Camera view = Camera.main;
        if (view == null || edgeArrow == null) return;
        Vector2 viewport = view.WorldToViewportPoint(target.position);
        bool onScreen = EdgePointerMath.Place(viewport, edgeMargin, out Vector2 clamped, out float angle);
        edgeArrow.gameObject.SetActive(!onScreen);
        if (onScreen) return;
        edgeArrow.anchorMin = edgeArrow.anchorMax = clamped;
        edgeArrow.anchoredPosition = Vector2.zero;
        // The arrow sprite points down (-90 degrees).
        edgeArrow.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }
}
