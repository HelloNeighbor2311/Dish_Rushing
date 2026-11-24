using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlSpawn : MonoBehaviour
{
    public List<BowlSO> bowlSOs;
    public List<Transform> spawnPositions; 
    public GameObject dishese;
    [SerializeField] private int spawnCount;
    private bool held;
    private bool started;

    private void Start()
    {
        if (!held) BeginSpawning();
    }

    /// <summary>Delays the timed spawning until Release. Call before Start (e.g. from another Awake).</summary>
    public void Hold() { held = true; }

    public void Release()
    {
        held = false;
        if (isActiveAndEnabled) BeginSpawning();
    }

    /// <summary>Spawns one stack right away. Returns null when no spawn position is left.</summary>
    public DishPickup SpawnNow()
    {
        return spawnPositions.Count > 0 && bowlSOs.Count > 0 ? SpawnBowlAtPos() : null;
    }

    private void BeginSpawning()
    {
        if (started) return;
        started = true;
        StartCoroutine(SpawnBowl());
    }
    private IEnumerator SpawnBowl()
    {
        while(spawnCount>0 && spawnPositions.Count>0){
        yield return new WaitForSeconds(2);
        if (spawnPositions.Count > 0) SpawnBowlAtPos();
        }
    }
    private DishPickup SpawnBowlAtPos()
    {
        
        BowlSO bowl = bowlSOs[Random.Range(0, bowlSOs.Count)];
        Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Count)];

        GameObject dishes = Instantiate(dishese, spawnPos);
        DishPickup pickup = dishes.GetComponent<DishPickup>();
        pickup.Setup(bowl);

        spawnPositions.Remove(spawnPos);
        spawnCount--;
        return pickup;
    }
}
