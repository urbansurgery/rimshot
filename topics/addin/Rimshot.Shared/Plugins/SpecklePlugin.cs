using Autodesk.Navisworks.Api.Plugins;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Rimshot {
  [Plugin( Speckle.Plugin, "Rimshot", DisplayName = "Speckle", ToolTip = "Speckle" )]
  [DockPanePlugin( 500, 800, FixedSize = false, AutoScroll = false, MinimumWidth = 100, MinimumHeight = 400 )]
  class SpecklePlugin : DockPanePlugin {

    // Called to tell the plugin to create it's pane.
    // Should be overriden in conjunction with DestroyControlPane.
    public override Control CreateControlPane () {
      ElementHost elementHost = new ElementHost {
        AutoSize = true,
        Child = new SpecklePane( /*address: "https://rimshot.app/speckleui"*/ ),
        Dock = DockStyle.Fill
      };
      elementHost.CreateControl();

      return elementHost;
    }

    // Called to tell the plugin to destroy it's pane.
    // Should be overriden in conjunction with CreateControlPane.
    public override void DestroyControlPane ( Control pane ) => pane.Dispose();
  }
}
