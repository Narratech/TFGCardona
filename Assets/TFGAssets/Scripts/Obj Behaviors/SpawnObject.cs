using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    // List of objects
    public List<GameObject> prefabs;
    private float timer = 0.0f;
    private float waitTime = 5.0f;
    public bool autoSpawn = false;

    public void Spawn(int index)
    {
        Instantiate(prefabs[index], transform.position, transform.rotation);
    }

    // Given a enumerate letter value, spawns one object showing that letter and its gesture.
    public void SpawnESLLetter(eESLalphabet letter)
    {
        Instantiate(prefabs[(int)letter], transform.position, transform.rotation);
    }

    public void Update()
    {
        // Spawner de una letra al azar en un lugar fijo.
        if (autoSpawn)
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

}