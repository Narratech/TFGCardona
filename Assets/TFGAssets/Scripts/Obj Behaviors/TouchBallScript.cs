using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum finger
{
    WRISTROOT,
    FOREARMSTUB,
    THUMB_Dystal,
    INDEX_Dystal,
    MIDDLE_Dystal,
    RING_Dystal,
    PINKY_Dystal,
    THUMB_Tip,
    INDEX_Tip,
    MIDDLE_Tip,
    RING_Tip,
    PINKY_Tip,
    THUMB_Proximal,
    INDEX_Proximal,
    MIDDLE_Proximal,
    RING_Proximal,
    PINKY_Proximal,
    INDEX_Intermediate,
    MIDDLE_Intermediate,
    RING_Intermediate,
    PINKY_Intermediate,
    THUMB_Metacarpal,
    PINKY_Metacarpal
}

/**
 * BONE INDEX Inside OVRSkeleton
 * 
 * Invalid          = -1
 * Hand_Start       = 0
 * Hand_WristRoot   = Hand_Start + 0 // root frame of the hand, where the wrist is located
 * Hand_ForearmStub = Hand_Start + 1 // frame for user's forearm
 * 
 * Hand_Thumb0      = Hand_Start + 2 // thumb trapezium bone
 * Hand_Thumb1      = Hand_Start + 3 // thumb metacarpal bone
 * Hand_Thumb2      = Hand_Start + 4 // thumb proximal phalange bone
 * Hand_Thumb3      = Hand_Start + 5 // thumb distal phalange bone
 * 
 * Hand_Index1      = Hand_Start + 6 // index proximal phalange bone
 * Hand_Index2      = Hand_Start + 7 // index intermediate phalange bone
 * Hand_Index3      = Hand_Start + 8 // index distal phalange bone
 * 
 * Hand_Middle1     = Hand_Start + 9 // middle proximal phalange bone
 * Hand_Middle2     = Hand_Start + 10 // middle intermediate phalange bone
 * Hand_Middle3     = Hand_Start + 11 // middle distal phalange bone
 * 
 * Hand_Ring1       = Hand_Start + 12 // ring proximal phalange bone
 * Hand_Ring2       = Hand_Start + 13 // ring intermediate phalange bone
 * Hand_Ring3       = Hand_Start + 14 // ring distal phalange bone
 * 
 * Hand_Pinky0      = Hand_Start + 15 // pinky metacarpal bone
 * Hand_Pinky1      = Hand_Start + 16 // pinky proximal phalange bone
 * Hand_Pinky2      = Hand_Start + 17 // pinky intermediate phalange bone
 * Hand_Pinky3      = Hand_Start + 18 // pinky distal phalange bone
 * 
 * Hand_MaxSkinnable= Hand_Start + 19
 * // Bone tips are position only. They are not used for skinning but are useful for hit-testing.
 * // NOTE: Hand_ThumbTip == Hand_MaxSkinnable since the extended tips need to be contiguous
 * Hand_ThumbTip    = Hand_Start + Hand_MaxSkinnable + 0 // tip of the thumb (19)
 * Hand_IndexTip    = Hand_Start + Hand_MaxSkinnable + 1 // tip of the index finger (20)
 * Hand_MiddleTip   = Hand_Start + Hand_MaxSkinnable + 2 // tip of the middle finger (21)
 * Hand_RingTip     = Hand_Start + Hand_MaxSkinnable + 3 // tip of the ring finger (22)
 * Hand_PinkyTip    = Hand_Start + Hand_MaxSkinnable + 4 // tip of the pinky (23)
 * Hand_End         = Hand_Start + Hand_MaxSkinnable + 5
 * Max              = Hand_End + 0 
 */


// BE SURE TO SET THE LAYER TO A NEW ONE THAT DO NOT INTERACT WITH OTHER LAYERS OTHER THAT ITSELF.
public class TouchBallScript : MonoBehaviour
{
    [SerializeField]
    private OVRSkeleton handSkeleton;
    [SerializeField]
    private OVRHand hand;

    [SerializeField]
    public finger chosenFinger;

    // Position of the selected finger
    private Vector3 fingerPos;

    // Start is called before the first frame update
    void Start()
    {
        fingerPos = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // NO REALIZAR RECONOCIMIENTO SI NO SE ESTA TRAQUEANDO ALGUNA DE LAS MANOS O NO HAY SUFICIENTE CONFIANZA.
        if (!handSkeleton.IsDataHighConfidence || !hand.IsTracked)
        {
            return;
        }

        // While in Unity Editor.
        if (handSkeleton.Bones.Count == 0) return;

        switch (chosenFinger) 
        {
            case finger.WRISTROOT:
                fingerPos = handSkeleton.Bones[0].Transform.position;
                break;
            case finger.FOREARMSTUB:
                fingerPos = handSkeleton.Bones[1].Transform.position;
                break;
            // TIPS
            case finger.THUMB_Tip:
                fingerPos = handSkeleton.Bones[19].Transform.position;
                break;
            case finger.INDEX_Tip:
                fingerPos = handSkeleton.Bones[20].Transform.position;
                break;
            case finger.MIDDLE_Tip:
                fingerPos = handSkeleton.Bones[21].Transform.position;
                break;
            case finger.RING_Tip:
                fingerPos = handSkeleton.Bones[22].Transform.position;
                break;
            case finger.PINKY_Tip:
                fingerPos = handSkeleton.Bones[23].Transform.position;
                break;
            // DISTAL
            case finger.THUMB_Dystal:
                fingerPos = handSkeleton.Bones[5].Transform.position;
                break;
            case finger.INDEX_Dystal:
                fingerPos = handSkeleton.Bones[8].Transform.position;
                break;
            case finger.MIDDLE_Dystal:
                fingerPos = handSkeleton.Bones[11].Transform.position;
                break;
            case finger.RING_Dystal:
                fingerPos = handSkeleton.Bones[14].Transform.position;
                break;
            case finger.PINKY_Dystal:
                fingerPos = handSkeleton.Bones[18].Transform.position;
                break;
            // INTERMEDIATES
            case finger.INDEX_Intermediate:
                fingerPos = handSkeleton.Bones[7].Transform.position;
                break;
            case finger.MIDDLE_Intermediate:
                fingerPos = handSkeleton.Bones[10].Transform.position;
                break;
            case finger.RING_Intermediate:
                fingerPos = handSkeleton.Bones[13].Transform.position;
                break;
            case finger.PINKY_Intermediate:
                fingerPos = handSkeleton.Bones[17].Transform.position;
                break;
            // PROXIMALS
            case finger.THUMB_Proximal:
                fingerPos = handSkeleton.Bones[4].Transform.position;
                break;
            case finger.INDEX_Proximal:
                fingerPos = handSkeleton.Bones[6].Transform.position;
                break;
            case finger.MIDDLE_Proximal:
                fingerPos = handSkeleton.Bones[9].Transform.position;
                break;
            case finger.RING_Proximal:
                fingerPos = handSkeleton.Bones[12].Transform.position;
                break;
            case finger.PINKY_Proximal:
                fingerPos = handSkeleton.Bones[16].Transform.position;
                break;
            // METACARPALS
            case finger.THUMB_Metacarpal:
                fingerPos = handSkeleton.Bones[3].Transform.position;
                break;
            case finger.PINKY_Metacarpal:
                fingerPos = handSkeleton.Bones[15].Transform.position;
                break;
        }

        // Move sphere to finger position
        gameObject.transform.position = fingerPos;
    }
}
