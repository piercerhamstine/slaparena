using Sandbox;
using Sandbox.UI;

namespace SlapArena.UI;

public partial class GloveGrid : Panel{
    public GloveGrid(){
        AddChild(new GloveEntry());
        AddChild(new GloveEntry());
        AddChild(new GloveEntry());
        AddChild(new GloveEntry());
    }   
}