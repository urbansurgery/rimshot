using Autodesk.Navisworks.Api.Plugins;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Rimshot.Shared.Workshop.IssueList {

  #region WebViewPlugin

  [Plugin( Tools.IssueList.Plugin, "Rimshot", DisplayName = "Issue List", ToolTip = "Workshop Issue List Management" )]
  [DockPanePlugin( 400, 400, FixedSize = false )]

  class WebViewPlugin : DockPanePlugin {

    public override Control CreateControlPane () {

#if DEBUG
      System.Console.WriteLine( "Mode=Debug" );

#else
      System.Console.WriteLine( "Mode=Release" );     
#endif

      ElementHost eh = new ElementHost {
        AutoSize = true,
        Child = new IssueListPane( /*address: "https://rimshot.app/issues"*/ )
      };

      eh.CreateControl();

      return eh;

    }

    public override void DestroyControlPane ( Control pane ) => pane.Dispose();

  }





  #endregion
}
