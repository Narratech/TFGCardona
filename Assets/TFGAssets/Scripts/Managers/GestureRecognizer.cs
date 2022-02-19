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
public enum gestureType 
{
    GESTURE_WORD,     // Si es una palabra (añadir espacio despues de ella)
    GESTURE_LETTER,   // Si es una letra (no añadir espacio)
    GESTURE_SIMPLE,   // Gestos sin movimiento
    GESTURE_BEGIN,    // Inicio de gesto en movimiento
    GESTURE_END       // Fin gesto en movimiento
}
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
    public UnityEvent onRecognized;            // Callback
    public handUsage usedHand;
    public List<gestureType> gTypes;           // Un gesto puede pertenecer a un gesto sin movimiento o ser parte de uno en movimiento.
    public string singleTranscription;         // La transcripción de la componente simple del gesto.
    public List<string> composedTranscription; // Un gesto compuesto puede interpretarse de varias formas (yo, mi). Al inicio del proyecto solo usaremos la primera transcripción.
};

public class GestureRecognizer : MonoBehaviour
{
    // Referencias Externas
    public DebugManager debugManager;
    public OVRSkeleton RHskeleton; // Esqueleto de la mano OVRRightHand
    public OVRSkeleton LHskeleton; // Esqueleto de la mano OVRLeftHand
    public List<Gesture> gesturesDB; // Base de datos de Gestos.
    public Stack<Gesture> recogGestStack; // Pila de gestos reconocidos.

    private Gesture previousProcessedGesture;
    private Gesture previousValidatedGesture;
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
    private float recognizedDist = 0.0f; // Distancia cuando se reconoce el gesto
    private float minDistFound = 0.0f;   // Distancia minima cuando NO se reconoce el gesto
    private string minNameFound = "";    // Nombre de gesto más parecido sin ser reconocido

    // Start is called before the first frame update
    void Start()
    {
        debugMode = true;

        // Debug Manager
        if (debugManager == null) debugManager = new DebugManager();
        debugManager.Init();
        
        // Persistencia
        if (_persistence == null) _persistence = new Persistence();
        _persistence.Init(this, Application.persistentDataPath + "/LSE_DB.xml");

        previousProcessedGesture = new Gesture();
        previousValidatedGesture = new Gesture();
    }


    public List<Gesture> getGestureList()
    {
        return gesturesDB;
    }

    public void setGestureList(List<Gesture> newList)
    {
        gesturesDB.Clear();
        gesturesDB.AddRange(newList);

        // Debug
        bool debugLoadedBones = true;
        if (debugLoadedBones)
        {
            Debug.Log("GestureRecognizer::setGestureList() Gestos en Manager: " + gesturesDB.Count);
            foreach (Gesture gesto in gesturesDB)
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

            // WORKFLOW DEL RECONOCIMIENTO GESTO
            // RECONOCIMIENTO->PROCESADO->VALIDACION
            // 
            // RECONOCIMIENTO: Toma los valores de la mano en escena, los compara con la base de datos y si encuentra una coincidencia la procesa.
            // PROCESADO: Tiene en cuenta gestos anteriores para intentar validar el gesto reconocido. Al terminar informa del resultado y actualiza debugs.
            // VALIDACION: Da el valor final de reconocimiento, transcribiendo al chat el valor del gesto segun si lo ha procesado como gesto simple o compuesto.

            // Recognize inicia el Workflow, llama a los metodos de procesado y estos a la validación si lo ven conveniente.
            Recognize();
            
            timeAcu = 0.0f;
        }
    }

   

    ////////////////////////////////////////////////////////////////////
    ///////////////  METODOS DE GESTION DE GESTOS //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Permite almacenar un nuevo gesto dentro del sistema de Gestos.
    /// </summary>
    public void SaveFullGesture(gestureType fase = gestureType.GESTURE_SIMPLE, gestureType categoria = gestureType.GESTURE_LETTER, string simpleTranscription = "CHANGE_THIS", List<string> composedTranscription = null)
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

        // Tipos
        g.gTypes.Add(fase);         // GESTURE_SIMPLE, GESTURE_BEGIN, GESTURE_END
        g.gTypes.Add(categoria);    // GESTURE_WORD, GESTURE_LETTER

        // Transcripciones
        g.singleTranscription = simpleTranscription;
        if (composedTranscription != null)
        {
            g.composedTranscription = new List<string>();
            foreach (string transcription in composedTranscription)
            {
                g.composedTranscription.Add(transcription);
            }
        }

        gesturesDB.Add(g);

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
    public void SaveRightHandGesture(gestureType fase = gestureType.GESTURE_SIMPLE, gestureType categoria = gestureType.GESTURE_LETTER, string simpleTranscription = "CHANGE_THIS", List<string> composedTranscription = null)
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

        // Tipos
        g.gTypes.Add(fase);         // GESTURE_SIMPLE, GESTURE_BEGIN, GESTURE_END
        g.gTypes.Add(categoria);    // GESTURE_WORD, GESTURE_LETTER

        // Transcripciones
        g.singleTranscription = simpleTranscription;
        if (composedTranscription != null) 
        {
            g.composedTranscription = new List<string>();
            foreach (string transcription in composedTranscription)
            {
                g.composedTranscription.Add(transcription);
            }
        }

        gesturesDB.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        _persistence.saveGesture(g);
    }
  
    /// <summary>
    /// Permite almacenar un nuevo gesto de la mano derecha en el sistema de gestos.
    /// </summary>
    public void SaveLeftHandGesture(gestureType fase = gestureType.GESTURE_SIMPLE, gestureType categoria = gestureType.GESTURE_LETTER, string simpleTranscription = "CHANGE_THIS", List<string> composedTranscription = null)
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

        // Tipos
        g.gTypes.Add(fase);         // GESTURE_SIMPLE, GESTURE_BEGIN, GESTURE_END
        g.gTypes.Add(categoria);    // GESTURE_WORD, GESTURE_LETTER

        // Transcripciones
        g.singleTranscription = simpleTranscription;
        if (composedTranscription != null)
        {
            g.composedTranscription = new List<string>();
            foreach (string transcription in composedTranscription)
            {
                g.composedTranscription.Add(transcription);
            }
        }

        gesturesDB.Add(g);

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


    //////////////////////////////////////////////////
    ///////// RECONOCIMIENTO DEL GESTO ///////////////
    //////////////////////////////////////////////////

    /// <summary>
    /// Por cada gesto almacenado en la lista de Gestos, lo compara contra la posición actual de las manos
    /// en la escena.
    /// Calcula las distancias de la posición del hueso respecto a la muñeca.
    /// AUN NO CONSIDERA LAS ROTACIONES.
    /// </summary>
    /// <returns></returns>
    private void Recognize()
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

        // Debug for out of threshold and not found. Also distance when recognized.
        minDistFound = Mathf.Infinity;
        minNameFound = "";
        recognizedDist = Mathf.Infinity;

        // Debug del metodo
        bool showDebug = false;

        // Por cada gesto en la lista de gestos
        foreach (var gesture in gesturesDB)
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
            //float sumRotDistRH = 0.0f;
            //float sumRotDistLH = 0.0f;

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

                        // GESTO NO RECONOCIDO: Guardamos valores del más parecido.
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
                    recognizedDist = RHcurrentMin;
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

        // Esta parte del reconocimiento procesa el gesto
        // Si es un gesto tiene componente en movimiento no final, devolverá gesto no reconocido
        // y esperará al siguiente gesto. Si además de componente en movimiento tiene componente
        // simple (es reconocible como otro gesto sin moverse), en el siguiente bucle, dependiendo
        // de lo reconocido, devolverá el componente simple, o el gesto en movimiento completo.
        // Finalmente mete en la pila el gesto.
        if (currentGesture.gestureName != "Unknown")
        {
            ProcessRecognizedGesture(currentGesture);
        }
        else
        {
            // Los gestos no procesados
            OnProcessed(currentGesture);
        }
    }



    //////////////////////////////////////////////////
    ///////////// PROCESADO DEL GESTO ////////////////
    //////////////////////////////////////////////////

    private void ProcessRecognizedGesture(Gesture recognizedGesture)
    {
        bool isCurrentPurelySimple = recognizedGesture.gTypes.Contains(gestureType.GESTURE_SIMPLE) && 
            !(recognizedGesture.gTypes.Contains(gestureType.GESTURE_BEGIN) || recognizedGesture.gTypes.Contains(gestureType.GESTURE_END));

        // Si el gesto reconocido solo es de tipo simple (Sin movimiento asociado)
        // Lo devolvemos para que se procese tal cual.
        if (isCurrentPurelySimple) 
        {
            ProcessSimpleGesture(recognizedGesture);
        }

        // A partir de aqui el gesto tiene UNA única componente de movimiento (también puede contener además una componente de gesto simple)
        // Lo que nunca podrá tener al mismo tiempo es dos componentes de movimiento, como sería Gesture_Begin y Gesture_End.
        if (recognizedGesture.gTypes.Contains(gestureType.GESTURE_BEGIN) && recognizedGesture.gTypes.Contains(gestureType.GESTURE_END))
        {
            Debug.Log("Recognize() - ERROR: Gesto Reconocido (" + recognizedGesture.gestureName + ") tiene componentes BEGIN y END. Revisar la DB y corregir.");
        }
        else if (recognizedGesture.gTypes.Contains(gestureType.GESTURE_BEGIN))
        {
            // Comprobará si es valido, verá si hay algun gesto compuesto con componente simple esperando ser procesado
            // y meterá el actual reconocido en la pila.
            ProcessBeginGesture(recognizedGesture);

        }
        else if (recognizedGesture.gTypes.Contains(gestureType.GESTURE_END))
        {
            ProcessEndGesture(recognizedGesture);
        }
        else 
        {
            Debug.Log("processRecognizedGesture() Error - Gesto reconocido que no es puro, pero no contiene GESTURE_BEGIN ni GESTURE_END. ¡No debería suceder!");
        }
    }

    /// <summary>
    /// Procesado de gestos que no tienen una componente de movimiento.
    /// </summary>
    /// <param name="recognizedGesture"></param>
    private void ProcessSimpleGesture(Gesture recognizedGesture)
    {
        if (recogGestStack.Count != 0)
        {
            // Debe tener en cuenta si hay gestos encolados antes del simple actual.
            Gesture PreviousGestureInStack = recogGestStack.Peek();
            
            // Si tiene un componente simple, se valida
            if (PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_SIMPLE))
            {
                ValidateAsSimple(PreviousGestureInStack);
            }
        }
        // Informamos del procesado
        OnProcessed(recognizedGesture);
        // Siempre validamos el gesto puramente simple.
        ValidateAsSimple(recognizedGesture);
    }

    /// <summary>
    /// Lógica que procesa el gesto reconocido de tipo GESTURE_BEGIN.
    /// Devolverá el gesto actual detectado o el anterior con componente compuesta no validada
    /// pero con componente simple esperando a ser validada.
    /// </summary>
    /// <param name="recognizedGesture"></param>
    /// <returns></returns>
    private void ProcessBeginGesture(Gesture recognizedGesture)
    {
        // Si el stack de gestos esta vacío, añadimos el gesto reconocido al stack esperando al siguiente ciclo para ser validada y volvemos.
        if (recogGestStack.Count == 0)
        {
            // Informamos del procesado
            OnProcessed(recognizedGesture);
            // Metemos en la pila
            recogGestStack.Push(recognizedGesture);
            
            return;
        }

        // Si el stack no esta vacío debemos tener en consideración el gesto que contiene.
        Gesture PreviousGestureInStack = recogGestStack.Peek();
        bool isPreviousPurelySimple = PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_SIMPLE) &&
            !(PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_BEGIN) || PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_END));
        bool isPreviousPurelyComposed = !PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_SIMPLE);

        // ¿El gesto previo reconocido era puramente simple o puramente compuesto?
        if (isPreviousPurelySimple || isPreviousPurelyComposed)
        {
            // Si el gesto previo es puramente simple, fue procesado directamente en el anterior ciclo.
            // Si el gesto previo es puramente compuesto, o era un END ya procesado, o era un BEGIN que no ha sido validado.
            // En ambos casos podemos eliminarlo de la pila como un descarte.
            recogGestStack.Pop();

            // Al haber reconocido el nuevo gesto como un gesto con componente
            // en movimiento de tipo inicio, podemos encolar el actual en la pila.
            recogGestStack.Push(recognizedGesture);

            // Informamos del procesado
            OnProcessed(recognizedGesture);
        }
        else 
        {
            // Si el gesto anterior tenía una componente simple además de la compuesta, esta esperando a ser validada.
            
            // Como el nuevo gesto no valida la componente compuesta del gesto anterior,
            // pero este gesto anterior tiene un componente simple, debemos mostrar el
            // gesto simple detectado anteriormente.

            // borramos el gesto anterior ya que vamos a procesarlo.
            recogGestStack.Pop();

            // Añadimos el gesto actual reconocido con componente BEGIN
            recogGestStack.Push(recognizedGesture);

            // Validamos el anterior
            ValidateAsSimple(PreviousGestureInStack);

            // Informamos del actual detectado
            OnProcessed(recognizedGesture);
        }
    }

    private void ProcessEndGesture(Gesture recognizedGesture)
    {
        // Si el stack de gestos esta vacío, el gesto actual compuesto no puede validarse.
        // Si tiene una componente simple, se valida con esta.
        // Si no tiene una componente simple, se devuelve un gesto No Reconocido.
        if (recogGestStack.Count == 0)
        {
            if (recognizedGesture.gTypes.Contains(gestureType.GESTURE_SIMPLE))
            {
                // Es importante tener en cuenta que aunque devolvamos el GESTURE_END como reconocido
                // el método OnRecognition() no lo dará por bueno si no existen en la pila de gestos
                // tanto el GESTURE_BEGIN como el GESTURE_END.
                // Por ello devolver el gesto no debería generar problemas.
                ValidateAsSimple(recognizedGesture);
            }
            else
            {
                // Solo informamos del gesto reconocido.
                OnProcessed(recognizedGesture);
            }
        }

        // Si el stack de gestos NO esta vacío, debemos tener en cuenta el gesto anterior a la hora de validar el gesto actual.
        Gesture PreviousGestureInStack = recogGestStack.Peek();
        bool isPreviousPurelySimple = PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_SIMPLE) &&
            !(PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_BEGIN) || PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_END));

        // ¿Este caso se puede dar? ¿Encolamos gestos puramente simples?
        if (isPreviousPurelySimple)
        {
            // Si el gesto anterior era puramente Simple, ya ha sido procesado
            // Podemos eliminarlo del stack.
            recogGestStack.Pop();

            // el gesto actual de tipo END no puede validarse y se descarta.
        }
        // si el gesto anterior tiene componente compuesta.
        else
        {
            // Si el gesto anterior corresponde al mismo signo
            if (PreviousGestureInStack.composedTranscription[0] == recognizedGesture.composedTranscription[0])
            {
                // Y el gesto anterior corresponde al inicio del signo
                if (PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_BEGIN))
                {
                    // Añadimos el gesto END a la pila despues del BEGIN
                    recogGestStack.Push(recognizedGesture);

                    // Y lo devolvemos como reconocido
                    ValidateAsComposed(recognizedGesture);
                }
                // Si no es la fase correcta
                else
                {
                    // Eliminamos el gesto anterior de la pila.
                    recogGestStack.Pop();

                    // Si el anterior tiene componente simple lo validamos.
                    if (PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_SIMPLE))
                    {
                        // Lo validamos
                        ValidateAsSimple(PreviousGestureInStack);
                    }

                    // Si el gesto actual GESTURE_END tiene componente simple
                    if (recognizedGesture.gTypes.Contains(gestureType.GESTURE_SIMPLE))
                    {
                        // Lo validamos
                        ValidateAsSimple(recognizedGesture);
                    }

                    // Si no tiene componente simple, simplemente no pasa a la fase de validación.
                }
            }
            else
            {
                // Eliminamos el gesto anterior de la pila.
                recogGestStack.Pop();

                // Si el anterior tiene componente simple lo validamos.
                if (PreviousGestureInStack.gTypes.Contains(gestureType.GESTURE_SIMPLE))
                {
                    // Lo validamos
                    ValidateAsSimple(PreviousGestureInStack);
                }

                // Si el gesto actual GESTURE_END tiene componente simple
                if (recognizedGesture.gTypes.Contains(gestureType.GESTURE_SIMPLE))
                {
                    // Lo validamos
                    ValidateAsSimple(recognizedGesture);
                }
            }
        }
        // Informamos del fin del proceso.
        OnProcessed(recognizedGesture);
    }

    /// <summary>
    /// Este metodo da información del gesto reconocido, pero solo informa
    /// y no tiene nada que ver con el proceso de validación que escribe
    /// el resultado del reconocimiento.
    /// </summary>
    /// <param name="ProcessedGesture"></param>
    private void OnProcessed(Gesture ProcessedGesture)
    {
        Debug.Log("Resultado de procesado: " + ProcessedGesture.gestureName);
        debugManager.enqueueDebugText("onProcessed() Resultado: " + ProcessedGesture.gestureName);

        // Vemos si se ha encontrado alguno
        bool hasRecognized = ProcessedGesture.gestureName != "Unknown";
        if (ProcessedGesture.gestureName == "Unknown")
            debugManager.setRecogText(ProcessedGesture.gestureName + "\nMinFound: " + minDistFound + "\nGesture: " + minNameFound);
        else
            debugManager.setRecogText(ProcessedGesture.gestureName + "\nDist: " + recognizedDist);

        // Check if new gesture
        if (hasRecognized && !ProcessedGesture.Equals(previousProcessedGesture))
        {
            // New Gesture
            Debug.Log("New Gesture Found: " + ProcessedGesture.gestureName);
            debugManager.enqueueDebugText("onProcessed() New Gesture Found: " + ProcessedGesture.gestureName);
        }
        else
        {
            if (!hasRecognized)
            {
                Debug.Log("Gesto no reconocido.");
                debugManager.enqueueDebugText("onProcessed() Gesto no reconocido.");
            }
            if (ProcessedGesture.Equals(previousProcessedGesture))
            {
                Debug.Log("Mismo gesto que el anterior reconocido.");
                debugManager.enqueueDebugText("onProcessed() Mismo gesto que el anterior.");
            }
        }
        
        // Marcamos la flag informando que ha terminado el procesado.
        isRecognizing = false;
    }

    //////////////////////////////////////////////////
    ///////////// VALIDACION DE GESTO ////////////////
    //////////////////////////////////////////////////

    /// <summary>
    /// 
    /// </summary>
    /// <param name="RecognizedSimpleGesture"></param>
    private void ValidateAsSimple(Gesture RecognizedSimpleGesture)
    {
        // Comprobar si anterior es la misma.
        // Check if new gesture
        if (!RecognizedSimpleGesture.Equals(previousValidatedGesture))
        {
            // New Gesture
            Debug.Log("New Simple Gesture Validated: " + RecognizedSimpleGesture.gestureName);
            debugManager.enqueueDebugText("ValidateAsSimple() Gesto Simple VALIDADO: " + RecognizedSimpleGesture.gestureName);
            debugManager.enqueueDebugText("ValidateAsSimple() Transcribiendo: " + RecognizedSimpleGesture.singleTranscription);

            previousValidatedGesture = RecognizedSimpleGesture;
            //currentGesture.onRecognized.Invoke(); // Callback of that gesture

            // TO DO 
            // Añadir la transcripción del gesto a la ventana de input
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="RecognizedSimpleGesture"></param>
    private void ValidateAsComposed(Gesture RecognizedComposedGesture)
    {
        if (!RecognizedComposedGesture.Equals(previousValidatedGesture))
        {
            // New Gesture
            Debug.Log("New Composed Gesture Validated: " + RecognizedComposedGesture.gestureName);
            debugManager.enqueueDebugText("ValidateAsComposed() Composed Gesture Validated: " + RecognizedComposedGesture.gestureName);
            debugManager.enqueueDebugText("ValidateAsComposed() Transcribiendo: " + RecognizedComposedGesture.composedTranscription[0]);

            previousValidatedGesture = RecognizedComposedGesture;
            //currentGesture.onRecognized.Invoke(); // Callback of that gesture

            // TO DO 
            // Añadir la transcripción del gesto a la ventana de input
        }
    }
}
