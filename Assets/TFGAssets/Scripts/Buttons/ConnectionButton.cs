using UnityEngine;

public class ConnectionButton : VR_Button_Template
{
    [SerializeField]
    private eConnectionType tipoConexion;

    // Panels
    public GameObject ConnectOptionPanel;
    public GameObject ConnectWithNamePanel;

    // LoginManager
    public LoginManager loginManager;

    public override void OnClick()
    {
        // CREATE AND CALL A LOGIC METHOD
        if (tipoConexion == eConnectionType.ANONYMOUS)
        {
            loginManager.ConnectAnonymously();
            Debug.Log("TipoConexion: Anonima.");
        }
        else if (tipoConexion == eConnectionType.WITHNAME)
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
