using Sandbox;
using Sandbox.UI;

namespace SlapArena.UI;

public partial class Hud : HudEntity<RootPanel>
{
    public Hud(){
        if(!Game.IsClient){
            return;
        }

        RootPanel.StyleSheet.Load("/UI/InGameUI/Hud/Hud.scss");

        RootPanel.AddChild<Crosshair>();
    }
}