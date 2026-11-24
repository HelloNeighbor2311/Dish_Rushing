using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject carriedDishVisual;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float interactRadius = 1.2f;
    [SerializeField] private int maxCarriedBowls = 10;
    [SerializeField] private List<Sprite> BowlSpritesList;

    private readonly List<Collider2D> hits = new List<Collider2D>();
    private readonly Dictionary<Collider2D, IInteractable> interactables = new Dictionary<Collider2D, IInteractable>();
    private CircleCollider2D feetCollider;
    private ContactFilter2D filter;
    private readonly List<DishPickup> carriedDishes = new List<DishPickup>();
    private SpriteRenderer carriedDishRenderer;
    private Animator carriedDishAnimator;
    private Vector3 carryPosition;
    private bool playingBowlBreak;
    private static readonly int BowlBreakState = Animator.StringToHash("Base Layer.BowlsBreak");
    private int lastInteractionFrame = -1;
    private InputAction interactAction;
    private Collider2D currentTarget;
    private Player movement;

    /// <summary>Raised after a dish stack was actually added to the player's hands.</summary>
    public event Action<DishPickup> DishPicked;

    public bool HasDish => carriedDishes.Count > 0;
    public int CarriedBowls { get; private set; }

    public int MaxCarriedBowls => maxCarriedBowls;
    public bool CanCarry(int bowls) => bowls > 0 && CarriedBowls + bowls <= maxCarriedBowls &&
        (movement == null || movement.CanLiftBowls);

    private void Awake()
    {
        feetCollider = GetComponent<CircleCollider2D>();
        movement = GetComponent<Player>();
        if (interactableLayer.value == 0)
            interactableLayer = LayerMask.GetMask("Interactable");
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
            interactAction = playerInput.actions.FindAction("Player/Interact", false);
        filter = new ContactFilter2D();
        filter.SetLayerMask(interactableLayer);
        filter.useTriggers = true;
        if (carriedDishVisual != null)
        {
            carriedDishRenderer = carriedDishVisual.GetComponent<SpriteRenderer>();
            carryPosition = carriedDishVisual.transform.localPosition;
            carriedDishAnimator = carriedDishVisual.GetComponent<Animator>();
            if (carriedDishAnimator != null) carriedDishAnimator.enabled = false;
            carriedDishVisual.SetActive(false);
        }
    }
    private void Start()
    {
        GameInput.instance.InteractPressed += OnInteractPressed;
    }

    private void OnInteractPressed()
    {
        TryInteract();
    }

    private void Update()
    {
        Collider2D target = FindNearest(out _);
        currentTarget = target;
        if (interactPrompt != null) interactPrompt.SetActive(target != null);
        if (target != null && interactPrompt != null)
        {
            interactPrompt.transform.position = target.transform.position + promptOffset;
        }
        // Supports scenes where PlayerInput's UnityEvent has not been wired.
        if (interactAction != null && interactAction.WasPressedThisFrame()) TryInteract();
    }

    private void OnDisable()
    {
        if (playingBowlBreak) FinishBowlBreak();
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }
    private void OnDestroy()
    {
        if (GameInput.instance != null) GameInput.instance.InteractPressed -= OnInteractPressed;
    }

    public int BreakBowls()
    {
        if (!HasDish) return 0;

        int brokenCount = UnityEngine.Random.Range(1, CarriedBowls + 1);
        int remaining = brokenCount;

        for (int i = carriedDishes.Count - 1; i >= 0 && remaining > 0; i--)
        {
            DishPickup dish = carriedDishes[i];
            int removeCount = Mathf.Min(remaining, dish.BowlCount);
            if (!dish.RemoveBowls(removeCount))
            {
                continue;
            }

            remaining -= removeCount;
            if (dish.BowlCount == 0)
            {
                carriedDishes.RemoveAt(i);
                Destroy(dish.gameObject);
            }
        }

        int actualBrokenCount = brokenCount - remaining;
        CarriedBowls -= actualBrokenCount;
        if (actualBrokenCount <= 0) return 0;

        PlayBowlBreak();
        return actualBrokenCount;
    }

    private void PlayBowlBreak()
    {
        if (carriedDishAnimator == null)
        {
            UpdateCarriedVisual();
            return;
        }

        carriedDishVisual.SetActive(true);
        carriedDishAnimator.enabled = true;
        playingBowlBreak = true;
        carriedDishAnimator.Play(BowlBreakState, 0, 0f);
        carriedDishAnimator.Update(0f);
    }

    private void LateUpdate()
    {
        if (!playingBowlBreak) return;
        AnimatorStateInfo state = carriedDishAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash != BowlBreakState || state.normalizedTime >= 1f)
            FinishBowlBreak();
    }

    private void FinishBowlBreak()
    {
        playingBowlBreak = false;
        carriedDishAnimator.enabled = false;
        carriedDishVisual.transform.localPosition = carryPosition;
        UpdateCarriedVisual();
    }
    private void TryInteract()
    {
        if (!isActiveAndEnabled || DishWashingMode.IsOpen || lastInteractionFrame == Time.frameCount) return;
        lastInteractionFrame = Time.frameCount;
        FindNearest(out IInteractable interactable);
        interactable?.Interact(this);
    }

    private void OnGUI()
    {
        if (DishWashingMode.IsOpen || currentTarget == null || interactPrompt != null) return;
        var camera = Camera.main;
        if (camera == null) return;
        Vector3 p = camera.WorldToScreenPoint(currentTarget.transform.position + promptOffset);
        if (p.z < 0) return;
        GUI.Box(new Rect(p.x - 80, Screen.height - p.y - 18, 160, 32), "[E] Interact");
    }

    public void AddDish(DishPickup dish)
    {
        if (!CanCarry(dish.BowlCount))
        {
            return;
        }

        carriedDishes.Add(dish);
        CarriedBowls += dish.BowlCount;
        Debug.Log(CarriedBowls);
        UpdateCarriedVisual();
        DishPicked?.Invoke(dish);
    }

    // Returns the most recently picked-up dish, or null when empty-handed.
    public DishPickup RemoveDish()
    {
        if (!HasDish)
        {
            return null;
        }

        DishPickup dish = carriedDishes[carriedDishes.Count - 1];
        carriedDishes.RemoveAt(carriedDishes.Count - 1);
        CarriedBowls -= dish.BowlCount;
        UpdateCarriedVisual();
        return dish;
    }

    private void UpdateCarriedVisual()
    {
        if (carriedDishVisual == null || playingBowlBreak) return;
        carriedDishVisual.SetActive(HasDish);
        if (!HasDish || carriedDishRenderer == null) return;

        int spriteIndex = CarriedBowls - 1;
        if (BowlSpritesList == null || spriteIndex < 0 || spriteIndex >= BowlSpritesList.Count)
        {
            Debug.LogWarning($"No carried visual configured for {CarriedBowls} bowl(s).");
            return;
        }

        carriedDishRenderer.sprite = BowlSpritesList[spriteIndex];
    }

    private Collider2D FindNearest(out IInteractable nearest)
    {
        Vector2 feet = (Vector2)transform.position + feetCollider.offset;
        Physics2D.OverlapCircle(feet, interactRadius, filter, hits);

        Collider2D nearestCollider = null;
        nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (!interactables.TryGetValue(hit, out IInteractable candidate))
            {
                candidate = hit.GetComponent<IInteractable>();
                interactables[hit] = candidate;
            }

            float distance = Vector2.Distance(feet, hit.ClosestPoint(feet));
            if (candidate != null && distance < nearestDistance && candidate.CanInteract(this))
            {
                nearestCollider = hit;
                nearest = candidate;
                nearestDistance = distance;
            }
        }

        return nearestCollider;
    }
}
