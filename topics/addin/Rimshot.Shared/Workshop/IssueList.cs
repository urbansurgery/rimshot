using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using Newtonsoft.Json;
using app = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Shared.Workshop.IssueList {

  #region WebViewPlugin

  [Plugin( Tools.IssueList.Plugin, "Rimshot", DisplayName = "Issue List", ToolTip = "Workshop Issue List Management" )]
  [DockPanePlugin( 400, 400, FixedSize = false )]

  class WebViewPlugin : DockPanePlugin {

    public override Control CreateControlPane () {


      ElementHost eh = new ElementHost {
        AutoSize = true,
        Child = new IssueListPane( address: "https://rimshot.app/issuelist/" )
      };

      eh.CreateControl();

      return eh;

    }

    public override void DestroyControlPane ( Control pane ) => pane.Dispose();

  }





  #endregion
}
