using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.Q);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.A);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.Z); 
                            break;
                        case 1:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.W);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.S);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.X);
                            break;
                        case 2:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.E);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.D);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.C);
                            break;
                        case 3:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.R);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.F);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.V);
                            break;
                        case 4:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.T);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.G);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.B);
                            break;
                        case 5:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.Y);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.H);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.N);
                            break;
                        case 6:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.U);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.J);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.M); 
                            break;
                        case 7:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.I);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.K);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.CLEAR);
                            break;
                        case 8:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.O);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.L);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.BACKSPACE);
                            break;
                        case 9:
                            if (y == 0) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.P);
                            else if (y == 1) generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.Ñ);
                            else generatedButton.GetComponentInChildren<KeyboardButton>().setKeyboardKey(KeyCommand.BACK);
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
        Debug.Log("No behavior for back pressed");
    }
}
