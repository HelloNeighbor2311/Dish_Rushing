using System;
using UnityEngine;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    [SerializeField, TextArea] private string tutorialMessage = "Giữ chuột và chà quanh chiếc đĩa cho đến khi hết vết bẩn.";
    [SerializeField] private GameObject rewardNotice;
    [SerializeField] private bool hasReward;

    public event Action RewardGiven;

    private void Start()
    {
        rewardNotice.SetActive(hasReward);
    }

    public bool CanInteract(PlayerInteractor player)
    {
        return true;
    }

    public void Interact(PlayerInteractor player)
    {
        if (!hasReward)
        {
            Debug.Log(tutorialMessage);
            return;
        }

        hasReward = false;
        rewardNotice.SetActive(false);
        Debug.Log("NPC reward given");
        RewardGiven?.Invoke();
    }
}
