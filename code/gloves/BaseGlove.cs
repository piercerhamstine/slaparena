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

        // I am sure there is a much cleaner way of doing this
        // But it works for now.
        var pos = Owner.Position + Vector3.Up * 64f;
        var camTrace = Trace.Ray(ray.Position, ray.Position + forward*195f)
                        .Ignore(Owner)
                        .WithAnyTags("solid")
                        .Run();

        var hits = Melee(pos, camTrace.EndPosition);
        foreach(var h in hits){
            if(h.Entity != null){
                PlaySound("rust_pistol.shoot");
                var plyr = h.Entity as Pawn;
                if(!plyr.IsInvincible){
                    using (Prediction.Off()){
                        var dmgInfo = new DamageInfo();
                        dmgInfo.Attacker = Owner;
                        dmgInfo.WithWeapon(this);

                        plyr.lastDamage = dmgInfo;
                        plyr.KnockBack(forward);
                    }
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