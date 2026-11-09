using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTime : MonoBehaviour
{
    [Header("Timer Settings")]
    public float totalTime = 120f;
    private float timeRemaining;

    [Header("UI")]
    public Image timerFill;
    public TMP_Text timerText;

    [Header("Timer Colors")]
    public Color normalColor = new Color32(54, 201, 165, 255);
    public Color warningColor = new Color32(255, 194, 71, 255);
    public Color dangerColor = new Color32(240, 68, 68, 255);

    private bool isRunning = true;

    void Start()
    {
        timeRemaining = totalTime;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!isRunning)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            timeRemaining = Mathf.Max(0, timeRemaining);

            UpdateTimerUI();
        }
        else
        {
            isRunning = false;
            GameOver();
        }
    }

    void UpdateTimerUI()
    {
        if (timerFill == null || timerText == null)
        {
            Debug.LogError("Bạn chưa gán TimerFill hoặc TimerText!");
            return;
        }

        float progress = timeRemaining / totalTime;


        // Cập nhật chiều dài thanh thời gian
        timerFill.fillAmount = progress;

        // Thay đổi màu theo thời gian còn lại
        if (progress > 0.5f)
        {
            timerFill.color = normalColor;
        }
        else if (progress > 0.2f)
        {
            timerFill.color = warningColor;
        }
        else
        {
            timerFill.color = dangerColor;
        }

        // Hiển thị phút và giây
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        timerText.text = string.Format("{0:00}:{1:00}",
            minutes, seconds);
    }

    void GameOver()
    {
        timerText.text = "00:00";
        Debug.Log("HẾT THỜI GIAN!");
    }
}
