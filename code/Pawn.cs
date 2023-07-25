using Sandbox;
using System;
using System.ComponentModel;
using System.Linq;

namespace SlapArena;

public partial class Pawn : AnimatedEntity
{
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

    [Net, Predicted]
    public Glove ActiveGlove {get; set;}

    [Net, Predicted]
    public bool IsInvincible {get; set;}

    protected float SafeZoneLingerRate = 1.0f;
    [Net, Predicted] public TimeSince TimeSinceSafeZone {get; set;}

    [Browsable(false)]
    public Vector3 EyePosition{
        get => Transform.PointToWorld(EyeLocalPosition);
        set => EyeLocalPosition = Transform.PointToLocal(value);
    }

    [Net, Predicted, Browsable(false)]
    public Vector3 EyeLocalPosition {get; set;}

    [Browsable(false)]
    public Rotation EyeRotation{
        get => Transform.RotationToWorld(EyeLocalRotation);
        set => EyeLocalRotation = Transform.RotationToLocal(value);
    }

    [Net, Predicted, Browsable(false)]
    public Rotation EyeLocalRotation {get; set;}

    public BBox Hull{
        get => new
        (
            new Vector3(-16, -16, 0),
            new Vector3(16, 16, 64)
        );
    }

	public override Ray AimRay => new Ray(EyePosition, EyeRotation.Forward);

    [BindComponent] public PawnController Controller {get;}
    [BindComponent] public PawnAnimator Animator{get;}

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
        base.Spawn();
		SetModel( "models/citizen/citizen.vmdl" );

        LifeState = LifeState.Alive;
        Health = 100;

        Components.Create<PawnController>();
        Components.Create<PawnAnimator>();

        SetActiveWeapon(new BaseGlove());

        CreateHull();
        Tags.Add("player");
        EnableAllCollisions = true;
        EnableDrawing = true;
        EnableHitboxes = true;
        EnableTouch = true;
        EnableLagCompensation = true;
        Predictable = true;
	}

    public void DressFromClient(IClient cl){
        var clothes = new ClothingContainer();
        clothes.LoadFromClient(cl);
        clothes.DressEntity(this);
    }

    public void SetActiveWeapon(Glove glove){
        ActiveGlove = glove;
        ActiveGlove.OnEquip(this);
    }

	public override void OnKilled()
	{
        Respawn();

        var spawns = Entity.All.OfType<SpawnPoint>();

        var rndSpawn = spawns.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

        if(rndSpawn != null){
            var trans = rndSpawn.Transform;

            trans.Position = trans.Position + Vector3.Up * 50.0f;
            Transform = trans;
        }
	}
	
    public void Respawn(){
        LifeState = LifeState.Alive;
        Health = 100;

        Components.Create<PawnController>();
        Components.Create<PawnAnimator>();

        SetActiveWeapon(new BaseGlove());

        CreateHull();
        Tags.Add("player");

        EnableAllCollisions = true;
        EnableDrawing = true;
        EnableHitboxes = true;
        EnableTouch = true;
        EnableLagCompensation = true;
        Predictable = true;
    }

    public void InSafeZone(bool inSafeZone, bool justLeft = true){
        if(inSafeZone){
            IsInvincible = true;
            return;
        }

        IsInvincible = false;
    }

    // Use this later for lingering safezone
    private void HandleSafeZone(){   
    }

    private void CreateHull(){
        SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, Hull.Mins, Hull.Maxs);

        EnableHitboxes = true;
    }

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

        if(Input.StopProcessing)
            return;

		var look = Input.AnalogLook;

        if(ViewAngles.pitch > 90f || ViewAngles.pitch < -90f){
            look = look.WithYaw(look.yaw * -1f);
        }

		var viewAngles = ViewAngles;
		viewAngles += look;
        viewAngles.pitch = viewAngles.pitch.Clamp(-89f, 89f);
        viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( IClient cl )
	{
        HandleSafeZone();
        ApplyRotations();
        Controller?.Simulate(cl);
        Animator?.Simulate();
        ActiveGlove?.Simulate(cl);
        EyeLocalPosition = Vector3.Up * (64 * Scale);
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
        ApplyRotations();

		Camera.Rotation = ViewAngles.ToRotation();;
		// Set field of view to whatever the user chose in options
        Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.Position = Position;

        Vector3 targetPos;
        var pos = Position + Vector3.Up * 64;
        var rot = Camera.Rotation * Rotation.FromAxis(Vector3.Up, -24);

        float dist = 120.0f * Scale;
        targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 50) * Scale);
        targetPos += rot.Forward * -dist;
        
        var trace = Trace.Ray(pos, targetPos).WithAnyTags("solid")
                    .Ignore(this)
                    .Radius(8)
                    .Run();

        Camera.FirstPersonViewer = null;
        Camera.Position = trace.EndPosition;
	}

    public TraceResult TraceBBox(Vector3 start, Vector3 end, float liftFeet = 0.0f){
        return TraceBBox(start, end, Hull.Mins, Hull.Maxs, liftFeet);
    }
    public TraceResult TraceBBox(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f){
        if(liftFeet > 0){
            start += Vector3.Up * liftFeet;
            maxs = maxs.WithZ(maxs.z - liftFeet);
        }

        var trace = Trace.Ray(start, end).Size(mins, maxs)
                        .WithAnyTags("solid", "playerclip", "passbullets")
                        .Ignore(this)
                        .Run();

        return trace;
    }

    private void ApplyRotations(){
        EyeRotation = ViewAngles.ToRotation();
        Rotation = ViewAngles.WithPitch(0f).ToRotation();
    }
}
