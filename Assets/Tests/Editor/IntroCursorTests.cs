using NUnit.Framework;

public class IntroCursorTests
{
    [Test]
    public void IntroCursor_WalksScenesInOrder()
    {
        var cursor = new IntroCursor(new[] { 2, 1 });
        Assert.AreEqual((0, 0), (cursor.Scene, cursor.Line));
        Assert.IsTrue(cursor.IsSceneStart);

        Assert.IsTrue(cursor.Next());
        Assert.AreEqual((0, 1), (cursor.Scene, cursor.Line));
        Assert.IsFalse(cursor.IsSceneStart);

        Assert.IsTrue(cursor.Next());
        Assert.AreEqual((1, 0), (cursor.Scene, cursor.Line));
        Assert.IsTrue(cursor.IsSceneStart);

        Assert.IsFalse(cursor.Next());
    }

    [Test]
    public void IntroCursor_SkipFinishesOnce()
    {
        var cursor = new IntroCursor(new[] { 3 });
        int finished = 0;
        cursor.Finished += () => finished++;

        cursor.Skip();
        cursor.Skip();

        Assert.IsFalse(cursor.Next());
        Assert.AreEqual(1, finished);
    }

    [Test]
    public void IntroCursor_EndOfLastLineFinishesOnce()
    {
        var cursor = new IntroCursor(new[] { 1 });
        int finished = 0;
        cursor.Finished += () => finished++;

        Assert.IsFalse(cursor.Next());
        Assert.IsFalse(cursor.Next());
        Assert.AreEqual(1, finished);
    }

    [Test]
    public void IntroCursor_SkipsEmptyScenes()
    {
        var cursor = new IntroCursor(new[] { 1, 0, 1 });
        cursor.Next();
        Assert.AreEqual(2, cursor.Scene);
    }
}
