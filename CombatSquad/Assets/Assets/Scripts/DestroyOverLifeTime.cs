using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverLifeTime : MonoBehaviour
{
    public float LifeTime = 1.5f;
    void Start()
    {
        Destroy(gameObject, LifeTime);
    }

    void Update()
    {
        
    }
}
