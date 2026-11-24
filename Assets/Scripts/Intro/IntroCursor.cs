using System;

/// <summary>Position in the intro (scene, line). Empty scenes are skipped. Plain C#.</summary>
public sealed class IntroCursor
{
    private readonly int[] linesPerScene;
    private bool finished;

    public int Scene { get; private set; }
    public int Line { get; private set; }
    public bool IsSceneStart => Line == 0;

    public event Action Finished;

    public IntroCursor(int[] linesPerScene)
    {
        this.linesPerScene = linesPerScene ?? new int[0];
        Scene = -1;
        if (!MoveToScene(0)) Finish();
    }

    /// <summary>Moves to the next line. Returns false (and finishes once) when there is none.</summary>
    public bool Next()
    {
        if (finished) return false;
        if (Line + 1 < linesPerScene[Scene])
        {
            Line++;
            return true;
        }
        if (MoveToScene(Scene + 1)) return true;
        Finish();
        return false;
    }

    public void Skip()
    {
        Finish();
    }

    private bool MoveToScene(int start)
    {
        for (int i = start; i < linesPerScene.Length; i++)
        {
            if (linesPerScene[i] <= 0) continue;
            Scene = i;
            Line = 0;
            return true;
        }
        return false;
    }

    private void Finish()
    {
        if (finished) return;
        finished = true;
        Finished?.Invoke();
    }
}
