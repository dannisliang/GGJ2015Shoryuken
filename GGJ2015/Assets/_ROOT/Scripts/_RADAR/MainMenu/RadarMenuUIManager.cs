using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RadarMenuUIManager : MonoBehaviour {

    public InputField inputField;//set in the inspector

    private JoinGameCmd joinGame = new JoinGameCmd();    

    public void JoinGame( )
    {
        joinGame.ip = inputField.text;
        joinGame.Execute();

    }
}
