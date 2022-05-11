using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.Navisworks.Api.Plugins;
using Rimshot.Shared.WebView;

namespace Rimshot.Shared.Registers.Issue {

  #region WebViewPlugin

  [Plugin( Tools.IssueRegister.Plugin, "Rimshot", DisplayName = "Issue Register", ToolTip = "Basic Webview with CEFSharp" )]
  [DockPanePlugin( 400, 900, FixedSize = false )]


  class WebViewPlugin : DockPanePlugin {
    public override System.Windows.Forms.Control CreateControlPane() {
      ElementHost eh = new ElementHost {
        AutoSize = true,
        Child = new WebViewPane( address: "https://Rimshot.atlassian.net" )
      };

      eh.CreateControl();

      return eh;

    }

    public override void DestroyControlPane( System.Windows.Forms.Control pane ) {
      pane.Dispose();
    }
  }

  #endregion
}
