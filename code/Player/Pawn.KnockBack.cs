using Sandbox;

namespace SlapArena;

public partial class Pawn{
    public void KnockBack(Vector3 direction, float verticalMultiplier = 1.5f, int knockBackPower = 600){
        if(Game.IsClient) return;
		Log.Info(-direction.z);
        this.Velocity = new Vector3(direction.x, direction.y, 0.5f) * knockBackPower;
    }
}
