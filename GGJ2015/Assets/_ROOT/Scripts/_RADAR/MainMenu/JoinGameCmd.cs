using UnityEngine;
using System.Collections;
using Kathulhu;

public class JoinGameCmd : EventCommand {

    public string ip = "127.0.0.1";
    public override void Execute()
    {
        base.Execute();
        Debug.Log("JoinGameCMD.Execute");
        BoltLauncher.StartClient();
        BoltNetwork.Connect( UdpKit.UdpEndPoint.Parse( ip + ":27000" ) );
    }
}