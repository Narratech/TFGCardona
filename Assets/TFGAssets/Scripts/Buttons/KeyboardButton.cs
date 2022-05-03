using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardButton : VR_Button_Template
{
    private KeyCommand keyboardKey;

    public void setKeyboardKey(KeyCommand kk)
    {
        keyboardKey = kk;
        reconfigureButton();
    }

    public void reconfigureButton()
    {
        TextMeshPro buttonText = GetComponentInChildren(typeof(TextMeshPro)) as TextMeshPro;
        switch (keyboardKey)
        {
            case KeyCommand.A:
                buttonText.text = "A";
                break;
            case KeyCommand.B:
                buttonText.text = "B";
                break;
            case KeyCommand.C:
                buttonText.text = "C";
                break;
            case KeyCommand.D:
                buttonText.text = "D";
                break;
            case KeyCommand.E:
                buttonText.text = "E";
                break;
            case KeyCommand.F:
                buttonText.text = "F";
                break;
            case KeyCommand.G:
                buttonText.text = "G";
                break;
            case KeyCommand.H:
                buttonText.text = "H";
                break;
            case KeyCommand.I:
                buttonText.text = "I";
                break;
            case KeyCommand.J:
                buttonText.text = "J";
                break;
            case KeyCommand.K:
                buttonText.text = "K";
                break;
            case KeyCommand.L:
                buttonText.text = "L";
                break;
            case KeyCommand.M:
                buttonText.text = "M";
                break;
            case KeyCommand.N:
                buttonText.text = "N";
                break;
            case KeyCommand.Ñ:
                buttonText.text = "Ñ";
                break;
            case KeyCommand.O:
                buttonText.text = "O";
                break;
            case KeyCommand.P:
                buttonText.text = "P";
                break;
            case KeyCommand.Q:
                buttonText.text = "Q";
                break;
            case KeyCommand.R:
                buttonText.text = "R";
                break;
            case KeyCommand.S:
                buttonText.text = "S";
                break;
            case KeyCommand.T:
                buttonText.text = "T";
                break;
            case KeyCommand.U:
                buttonText.text = "U";
                break;
            case KeyCommand.V:
                buttonText.text = "V";
                break;
            case KeyCommand.W:
                buttonText.text = "W";
                break;
            case KeyCommand.X:
                buttonText.text = "X";
                break;
            case KeyCommand.Y:
                buttonText.text = "Y";
                break;
            case KeyCommand.Z:
                buttonText.text = "Z";
                break;
            case KeyCommand.ENTER:
                buttonText.text = "ENTER";
                break;
            case KeyCommand.BACKSPACE:
                buttonText.text = "BCKSPACE";
                break;
            case KeyCommand.BACK:
                buttonText.text = "BACK";
                break;
            case KeyCommand.CLEAR:
                buttonText.text = "LIMPIAR";
                break; ;
            default:
                buttonText.text = "Please Choose Key";
                break;
        }
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
                //LoginManager.Instance.ConnectToPhotonServer();
                KeyboardManager.Instance.OnEnterPressed();
                break;
            case KeyCommand.BACKSPACE:
                KeyboardManager.Instance.removeOneCharacter();
                return;
            case KeyCommand.BACK:
                KeyboardManager.Instance.OnBackPressed();
                //LoginManager.Instance.backToSelection();
                return;
            default:
                key = "invalid";
                break;
        }

        KeyboardManager.Instance.addCharacter(key);
    }
}
