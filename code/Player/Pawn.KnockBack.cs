using Sandbox;

namespace SlapArena;

public partial class Pawn{
    public void KnockBack(Vector3 direction, float verticalMultiplier = 3f, int knockBackPower = 500){
        if(Game.IsClient) return;
        this.Velocity = new Vector3(direction.x, direction.y, -direction.z * verticalMultiplier) * knockBackPower;
    }
}