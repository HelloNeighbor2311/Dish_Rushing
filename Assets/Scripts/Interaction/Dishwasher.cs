using System;
using UnityEngine;

public class Dishwasher : MonoBehaviour, IInteractable
{
    [SerializeField] private int bowlCapacity = 30;

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

        // Unload everything the player carries, or nothing if it would overflow the machine.
        if (player.HasDish && loadedBowls + player.CarriedBowls <= bowlCapacity)
        {
            while (player.HasDish)
            {
                DishPickup dish = player.RemoveDish();
                loadedDishes++;
                loadedBowls += dish.BowlCount;
                DishLoaded?.Invoke();
            }
            Debug.Log($"Dishwasher loaded: {loadedDishes} dish(es), {loadedBowls}/{bowlCapacity} bowl(s)");
        }

        // Reopening keeps the same dirty dish. Washing itself is a later task.
        DishWashingMode.Open(player);
    }
}
