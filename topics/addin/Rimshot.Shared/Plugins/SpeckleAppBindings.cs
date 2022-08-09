using Autodesk.Navisworks.Api;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComApiBridge = Autodesk.Navisworks.Api.ComApi;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Bindings {

  public class SpeckleAccount {
    public string Email { get => this.Speckle_account.userInfo.email; }
    public string Host { get => this.Speckle_account.serverInfo.url; }
    public string Displayname { get => this.Speckle_account.userInfo.name; }
    public bool IsDefault { get; private set; }
    public string Id { get => Speckle_account.id; }

    private Account Speckle_account { get; set; }


    public SpeckleAccount ( Account account, bool isDefault ) {
      this.Speckle_account = account;
      this.IsDefault = isDefault;
    }
  }



  public abstract class SpeckleAppBindings : DefaultBindings {

    public Document ActiveDocument { get; set; }

    public ComApi.InwOpSelection currSelect { get; set; }
    public ComApi.InwOpSelection newSelect { get; set; }

    public SpeckleAppBindings () {
      AppName = "Speckle";
      this.ActiveDocument = NavisworksApp.ActiveDocument;
      this.ActiveDocument.SelectionSets.Changed += SelectionSets_Changed;
      this.ActiveDocument.CurrentSelection.Changed += CurrentSelection_Changed;
      this.ActiveDocument.CurrentSelection.Changing += CurrentSelection_Changing;
    }

    private void SelectionSets_Changed ( object sender, SavedItemChangedEventArgs e ) {
      Logging.ConsoleLog( "Selection Sets Changed." );
      NotifyUI( "changed-selection-sets", "Bananas" );
    }

    public virtual void GetCurrentSelectedItems () => CurrentSelection_Changed( null, null );

    public virtual List<SpeckleAccount> GetAccounts () {
      List<SpeckleAccount> accounts; // = new List<SpeckleAccount>() { account };

      IEnumerable<Account> speckleAccounts = AccountManager.GetAccounts();
      Account defaultAccount = AccountManager.GetDefaultAccount();

      accounts = speckleAccounts.Select( acc => new SpeckleAccount(
        account: acc,
        isDefault: acc.Equals( defaultAccount ) ) ).ToList();

      return accounts;
    }

    public virtual List<Stream> GetStreams ( dynamic selectedAccount ) {
      IEnumerable<Account> speckleAccounts = AccountManager.GetAccounts();
      Account account = speckleAccounts.Where( acc => acc.id.Equals( selectedAccount.Id ) ).FirstOrDefault();

      if ( account == null ) {
        return null;
      }

      Client client = new Client( account );

      List<Stream> streams = client.StreamsGet().Result;

      return streams;
    }

    public virtual List<Branch> GetBranches ( dynamic selectedAccount, dynamic selectedStream ) {
      IEnumerable<Account> speckleAccounts = AccountManager.GetAccounts();
      Account account = speckleAccounts.Where( acc => acc.id.Equals( selectedAccount.Id ) ).FirstOrDefault();

      if ( account == null ) {
        return null;
      }

      Client client = new Client( account );

      List<Branch> branches = client.StreamGetBranches( selectedStream.id ).Result;

      return branches;
    }

    private void CurrentSelection_Changed ( object sender, EventArgs e ) {
      this.newSelect = ComApiBridge.ComApiBridge.State.CurrentSelection;

      int before = this.currSelect.Paths().Count;
      int after = this.newSelect.Paths().Count;

      if ( before == after && after == 0 ) {
        //Logging.ConsoleLog( "System Loading Event Amended Selection." );
        //return;
      }

      //Console.WriteLine( $"Before:{before}, After:{after}" );

      NotifyUI( "changed-current-selection", after );
    }
    private void CurrentSelection_Changing ( object sender, EventArgs e ) => this.currSelect = ( ComApi.InwOpSelection )ComApiBridge.ComApiBridge.State.CurrentSelection.Copy();

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

