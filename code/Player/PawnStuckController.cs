using Sandbox;
using System;

namespace SlapArena;

public partial class PawnStuckController : EntityComponent<Pawn>{

    internal Vector3 pawnLastPos;

    public void Simulate(IClient cl){
        if(Game.IsServer){
            TryUnstuck();
        }
    }

    public void TryUnstuck(){

        var result = Entity.TraceBBox(Entity.Position, Entity.Position);

        if(!result.StartedSolid){
            pawnLastPos = Entity.Position;
            return;
        }

        if(result.StartedSolid){
            if(pawnLastPos != Entity.Position){
                return;
            }

            var pos = Entity.Position + Vector3.Up * 5;

            Entity.Position = pos;
        }
    }
}