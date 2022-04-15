using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VR_Button_Template : MonoBehaviour
{
    // Posiciones mínimas y máximas del boton
    // Horizontal
    [SerializeField]
    private float MinLocalX = 0.105f;
    [SerializeField]
    private float TriggerPosX = 0.125f;
    [SerializeField]
    private float UntriggerPosX = 0.190f;
    [SerializeField]
    private float MaxLocalX = 0.2f;
    [SerializeField]
    private TextMeshPro debug;

    // Booleans
    private bool isClicked = false;

    // Color del boton
    public Material greenMat;
    public Material redMat;

    // Position
    Vector3 OriginalPosition;

    // Movimiento Recuperacion boton
    public float recoverySpeed = 0.01f;

    void Start()
    {
        // Set position to unpushed
        transform.localPosition = new Vector3(MaxLocalX, transform.localPosition.y, transform.localPosition.z);
        OriginalPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }

    /* TFG Cardona -> Need of vr physical buttons.
     * 
     * VR "Physical" BUTTON
     * ---------------------
     * 
     * Block Y & Z Movement on unity editor.
     * Block ALL rotation on unity editor.
     * 
     *         MaxPos_ _____________________________
     *   UnTriggerPos_|                             |
     *                |                             |
     *                |                             |
     *     TriggerPos_|                             |
     *                |                             |
     *         MinPos_|                             |
     *          _____________________________________________
     *                
     *   BUTTON POSITION
     *   If Pos > Max Pos -> Pos = MaxPos
     *   If Pos < Max Pos && Pos > MinPos -> Lerp to MaxPos
     *   If Pos < Min Pos -> Pos = MinPos
     *
     *   TRIGGER EFFECTS
     *   If Pos < TriggerPos -> isClicked = true.
     *   If Pos > Untrigger Pos -> isClicked = false.                
     */
    virtual public void Update()
    {
        string debugText = "";
        // Constrain movement to only allow movement in X axys in LOCAL space.
        // This is a fix to avoid using Rigidbody constraints that only work on Global Space.
        //Vector3 localVelocity = transform.InverseTransformDirection(gameObject.GetComponent<Rigidbody>().velocity);
        //localVelocity.y = 0;
        //localVelocity.z = 0;
        //gameObject.GetComponent<Rigidbody>().velocity = transform.TransformDirection(localVelocity);

        // ------- MOVEMENT CONTROL ------------
        // Getting it back into normal position (X)
        if (transform.localPosition.x < MaxLocalX)
        {
            debugText = "Recovering: X = ";
            // Be sure object speed is 0.
            gameObject.GetComponent<Rigidbody>().velocity.Set(0f, 0f, 0f);
            // Recover
            transform.Translate((float)recoverySpeed * Time.deltaTime, 0f, 0f);
        }
        else
        {
            debugText = "Other: X = ";
            //Debug.Log("Setting position to max");
            transform.localPosition.Set(MaxLocalX, transform.localPosition.y, transform.localPosition.z);
        }

        // Reset Z e Y
        transform.localPosition.Set(transform.localPosition.x, OriginalPosition.y, OriginalPosition.z);
        
        // Text
        debugText = debugText + transform.localPosition.x;
        debug.text = debugText;

        // ---------- CLICK LOGIC ---------------
        // If it's not clicked
        if (!isClicked)
        {
            // And it's position is below the trigger position.
            if (transform.localPosition.x < TriggerPosX)
            {
                isClicked = true;
                OnButtonDown();
            }
        }
        // If it is already clicked
        else
        {
            // Reset when reaching MaxLocalX.
            if (transform.localPosition.x > UntriggerPosX)// - 0.02f)
            {
                isClicked = false;
                OnButtonUp();
            }
        }
    }

    void OnButtonDown()
    {
        GetComponent<MeshRenderer>().material = greenMat;
        GetComponent<Collider>().isTrigger = true;

        // Call to the method that does something when the button is clicked
        OnClick();
    }

    void OnButtonUp()
    {
        GetComponent<MeshRenderer>().material = redMat;
        GetComponent<Collider>().isTrigger = false;
    }

    public virtual void OnClick()
    {
        //...
    }

    // Methods for collision reaction, not used here.
    private void OnTriggerEnter(Collider other)
    {
        // do something...
    }

    private void OnCollisionEnter(Collision collision)
    {
        // do something...
    }
    private void OnCollisionStay(Collision collision)
    {
        // do something...
    }

    private void OnCollisionExit(Collision collision)
    {
        // do something...
    }
}
