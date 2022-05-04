using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> steps;

    int currentStep = 0;

    // CLASE COMO SINGLETON.
    public static TutorialManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // Nos aseguramos de esconder todos los pasos
        foreach (GameObject step in steps)
        {
            step.SetActive(false);
        }

        // Ponemos el indice a 0
        currentStep = 0;

        // activamos el primero
        steps[currentStep].SetActive(true);
    }

    public void OnNextStep()
    {
        steps[currentStep].SetActive(false);
        currentStep++;
        if (currentStep == steps.Count) currentStep = 0;
        steps[currentStep].SetActive(true);
    }


    public void OnPreviousStep()
    {
        steps[currentStep].SetActive(false);
        currentStep--;
        if (currentStep < 0) currentStep = steps.Count - 1;
        steps[currentStep].SetActive(true);
    }
}
