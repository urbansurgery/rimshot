using Autodesk.Navisworks.Api.Plugins;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Rimshot {
  [Plugin( Speckle.Plugin, "Rimshot", DisplayName = "Speckle", ToolTip = "Speckle" )]
  [DockPanePlugin( 400, 400, FixedSize = false )]

  class SpecklePlugin : DockPanePlugin {

    // Called to tell the plugin to create it's pane.
    // Should be overriden in conjunction with DestroyControlPane.
    public override Control CreateControlPane () {
      ElementHost elementHost = new ElementHost {
        AutoSize = true,
        Child = new SpecklePane( /*address: "https://rimshot.app/speckleui"*/ )
      };

      elementHost.CreateControl();

      return elementHost;
    }

    // Called to tell the plugin to destroy it's pane.
    // Should be overriden in conjunction with CreateControlPane.
    public override void DestroyControlPane ( Control pane ) => pane.Dispose();
  }
}
