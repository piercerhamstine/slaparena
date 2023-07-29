using Sandbox.UI;

namespace SlapArena.UI;

public class Crosshair : Panel{
    Panel center;

    public Crosshair(){
        StyleSheet.Load("/UI/InGameUI/hud/Crosshair.scss");
        center = Add.Panel("center");
    }

    public override void Tick(){
        center.Style.Dirty();
    }
}