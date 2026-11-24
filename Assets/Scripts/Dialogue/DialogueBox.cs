using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Bottom-of-screen dialogue box (prefab Resources/DialogueUI). Shows lines with a typewriter
/// effect, or one prompt with choice buttons. Locks player movement while open.
/// Continue with E, Space, Enter or the left mouse button.
/// </summary>
public sealed class DialogueBox : MonoBehaviour
{
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject continueHint;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceLabels;
    [SerializeField] private float charsPerSecond = 40f;

    private static DialogueBox instance;

    private DialogueLine[] lines;
    private DialogueCursor cursor;
    private Action onFinished;
    private Action<int> onChosen;
    private bool choosing;
    private bool wasUIMode;

    public static bool IsOpen => instance != null && instance.gameObject.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() { instance = null; }

    public static void Show(DialogueLine[] lines, Action onFinished = null)
    {
        if (lines == null || lines.Length == 0)
        {
            onFinished?.Invoke();
            return;
        }
        DialogueBox box = Get();
        box.Begin(lines, onFinished, null, null);
    }

    public static void ShowChoice(DialogueLine prompt, string[] options, Action<int> onChosen)
    {
        DialogueBox box = Get();
        box.Begin(new[] { prompt }, null, options, onChosen);
    }

    /// <summary>Closes without calling any callback (used when the tutorial is skipped).</summary>
    public static void Close()
    {
        if (IsOpen) instance.End();
    }

    private static DialogueBox Get()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<DialogueBox>("DialogueUI"));
            instance.name = "DialogueUI";
            for (int i = 0; i < instance.choiceButtons.Length; i++)
            {
                int index = i;
                instance.choiceButtons[i].onClick.AddListener(() => instance.Choose(index));
            }
        }
        return instance;
    }

    private void Begin(DialogueLine[] newLines, Action finished, string[] options, Action<int> chosen)
    {
        if (!gameObject.activeSelf)
        {
            wasUIMode = InputModeManager.instance != null && InputModeManager.instance.IsUIMode;
            if (InputModeManager.instance != null) InputModeManager.instance.EnterUIMode();
        }
        lines = newLines;
        onFinished = finished;
        onChosen = chosen;
        choosing = options != null && options.Length > 0;
        cursor = new DialogueCursor(lines.Length, charsPerSecond);
        cursor.Open(Time.frameCount);
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool used = choosing && i < options.Length;
            choiceButtons[i].gameObject.SetActive(false);
            if (used) choiceLabels[i].text = options[i];
        }
        gameObject.SetActive(true);
        ShowLine();
    }

    private void Update()
    {
        if (lines == null) return;
        // PauseUI's Continue button toggles the input mode; keep the player locked while talking.
        if (InputModeManager.instance != null && !InputModeManager.instance.IsUIMode && Time.timeScale > 0f)
            InputModeManager.instance.EnterUIMode();
        DialogueLine line = lines[cursor.Index];
        cursor.Tick(Time.deltaTime);
        int length = line.text != null ? line.text.Length : 0;
        bodyText.maxVisibleCharacters = cursor.VisibleChars(length);
        bool fullyShown = cursor.VisibleChars(length) >= length;
        continueHint.SetActive(fullyShown && !choosing);
        if (choosing && fullyShown) ShowChoices();

        if (ContinuePressed()) Continue();
    }

    /// <summary>Same as pressing E / Space: reveals the line, goes to the next one, or finishes.</summary>
    public void Continue()
    {
        if (lines == null) return;
        DialogueLine line = lines[cursor.Index];
        int length = line.text != null ? line.text.Length : 0;
        if (choosing && cursor.VisibleChars(length) >= length) return; // a choice must be picked with a button
        switch (cursor.Press(length, Time.frameCount))
        {
            case PressResult.Next:
                ShowLine();
                break;
            case PressResult.Finished:
                Action done = onFinished;
                End();
                done?.Invoke();
                break;
        }
    }

    private void ShowLine()
    {
        DialogueLine line = lines[cursor.Index];
        speakerText.text = line.speaker;
        // The storyteller has no name: hide the name plate.
        speakerText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(line.speaker));
        bodyText.text = line.text;
        bodyText.maxVisibleCharacters = 0;
        portraitImage.sprite = line.portrait;
        portraitImage.enabled = line.portrait != null;
        // No portrait (the storyteller): hide its frame and let the text use the whole width.
        bool hasPortrait = line.portrait != null;
        portraitImage.transform.parent.gameObject.SetActive(hasPortrait);
        var body = bodyText.rectTransform;
        body.anchoredPosition = new Vector2(hasPortrait ? 80f : 0f, body.anchoredPosition.y);
        body.sizeDelta = new Vector2(hasPortrait ? 820f : 980f, body.sizeDelta.y);
    }

    private void ShowChoices()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool used = !string.IsNullOrEmpty(choiceLabels[i].text) && onChosen != null;
            if (choiceButtons[i].gameObject.activeSelf != used) choiceButtons[i].gameObject.SetActive(used);
        }
    }

    private void Choose(int index)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        Action<int> chosen = onChosen;
        End();
        chosen?.Invoke(index);
    }

    private void End()
    {
        lines = null;
        onFinished = null;
        onChosen = null;
        foreach (TMP_Text label in choiceLabels) label.text = "";
        gameObject.SetActive(false);
        if (InputModeManager.instance != null && !wasUIMode) InputModeManager.instance.EnterGameplayMode();
    }

    private static bool ContinuePressed()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        return (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame ||
                                     keyboard.enterKey.wasPressedThisFrame))
               || (mouse != null && mouse.leftButton.wasPressedThisFrame);
    }
}
