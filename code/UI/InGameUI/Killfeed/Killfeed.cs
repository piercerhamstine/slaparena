using Sandbox;
using Sandbox.UI;

public partial class Killfeed : Panel{
    public static Killfeed current;

    public Killfeed(){
        current = this;

        StyleSheet.Load("/UI/InGameUI/Killfeed/Killfeed.scss");
    }

    public virtual Panel AddEntry(long leftSteamId, string left, long rightSteamId, string right, string method){
        var e = current.AddChild<KillfeedEntry>();
        e.Left.Text = left;
        Log.Info($"{Game.LocalClient?.SteamId} | {leftSteamId}");
        e.Left.SetClass("me", leftSteamId == (Game.LocalClient?.SteamId));
        e.Method.Text = method;
        e.Right.Text = right;
        e.Right.SetClass("me", rightSteamId == (Game.LocalClient?.SteamId));
        return e;
    }
}