
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject exitConfirmationPanel;

    private bool isPaused = false;

    private GameInput gameInput;

    private void Start()
    {
        gameInput = GameInput.instance;
        gameInput.PausePressed += HandlePausePressed;
    }

    private void OnDestroy()
    {
        if (gameInput != null) gameInput.PausePressed -= HandlePausePressed;
    }

    private void HandlePausePressed()
    {
        if (exitConfirmationPanel.activeSelf)
            CancelExit();
        else if (settingsPanel.activeSelf)
            BackFromSettings();
        else if (isPaused)
            ContinueGame();
        else
            PauseGame();
    }

    // Dừng game và mở Pause Panel
    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        GameManager.Instance.PauseGame();
        InputModeManager.instance.EnterUIMode();
    }

    // Mở Settings
    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Quay lại Pause Menu
    public void BackFromSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // Tiếp tục chơi
    public void ContinueGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);
        GameManager.Instance.UnPauseGame();
        InputModeManager.instance.EnterGameplayMode();
    }

    // Trở về Main Menu
    public void ReturnToMainMenu()
    {
        ShowExitConfirmation();
    }
    public void ShowExitConfirmation()
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
        SceneManager.LoadScene("MainMenu");
    }
}
