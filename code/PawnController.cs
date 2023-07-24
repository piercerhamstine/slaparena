using Sandbox;
using System;
using System.Collections.Generic;

namespace SlapArena;

public class PawnController : EntityComponent<Pawn>
{
    public int GroundAngle => 45;
    public int StepSize => 24;

    public float Gravity => 800f;

    bool Grounded => Entity.GroundEntity.IsValid();

    HashSet<string> ControllerEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public void Simulate(IClient cl){
        ControllerEvents.Clear();

		// build movement from the input values
		var movement = Entity.InputDirection.Normal;
        var angles = Entity.ViewAngles.WithPitch(0);
        var moveVec = Rotation.From(angles) * movement * 320f;
        var groundEntity = CheckGrounded();

        if(groundEntity.IsValid()){
            if(!Grounded){
                Entity.Velocity = Entity.Velocity.WithZ(0);
                AddEvent("grounded");
            }

            Entity.Velocity = Accelerate(Entity.Velocity, moveVec.Normal, moveVec.Length, 200.0f * (Input.Down("run") ? 2.5f : 1f), 7.5f);            
            Entity.Velocity = ApplyFriction(Entity.Velocity, 4.0f);
        }
        else{
            Entity.Velocity = Accelerate(Entity.Velocity, moveVec.Normal, moveVec.Length, 100f, 20f);
            Entity.Velocity += Vector3.Down * Gravity * Time.Delta;
        }

        var moveHelper = new MoveHelper(Entity.Position, Entity.Velocity);
        moveHelper.Trace = moveHelper.Trace.Size(Entity.Hull).Ignore(Entity);

        if(moveHelper.TryMoveWithStep(Time.Delta, StepSize) > 0){
            if(Grounded){
                moveHelper.Position = StayGrounded(moveHelper.Position);
            }
            Entity.Position = moveHelper.Position;
            Entity.Velocity = moveHelper.Velocity;
        }

        Entity.GroundEntity = groundEntity;
    }

    private Vector3 Accelerate(Vector3 input, Vector3 wantedDir, float wantedSpeed, float maxSpeed, float acceleration){
        if ( maxSpeed > 0 && wantedSpeed > maxSpeed){
            wantedSpeed = maxSpeed;
        }

        var currSpeed = input.Dot(wantedDir);
        var addSpeed = wantedSpeed - currSpeed;

        if(addSpeed <= 0){
            return input;
        }

        var accelSpeed = acceleration * Time.Delta * wantedSpeed;
        if(accelSpeed > addSpeed){
            accelSpeed = addSpeed;
        }

        input += wantedDir * accelSpeed;

        return input;
    }

    public Vector3 ApplyFriction(Vector3 input, float frictionAmount){
        float stopSpeed = 100.0f;

        var speed = input.Length;
        if(speed < 0.1f) return input;

        float control = (speed < stopSpeed) ? stopSpeed : speed;

        var drop = control * Time.Delta * frictionAmount;

        float newSpeed = speed - drop;
        if(newSpeed < 0) newSpeed = 0;
        if(newSpeed == speed) return input;

        newSpeed /= speed;
        input *= newSpeed;
        
        return input;
    }

    private Vector3 StayGrounded(Vector3 pos){
        var start = pos + Vector3.Up * 2;
        var end = pos + Vector3.Down * StepSize;

        var trace = Entity.TraceBBox(pos, start);
        start = trace.EndPosition;

        trace = Entity.TraceBBox(start, end);

        if(trace.Fraction <= 0) return pos;
        if(trace.Fraction >= 1) return pos;
        if(trace.StartedSolid) return pos;
        if(Vector3.GetAngle(Vector3.Up, trace.Normal) > GroundAngle) return pos;

        return trace.EndPosition;
    }

    Entity CheckGrounded(){
        if(Entity.Velocity.z > 100f){
            return null;
        }

        var trace = Entity.TraceBBox(Entity.Position, Entity.Position + Vector3.Down, 2f);

        if(!trace.Hit){
            return null;
        }

        if(trace.Normal.Angle(Vector3.Up) > GroundAngle){
            return null;
        }

        return trace.Entity;
    }

    public bool HasEvent(string eventName){
        return ControllerEvents.Contains(eventName);
    }

    public void AddEvent(string eventName){
        if(HasEvent(eventName)){
            return;
        }

        ControllerEvents.Add(eventName);
    }
}