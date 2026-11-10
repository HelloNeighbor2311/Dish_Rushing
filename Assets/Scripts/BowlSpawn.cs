using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlSpawn : MonoBehaviour
{
    public List<BowlSO> bowlSOs;
    public List<Transform> spawnPositions; 
    public GameObject dishese;
    [SerializeField] private int spawnCount;


    private void Start()
    {
        StartCoroutine(SpawnBowl());
    }
    private IEnumerator SpawnBowl()
    {
        while(spawnCount>0){
        yield return new WaitForSeconds(2);
        SpawnBowlAtPos();
        }
    }
    private void SpawnBowlAtPos()
    {
        
        BowlSO bowl = bowlSOs[Random.Range(0, bowlSOs.Count)];
        Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Count)];

        GameObject dishes = Instantiate(dishese, spawnPos);
        dishes.GetComponent<SpriteRenderer>().sprite = bowl.sprite;

        spawnPositions.Remove(spawnPos);
        spawnCount--;    
    }
}
