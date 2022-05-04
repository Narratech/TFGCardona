using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GestureCaptureManager : MonoBehaviour
{
    /// <summary>
    /// Esta clase debe usarse en conjunto con ConfigureCaptureButton y HandConfiguratorButton
    /// </summary>

    [SerializeField]
    private TextMeshPro gestureNameText;
    [SerializeField]
    private TextMeshPro guideText;

    // Valores son tomados de los botones de configuración
    private eHandUsage usedHand;
    private eGesturePhase phase;    
    private eGestureCategory category;
    private List<string> composedTranscription;

    public static GestureCaptureManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        usedHand = eHandUsage.NOHAND;
        phase = eGesturePhase.GESTURE_SIMPLE;
        category = eGestureCategory.GESTURE_LETTER;
        composedTranscription = new List<string>();
    }

    public string getGestureName()
    {
        // Construcción del nombre LSE_RH_GestureName
        string gestureName = "LSE";

        switch (usedHand)
        {
            case eHandUsage.LEFT_HAND_ONLY:
                gestureName = gestureName + "_LH_";
                break;
            case eHandUsage.RIGHT_HAND_ONLY:
                gestureName = gestureName + "_RH_";
                break;
            case eHandUsage.BOTH_HANDS:
                gestureName = gestureName + "_2H_";
                break;
        }

        gestureName = gestureName + gestureNameText.text;

        return gestureName;
    }

    public eGesturePhase getPhase()
    {
        return phase;
    }

    public eHandUsage getHand() 
    {
        return usedHand;
    }

    public eGestureCategory getCategory()
    {
        return category;
    }

    public string getSimpleTranscription()
    {
        return gestureNameText.text;
    }

    public List<string> getComplexTranscription()
    {
        // WIP por ahora solo se devuelve la transcripción simple.
        composedTranscription.Clear();
        composedTranscription.Add(gestureNameText.text);
        return composedTranscription;
    }

    public void setPhase(eGesturePhase newPhase)
    {
        guideText.text = "Fase configurada a " + newPhase;
        phase = newPhase;
    }

    public void setHand(eHandUsage newHand)
    {
        guideText.text = "Uso manos configurado a " + newHand;
        usedHand = newHand;
    }

    public void setCategory(eGestureCategory newCat)
    {
        guideText.text = "Categoría gesto configurado a " + newCat;
        category = newCat;
    }

}
