using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Tutorial overlay (prefab Resources/TutorialUI): current objective, Skip button, screen-edge arrow.</summary>
public sealed class TutorialHud : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private Button skipButton;
    [SerializeField] private RectTransform edgeArrow;
    [SerializeField] private GameObject remarkPanel;
    [SerializeField] private TMP_Text remarkText;
    [SerializeField] private float remarkSeconds = 3.5f;

    private static TutorialHud instance;
    private Action onSkip;
    private float remarkUntil;
    private bool washingLayout;
    private Vector2 normalAnchor;
    private Vector2 normalPivot;
    private Vector2 normalPosition;

    public RectTransform EdgeArrow => edgeArrow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() { instance = null; }

    public static TutorialHud Get()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<TutorialHud>("TutorialUI"));
            instance.name = "TutorialUI";
            instance.skipButton.onClick.AddListener(instance.OnSkipClicked);
            var panel = (RectTransform)instance.objectiveText.transform.parent;
            instance.normalAnchor = panel.anchorMin;
            instance.normalPivot = panel.pivot;
            instance.normalPosition = panel.anchoredPosition;
        }
        return instance;
    }

    public void Show(Action skip)
    {
        onSkip = skip;
        edgeArrow.gameObject.SetActive(false);
        remarkPanel.SetActive(false);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        onSkip = null;
        gameObject.SetActive(false);
    }

    public void SetObjective(string text)
    {
        objectiveText.text = text;
    }

    /// <summary>
    /// The washing screen has its own title where the objective normally sits, so the objective
    /// (and remarks under it) move to the top-left corner while it is open.
    /// </summary>
    public void SetWashingLayout(bool washing)
    {
        if (washingLayout == washing) return;
        washingLayout = washing;
        var panel = (RectTransform)objectiveText.transform.parent;
        panel.anchorMin = panel.anchorMax = washing ? new Vector2(0f, 1f) : normalAnchor;
        panel.pivot = washing ? new Vector2(0f, 1f) : normalPivot;
        panel.anchoredPosition = washing ? new Vector2(24f, -24f) : normalPosition;
    }

    /// <summary>A short remark from the owner under the objective, for when her bubble is off screen.</summary>
    public void ShowRemark(string text)
    {
        remarkText.text = text;
        remarkPanel.SetActive(true);
        remarkUntil = Time.time + remarkSeconds;
    }

    private void Update()
    {
        if (remarkPanel.activeSelf && Time.time >= remarkUntil) remarkPanel.SetActive(false);
    }

    private void OnSkipClicked()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        onSkip?.Invoke();
    }
}
