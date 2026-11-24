using System;

public enum TutorialStep { Welcome, Move, PickUp, Stamina, LoadWasher, Scrub, StartShift, Done }

public enum TutorialSignal { DialogueFinished, Moved, DishPicked, TimerElapsed, DishLoaded, BowlWashed }

/// <summary>Order of the first-shift tutorial and the signal that completes each step. Plain C#.</summary>
public sealed class TutorialSteps
{
    private static readonly TutorialSignal[] Completes =
    {
        TutorialSignal.DialogueFinished, // Welcome
        TutorialSignal.Moved,            // Move
        TutorialSignal.DishPicked,       // PickUp
        TutorialSignal.TimerElapsed,     // Stamina
        TutorialSignal.DishLoaded,       // LoadWasher
        TutorialSignal.BowlWashed,       // Scrub
        TutorialSignal.DialogueFinished, // StartShift
    };

    public TutorialStep Current { get; private set; } = TutorialStep.Welcome;

    public event Action<TutorialStep> StepChanged;
    public event Action Finished;

    /// <summary>Advances when the signal completes the current step. Returns true if it advanced.</summary>
    public bool Signal(TutorialSignal signal)
    {
        if (Current == TutorialStep.Done || Completes[(int)Current] != signal) return false;
        GoTo(Current + 1);
        return true;
    }

    public void Skip()
    {
        if (Current != TutorialStep.Done) GoTo(TutorialStep.Done);
    }

    private void GoTo(TutorialStep step)
    {
        Current = step;
        StepChanged?.Invoke(step);
        if (step == TutorialStep.Done) Finished?.Invoke();
    }
}
