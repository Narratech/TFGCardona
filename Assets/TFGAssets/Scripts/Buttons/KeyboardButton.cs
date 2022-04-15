using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardButton : VR_Button_Template
{
    [SerializeField]
    private LoginManager loginManager;

    [SerializeField]
    //private TMP_InputField outputField;
    private TMP_Text outputField;

    [SerializeField]
    private KeyCommand keyboardKey;

    void Start()
    {
        outputField.text = "";
    }

    public override void OnClick()
    { 
        string key = "";
        switch (keyboardKey)
        {
            case KeyCommand.A:
                key = "A";
                break;
            case KeyCommand.B:
                key = "B";
                break;
            case KeyCommand.C:
                key = "C";
                break;
            case KeyCommand.D:
                key = "D";
                break;
            case KeyCommand.E:
                key = "E";
                break;
            case KeyCommand.F:
                key = "F";
                break;
            case KeyCommand.G:
                key = "G";
                break;
            case KeyCommand.H:
                key = "H";
                break;
            case KeyCommand.I:
                key = "I";
                break;
            case KeyCommand.J:
                key = "J";
                break;
            case KeyCommand.K:
                key = "K";
                break;
            case KeyCommand.L:
                key = "L";
                break;
            case KeyCommand.M:
                key = "M";
                break;
            case KeyCommand.N:
                key = "N";
                break;
            case KeyCommand.Ñ:
                key = "Ñ";
                break;
            case KeyCommand.O:
                key = "O";
                break;
            case KeyCommand.P:
                key = "P";
                break;
            case KeyCommand.Q:
                key = "Q";
                break;
            case KeyCommand.R:
                key = "R";
                break;
            case KeyCommand.S:
                key = "S";
                break;
            case KeyCommand.T:
                key = "T";
                break;
            case KeyCommand.U:
                key = "U";
                break;
            case KeyCommand.V:
                key = "V";
                break;
            case KeyCommand.W:
                key = "W";
                break;
            case KeyCommand.X:
                key = "X";
                break;
            case KeyCommand.Y:
                key = "Y";
                break;
            case KeyCommand.Z:
                key = "Z";
                break;
            case KeyCommand.ENTER:
                loginManager.ConnectToPhotonServer();
                break;
            case KeyCommand.BACKSPACE:
                string textEdited = "";
                // Eliminamos el último caracter
                if (outputField.text.Length > 1)
                {
                    textEdited = outputField.text.Remove(outputField.text.Length - 1);
                }
                else
                {
                    textEdited = "";
                }
                outputField.text = textEdited;
                return;
            case KeyCommand.BACK:
                loginManager.backToSelection();
                return;
            default:
                key = "invalid";
                break;
        }

        outputField.text = outputField.text + key;
    }
}
