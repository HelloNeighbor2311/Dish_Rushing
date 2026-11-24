using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private CustomerController customerPrefab;
    [Tooltip("Optional customer already placed in the scene, used for the first visit.")]
    [SerializeField] private CustomerController firstCustomer;
    [SerializeField] private CustomerSeat[] seats;
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;
    [SerializeField] private CountDownTime shiftClock;
    private SimplePool2.Pool pool;

    private IEnumerator Start()
    {
        if (shiftClock != null) shiftClock.Finished += EndShift;
        if (customerPrefab == null || seats == null || seats.Length == 0) yield break;

        pool = new SimplePool2.Pool(customerPrefab.gameObject, seats.Length);
        if (firstCustomer != null)
        {
            firstCustomer.transform.SetParent(transform, true);
            pool.AddExisting(firstCustomer.gameObject);
        }
        pool.Preload(seats.Length - pool.StackCount, transform);

        while (true)
        {
            SpawnIfSeatAvailable();
            yield return new WaitForSeconds(Mathf.Max(0.1f, spawnInterval));
        }
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

        CustomerController guest = null;
        while (availableSeats.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSeats.Count);
            CustomerSeat seat = availableSeats[randomIndex];
            availableSeats.RemoveAt(randomIndex);

            if (guest == null)
            {
                guest = pool.Spawn<CustomerController>(transform.position, Quaternion.identity);
                if (guest.transform.parent != transform) guest.transform.SetParent(transform, true);
            }
            if (guest.BeginVisit(transform.position, seat, pool)) return;
        }
        if (guest != null) pool.Despawn(guest.gameObject);
    }

    private void EndShift()
    {
        if (pool != null)
        {
            foreach (var guest in GetComponentsInChildren<CustomerController>())
                pool.Despawn(guest.gameObject);
        }
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (shiftClock != null) shiftClock.Finished -= EndShift;
    }
}
