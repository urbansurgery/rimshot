using Autodesk.Navisworks.Api;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Rimshot {

  /// <summary>
  /// Jira is the main datacontext, it just contains a IssuesBCFCollection, but I still follow this structure in case 
  /// of future developement and uniformation to other Jira/BCF plugins
  /// </summary>
  public class Jira : INotifyPropertyChanged {


    public ObservableCollection<IssueBCF> issuesBCFCollection;

    public ObservableCollection<IssueBCF> IssuesBCFCollection {
      get {
        return issuesBCFCollection;
      }
      set {
        issuesBCFCollection = value;
        NotifyPropertyChanged( "IssuesBCFCollection" );
      }
    }

    /// <summary>
    /// Changed Property Handler
    /// </summary>
    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Changed Property Handler
    /// </summary>
    /// <param name="info"></param>
    public void NotifyPropertyChanged ( String info ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( info ) );
  }

  /// <summary>
  /// class the defines the items inside the listview. it just contains a SavedViewpoint, but I still follow this structure in case 
  /// of future developement and uniformation to other Jira/BCF plugins
  /// </summary>
  public class IssueBCF {
    public SavedViewpoint SavedViewpoint {
      get; set;
    }

    /// <summary>
    /// Changed Property Handler
    /// </summary>
    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Changed Property Handler
    /// </summary>
    /// <param name="info"></param>
    public void NotifyPropertyChanged ( String info ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( info ) );
  }

}
