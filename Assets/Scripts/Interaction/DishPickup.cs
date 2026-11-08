using UnityEngine;

public class DishPickup : MonoBehaviour, IInteractable
{
    public bool CanInteract(PlayerInteractor player)
    {
        return player.CanCarryMore;
    }

    public void Interact(PlayerInteractor player)
    {
        player.AddDish();
        // SetActive rather than Destroy: the collider must disappear this frame so the dish can't be taken twice.
        gameObject.SetActive(false);
    }
}
