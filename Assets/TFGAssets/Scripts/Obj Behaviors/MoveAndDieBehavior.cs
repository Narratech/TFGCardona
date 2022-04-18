using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAndDieBehavior : MonoBehaviour
{
    public float xSpeed = 0.0f;
    public float ySpeed = 0.0f;
    public float zSpeed = 0.0f;
    public float deathTime = 20f;
    public float timeAcu = 0f;

    // Update is called once per frame
    void Update()
    {
        timeAcu += Time.deltaTime;
        gameObject.transform.Translate(1 * Time.deltaTime * xSpeed, 1 * Time.deltaTime * ySpeed, 1 * Time.deltaTime * zSpeed);

        if (timeAcu > deathTime)
            Destroy(gameObject);
    }
}
