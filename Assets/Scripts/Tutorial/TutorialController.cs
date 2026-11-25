using System.Collections;
using UnityEngine;

/// <summary>
/// Runs the first-shift tutorial led by the shop owner, after the opening story on the very
/// first play. Until it finishes (or is skipped) the shift clock, bowl spawning and customers
/// are held. Shown once; the owner can replay the tutorial (without the story).
/// </summary>
public sealed class TutorialController : MonoBehaviour
{
    [SerializeField] private OwnerNpc owner;
    [SerializeField] private Dishwasher dishwasher;
    [SerializeField] private BowlSpawn bowlSpawn;
    [SerializeField] private CountDownTime timer;
    [SerializeField] private CustomerSpawner customers;
    [SerializeField] private PlayerInteractor player;
    [SerializeField] private OwnerDialogueSO dialogue;
    [SerializeField] private IntroContentSO intro;
    [SerializeField] private TutorialPointer pointer;

    [Header("Step tuning")]
    [SerializeField, Min(0.1f)] private float moveDistance = 2f;
    [SerializeField, Min(0f)] private float staminaSeconds = 4f;
    [SerializeField] private float arrowAboveDish = 0.9f;
    [SerializeField] private float arrowAboveWasher = 1.4f;
    [Tooltip("Seconds of standing still before the owner nudges the player.")]
    [SerializeField, Min(1f)] private float idleNudgeSeconds = 12f;
    [Tooltip("Pause between a 'well done' remark and the next step's opening remark.")]
    [SerializeField, Min(0f)] private float remarkGap = 3.2f;

    private TutorialSteps steps;
    private TutorialHud hud;
    private DishPickup tutorialDish;
    private Vector3 lastPlayerPosition;
    private float moved;
    private float stepTime;
    private float idleTime;
    private int nextNudge;
    private Vector3 idlePosition;
    private float lastCleanRatio = -1f;
    private bool lastBowlClean;
    private bool hadTutorialDish;
    private bool holdingShift;
    private bool startShiftPending;
    private bool hooked;
    private Coroutine pendingRemark;

    public bool IsRunning => steps != null && steps.Current != TutorialStep.Done;

    private void Awake()
    {
        if (TutorialProgress.IsDone) return;
        // Must run before the other components' Start, which is why it lives in Awake.
        holdingShift = true;
        timer.Hold();
        bowlSpawn.Hold();
        customers.Hold();
    }

    private void Start()
    {
        owner.ReplayTutorialRequested += Replay;
        if (!holdingShift) return;
        owner.Silenced = true;
        if (intro != null) IntroPlayer.Play(intro, Begin);
        else Begin();
    }

    private void OnDestroy()
    {
        if (owner != null) owner.ReplayTutorialRequested -= Replay;
        Unhook();
    }

    public void Begin()
    {
        if (IsRunning) return;
        steps = new TutorialSteps();
        steps.StepChanged += OnStepChanged;
        steps.Finished += OnFinished;
        Hook();
        hud = TutorialHud.Get();
        hud.Show(SkipNow);
        owner.Silenced = true;
        OnStepChanged(TutorialStep.Welcome);
    }

    public void SkipNow()
    {
        steps?.Skip();
    }

    private void Replay()
    {
        if (IsRunning) return;
        TutorialProgress.Reset();
        Begin(); // the shift keeps running: only the first tutorial holds it
    }

    // ---------- Steps ----------

    private void OnStepChanged(TutorialStep step)
    {
        if (step == TutorialStep.Done) return;
        if (dialogue.stepObjectives != null && (int)step < dialogue.stepObjectives.Length)
            hud.SetObjective(dialogue.stepObjectives[(int)step]);
        pointer.Clear();
        stepTime = 0f;
        idleTime = 0f;
        nextNudge = 0;
        lastCleanRatio = -1f;
        idlePosition = player.transform.position;

        string completed = CompletedRemark(step - 1);
        string opening = StepIndex(step) >= 0 ? At(dialogue.stepIntro, StepIndex(step)) : null;
        if (pendingRemark != null) StopCoroutine(pendingRemark);
        if (!string.IsNullOrEmpty(completed))
        {
            Speak(completed);
            if (!string.IsNullOrEmpty(opening)) pendingRemark = StartCoroutine(SpeakLater(opening, remarkGap));
        }
        else if (!string.IsNullOrEmpty(opening))
        {
            Speak(opening);
        }

        switch (step)
        {
            case TutorialStep.Welcome:
                DialogueBox.Show(dialogue.AsLines(dialogue.welcome), () => steps.Signal(TutorialSignal.DialogueFinished));
                break;
            case TutorialStep.Move:
                lastPlayerPosition = player.transform.position;
                moved = 0f;
                break;
            case TutorialStep.PickUp:
                hadTutorialDish = SpawnTutorialDish();
                break;
            case TutorialStep.LoadWasher:
                pointer.Point(dishwasher.transform, arrowAboveWasher);
                break;
            case TutorialStep.StartShift:
                startShiftPending = true;
                break;
        }
    }

    private void Update()
    {
        if (!IsRunning) return;
        hud.SetWashingLayout(DishWashingMode.IsOpen);
        TrackIdle();

        switch (steps.Current)
        {
            case TutorialStep.Move:
                moved += Vector3.Distance(player.transform.position, lastPlayerPosition);
                lastPlayerPosition = player.transform.position;
                if (moved >= moveDistance) steps.Signal(TutorialSignal.Moved);
                break;
            case TutorialStep.PickUp:
                // The tutorial stack broke or vanished before it was picked up: put out a new one.
                if (tutorialDish == null && SpawnTutorialDish())
                {
                    if (hadTutorialDish) Speak(At(dialogue.dishBrokenDuringTutorial, 0));
                    hadTutorialDish = true;
                }
                break;
            case TutorialStep.Stamina:
                stepTime += Time.deltaTime;
                if (stepTime >= staminaSeconds) steps.Signal(TutorialSignal.TimerElapsed);
                break;
            case TutorialStep.LoadWasher:
                // The carried bowls broke on the way: send the player back for another stack.
                if (!player.HasDish && dishwasher.DirtyBowls == 0 && tutorialDish == null && SpawnTutorialDish())
                {
                    Speak(At(dialogue.dishBrokenDuringTutorial, 1));
                }
                else if (player.HasDish && tutorialDish != null)
                {
                    tutorialDish = null;
                    pointer.Point(dishwasher.transform, arrowAboveWasher);
                }
                break;
            case TutorialStep.StartShift:
                if (startShiftPending && !DishWashingMode.IsOpen && !DialogueBox.IsOpen)
                {
                    startShiftPending = false;
                    DialogueBox.Show(dialogue.AsLines(dialogue.startShift), () => steps.Signal(TutorialSignal.DialogueFinished));
                }
                break;
        }
    }

    // Nudges the player after standing still (or not scrubbing) for too long.
    private void TrackIdle()
    {
        int index = StepIndex(steps.Current);
        if (index < 0 || DialogueBox.IsOpen || dialogue.stepIdle == null || index >= dialogue.stepIdle.Length) return;

        bool active = Vector3.Distance(player.transform.position, idlePosition) > 0.001f;
        idlePosition = player.transform.position;
        if (steps.Current == TutorialStep.Scrub && dishwasher.CurrentMask != null)
        {
            float ratio = dishwasher.CurrentMask.CleanRatio;
            active = lastCleanRatio >= 0f && ratio != lastCleanRatio;
            lastCleanRatio = ratio;
        }
        if (active) { idleTime = 0f; return; }

        idleTime += Time.deltaTime;
        string[] nudges = dialogue.stepIdle[index].lines;
        if (idleTime < idleNudgeSeconds || nudges == null || nextNudge >= nudges.Length) return;
        idleTime = 0f;
        Speak(nudges[nextNudge++]);
    }

    /// <summary>Puts out one stack for the tutorial and points at it. False when no spot is free.</summary>
    private bool SpawnTutorialDish()
    {
        tutorialDish = bowlSpawn.SpawnNow();
        if (tutorialDish == null) return false;
        pointer.Point(tutorialDish.transform, arrowAboveDish);
        return true;
    }

    // ---------- Remarks ----------

    /// <summary>Index into the per-step arrays (Move = 0 ... Scrub = 4), or -1 for other steps.</summary>
    private static int StepIndex(TutorialStep step)
    {
        return step >= TutorialStep.Move && step <= TutorialStep.Scrub ? step - TutorialStep.Move : -1;
    }

    private string CompletedRemark(TutorialStep finishedStep)
    {
        if (finishedStep == TutorialStep.Scrub && !lastBowlClean) return dialogue.scrubDirtyReaction;
        int index = StepIndex(finishedStep);
        return index >= 0 ? At(dialogue.stepCompleted, index) : null;
    }

    private static string At(string[] lines, int index)
    {
        return lines != null && index >= 0 && index < lines.Length ? lines[index] : null;
    }

    private IEnumerator SpeakLater(string line, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pendingRemark = null;
        if (IsRunning) Speak(line);
    }

    /// <summary>Speech bubble over the owner when she is on screen, otherwise under the objective.</summary>
    private void Speak(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        Camera view = Camera.main;
        bool ownerVisible = false;
        if (view != null && !DishWashingMode.IsOpen)
        {
            Vector3 p = view.WorldToViewportPoint(owner.transform.position);
            ownerVisible = p.x > 0.05f && p.x < 0.95f && p.y > 0.05f && p.y < 0.9f;
        }
        if (ownerVisible) owner.Say(line);
        else hud.ShowRemark(dialogue.speakerName + ": " + line);
    }

    // ---------- Finish ----------

    private void OnFinished()
    {
        if (IntroPlayer.IsPlaying) IntroPlayer.Stop();
        if (pendingRemark != null) StopCoroutine(pendingRemark);
        pendingRemark = null;
        DialogueBox.Close();
        pointer.Clear();
        if (hud != null) hud.Hide();
        Unhook();
        owner.Silenced = false;
        startShiftPending = false;
        tutorialDish = null;
        TutorialProgress.MarkDone();
        if (!holdingShift) return;
        holdingShift = false;
        timer.Release();
        bowlSpawn.Release();
        customers.Release();
    }

    private void Hook()
    {
        if (hooked) return;
        hooked = true;
        player.DishPicked += OnDishPicked;
        dishwasher.DishLoaded += OnDishLoaded;
        if (GameManager.Instance != null) GameManager.Instance.BowlWashedEvent += OnBowlWashed;
    }

    private void Unhook()
    {
        if (!hooked) return;
        hooked = false;
        if (player != null) player.DishPicked -= OnDishPicked;
        if (dishwasher != null) dishwasher.DishLoaded -= OnDishLoaded;
        if (GameManager.Instance != null) GameManager.Instance.BowlWashedEvent -= OnBowlWashed;
    }

    private void OnDishPicked(DishPickup dish) { steps.Signal(TutorialSignal.DishPicked); }
    private void OnDishLoaded() { steps.Signal(TutorialSignal.DishLoaded); }

    private void OnBowlWashed(bool clean)
    {
        lastBowlClean = clean;
        steps.Signal(TutorialSignal.BowlWashed);
    }
}
