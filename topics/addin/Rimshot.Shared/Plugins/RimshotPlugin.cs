using Autodesk.Navisworks.Api.Plugins;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Rimshot {
  [Plugin( Rimshot.Plugin, "Rimshot", DisplayName = "Rimshot", ToolTip = "Rimshot Issue Register" )]
  [DockPanePlugin( 400, 400, FixedSize = false )]
  internal class RimshotPlugin : DockPanePlugin {
    // Called to tell the plugin to create it's pane.
    // Should be overriden in conjunction with DestroyControlPane.
    public override Control CreateControlPane () {
      ElementHost elementHost = new ElementHost {
        AutoSize = true,
        Child = new RimshotPane( /*address: "https://rimshot.app/issues"*/ )
      };

      elementHost.CreateControl();

      return elementHost;
    }

    // Called to tell the plugin to destroy it's pane.
    // Should be overriden in conjunction with CreateControlPane.
    public override void DestroyControlPane ( Control pane ) => pane.Dispose();
  }
}
