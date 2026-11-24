using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Plays the opening story (prefab Resources/IntroUI): comic-style scenes with a background,
/// characters standing on it, simple visual effects, and lines shown in the DialogueBox.
/// All timing uses scaled time, so pausing the game freezes the intro.
/// </summary>
public sealed class IntroPlayer : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform frame;
    [SerializeField] private RectTransform background;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform actorLayer;
    [SerializeField] private GameObject titleBoard;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image flash;
    [SerializeField] private Image fade;
    [SerializeField] private Button skipButton;

    [Header("Timing and layout")]
    [SerializeField] private float fadeSeconds = 0.4f;
    [SerializeField] private float titleSeconds = 1.5f;
    [SerializeField] private float actorSpacing = 340f;
    [SerializeField] private float actorSize = 192f;
    [SerializeField] private float actorFramesPerSecond = 5f;

    private static IntroPlayer instance;

    private IntroContentSO content;
    private IntroCursor cursor;
    private Action onFinished;
    private readonly List<Actor> actors = new List<Actor>();
    private readonly List<Coroutine> effects = new List<Coroutine>();
    private bool lineDone;
    private bool ending;
    private int normalSortingOrder;

    private sealed class Actor
    {
        public string key;
        public RectTransform rect;
        public Image image;
        public Sprite[] frames;
        public float jumpOffset;
        public bool leaving;
    }

    public static bool IsPlaying => instance != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() { instance = null; }

    public static void Play(IntroContentSO content, Action onFinished)
    {
        if (content == null || content.scenes == null || content.scenes.Length == 0)
        {
            onFinished?.Invoke();
            return;
        }
        if (instance != null) Destroy(instance.gameObject);
        instance = Instantiate(Resources.Load<IntroPlayer>("IntroUI"));
        instance.name = "IntroUI";
        instance.Begin(content, onFinished);
    }

    /// <summary>Removes the intro without calling its callback (the tutorial was skipped meanwhile).</summary>
    public static void Stop()
    {
        if (instance == null) return;
        instance.onFinished = null;
        DialogueBox.Close();
        Destroy(instance.gameObject);
        instance = null;
    }

    private void Begin(IntroContentSO intro, Action finished)
    {
        content = intro;
        onFinished = finished;
        normalSortingOrder = canvas.sortingOrder;
        skipButton.onClick.AddListener(OnSkipClicked);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onGamePause += OnGamePaused;
            GameManager.Instance.onGameUnPause += OnGameUnpaused;
        }
        cursor = new IntroCursor(content.LinesPerScene());
        cursor.Finished += OnCursorFinished;
        SetAlpha(fade, 1f);
        SetAlpha(flash, 0f);
        titleBoard.SetActive(false);
        StartCoroutine(Run());
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onGamePause -= OnGamePaused;
            GameManager.Instance.onGameUnPause -= OnGameUnpaused;
        }
        if (instance == this) instance = null;
    }

    // The pause menu lives on the HUD canvas (order 0); drop below it while paused so it shows.
    private void OnGamePaused() { canvas.sortingOrder = -1; }
    private void OnGameUnpaused() { canvas.sortingOrder = normalSortingOrder; }

    private IEnumerator Run()
    {
        int shownScene = -1;
        while (!ending)
        {
            if (cursor.Scene != shownScene)
            {
                shownScene = cursor.Scene;
                if (shownScene > 0) yield return Fade(0f, 1f, fadeSeconds);
                SetupScene(content.scenes[shownScene]);
                yield return Fade(1f, 0f, fadeSeconds);
                titleText.text = content.scenes[shownScene].title;
                titleBoard.SetActive(true);
                yield return new WaitForSeconds(titleSeconds);
                titleBoard.SetActive(false);
            }
            if (ending) yield break;

            IntroLine line = content.scenes[cursor.Scene].lines[cursor.Line];
            ApplyEffect(line.effect);
            ApplyMove(line.actorKey, line.move);
            lineDone = false;
            DialogueBox.Show(new[] { ToDialogueLine(line) }, () => lineDone = true);
            while (!lineDone && !ending) yield return null;
            if (ending) yield break;
            if (!cursor.Next()) yield break;
        }
    }

    private void SetupScene(IntroScene scene)
    {
        // Effects of the previous scene (a slow zoom still running, a shake) must not carry over.
        foreach (Coroutine effect in effects)
        {
            if (effect != null) StopCoroutine(effect);
        }
        effects.Clear();
        frame.anchoredPosition = Vector2.zero;
        SetAlpha(flash, 0f);
        backgroundImage.sprite = scene.background;
        background.localScale = Vector3.one;
        foreach (Actor actor in actors) Destroy(actor.rect.gameObject);
        actors.Clear();
        if (scene.actorsOnStage != null)
        {
            foreach (string key in scene.actorsOnStage) AddActor(key);
        }
        for (int i = 0; i < actors.Count; i++) actors[i].rect.anchoredPosition = SlotPosition(i, actors.Count);
    }

    private DialogueLine ToDialogueLine(IntroLine line)
    {
        if (line.speakerKey == IntroContentSO.NarratorKey || !content.TryGetCharacter(line.speakerKey, out IntroCharacter who))
        {
            return new DialogueLine { speaker = "", portrait = null, text = "<i>" + line.text + "</i>" };
        }
        return new DialogueLine { speaker = who.displayName, portrait = who.portrait, text = line.text };
    }

    private void Update()
    {
        // Idle animation of every character on stage.
        int frameIndex = Mathf.FloorToInt(Time.time * actorFramesPerSecond);
        foreach (Actor actor in actors)
        {
            if (actor.frames != null && actor.frames.Length > 0)
                actor.image.sprite = actor.frames[frameIndex % actor.frames.Length];
        }
    }

    // ---------- Characters ----------

    private Actor AddActor(string key)
    {
        if (!content.TryGetCharacter(key, out IntroCharacter who)) return null;
        var go = new GameObject(key, typeof(RectTransform), typeof(Image));
        go.layer = gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(actorLayer, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(actorSize, actorSize);
        var image = go.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.sprite = who.portrait;
        var actor = new Actor { key = key, rect = rect, image = image, frames = who.idleFrames };
        actors.Add(actor);
        return actor;
    }

    private Vector2 SlotPosition(int index, int count)
    {
        float x = (index - (count - 1) / 2f) * actorSpacing;
        return new Vector2(x, -130f);
    }

    private void ApplyMove(string key, ActorMove move)
    {
        if (move == ActorMove.None || string.IsNullOrEmpty(key)) return;
        Actor actor = actors.Find(a => a.key == key && !a.leaving);
        switch (move)
        {
            case ActorMove.EnterLeft:
            case ActorMove.EnterRight:
                if (actor != null) return;
                actor = AddActor(key);
                if (actor == null) return;
                actor.rect.anchoredPosition = new Vector2(move == ActorMove.EnterLeft ? -900f : 900f, -130f);
                StartCoroutine(Relayout(0.4f));
                break;
            case ActorMove.ExitRight:
                if (actor == null) return;
                actor.leaving = true;
                StartCoroutine(Leave(actor, 0.3f));
                break;
            case ActorMove.Jump:
                if (actor != null) StartCoroutine(Jump(actor, 0.25f, 24f));
                break;
        }
    }

    private IEnumerator Relayout(float seconds)
    {
        var staying = actors.FindAll(a => !a.leaving);
        var from = new Vector2[staying.Count];
        for (int i = 0; i < staying.Count; i++) from[i] = staying[i].rect.anchoredPosition;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / seconds);
            for (int i = 0; i < staying.Count; i++)
            {
                if (staying[i].rect == null) continue;
                Vector2 to = SlotPosition(i, staying.Count);
                staying[i].rect.anchoredPosition = Vector2.Lerp(from[i], to, k) + new Vector2(0f, staying[i].jumpOffset);
            }
            yield return null;
        }
        for (int i = 0; i < staying.Count; i++)
        {
            if (staying[i].rect != null) staying[i].rect.anchoredPosition = SlotPosition(i, staying.Count);
        }
    }

    private IEnumerator Leave(Actor actor, float seconds)
    {
        Vector2 from = actor.rect.anchoredPosition;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (actor.rect == null) yield break;
            actor.rect.anchoredPosition = Vector2.Lerp(from, new Vector2(900f, from.y), t / seconds);
            yield return null;
        }
        actors.Remove(actor);
        if (actor.rect != null) Destroy(actor.rect.gameObject);
        StartCoroutine(Relayout(0.3f));
    }

    private IEnumerator Jump(Actor actor, float seconds, float height)
    {
        Vector2 basePosition = actor.rect.anchoredPosition;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (actor.rect == null) yield break;
            actor.rect.anchoredPosition = basePosition + new Vector2(0f, Mathf.Sin(t / seconds * Mathf.PI) * height);
            yield return null;
        }
        if (actor.rect != null) actor.rect.anchoredPosition = basePosition;
    }

    // ---------- Effects ----------

    private void ApplyEffect(IntroEffect effect)
    {
        switch (effect)
        {
            case IntroEffect.Shake: effects.Add(StartCoroutine(Shake(0.3f, 8f))); break;
            case IntroEffect.Flash: effects.Add(StartCoroutine(Flash(0.15f))); break;
            case IntroEffect.ZoomIn: effects.Add(StartCoroutine(Zoom(1.15f, 0.4f))); break;
            case IntroEffect.SlowZoom: effects.Add(StartCoroutine(Zoom(1.08f, 3f))); break;
        }
    }

    private IEnumerator Shake(float seconds, float amplitude)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (Time.timeScale > 0f) frame.anchoredPosition = UnityEngine.Random.insideUnitCircle * amplitude;
            yield return null;
        }
        frame.anchoredPosition = Vector2.zero;
    }

    private IEnumerator Flash(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            SetAlpha(flash, 1f - t / seconds);
            yield return null;
        }
        SetAlpha(flash, 0f);
    }

    private IEnumerator Zoom(float target, float seconds)
    {
        float from = background.localScale.x;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            float s = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / seconds));
            background.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        background.localScale = new Vector3(target, target, 1f);
    }

    private IEnumerator Fade(float from, float to, float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            SetAlpha(fade, Mathf.Lerp(from, to, t / seconds));
            yield return null;
        }
        SetAlpha(fade, to);
    }

    private static void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
        image.raycastTarget = alpha > 0.01f && image.gameObject.name == "Fade";
    }

    // ---------- Ending ----------

    private void OnSkipClicked()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        cursor.Skip();
    }

    private void OnCursorFinished()
    {
        if (ending) return;
        ending = true;
        DialogueBox.Close();
        StartCoroutine(End());
    }

    private IEnumerator End()
    {
        skipButton.gameObject.SetActive(false);
        titleBoard.SetActive(false);
        yield return Fade(fade.color.a, 1f, 0.5f);
        Action done = onFinished;
        onFinished = null;
        instance = null;
        Destroy(gameObject);
        done?.Invoke();
    }
}
