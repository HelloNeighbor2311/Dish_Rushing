using NUnit.Framework;
using UnityEngine;

public class TutorialStepsTests
{
    [Test]
    public void TutorialSteps_HappyPathReachesDoneOnce()
    {
        var steps = new TutorialSteps();
        int finished = 0;
        steps.Finished += () => finished++;

        foreach (TutorialSignal signal in new[]
                 {
                     TutorialSignal.DialogueFinished, TutorialSignal.Moved, TutorialSignal.DishPicked,
                     TutorialSignal.TimerElapsed, TutorialSignal.DishLoaded, TutorialSignal.BowlWashed,
                     TutorialSignal.DialogueFinished,
                 })
        {
            Assert.IsTrue(steps.Signal(signal), signal.ToString());
        }

        Assert.AreEqual(TutorialStep.Done, steps.Current);
        Assert.AreEqual(1, finished);
    }

    [Test]
    public void TutorialSteps_IgnoresWrongSignal()
    {
        var steps = new TutorialSteps();
        Assert.IsFalse(steps.Signal(TutorialSignal.DishPicked));
        Assert.AreEqual(TutorialStep.Welcome, steps.Current);
    }

    [Test]
    public void TutorialSteps_SkipFromAnyStepFinishesOnce()
    {
        var steps = new TutorialSteps();
        int finished = 0;
        steps.Finished += () => finished++;

        steps.Signal(TutorialSignal.DialogueFinished);
        steps.Skip();
        steps.Skip();

        Assert.AreEqual(TutorialStep.Done, steps.Current);
        Assert.AreEqual(1, finished);
        Assert.IsFalse(steps.Signal(TutorialSignal.Moved));
    }

    [Test]
    public void TutorialSteps_StepChangedReportsNewStep()
    {
        var steps = new TutorialSteps();
        TutorialStep? seen = null;
        steps.StepChanged += step => seen = step;

        steps.Signal(TutorialSignal.DialogueFinished);

        Assert.AreEqual(TutorialStep.Move, seen);
    }

    [Test]
    public void TutorialProgress_RoundTrip()
    {
        TutorialProgress.Reset();
        Assert.IsFalse(TutorialProgress.IsDone);
        TutorialProgress.MarkDone();
        Assert.IsTrue(TutorialProgress.IsDone);
        TutorialProgress.Reset();
    }
}

public class EdgePointerMathTests
{
    [Test]
    public void EdgePointer_OnScreenUnchanged()
    {
        Assert.IsTrue(EdgePointerMath.Place(new Vector2(0.3f, 0.7f), 0.05f, out Vector2 point, out _));
        Assert.AreEqual(new Vector2(0.3f, 0.7f), point);
    }

    [Test]
    public void EdgePointer_OffRightClampsToRightEdge()
    {
        Assert.IsFalse(EdgePointerMath.Place(new Vector2(2f, 0.5f), 0.05f, out Vector2 point, out float angle));
        Assert.AreEqual(0.95f, point.x, 1e-4f);
        Assert.AreEqual(0.5f, point.y, 1e-4f);
        Assert.AreEqual(0f, angle, 1e-3f);
    }

    [Test]
    public void EdgePointer_OffTopLeftPointsUpLeft()
    {
        EdgePointerMath.Place(new Vector2(-1f, 2f), 0.05f, out Vector2 point, out float angle);
        Assert.That(point.x, Is.InRange(0.05f, 0.95f));
        Assert.That(point.y, Is.InRange(0.05f, 0.95f));
        Assert.AreEqual(135f, angle, 1e-3f);
    }
}
