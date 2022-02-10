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
    
    public OVRSkeleton RHskeleton; // Esqueleto de la mano OVRRightHand
    public OVRSkeleton LHskeleton; // Esqueleto de la mano OVRLeftHand
    public List<Gesture> gestures; // Lista de 
    private Gesture previousGesture;
    public GameObject CuboReconocimiento;
    public Material colorRojo;
    public Material colorAmarillo;
    public Material colorVerde;

    // Textos
    [SerializeField]
    private TextMeshPro textoCuboRecog;
    [SerializeField]
    private TextMeshPro textoEstadoRecog;
    [SerializeField]
    private TextMeshPro textoTimer;
    [SerializeField]
    private TextMeshPro textoDebug;
    [SerializeField]
    private TextMeshPro textoChat;
    private string sessionStart;

    // Colas de texto
    private Queue<string> debugTextQueue;
    private List<string> debugLog;
    private Queue<string> chatTextQueue;
    private List<string> chatLog;
    private int debugTextMaxLines = 45;
    private int chatTextMaxLines = 5;
    private int debugTextIndex = 0;
    private int chatTextIndex = 0;

    // Gestores
    public Persistence _persistence;

    // Bools
    public bool debugMode = true;
    public bool gestureCaptured = false;
    public bool useDummyCapture = false;
    public bool isRecognizing = false;

    // Timers
    private float timeAcu = 0.0f;
    private float timeBetweenRecognition = 5.0f; // 5 seconds

    // Reconocimiento de mano
    private float threshold = 0.08f; // Umbral de reconocimiento

    // Start is called before the first frame update
    void Start()
    {
        debugMode = true;
        previousGesture = new Gesture();
        _persistence = new Persistence();
        _persistence.Init(this, Application.persistentDataPath + "/gestos.xml");
        
        // Chat And Debug
        debugTextQueue = new Queue<string>();
        chatTextQueue = new Queue<string>();
        debugLog = new List<string>();
        chatLog = new List<string>();
        // First message.
        string nowTime = DateTime.Now.ToString();
        sessionStart = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        enqueueDebugText("Session: " + nowTime);
        enqueueChatText("Session: " + nowTime);
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
    ////////////////  METODOS DE GESTION DE TEXTO //////////////////////
    ////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Metodo que llama a la persistencia para guardar los archivos de log con los datos almacenados en el debug y chat.
    /// </summary>
    private void saveLogs()
    {
        _persistence.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-debugLog.xml", debugLog);
        _persistence.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-chatLog.xml", chatLog);
    }

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    public void enqueueDebugText(string line)
    {
        debugTextIndex++;

        // Add line
        debugTextQueue.Enqueue(debugTextIndex + ": " + line);
        debugLog.Add(debugTextIndex + ": " + line);
     
        // Si tenemos el máximo de lineas, borramos la última.
        if (debugTextQueue.Count > debugTextMaxLines)
        {
            debugTextQueue.Dequeue(); // Quitamos el primer elemento de la cola (FIFO)
        }

        // Mandamos imprimir
        updateDebugPanel();
    }

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="queue"></param>
    public void enqueueChatText(string line)
    {
        chatTextIndex++;
        
        // Add line
        chatTextQueue.Enqueue(line);
        chatLog.Add(chatTextIndex + ": " + line);

        if (chatTextQueue.Count > chatTextMaxLines)
        {
            chatTextQueue.Dequeue(); // Quitamos el primer elemento de la cola (FIFO)
        }

        updateChatPanel();
    }

    /// <summary>
    /// Actualiza el texto del panel de debug en la escena.
    /// </summary>
    public void updateDebugPanel()
    {
        int index = 0;
        string newText = "";
        textoDebug.text = "";

        
        foreach (string line in debugTextQueue)
        {
            if (index < debugTextQueue.Count - 1)
                newText += line + "\n";
            else
                newText += line;
            index++;
        }

        textoDebug.text = newText;
        saveLogs();
    }

    /// <summary>
    /// Actualiza el texto del panel de chat en la escena.
    /// </summary>
    public void updateChatPanel()
    {
        int index = 0;
        textoChat.text = "";

        foreach (string line in chatTextQueue)
        {
            if (index < chatTextQueue.Count - 1)
                textoChat.text += line + "\n";
            else
                textoChat.text += line;
            index++;
        }
        saveLogs();
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
        
        //if (nextIn < 2.0f)
        //{ 
        //    textoCuboRecog.text = "Esperando\nsiguiente\nintento";
        //    textoEstadoRecog.text = "Esperando\nsiguiente\nintento";
        //}

        // GESTURE RECOGNITION
        if (timeBetweenRecognition < timeAcu) //!gestureCaptured
        {
            isRecognizing = true;

            // Textos debug
            Debug.Log("Update() - Intentando reconocer gesto.");
            enqueueDebugText("-------------------------------------------");
            enqueueDebugText("Update() Intentando reconocer gesto actual.");
            textoEstadoRecog.text = "Intentando reconocer\n gesto.";
            
            // Obtenemos el gesto Actual.
            Gesture currentGesture = Recognize();
            
            // Debug
            Debug.Log("Resultado de reconocimiento: " + currentGesture.gestureName);
            enqueueDebugText("Update() Resultado: " + currentGesture.gestureName);
            textoEstadoRecog.text = textoEstadoRecog.text + "\nResultado reconocimiento:\n " + currentGesture.gestureName;
            
            // Vemos si se ha encontrado alguno
            bool hasRecognized = currentGesture.gestureName != "notFound";

            // Check if new gesture
            if (hasRecognized && !currentGesture.Equals(previousGesture))
            {
                // New Gesture
                Debug.Log("New Gesture Found: " + currentGesture.gestureName);
                enqueueDebugText("Update() New Gesture Found: " + currentGesture.gestureName);
                CuboReconocimiento.GetComponent<Renderer>().material = colorVerde;
                textoCuboRecog.text = "GESTO RECONOCIDO! (" + currentGesture.gestureName + ")";
                previousGesture = currentGesture;
                //currentGesture.onRecognized.Invoke(); // Callback of that gesture
                isRecognizing = false;
            }
            else
            {
                if (!hasRecognized)
                { 
                    Debug.Log("Gesto no reconocido.");
                    enqueueDebugText("Update() Gesto no reconocido.");
                    textoCuboRecog.text = "GESTO NO RECONOCIDO.";
                    CuboReconocimiento.GetComponent<Renderer>().material = colorRojo;
                }
                if (currentGesture.Equals(previousGesture))
                { 
                    Debug.Log("Mismo gesto que el anterior reconocido.");
                    enqueueDebugText("Update() Mismo gesto que el anterior.");
                    textoCuboRecog.text = "MISMO GESTO QUE EL ANTERIOR RECONOCIDO.";
                    CuboReconocimiento.GetComponent<Renderer>().material = colorAmarillo;
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
        currentGesture.gestureName = "notFound";
        currentGesture.LHBoneInfo = new List<BoneData>();
        currentGesture.RHBoneInfo = new List<BoneData>();

        // Variables de calculo de distancias generales
        // Inicializadas al infinito.
        float RHcurrentMin = Mathf.Infinity;
        float LHcurrentMin = Mathf.Infinity;

        // Debug del metodo
        bool showDebug = false;

        // Por cada gesto en la lista de gestos
        foreach (var gesture in gestures)
        {
            enqueueDebugText("Recognize() Comparando con Gesto: " + gesture.gestureName);
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
                        enqueueDebugText("Distancia Pos hueso RH " + gesture.RHBoneInfo[i].id + " : " + RHPosDistance);
                        Debug.Log("Captured RH Pos: " + currentRHData);
                        Debug.Log("Stored RH Pos: " + gesture.LHBoneInfo[i].position);
                    }

                    // ROTACION
                    Quaternion currentRHRotData = RHskeleton.Bones[i].Transform.rotation;
                    float RHRotDistance = quaternionDistance(currentRHRotData, gesture.RHBoneInfo[i].rotation);

                    if (showDebug)
                    {
                        enqueueDebugText("Distancia Rot hueso RH " + gesture.RHBoneInfo[i].id + " : " + RHRotDistance);
                        Debug.Log("Captured RH Rot: " + currentRHRotData);
                        Debug.Log("Stored RH Rot: " + gesture.LHBoneInfo[i].rotation);
                    }

                    // Si la suma de la distancia supera el umbral máximo de reconocimiento, descartamos la mano.
                    if (sumDistanceRH > threshold)
                    {
                        isDiscardedRH = true;

                        Debug.Log("Distance too great, discarding hand.");
                        enqueueDebugText("Distancia demasiado grande (" + sumDistanceRH + "/" + threshold + "), descartando mano.");

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
                    enqueueDebugText("Esqueleto de RH no encontrado. RH Descartada");
                }

                if (handNotUsed)
                { 
                    Debug.Log("hand not used in this gesture.");
                    enqueueDebugText("Mano RH no usada en este gesto. RH Descartada");
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
                        enqueueDebugText("Distancia Pos hueso LH " + gesture.LHBoneInfo[i].id + " : " + LHPosDistance);
                        Debug.Log("Captured LH Pos: " + currentLHData);
                        Debug.Log("Stored LH Pos: " + gesture.LHBoneInfo[i].position);
                    }

                    // ROTACION
                    Quaternion currentLHRotData = LHskeleton.Bones[i].Transform.rotation;
                    float LHRotDistance = quaternionDistance(currentLHRotData, gesture.LHBoneInfo[i].rotation);

                    if (showDebug)
                    {
                        enqueueDebugText("Distancia Rot hueso LH " + gesture.LHBoneInfo[i].id + " : " + LHRotDistance);
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
            enqueueDebugText("Recognize() Distancia RH: " + sumDistanceRH);
            enqueueDebugText("Recognize() Distancia LH: " + sumDistanceLH);

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
                    enqueueDebugText("Recognize() Nuevo minimo encontrado con gesto: " + gesture.gestureName);
                    RHcurrentMin = sumDistanceRH;
                    currentGesture = gesture;
                }
                else 
                {
                    if (isDiscardedRH)
                    { 
                        enqueueDebugText("Recognize() RH Descartada para gesto: " + gesture.gestureName);
                    }
                    else if (sumDistanceRH >= RHcurrentMin)
                    {
                        enqueueDebugText("Recognize() Suma distancias RH (" + sumDistanceRH + ") > minimo actual : " + RHcurrentMin);
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
                        enqueueDebugText("Recognize() LH Descartada para gesto : " + gesture.gestureName);
                    }
                    else if (sumDistanceRH >= RHcurrentMin)
                    {
                        enqueueDebugText("Recognize() Suma distancias LH (" + sumDistanceLH + ") >= minimo actual : " + RHcurrentMin);
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
