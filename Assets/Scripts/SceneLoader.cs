using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public enum SceneEnum
    {
        MainMenu,
        GamePlay
    }
    public static void LoadScene(SceneEnum scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }
}
