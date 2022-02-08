using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

public class Persistence : MonoBehaviour
{
    // Gestor de gestos con la lista de gestos a reconocer
    private GestureRecognizer GR;

    // Lista interna de gestos? Usar la del manager
    private List<Gesture> gestos;

    // nombre del fichero a guardar
    private string _file;

    // Datos del gesto
    private Gesture persistenceGesture;

    // Bools
    public bool debugMode = true;

    public void Init(GestureRecognizer gr, string fileName)
    {
        // Reconocedor de gestos
        GR = gr;

        // Nombre del archivo
        _file = fileName;

        // Inicializamos lista interna usando los elementos ya almacenados en el GestureList
        gestos = new List<Gesture>();

        // Leer el fichero y cargar los gestos nada más se inicie la aplicación
        // Si falla la carga entramos en el if
        Debug.Log("Persistence::Init() - Intentando leer archivo de gestos.");
        if (!Deserialize())
        {
            Debug.Log("Persistence::Init() - No se han podido cargar gestos.");
        }
    }

    public void SaveGestureList(List<Gesture> GL)
    {
        Debug.Log("Guardando lista de gestos con tamaño (" + GL.Count + ")");
        Debug.Log("Lista de gestos almacenada en Persistence de tamaño (" + gestos.Count + ")");
        gestos.Clear();
        foreach (Gesture gesto in GL)
        {
            Gesture toAddGesture = new Gesture();
            
            // Nombre
            toAddGesture.gestureName = gesto.gestureName;
            
            // Huesos
            toAddGesture.RHBoneInfo = new List<BoneData>();
            toAddGesture.LHBoneInfo = new List<BoneData>();
            
            foreach (BoneData rhbones in gesto.RHBoneInfo)
            {
                toAddGesture.RHBoneInfo.Add(rhbones);
            }
            
            foreach (BoneData lhbones in gesto.LHBoneInfo)
            {
                toAddGesture.LHBoneInfo.Add(lhbones);
            }
            
            //Hand Usage
            toAddGesture.usedHand = gesto.usedHand;

            // Añadir gesto a lista interna del persistance
            gestos.Add(toAddGesture);
        }
        Serialize();
    }

    public void saveGesture(Gesture g)
    {
        Debug.Log("Persistence::saveGesture()");
        
        Gesture toAddGesture = new Gesture();

        // Nombre
        toAddGesture.gestureName = g.gestureName;

        // Huesos
        toAddGesture.RHBoneInfo = new List<BoneData>();
        toAddGesture.LHBoneInfo = new List<BoneData>();

        if (g.RHBoneInfo != null)
        {
            foreach (BoneData rhbones in g.RHBoneInfo)
            {
                toAddGesture.RHBoneInfo.Add(rhbones);
            }
        }
        else
        {
            Debug.Log("Persistence::saveGesture() Right Hand Gesture not found.");
        }

        if (g.LHBoneInfo != null)
        {
            foreach (BoneData lhbones in g.LHBoneInfo)
            {
                toAddGesture.LHBoneInfo.Add(lhbones);
            }
        }
        else
        {
            Debug.Log("Persistence::saveGesture() Left Hand Gesture not found.");
        }

        //Hand Usage
        toAddGesture.usedHand = g.usedHand;

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
        XmlSerializer xmlSer = new XmlSerializer(typeof(List<Gesture>));
        TextWriter writer = new StreamWriter(_file);

        bool success = true;
        try
        {
            xmlSer.Serialize(writer, gestos);
        }
        catch (Exception e)
        {
            success = false;
            Debug.Log("Persistence::Serialize() - Error al serializar el archivo: " + e.ToString());
        }

        if (success) Debug.Log("Persistence::Serialize() - Exito al guardar el archivo de persistencia");
        writer.Close();
    }


    /// <summary>
    /// Método encargado de deserializar los datos de la lista de gestos.
    /// </summary>
    public bool Deserialize()
    {
        bool debugThis = true;
        if (debugThis) Debug.Log("Persistence::Deserialize() - file: " + _file); //fileName
                                                                                 // Intentamos abrir el archivo de guardado si existe.
        FileStream fileHandler;
        try
        {
            fileHandler = new FileStream(_file, FileMode.Open); //fileName
        }
        catch (Exception)
        {
            Debug.Log("Persistence::Deserialize() - WARNING: No se ha encontrado o podido abrir el archivo: " + _file);//Name);
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
                foreach (Gesture gesto in gestos)
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

            // Pasamos los gestos deserializados a los gestos del gesture recognizer
            GR.setGestureList(gestos);

            // Cerramos el archivo pues ya no necesitamos leer nada más de él
            fileHandler.Close();

            if (debugThis) Debug.Log("Persistence::Deserialize() - Cargado archivo con exito.");

            return true;
        }
        else
        {
            fileHandler.Close();
            
            if (debugThis) Debug.Log("Persistence::Deserialize() - No se ha podido cargar el archivo de gestos.");

            return true;
        }

    }


}
