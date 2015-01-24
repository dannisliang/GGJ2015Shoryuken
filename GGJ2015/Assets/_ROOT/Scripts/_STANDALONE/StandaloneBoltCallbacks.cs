using UnityEngine;
using System.Collections;
using Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class StandaloneBoltCallbacks : GlobalEventListener {

    public override void BoltStarted()
    {
        base.BoltStarted();

        Debug.Log("Bolt:BoltStarted");
    }

    public override void Connected( BoltConnection connection )
    {
        base.Connected( connection );

        Debug.Log("Bolt:Connected");
    }

    public override void ConnectRequest( UdpKit.UdpEndPoint endpoint )
    {
        base.ConnectRequest( endpoint );

        Debug.Log( "Bolt:ConnectRequest" );
    }
}
