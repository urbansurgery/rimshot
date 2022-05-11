using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Autodesk.Navisworks.Api.Plugins;

namespace Rimshot.Shared.Analysis {

  [DockPanePluginAttribute( 722, 210,
      AutoScroll = true,
      FixedSize = false,
      MinimumHeight = 210,
      MinimumWidth = 722 ), PluginAttribute( Tools.TestRunner.Plugin, "Rimshot",
      DisplayName = "Clash Test Selected Runner",
      ExtendedToolTip = "",
      Options = PluginOptions.None,
      SupportsIsSelfEnabled = false,
      ToolTip = "" )]

  public class TestRunner : DockPanePlugin {


    public override Control CreateControlPane() {

      //create the control that will be used to display in the pane
      Control_SelectedClashes control = new Control_SelectedClashes {
        Dock = DockStyle.Bottom
      };
      control.CreateControl();

      return control;
    }

    public override IWin32Window CreateHWndPane( IWin32Window parent ) {
      return default;
    }

    public override void DestroyControlPane( System.Windows.Forms.Control pane ) {
      if (pane is UserControl control) {
        control.Dispose();
      }
    }

    public override void DestroyHWndPane( System.Windows.Forms.IWin32Window pane ) {
    }

    public override void OnActivePaneChanged( bool isActive ) {
    }

    public override void OnVisibleChanged() {
    }

    public override bool TryShowHelp() {
      return default;
    }

    public override bool TryShowHelpAtScreenPoint( int x, int y ) {
      return default;
    }

    public override bool TryShowHelpForHighlight() {
      return default;
    }
  }
}
