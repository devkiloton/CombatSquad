using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public Transform[] SpawnPoints;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        foreach(Transform spawn in SpawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
    }
    
    public Transform SpawnPosition()
    {
        return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
    }
}
