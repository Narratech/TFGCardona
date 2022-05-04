using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardButton : VR_Button_Template
{
    private eKeyCommand keyboardKey;

    public void setKeyboardKey(eKeyCommand kk)
    {
        keyboardKey = kk;
        reconfigureButton();
    }

    public void reconfigureButton()
    {
        TextMeshPro buttonText = GetComponentInChildren(typeof(TextMeshPro)) as TextMeshPro;
        switch (keyboardKey)
        {
            case eKeyCommand.A:
                buttonText.text = "A";
                break;
            case eKeyCommand.B:
                buttonText.text = "B";
                break;
            case eKeyCommand.C:
                buttonText.text = "C";
                break;
            case eKeyCommand.D:
                buttonText.text = "D";
                break;
            case eKeyCommand.E:
                buttonText.text = "E";
                break;
            case eKeyCommand.F:
                buttonText.text = "F";
                break;
            case eKeyCommand.G:
                buttonText.text = "G";
                break;
            case eKeyCommand.H:
                buttonText.text = "H";
                break;
            case eKeyCommand.I:
                buttonText.text = "I";
                break;
            case eKeyCommand.J:
                buttonText.text = "J";
                break;
            case eKeyCommand.K:
                buttonText.text = "K";
                break;
            case eKeyCommand.L:
                buttonText.text = "L";
                break;
            case eKeyCommand.M:
                buttonText.text = "M";
                break;
            case eKeyCommand.N:
                buttonText.text = "N";
                break;
            case eKeyCommand.Ñ:
                buttonText.text = "Ñ";
                break;
            case eKeyCommand.O:
                buttonText.text = "O";
                break;
            case eKeyCommand.P:
                buttonText.text = "P";
                break;
            case eKeyCommand.Q:
                buttonText.text = "Q";
                break;
            case eKeyCommand.R:
                buttonText.text = "R";
                break;
            case eKeyCommand.S:
                buttonText.text = "S";
                break;
            case eKeyCommand.T:
                buttonText.text = "T";
                break;
            case eKeyCommand.U:
                buttonText.text = "U";
                break;
            case eKeyCommand.V:
                buttonText.text = "V";
                break;
            case eKeyCommand.W:
                buttonText.text = "W";
                break;
            case eKeyCommand.X:
                buttonText.text = "X";
                break;
            case eKeyCommand.Y:
                buttonText.text = "Y";
                break;
            case eKeyCommand.Z:
                buttonText.text = "Z";
                break;
            case eKeyCommand.ENTER:
                buttonText.text = "<size=25%>ENTER";
                break;
            case eKeyCommand.BACKSPACE:
                buttonText.text = "<size=18%>BORRAR";
                break;
            case eKeyCommand.BACK:
                buttonText.text = "<size=25%>SALIR";
                break;
            case eKeyCommand.CLEAR:
                buttonText.text = "<size=18%>LIMPIAR";
                break; ;
            default:
                buttonText.text = "<size=10%>Please Choose Key";
                break;
        }
    }

    public override void OnClick()
    { 
        string key = "";
        switch (keyboardKey)
        {
            case eKeyCommand.A:
                key = "A";
                break;
            case eKeyCommand.B:
                key = "B";
                break;
            case eKeyCommand.C:
                key = "C";
                break;
            case eKeyCommand.D:
                key = "D";
                break;
            case eKeyCommand.E:
                key = "E";
                break;
            case eKeyCommand.F:
                key = "F";
                break;
            case eKeyCommand.G:
                key = "G";
                break;
            case eKeyCommand.H:
                key = "H";
                break;
            case eKeyCommand.I:
                key = "I";
                break;
            case eKeyCommand.J:
                key = "J";
                break;
            case eKeyCommand.K:
                key = "K";
                break;
            case eKeyCommand.L:
                key = "L";
                break;
            case eKeyCommand.M:
                key = "M";
                break;
            case eKeyCommand.N:
                key = "N";
                break;
            case eKeyCommand.Ñ:
                key = "Ñ";
                break;
            case eKeyCommand.O:
                key = "O";
                break;
            case eKeyCommand.P:
                key = "P";
                break;
            case eKeyCommand.Q:
                key = "Q";
                break;
            case eKeyCommand.R:
                key = "R";
                break;
            case eKeyCommand.S:
                key = "S";
                break;
            case eKeyCommand.T:
                key = "T";
                break;
            case eKeyCommand.U:
                key = "U";
                break;
            case eKeyCommand.V:
                key = "V";
                break;
            case eKeyCommand.W:
                key = "W";
                break;
            case eKeyCommand.X:
                key = "X";
                break;
            case eKeyCommand.Y:
                key = "Y";
                break;
            case eKeyCommand.Z:
                key = "Z";
                break;
            case eKeyCommand.ENTER:
                //LoginManager.Instance.ConnectToPhotonServer();
                KeyboardManager.Instance.OnEnterPressed();
                break;
            case eKeyCommand.BACKSPACE:
                KeyboardManager.Instance.removeOneCharacter();
                return;
            case eKeyCommand.BACK:
                KeyboardManager.Instance.OnBackPressed();
                //LoginManager.Instance.backToSelection();
                return;
            case eKeyCommand.CLEAR:
                KeyboardManager.Instance.clearText();
                return;
            default:
                key = "invalid";
                break;
        }

        KeyboardManager.Instance.addCharacter(key);
    }
}
