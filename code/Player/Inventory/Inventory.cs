using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SlapArena;

public partial class Inventory : EntityComponent<Pawn>{
	[Net] public IList<Glove> AvailableGloves {get; set;}

	[Net, Predicted] public Glove ActiveGlove {get; set;}


	public void Simulate(IClient cl){
		ActiveGlove?.Simulate(cl);
	}

	// Call this when the player leaves the server.
	public virtual void Clean(){
		foreach(var glove in AvailableGloves.ToArray()){
			glove.Delete();
		}

		AvailableGloves.Clear();
	}

	public void SetActiveGlove(string gloveName){
		var glove = AvailableGloves.FirstOrDefault(x => x.GloveName == gloveName);

		if(glove == null){
			return;
		}

		ActiveGlove = glove;
        ActiveGlove?.OnEquip(Entity);
		Log.Info(ActiveGlove.GloveName);
	}

	public virtual void Add(Glove glove, bool makeActive=false){
		if(AvailableGloves.Contains(glove)){
			Log.Info("No Good!");
			return;
		}

		if(!glove.IsValid()){
			Log.Info("Glove not valid?");
			return;
		}

		AvailableGloves.Add(glove);

		if(makeActive){
			SetActiveGlove(glove.GloveName);
		}
	}
}
