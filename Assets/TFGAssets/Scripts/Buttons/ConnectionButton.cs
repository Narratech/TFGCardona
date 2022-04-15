using UnityEngine;

public class ConnectionButton : VR_Button_Template
{
    private connectionType tipoConexion;

    // Panels
    public GameObject ConnectOptionPanel;
    public GameObject ConnectWithNamePanel;

    // LoginManager
    public LoginManager loginManager;

    public override void OnClick()
    {
        // CREATE AND CALL A LOGIC METHOD
        if (tipoConexion == connectionType.ANONYMOUS)
        {
            loginManager.ConnectAnonymously();
            Debug.Log("TipoConexion: Anonima.");
        }
        else if (tipoConexion == connectionType.WITHNAME)
        {
            // to do
            Debug.Log("TipoConexion: Con Nombre.");
            ConnectOptionPanel.SetActive(false);
            ConnectWithNamePanel.SetActive(true);
        }
        else
            Debug.Log("TipoConexion: Invalida.");
    }
}
