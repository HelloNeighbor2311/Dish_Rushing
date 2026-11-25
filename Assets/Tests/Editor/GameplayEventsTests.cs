using NUnit.Framework;
using UnityEngine;

public class GameplayEventsTests
{
    private GameObject host;

    [TearDown]
    public void TearDown()
    {
        if (host != null) Object.DestroyImmediate(host);
    }

    private GameManager CreateGameManager()
    {
        host = new GameObject("GameManagerTest");
        return host.AddComponent<GameManager>();
    }

    [Test]
    public void BowlWashed_RaisesEventWithCleanFlag()
    {
        GameManager gm = CreateGameManager();
        bool? got = null;
        gm.BowlWashedEvent += clean => got = clean;

        gm.BowlWashed(true);
        Assert.AreEqual(true, got);
        gm.BowlWashed(false);
        Assert.AreEqual(false, got);
    }

    [Test]
    public void BowlBroken_RaisesEventWithCount_AndSkipsZero()
    {
        GameManager gm = CreateGameManager();
        int calls = 0, last = 0;
        gm.BowlBrokenEvent += count => { calls++; last = count; };

        gm.BowlBroken(0);
        gm.BowlBroken(3);

        Assert.AreEqual(1, calls);
        Assert.AreEqual(3, last);
    }
}
