using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class KeyboardManager : MonoBehaviour
{
    [SerializeField]
    public TMP_Text outputField;
    [SerializeField]
    GameObject keybuttonPrefab;
    [SerializeField]
    Vector3 rotateKeyboard;

    // CLASE COMO SINGLETON
    public static KeyboardManager Instance;

    private bool instantiateKeyboard;

    private void Awake()
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
        instantiateKeyboard = true;

        if (instantiateKeyboard)
        {
            // Colocar a 0.2 de distancia uno de otro
            //    x0 x1 x2 x3 x4 x5 x6  x7    x8      x9
            // y0 Q  W  E  R  T  Y  U   I     O       P
            // y1 A  S  D  F  G  H  J   K     L       Ñ
            // y2 Z  X  C  V  B  N  M CLEAR BCKSPC BACK(EXIT)

            // QWERTYUIOP
            GameObject generatedButton;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    generatedButton = Instantiate(keybuttonPrefab, new Vector3(gameObject.transform.position.x + 0.11f * gameObject.transform.localScale.x * x, gameObject.transform.position.y - 0.11f * gameObject.transform.localScale.y * y, gameObject.transform.position.z), new Quaternion(Quaternion.identity.x, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w));
                    generatedButton.transform.parent = gameObject.transform;
                    generatedButton.transform.localScale = new Vector3(
                        gameObject.transform.localScale.x * generatedButton.transform.localScale.x,
                        gameObject.transform.localScale.y * generatedButton.transform.localScale.y,
                        gameObject.transform.localScale.z * generatedButton.transform.localScale.z
                        );
                    generatedButton.transform.Rotate(new Vector3(0, 90, 0));

                    switch (x)
                    {
                        case 0:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.Q);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.A);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.Z); 
                            break;
                        case 1:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.W);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.S);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.X);
                            break;
                        case 2:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.E);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.D);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.C);
                            break;
                        case 3:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.R);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.F);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.V);
                            break;
                        case 4:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.T);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.G);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.B);
                            break;
                        case 5:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.Y);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.H);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.N);
                            break;
                        case 6:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.U);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.J);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.M); 
                            break;
                        case 7:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.I);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.K);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.CLEAR);
                            break;
                        case 8:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.O);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.L);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.BACKSPACE);
                            break;
                        case 9:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.P);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.Ñ);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(eKeyCommand.BACK);
                            break;
                    }
                }
            } // Key generation loop (for)

            gameObject.transform.Rotate(rotateKeyboard);
        }
    }

    public TMP_Text getOutputText()
    {
        return outputField;
    }

    public void removeOneCharacter()
    {
        string textEdited = "";
        if (outputField.text.Length > 1)
        {
            textEdited = outputField.text.Remove(outputField.text.Length - 1);
        }
        else
        {
            textEdited = "";
        }
        outputField.text = textEdited;
    }

    public void clearText() 
    {
        outputField.text = "";
    }

    public void addCharacter(string key)
    {
        outputField.text = outputField.text + key;
    }

    public void OnEnterPressed()
    {
        Debug.Log("No behavior for Enter pressed");
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("Login_Scene");
    }
}
