using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;


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
    public eHandUsage usedHand;
    public eGestureCategory gCategory;
    public List<eGesturePhase> gPhases;        // Un gesto puede pertenecer a un gesto sin movimiento o ser parte de uno en movimiento.
    public string singleTranscription;         // La transcripción de la componente simple del gesto.
    public List<string> composedTranscription; // Un gesto compuesto puede interpretarse de varias formas (yo, mi). Al inicio del proyecto solo usaremos la primera transcripción.
};

public class GestureRecognizer : MonoBehaviour
{
    // Referencias Externas
    [SerializeField]
    private OVRHand RightHand;
    [SerializeField]
    private OVRSkeleton RHskeleton; // Esqueleto de la mano OVRRightHand
    [SerializeField]
    private OVRHand LeftHand;
    [SerializeField]
    private OVRSkeleton LHskeleton; // Esqueleto de la mano OVRLeftHand

    // Base de datos cargada en memoria.
    [SerializeField]
    private List<Gesture> gesturesDB; // Base de datos de Gestos.

    // Gestores
    [SerializeField]
    private TextManager textManager;
    [SerializeField]
    private Persistence _persistence;

    // Variables internas
    private Gesture previousProcessedGesture;
    private Gesture previousValidatedGesture;
    private Stack<Gesture> recogGestStack; // Pila de gestos reconocidos.

    // Bools
    public bool debugMode = true;
    public bool gestureCaptured = false;
    public bool useDummyCapture = false;
    public bool isRecognizing = false;

    // Timers
    private float timeAcu = 0.0f;
    private float timeBetweenRecognition = 0.1f; // 5 seconds

    // Reconocimiento de mano
    private float threshold = 0.1f; // Umbral de reconocimiento
    private float recognizedDist = 0.0f; // Distancia cuando se reconoce el gesto
    private float minDistFound = 0.0f;   // Distancia minima cuando NO se reconoce el gesto
    private string minNameFound = "";    // Nombre de gesto más parecido sin ser reconocido
    
    // Controladores de comandos
    // Sirve para el control del tiempo entre
    // comandos consecutivos del mismo tipo
    // desde el gesture recognizer.
    //private bool allowRepeatCommand = true;
    private string lastCommand = "";
    private float timeFromLastCommand = 0.0f;
    private float timeBetweenSameCommand = 1.0f;

    // CLASE COMO SINGLETON (Es parte del prefab de OVRPlayer, así que solo debe existir uno en escena).
    // En multijugador, desactivar el gesture recognizer de todo player que no sea local.
    public static GestureRecognizer Instance;

    //////////////////////////////////////////////////////////////////////
    ///////////////////////  UNITY LIFE CYCLE ////////////////////////////
    //////////////////////////////////////////////////////////////////////
    #region Unity Life Cycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        debugMode = true;

        // Debug Manager
        if (textManager == null) textManager = new TextManager();
        textManager.Init();
        
        // Persistencia
        if (_persistence == null) _persistence = new Persistence();
        _persistence.Init(this, Application.persistentDataPath + "/LSE_DB.xml");

        // Inicializamos estructuras de datos
        previousProcessedGesture = new Gesture();
        previousValidatedGesture = new Gesture();        
        recogGestStack = new Stack<Gesture>();        
        if (gesturesDB == null) gesturesDB = new List<Gesture>();
    }


    // Update is called once per frame
    void Update()
    {
        //textManager.UpdateBonePanels();

        // Actualizar temporizadores
        timeAcu += Time.deltaTime;
        timeFromLastCommand += Time.deltaTime;

        // GESTURE RECOGNITION
        if (timeBetweenRecognition < timeAcu && !isRecognizing)
        {
            isRecognizing = true;

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
    #endregion

    //////////////////////////////////////////////////////////////////////
    ///////////////////////  GETTERS/SETTERS /////////////////////////////
    //////////////////////////////////////////////////////////////////////
    #region Getters And Setters
    public OVRHand getLeftHand()
    {
        return LeftHand;
    }

    public OVRHand getRightHand()
    {
        return RightHand;
    }

    public OVRSkeleton getLeftHandSkeleton()
    {
        return LHskeleton;
    }

    public OVRSkeleton getRightHandSkeleton()
    {
        return RHskeleton;
    }
    #endregion

    ////////////////////////////////////////////////////////////////////
    ///////////////////////  BASE DE DATOS DE GESTOS ///////////////////
    ////////////////////////////////////////////////////////////////////
    #region Gesture Database
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
                //Debug.Log(datosGesto);
            }
        }

        if (DBManager.Instance != null) DBManager.Instance.updateDB();
    }
    #endregion

    ////////////////////////////////////////////////////////////////////
    ///////////////  METODOS DE GESTION DE GESTOS //////////////////////
    ////////////////////////////////////////////////////////////////////
    #region Gesture Capture
    
    public void SaveGesture(eHandUsage handSelector, eGesturePhase phase, eGestureCategory category, string simpleTranscription, List<string> composedTranscription, string gestureName = "")
    {
        // Instanciamos un nuevo gesto
        Gesture g = new Gesture();

        // Inicializamos sus listas
        g.RHBoneInfo = new List<BoneData>();
        g.LHBoneInfo = new List<BoneData>();
        g.gPhases = new List<eGesturePhase>();
        g.composedTranscription = new List<string>();

        // Variables donde almacenamos la info de los huesos que vamos obteniendo.
        BoneData rhBoneData = new BoneData();
        BoneData lhBoneData = new BoneData();

        // Capturar la mano derecha si es necesario.
        if (handSelector == eHandUsage.RIGHT_HAND_ONLY || handSelector == eHandUsage.BOTH_HANDS)
        { 
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
        }

        // Capturar la mano izquierda si es necesario.
        if (handSelector == eHandUsage.LEFT_HAND_ONLY || handSelector == eHandUsage.BOTH_HANDS)
        { 
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
        }

        // Nombre del gesto si no ha sido aportado
        if (gestureName == "")
        {
            switch (handSelector)
            {
                case eHandUsage.RIGHT_HAND_ONLY:
                    g.gestureName = "RightGesture-" + Time.time;
                    break;
                case eHandUsage.LEFT_HAND_ONLY:
                    g.gestureName = "LeftGesture-" + Time.time;
                    break;
                case eHandUsage.BOTH_HANDS:
                    g.gestureName = "FullGesture-" + Time.time;
                    break;
                default:
                    break;
            }
        }
        else
        {
            g.gestureName = gestureName;
        }

        // HAND USAGE
        g.usedHand = handSelector;

        // Tipos
        g.gPhases.Add(phase);      // GESTURE_SIMPLE, GESTURE_BEGIN, GESTURE_END
        g.gCategory = category;    // GESTURE_WORD, GESTURE_LETTER

        // Transcripciones
        // Simple
        g.singleTranscription = simpleTranscription;

        // Compuesta
        if (composedTranscription != null)
        {
            foreach (string transcription in composedTranscription)
            {
                g.composedTranscription.Add(transcription);
            }
        }

        // Añadir gesto a la base de datos.
        gesturesDB.Add(g);

        // Guardamos en el archivo de persistencia el gesto capturado.
        Debug.Log("Llamando a guardar gesto.");
        textManager.EnqueuePersistenceText("GestureRecognizer::SaveGesture() llamando a guardar gesto.");
        _persistence.SaveGesture(g);
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
    #endregion

    //////////////////////////////////////////////////
    ///////// RECONOCIMIENTO DEL GESTO ///////////////
    //////////////////////////////////////////////////
    #region Gesture Recognition
    
    /// <summary>
    /// Por cada gesto almacenado en la lista de Gestos, lo compara contra la posición actual de las manos
    /// en la escena.
    /// Calcula las distancias de la posición del hueso respecto a la muñeca.
    /// AUN NO CONSIDERA LAS ROTACIONES.
    /// </summary>
    /// <returns></returns>
    private void Recognize()
    {
        // Debug de este método
        bool displayInDebug = false;

        // NO REALIZAR RECONOCIMIENTO SI NO SE ESTA TRAQUEANDO ALGUNA DE LAS MANOS O NO HAY SUFICIENTE CONFIANZA.
        //if (!RHskeleton.IsDataHighConfidence || !LHskeleton.IsDataHighConfidence || !RightHand.IsTracked || !LeftHand.IsTracked)
        if (!RightHand.IsTracked && !LeftHand.IsTracked)
        {
            textManager.EnqueueDebugText("Manos no encontradas.");
            isRecognizing = false;
            if (!RightHand.IsTracked || !LeftHand.IsTracked) textManager.SetRecogGUIText("Hands Untracked.");
            return;
        }
        if (!RHskeleton.IsDataHighConfidence || !LHskeleton.IsDataHighConfidence)
        { 
            textManager.SetRecogGUIText("Low Confidence.");
        }    

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

        // Por cada gesto en la lista de gestos
        foreach (var gesture in gesturesDB)
        {
            if (displayInDebug)
            { 
                textManager.EnqueueDebugText("Recognize() Comparando con Gesto: " + gesture.gestureName);
                Debug.Log("Recognize() Comparando con Gesto: " + gesture.gestureName);
            }

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
            textManager.EnqueueDebugText("Calculo Distancia Mano derecha.");
            // Almacenamos suma distancias de la mano derecha
            if ((gesture.usedHand == eHandUsage.RIGHT_HAND_ONLY || gesture.usedHand == eHandUsage.BOTH_HANDS) && RHskeleton.Bones.Count > 0)
            {
                if (displayInDebug) Debug.Log("Comparando RH. RHskeleton.Bones.Count = " + RHskeleton.Bones.Count);

                for (int i = 0; i < RHskeleton.Bones.Count; i++)
                {
                    // POSICION
                    Vector3 currentRHData = RHskeleton.transform.InverseTransformPoint(RHskeleton.Bones[i].Transform.position);
                    float RHPosDistance = Vector3.Distance(currentRHData, gesture.RHBoneInfo[i].position);

                    if (displayInDebug)
                    {
                        textManager.EnqueueDebugText("Distancia Pos hueso RH " + gesture.RHBoneInfo[i].id + " : " + RHPosDistance);
                        Debug.Log("Captured RH Pos: " + currentRHData);
                        Debug.Log("Stored RH Pos: " + gesture.LHBoneInfo[i].position);
                    }

                    // ROTACION
                    /*
                    Quaternion currentRHRotData = RHskeleton.Bones[i].Transform.rotation;
                    float RHRotDistance = quaternionDistance(currentRHRotData, gesture.RHBoneInfo[i].rotation);

                    if (displayInDebug)
                    {
                        textManager.EnqueueDebugText("Distancia Rot hueso RH " + gesture.RHBoneInfo[i].id + " : " + RHRotDistance);
                        Debug.Log("Captured RH Rot: " + currentRHRotData);
                        Debug.Log("Stored RH Rot: " + gesture.LHBoneInfo[i].rotation);
                    }
                    */
                    
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

                        if (displayInDebug)
                        { 
                            Debug.Log("Distance too great, discarding hand.");
                            textManager.EnqueueDebugText("Distancia demasiado grande (" + sumDistanceRH + "/" + threshold + "), descartando mano.");
                        }

                        break;
                    }

                    // Añadimos la distancia de este hueso a la suma total
                    sumDistanceRH += RHPosDistance; //+RHRotDistance);
                }
            }
            else
            {
                bool handNotUsed = !(gesture.usedHand == eHandUsage.RIGHT_HAND_ONLY || gesture.usedHand == eHandUsage.BOTH_HANDS);
                if (RHskeleton.Bones.Count == 0 && displayInDebug)
                {
                    Debug.Log("No Skeleton Found.");
                    textManager.EnqueueDebugText("Esqueleto de RH no encontrado. RH Descartada");
                    Debug.Log("RIGHT Hand Discarded.");
                }

                if (handNotUsed && displayInDebug)
                { 
                    Debug.Log("hand not used in this gesture.");
                    textManager.EnqueueDebugText("Mano RH no usada en este gesto. RH Descartada");
                    Debug.Log("RIGHT Hand Discarded.");
                }

                isDiscardedRH = true;
            }

            textManager.EnqueueDebugText("Calculo Distancia Mano izquierda.");
            // LEFT HAND
            // Almacenamos suma distancias de la mano izquierda
            if ((gesture.usedHand == eHandUsage.LEFT_HAND_ONLY || gesture.usedHand == eHandUsage.BOTH_HANDS) && LHskeleton.Bones.Count > 0)
            {
                if (displayInDebug) Debug.Log("Comparando LH. LHfingerBones.Count = " + LHskeleton.Bones.Count);
                for (int i = 0; i < LHskeleton.Bones.Count; i++)
                {
                    // POSICION
                    Vector3 currentLHData = LHskeleton.transform.InverseTransformPoint(LHskeleton.Bones[i].Transform.position);
                    float LHPosDistance = Vector3.Distance(currentLHData, gesture.LHBoneInfo[i].position);

                    if (displayInDebug)
                    {
                        textManager.EnqueueDebugText("Distancia Pos hueso LH " + gesture.LHBoneInfo[i].id + " : " + LHPosDistance);
                        Debug.Log("Captured LH Pos: " + currentLHData);
                        Debug.Log("Stored LH Pos: " + gesture.LHBoneInfo[i].position);
                    }

                    // ROTACION
                    Quaternion currentLHRotData = LHskeleton.Bones[i].Transform.rotation;
                    float LHRotDistance = quaternionDistance(currentLHRotData, gesture.LHBoneInfo[i].rotation);

                    if (displayInDebug)
                    {
                        textManager.EnqueueDebugText("Distancia Rot hueso LH " + gesture.LHBoneInfo[i].id + " : " + LHRotDistance);
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
                bool handNotUsed = !(gesture.usedHand == eHandUsage.LEFT_HAND_ONLY || gesture.usedHand == eHandUsage.BOTH_HANDS);

                if (displayInDebug)
                { 
                    if (LHskeleton.Bones.Count == 0)
                        Debug.Log("No Skeleton Found.");
                    if (handNotUsed)
                        Debug.Log("hand not used in this gesture.");
                    Debug.Log("LEFT Hand Discarded.");
                }
                isDiscardedLH = true;
            }


            //----------------PROCESADO DE DISTANCIAS-------------------
            textManager.EnqueueDebugText("Procesado de distancias.");
            if (displayInDebug)
            {
                Debug.Log("Distancia RH: " + sumDistanceRH);
                textManager.EnqueueDebugText("Recognize() Distancia RH: " + sumDistanceRH);
                //textManager.enqueueDebugText("Recognize() Distancia LH: " + sumDistanceLH);
            }

            if (gesture.usedHand == eHandUsage.BOTH_HANDS)
            {
                textManager.EnqueueDebugText("Procesado de distancias: Gesto de ambas manos.");
                
                if (!isDiscardedRH && !isDiscardedLH && sumDistanceRH < RHcurrentMin && sumDistanceLH < LHcurrentMin)
                {
                    if (displayInDebug) textManager.EnqueueDebugText("Recognize() Nuevo minimo encontrado con gesto: " + gesture.gestureName);
                    RHcurrentMin = sumDistanceRH;
                    LHcurrentMin = sumDistanceLH;
                    currentGesture = gesture;
                    recognizedDist = RHcurrentMin + LHcurrentMin;
                }
            }
            else if (gesture.usedHand == eHandUsage.RIGHT_HAND_ONLY)
            {
                textManager.EnqueueDebugText("Procesado de distancias: Gesto de mano derecha.");
                if (displayInDebug) Debug.Log("Comparando gesto actual contra gesto almacenado de solo mano izquierda (" + gesture.gestureName + ")");
                // Si la mano derecha capturada no ha sido descartada
                // y la suma de sus distancias es menor que la encontrada con otros gestos
                // establecemos este gesto almacenado como el gesto actual reconocido más cercano
                if (!isDiscardedRH && sumDistanceRH < RHcurrentMin)
                {
                    if (displayInDebug) textManager.EnqueueDebugText("Recognize() Nuevo minimo encontrado con gesto: " + gesture.gestureName);
                    RHcurrentMin = sumDistanceRH;
                    currentGesture = gesture;
                    recognizedDist = RHcurrentMin;
                }
                else 
                {
                    if (isDiscardedRH && displayInDebug)
                    {
                        textManager.EnqueueDebugText("Recognize() RH Descartada para gesto: " + gesture.gestureName);
                    }
                    else if (sumDistanceRH >= RHcurrentMin && displayInDebug)
                    {
                        textManager.EnqueueDebugText("Recognize() Suma distancias RH (" + sumDistanceRH + ") > minimo actual : " + RHcurrentMin);
                    }
                }
            }
            else if (gesture.usedHand == eHandUsage.LEFT_HAND_ONLY)
            {
                textManager.EnqueueDebugText("Procesado de distancias: Gesto de mano izquierda.");
                if (!isDiscardedLH && sumDistanceLH < LHcurrentMin)
                {
                    LHcurrentMin = sumDistanceLH;
                    currentGesture = gesture;
                }
                else
                {
                    if (isDiscardedRH && displayInDebug)
                    {
                        textManager.EnqueueDebugText("Recognize() LH Descartada para gesto : " + gesture.gestureName);
                    }
                    else if (sumDistanceRH >= RHcurrentMin && displayInDebug)
                    {
                        textManager.EnqueueDebugText("Recognize() Suma distancias LH (" + sumDistanceLH + ") >= minimo actual : " + RHcurrentMin);
                    }
                }
            }
            else 
            {
                // Should not go here.
                if (displayInDebug) Debug.Log("GestureRecognizer::Recognize() Caso de eHandUsage no contemplado.");
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
            textManager.EnqueueDebugText("Recognize: Gesto candidato encontrado en Recognize. Llamando a procesado.");
            ProcessRecognizedGesture(currentGesture);
        }
        else
        {
            textManager.EnqueueDebugText("Recognize: Gesto no reconocido.");
            // Los gestos no procesados
            OnProcessed(currentGesture);
        }
    }
    #endregion

    //////////////////////////////////////////////////
    ///////////// PROCESADO DEL GESTO ////////////////
    //////////////////////////////////////////////////
    #region Gesture Processing
    
    private void ProcessRecognizedGesture(Gesture recognizedGesture)
    {
        textManager.EnqueueDebugText("ProcessRecognizedGesture()\n------------------");
        textManager.EnqueueDebugText("Nombre: " + recognizedGesture.gestureName);
        textManager.EnqueueDebugText("Mano: " + recognizedGesture.usedHand);
        textManager.EnqueueDebugText("Categoria: " + recognizedGesture.gCategory);
        textManager.EnqueueDebugText("Fase Simple: " + recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE));
        textManager.EnqueueDebugText("Fase Inicio: " + recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_BEGIN));
        textManager.EnqueueDebugText("Fase Final: " + recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_END));
        textManager.EnqueueDebugText("Transcripccion simple: " + recognizedGesture.singleTranscription);

        bool showDebugInfo = true;
        bool isCommand = (recognizedGesture.gCategory == eGestureCategory.GESTURE_COMMAND);
        bool isCurrentPurelySimple = recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE) && 
            !(recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_BEGIN) || recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_END));

        if (isCommand)
        {
            OnProcessed(recognizedGesture);
            ValidateCommand(recognizedGesture);
        }
        else
        { 
            // Si el gesto reconocido solo es de tipo simple (Sin movimiento asociado)
            // Lo devolvemos para que se procese tal cual.
            if (isCurrentPurelySimple) 
            {
                if (showDebugInfo)
                {
                    textManager.EnqueueDebugText("ProcessRecognizedGesture() - Gesto reconocido solo tiene componente simple.");
                }
                ProcessSimpleGesture(recognizedGesture);
            }

            // A partir de aqui el gesto tiene UNA única componente de movimiento (también puede contener además una componente de gesto simple)
            // Lo que nunca podrá tener al mismo tiempo es dos componentes de movimiento, como sería Gesture_Begin y Gesture_End.
            if (recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_BEGIN) && recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_END))
            {
                if (showDebugInfo) 
                { 
                    Debug.Log("ProcessRecognizedGesture() - ERROR: Gesto Reconocido (" + recognizedGesture.gestureName + ") tiene componentes BEGIN y END. Revisar la DB y corregir.");
                    textManager.EnqueueDebugText("ProcessRecognizedGesture() - ERROR: Gesto Reconocido (" + recognizedGesture.gestureName + ") tiene componentes BEGIN y END. Revisar la DB y corregir.");
                } 
            }
            else if (recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_BEGIN))
            {
                if (showDebugInfo)
                {
                    textManager.EnqueueDebugText("ProcessRecognizedGesture() - Gesto reconocido tiene componente BEGIN.");
                }
                // Comprobará si es valido, verá si hay algun gesto compuesto con componente simple esperando ser procesado
                // y meterá el actual reconocido en la pila.
                ProcessBeginGesture(recognizedGesture);

            }
            else if (recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_END))
            {
                if (showDebugInfo)
                {
                    textManager.EnqueueDebugText("ProcessRecognizedGesture() - Gesto reconocido tiene componente END.");
                }
                ProcessEndGesture(recognizedGesture);
            }
            else 
            {
                if (showDebugInfo)
                { 
                    Debug.Log("processRecognizedGesture() Error - Gesto reconocido que no es puro, pero no contiene GESTURE_BEGIN ni GESTURE_END. ¡No debería suceder!");
                    textManager.EnqueueDebugText("Gesto: " + recognizedGesture.gestureName);
                    textManager.EnqueueDebugText("processRecognizedGesture() Error - Gesto reconocido que no es puro, pero no contiene GESTURE_BEGIN ni GESTURE_END. ¡No debería suceder!");
                } 
            }
        }
    }

    /// <summary>
    /// Procesado de gestos que no tienen una componente de movimiento.
    /// </summary>
    /// <param name="recognizedGesture"></param>
    private void ProcessSimpleGesture(Gesture recognizedGesture)
    {
        bool showDebugInfo = false;
        
        if (showDebugInfo) textManager.EnqueueDebugText("ProcessSimpleGesture()");

        if (recogGestStack.Count != 0)
        {
            // Debe tener en cuenta si hay gestos encolados antes del simple actual.
            Gesture PreviousGestureInStack = recogGestStack.Peek();
            
            // Si tiene un componente simple, se valida
            if (PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE))
            {
                ValidateAsSimple(PreviousGestureInStack);
            }
        }
        
        // Siempre validamos el gesto puramente simple.
        ValidateAsSimple(recognizedGesture);

        // Informamos del procesado
        OnProcessed(recognizedGesture);
        
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
        //if (true) textManager.EnqueueDebugText("ProcessBeginGesture()");
        // Si el stack de gestos esta vacío, añadimos el gesto reconocido al stack esperando al siguiente ciclo para ser validada y volvemos.
        if (recogGestStack.Count == 0)
        {
            textManager.EnqueueDebugText("ProcessBeginGesture() Stack vacío, pusheando gesto inicial al stack.");
            // Informamos del procesado
            OnProcessed(recognizedGesture);
            // Metemos en la pila
            recogGestStack.Push(recognizedGesture);
            
            return;
        }

        // Si el stack no esta vacío debemos tener en consideración el gesto que contiene.
        Gesture PreviousGestureInStack = recogGestStack.Peek();
        bool isPreviousPurelySimple = PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE) &&
            !(PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_BEGIN) || PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_END));
        bool isPreviousPurelyComposed = !PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE);

        // ¿El gesto previo reconocido era puramente simple o puramente compuesto?
        if (isPreviousPurelySimple || isPreviousPurelyComposed)
        {
            textManager.EnqueueDebugText("ProcessBeginGesture() Gesto previo era puro simple o puro compuesto.");
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
            textManager.EnqueueDebugText("ProcessBeginGesture() Gesto previo tenía componente simple.");
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
        //if (true) textManager.EnqueueDebugText("ProcessEndGesture()");
        
        // Si el stack de gestos esta vacío, el gesto actual compuesto no puede validarse.
        // Si tiene una componente simple, se valida con esta.
        // Si no tiene una componente simple, se devuelve un gesto No Reconocido.
        if (recogGestStack.Count == 0)
        {
            if (recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE))
            {
                textManager.EnqueueDebugText("ProcessEndGesture() Stack vacío y gesto compuesto con componente simple.");
                // Es importante tener en cuenta que aunque devolvamos el GESTURE_END como reconocido
                // el método OnRecognition() no lo dará por bueno si no existen en la pila de gestos
                // tanto el GESTURE_BEGIN como el GESTURE_END.
                // Por ello devolver el gesto no debería generar problemas.
                ValidateAsSimple(recognizedGesture);
            }
            else
            {
                textManager.EnqueueDebugText("ProcessEndGesture() Stack vacío, descartamos gesto.");
                // Solo informamos del gesto reconocido.
                OnProcessed(recognizedGesture);
            }
        }

        // Si el stack de gestos NO esta vacío, debemos tener en cuenta el gesto anterior a la hora de validar el gesto actual.
        Gesture PreviousGestureInStack = recogGestStack.Peek();
        bool isPreviousPurelySimple = PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE) &&
            !(PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_BEGIN) || PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_END));

        // ¿Este caso se puede dar? ¿Encolamos gestos puramente simples?
        if (isPreviousPurelySimple)
        {
            textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo puramente simple. Descartando gesto END.");
            // Si el gesto anterior era puramente Simple, ya ha sido procesado
            // Podemos eliminarlo del stack.
            recogGestStack.Pop();

            // Si el gesto actual GESTURE_END tiene componente simple
            if (recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE))
            {
                textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo simple ya procesado."); 
                textManager.EnqueueDebugText("ProcessEndGesture() Gesto actual con componente Simple. Validando simple.");
                // Lo validamos
                ValidateAsSimple(recognizedGesture);
            }

            // el gesto actual de tipo END no puede validarse y se descarta.
        }
        // si el gesto anterior tiene componente compuesta.
        else
        {
            // Si el gesto anterior corresponde al mismo signo
            if (PreviousGestureInStack.composedTranscription[0] == recognizedGesture.composedTranscription[0])
            {
                textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo del mismo SIGNO que el actual.");
                // Y el gesto anterior corresponde al inicio del signo
                if (PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_BEGIN))
                {
                    textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo es la componente BEGIN del Gesto actual.");

                    // Añadimos el gesto END a la pila despues del BEGIN
                    recogGestStack.Push(recognizedGesture);

                    textManager.EnqueueDebugText("ProcessEndGesture() Validando gesto en su componente compuesta.");
                    // Y lo devolvemos como reconocido
                    ValidateAsComposed(recognizedGesture);
                }
                // Si no es la fase correcta
                else
                {
                    // Eliminamos el gesto anterior de la pila.
                    recogGestStack.Pop();
                    textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo no es de la componente Begin.");

                    // Si el anterior tiene componente simple lo validamos.
                    if (PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE))
                    {
                        textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo tiene componente simple, validando.");
                        // Lo validamos
                        ValidateAsSimple(PreviousGestureInStack);
                    }

                    // Si el gesto actual GESTURE_END tiene componente simple
                    if (recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE))
                    {
                        textManager.EnqueueDebugText("ProcessEndGesture() Gesto actual tiene componente simple, validando.");
                        // Lo validamos
                        ValidateAsSimple(recognizedGesture);
                    }
                    else
                    {
                        textManager.EnqueueDebugText("ProcessEndGesture() Descartamos gesto actual.");
                        // Si no tiene componente simple, simplemente no pasa a la fase de validación.
                    }

                }
            }
            else
            {
                textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo corresponde a otro SIGNO.");
                // Eliminamos el gesto anterior de la pila.
                recogGestStack.Pop();

                // Si el anterior tiene componente simple lo validamos.
                if (PreviousGestureInStack.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE))
                {
                    textManager.EnqueueDebugText("ProcessEndGesture() Gesto previo tiene componente simple. Validando.");
                    // Lo validamos
                    ValidateAsSimple(PreviousGestureInStack);
                }

                // Si el gesto actual GESTURE_END tiene componente simple
                if (recognizedGesture.gPhases.Contains(eGesturePhase.GESTURE_SIMPLE))
                {
                    textManager.EnqueueDebugText("ProcessEndGesture() Gesto actual tiene componente simple. Validando (Ya que es un end).");
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
        bool showDebug = false;

        // Vemos si se ha encontrado alguno
        bool hasRecognized = ProcessedGesture.gestureName != "Unknown";
        
        if (ProcessedGesture.gestureName == "Unknown")
            textManager.SetRecogGUIText("?");

        if (showDebug)
        {
            Debug.Log("Resultado de procesado: " + ProcessedGesture.gestureName);
            textManager.EnqueueDebugText("onProcessed() Resultado: " + ProcessedGesture.gestureName);

            if (ProcessedGesture.gestureName == "Unknown")
                textManager.SetRecogText(ProcessedGesture.gestureName + "\nMinFound: " + minDistFound + "\nGesture: " + minNameFound);
            else
            {
                if (ProcessedGesture.gCategory == eGestureCategory.GESTURE_COMMAND)
                {
                    textManager.SetRecogText(ProcessedGesture.gestureName);
                }
                else if (ProcessedGesture.usedHand == eHandUsage.BOTH_HANDS)
                {
                    textManager.SetRecogText(ProcessedGesture.gestureName + "\nSuma Dist Ambas Manos: " + recognizedDist);
                }
                else
                    textManager.SetRecogText(ProcessedGesture.gestureName + "\nDist: " + recognizedDist);
            }

            // Check if new gesture
            if (hasRecognized && !ProcessedGesture.Equals(previousProcessedGesture))
            {
                // New Gesture
                Debug.Log("New Gesture Found: " + ProcessedGesture.gestureName);
                textManager.EnqueueDebugText("onProcessed() New Gesture Found: " + ProcessedGesture.gestureName);
            }
            else
            {
                if (!hasRecognized)
                {
                    Debug.Log("Gesto no reconocido.");
                    textManager.EnqueueDebugText("onProcessed() Gesto no reconocido.");
                }
                if (ProcessedGesture.Equals(previousProcessedGesture))
                {
                    Debug.Log("Mismo gesto que el anterior reconocido.");
                    textManager.EnqueueDebugText("onProcessed() Mismo gesto que el anterior.");
                }
            }
        }

        // Marcamos la flag informando que ha terminado el procesado.
        isRecognizing = false;
    }
    #endregion

    //////////////////////////////////////////////////
    ///////////// VALIDACION DE GESTO ////////////////
    //////////////////////////////////////////////////
    #region Gesture Validation
    private void ValidateCommand(Gesture RecognizedCommandGesture)
    {
        textManager.EnqueueDebugText("ValidateCommand() : " + RecognizedCommandGesture.gestureName);
        // Actualizar RecogGUI Jugador
        textManager.SetRecogGUIText(RecognizedCommandGesture.gestureName);

        if (lastCommand != RecognizedCommandGesture.gestureName || timeBetweenSameCommand < timeFromLastCommand)
        { 
            switch (RecognizedCommandGesture.gestureName)
            {
                case "SEND":
                    textManager.OnSendCommand();
                    lastCommand = "SEND";
                    break;
                case "CLEAR":
                    textManager.ClearChatBuffer();
                    lastCommand = "CLEAR";
                    break;
                case "SPACE":
                    if (lastCommand != "SPACE")
                    { 
                        textManager.AppendChatBuffer(RecognizedCommandGesture.singleTranscription);
                        lastCommand = "SPACE";
                    }
                    break;
                case "BACKSPACE":
                    textManager.BackspaceOnBuffer();
                    lastCommand = "BACKSPACE";
                    break;
            }
            previousValidatedGesture = RecognizedCommandGesture;
            timeFromLastCommand = 0.0f;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="RecognizedSimpleGesture"></param>
    private void ValidateAsSimple(Gesture RecognizedSimpleGesture)
    {
        bool debugThis = false;
        textManager.EnqueueDebugText("ValidateAsSimple() : " + RecognizedSimpleGesture.gestureName);
        // Si el anterior NO es el mismo gesto ni tiene la misma transcripción simple.
        if (!RecognizedSimpleGesture.Equals(previousValidatedGesture) && RecognizedSimpleGesture.singleTranscription != previousValidatedGesture.singleTranscription)
        {
            if (debugThis)
            {
                Debug.Log("New Simple Gesture Validated: " + RecognizedSimpleGesture.gestureName);
                textManager.EnqueueDebugText("ValidateAsSimple() Gesto Simple VALIDADO: " + RecognizedSimpleGesture.gestureName);
                textManager.EnqueueDebugText("ValidateAsSimple() Transcribiendo: " + RecognizedSimpleGesture.singleTranscription);
            }

            previousValidatedGesture = RecognizedSimpleGesture;
            //currentGesture.onRecognized.Invoke(); // Callback of that gesture

            // Actualizar RecogGUI Jugador
            textManager.SetRecogGUIText(RecognizedSimpleGesture.gestureName);
            
            // Añadir la transcripción del gesto a la ventana del buffer
            // Si es una palabra, añadir un espacio.
            if (RecognizedSimpleGesture.gCategory == eGestureCategory.GESTURE_WORD)
                textManager.AppendChatBuffer(RecognizedSimpleGesture.singleTranscription, true); // add space
            else
                textManager.AppendChatBuffer(RecognizedSimpleGesture.singleTranscription, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="RecognizedComposedGesture"></param>
    private void ValidateAsComposed(Gesture RecognizedComposedGesture)
    {
        bool debugThis = false;
        textManager.EnqueueDebugText("ValidateAsComposed() : " + RecognizedComposedGesture.gestureName);
        if (!RecognizedComposedGesture.Equals(previousValidatedGesture))
        {
            // New Gesture
            if (debugThis)
            {
                Debug.Log("New Composed Gesture Validated: " + RecognizedComposedGesture.gestureName);
                textManager.EnqueueDebugText("ValidateAsComposed() Composed Gesture Validated: " + RecognizedComposedGesture.gestureName);
                textManager.EnqueueDebugText("ValidateAsComposed() Transcribiendo: " + RecognizedComposedGesture.composedTranscription[0]);
            }

            previousValidatedGesture = RecognizedComposedGesture;
            //currentGesture.onRecognized.Invoke(); // Callback of that gesture

            // Añadir la transcripción del gesto a la ventana de input
            textManager.SetRecogGUIText(RecognizedComposedGesture.gestureName);
            

            // Añadir la transcripción del gesto a la ventana del buffer
            // Si es una palabra, añadir un espacio.
            if (RecognizedComposedGesture.gCategory == eGestureCategory.GESTURE_WORD)
                // Añadir la transcripción del gesto a la ventana del buffer
                textManager.AppendChatBuffer(RecognizedComposedGesture.composedTranscription[0], true); // add space
            else
                textManager.AppendChatBuffer(RecognizedComposedGesture.composedTranscription[0], false);
        }
    }
    #endregion
}
