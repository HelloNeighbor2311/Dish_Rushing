using System;
using UnityEngine;

public class Dishwasher : MonoBehaviour, IInteractable
{
    [SerializeField] private int capacity = 1;

    private int loadedDishes;

    public event Action DishLoaded;

    public bool CanInteract(PlayerInteractor player)
    {
        return player.HasDish && loadedDishes < capacity;
    }

    public void Interact(PlayerInteractor player)
    {
        player.RemoveDish();
        loadedDishes++;
        Debug.Log($"Dishwasher loaded: {loadedDishes}/{capacity}");
        DishLoaded?.Invoke();
    }
}
