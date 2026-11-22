using TMPro;
using UnityEngine;

/// <summary>Keeps the money and "bowls to wash" HUD texts in sync with GameManager.</summary>
public class HudStats : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text needWashingText;

    private void Start()
    {
        GameManager.Instance.Changed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.Changed -= Refresh;
    }

    private void Refresh()
    {
        moneyText.text = GameManager.Instance.Money.ToString();
        needWashingText.text = $"Cần Rửa: {GameManager.Instance.BowlsToWash}";
    }
}
