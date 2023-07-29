using Sandbox;
using System.Collections.Generic;

namespace SlapArena;

public partial class Glove : ModelEntity
{
    public Pawn Pawn => Owner as Pawn;

    public virtual string ModelPath => null;

    /// <summary>
    /// How often the glove can attack
    /// </summary>
    public virtual float RateOfFire => 0.5f;

    [Net, Predicted] public TimeSince TimeSincePrimaryAttack {get; set;}
    
    public override void Spawn(){
        Predictable = true;
        EnableDrawing = true;

        if(ModelPath != null){
            SetModel(ModelPath);
        }
    }

	/// <summary>
	/// Called when <see cref="Pawn.SetActiveWeapon(Weapon)"/> is called for this weapon.
	/// </summary>
	/// <param name="pawn"></param>
    public void OnEquip(Pawn pawn){
        Owner = pawn;
        SetParent(pawn, true);
        //EnableDrawing = true;
    }

    public void OnHolster(){
        EnableDrawing = false;
    }

	public override void Simulate( IClient cl )
	{
        Animate();

        if(CanPrimaryAttack()){
            using( LagCompensation() )
            {
                TimeSincePrimaryAttack = 0;
                PrimaryAttack();
    
            }
        }
	}

   public virtual bool CanPrimaryAttack(){
        if(!Owner.IsValid() || !Input.Down("attack1")){
            return false;
        }

        var rate = RateOfFire;
        if(rate <= 0){
            return true;
        }

        return TimeSincePrimaryAttack > (1.0f / rate);
    }	

    public virtual void PrimaryAttack(){}

    protected virtual void Animate(){
        
    }
}
