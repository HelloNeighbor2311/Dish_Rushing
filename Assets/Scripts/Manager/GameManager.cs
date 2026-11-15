using System;
using UnityEngine;

/// <summary>Shift state: money and how many spawned bowls are still unwashed.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int cleanReward = 5;
    [SerializeField] private int dirtyPenalty = 3;

    public int Money { get; private set; }
    public int BowlsToWash { get; private set; }

    public event Action Changed;

    private void Awake()
    {
        Instance = this;
    }

    public void BowlsSpawned(int count)
    {
        BowlsToWash += count;
        Changed?.Invoke();
    }

    /// <summary>Returns the money change: positive for a clean bowl, negative for a dirty one.</summary>
    public int BowlWashed(bool clean)
    {
        int before = Money;
        Money = Mathf.Max(0, Money + (clean ? cleanReward : -dirtyPenalty));
        BowlsToWash = Mathf.Max(0, BowlsToWash - 1);
        Changed?.Invoke();
        return Money - before;
    }
}
