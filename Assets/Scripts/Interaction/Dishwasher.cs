using System;
using UnityEngine;

public class Dishwasher : MonoBehaviour, IInteractable
{
    [SerializeField] private int capacity = 1;

    private int loadedDishes;
    private int loadedBowls;

    public event Action DishLoaded;

    public bool CanInteract(PlayerInteractor player)
    {
        return player != null && !DishWashingMode.IsOpen;
    }

    public void Interact(PlayerInteractor player)
    {
        if (!CanInteract(player)) return;

        if (player.HasDish && loadedDishes < capacity)
        {
            DishPickup dish = player.RemoveDish();
            loadedDishes++;
            loadedBowls += dish.BowlCount;
            Debug.Log($"Dishwasher loaded: {loadedDishes}/{capacity} dish(es), {loadedBowls} bowl(s)");
            DishLoaded?.Invoke();
        }

        // Reopening keeps the same dirty dish. Washing itself is a later task.
        DishWashingMode.Open(player);
    }
}
