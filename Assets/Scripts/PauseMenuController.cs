
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject exitConfirmationPanel;

    private bool isPaused = false;

    private void Update()
    {
        // Nhấn ESC để bật/tắt Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                // Nếu đang mở Settings thì quay lại Pause
                BackFromSettings();
            }
            else if (isPaused)
            {
                ContinueGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Dừng game và mở Pause Panel
    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
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
        Time.timeScale = 1f;
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
