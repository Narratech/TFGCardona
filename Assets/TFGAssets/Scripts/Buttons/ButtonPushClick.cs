using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPushClick : MonoBehaviour
{
    public GameObject mirror;
    public float MinLocalY = 0.38f;
    public float MaxLocalY = 0.55f;

    public bool isBeingTouched = false;
    public bool isClicked = false;

    public Material greenMat;
    public Material redMat;

    private bool mirrorVisible = true;
    public float smooth = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 buttonDownPosition = new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z);
        Vector3 buttonUpPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);

        // Getting it back into normal position
        if (!isBeingTouched && (transform.localPosition.y > MaxLocalY || transform.localPosition.y < MaxLocalY))
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUpPosition, Time.deltaTime * smooth);
        }

        if (!isClicked)
        {
            if (transform.localPosition.y < MinLocalY)
            {
                isClicked = true;
                transform.localPosition = buttonDownPosition;
                OnButtonDown();
            }
        }
        else
        {
            if (transform.localPosition.y > MaxLocalY-0.01f)
            {
                isClicked = false;
                transform.localPosition = buttonUpPosition;
                OnButtonUp();
            }
        }
    }

    void OnButtonDown()
    {
        GetComponent<MeshRenderer>().material = greenMat;
        GetComponent<Collider>().isTrigger = true;

        mirrorVisible = !mirrorVisible;

        // Disable mirror.
        mirror.SetActive(mirrorVisible);
    }

    void OnButtonUp()
    {
        GetComponent<MeshRenderer>().material = redMat;
        GetComponent<Collider>().isTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isClicked)
        {
            // do something...
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.tag != "BackButton")
        {
            isBeingTouched = true;
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.tag != "BackButton")
        {
            isBeingTouched = false;
        }
    }
}
