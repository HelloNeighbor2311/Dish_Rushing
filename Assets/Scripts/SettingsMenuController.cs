
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pausePanel;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Sliders")]
    public Slider sensitivitySlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Value Texts")]
    public TMP_Text sensitivityValue;
    public TMP_Text musicValue;
    public TMP_Text sfxValue;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private const string SensitivityKey = "Sensitivity";
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";

    public static float CursorSensitivity { get; private set; } = 1f;

    private bool isLoading;

    private void Start()
    {
        LoadSettings();
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        // Không thay đổi Time.timeScale ở đây.
        // Nếu đang mở từ Pause Menu, game vẫn tiếp tục tạm dừng.
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    private void LoadSettings()
    {
        isLoading = true;

        float sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SFXKey, 1f);

        sensitivitySlider.value = sensitivity;
        musicSlider.value = music;
        sfxSlider.value = sfx;

        isLoading = false;

        UpdateSensitivity(sensitivity);
        UpdateMusic(music);
        UpdateSFX(sfx);
    }

    public void UpdateSensitivity(float value)
    {
        CursorSensitivity = value;

        sensitivityValue.text = value.ToString("0.0") + "x";

        if (!isLoading)
        {
            PlayerPrefs.SetFloat(SensitivityKey, value);
            PlayerPrefs.Save();
        }
    }

    public void UpdateMusic(float value)
    {
        SetMixerVolume("MusicVolume", value);

        musicValue.text = Mathf.RoundToInt(value * 100) + "%";

        if (!isLoading)
        {
            PlayerPrefs.SetFloat(MusicKey, value);
            PlayerPrefs.Save();
        }
    }

    public void UpdateSFX(float value)
    {
        SetMixerVolume("SFXVolume", value);

        sfxValue.text = Mathf.RoundToInt(value * 100) + "%";

        if (!isLoading)
        {
            PlayerPrefs.SetFloat(SFXKey, value);
            PlayerPrefs.Save();
        }
    }

    private void SetMixerVolume(string parameter, float value)
    {
        float decibels = value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(parameter, decibels);
    }
}
