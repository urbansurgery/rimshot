using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Bindings {

  public abstract class SpeckleAppBindings : DefaultBindings {

    public Document ActiveDocument { get; set; }
    public SpeckleAppBindings () {
      AppName = "Speckle";
      this.ActiveDocument = NavisworksApp.ActiveDocument;
      this.ActiveDocument.SelectionSets.Changed += SelectionSets_Changed;
    }

    private void SelectionSets_Changed ( object sender, SavedItemChangedEventArgs e ) {
      Logging.ConsoleLog( "Selection Sets Changed." );
      NotifyUI( "changed-selection-sets", "Bananas" );
    }

#if DEBUGUI
    const string speckleUrl = "http://192.168.86.29:8080/speckleui";
#else
    const string speckleUrl = "https://rimshot.app/speckleui";
#endif
    public new const string Url = speckleUrl;
    public SpecklePane Window { get; set; }


    public virtual List<SavedSelectionTree> GetSets () {

      FolderItem rootItem = this.ActiveDocument.SelectionSets.RootItem;

      List<SavedSelectionTree> root = new List<SavedSelectionTree>( rootItem.Children.Select( child => new SavedSelectionTree( child ) ) );

      return root;
    }
  }
  public class SavedSelectionTree {
    public string Name { get; set; }
    public string Guid { get; set; }
    public string Type { get; set; }

    public readonly List<SavedSelectionTree> Children;

    public SavedSelectionTree ( SavedItem item ) {
      this.Name = item.DisplayName;
      this.Guid = item.Guid.ToString();

      if ( item.IsGroup ) {
        GroupItem group = ( GroupItem )item;
        IEnumerable<SavedSelectionTree> groupChildren = group.Children.Select(
          child => new SavedSelectionTree( child )
          );

        this.Children = groupChildren.ToList();

        this.Type = "folder";
      } else {
        if ( ( ( SelectionSet )item ).HasSearch ) {
          this.Type = "search";
        }

        if ( ( ( SelectionSet )item ).HasExplicitModelItems ) {
          this.Type = "selection";
        }

        this.Children = null;
      }
    }

    public static implicit operator SavedSelectionTree ( List<SavedSelectionTree> v ) {
      throw new NotImplementedException();
    }
  }
}

