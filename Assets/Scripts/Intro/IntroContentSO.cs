using System;
using UnityEngine;

public enum IntroEffect { None, Shake, Flash, ZoomIn, SlowZoom }

public enum ActorMove { None, EnterLeft, EnterRight, ExitRight, Jump }

[Serializable]
public struct IntroCharacter
{
    public string key;
    public string displayName;
    public Sprite portrait;
    public Sprite[] idleFrames;
}

[Serializable]
public struct IntroLine
{
    [Tooltip("Character key, or \"narrator\" for the storyteller (no name, italic).")]
    public string speakerKey;
    [TextArea] public string text;
    public IntroEffect effect;
    [Tooltip("Character key the move applies to; empty for none.")]
    public string actorKey;
    public ActorMove move;
}

[Serializable]
public struct IntroScene
{
    public string title;
    public Sprite background;
    [Tooltip("Characters already standing when the scene starts (left to right).")]
    public string[] actorsOnStage;
    public IntroLine[] lines;
}

/// <summary>The opening story shown before the first tutorial. Edit Assets/Data/IntroContent.asset.</summary>
[CreateAssetMenu(menuName = "ScriptableObject/Intro Content")]
public sealed class IntroContentSO : ScriptableObject
{
    public const string NarratorKey = "narrator";

    public IntroCharacter[] characters;
    public IntroScene[] scenes;

    public bool TryGetCharacter(string key, out IntroCharacter character)
    {
        if (characters != null)
        {
            foreach (IntroCharacter c in characters)
            {
                if (c.key != key) continue;
                character = c;
                return true;
            }
        }
        character = default;
        return false;
    }

    public int[] LinesPerScene()
    {
        var counts = new int[scenes != null ? scenes.Length : 0];
        for (int i = 0; i < counts.Length; i++) counts[i] = scenes[i].lines != null ? scenes[i].lines.Length : 0;
        return counts;
    }
}
