﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Bolt;

[BoltGlobalBehaviour( BoltNetworkModes.Server )]
public class StandaloneBoltCallbacks : GlobalEventListener
{

    public override void BoltStarted()
    {
        base.BoltStarted();

        Debug.Log( "Bolt:BoltStarted" );
    }

    public override void Connected( BoltConnection connection )
    {
        base.Connected( connection );

        logMessages.Insert( 0, string.Format( "{0} connected", connection.RemoteEndPoint ) );
    }

    List<string> logMessages = new List<string>();

    //void OnGUI()
    //{
    //    // only display max the 5 latest log messages
    //    int maxMessages = Mathf.Min( 5, logMessages.Count );

    //    GUILayout.BeginArea( new Rect( Screen.width / 2 - 200, Screen.height - 100, 400, 100 ), GUI.skin.box );

    //    for ( int i = 0; i < maxMessages; ++i )
    //    {
    //        GUILayout.Label( logMessages[i] );
    //    }

    //    GUILayout.EndArea();
    //}


    //"Listen to event" example
    //public override void OnEvent( ShowDebugMessage evnt )
    //{
    //    logMessages.Insert( 0, evnt.message );
    //}

    //"Raise event" example
    //var log = ShowDebugMessage.Create();
    //log.message = string.Format( "{0} connected", connection.RemoteEndPoint );
    //log.Send();
}
