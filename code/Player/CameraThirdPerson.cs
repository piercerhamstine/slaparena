using Sandbox;

namespace SlapArena;

public partial class CameraThirdPerson : EntityComponent<Pawn>{
    public void Tick(){
        if(Entity is Pawn plyr){
            Camera.FirstPersonViewer = null;
            Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
            Camera.Rotation = plyr.ViewAngles.ToRotation();
            Camera.Position = GetCameraPosition();

            plyr.EyePosition = Camera.Position;
            plyr.EyeRotation = GetEyeRotation();
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

    public Rotation GetEyeRotation(){
        if(Entity is not Pawn plyr) return Rotation.Identity;

        var camPos = Game.IsClient ? Camera.Position : GetCameraPosition();

        var rot = plyr.ViewAngles.ToRotation();
        var camTr = Trace.Ray(camPos + rot.Forward * 20, camPos + rot.Forward * 9999)
            .WithAnyTags("solid")
            .Ignore(plyr)
            .Radius(8)
            .Run();

        var posDiff = camTr.EndPosition - plyr.EyePosition;
        var newAng = Vector3.VectorAngle(posDiff);
        var newRot = newAng.ToRotation();

        // Cam
        DebugOverlay.Line(camPos, camTr.EndPosition, color: Color.Blue);

        // Eye
        DebugOverlay.Line(plyr.EyePosition, plyr.EyePosition + newRot.Forward * 9999, color: Color.Red);

        return newRot;
    }
}