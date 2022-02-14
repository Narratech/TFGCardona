using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;

// ENUMS
public enum handUsage
{
    NOHAND,
    LEFT_HAND_ONLY,
    RIGHT_HAND_ONLY,
    BOTH_HANDS
};

public enum ESLalphabet
{
    A,
    B,
    C,
    CH,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    LL,
    M,
    N,
    Ñ,
    O,
    P,
    Q,
    R,
    RR,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z
};

// STRUCTS
public struct BoneData
{
    public OVRSkeleton.BoneId id;
    public Vector3 position;
    public Quaternion rotation;
}

/// <summary>
/// Gesture es una estructura de datos que almacena la información necesaria de un gesto concreto.
/// Tendrá un nombre, las posiciones de las manos almacenadas para el gesto.
/// Contendrá un evento de callback si es reconocido
/// </summary>
[System.Serializable]
public struct Gesture
{ 
    public string gestureName;
    public List<BoneData> RHBoneInfo;
    public List<BoneData> LHBoneInfo;
    public UnityEvent onRecognized; //Callback
    public handUsage usedHand;
};

public class GestureRecognizer : MonoBehaviour
{
    // Referencias Externas
    public DebugManager debugManager;
    public OVRSkeleton RHskeleton; // Esqueleto de la mano OVRRightHand
    public OVRSkeleton LHskeleton; // Esqueleto de la mano OVRLeftHand
    public List<Gesture> gestures; // Lista de 
    private Gesture previousGesture;
    public GameObject CuboReconocimiento;
    public Material colorRojo;
    public Material colorAmarillo;
    public Material colorVerde;

    // Gestores
    public Persistence _persistence;

    // Bools
    public bool debugMode = true;
    public bool gestureCaptured = false;
    public bool useDummyCapture = false;
    public bool isRecognizing = false;

    // Timers
    [SerializeField]
    private TextMeshPro textoTimer;
    private float timeAcu = 0.0f;
    private float timeBetweenRecognition = 5.0f; // 5 seconds

    // Reconocimiento de mano
    private float threshold = 0.1f; // Umbral de reconocimiento
    private float minDistFound = 0.0f;
    private string minNameFound = "";

    // Start is called before the first frame update
    void Start()
    {
        debugMode = true;

        // Debug Manager
        if (debugManager == null) debugManager = new DebugManager();
        debugManager.Init();
        
        // Persistencia
        if (_persistence == null) _persistence = new Persistence();
        _persistence.Init(this, Application.persistentDataPath + "/gestos.xml");
        
        previousGesture = new Gesture();
    }


    public List<Gesture> getGestureList()
    {
        return gestures;
    }

    public void setGestureList(List<Gesture> newList)
    {
        gestures.Clear();
        gestures.AddRange(newList);

        // Debug
        bool debugLoadedBones = true;
        if (debugLoadedBones)
        {
            Debug.Log("GestureRecognizer::setGestureList() Gestos en Manager: " + gestures.Count);
            foreach (Gesture gesto in gestures)
            {
                string datosGesto = "Gesto: " + gesto.gestureName + "\n" + "Mano usada: " + gesto.usedHand + "\n" + "Huesos: {\n";
                string datosHueso = "";
                if (gesto.RHBoneInfo != null)
                {
                    foreach (BoneData huesoiz in gesto.RHBoneInfo)
                    {
                        datosHueso = datosHueso + "    ID: " + huesoiz.id + "\n";
                        datosHueso = datosHueso + "    Pos: " + huesoiz.position + "\n";
                        datosHueso = datosHueso + "    Rot: " + huesoiz.rotation + "\n";
                        datosGesto = datosGesto + datosHueso;
                    }
                    datosGesto = datosGesto + "}";
                }
                else
                {
                    Debug.Log("Gesto no contiene datos de huesos de la mano izquierda");
                }
                Debug.Log(datosGesto);
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    ///////////////////////  LOGICA INTERNA ////////////////////////////
    ////////////////////////////////////////////////////////////////////

    // Update is called once per frame
    void Update()
    {
        timeAcu += Time.deltaTime;
        float nextIn = 6.0f - timeAcu;
        if (!isRecognizing) textoTimer.text = "Siguiente intento en: " + (int)nextIn + " seg.";
        if (timeAcu > 3.5f) debugManager.setRecogText("");

        // GESTURE RECOGNITION
        if (timeBetweenRecognition < timeAcu) //!gestureCaptured
        {
            isRecognizing = true;

            // Textos debug
            Debug.Log("Update() - Intentando reconocer gesto.");
            debugManager.enqueueDebugText("-------------------------------------------");
            debugManager.enqueueDebugText("Update() Intentando reconocer gesto actual.");
            debugManager.updateBonePanels();
            
            // Obtenemos el gesto Actual.
            Gesture currentGesture = Recognize();
            
            // Debug
            Debug.Log("Resultado de reconocimiento: " + currentGesture.gestureName);
            debugManager.enqueueDebugText("Update() Resultado: " + currentGesture.gestureName);
            
            // Vemos si se ha encontrado alguno
            bool hasRecognized = currentGesture.gestureName != "Unknown";
            if (currentGesture.gestureName == "Unknown")
                debugManager.setRecogText(currentGesture.gestureName + "\nMinFound: " + minDistFound + "\nGesture: " + minNameFound);
            else
                debugManager.setRecogText(currentGesture.gestureName);

            // Check if new gesture
            if (hasRecognized && !currentGesture.Equals(previousGesture))
            {
                // New Gesture
                Debug.Log("New Gesture Found: " + currentGesture.gestureName);
                debugManager.enqueueDebugText("Update() New Gesture Found: " + currentGesture.gestureName);
                CuboReconocimiento.GetComponent<Renderer>().material = colorVerde;
                //debugManager.setCubeText("GESTO RECONOCIDO! (" + currentGesture.gestureName + ")");
                previousGesture = currentGesture;
                //currentGesture.onRecognized.Invoke(); // Callback of that gesture
                
                isRecognizing = false;
            }
            else
            {
                if (!hasRecognized)
                { 
                    Debug.Log("Gesto no reconocido.");
                    debugManager.enqueueDebugText("Update() Gesto no reconocido.");
                    //debugManager.setCubeText("GESTO NO RECONOCIDO.");
                    //CuboReconocimiento.GetComponent<Renderer>().material = colorRojo;
                }
                if (currentGesture.Equals(previousGesture))
                { 
                    Debug.Log("Mismo gesto que el anterior reconocido.");
                    debugManager.enqueueDebugText("Update() Mismo gesto que el anterior.");
                    //debugManager.setCubeText("MISMO GESTO QUE EL ANTERIOR RECONOCIDO.");
                    //CuboReconocimiento.GetComponent<Renderer>().material = colorAmarillo;
                }
                isRecognizing = false;
            }
            timeAcu = 0.0f;
        }
    }

    ////////////////////////////////////////////////////////////////////
    ///////////////  METODOS DE GESTION DE GESTOS //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Permite almacenar un nuevo gesto dentro del sistema de Gestos.
    /// </summary>
    public void SaveFullGesture()
    {
        // New gesture instantiation
        Gesture g = new Gesture();
        g.RHBoneInfo = new List<BoneData>();
        g.LHBoneInfo = new List<BoneData>();
        BoneData rhBoneData = new BoneData();
        BoneData lhBoneData = new BoneData();

        // Obtain finger data for each hand
        foreach (OVRBone bone in RHskeleton.Bones)
        {
            // Bone information
            rhBoneData.id = bone.Id;
            rhBoneData.position = RHskeleton.transform.InverseTransformPoint(bone.Transform.position);
            rhBoneData.rotation = bone.Transform.rotation;

            // Add to gesture bone list.
            g.RHBoneInfo.Add(rhBoneData);
        }

        // Obtain finger data for left hand
        foreach (OVRBone bone in LHskeleton.Bones)
        {
            // Bone information
            lhBoneData.id = bone.Id;
            lhBoneData.position = LHskeleton.transform.InverseTransformPoint(bone.Transform.position);
            lhBoneData.rotation = bone.Transform.rotation;

            // Add to gesture bone list.
            g.LHBoneInfo.Add(lhBoneData);
        }

        // Le damos nombre
        g.gestureName = "FullGesture-" + Time.time;

        // HAND USAGE
        g.usedHand = handUsage.BOTH_HANDS;

        gestures.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        _persistence.saveGesture(g);
    }


    // TO DO: Multiple ways to compare a gesture
    //----------------------
    // local position
    // local rotation
    // flex / distance of fingers
    // others...

    /// <summary>
    /// Permite almacenar un nuevo gesto de la mano derecha en el sistema de gestos.
    /// </summary>
    public void SaveRightHandGesture()
    {
        // New gesture instantiation
        Gesture g = new Gesture();
        g.RHBoneInfo = new List<BoneData>();
        BoneData rhBoneData = new BoneData();

        // Obtain finger data for each hand
        foreach (OVRBone bone in RHskeleton.Bones)
        {
            // Bone information
            rhBoneData.id = bone.Id;
            rhBoneData.position = RHskeleton.transform.InverseTransformPoint(bone.Transform.position);
            rhBoneData.rotation = bone.Transform.rotation;

            // Add to gesture bone list.
            g.RHBoneInfo.Add(rhBoneData);
        }

        // Le damos nombre
        g.gestureName = "RightGesture-" + Time.time;

        // HAND USAGE
        g.usedHand = handUsage.RIGHT_HAND_ONLY;

        gestures.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        _persistence.saveGesture(g);
    }
  
    /// <summary>
    /// Permite almacenar un nuevo gesto de la mano derecha en el sistema de gestos.
    /// </summary>
    public void SaveLeftHandGesture()
    {
        // New gesture instantiation
        Gesture g = new Gesture();
        g.LHBoneInfo = new List<BoneData>();
        BoneData lhBoneData = new BoneData();

        // Obtain finger data for left hand
        foreach (OVRBone bone in LHskeleton.Bones)
        {
            // Bone information
            lhBoneData.id = bone.Id;
            lhBoneData.position = LHskeleton.transform.InverseTransformPoint(bone.Transform.position);
            lhBoneData.rotation = bone.Transform.rotation;

            // Add to gesture bone list.
            g.LHBoneInfo.Add(lhBoneData);
        }

        // Le damos nombre
        g.gestureName = "LeftGesture-" + Time.time;

        // HAND USAGE
        g.usedHand = handUsage.LEFT_HAND_ONLY;

        gestures.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        //_persistence.SaveGestureList(gestures);
        _persistence.saveGesture(g);
    }

    /// <summary>
    /// Calculates the absolute distance between all quaternion components
    /// of two given quaternions
    /// </summary>
    /// <param name="captured"></param>
    /// <param name="stored"></param>
    /// <returns></returns>
    public float quaternionDistance(Quaternion captured, Quaternion stored)
    {
        float quatDist = 0.0f;

        float wCompo = Mathf.Abs(captured.w - stored.w);
        float xCompo = Mathf.Abs(captured.x - stored.x);
        float yCompo = Mathf.Abs(captured.y - stored.y);
        float zCompo = Mathf.Abs(captured.z - stored.z);

        // Sum of quaternion distances
        quatDist = wCompo + xCompo + yCompo + zCompo;
        //enqueueDebugText("GestureRecog::quaternionDistance() dist: " + quatDist);

        return quatDist;
    }

    /// <summary>
    /// Por cada gesto almacenado en la lista de Gestos, lo compara contra la posición actual de las manos
    /// en la escena.
    /// Calcula las distancias de la posición del hueso respecto a la muñeca.
    /// AUN NO CONSIDERA LAS ROTACIONES.
    /// </summary>
    /// <returns></returns>
    Gesture Recognize()
    {
        // Inicializamos el gesto a devolver
        Gesture currentGesture = new Gesture();
        currentGesture.gestureName = "Unknown";
        currentGesture.LHBoneInfo = new List<BoneData>();
        currentGesture.RHBoneInfo = new List<BoneData>();

        // Variables de calculo de distancias generales
        // Inicializadas al infinito.
        float RHcurrentMin = Mathf.Infinity;
        float LHcurrentMin = Mathf.Infinity;

        // Debug for out of threshold and not found.
        minDistFound = Mathf.Infinity;
        minNameFound = "";

        // Debug del metodo
        bool showDebug = false;

        // Por cada gesto en la lista de gestos
        foreach (var gesture in gestures)
        {
            debugManager.enqueueDebugText("Recognize() Comparando con Gesto: " + gesture.gestureName);
            Debug.Log("Recognize() Comparando con Gesto: " + gesture.gestureName);

            // Variables de suma de distancias y bools de descarte si la distancia supera el umbral máximo de reconocimiento.
            // POSICION
            float sumDistanceRH = 0.0f;
            float sumDistanceLH = 0.0f;
            bool isDiscardedRH = false;
            bool isDiscardedLH = false;

            // ROTACION
            float sumRotDistRH = 0.0f;
            float sumRotDistLH = 0.0f;

            // -----------CALCULO DE DISTANCIAS----------
            // RIGHT HAND
            // Almacenamos suma distancias de la mano derecha
            if ((gesture.usedHand == handUsage.RIGHT_HAND_ONLY || gesture.usedHand == handUsage.BOTH_HANDS) && RHskeleton.Bones.Count > 0)
            {
                if (showDebug) Debug.Log("Comparando RH. RHskeleton.Bones.Count = " + RHskeleton.Bones.Count);

                for (int i = 0; i < RHskeleton.Bones.Count; i++)
                {
                    // POSICION
                    Vector3 currentRHData = RHskeleton.transform.InverseTransformPoint(RHskeleton.Bones[i].Transform.position);
                    float RHPosDistance = Vector3.Distance(currentRHData, gesture.RHBoneInfo[i].position);

                    if (showDebug)
                    {
                        debugManager.enqueueDebugText("Distancia Pos hueso RH " + gesture.RHBoneInfo[i].id + " : " + RHPosDistance);
                        Debug.Log("Captured RH Pos: " + currentRHData);
                        Debug.Log("Stored RH Pos: " + gesture.LHBoneInfo[i].position);
                    }

                    // ROTACION
                    Quaternion currentRHRotData = RHskeleton.Bones[i].Transform.rotation;
                    float RHRotDistance = quaternionDistance(currentRHRotData, gesture.RHBoneInfo[i].rotation);

                    if (showDebug)
                    {
                        debugManager.enqueueDebugText("Distancia Rot hueso RH " + gesture.RHBoneInfo[i].id + " : " + RHRotDistance);
                        Debug.Log("Captured RH Rot: " + currentRHRotData);
                        Debug.Log("Stored RH Rot: " + gesture.LHBoneInfo[i].rotation);
                    }

                    // Si la suma de la distancia supera el umbral máximo de reconocimiento, descartamos la mano.
                    if (sumDistanceRH > threshold)
                    {
                        isDiscardedRH = true;

                        if (sumDistanceRH < minDistFound)
                        {
                            minDistFound = sumDistanceRH;
                            minNameFound = gesture.gestureName;
                        }

                        Debug.Log("Distance too great, discarding hand.");
                        debugManager.enqueueDebugText("Distancia demasiado grande (" + sumDistanceRH + "/" + threshold + "), descartando mano.");

                        break;
                    }

                    // Añadimos la distancia de este hueso a la suma total
                    sumDistanceRH += RHPosDistance; //+RHRotDistance);
                }
            }
            else
            {
                bool handNotUsed = !(gesture.usedHand == handUsage.RIGHT_HAND_ONLY || gesture.usedHand == handUsage.BOTH_HANDS);
                if (RHskeleton.Bones.Count == 0)
                {
                    Debug.Log("No Skeleton Found.");
                    debugManager.enqueueDebugText("Esqueleto de RH no encontrado. RH Descartada");
                }

                if (handNotUsed)
                { 
                    Debug.Log("hand not used in this gesture.");
                    debugManager.enqueueDebugText("Mano RH no usada en este gesto. RH Descartada");
                }

                Debug.Log("RIGHT Hand Discarded.");
                isDiscardedRH = true;
            }

            // LEFT HAND
            // Almacenamos suma distancias de la mano izquierda
            if ((gesture.usedHand == handUsage.LEFT_HAND_ONLY || gesture.usedHand == handUsage.BOTH_HANDS) && LHskeleton.Bones.Count > 0)
            {
                if (showDebug) Debug.Log("Comparando LH. LHfingerBones.Count = " + LHskeleton.Bones.Count);
                for (int i = 0; i < LHskeleton.Bones.Count; i++)
                {
                    // POSICION
                    Vector3 currentLHData = LHskeleton.transform.InverseTransformPoint(LHskeleton.Bones[i].Transform.position);
                    float LHPosDistance = Vector3.Distance(currentLHData, gesture.LHBoneInfo[i].position);

                    if (showDebug)
                    {
                        debugManager.enqueueDebugText("Distancia Pos hueso LH " + gesture.LHBoneInfo[i].id + " : " + LHPosDistance);
                        Debug.Log("Captured LH Pos: " + currentLHData);
                        Debug.Log("Stored LH Pos: " + gesture.LHBoneInfo[i].position);
                    }

                    // ROTACION
                    Quaternion currentLHRotData = LHskeleton.Bones[i].Transform.rotation;
                    float LHRotDistance = quaternionDistance(currentLHRotData, gesture.LHBoneInfo[i].rotation);

                    if (showDebug)
                    {
                        debugManager.enqueueDebugText("Distancia Rot hueso LH " + gesture.LHBoneInfo[i].id + " : " + LHRotDistance);
                        Debug.Log("Captured LH Rot: " + currentLHRotData);
                        Debug.Log("Stored LH Rot: " + gesture.LHBoneInfo[i].rotation);
                    }

                    // Si la suma de la distancia supera el umbral máximo de reconocimiento, descartamos la mano.
                    if (sumDistanceLH > threshold)
                    {
                        isDiscardedLH = true;
                        break;
                    }
                    sumDistanceLH += LHPosDistance; // +LHRotDistance);
                }
            }
            else
            {
                bool handNotUsed = !(gesture.usedHand == handUsage.LEFT_HAND_ONLY || gesture.usedHand == handUsage.BOTH_HANDS);
                if (LHskeleton.Bones.Count == 0)
                    Debug.Log("No Skeleton Found.");
                if (handNotUsed)
                    Debug.Log("hand not used in this gesture.");
                Debug.Log("LEFT Hand Discarded.");
                isDiscardedLH = true;
            }


            //----------------PROCESADO DE DISTANCIAS-------------------
            Debug.Log("Distancia RH: " + sumDistanceRH);
            debugManager.enqueueDebugText("Recognize() Distancia RH: " + sumDistanceRH);
            debugManager.enqueueDebugText("Recognize() Distancia LH: " + sumDistanceLH);

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
                Debug.Log("Comparando gesto actual contra gesto almacenado de solo mano izquierda (" + gesture.gestureName + ")");
                // Si la mano derecha capturada no ha sido descartada
                // y la suma de sus distancias es menor que la encontrada con otros gestos
                // establecemos este gesto almacenado como el gesto actual reconocido más cercano
                if (!isDiscardedRH && sumDistanceRH < RHcurrentMin)
                {
                    debugManager.enqueueDebugText("Recognize() Nuevo minimo encontrado con gesto: " + gesture.gestureName);
                    RHcurrentMin = sumDistanceRH;
                    currentGesture = gesture;
                }
                else 
                {
                    if (isDiscardedRH)
                    {
                        debugManager.enqueueDebugText("Recognize() RH Descartada para gesto: " + gesture.gestureName);
                    }
                    else if (sumDistanceRH >= RHcurrentMin)
                    {
                        debugManager.enqueueDebugText("Recognize() Suma distancias RH (" + sumDistanceRH + ") > minimo actual : " + RHcurrentMin);
                    }
                }
            }
            else if (gesture.usedHand == handUsage.LEFT_HAND_ONLY)
            {
                if (!isDiscardedLH && sumDistanceLH < LHcurrentMin)
                {
                    LHcurrentMin = sumDistanceLH;
                    currentGesture = gesture;
                }
                else
                {
                    if (isDiscardedRH)
                    {
                        debugManager.enqueueDebugText("Recognize() LH Descartada para gesto : " + gesture.gestureName);
                    }
                    else if (sumDistanceRH >= RHcurrentMin)
                    {
                        debugManager.enqueueDebugText("Recognize() Suma distancias LH (" + sumDistanceLH + ") >= minimo actual : " + RHcurrentMin);
                    }
                }
            }
            else 
            {
                // Should not go here.
                Debug.Log("GestureRecognizer::Recognize() Caso de handUsage no contemplado.");
            }
        }

        // Devolvemos el gesto generado o encontrado de la lista de gestos en el Gesture Manager.
        return currentGesture;
    }
}
