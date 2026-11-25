using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private CustomerController customerPrefab;
    [Tooltip("Optional instance of the primary prefab already placed in the scene.")]
    [SerializeField] private CustomerController firstCustomer;
    [System.Serializable]
    private struct CustomerVariant
    {
        public CustomerController prefab;
        public CustomerController placedCustomer;
    }
    [Tooltip("Each distinct prefab, including the primary prefab, has an equal chance per arrival.")]
    [SerializeField] private CustomerVariant[] additionalCustomers = new CustomerVariant[0];
    [SerializeField] private CustomerSeat[] seats;
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;
    [SerializeField] private CountDownTime shiftClock;
    private readonly List<SimplePool2.Pool> pools = new List<SimplePool2.Pool>();
    private bool held;

    /// <summary>Delays customer visits until Release. Call before Start (e.g. from another Awake).</summary>
    public void Hold() { held = true; }

    public void Release() { held = false; }

    private IEnumerator Start()
    {
        if (shiftClock != null) shiftClock.Finished += EndShift;
        if (seats == null || seats.Length == 0) yield break;

        var byPrefab = new Dictionary<CustomerController, SimplePool2.Pool>();
        RegisterVariant(customerPrefab, firstCustomer, byPrefab);
        foreach (var variant in additionalCustomers)
            RegisterVariant(variant.prefab, variant.placedCustomer, byPrefab);
        if (pools.Count == 0) yield break;
        for (int i = 0; i < pools.Count; i++)
        {
            int target = seats.Length / pools.Count + (i < seats.Length % pools.Count ? 1 : 0);
            pools[i].Preload(Mathf.Max(0, target - pools[i].StackCount), transform);
        }

        while (held) yield return null;
        while (true)
        {
            SpawnIfSeatAvailable();
            yield return new WaitForSeconds(Mathf.Max(0.1f, spawnInterval));
        }
    }

    private void RegisterVariant(CustomerController prefab, CustomerController placedCustomer,
        Dictionary<CustomerController, SimplePool2.Pool> byPrefab)
    {
        if (prefab == null) return;
        if (!byPrefab.TryGetValue(prefab, out var pool))
        {
            pool = new SimplePool2.Pool(prefab.gameObject, 1);
            byPrefab.Add(prefab, pool);
            pools.Add(pool);
        }
        if (placedCustomer == null) return;
        placedCustomer.transform.SetParent(transform, true);
        pool.AddExisting(placedCustomer.gameObject);
    }

    private CustomerController SpawnRandomCustomer(out SimplePool2.Pool pool)
    {
        pool = pools[Random.Range(0, pools.Count)];
        var guest = pool.Spawn<CustomerController>(transform.position, Quaternion.identity);
        if (guest.transform.parent != transform) guest.transform.SetParent(transform, true);
        return guest;
    }

    private void SpawnIfSeatAvailable()
    {
        var availableSeats = new List<CustomerSeat>(seats.Length);
        foreach (CustomerSeat seat in seats)
        {
            if (seat != null && seat.IsAvailable)
                availableSeats.Add(seat);
        }

        if (availableSeats.Count == 0) return;

        CustomerController guest = SpawnRandomCustomer(out var pool);
        while (availableSeats.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSeats.Count);
            CustomerSeat seat = availableSeats[randomIndex];
            availableSeats.RemoveAt(randomIndex);

            if (guest.BeginVisit(transform.position, seat, pool)) return;
        }
        pool.Despawn(guest.gameObject);
    }

    private void EndShift()
    {
        foreach (var guest in GetComponentsInChildren<CustomerController>())
            guest.TryReturnToPool();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (shiftClock != null) shiftClock.Finished -= EndShift;
    }
}
