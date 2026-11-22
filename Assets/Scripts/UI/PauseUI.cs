using System;
using UnityEngine.UI;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button ExitBtn;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject exitConfirmationPanel;

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
            ShowExitConfirmation();

        });
        settingBtn.onClick.AddListener(() =>
        {
           OpenSettings(); 
        });
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onGamePause -= OnGamePaused;
            GameManager.Instance.onGameUnPause -= OnGameUnPaused;
        }
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
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Quay lại Pause Menu
    public void BackFromSettings()
    {
        settingsPanel.SetActive(false);
        Show();
    }
    private void ShowExitConfirmation()
    {
        exitConfirmationPanel.SetActive(true);
    }

    public void CancelExit()
    {
        exitConfirmationPanel.SetActive(false);
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene(SceneLoader.SceneEnum.MainMenu);
    }
}
