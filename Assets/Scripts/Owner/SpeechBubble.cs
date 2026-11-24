using TMPro;
using UnityEngine;

/// <summary>
/// World-space speech bubble next to a character's head. Grows to the right from its own
/// position so it stays on screen when the character is near the top of the view.
/// Sized to its text and hides itself after a while.
/// </summary>
public sealed class SpeechBubble : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private Vector2 padding = new Vector2(0.35f, 0.3f);
    [SerializeField] private float maxTextWidth = 4.5f;

    private float hideAt;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Say(string line, float seconds = 3f)
    {
        gameObject.SetActive(true);
        text.text = line;
        Vector2 size = text.GetPreferredValues(line, maxTextWidth, 0f);
        size.x = Mathf.Min(size.x, maxTextWidth);
        text.rectTransform.sizeDelta = size;
        background.size = size + padding * 2f;
        float halfWidth = background.size.x / 2f;
        background.transform.localPosition = new Vector3(halfWidth, 0f, 0f);
        text.transform.localPosition = new Vector3(halfWidth, text.transform.localPosition.y, 0f);
        hideAt = Time.time + seconds;
    }

    private void Update()
    {
        if (Time.time >= hideAt) gameObject.SetActive(false);
    }
}
