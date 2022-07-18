using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Shared {
  public class Selections {

    public Selections () {
    }

    public static void ShowSelected ( bool showOnlySelected = false, bool clearSelectionAfterShow = true ) {

      Document activeDocument = NavisworksApp.ActiveDocument;
      DocumentModels models = activeDocument.Models;
      ModelItemCollection selectedItems = NavisworksApp.ActiveDocument.CurrentSelection.SelectedItems;

      if ( selectedItems.Count == 0 ) {
        return;
      }

      if ( showOnlySelected ) {
        // hide all root models
        foreach ( Model model in models ) {
          ModelItem rootItem = model.RootItem;
          models.SetHidden( rootItem.Self, true );
        }
      }

      SelectionSet selectionSet = new SelectionSet();

      selectionSet.ExplicitModelItems.AddRange( selectedItems );

      Views.ShowSelectionSet_COM( selectionSet );

      if ( clearSelectionAfterShow ) {
        activeDocument.CurrentSelection.Clear();
      }
    }
  }
}
