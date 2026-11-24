using UnityEngine;

public enum PressResult { Ignored, RevealedAll, Next, Finished }

/// <summary>
/// Typewriter state for a list of lines: reveals characters over time and decides what a
/// "continue" press does. Plain C# so it can be tested without Play Mode.
/// </summary>
public sealed class DialogueCursor
{
    private readonly int lineCount;
    private readonly float charsPerSecond;
    private float revealed;
    private bool revealedAll;
    private int openFrame = -1;

    public int Index { get; private set; }

    public DialogueCursor(int lineCount, float charsPerSecond)
    {
        this.lineCount = Mathf.Max(1, lineCount);
        this.charsPerSecond = Mathf.Max(1f, charsPerSecond);
    }

    /// <summary>Starts from the first line. A press on this same frame is ignored.</summary>
    public void Open(int frame)
    {
        Index = 0;
        revealed = 0f;
        revealedAll = false;
        openFrame = frame;
    }

    public void Tick(float seconds)
    {
        if (seconds > 0f) revealed += seconds * charsPerSecond;
    }

    public int VisibleChars(int lineLength)
    {
        return revealedAll ? lineLength : Mathf.Min(lineLength, Mathf.FloorToInt(revealed));
    }

    public PressResult Press(int lineLength, int frame)
    {
        // The key that opened the box (E in the Player map) must not also skip its first line.
        if (frame == openFrame) return PressResult.Ignored;

        if (VisibleChars(lineLength) < lineLength)
        {
            revealedAll = true;
            return PressResult.RevealedAll;
        }

        if (Index >= lineCount - 1) return PressResult.Finished;

        Index++;
        revealed = 0f;
        revealedAll = false;
        return PressResult.Next;
    }
}
