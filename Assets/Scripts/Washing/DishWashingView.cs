using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The close-up washing screen (prefab Resources/DishWashingUI). Hold the left mouse button and
/// move over the plate to scrub. Done / Space scores the bowl; a fully clean bowl finishes itself.
/// </summary>
public class DishWashingView : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private RawImage plateImage;
    [SerializeField] private RawImage dirtImage;
    [SerializeField] private RectTransform sponge;
    [SerializeField] private RectTransform progressFill;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button doneButton;
    [SerializeField] private Button closeButton;

    [Header("Scrubbing (in dirt-grid pixels, grid is 128 wide)")]
    [SerializeField] private float brushRadius = 9f;
    [SerializeField] private int brushStrength = 110;
    [SerializeField] private float brushSpacing = 3f;

    [Header("Feedback")]
    [SerializeField] private Color dirtColor = new Color32(120, 82, 45, 255);
    [SerializeField] private Color dirtSpeckleColor = new Color32(92, 60, 34, 255);
    [SerializeField] private Color dirtyBarColor = new Color32(240, 68, 68, 255);
    [SerializeField] private Color halfBarColor = new Color32(255, 194, 71, 255);
    [SerializeField] private Color cleanBarColor = new Color32(54, 201, 165, 255);
    [SerializeField] private Color cleanTextColor = new Color32(24, 110, 72, 255);
    [SerializeField] private Color dirtyTextColor = new Color32(176, 38, 30, 255);
    [SerializeField] private float feedbackSeconds = 0.9f;
    [SerializeField] private float closeDelayWhenEmpty = 0.8f;

    private Dishwasher dishwasher;
    private DirtMask shownMask;
    private Texture2D dirtTexture;
    private Color32[] dirtPixels;
    private RectTransform dirtRect;
    private bool stroking;
    private int lastFinishFrame = -1;
    private float feedbackUntil;
    private float closeAt = -1f;

    private void Awake()
    {
        dirtRect = dirtImage.rectTransform;
        dirtTexture = new Texture2D(DirtMask.Size, DirtMask.Size, TextureFormat.RGBA32, false);
        dirtTexture.filterMode = FilterMode.Point;
        dirtPixels = new Color32[DirtMask.Size * DirtMask.Size];
        dirtImage.texture = dirtTexture;
        doneButton.onClick.AddListener(OnDoneClicked);
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        Destroy(dirtTexture);
    }

    public void Show(Dishwasher station)
    {
        dishwasher = station;
        shownMask = null;
        stroking = false;
        closeAt = -1f;
        feedbackText.text = "";
        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (feedbackText.text.Length > 0 && Time.unscaledTime >= feedbackUntil)
        {
            feedbackText.text = "";
        }

        if (closeAt >= 0f)
        {
            if (Time.unscaledTime >= closeAt) DishWashingMode.CloseCurrent();
            return;
        }

        Scrub();

        Keyboard keyboard = Keyboard.current;
        bool spacePressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        if (spacePressed || (shownMask != null && shownMask.IsClean))
        {
            FinishBowl();
        }
    }

    private void Scrub()
    {
        Mouse mouse = Mouse.current;
        bool scrubbing = shownMask != null && mouse != null && mouse.leftButton.isPressed;
        sponge.gameObject.SetActive(scrubbing);
        if (!scrubbing)
        {
            stroking = false;
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dirtRect, mouse.position.ReadValue(), null, out Vector2 local))
        {
            return;
        }

        // The sponge is a child of the dirt image, so it shares this local space.
        sponge.anchoredPosition = local;

        Rect rect = dirtRect.rect;
        Vector2 point = new Vector2(
            (local.x - rect.xMin) / rect.width * DirtMask.Size,
            (local.y - rect.yMin) / rect.height * DirtMask.Size);

        bool changed = stroking
            ? shownMask.ContinueStroke(point, brushRadius, brushStrength, brushSpacing)
            : shownMask.BeginStroke(point, brushRadius, brushStrength);
        stroking = true;
        if (changed)
        {
            UploadDirt();
            UpdateProgress();
        }
    }

    private void FinishBowl()
    {
        // One bowl per frame: without this, Space on the frame a bowl auto-finishes would also score the next one.
        if (dishwasher.DirtyBowls == 0 || lastFinishFrame == Time.frameCount) return;
        lastFinishFrame = Time.frameCount;

        int moneyChange = dishwasher.FinishCurrentBowl(out bool clean);
        feedbackText.text = (clean ? "Sạch!" : "Còn bẩn!") + (moneyChange != 0 ? $" {moneyChange:+#;-#}" : "");
        feedbackText.color = clean ? cleanTextColor : dirtyTextColor;
        feedbackUntil = Time.unscaledTime + feedbackSeconds;
        Refresh();
    }

    // Shows the dishwasher's current bowl, or the empty state when none is left.
    private void Refresh()
    {
        shownMask = dishwasher.CurrentMask;
        stroking = false;
        sponge.gameObject.SetActive(false);
        bool hasBowl = shownMask != null;
        plateImage.enabled = hasBowl;
        dirtImage.enabled = hasBowl;
        doneButton.interactable = hasBowl;
        if (hasBowl)
        {
            counterText.text = $"Còn {dishwasher.DirtyBowls} bát trong máy";
            UploadDirt();
        }
        else
        {
            counterText.text = "Hết bát! Ra quán lấy thêm.";
            closeAt = Time.unscaledTime + closeDelayWhenEmpty;
        }
        UpdateProgress();
    }

    private void UploadDirt()
    {
        byte[] values = shownMask.Values;
        Color32 main = dirtColor;
        Color32 speckle = dirtSpeckleColor;
        for (int i = 0; i < values.Length; i++)
        {
            // A fixed scatter of darker pixels so the stain does not look flat.
            Color32 color = ((uint)i * 2654435761u >> 28) < 3 ? speckle : main;
            color.a = values[i];
            dirtPixels[i] = color;
        }
        dirtTexture.SetPixels32(dirtPixels);
        dirtTexture.Apply(false);
    }

    private void UpdateProgress()
    {
        float ratio = shownMask != null ? shownMask.CleanRatio : 0f;
        progressFill.anchorMax = new Vector2(ratio, 1f);
        // Red -> yellow -> green, the same colors as the shift timer.
        progressFillImage.color = ratio < 0.5f
            ? Color.Lerp(dirtyBarColor, halfBarColor, ratio * 2f)
            : Color.Lerp(halfBarColor, cleanBarColor, ratio * 2f - 1f);
    }

    private void OnDoneClicked()
    {
        Deselect();
        FinishBowl();
    }

    private void OnCloseClicked()
    {
        Deselect();
        DishWashingMode.CloseCurrent();
    }

    // E is "Submit" in the UI action map: a button left selected would be pressed again by E.
    private static void Deselect()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
}
