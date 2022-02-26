using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public class Persistence : MonoBehaviour
{
    // Managers
    [SerializeField]
    private GestureRecognizer GR;
    [SerializeField]
    private DebugManager _debugManager;

    // Lista interna de gestos? Usar la del manager
    private List<Gesture> gestos;

    // nombre del fichero a guardar
    private string _gestureFile;
    private string _logFile;

    // Lista auxiliar de strings para guardar logs de texto.
    private List<string> persistenceDebugText;

    // Bools
    public bool debugMode = true;

    // Debug count
    int debugCounter = 0;

    public void Init(GestureRecognizer gr, string fileName)
    {
        _debugManager.enqueuePersistenceText(debugCounter++ + "- Inicializada la Persistencia.");

        // Reconocedor de gestos
        GR = gr;

        // Nombre del archivo
        _gestureFile = fileName;

        // Debug text
        persistenceDebugText = new List<string>();

        // Inicializamos lista interna usando los elementos ya almacenados en el GestureList
        gestos = new List<Gesture>();

        // Leer el fichero y cargar los gestos nada más se inicie la aplicación
        // Si falla la carga entramos en el if
        Debug.Log("Persistence::Init() - Intentando leer archivo de gestos.");
        _debugManager.enqueuePersistenceText(debugCounter++ + " - Persistence::Init() - Intentando leer archivo de gestos.");

        if (!Deserialize())
        {
            Debug.Log("Persistence::Init() - No se han podido cargar gestos.");
            _debugManager.enqueuePersistenceText(debugCounter++ + " - Persistence::Init() - No se han podido cargar gestos.");
        }

        
    }

    ////////////////////////////////////////////////
    ///   SERIALIZADO DE GESTOS
    ////////////////////////////////////////////////
    public void SaveGestureList(List<Gesture> GL)
    {
        // Debug y trazas
        Debug.Log("Guardando lista de gestos con tamaño (" + GL.Count + ")");
        //debugMan.enqueuePersistenceText("Guardando lista de gestos con tamaño (" + GL.Count + ")");
        Debug.Log("Lista de gestos almacenada en Persistence de tamaño (" + gestos.Count + ")");
        //debugMan.enqueuePersistenceText("Lista de gestos almacenada en Persistence de tamaño (" + gestos.Count + ")");
        
        // Limpiamos la lista auxiliar interna
        gestos.Clear();
        
        // Por cada gesto recibido, lo añadimos a la lista interna.
        foreach (Gesture gesto in GL)
        {
            Gesture toAddGesture = new Gesture();

            // Nombre
            toAddGesture.gestureName = gesto.gestureName;

            // Huesos
            toAddGesture.RHBoneInfo = new List<BoneData>();
            toAddGesture.LHBoneInfo = new List<BoneData>();

            // Hand Usage
            toAddGesture.usedHand = gesto.usedHand;

            // Tipos de gesto
            toAddGesture.gPhases = new List<gesturePhase>();

            // Categoria
            toAddGesture.gCategory = gesto.gCategory;

            // Transcripciones
            toAddGesture.singleTranscription = gesto.singleTranscription;
            toAddGesture.composedTranscription = new List<string>();

            // Rellenamos las distintas listas de valores.

            foreach (BoneData rhbones in gesto.RHBoneInfo)
            {
                toAddGesture.RHBoneInfo.Add(rhbones);
            }

            foreach (BoneData lhbones in gesto.LHBoneInfo)
            {
                toAddGesture.LHBoneInfo.Add(lhbones);
            }

            foreach (gesturePhase type in gesto.gPhases)
            {
                toAddGesture.gPhases.Add(type);
            }

            foreach (string transcription in gesto.composedTranscription)
            {
                toAddGesture.composedTranscription.Add(transcription);
            }

            // Añadir gesto a lista interna del persistance
            gestos.Add(toAddGesture);
        }

        // Serializamos los gestos almacenados en persistencia.
        Serialize();
    }

    public void SaveGesture(Gesture gesto)
    {
        Debug.Log("Persistence::saveGesture()");
        _debugManager.enqueuePersistenceText(debugCounter++ + "- Llamando a SaveGesture.");

        Gesture toAddGesture = new Gesture();

        // Nombre
        toAddGesture.gestureName = gesto.gestureName;

        // Huesos
        toAddGesture.RHBoneInfo = new List<BoneData>();
        toAddGesture.LHBoneInfo = new List<BoneData>();

        // Hand Usage
        toAddGesture.usedHand = gesto.usedHand;

        // Tipos de gesto
        toAddGesture.gPhases = new List<gesturePhase>();

        // Categoria
        toAddGesture.gCategory = gesto.gCategory;

        // Transcripciones
        toAddGesture.singleTranscription = gesto.singleTranscription;
        toAddGesture.composedTranscription = new List<string>();

        // Rellenamos las distintas listas de valores.

        foreach (BoneData rhbones in gesto.RHBoneInfo)
        {
            toAddGesture.RHBoneInfo.Add(rhbones);
        }

        foreach (BoneData lhbones in gesto.LHBoneInfo)
        {
            toAddGesture.LHBoneInfo.Add(lhbones);
        }

        foreach (gesturePhase type in gesto.gPhases)
        {
            toAddGesture.gPhases.Add(type);
        }

        foreach (string transcription in gesto.composedTranscription)
        {
            toAddGesture.composedTranscription.Add(transcription);
        }

        // Añadir gesto a lista interna del persistance
        gestos.Add(toAddGesture);

        // Llamada al serialize
        Serialize();
    }

    /// <summary>
    /// Método encargado de serializar los datos de la lista de gestos.
    /// </summary>
    private void Serialize()
    {
        _debugManager.enqueuePersistenceText(debugCounter++ + "- Llamando a Serializar.");

        // Preparamos el serializador a XML
        XmlSerializer xmlSer = new XmlSerializer(typeof(List<Gesture>));
        TextWriter writer = new StreamWriter(_gestureFile);

        bool success = true;

        // Intentamos guardar los datos
        try
        {
            xmlSer.Serialize(writer, gestos);
        }
        catch (Exception e)
        {
            success = false;
            Debug.Log("Persistence::Serialize() - Error al serializar los gestos: " + e.ToString());
            _debugManager.enqueuePersistenceText(debugCounter++ + "- Error al serializar el gesto.");
        }

        if (success)
        { 
            Debug.Log("Persistence::Serialize() - Exito al guardar el archivo de gestos");
            _debugManager.enqueuePersistenceText(debugCounter++ + "- Exito al guardar el archivo de gestos.");
        }

        // Cerramos el writer.
        writer.Close();
    }

    ////////////////////////////////////////////////
    ///   SERIALIZADO DE LOGS
    ////////////////////////////////////////////////

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="text"></param>
    public void saveTextLog(string filename, List<string> textList)
    {
        Debug.Log("saveTextLog() Archivo: " + filename + " - Tamaño textList: " + textList.Count);
        _debugManager.enqueuePersistenceText(debugCounter++ + "- saveTextLog() Archivo: " + filename + " - Tamaño textList: " + textList.Count);
        
        _logFile = filename;

        if (persistenceDebugText == null)
        {
            Debug.Log("saveTextLog() PersistenceDebugText es Null, generando nuevo.");
            _debugManager.enqueuePersistenceText(debugCounter++ + "- PersistenceDebugText es Null, generando nuevo.");
            persistenceDebugText = new List<string>(textList);
        }
        else
        {
            persistenceDebugText.Clear();
            persistenceDebugText.AddRange(textList);
            Debug.Log("saveTextLog() PersistenceDebugText existe. Limpiando y rellenando, ahora tiene tamaño: " + persistenceDebugText.Count);
            _debugManager.enqueuePersistenceText(debugCounter++ + "- PersistenceDebugText existe. Limpiando y rellenando.");
        }

        SerializeDebugText();        
    }


    private void SerializeDebugText()
    {
        Debug.Log("serializeDebugText() Preparando serializador XML");
        _debugManager.enqueuePersistenceText(debugCounter++ + "- serializeDebugText() Preparando serializador XML.");
        
        XmlSerializer xmlSer = new XmlSerializer(typeof(List<string>));
        TextWriter writer = new StreamWriter(_logFile);

        bool success = true;
        try
        {
            xmlSer.Serialize(writer, persistenceDebugText);
        }
        catch (Exception e)
        {
            success = false;
            Debug.Log("Persistence::SerializeDebugText() - Error al serializar log texto: " + e.ToString());
            _debugManager.enqueuePersistenceText(debugCounter++ + "- Error al serializar log texto");
        }

        if (success)
        { 
            Debug.Log("Persistence::SerializeDebugText() - Exito al guardar el log texto.");
            _debugManager.enqueuePersistenceText(debugCounter++ + "- Exito al guardar el log texto.");
        }
        writer.Close();
    }


    /// <summary>
    /// Método encargado de deserializar los datos de la lista de gestos.
    /// </summary>
    public bool Deserialize()
    {
        bool debugThis = true;
        if (debugThis)
        {
            Debug.Log("Persistence::Deserialize() - file: " + _gestureFile); //fileName
            _debugManager.enqueuePersistenceText(debugCounter++ + "Persistence::Deserialize()");
        }

        // Intentamos abrir el archivo de guardado si existe.
        FileStream fileHandler;
        try
        {
            fileHandler = new FileStream(_gestureFile, FileMode.Open); //fileName
        }
        catch (Exception)
        {
            Debug.Log("Persistence::Deserialize() - WARNING: No se ha encontrado o podido abrir el archivo: " + _gestureFile);//Name);
            _debugManager.enqueuePersistenceText(debugCounter++ + "Persistence::Deserialize() - WARNING: No se ha encontrado o podido abrir el archivo");
            // Volvemos al init.
            return false;
        }

        // Preparamos el serializador de XML para poder interpretar los datos.
        XmlSerializer xmlSer = new XmlSerializer(typeof(List<Gesture>));

        // Cargamos los datos del archivo al objeto estado
        if (fileHandler != null)
        {
            // Deserializamos en la interfaz de datos de gestos
            gestos = (List<Gesture>)xmlSer.Deserialize(fileHandler);

            // Debug
            bool debugLoadedBones = false;
            if (debugLoadedBones)
            {
                Debug.Log("Gestos en archivo: " + gestos.Count);
                _debugManager.enqueuePersistenceText(debugCounter++ + ".- Gestos en archivo: " + gestos.Count);

                foreach (Gesture gesto in gestos)
                {
                    string datosGesto = "Gesto: " + gesto.gestureName + "\n" + "Mano usada: " + gesto.usedHand + "\n" + "Huesos: {\n";
                    string datosHueso = "";
                    if (gesto.RHBoneInfo != null)
                    {
                        foreach (BoneData huesoiz in gesto.RHBoneInfo)
                        {
                            datosHueso = datosHueso + "    ID: "  + huesoiz.id + "\n";
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

            // Pasamos los gestos deserializados a los gestos del gesture recognizer
            GR.setGestureList(gestos);

            // Cerramos el archivo pues ya no necesitamos leer nada más de él
            fileHandler.Close();

            if (debugThis)
            {
                Debug.Log("Persistence::Deserialize() - Cargado archivo con exito.");
                _debugManager.enqueuePersistenceText(debugCounter++ + "Persistence::Deserialize() - Cargado archivo con exito");
            } 

            return true;
        }
        else
        {
            fileHandler.Close();

            if (debugThis)
            {
                Debug.Log("Persistence::Deserialize() - No se ha podido cargar el archivo de gestos.");
                _debugManager.enqueuePersistenceText(debugCounter++ + "Persistence::Deserialize() - No se ha podido cargar el archivo de gestos.");
            }

            return true;
        }

    }


}
