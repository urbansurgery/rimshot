using Autodesk.Navisworks.Api.Plugins;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Rimshot {

  #region WebViewPlugin

  [Plugin( IssueList.Plugin, "Rimshot", DisplayName = "Rimshot", ToolTip = "Issue Register" )]
  [DockPanePlugin( 400, 400, FixedSize = false )]

  class WebViewPlugin : DockPanePlugin {

    public override Control CreateControlPane () {

      // Called to tell the plugin to create it's pane.
      // Should be overriden in conjunction with DestroyControlPane.
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





  #endregion
}
