using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button ExitBtn;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject exitConfirmationPanel;
    private CanvasGroup pauseCanvasGroup;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private bool cursorStateCaptured;

    private void Awake()
    {
        pauseCanvasGroup = GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetPauseMenuVisible(false);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        if (continueBtn != null)
            continueBtn.onClick.AddListener(ResumeGame);
        if (settingBtn != null)
            settingBtn.onClick.AddListener(OpenSettings);
        if (ExitBtn != null)
            ExitBtn.onClick.AddListener(ShowExitConfirmation);

        if (exitConfirmationPanel != null)
        {
            Button cancelButton = exitConfirmationPanel.transform.Find("CancelButton")?.GetComponent<Button>();
            Button confirmButton = exitConfirmationPanel.transform.Find("ConfirmButton")?.GetComponent<Button>();
            if (cancelButton != null && cancelButton.onClick.GetPersistentEventCount() == 0)
                cancelButton.onClick.AddListener(CancelExit);
            if (confirmButton != null && confirmButton.onClick.GetPersistentEventCount() == 0)
                confirmButton.onClick.AddListener(ConfirmExit);
        }
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("PauseUI requires a GameManager in the scene.", this);
            return;
        }

        GameManager.Instance.onGamePause += OnGamePaused;
        GameManager.Instance.onGameUnPause += OnGameUnPaused;

        StylePauseMenu();
    }

    private void Update()
    {
        // Keep the HUD pause icon usable even if PlayerInput has disabled the
        // UI action map after returning to gameplay.
        if (pauseButton == null || !pauseButton.gameObject.activeInHierarchy ||
            !pauseButton.interactable || Time.timeScale <= 0f)
            return;

        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        RectTransform buttonRect = pauseButton.transform as RectTransform;
        if (buttonRect == null) return;

        Canvas canvas = pauseButton.GetComponentInParent<Canvas>();
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        if (RectTransformUtility.RectangleContainsScreenPoint(
                buttonRect, mouse.position.ReadValue(), eventCamera))
            PauseGame();
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(PauseGame);
        if (continueBtn != null)
            continueBtn.onClick.RemoveListener(ResumeGame);
        if (settingBtn != null)
            settingBtn.onClick.RemoveListener(OpenSettings);
        if (ExitBtn != null)
            ExitBtn.onClick.RemoveListener(ShowExitConfirmation);

        if (exitConfirmationPanel != null)
        {
            Button cancelButton = exitConfirmationPanel.transform.Find("CancelButton")?.GetComponent<Button>();
            Button confirmButton = exitConfirmationPanel.transform.Find("ConfirmButton")?.GetComponent<Button>();
            if (cancelButton != null && cancelButton.onClick.GetPersistentEventCount() == 0)
                cancelButton.onClick.RemoveListener(CancelExit);
            if (confirmButton != null && confirmButton.onClick.GetPersistentEventCount() == 0)
                confirmButton.onClick.RemoveListener(ConfirmExit);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.onGamePause -= OnGamePaused;
            GameManager.Instance.onGameUnPause -= OnGameUnPaused;
        }
    }

    private void PauseGame()
    {
        if (GameManager.Instance != null && Time.timeScale > 0f)
            GameManager.Instance.PauseGame();
    }

    private void ResumeGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnPauseGame();
    }

    private void StylePauseMenu()
    {
        RectTransform panel = continueBtn != null ? continueBtn.transform.parent as RectTransform : null;
        if (panel == null) return;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color32(25, 74, 65, 255);
            Sprite panelSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Sliced;
            }
            if (panel.GetComponent<Shadow>() == null)
            {
                var shadow = panel.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.32f);
                shadow.effectDistance = new Vector2(0f, -8f);
            }
        }

        Transform background = transform.Find("Background");
        Image dimmer = background != null ? background.GetComponent<Image>() : null;
        if (dimmer != null) dimmer.color = new Color(0.025f, 0.04f, 0.035f, 0.76f);

        RectTransform titleRect = panel.Find("Title") as RectTransform;
        if (titleRect != null)
        {
            Image placeholder = titleRect.GetComponent<Image>();
            if (placeholder != null) placeholder.enabled = false;
            titleRect.anchoredPosition = new Vector2(0f, 166f);
            titleRect.sizeDelta = new Vector2(440f, 68f);
            TMP_Text title = titleRect.GetComponent<TMP_Text>();
            if (title == null) title = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
            title.text = "PAUSED";
            title.font = GetButtonFont();
            title.fontSize = 54f;
            title.fontStyle = FontStyles.Bold;
            title.color = new Color32(255, 211, 112, 255);
            title.alignment = TextAlignmentOptions.Center;
            title.raycastTarget = false;
        }

        AddSubtitle(panel);
        AddHeaderDivider(panel);
        StyleButton(continueBtn, "RESUME", new Vector2(0f, 30f),
            new Color32(63, 145, 89, 255), Color.white);
        StyleButton(settingBtn, "SETTINGS", new Vector2(0f, -53f),
            new Color32(235, 174, 67, 255), new Color32(47, 53, 46, 255));
        StyleButton(ExitBtn, "MAIN MENU", new Vector2(0f, -136f),
            new Color32(183, 76, 59, 255), Color.white);
        StyleExitConfirmation();
    }

    private void AddHeaderDivider(RectTransform panel)
    {
        if (panel.Find("HeaderDivider") != null) return;
        var dividerObject = new GameObject("HeaderDivider", typeof(RectTransform), typeof(Image));
        var rect = dividerObject.GetComponent<RectTransform>();
        rect.SetParent(panel, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 99f);
        rect.sizeDelta = new Vector2(320f, 3f);
        var divider = dividerObject.GetComponent<Image>();
        divider.color = new Color32(255, 211, 112, 210);
        divider.raycastTarget = false;
    }

    private void StyleExitConfirmation()
    {
        if (exitConfirmationPanel == null) return;
        RectTransform modal = exitConfirmationPanel.transform as RectTransform;
        if (modal == null) return;

        Image modalImage = modal.GetComponent<Image>();
        if (modalImage != null)
        {
            modalImage.color = new Color32(25, 74, 65, 255);
            Sprite panelSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            if (panelSprite != null)
            {
                modalImage.sprite = panelSprite;
                modalImage.type = Image.Type.Sliced;
            }
        }

        Transform promptTransform = modal.Find("ExitConfirmationLabel");
        if (promptTransform != null)
        {
            RectTransform promptRect = promptTransform as RectTransform;
            if (promptRect != null)
            {
                promptRect.anchoredPosition = new Vector2(0f, 95f);
                promptRect.sizeDelta = new Vector2(680f, 100f);
            }
            TMP_Text prompt = promptTransform.GetComponent<TMP_Text>();
            if (prompt != null)
            {
                prompt.text = "Return to the main menu?\nYour current shift will be lost.";
                prompt.color = new Color32(255, 246, 224, 255);
                prompt.font = GetButtonFont();
                prompt.fontSize = 30f;
                prompt.fontStyle = FontStyles.Bold;
                prompt.alignment = TextAlignmentOptions.Center;
                prompt.raycastTarget = false;
            }
        }

        Button cancel = modal.Find("CancelButton") != null
            ? modal.Find("CancelButton").GetComponent<Button>() : null;
        Button confirm = modal.Find("ConfirmButton") != null
            ? modal.Find("ConfirmButton").GetComponent<Button>() : null;
        StyleButton(cancel, "CANCEL", new Vector2(-145f, -115f),
            new Color32(63, 145, 89, 255), Color.white);
        StyleButton(confirm, "RETURN TO MENU", new Vector2(145f, -115f),
            new Color32(183, 76, 59, 255), Color.white);
    }

    private void AddSubtitle(RectTransform panel)
    {
        Transform existing = panel.Find("PauseSubtitle");
        if (existing != null) return;

        var subtitleObject = new GameObject("PauseSubtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        var rect = subtitleObject.GetComponent<RectTransform>();
        rect.SetParent(panel, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 119f);
        rect.sizeDelta = new Vector2(430f, 32f);
        var subtitle = subtitleObject.GetComponent<TextMeshProUGUI>();
        subtitle.text = "Take a breather. Your shift is paused.";
        subtitle.font = GetButtonFont();
        subtitle.fontSize = 19f;
        subtitle.color = new Color32(225, 236, 218, 255);
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.raycastTarget = false;
    }

    private TMP_FontAsset GetButtonFont()
    {
        if (continueBtn != null)
        {
            TMP_Text buttonText = continueBtn.GetComponentInChildren<TMP_Text>();
            if (buttonText != null && buttonText.font != null) return buttonText.font;
        }
        return TMP_Settings.defaultFontAsset;
    }

    private void StyleButton(Button button, string label, Vector2 position, Color baseColor, Color textColor)
    {
        if (button == null) return;
        RectTransform rect = button.transform as RectTransform;
        if (rect != null)
        {
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(320f, 64f);
        }

        Image image = button.targetGraphic as Image;
        if (image != null) image.color = Color.white;
        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.16f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.2f);
        colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.45f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.12f;
        button.colors = colors;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
            text.color = textColor;
            text.font = GetButtonFont();
            text.fontSize = 25f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }
    }
    
    private void OnGameUnPaused()
    {
        if (InputModeManager.instance != null)
            InputModeManager.instance.EnterGameplayMode();
        if (cursorStateCaptured)
        {
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
            cursorStateCaptured = false;
        }
        SetPauseMenuVisible(false);
    }

    private void OnGamePaused()
    {
        if (InputModeManager.instance != null)
            InputModeManager.instance.EnterUIMode();
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        cursorStateCaptured = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetPauseMenuVisible(true);
    }

    private void SetPauseMenuVisible(bool visible)
    {
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null) return;

        pauseCanvasGroup.alpha = visible ? 1f : 0f;
        pauseCanvasGroup.interactable = visible;
        pauseCanvasGroup.blocksRaycasts = visible;
    }
    public void OpenSettings()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
    }

    // Quay lại Pause Menu
    public void BackFromSettings()
    {
        settingsPanel.SetActive(false);
        SetPauseMenuVisible(true);
    }
    private void ShowExitConfirmation()
    {
        if (exitConfirmationPanel == null) return;
        exitConfirmationPanel.SetActive(true);
        exitConfirmationPanel.transform.SetAsLastSibling();
    }

    public void CancelExit()
    {
        exitConfirmationPanel.SetActive(false);
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneLoader.LoadScene(SceneLoader.SceneEnum.MainMenu);
    }
}
