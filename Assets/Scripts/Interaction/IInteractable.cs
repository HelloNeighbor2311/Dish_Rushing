public interface IInteractable
{
    bool CanInteract(PlayerInteractor player);
    void Interact(PlayerInteractor player);
}
