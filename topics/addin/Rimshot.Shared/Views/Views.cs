using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Shared {
  public class Views {
    public Views () { }

    /// Show only items in Selection Set
    internal static void ShowSelectionSet_COM ( SelectionSet selectionSet ) {
      LcOpSelectionSetsElement.MakeVisible( NavisworksApp.MainDocument.State, selectionSet );
    }
    /// Show only items in Model Item Collection 
    internal static void ShowSelectionSet_COM ( ModelItemCollection modelItems ) {
      LcOpSelectionSetsElement.MakeVisible( NavisworksApp.MainDocument.State, new SelectionSet( modelItems ) );
    }
  }
}
