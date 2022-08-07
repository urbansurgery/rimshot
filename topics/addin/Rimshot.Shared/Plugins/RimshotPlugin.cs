using Autodesk.Navisworks.Api.Plugins;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Rimshot {
  [Plugin( Rimshot.Plugin, "Rimshot", DisplayName = "Rimshot", ToolTip = "Rimshot Issue Register" )]
  [DockPanePlugin( 500, 800, FixedSize = false, AutoScroll = false, MinimumWidth = 100, MinimumHeight = 400 )]
  internal class RimshotPlugin : DockPanePlugin {
    // Called to tell the plugin to create it's pane.
    // Should be overriden in conjunction with DestroyControlPane.
    public override Control CreateControlPane () {
      ElementHost elementHost = new ElementHost {
        AutoSize = true,
        Child = new RimshotPane( /*address: "https://rimshot.app/issues"*/ ),
        Dock = DockStyle.Fill
      };
      elementHost.CreateControl();

      return elementHost;
    }


    //gpg

    // Called to tell the plugin to destroy it's pane.
    // Should be overriden in conjunction with CreateControlPane.
    public override void DestroyControlPane ( Control pane ) => pane.Dispose();
  }
}
