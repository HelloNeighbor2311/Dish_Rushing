using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the end-of-shift result without changing the existing gameplay systems.</summary>
public sealed class GameOverUI : MonoBehaviour
{
    private enum FailureReason
    {
        None,
        TimeExpired,
        TooManyBrokenBowls
    }

    [Header("Regular shift rules")]
    [SerializeField, Min(1)] private int requiredCleanBowls = 10;
    [SerializeField, Min(0)] private int allowedBrokenBowls = 3;
    [SerializeField] private CountDownTime shiftTimer;

    private int cleanBowls;
    private bool isGameOver;
    private GameObject overlay;
    private TMP_FontAsset uiFont;
    private TMP_Text titleText;
    private TMP_Text summaryText;
    private AudioSource resultAudio;
    private AudioClip winClip;
    private AudioClip loseClip;
    private Sprite fireworkSprite;
    private RectTransform fireworksLayer;
    private Coroutine fireworksRoutine;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameOverUI requires a GameManager in the scene.", this);
            enabled = false;
            return;
        }

        if (shiftTimer == null)
            shiftTimer = FindFirstObjectByType<CountDownTime>();

        GameManager.Instance.BowlWashedEvent += OnBowlWashed;
        GameManager.Instance.BowlBrokenEvent += OnBowlsBroken;
        if (shiftTimer != null)
            shiftTimer.Finished += OnTimeFinished;
        else
            Debug.LogError("GameOverUI could not find the shift timer.", this);

        CreateOverlay();
    }

    private void OnDestroy()
    {
        if (fireworksRoutine != null) StopCoroutine(fireworksRoutine);
        if (winClip != null) Destroy(winClip);
        if (loseClip != null) Destroy(loseClip);
        if (fireworkSprite != null)
        {
            Texture2D texture = fireworkSprite.texture;
            Destroy(fireworkSprite);
            if (texture != null) Destroy(texture);
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BowlWashedEvent -= OnBowlWashed;
            GameManager.Instance.BowlBrokenEvent -= OnBowlsBroken;
        }
        if (shiftTimer != null)
            shiftTimer.Finished -= OnTimeFinished;
    }

    private void OnBowlWashed(bool clean)
    {
        if (isGameOver || !clean) return;

        cleanBowls++;
        if (cleanBowls >= requiredCleanBowls &&
            GameManager.Instance.BrokenBowls <= allowedBrokenBowls)
            EndShift(true, FailureReason.None);
    }

    private void OnBowlsBroken(int count)
    {
        if (!isGameOver && GameManager.Instance.BrokenBowls > allowedBrokenBowls)
            EndShift(false, FailureReason.TooManyBrokenBowls);
    }

    private void OnTimeFinished()
    {
        if (!isGameOver)
        {
            bool won = cleanBowls >= requiredCleanBowls &&
                       GameManager.Instance.BrokenBowls <= allowedBrokenBowls;
            FailureReason reason = won ? FailureReason.None :
                GameManager.Instance.BrokenBowls > allowedBrokenBowls
                    ? FailureReason.TooManyBrokenBowls
                    : FailureReason.TimeExpired;
            EndShift(won, reason);
        }
    }

    private void EndShift(bool won, FailureReason failureReason)
    {
        isGameOver = true;
        Time.timeScale = 0f;
        titleText.text = won ? "SHIFT COMPLETED!" : "SHIFT FAILED";
        titleText.color = won ? new Color32(54, 126, 73, 255) : new Color32(177, 57, 43, 255);
        string reasonLine = failureReason switch
        {
            FailureReason.TimeExpired => "Reason: Time ran out before reaching the goal.\n",
            FailureReason.TooManyBrokenBowls => "Reason: Too many bowls were broken.\n",
            _ => string.Empty
        };
        summaryText.text = reasonLine +
                           $"Bowls completed: {cleanBowls}/{requiredCleanBowls}\n" +
                           $"Bowls broken: {GameManager.Instance.BrokenBowls}/{allowedBrokenBowls}\n" +
                           $"Money earned after bonuses and penalties: {GameManager.Instance.Money}";
        overlay.SetActive(true);
        if (won)
        {
            resultAudio.PlayOneShot(winClip);
            fireworksRoutine = StartCoroutine(PlayFireworks());
        }
        else
        {
            resultAudio.PlayOneShot(loseClip);
        }
    }

    private void CreateOverlay()
    {
        uiFont = TMP_Settings.defaultFontAsset;
        if (uiFont == null)
        {
            var existingText = FindFirstObjectByType<TMP_Text>();
            if (existingText != null) uiFont = existingText.font;
        }

        var canvasObject = new GameObject("GameOverCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        resultAudio = canvasObject.AddComponent<AudioSource>();
        resultAudio.playOnAwake = false;
        resultAudio.spatialBlend = 0f;
        resultAudio.ignoreListenerPause = true;
        winClip = CreateResultClip("GameOverWinChime", new[] { 523.25f, 659.25f, 783.99f, 1046.5f, 783.99f, 1046.5f }, 0.17f, true);
        loseClip = CreateResultClip("GameOverLoseCue", new[] { 392f, 329.63f, 261.63f, 196f }, 0.27f, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        overlay = MakeRect("GameOverOverlay", canvasObject.transform, Vector2.zero,
            Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        MakeImage(overlay, new Color(0.08f, 0.07f, 0.06f, 0.72f));
        fireworksLayer = MakeRect("Fireworks", overlay.transform, Vector2.zero,
            Vector2.one, Vector2.zero, Vector2.zero);
        fireworkSprite = CreateFireworkSprite();

        var card = MakeRect("ResultCard", overlay.transform, new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 520));
        MakeImage(card.gameObject, new Color32(255, 246, 224, 255));

        titleText = MakeText("ResultTitle", card, new Vector2(0.08f, 0.74f),
            new Vector2(0.92f, 0.92f), "SHIFT RESULTS", 46, new Color32(117, 64, 37, 255),
            FontStyles.Bold);
        summaryText = MakeText("ResultSummary", card, new Vector2(0.12f, 0.32f),
            new Vector2(0.88f, 0.71f), string.Empty, 26, new Color32(67, 57, 47, 255),
            FontStyles.Normal);
        summaryText.alignment = TextAlignmentOptions.Center;

        MakeButton(card, "RetryButton", "PLAY AGAIN", new Vector2(0.08f, 0.12f),
            new Vector2(0.48f, 0.29f), new Color32(65, 133, 78, 255), RestartGame);
        MakeButton(card, "MainMenuButton", "MAIN MENU", new Vector2(0.52f, 0.12f),
            new Vector2(0.92f, 0.29f), new Color32(177, 78, 54, 255), ReturnToMenu);
        overlay.SetActive(false);
    }

    private static RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        var child = new GameObject(name, typeof(RectTransform));
        var rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image MakeImage(GameObject target, Color color)
    {
        var image = target.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private IEnumerator PlayFireworks()
    {
        Vector2 canvasSize = ((RectTransform)overlay.transform).rect.size;
        float sideDistance = canvasSize.x * 0.34f;
        float height = canvasSize.y * 0.34f;
        for (int burst = 0; burst < 9; burst++)
        {
            float side = burst % 2 == 0 ? -1f : 1f;
            Vector2 center = new Vector2(
                side * Random.Range(sideDistance * 0.45f, sideDistance),
                Random.Range(height * 0.45f, height));
            SpawnFireworkBurst(center);
            yield return new WaitForSecondsRealtime(0.38f);
        }
        fireworksRoutine = null;
    }

    private void SpawnFireworkBurst(Vector2 center)
    {
        Color[] colors =
        {
            new Color32(255, 205, 67, 255), new Color32(255, 91, 77, 255),
            new Color32(74, 210, 176, 255), new Color32(106, 164, 255, 255),
            new Color32(255, 130, 205, 255), Color.white
        };
        const int particleCount = 30;
        for (int i = 0; i < particleCount; i++)
        {
            float angle = (Mathf.PI * 2f * i / particleCount) + Random.Range(-0.09f, 0.09f);
            float speed = Random.Range(150f, 390f);
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            var particleObject = new GameObject("FireworkSpark", typeof(RectTransform), typeof(Image));
            var rect = particleObject.GetComponent<RectTransform>();
            rect.SetParent(fireworksLayer, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = center;
            float size = Random.Range(7f, 14f);
            rect.sizeDelta = new Vector2(size, size);
            var image = particleObject.GetComponent<Image>();
            image.sprite = fireworkSprite;
            image.color = colors[Random.Range(0, colors.Length)];
            image.raycastTarget = false;
            StartCoroutine(AnimateSpark(rect, image, velocity, Random.Range(1.1f, 1.7f)));
        }
    }

    private static IEnumerator AnimateSpark(RectTransform rect, Image image, Vector2 velocity, float lifetime)
    {
        float elapsed = 0f;
        while (elapsed < lifetime && rect != null && image != null)
        {
            float delta = Time.unscaledDeltaTime;
            elapsed += delta;
            rect.anchoredPosition += velocity * delta;
            velocity.y -= 250f * delta;
            Color color = image.color;
            color.a = Mathf.Clamp01(1f - elapsed / lifetime);
            image.color = color;
            yield return null;
        }
        if (rect != null) Destroy(rect.gameObject);
    }

    private static Sprite CreateFireworkSprite()
    {
        const int textureSize = 32;
        var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "GameOverFireworkSpark",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color32[textureSize * textureSize];
        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.46f;
        for (int y = 0; y < textureSize; y++)
        for (int x = 0; x < textureSize; x++)
        {
            float distance = Vector2.Distance(new Vector2(x, y), center);
            byte alpha = (byte)(Mathf.Clamp01(radius + 0.75f - distance) * 255f);
            pixels[y * textureSize + x] = new Color32(255, 255, 255, alpha);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
    }

    private static AudioClip CreateResultClip(string clipName, float[] notes, float noteDuration, bool bright)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int noteSamples = Mathf.Max(1, Mathf.RoundToInt(noteDuration * sampleRate));
        int sampleCount = noteSamples * notes.Length;
        var samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            int noteIndex = i / noteSamples;
            float noteTime = (i % noteSamples) / (float)sampleRate;
            float frequency = notes[noteIndex];
            float envelope = Mathf.Min(1f, noteTime * 45f) * Mathf.Exp(-noteTime * (bright ? 3.8f : 2.4f));
            float fundamental = Mathf.Sin(2f * Mathf.PI * frequency * noteTime);
            float overtone = Mathf.Sin(2f * Mathf.PI * frequency * 2f * noteTime) * (bright ? 0.32f : 0.12f);
            float lower = bright ? 0f : Mathf.Sin(2f * Mathf.PI * frequency * 0.5f * noteTime) * 0.22f;
            samples[i] = (fundamental * 0.65f + overtone + lower) * envelope * (bright ? 0.42f : 0.48f);
        }
        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private TMP_Text MakeText(string name, Transform parent, Vector2 anchorMin,
        Vector2 anchorMax, string value, float size, Color color, FontStyles style)
    {
        var rect = MakeRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = uiFont;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private void MakeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction action)
    {
        var rect = MakeRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        var image = MakeImage(rect.gameObject, color);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        var text = MakeText("Label", rect, Vector2.zero, Vector2.one, label, 26,
            Color.white, FontStyles.Bold);
        text.raycastTarget = false;
    }

    private static void RestartGame()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene(SceneLoader.SceneEnum.GamePlay);
    }

    private static void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene(SceneLoader.SceneEnum.MainMenu);
    }
}
