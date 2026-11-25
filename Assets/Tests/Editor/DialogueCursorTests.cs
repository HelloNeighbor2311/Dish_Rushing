using NUnit.Framework;

public class DialogueCursorTests
{
    [Test]
    public void DialogueCursor_IgnoresPressOnOpeningFrame()
    {
        var cursor = new DialogueCursor(2, 40f);
        cursor.Open(10);
        Assert.AreEqual(PressResult.Ignored, cursor.Press(20, 10));
    }

    [Test]
    public void DialogueCursor_FirstPressRevealsSecondAdvances()
    {
        var cursor = new DialogueCursor(2, 40f);
        cursor.Open(0);
        cursor.Tick(0.1f); // 4 characters at 40 per second

        Assert.AreEqual(4, cursor.VisibleChars(20));
        Assert.AreEqual(PressResult.RevealedAll, cursor.Press(20, 1));
        Assert.AreEqual(20, cursor.VisibleChars(20));
        Assert.AreEqual(PressResult.Next, cursor.Press(20, 2));
        Assert.AreEqual(1, cursor.Index);
        Assert.AreEqual(0, cursor.VisibleChars(20));
    }

    [Test]
    public void DialogueCursor_LastLineFinishes()
    {
        var cursor = new DialogueCursor(1, 40f);
        cursor.Open(0);
        cursor.Tick(5f);
        Assert.AreEqual(PressResult.Finished, cursor.Press(20, 1));
    }

    [Test]
    public void DialogueCursor_ZeroTickRevealsNothing()
    {
        var cursor = new DialogueCursor(1, 40f);
        cursor.Open(0);
        cursor.Tick(0f);
        Assert.AreEqual(0, cursor.VisibleChars(20));
    }
}
