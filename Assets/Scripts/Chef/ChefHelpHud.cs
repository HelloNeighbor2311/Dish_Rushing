using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Persistent collected-drink slot, also reachable while washing.</summary>
public sealed class ChefHelpHud : MonoBehaviour
{
    private ChefNpc chef;
    private TMP_Text status;
    private Button use;
    private GameObject panel;

    public static ChefHelpHud Create(ChefNpc chef)
    {
        var root = new GameObject("ChefHelpHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.GetComponent<Canvas>().sortingOrder = 15;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        var hud = root.AddComponent<ChefHelpHud>();
        hud.chef = chef;
        var panel = new GameObject("EnergyDrinkSlot", typeof(RectTransform), typeof(Image));
        hud.panel = panel;
        panel.transform.SetParent(root.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -12f);
        rect.sizeDelta = new Vector2(410f, 66f);
        panel.GetComponent<Image>().color = new Color32(37, 58, 55, 240);
        hud.status = Label(panel.transform, "Status", new Vector2(-52f, 0f), new Vector2(290f, 60f));
        var button = new GameObject("UseDrink", typeof(RectTransform), typeof(Image), typeof(Button));
        button.transform.SetParent(panel.transform, false);
        var br = button.GetComponent<RectTransform>();
        br.anchoredPosition = new Vector2(147f, 0f);
        br.sizeDelta = new Vector2(104f, 46f);
        button.GetComponent<Image>().color = new Color32(56, 119, 99, 255);
        hud.use = button.GetComponent<Button>();
        hud.use.targetGraphic = button.GetComponent<Image>();
        hud.use.onClick.AddListener(() => chef.TryUseDrink());
        Label(button.transform, "Label", Vector2.zero, br.sizeDelta).text = "Uống [R]";
        return hud;
    }

    private static TMP_Text Label(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(parent, false);
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = size;
        var dialogue = Resources.Load<DialogueBox>("DialogueUI");
        if (dialogue != null) text.font = dialogue.GetComponentInChildren<TMP_Text>(true).font;
        text.fontSize = 18f;
        text.color = new Color32(255, 243, 211, 255);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private void Update()
    {
        panel.SetActive(chef.ShowHud);
        if (!chef.ShowHud) return;
        bool blocked = Time.timeScale <= 0f || DialogueBox.IsOpen || IntroPlayer.IsPlaying;
        status.text = $"Nước tăng lực: {chef.Drinks}  (+{chef.StaminaRecovery:0})\n" +
            (chef.HelpAvailable ? "Đầu bếp đang có trợ giúp [!]" : "Nhận nước khi đầu bếp hiện [!]");
        use.interactable = !blocked && chef.CanUseDrink;
        if (!blocked && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) chef.TryUseDrink();
    }
}
