using Sandbox;

namespace SlapArena;

public partial class PawnController{
    public int MaxDashes => 2;
    private int CurrentDashes {get; set;} = 2;
    private TimeSince timeSinceLastDash;
    private float dashCoolDownSec = 4.0f;
	public float dashDuration = .1f;
	public TimeSince timeSinceDashed;
	public bool isDashing;

    private void HandleDash(){
        if(CurrentDashes > 0){
			isDashing = true;
			Log.Info(isDashing);
			timeSinceDashed = 0;
			Entity.Velocity = 0;
            Entity.Velocity = ApplyDash(Entity.Velocity, "noclip");
            CurrentDashes -= 1;
        }
    }

    private void DashTick(){
		if(isDashing && timeSinceDashed > dashDuration){
			isDashing = false;
		}

        if(Game.IsClient){return;}

        if(CurrentDashes >= MaxDashes){timeSinceLastDash = 0; return;}
        if(timeSinceLastDash >= dashCoolDownSec){
            CurrentDashes += 1;
            timeSinceLastDash = 0;
            Log.Info("Dash refreshed");
        }
    }

    private Vector3 ApplyDash(Vector3 velocity, string eventName){
        AddEvent(eventName);

        var dir = Entity.InputDirection.Normal;
        dir = Entity.Transform.NormalToWorld(dir);
        return Entity.Velocity + dir * 700f;
    }
}
