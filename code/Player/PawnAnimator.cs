using Sandbox;
using System;

namespace SlapArena;

public class PawnAnimator : EntityComponent<Pawn>, ISingletonComponent
{
	public void Simulate()
	{
        var helper = new CitizenAnimationHelper(Entity);
        helper.WithVelocity(Entity.Velocity);
        helper.WithLookAt(Entity.EyePosition + Entity.EyeRotation.Forward * 100);
        helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
        helper.IsGrounded = Entity.GroundEntity.IsValid();

        if(Entity.Controller.HasEvent("jump")){
            helper.TriggerJump();
        }
        if(Entity.Controller.HasEvent("dash")){
            helper.IsNoclipping = true;
        }
	}
}