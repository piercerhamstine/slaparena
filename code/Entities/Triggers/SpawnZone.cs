using Sandbox;
using Editor;

namespace SlapArena;

[HammerEntity]
public partial class SpawnZone : BaseTrigger{
    public override void Spawn(){
        base.Spawn();

        EnableTouchPersists = true;
    }

	public override void Touch( Entity other )
	{
        if(!Game.IsServer) return;

        if(other is not Pawn plyr) return;

        plyr.InSafeZone(true);
	}

	public override void OnTouchEnd( Entity toucher )
	{
		base.OnTouchEnd( toucher );

        if(!Game.IsServer) return;

        if(toucher is not Pawn plyr) return;

        plyr.InSafeZone(false);
	}
}