using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Keeps the money and "bowls to wash" HUD texts in sync with GameManager.</summary>
public class HudStats : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text needWashingText;
    [SerializeField] private TMP_Text brokenBowlText;
    [SerializeField] private Image staminaFill;
    [SerializeField] private TMP_Text staminaText;
    private Player player;
    private PlayerInteractor interactor;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (player != null) interactor = player.GetComponent<PlayerInteractor>();
        if (staminaText == null && staminaFill != null && moneyText != null)
        {
            var label = new GameObject("StaminaAndCarryCount", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(staminaFill.transform.parent, false);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            staminaText = label.GetComponent<TextMeshProUGUI>();
            staminaText.font = moneyText.font;
            staminaText.fontSize = 14;
            staminaText.color = Color.black;
            staminaText.alignment = TextAlignmentOptions.Center;
            staminaText.raycastTarget = false;
        }
        GameManager.Instance.Changed += Refresh;
        Refresh();
    }

    private void Update()
    {
        if (player == null) return;
        float ratio = player.StaminaRatio;
        if (staminaFill != null)
        {
            staminaFill.rectTransform.anchorMax = new Vector2(ratio, 1f);
            staminaFill.color = ratio <= 0.2f ? new Color32(240, 68, 68, 255) :
                ratio <= 0.4f ? new Color32(255, 194, 71, 255) : new Color32(54, 201, 165, 255);
        }
        if (staminaText != null)
        {
            int bowls = interactor != null ? interactor.CarriedBowls : 0;
            int capacity = interactor != null ? interactor.MaxCarriedBowls : 0;
            staminaText.text = $"{Mathf.CeilToInt(ratio * 100f)}% | {bowls}/{capacity}";
            if (player.IsStaminaLow) staminaText.text += " | REST";
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.Changed -= Refresh;
    }

    private void Refresh()
    {
        moneyText.text = GameManager.Instance.Money.ToString();
        needWashingText.text = $"Cần Rửa: {GameManager.Instance.BowlsToWash}";
        if (brokenBowlText != null) brokenBowlText.text = $"Bát Vỡ: {GameManager.Instance.BrokenBowls}";
    }
}
