using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterLifeSpan : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Destroy after 2 seconds
        Destroy(gameObject, 2);
    }
}
