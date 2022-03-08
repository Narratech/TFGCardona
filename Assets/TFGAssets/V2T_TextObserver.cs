using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class V2T_TextObserver : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro Voice2TextOutput;
    [SerializeField]
    private DebugManager debugManager;

    private string lastText;

    // Start is called before the first frame update
    void Start()
    {
        lastText = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Voice2TextOutput.text != lastText)
        { 
            lastText = Voice2TextOutput.text;
            debugManager.EnqueueChatText(lastText, eMessageSource.VOICE);
        }
    }
}
