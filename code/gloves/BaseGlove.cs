using Sandbox;
using System;
using System.Collections.Generic;

namespace SlapArena;

public partial class BaseGlove : Glove{
	public override string ModelPath => "models/glove_r.vmdl";

    protected virtual void AttackEffect(){
        Pawn.SetAnimParameter("holdtype_attack", (int)CitizenAnimationHelper.Hand.Right);
        Pawn.SetAnimParameter("b_attack", true);
    }

    public override void PrimaryAttack(){
        AttackEffect();
        Attack();
    }

    private void Attack(){
        var ray = Owner.AimRay;
        var forward = ray.Forward;
        forward = forward.Normal;

        var ply = Owner as Pawn;
        var diff = ply.Position + Vector3.Up * 64;
        var sub = ply.EyePosition - diff;
        var newAng = Vector3.VectorAngle(sub);
        var newRot = newAng.ToRotation();
        var f = newRot.Forward.Normal;
        DebugOverlay.Line(ply.EyePosition, ray.Position+f * -999f, Color.Red, 3, true);
        var hits = Melee(ray.Position, ray.Position+forward * 100f, 10f);
        
        Log.Info("fatd");
        foreach(var h in hits){
            if(h.Entity != null){
                PlaySound("rust_pistol.shoot");
                var plyr = h.Entity as Pawn;
                if(!plyr.IsInvincible){
                    plyr.KnockBack(forward);
                }
            }
        }
    }

    public IEnumerable<TraceResult> Melee(Vector3 start, Vector3 end, float radius = 20.0f){
        var trace = Trace.Ray(start, end)
                        .UseHitboxes()
                        .WithAnyTags("player", "npc")
                        .Ignore(this);
        var tr = trace.Run();

        if(tr.Hit){
            yield return tr;
        }
        else{
            trace = trace.Size(radius);
            tr = trace.Run();

            if(tr.Hit){
                yield return tr;
            }
        }
    }

    protected override void Animate(){
        Pawn.SetAnimParameter("holdtype", (int)CitizenAnimationHelper.HoldTypes.Punch);
    }
}