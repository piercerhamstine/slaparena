using Sandbox;
using System;
using System.Net;

namespace SlapArena;

public partial class MyGame{
	[ConCmd.Server("sa_reset")]
	public static void Reset(){
		var owner = ConsoleSystem.Caller.Pawn as Pawn;
		owner.OnKilled();
		Log.Info(owner);
	}
}
