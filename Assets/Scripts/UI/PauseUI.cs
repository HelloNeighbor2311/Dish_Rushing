using System;
using UnityEngine.UI;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button ExitBtn;
    private void Start()
    {
        GameManager.Instance.onGamePause += OnGamePaused;
        GameManager.Instance.onGameUnPause += OnGameUnPaused;
        Hide();
        continueBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.PauseUnpauseGame();
            InputModeManager.instance.SwitchMode();
        });
        ExitBtn.onClick.AddListener(() =>
        {
           SceneLoader.LoadScene(SceneLoader.SceneEnum.MainMenu); 
        });
    }

    private void OnGameUnPaused()
    {
        Hide();
    }

    private void OnGamePaused()
    {
        Show();
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
