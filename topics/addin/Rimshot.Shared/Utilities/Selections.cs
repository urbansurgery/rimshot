using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Text;
using Autodesk.Navisworks.Api;


using NavisworksApp = Autodesk.Navisworks.Api.Application;
using Autodesk.Navisworks.Api.DocumentParts;

namespace Rimshot.Shared {
  public class Selections {

    public Selections() {
    }

    public static void ShowSelected(bool showOnlySelected = false, bool clearSelectionAfterShow = true) {

      Document activeDocument = NavisworksApp.ActiveDocument;
      DocumentModels models = activeDocument.Models;
      ModelItemCollection selectedItems = NavisworksApp.ActiveDocument.CurrentSelection.SelectedItems;

      if (selectedItems.Count == 0) return;

      if (showOnlySelected) {
        // hide all root models
        foreach (Model model in models) {
          ModelItem rootItem = model.RootItem;
          models.SetHidden(rootItem.Self, true);
        }
      }

      SelectionSet selectionSet = new SelectionSet();

      selectionSet.ExplicitModelItems.AddRange(selectedItems);

      Views.ShowSelectionSet_COM(selectionSet);

      if (clearSelectionAfterShow) {
        activeDocument.CurrentSelection.Clear();
      }
    }
  }
}
