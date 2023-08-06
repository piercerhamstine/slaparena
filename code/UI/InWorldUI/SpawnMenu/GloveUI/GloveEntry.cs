using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using SlapArena;

namespace SlapArena.UI;

public partial class GloveEntry : Panel{
    public Label GloveName;

    public GloveEntry(string gloveName){
        StyleSheet.Load("UI/InWorldUI/SpawnMenu/GloveUI/GloveEntry.scss");
        AddClass("entry");
        GloveName = Add.Label(gloveName, "name");
    }
    
    public void OnClick(Entity src, string name){
        var plyr = src as Pawn;
    }
}
