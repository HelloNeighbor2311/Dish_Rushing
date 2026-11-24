using System;
using UnityEngine;

/// <summary>Shift state: money and how many spawned bowls are still unwashed.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int cleanReward = 5;
    [SerializeField] private int dirtyPenalty = 3;
    [SerializeField, Min(0)] private int brokenPenalty = 3;

    public event Action onGamePause;
    public event Action onGameUnPause;
    public int Money { get; private set; }
    public int BowlsToWash { get; private set; }
    public int BrokenBowls { get; private set; }

    public event Action Changed;
    /// <summary>Raised after a bowl is scored; true when it was clean.</summary>
    public event Action<bool> BowlWashedEvent;
    /// <summary>Raised with the number of bowls that just broke (never 0).</summary>
    public event Action<int> BowlBrokenEvent;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        GameInput.instance.PausePressed += OnPausePressed;
    }

    private void OnDestroy()
    {
        if (GameInput.instance != null)
            GameInput.instance.PausePressed -= OnPausePressed;

        if (Instance == this)
            Instance = null;
    }

    private void OnPausePressed()
    {
        PauseUnpauseGame();
    }

    public void BowlsSpawned(int count)
    {
        BowlsToWash += count;
        Changed?.Invoke();
    }

    ///Returns the money change: positive for a clean bowl, negative for a dirty one
    public int BowlWashed(bool clean)
    {
        int before = Money;
        Money = Mathf.Max(0, Money + (clean ? cleanReward : -dirtyPenalty));
        BowlsToWash = Mathf.Max(0, BowlsToWash - 1);
        Changed?.Invoke();
        BowlWashedEvent?.Invoke(clean);
        return Money - before;
    }

    public void BowlBroken(int count = 1)
    {
        count = Mathf.Max(0, count);
        if (count == 0) return;

        BrokenBowls += count;
        Money = Mathf.Max(0, Money - brokenPenalty * count);
        // Broken bowls leave the unwashed inventory, keeping HUD counts consistent.
        BowlsToWash = Mathf.Max(0, BowlsToWash - count);
        Changed?.Invoke();
        BowlBrokenEvent?.Invoke(count);
    }
    public void PauseUnpauseGame()
    {
        if(Time.timeScale >= 1.0f)
        {
            PauseGame();
        }
        else
        {
            UnPauseGame();
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0.0f;
        onGamePause?.Invoke();
    }
    public void UnPauseGame()
    {
        Time.timeScale = 1.0f;
        onGameUnPause?.Invoke();
    }
}
