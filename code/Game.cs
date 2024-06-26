﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SlapArena.UI;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace SlapArena;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : GameManager
{
    public MyGame()
	{
        if(Game.IsServer){
            _ = new Hud();
        }

        if(Game.IsClient){
            _ = new InfoDisplay();
			_ = new SpawnMenu(new Vector3(0, 570, 1900), Rotation.FromAxis(Vector3.Up, 270));

        }

	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Pawn();
		client.Pawn = pawn;
        pawn.Respawn();
        pawn.DressFromClient(client);
		
		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}
	}

	public override void Simulate( IClient cl )
	{
        var plyr = cl.Pawn as Pawn;
        if(plyr.IsValid() && Input.Down("voice")){
            VoiceList.Current?.OnVoicePlayed(cl.SteamId, 0.5f);
        }

        base.Simulate(cl);
	}

    public override void OnVoicePlayed(IClient cl){
        cl.Voice.WantsStereo = false;

        base.OnVoicePlayed(cl);
    }

    [ClientRpc]
	public override void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
        Killfeed.current?.AddEntry(leftid, left, rightid, right, method);
	}
}
