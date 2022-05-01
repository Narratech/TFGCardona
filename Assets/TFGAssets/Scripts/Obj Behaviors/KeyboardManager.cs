using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    [SerializeField]
    //private TMP_InputField outputField;
    public TMP_Text outputField;

    // CLASE COMO SINGLETON
    public static KeyboardManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }
}
