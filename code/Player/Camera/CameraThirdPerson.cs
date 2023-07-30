using Sandbox;

namespace SlapArena;

public partial class CameraThirdPerson : EntityComponent<Pawn>{
    public void Simulate(){
        if(Entity is Pawn plyr){
            Camera.FirstPersonViewer = null;
            Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
            Camera.Rotation = plyr.ViewAngles.ToRotation();
            Camera.Position = GetCameraPosition();

            plyr.EyePosition = Camera.Position;
        }
    }

    public Vector3 GetCameraPosition(){
        if(Entity is not Pawn plyr) return Vector3.Zero;

        plyr.EyeRotation = plyr.ViewAngles.ToRotation();
        
        Vector3 target;
        var centerPos = plyr.Position + Vector3.Up * 64;
        var rot = plyr.ViewAngles.ToRotation() * Rotation.FromAxis(Vector3.Up, -16);
        float distance = 120.0f * plyr.Scale;

        target = centerPos + rot.Right * ((plyr.CollisionBounds.Mins.x + 70) * plyr.Scale);
        target += rot.Forward * -distance;

        var trace = Trace.Ray(centerPos, target)
                        .WithAnyTags("solid")
                        .Ignore(plyr)
                        .Radius(8)
                        .Run();

        return trace.EndPosition;
    }
}