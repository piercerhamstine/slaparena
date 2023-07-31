﻿using Sandbox;
using System;
using System.ComponentModel;
using System.Linq;

namespace SlapArena;

public partial class Pawn : AnimatedEntity
{
    [Net]
    public string SteamNickname {get; private set;}

    public DamageInfo lastDamage;
    private TimeSince timeSinceLastDmg;
    private float timeToClearDmgInfo = 4.0f;

	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

    [Net, Predicted]
    public Glove ActiveGlove {get; set;}

    [Net, Predicted]
    public bool IsInvincible {get; set;}

    [Net, Predicted] public TimeSince TimeSinceSafeZone {get; set;}

    [Browsable(false)]
    public Vector3 EyePosition{
        get => EyeLocalPosition;
        set => EyeLocalPosition = value;
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
    [BindComponent] public PawnStuckController StuckController {get;}
    [BindComponent] public CameraThirdPerson ThirdPersonCam {get;}
    
	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
        base.Spawn();
		SetModel("models/citizen/citizen.vmdl");

        LifeState = LifeState.Alive;
        Health = 100;

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
        SteamNickname = cl.Name;
        var clothes = new ClothingContainer();
        clothes.LoadFromClient(cl);
        clothes.DressEntity(this);
    }

	public override void ClientSpawn()
	{
        _ = new UI.Nameplate(this, new Vector3(-8,0,65));
	}

    public void SetActiveWeapon(Glove glove){
        ActiveGlove = glove;
        ActiveGlove.OnEquip(this);
    }

	public override void OnKilled()
	{
        if(Game.IsClient){return;}

        // Remove velocity;
        Velocity = Vector3.Zero;
        LifeState = LifeState.Dead;
        Log.Info(lastDamage);
        if(lastDamage.Attacker == null){
            MyGame.Current.OnKilledMessage(this.Client.SteamId, this.Client.Name, 0, "", "Commited Suicide");
        }
        else{
            lastDamage.Attacker.Client.AddInt("kills", 1);
            MyGame.Current.OnKilledMessage(lastDamage.Attacker.Client.SteamId, lastDamage.Attacker.Client.Name, this.Client.SteamId, this.Client.Name, "Spanked");
        }

        Client?.AddInt("deaths", 1);

        if(Game.IsServer){
            Respawn();

            var spawns = Entity.All.OfType<SpawnPoint>();
            var rndSpawn = spawns.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if(rndSpawn != null){
                var trans = rndSpawn.Transform;

                trans.Position = trans.Position + Vector3.Up * 50.0f;
                Transform = trans;
            }
        }
	}

    public void Respawn(){
        LifeState = LifeState.Alive;
        Health = 100;

        SetActiveWeapon(new BaseGlove());

        Components.Create<PawnController>();
        Components.Create<PawnAnimator>();
        Components.Create<PawnStuckController>();
        Components.Create<CameraThirdPerson>();

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

    private void UpdateDmgInfo(){
        if(lastDamage.Attacker == null){
            timeSinceLastDmg = 0;
            return;
        }

        if(timeSinceLastDmg > timeToClearDmgInfo){
            timeSinceLastDmg = 0;
            lastDamage = new DamageInfo();
        }
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
        if(LifeState != LifeState.Alive){return;}

        ApplyRotations();
        UpdateDmgInfo();

        // Components
        StuckController?.Simulate(cl);
        Controller?.Simulate(cl);
        Animator?.Simulate();
        ActiveGlove?.Simulate(cl);
        ThirdPersonCam?.Simulate();
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
        ThirdPersonCam?.Simulate();
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
