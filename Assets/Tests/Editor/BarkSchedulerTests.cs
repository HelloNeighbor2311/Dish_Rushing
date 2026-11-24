using NUnit.Framework;

public class BarkSchedulerTests
{
    [Test]
    public void BarkScheduler_RespectsGlobalCooldown()
    {
        var scheduler = new BarkScheduler(6f, 15f);
        scheduler.Request(BarkKind.Dirty);
        Assert.IsTrue(scheduler.TryTake(0f, out _));

        scheduler.Request(BarkKind.Broken);
        Assert.IsFalse(scheduler.TryTake(5.9f, out _));
        Assert.IsTrue(scheduler.TryTake(6f, out BarkKind kind));
        Assert.AreEqual(BarkKind.Broken, kind);
    }

    [Test]
    public void BarkScheduler_PicksHighestPriority()
    {
        var scheduler = new BarkScheduler(6f, 15f);
        scheduler.Request(BarkKind.CleanStreak);
        scheduler.Request(BarkKind.Broken);
        scheduler.Request(BarkKind.Dirty);

        Assert.IsTrue(scheduler.TryTake(0f, out BarkKind kind));
        Assert.AreEqual(BarkKind.Broken, kind);
        Assert.IsFalse(scheduler.TryTake(100f, out _)); // the queue was cleared
    }

    [Test]
    public void BarkScheduler_RespectsPerKindCooldown()
    {
        var scheduler = new BarkScheduler(6f, 15f);
        scheduler.Request(BarkKind.Dirty);
        scheduler.TryTake(0f, out _);

        scheduler.Request(BarkKind.Dirty);
        Assert.IsFalse(scheduler.TryTake(10f, out _));
        Assert.IsTrue(scheduler.TryTake(15f, out _));
    }

    [Test]
    public void BarkScheduler_NothingRequestedNothingTaken()
    {
        Assert.IsFalse(new BarkScheduler(6f, 15f).TryTake(0f, out _));
    }
}
