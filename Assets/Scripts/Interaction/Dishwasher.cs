using System;
using UnityEngine;

public class Dishwasher : MonoBehaviour, IInteractable
{
    [SerializeField] private int bowlCapacity = 30;
    [SerializeField, Range(0f, 1f)] private float passRatio = 0.9f;
    [SerializeField] private int dirtSpots = 4;

    private readonly System.Random random = new System.Random();
    private DirtMask currentMask;

    /// <summary>Bowls waiting in the machine, including the one being scrubbed.</summary>
    public int DirtyBowls { get; private set; }

    /// <summary>Dirt of the bowl being scrubbed; kept while the screen is closed. Null when empty.</summary>
    public DirtMask CurrentMask
    {
        get
        {
            if (currentMask == null && DirtyBowls > 0)
            {
                currentMask = DirtMask.Generate(random, dirtSpots);
            }
            return currentMask;
        }
    }

    public event Action DishLoaded;

    public bool CanInteract(PlayerInteractor player)
    {
        return player != null && !DishWashingMode.IsOpen && (player.HasDish || DirtyBowls > 0);
    }

    public void Interact(PlayerInteractor player)
    {
        if (!CanInteract(player)) return;

        // Unload everything the player carries, or nothing if it would overflow the machine.
        if (player.HasDish && DirtyBowls + player.CarriedBowls <= bowlCapacity)
        {
            while (player.HasDish)
            {
                DirtyBowls += player.RemoveDish().BowlCount;
                DishLoaded?.Invoke();
            }
        }

        if (DirtyBowls > 0)
        {
            DishWashingMode.Open(player, this);
        }
    }

    /// <summary>
    /// Scores the bowl being scrubbed and moves on to the next one.
    /// Returns the money change, or 0 if there was no bowl.
    /// </summary>
    public int FinishCurrentBowl(out bool clean)
    {
        clean = false;
        if (DirtyBowls == 0) return 0;

        clean = CurrentMask.CleanRatio >= passRatio;
        DirtyBowls--;
        currentMask = null;
        return GameManager.Instance != null ? GameManager.Instance.BowlWashed(clean) : 0;
    }
}
