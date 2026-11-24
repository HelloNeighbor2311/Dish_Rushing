using System;
using UnityEngine;

/// <summary>
/// The shop owner at the cashier. Press E to ask for a tip or replay the tutorial.
/// She also reacts to what happens in the shift with short speech bubbles.
/// </summary>
public sealed class OwnerNpc : MonoBehaviour, IInteractable
{
    [SerializeField] private OwnerDialogueSO dialogue;
    [SerializeField] private SpeechBubble bubble;

    [Header("When she speaks up")]
    [SerializeField, Min(0f)] private float globalCooldown = 6f;
    [SerializeField, Min(0f)] private float perKindCooldown = 15f;
    [SerializeField, Min(1)] private int pileUpThreshold = 12;
    [SerializeField, Min(0f)] private float timeLowSeconds = 20f;
    [SerializeField, Min(1)] private int cleanStreakToPraise = 3;
    [SerializeField, Min(0.5f)] private float bubbleSeconds = 3f;

    private readonly int[] nextLine = new int[5];
    private BarkScheduler scheduler;
    private CountDownTime timer;
    private int cleanStreak;
    private int nextTip;
    private int tipsInARow;
    private bool timeLowSaid;

    /// <summary>True while the tutorial runs: no remarks, no talking.</summary>
    public bool Silenced { get; set; }

    public event Action ReplayTutorialRequested;

    private void Awake()
    {
        scheduler = new BarkScheduler(globalCooldown, perKindCooldown);
    }

    private void Start()
    {
        timer = FindFirstObjectByType<CountDownTime>();
        if (GameManager.Instance == null) return;
        GameManager.Instance.BowlWashedEvent += OnBowlWashed;
        GameManager.Instance.BowlBrokenEvent += OnBowlBroken;
        GameManager.Instance.Changed += OnChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.BowlWashedEvent -= OnBowlWashed;
        GameManager.Instance.BowlBrokenEvent -= OnBowlBroken;
        GameManager.Instance.Changed -= OnChanged;
    }

    private void Update()
    {
        if (!timeLowSaid && timer != null && timer.TimeRemaining > 0f && timer.TimeRemaining <= timeLowSeconds)
        {
            timeLowSaid = true;
            scheduler.Request(BarkKind.TimeLow);
        }

        // The bubble lives in the world, so the washing screen or a dialogue would hide it.
        if (Silenced || DialogueBox.IsOpen || DishWashingMode.IsOpen) return;
        if (scheduler.TryTake(Time.time, out BarkKind kind)) bubble.Say(PickLine(kind), bubbleSeconds);
    }

    public bool CanInteract(PlayerInteractor player)
    {
        return !DialogueBox.IsOpen && !Silenced;
    }

    public void Interact(PlayerInteractor player)
    {
        string question = dialogue.talkPrompts != null && dialogue.talkPrompts.Length > 0
            ? dialogue.talkPrompts[UnityEngine.Random.Range(0, dialogue.talkPrompts.Length)]
            : "";
        DialogueLine prompt = dialogue.AsLines(new[] { question })[0];
        DialogueBox.ShowChoice(prompt, dialogue.talkOptions, OnTalkChoice);
    }

    /// <summary>Shows a speech bubble right away, bypassing the remark cooldowns (used by the tutorial).</summary>
    public void Say(string line)
    {
        if (!string.IsNullOrEmpty(line)) bubble.Say(line, bubbleSeconds);
    }

    private void OnTalkChoice(int choice)
    {
        if (choice == 1)
        {
            tipsInARow = 0;
            DialogueBox.Show(dialogue.AsLines(new[] { dialogue.replayAck }), () => ReplayTutorialRequested?.Invoke());
            return;
        }
        if (dialogue.tips == null || dialogue.tips.Length == 0) return;
        tipsInARow++;
        if (tipsInARow > dialogue.tips.Length && !string.IsNullOrEmpty(dialogue.tipsEasterEgg))
        {
            tipsInARow = 0;
            DialogueBox.Show(dialogue.AsLines(new[] { dialogue.tipsEasterEgg }));
            return;
        }
        string tip = dialogue.tips[nextTip % dialogue.tips.Length];
        nextTip++;
        DialogueBox.Show(dialogue.AsLines(new[] { tip }));
    }

    private void OnBowlWashed(bool clean)
    {
        if (!clean)
        {
            cleanStreak = 0;
            scheduler.Request(BarkKind.Dirty);
            return;
        }
        cleanStreak++;
        if (cleanStreak < cleanStreakToPraise) return;
        cleanStreak = 0;
        scheduler.Request(BarkKind.CleanStreak);
    }

    private void OnBowlBroken(int count)
    {
        cleanStreak = 0;
        scheduler.Request(BarkKind.Broken);
    }

    private void OnChanged()
    {
        if (GameManager.Instance.BowlsToWash >= pileUpThreshold) scheduler.Request(BarkKind.PileUp);
    }

    private string PickLine(BarkKind kind)
    {
        string[] lines = kind switch
        {
            BarkKind.Broken => dialogue.barkBroken,
            BarkKind.Dirty => dialogue.barkDirty,
            BarkKind.PileUp => dialogue.barkPileUp,
            BarkKind.TimeLow => dialogue.barkTimeLow,
            _ => dialogue.barkCleanStreak,
        };
        if (lines == null || lines.Length == 0) return "";
        int index = nextLine[(int)kind]++ % lines.Length;
        return lines[index];
    }
}
