using Sandbox.UI;

namespace SlapArena.UI;

public class Crosshair : Panel{
    public static Crosshair curr;
    public Crosshair(){
        curr = this;
        StyleSheet.Load("/UI/InGameUI/hud/Crosshair.scss");
    }
}