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

/// <summary>
/// Gesture es una estructura de datos que almacena la información necesaria de un gesto concreto.
/// Tendrá un nombre, las posiciones de las manos almacenadas para el gesto.
/// Contendrá un evento de callback si es reconocido
/// </summary>
[System.Serializable]
public struct Gesture
{ 
    public string name;
    public List<Vector3> RHfingerData;
    public List<Vector3> LHfingerData;
    public UnityEvent onRecognized; //Callback
    public handUsage usedHand;
}

public class GestureRecognizer : MonoBehaviour
{
    public float threshold = 0.1f; // Rango de error de reconocimiento
    public OVRSkeleton RHskeleton; // Esqueleto de la mano OVRRightHand
    public OVRSkeleton LHskeleton; // Esqueleto de la mano OVRLeftHand
    public List<Gesture> gestures; // Lista de 
    private List<OVRBone> RHfingerBones;
    private List<OVRBone> LHfingerBones;
    private Gesture previousGesture;

    // Gestores
    public Persistence _persistence;

    // Bools
    public bool debugMode = true;
    public bool gestureCaptured = false;

    // Timers
    private float timeAcu = 0.0f;
    private float timeBetweenRecognition = 1.0f; // 1 second

    // Start is called before the first frame update
    void Start()
    {
        RHfingerBones = new List<OVRBone>(RHskeleton.Bones);
        LHfingerBones = new List<OVRBone>(LHskeleton.Bones);
        debugMode = true;
        threshold = 0.05f;   // TEST DIFFERENT THRESHOLD VALUES
        previousGesture = new Gesture();
        _persistence = new Persistence();
        _persistence.Init(this, Application.persistentDataPath + "/datos.xml");
    }

    public List<Gesture> getGestureList()
    {
        return gestures;
    }

    public void setGestureList(List<Gesture> newList)
    {
        gestures.Clear();
        gestures.AddRange(newList);
    }

    // Update is called once per frame
    void Update()
    {
        timeAcu += Time.deltaTime;

        // We allow a gesture to be captured every "timeBetweenRecognition" seconds (1 sec now)
        if (timeAcu > timeBetweenRecognition)
        {
            if (gestureCaptured) gestureCaptured = false;
            timeAcu = 0.0f;
        }

        // Keyboard capture
        //if (debugMode && Input.GetKeyDown(KeyCode.Space)) {
        //    SaveFullGesture();
        //}

        // GESTURE RECOGNITION
        if (!gestureCaptured)
        { 
            // Need to do a deltatime to reduce checks in time?
            Gesture currentGesture = Recognize();
            bool hasRecognized = !currentGesture.Equals(new Gesture());

            // Check if new gesture
            if (hasRecognized && !currentGesture.Equals(previousGesture))
            {
                // New Gesture
                Debug.Log("New Gesture Found: " + currentGesture.name);
                previousGesture = currentGesture;
                currentGesture.onRecognized.Invoke(); // Callback of that gesture
            }
        }
    }

    /// <summary>
    /// Permite almacenar un nuevo gesto dentro del sistema de Gestos.
    /// </summary>
    public void SaveFullGesture()
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

        // Le damos nombre
        g.name = "FullGesture-" + Time.time;

        // Give data to the gesture structure
        g.RHfingerData = RHdata;
        g.LHfingerData = LHdata;

        // HAND USAGE
        // TO DO: Some way to check what hand have been used for the gesture.
        g.usedHand = handUsage.BOTH_HANDS;

        gestures.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        _persistence.SaveGestureList(gestures);
    }

    /// <summary>
    /// Permite almacenar un nuevo gesto de la mano derecha en el sistema de gestos.
    /// </summary>
    public void SaveRightHandGesture()
    {
        // New gesture instantiation
        Gesture g = new Gesture();
        g.name = "New RH Gesture";
        List<Vector3> RHdata = new List<Vector3>();

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

        // Le damos nombre
        g.name = "RightGesture-" + Time.time;

        // Give data to the gesture structure
        g.RHfingerData = RHdata;

        // HAND USAGE
        g.usedHand = handUsage.RIGHT_HAND_ONLY;

        gestures.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        _persistence.SaveGestureList(gestures);
    }

    /// <summary>
    /// Permite almacenar un nuevo gesto de la mano derecha en el sistema de gestos.
    /// </summary>
    public void SaveLeftHandGesture()
    {
        // New gesture instantiation
        Gesture g = new Gesture();
        g.name = "New LH Gesture";
        List<Vector3> LHdata = new List<Vector3>();

        // Obtain finger data for each hand
        foreach (var bone in LHfingerBones)
        {
            // TO DO: Multiple ways to compare a gesture
            //----------------------
            // local position
            // local rotation
            // flex / distance of fingers
            // others...

            // Position of finger relative to root
            LHdata.Add(LHskeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        // Le damos nombre
        g.name = "LeftGesture-" + Time.time;

        // Give data to the gesture structure
        g.LHfingerData = LHdata;

        // HAND USAGE
        g.usedHand = handUsage.LEFT_HAND_ONLY;

        gestures.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        _persistence.SaveGestureList(gestures);
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
