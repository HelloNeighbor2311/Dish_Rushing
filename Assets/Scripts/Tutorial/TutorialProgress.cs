using UnityEngine;

/// <summary>Remembers whether the player has finished (or skipped) the tutorial.</summary>
public static class TutorialProgress
{
    public const string Key = "DishRushing.TutorialDone";

    public static bool IsDone => PlayerPrefs.GetInt(Key, 0) == 1;

    public static void MarkDone()
    {
        PlayerPrefs.SetInt(Key, 1);
        PlayerPrefs.Save();
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
