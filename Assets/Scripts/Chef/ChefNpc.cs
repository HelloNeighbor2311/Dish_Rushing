using TMPro;
using UnityEngine;

/// <summary>Chef offers a limited energy drink during each configured shift window.</summary>
public sealed class ChefNpc : MonoBehaviour, IInteractable
{
    [SerializeField] private bool helpEnabled = true;
    [SerializeField] private float[] offerAtSeconds = { 20f, 70f, 120f };
    [SerializeField, Min(1f)] private float offerDuration = 20f;
    [SerializeField, Min(1f)] private float staminaRecovery = 40f;

    private ChefHelpSupply supply;
    private CountDownTime timer;
    private TutorialController tutorial;
    private Player recipient;
    private TextMeshPro marker;
    private ChefHelpHud hud;
    private bool shiftEnded;
    public int Drinks => supply != null ? supply.Drinks : 0;
    public bool HelpAvailable => helpEnabled && !shiftEnded && supply != null && supply.Available;
    public float StaminaRecovery => staminaRecovery;
    public bool ShowHud => helpEnabled && !shiftEnded && timer != null && timer.TimeRemaining > 0f &&
        !IntroPlayer.IsPlaying && (tutorial == null || !tutorial.IsRunning);
    public bool CanUseDrink => helpEnabled && !shiftEnded && Time.timeScale > 0f && recipient != null &&
        recipient.StaminaRatio < 0.999f && Drinks > 0;

    private void Awake()
    {
        supply = new ChefHelpSupply(offerAtSeconds, offerDuration);
    }

    private void Start()
    {
        timer = FindFirstObjectByType<CountDownTime>();
        tutorial = FindFirstObjectByType<TutorialController>();
        recipient = FindFirstObjectByType<Player>();
        if (timer != null) timer.Finished += OnShiftEnded;
        marker = new GameObject("ChefHelpMarker", typeof(TextMeshPro)).GetComponent<TextMeshPro>();
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        marker.fontSize = 5f;
        marker.alignment = TextAlignmentOptions.Center;
        marker.color = new Color32(255, 211, 76, 255);
        marker.outlineWidth = 0.15f;
        marker.rectTransform.sizeDelta = new Vector2(2f, 1f);
        marker.GetComponent<MeshRenderer>().sortingOrder = 30000;
        marker.gameObject.SetActive(false);
        hud = ChefHelpHud.Create(this);
    }

    private void Update()
    {
        if (timer == null || shiftEnded) return;
        if (timer.TimeRemaining <= 0f) { OnShiftEnded(); return; }
        if (helpEnabled && (tutorial == null || !tutorial.IsRunning))
            supply.Tick(Mathf.Max(0f, timer.totalTime - timer.TimeRemaining));
        marker.gameObject.SetActive(HelpAvailable);
        marker.text = "!";
    }

    public bool CanInteract(PlayerInteractor player)
    {
        return player != null && !shiftEnded && Time.timeScale > 0f &&
            !DialogueBox.IsOpen && !DishWashingMode.IsOpen && !IntroPlayer.IsPlaying &&
            (tutorial == null || !tutorial.IsRunning);
    }

    public void Interact(PlayerInteractor player)
    {
        if (!CanInteract(player)) return;
        recipient = player.GetComponent<Player>();
        if (recipient == null) return;
        if (HelpAvailable && supply.Claim())
        {
            marker.gameObject.SetActive(false);
            DialogueBox.ShowChoice(Line($"Cầm chai nước tăng lực này nhé! Hồi {staminaRecovery:0} thể lực. Em muốn uống luôn hay để dành?"),
                new[] { "Uống ngay", "Để dành" }, choice =>
                {
                    if (this == null || shiftEnded) return;
                    if (choice == 0)
                    {
                        bool used = TryUseDrink();
                        DialogueBox.Show(new[] { Line(used ? "Khỏe hơn rồi đấy! Nhớ bê vừa sức thôi nhé."
                            : "Thể lực đang đầy rồi. Anh để chai nước trên HUD cho em dùng sau nhé.") });
                    }
                    else DialogueBox.Show(new[] { Line("Anh để nước trên HUD rồi nhé. Khi mệt, bấm nút Uống hoặc phím R để dùng.") });
                });
            return;
        }
        DialogueBox.Show(new[] { Line(helpEnabled
            ? "Bê càng nhiều bát càng nhanh mệt. Đứng nghỉ sẽ hồi thể lực. Khi anh có nước tăng lực, dấu ! sẽ hiện trên đầu; nhớ ghé nhận trước khi hết giờ nhé!"
            : "Ca này không có nước hỗ trợ. Nhớ bê vừa sức và đứng nghỉ khi mệt nhé!") });
    }

    public bool TryUseDrink()
    {
        if (!CanUseDrink || !supply.Consume()) return false;
        recipient.RestoreStamina(staminaRecovery);
        return true;
    }

    private static DialogueLine Line(string text) => new DialogueLine { speaker = "Đầu bếp", text = text };

    private void OnShiftEnded()
    {
        shiftEnded = true;
        if (marker != null) marker.gameObject.SetActive(false);
        if (hud != null) hud.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (timer != null) timer.Finished -= OnShiftEnded;
        if (hud != null) Destroy(hud.gameObject);
    }
}
