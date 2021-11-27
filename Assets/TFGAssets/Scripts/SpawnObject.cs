using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    // List of objects
    public List<GameObject> prefabs;
    private float timer = 0.0f;
    private float waitTime = 5.0f;

    public void Spawn(int index)
    {
        Instantiate(prefabs[index], transform.position, transform.rotation);
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer > waitTime)
        {
            int nextSpawn = (int)(Random.Range(0, prefabs.Count));
            timer = 0.0f;
            Spawn(nextSpawn);
        }
    }

}