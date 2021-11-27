using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum handUsage
{
    NOHAND,
    LEFT_HAND_ONLY,
    RIGHT_HAND_ONLY,
    BOTH_HANDS
};

[System.Serializable]
public struct Gesture
{ 
    public string name;
    public List<Vector3> RHfingerData;
    public List<Vector3> LHfingerData;
    public UnityEvent onRecognized; //Callback
    public handUsage usedHand;
}

public struct LSEAlphabet
{ 

}

public class GestureRecognizer : MonoBehaviour
{
    public bool debugMode = true;
    public float threshold = 0.1f;
    public OVRSkeleton RHskeleton;
    public OVRSkeleton LHskeleton;
    public List<Gesture> gestures;
    private List<OVRBone> RHfingerBones;
    private List<OVRBone> LHfingerBones;
    private Gesture previousGesture;

    // Start is called before the first frame update
    void Start()
    {
        RHfingerBones = new List<OVRBone>(RHskeleton.Bones);
        LHfingerBones = new List<OVRBone>(LHskeleton.Bones);
        debugMode = true;
        threshold = 0.05f;   // TEST DIFFERENT THRESHOLD VALUES
        previousGesture = new Gesture();
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.Space)) {
            Save();
        }

        // Need to do a deltatime to reduce checks in time?
        Gesture currentGesture = Recognize();
        bool hasRecognized = !currentGesture.Equals(new Gesture());

        // Check if new gesture
        if (hasRecognized && !currentGesture.Equals(previousGesture))
        {
            // New Gesture
            Debug.Log("New Gesture Found: " + currentGesture.name);
            previousGesture = currentGesture;
            currentGesture.onRecognized.Invoke(); // Callback
        }
    }

    void Save()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> RHdata = new List<Vector3>();
        List<Vector3> LHdata = new List<Vector3>();

        // Obtain finger data for each hand
        foreach (var bone in RHfingerBones)
        {
            // TO DO: Multiple ways to compare a gesture
            //----------------------
            // local position
            // local rotation
            // flex / distance of fingers
            // others...

            // Position of finger relative to root
            RHdata.Add(RHskeleton.transform.InverseTransformPoint(bone.Transform.position));
        }
        foreach (var bone in LHfingerBones)
        {
            LHdata.Add(LHskeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        // Give data to the gesture structure
        g.RHfingerData = RHdata;
        g.LHfingerData = LHdata;

        // HAND USAGE
        // TO DO: Some way to check what hand have been used for the gesture.
        g.usedHand = handUsage.BOTH_HANDS;

        gestures.Add(g);
    }

    Gesture Recognize()
    {
        // Go through all fingers and compare with saved Gestures
        Gesture currentGesture = new Gesture(); //Includes both hands
        float RHcurrentMin = Mathf.Infinity;
        float LHcurrentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistanceRH = 0;
            float sumDistanceLH = 0;
            bool isDiscardedRH = false;
            bool isDiscardedLH = false;

            // RIGHT HAND
            if (gesture.usedHand == handUsage.BOTH_HANDS || gesture.usedHand == handUsage.RIGHT_HAND_ONLY)
            {
                for (int i = 0; i < RHfingerBones.Count; i++)
                {
                    Vector3 currentRHData = RHskeleton.transform.InverseTransformPoint(RHfingerBones[i].Transform.position);
                    float RHdistance = Vector3.Distance(currentRHData, gesture.RHfingerData[i]);
                    if (sumDistanceRH > threshold)
                    {
                        isDiscardedRH = true;
                        break;
                    }

                    sumDistanceRH += RHdistance;
                }
            }

            // LEFT HAND
            if (gesture.usedHand == handUsage.BOTH_HANDS || gesture.usedHand == handUsage.RIGHT_HAND_ONLY) 
            {
                for (int i = 0; i < LHfingerBones.Count; i++)
                {
                    Vector3 currentLHData = LHskeleton.transform.InverseTransformPoint(LHfingerBones[i].Transform.position);
                    float LHdistance = Vector3.Distance(currentLHData, gesture.LHfingerData[i]);
                    if (sumDistanceLH > threshold)
                    {
                        isDiscardedLH = true;
                        break;
                    }
                    sumDistanceLH += LHdistance;
                }
            }

            // GESTURE SELECTION
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // WARNING THIS MAY BE VERY WRONG
            // NEED TO SEPARATE GESTURES BY HAND??? YEAH PROBABLY
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (gesture.usedHand == handUsage.BOTH_HANDS)
            {
                if (!isDiscardedRH && !isDiscardedLH && sumDistanceRH < RHcurrentMin && sumDistanceLH < LHcurrentMin)
                {
                    RHcurrentMin = sumDistanceRH;
                    LHcurrentMin = sumDistanceLH;
                    currentGesture = gesture;
                }
            }
            else if (gesture.usedHand == handUsage.RIGHT_HAND_ONLY)
            {
                if (!isDiscardedRH && sumDistanceRH < RHcurrentMin)
                {
                    RHcurrentMin = sumDistanceRH;
                    currentGesture = gesture;
                }
            }
            else if (gesture.usedHand == handUsage.LEFT_HAND_ONLY)
            {
                if (!isDiscardedLH && sumDistanceLH < LHcurrentMin)
                {
                    LHcurrentMin = sumDistanceLH;
                    currentGesture = gesture;
                }
            }
            else 
            { 
            }
        }
        return currentGesture;
    }
}
