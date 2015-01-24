using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RadarMenuUIManager : MonoBehaviour {

    public InputField inputField;//set in the inspector


    public void JoinGame( )
    {
        BoltConsole.Write( "[Trying to join game]" );

        BoltLauncher.StartClient();
        BoltNetwork.Connect( UdpKit.UdpEndPoint.Parse( inputField.text + ":27000" ) );

    }
}
