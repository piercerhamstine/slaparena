using Sandbox;
using System;

namespace SlapArena;

public class PawnStuckController{
	public PawnController Controller;

	private int _stuckTries = 0;

	public PawnStuckController( PawnController controller )
	{
		Controller = controller;
	}

	public virtual bool TestAndFix()
	{
		var result = Controller.Entity.TraceBBox( Controller.player.Position, Controller.player.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			_stuckTries = 0;
			return false;
		}

		//
		// Client can't jiggle its way out, needs to wait for
		// server correction to come
		//
		if ( Game.IsClient )
			return true;

		var AttemptsPerTick = 20;

		for ( var i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Controller.player.Position + Vector3.Random.Normal * (((float)_stuckTries) / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
				pos = Controller.player.Position + Vector3.Up * 5;

			result = Controller.Entity.TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				Controller.player.Position = pos;
				return false;
			}
		}

		_stuckTries++;

		return true;
	}
}
