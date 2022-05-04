using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DBManager : MonoBehaviour
{
    private List<Gesture> gesturesDB; // Base de datos de Gestos.

    // CLASE COMO SINGLETON.
    public static DBManager Instance;

    [SerializeField]
    private TextMeshPro textOutput;

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
        if (GestureRecognizer.Instance != null)
            gesturesDB = new List<Gesture>(GestureRecognizer.Instance.getGestureList());
        else
            gesturesDB = new List<Gesture>();

        updateText();
    }

    public void updateDB()
    {
        gesturesDB = GestureRecognizer.Instance.getGestureList().GetRange(0, GestureRecognizer.Instance.getGestureList().Count);
        updateText();
    }

    void updateText()
    {
        string buildingText = "";
        if (gesturesDB.Count > 0)
        {
            foreach (Gesture g in gesturesDB)
            {
                buildingText = g.gestureName + "\n" + buildingText;
            }
        }
        else
        {
            buildingText = "Gesture DB is empty";
        }
        textOutput.text = buildingText;
    }

    public void removeLastGestureInDB()
    {
        gesturesDB.RemoveAt(gesturesDB.Count - 1);
        GestureRecognizer.Instance.setGestureList(gesturesDB); // It calls UpdateDB()
        Persistence.Instance.SaveGestureList(gesturesDB);
    }
}
