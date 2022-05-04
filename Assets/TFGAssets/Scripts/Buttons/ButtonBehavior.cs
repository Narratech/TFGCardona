using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonBehavior : MonoBehaviour
{
    // -- VARIABLES CONEXION --
    // Panel de Conexion
    [Header("CONEXION")]
    // Panels
    public GameObject ConnectOptionPanel;
    public GameObject ConnectWithNamePanel;
    // LoginManager
    public LoginManager loginManager;

    // -- VARIABLES ONOFF --
    [Header("ON-OFF")]
    public GameObject OnOffObject;

    // -- VARIABLES TECLADO --
    [Header("TECLADO")]
    [SerializeField]
    private TMP_Text keyboardOutputField;

    // -- VARIABLES RECONOCIMIENTO --
    // Gesture Recognizer GO
    [SerializeField]
    private GestureRecognizer GR;
    [SerializeField]
    private TextManager _debugManager;
    [SerializeField]
    private TextMeshPro guideText;
}
