using UnityEngine;

public class DishPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private int bowlCount = 1;

    public int BowlCount => bowlCount;
    public Sprite Sprite => GetComponent<SpriteRenderer>().sprite;

    // Called by BowlSpawn right after Instantiate.
    public void Setup(BowlSO bowl)
    {
        bowlCount = bowl.bowlCount;
        GetComponent<SpriteRenderer>().sprite = bowl.sprite;
    }

    public bool CanInteract(PlayerInteractor player)
    {
        return player.CanCarryMore;
    }

    public void Interact(PlayerInteractor player)
    {
        player.AddDish(this);
        // SetActive rather than Destroy: the collider must disappear this frame so the dish can't be taken twice.
        gameObject.SetActive(false);
    }
}
