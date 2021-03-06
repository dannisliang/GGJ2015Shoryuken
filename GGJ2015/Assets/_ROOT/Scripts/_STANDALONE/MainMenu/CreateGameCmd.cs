﻿using UnityEngine;
using System.Collections;
using Kathulhu;
using UdpKit;

public class CreateGameCmd : EventCommand
{

    public override void Execute()
    {        
        base.Execute();
        
        BoltLauncher.StartServer( new UdpEndPoint( UdpIPv4Address.Any, ( ushort )27000 ) );

        if ( UIManager.Current == null )
            return;

        UIManager.Current.Panels["MainMenuPanel"].Deactivate();
        UIManager.Current.Panels["WaitingPanel"].Activate();
        
    }
}
