using System.Windows.Forms.Integration;
using System.Windows.Forms;
using Autodesk.Navisworks.Api.Plugins;

namespace Rimshot.BCF {
  [DockPanePluginAttribute(
      410, 450, FixedSize = false,
      AutoScroll = true,
      MinimumHeight = 410,
      MinimumWidth = 250
  ),

   PluginAttribute( Tools.BCFExport.Plugin, "Rimshot",
      DisplayName = "Export BCF...",
      ExtendedToolTip = "",
      Options = PluginOptions.None,
      SupportsIsSelfEnabled = false,
      ToolTip = "" )]

  public class ExportBCF : Autodesk.Navisworks.Api.Plugins.DockPanePlugin {
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override System.Windows.Forms.Control CreateControlPane() {
      //create an ElementHost
      ElementHost eh = new ElementHost {
        AutoSize = true,
        Child = new ExportBCFWin()
      };
      eh.CreateControl();

      //return the ElementHost
      return eh;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pane"></param>
    public override void DestroyControlPane( System.Windows.Forms.Control pane ) {
      if (pane is UserControl control) {
        control.Dispose();
      }
    }
  }
}