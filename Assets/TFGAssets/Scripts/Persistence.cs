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

    // Bools
    bool debugMode = true;

    public void Init(GestureRecognizer gr, string fileName)
    {
        // Reconocedor de gestos
        GR = gr;

        // Nombre del archivo
        _file = fileName;

        // Inicializamos lista interna usando los elementos ya almacenados en el GestureList
        gestos = new List<Gesture>(GR.getGestureList());
        
        // Leer el fichero y cargar los gestos nada más se inicie la aplicación
        // Si falla la carga entramos en el if
        if (!Deserialize())
        {
            Debug.Log("No se han podido cargar gestos.");
        }
    }

    public void SaveGestureList(List<Gesture> GL)
    {
        Debug.Log("Guardando lista de gestos con tamaño (" + GL.Count + ")");
        Debug.Log("Lista de gestos almacenada en Persistence de tamaño (" + gestos.Count + ")");
        gestos.Clear();
        gestos.AddRange(GL);
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
        bool debugThis = false;
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
        gestos = (List<Gesture>)xmlSer.Deserialize(fileHandler);

        // Cerramos el archivo pues ya no necesitamos leer nada más de él
        fileHandler.Close();

        if (debugThis) Debug.Log("Persistence::Deserialize() - Cargado archivo con exito. Procedemos al desencriptado.");

        return true;
    }


}
