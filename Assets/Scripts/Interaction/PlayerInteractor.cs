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
    [SerializeField] private int maxCarriedDishes = 1;

    private readonly List<Collider2D> hits = new List<Collider2D>();
    private readonly Dictionary<Collider2D, IInteractable> interactables = new Dictionary<Collider2D, IInteractable>();
    private CircleCollider2D feetCollider;
    private ContactFilter2D filter;
    private int carriedDishes;

    public bool HasDish => carriedDishes > 0;
    public bool CanCarryMore => carriedDishes < maxCarriedDishes;

    private void Awake()
    {
        feetCollider = GetComponent<CircleCollider2D>();
        filter = new ContactFilter2D();
        filter.SetLayerMask(interactableLayer);
        filter.useTriggers = true;
        carriedDishVisual.SetActive(false);
    }

    private void Update()
    {
        Collider2D target = FindNearest(out _);
        interactPrompt.SetActive(target != null);
        if (target != null)
        {
            interactPrompt.transform.position = target.transform.position + promptOffset;
        }
    }

    // Wired to the Player/Interact action on PlayerInput.
    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        FindNearest(out IInteractable interactable);
        interactable?.Interact(this);
    }

    public void AddDish()
    {
        if (!CanCarryMore)
        {
            return;
        }

        carriedDishes++;
        carriedDishVisual.SetActive(true);
    }

    public void RemoveDish()
    {
        if (!HasDish)
        {
            return;
        }

        carriedDishes--;
        carriedDishVisual.SetActive(HasDish);
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
