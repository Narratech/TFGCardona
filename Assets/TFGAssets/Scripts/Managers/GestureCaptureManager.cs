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
    private handUsage usedHand;
    private gesturePhase phase;    
    private gestureCategory category;
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
        usedHand = handUsage.NOHAND;
        phase = gesturePhase.GESTURE_SIMPLE;
        category = gestureCategory.GESTURE_LETTER;
        composedTranscription = new List<string>();
    }

    public string getGestureName()
    {
        // Construcción del nombre LSE_RH_GestureName
        string gestureName = "LSE";

        switch (usedHand)
        {
            case handUsage.LEFT_HAND_ONLY:
                gestureName = gestureName + "_LH_";
                break;
            case handUsage.RIGHT_HAND_ONLY:
                gestureName = gestureName + "_RH_";
                break;
            case handUsage.BOTH_HANDS:
                gestureName = gestureName + "_2H_";
                break;
        }

        gestureName = gestureName + gestureNameText.text;

        return gestureName;
    }

    public gesturePhase getPhase()
    {
        return phase;
    }

    public handUsage getHand() 
    {
        return usedHand;
    }

    public gestureCategory getCategory()
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

    public void setPhase(gesturePhase newPhase)
    {
        guideText.text = "Fase configurada a " + newPhase;
        phase = newPhase;
    }

    public void setHand(handUsage newHand)
    {
        guideText.text = "Uso manos configurado a " + newHand;
        usedHand = newHand;
    }

    public void setCategory(gestureCategory newCat)
    {
        guideText.text = "Categoría gesto configurado a " + newCat;
        category = newCat;
    }

}
