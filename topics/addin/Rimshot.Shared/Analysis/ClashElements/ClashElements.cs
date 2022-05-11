using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;

namespace Rimshot.Analysis {

  [DockPanePlugin(
        410, 450, FixedSize = false,
        AutoScroll = true,
        MinimumHeight = 250,
        MinimumWidth = 250
    ),

     Plugin( Tools.ClashElements.Plugin, "Rimshot",
        DisplayName = "Clashing Elements",
        ExtendedToolTip = "",
        Options = PluginOptions.None,
        SupportsIsSelfEnabled = false,
        ToolTip = "" )]

  public class ClashElementsPlugin : DockPanePlugin {

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Control CreateControlPane() {

      //create an ElementHost
      ElementHost eh = new ElementHost {
        AutoSize = true,
        Child = new ClashElements()
      };
      eh.CreateControl();


      //return the ElementHost
      return eh;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pane"></param>
    public override void DestroyControlPane( Control pane ) {
      if (pane is UserControl control) {
        control.Dispose();
      }
    }
  }
}

