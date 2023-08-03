using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace SlapArena.UI;

public partial class GloveEntry : Panel{
    public Label GloveName;

    public GloveEntry(){
        StyleSheet.Load("UI/InWorldUI/SpawnMenu/GloveUI/GloveEntry.scss");
        AddClass("entry");
        GloveName = Add.Label("GloveName", "name");
    }
}