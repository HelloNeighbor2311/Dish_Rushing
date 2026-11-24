using System;
using UnityEngine;

/// <summary>A small list of lines (Unity cannot show arrays of arrays in the Inspector).</summary>
[Serializable]
public struct StepLines
{
    [TextArea] public string[] lines;
}

/// <summary>
/// Everything the shop owner says. Edit the text in Assets/Data/OwnerDialogue.asset.
/// The script these lines come from is docs/design/intro-tutorial-script.md.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Owner Dialogue")]
public sealed class OwnerDialogueSO : ScriptableObject
{
    [Header("Who is speaking")]
    public string speakerName = "Bà chủ";
    public Sprite portrait;

    [Header("Tutorial")]
    [TextArea] public string[] welcome;
    [TextArea] public string[] startShift;
    [Tooltip("One line per TutorialStep, in enum order (Welcome ... StartShift).")]
    [TextArea] public string[] stepObjectives;
    [Tooltip("Said when steps Move, PickUp, Stamina, LoadWasher, Scrub begin (5 lines).")]
    [TextArea] public string[] stepIntro;
    [Tooltip("Up to 3 nudges per step (Move ... Scrub) when the player stands still too long.")]
    public StepLines[] stepIdle;
    [Tooltip("Said when steps Move ... Scrub are done (5 lines). Scrub uses this for a clean bowl.")]
    [TextArea] public string[] stepCompleted;
    [TextArea] public string scrubDirtyReaction;
    [Tooltip("[0] = broke before picking up, [1] = broke on the way to the sink.")]
    [TextArea] public string[] dishBrokenDuringTutorial;

    [Header("Talking with E")]
    [TextArea] public string[] talkPrompts;
    public string[] talkOptions = { "Chị cho em xin bí kíp", "Chị dạy lại từ đầu đi ạ" };
    [TextArea] public string[] tips;
    [Tooltip("Said instead of a tip after asking for every tip in a row.")]
    [TextArea] public string tipsEasterEgg;
    [TextArea] public string replayAck;

    [Header("Speech bubbles")]
    public string[] barkBroken;
    public string[] barkDirty;
    public string[] barkPileUp;
    public string[] barkTimeLow;
    public string[] barkCleanStreak;

    [Header("End of shift (not used yet)")]
    [TextArea] public string[] winShift;
    [TextArea] public string loseTime;
    [TextArea] public string loseBroken;
    [TextArea] public string chefEndShift;

    /// <summary>Wraps plain strings as dialogue lines spoken by the owner.</summary>
    public DialogueLine[] AsLines(string[] texts)
    {
        if (texts == null) return new DialogueLine[0];
        var result = new DialogueLine[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            result[i] = new DialogueLine { speaker = speakerName, portrait = portrait, text = texts[i] };
        }
        return result;
    }
}
