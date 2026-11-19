using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame

    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Bắt đầu game");
    }
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Thoát game");
    }
}
