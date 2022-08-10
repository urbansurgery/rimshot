using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;
using ComApiBridge = Autodesk.Navisworks.Api.ComApi;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Bindings {



  public abstract class SpeckleAppBindings : DefaultBindings {
    public Document ActiveDocument { get; set; }

    public InwOpSelection CurrentSelection { get; set; }
    public InwOpSelection NewSelection { get; set; }

    protected SpeckleAppBindings () {
      this.AppName = "Speckle";
      this.ActiveDocument = NavisworksApplication.ActiveDocument;
      this.ActiveDocument.SelectionSets.Changed += SelectionSets_Changed;
      this.ActiveDocument.CurrentSelection.Changed += CurrentSelection_Changed;
      this.ActiveDocument.CurrentSelection.Changing += CurrentSelection_Changing;
    }

    private void SelectionSets_Changed ( object sender, SavedItemChangedEventArgs e ) {
      Logging.ConsoleLog( "Selection Sets Changed." );
      NotifyUi( "changed-selection-sets", "Bananas" );
    }

    public virtual void GetCurrentSelectedItems () => CurrentSelection_Changed( null, null );

    public virtual IEnumerable<Account> GetAccounts () {
      //List<SpeckleAccount> accounts; // = new List<SpeckleAccount>() { account };

      IEnumerable<Account> speckleAccounts = AccountManager.GetAccounts();
      //Account defaultAccount = AccountManager.GetDefaultAccount();

      //accounts = speckleAccounts.Select( acc => new SpeckleAccount(
      //  account: acc,
      //  isDefault: acc.Equals( defaultAccount ) ) ).ToList();


      return speckleAccounts;
    }

    public virtual List<Stream> GetStreams ( dynamic selectedAccount ) {
      IEnumerable<Account> speckleAccounts = AccountManager.GetAccounts();
      Account account = speckleAccounts.FirstOrDefault( acc => acc.id.Equals( selectedAccount.id ) );

      if ( account == null ) {
        return null;
      }

      Client client = new Client( account );

      List<Stream> streams = client.StreamsGet().Result;

      return streams;
    }

    public virtual List<Branch> GetBranches ( dynamic selectedAccount, dynamic selectedStream ) {
      IEnumerable<Account> speckleAccounts = AccountManager.GetAccounts();
      Account account = speckleAccounts.FirstOrDefault( acc => acc.id.Equals( selectedAccount.id ) );

      if ( account == null ) {
        return null;
      }

      Client client = new Client( account );

      List<Branch> branches = client.StreamGetBranches( selectedStream.id ).Result;

      return branches;
    }

    private void CurrentSelection_Changed ( object sender, EventArgs e ) {
      this.NewSelection = ComApiBridge.ComApiBridge.State.CurrentSelection;

      int before = this.CurrentSelection.Paths().Count;
      int after = this.NewSelection.Paths().Count;

      if ( before == after && after == 0 ) {
        //Logging.ConsoleLog( "System Loading Event Amended Selection." );
        //return;
      }

      //Console.WriteLine( $"Before:{before}, After:{after}" );

      NotifyUi( "changed-current-selection", after );
    }

    private void CurrentSelection_Changing ( object sender, EventArgs e ) => this.CurrentSelection =
      ( InwOpSelection )ComApiBridge.ComApiBridge.State.CurrentSelection.Copy();


    private static bool ValidateTarget ( dynamic speckleAccount, dynamic stream, dynamic branch ) {
      IEnumerable<Account> speckleAccounts = AccountManager.GetAccounts();
      if ( speckleAccount != null ) {
        IEnumerable<Account> account = speckleAccounts.Where( acc => acc.id == ( ( Account )speckleAccount ).id );
      }


      return false;
    }

    private bool ValidateOptions ( dynamic options ) => false;

    private bool ValidateSelection ( dynamic selection ) => false;


    public virtual void CommitToSpeckle ( dynamic account, dynamic stream, dynamic branch, dynamic options ) =>
      ValidateTarget( account, stream, branch );
    // Validate target
    // Validate options object
    // Validate Selections
    // Begin Progress
    // Get Selected Elements      
    // Populate properties
    // Cast to Base
    // Build Commit
    // End Progress
    // Send
#if DEBUGUI
    private const string SpeckleUrl = "http://192.168.86.29:8080/speckleui";
#else
    const string speckleUrl = "https://rimshot.app/speckleui";
#endif
    public new const string Url = SpeckleUrl;
    public SpecklePane Window { get; set; }


    public virtual List<SavedSelectionTree> GetSets () {
      FolderItem rootItem = this.ActiveDocument.SelectionSets.RootItem;

      List<SavedSelectionTree> root =
        new List<SavedSelectionTree>( rootItem.Children.Select( child => new SavedSelectionTree( child ) ) );

      return root;
    }
  }

  public class SavedSelectionTree {
    public readonly List<SavedSelectionTree> children;

    public SavedSelectionTree ( SavedItem item ) {
      this.Name = item.DisplayName;
      this.Guid = item.Guid.ToString();

      if ( item.IsGroup ) {
        GroupItem group = ( GroupItem )item;
        IEnumerable<SavedSelectionTree> groupChildren = group.Children.Select(
          child => new SavedSelectionTree( child )
        );

        this.children = groupChildren.ToList();

        this.Type = "folder";
      } else {
        if ( ( ( SelectionSet )item ).HasSearch ) {
          this.Type = "search";
        }

        if ( ( ( SelectionSet )item ).HasExplicitModelItems ) {
          this.Type = "selection";
        }

        this.children = null;
      }
    }

    public string Name { get; set; }
    public string Guid { get; set; }
    public string Type { get; set; }

    public static implicit operator SavedSelectionTree ( List<SavedSelectionTree> v ) =>
      throw new NotImplementedException();
  }
}