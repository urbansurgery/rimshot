using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using COMApi = Autodesk.Navisworks.Api.Interop.ComApi;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Analysis {

  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class ClashElements : UserControl, INotifyPropertyChanged {

    private string analysisScopeSelectionSetName = "Analysis Scope";
    private Dictionary<string, SelectionSet> selectedZones = new Dictionary<string, SelectionSet>();
    private Dictionary<string, ClashResultGroup> selectedClashes = new Dictionary<string, ClashResultGroup>();
    readonly Document oDoc = NavisworksApp.ActiveDocument;

    public ClashElements() {

      this.ActionMode = "Split";
      this.ActionTarget = "Clash";
      this.ZonesCount = 0;
      this.ClashesCount = 0;

      DataContext = this;

      // add responsive events
      oDoc.SelectionSets.Changed += UpdateZonesTree;
      oDoc.GetClash().TestsData.Changed += UpdateClashTree;

      InitializeComponent();
    }

    public void UpdateZonesTree( object sender, SavedItemChangedEventArgs e ) {

      var senderDoc = (Document)sender;


      if (senderDoc.FileName == "") {
        return;
      }

      if (e.OldItem != null && e.OldItem.DisplayName == analysisScopeSelectionSetName) {
        ZonesTree = RootZones();
        MessageBox.Show( $"{analysisScopeSelectionSetName} Folder renamed", "Warning" );
        return;
      }

      if (e.Action.ToString() == "Reset" && senderDoc.FileName != "") {
        ZonesTree = RootZones();
        return;
      }

      if (e.EditedParts == SavedItemParts.DisplayName) {
        if ((!(e.NewItem is null)
            && e.NewItem.DisplayName == analysisScopeSelectionSetName)) {
          ZonesTree = RootZones();
          return;
        }
        if ((!(e.NewParent is null)
            && e.NewParent.DisplayName == analysisScopeSelectionSetName)) {
          ZonesTree = RootZones();
          return;
        }

        if ((!(e.NewParent.Parent is null)
            && e.NewParent.Parent.DisplayName != ""
            && e.NewParent.Parent.DisplayName == analysisScopeSelectionSetName)) {
          ZonesTree = RootZones();
          return;
        }


      }
    }

    public void UpdateClashTree( object sender, SavedItemChangedEventArgs e ) {

      var senderDoc = (Document)sender;

      if (senderDoc.FileName == "") {
        return;
      }

      if (e.NewItem is null) {
        return;
      }

      if (e.NewItem.GetType() == (new ClashTest()).GetType()) {
        if (e.Action == SavedItemChangedAction.Replace) {
          return;
        }
        ClashTree = RootClashes();
        return;
      }

      if (e.Action == SavedItemChangedAction.Remove) {
        return;
      }

      if (e.EditedParts == SavedItemParts.Comments ||
          e.EditedParts.ToString() == "Status") {
        return;
      }

      if (e.Action == SavedItemChangedAction.Replace && senderDoc.FileName != "") {
        ClashTree = RootClashes();
        return;
      }

      try {

        if ((e.Action == SavedItemChangedAction.Edit || e.Action == SavedItemChangedAction.Add)
            && e.NewItem.IsGroup
            && ((ClashResultGroup)e.NewItem).Children.Count > 0
            && e.EditedParts != SavedItemParts.Comments) {
          ClashTree = RootClashes();
          return;
        }
      }
      catch (Exception err) {
        Console.WriteLine( err.Message );
      }

      //ClashTree = RootClashes();

    }

    private string _clashFilter = "";
    public string ClashFilter {
      get {
        return _clashFilter;
      }
      set {
        _clashFilter = value;
        OnPropertyChanged( "ClashFilter" );
      }
    }

    private bool _showResolved = false;
    public bool ShowResolved {
      get {
        return _showResolved;
      }
      set {
        _showResolved = value;
        OnPropertyChanged( "ShowResolved" );
      }
    }

    private bool _showAcknowledged = false;
    public bool ShowAcknowledged {
      get {
        return _showAcknowledged;
      }
      set {
        _showAcknowledged = value;
        OnPropertyChanged( "ShowAcknowledged" );
      }
    }

    private SavedItem _clashTree;
    public SavedItem ClashTree {
      get {
        return _clashTree;
      }
      set {
        _clashTree = value;
        OnPropertyChanged( "ClashTree" );
      }
    }

    private SavedItem _zonesTree;
    public SavedItem ZonesTree {
      get {
        return _zonesTree;
      }
      set {
        _zonesTree = value;
        OnPropertyChanged( "ZonesTree" );
      }
    }

    private string _actionTarget;
    public string ActionTarget {
      get {
        return _actionTarget;
      }
      set {
        if (_actionTarget != value) {
          _actionTarget = value;
          OnPropertyChanged( "ActionTarget" );
        }
      }
    }

    private string _actionMode;

    public string ActionMode {
      get {
        return _actionMode;
      }
      set {
        if (_actionMode != value) {
          _actionMode = value;
          OnPropertyChanged( "ActionMode" );
        }
      }
    }

    private int _zonesCount;

    public int ZonesCount {
      get {
        return _zonesCount;
      }
      set {
        _zonesCount = value;
        OnPropertyChanged( "ZonesCount" );
      }
    }
    private int _clashesCount;

    public int ClashesCount {
      get {
        return _clashesCount;
      }
      set {
        _clashesCount = value;
        OnPropertyChanged( "ClashesCount" );
      }
    }

    public SavedItem RootClashes() {

      SavedItemCollection allTests = NavisworksApp.ActiveDocument.GetClash().TestsData.Tests;

      FolderItem allTestsFolder = new FolderItem();

      foreach (ClashTest test in allTests) {

        // filter results that are groups only
        var groups = test.Children as IEnumerable<SavedItem>;
        var singletons = test.Children as IEnumerable<SavedItem>;

        if (_clashFilter.Length > 0) {

          groups = test.Children
              .Where( g => g.IsGroup
                   && (g as ClashResultGroup).Children.Count > 0
                   && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() ) );

          if (!_showAcknowledged && !_showResolved) {

            groups = test.Children
                .Where( g => g.IsGroup
                     && (g as ClashResultGroup).Children.Count > 0
                     && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() )
                     && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() )
                     && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
          }
          else if (!_showAcknowledged) {
            groups = test.Children
               .Where( g => g.IsGroup
                     && (g as ClashResultGroup).Children.Count > 0
                     && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() )
                     && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() ) );
          }
          else if (!_showResolved) {
            groups = test.Children
               .Where( g => g.IsGroup
                     && (g as ClashResultGroup).Children.Count > 0
                     && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() )
                     && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
          }
        }
        else if (!_showAcknowledged && !_showResolved) {
          groups = test.Children
              .Where( g => g.IsGroup
                   && (g as ClashResultGroup).Children.Count > 0
                   && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() )
                   && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
        }
        else if (!_showAcknowledged) {
          groups = test.Children
             .Where( g => g.IsGroup
                   && (g as ClashResultGroup).Children.Count > 0
                   && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() ) );
        }
        else if (!_showResolved) {
          groups = test.Children
             .Where( g => g.IsGroup
                   && (g as ClashResultGroup).Children.Count > 0
                   && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
        }

        // only process tests with results
        if (groups.Count() > 0) {

          var testFolder = new FolderItem {
            DisplayName = test.DisplayName
          };

          foreach (SavedItem result in groups.OrderBy( g => g.DisplayName )) {

            var clashResult = result as ClashResultGroup;
            var clashCount = clashResult.Children.Count;

            if (clashCount > 0) {

              var resultFolder = new FolderItem {
                DisplayName = $"{result.DisplayName} <count={clashCount}>"
              };
              testFolder.Children.Add( resultFolder );
            }
          }
          allTestsFolder.Children.Add( testFolder );
        }
      }

      return allTestsFolder;
    }

    public SavedItem RootZones( SavedItem root = null ) {

      // no root has been set - default to all selection sets.
      if (root == null) {
        root = oDoc.SelectionSets.RootItem;
      }

      // root has been set to a search set and not a folder
      if (!root.IsGroup) {
        return root;
      }

      // root is already set to the volumetric scope for zones
      if (root.DisplayName == analysisScopeSelectionSetName) {
        return root;
      }

      SavedItem AnalysisScope = null;
      // proceed with finding the matching folders

      foreach (SavedItem s in ((Autodesk.Navisworks.Api.GroupItem)root).Children) {
        if (s.IsGroup && s.DisplayName == analysisScopeSelectionSetName) {
          AnalysisScope = (FolderItem)s;
        }
      }

      if (AnalysisScope != null) {
        return AnalysisScope;
      }

      return root;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged( string propertyName = null ) {
      if (PropertyChanged != null) {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }

    private void Button_Refresh_Click( object sender, RoutedEventArgs e ) {
      ClashFilter = "";
      ZonesTree = RootZones();
      ClashTree = RootClashes();
    }

    private void Button_ZoneTreeRefresh_Click( object sender, RoutedEventArgs e ) {
      ZonesTree = RootZones();
    }

    private void Button_Filter_Click( object sender, RoutedEventArgs e ) {
      ClashTree = RootClashes();
    }

    private void Button_Execute_Click( object sender, RoutedEventArgs e ) {


      // get selected zones
      // get selected clash
      // get selected mode
      // execute method

      SplitByItemsInSearch( sender, e );
    }

    private string NumberOrLabel( SavedItem item ) {

      string label = item.DisplayName;
      RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

      // this matches either an issue number or whatever the first part of name is prior to the first space
      string pattern = @"(\d{3,4})|(\S+\s)";

      Regex rgx = new Regex( pattern, options );

      Match match = rgx.Match( label );
      string extract = match.Success ? match.Groups[0].Value : label;

      return extract;

    }

    private void Button_Select_Click( object sender, RoutedEventArgs e ) {
      // get selected zones collection
      // perform selection using that collection

      oDoc.CurrentSelection.Clear();

      foreach (var zone in selectedZones) {
        SelectionSource selectionSource = oDoc.SelectionSets.CreateSelectionSource( zone.Value );
        ModelItemCollection mList = selectionSource.TryGetSelectedItems( oDoc );
        oDoc.CurrentSelection.AddRange( mList );
      }
    }

    private void ShowItemsInSearch( object sender, RoutedEventArgs e ) {
      // Set selected items as visible
      // If parent is not visible set parent visible and siblings hidden
      // recurse to root
    }

    private void HideItemsInSearch( object sender, RoutedEventArgs e ) {
      // Set selected items as hidden
    }










    private void SplitByItemsInSearch( object sender, RoutedEventArgs e ) {

      MessageBox.Show( $"Mode > {ActionMode}\n\n" +
          $"Zones:\n{string.Join( ",\n", selectedZones.Keys )}\n\n" +
          $"Clashes:\n{string.Join( ",\n", selectedClashes.Values.Select( l => NumberOrLabel( l ) ) )}\n" +
          $"  of > {selectedClashes.First().Value.Parent.DisplayName}\n\n"
          );

      // with the selected clash result group(s)
      // populate the dictionaries


      // Clash Tests and results
      foreach (KeyValuePair<string, ClashResultGroup> clashGroup in selectedClashes.ToList()) {
        Result r = new Result( clashGroup.Value );

        var first = Tests.FirstOrDefault( kvp => kvp.Key.Equals( r.TestName ) );

        // Tests already has this test name
        if (first.Value != null) {
          first.Value.Item2.Add( r ); // Add a result to the existing list.
          first.Value.Item1.Extend( r.BoundingBox ); // Test Bounding Box increased to incorporate additional result.
        }
        else {
          var rr = new List<Result> {
            r
          };
          Tests.Add( r.TestName, new Tuple<BoundingBox3D, List<Result>>( r.BoundingBox, rr ) );
        }
      }

      // Zones and Geometry
      foreach (KeyValuePair<string, SelectionSet> zone in selectedZones) {

        var first = Zones.FirstOrDefault( kvp => kvp.Key.Equals( zone.Key ) );
        var l = new List<SelectionSet> {
          zone.Value
        };

        var z = new Zone( zone.Key, l );

        if (first.Value != null) {
          first.Value.Item1.Extend( z.BoundingBox );
          first.Value.Item2.Add( z );
        }
        else {
          var zz = new List<Zone> {
            z
          };
          Zones.Add( zone.Key, new Tuple<BoundingBox3D, List<Zone>>( z.BoundingBox, zz ) );
        }
      }

      Console.WriteLine( "Phew" );



      // filter each clash result into a dictionary of type <zone>,[clashresult]

      // dictionary of type <zone>, [bounding box for zone]







      /*
       * switch by clash / view
       * case clash:
       *     Get selected [ClashGroup] [Clash] results
       */

      //DocumentClash documentClash = NavisworksApp.ActiveDocument.GetClash();
      //DocumentClashTests testData = documentClash.TestsData;

      //FolderItem clashTestTree = new FolderItem();

      //SavedItemCollection tests = testData.Tests;

      //ClashTree = clashTestTree;



      /*     for each [Member] of the selected zone group (if single there is only one [Member]):
      *         for each [Volume] found by zone search:
      *             allocate to [Member] any [Clash] with position within bounding box of volume
      *         
      *     group all unallocated [Clash]s as "Unzoned"
      *     
      *     for each [Allocation]
      *     with all clashes within multiple [Allocation]s:
      *             pass [Clash]s, [Member]s and [Allocation]s to fine grain allocation
      *         
      *     generate new [ClashGroup]s per [Allocation] based on the name of the [ClashGroup] and [Member]
      *     add [Clash]s to groups
      *     add groups to [ClashTest]
      * case view:
      *    To Be Done
      */
    }

    private void SelectItemsInSearch( object sender, RoutedEventArgs e ) {

      SavedItem s = ((FrameworkElement)sender).DataContext as SavedItem;

      selectedZones.Clear();

      try {
        selectedZones = RecurseZoneSelections( s );
        ZonesCount = selectedZones.Count();

        oDoc.CurrentSelection.Clear();

        foreach (var zone in selectedZones) {
          SelectionSource selectionSource = oDoc.SelectionSets.CreateSelectionSource( zone.Value );
          ModelItemCollection mList = selectionSource.TryGetSelectedItems( oDoc );
          oDoc.CurrentSelection.AddRange( mList );
        }
      }
      catch (Exception err) {
        Console.WriteLine( err.ToString() );
      }
    }

    private void TreeView_SelectedZoneItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e ) {

      SavedItem s = (SavedItem)e.NewValue;

      selectedZones.Clear();

      try {
        selectedZones = RecurseZoneSelections( s );
        ZonesCount = selectedZones.Count();
      }
      catch (Exception err) {
        Console.WriteLine( err.ToString() );
      }
    }

    private void TreeView_SelectedClashItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e ) {

      SavedItem s = (SavedItem)e.NewValue;
      IEnumerable<SavedItem> groups = default;

      selectedClashes.Clear();

      if (s is null) return; // selection changed to a non thing - model unloaded perhaps?

      // clash test selected
      if (s.IsGroup) {
        var selectedTestName = s.DisplayName;

        foreach (ClashTest test in oDoc.GetClash().TestsData.Tests) {

          if (test.DisplayName == selectedTestName) {

            // filter results that are groups only
            groups = test.Children;

            if (_clashFilter.Length > 0) {

              groups = test.Children
                  .Where( g => g.IsGroup
                       && (g as ClashResultGroup).Children.Count > 0
                       && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() ) );

              if (!_showAcknowledged && !_showResolved) {

                groups = test.Children
                    .Where( g => g.IsGroup
                         && (g as ClashResultGroup).Children.Count > 0
                         && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() )
                         && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() )
                         && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
              }
              else if (!_showAcknowledged) {
                groups = test.Children
                   .Where( g => g.IsGroup
                         && (g as ClashResultGroup).Children.Count > 0
                         && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() )
                         && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() ) );
              }
              else if (!_showResolved) {
                groups = test.Children
                   .Where( g => g.IsGroup
                         && (g as ClashResultGroup).Children.Count > 0
                         && g.DisplayName.ToLower().Contains( _clashFilter.ToLower() )
                         && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
              }
            }
            else if (!_showAcknowledged && !_showResolved) {
              groups = test.Children
                  .Where( g => g.IsGroup
                       && (g as ClashResultGroup).Children.Count > 0
                       && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() )
                       && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
            }
            else if (!_showAcknowledged) {
              groups = test.Children
                 .Where( g => g.IsGroup
                       && (g as ClashResultGroup).Children.Count > 0
                       && !g.DisplayName.ToLower().Contains( ("ACKNOWLEDGED").ToLower() ) );
            }
            else if (!_showResolved) {
              groups = test.Children
                 .Where( g => g.IsGroup
                       && (g as ClashResultGroup).Children.Count > 0
                       && !g.DisplayName.ToLower().Contains( ("RESOLVED").ToLower() ) );
            }
          }
        }
      }

      // clash issue group selected (the tree representation of a group of issues is not a group)
      var testName = s.Parent.DisplayName;

      // find the parent test in all tests
      foreach (ClashTest test in oDoc.GetClash().TestsData.Tests) {

        if (test.DisplayName == testName) {

          // find matching result group
          groups = test.Children.Where(
              g => s.DisplayName.Contains( g.DisplayName )
          );
        }
      }

      foreach (ClashResultGroup g in groups) {
        selectedClashes.Add( g.DisplayName, g );
      }
      ClashesCount = selectedClashes.Count();
      Console.WriteLine( $"{selectedClashes.Keys.Count} Clash Groups Selected" );
    }

    private Dictionary<string, SelectionSet> RecurseZoneSelections( SavedItem si ) {

      Dictionary<string, SelectionSet> temp = new Dictionary<string, SelectionSet>();

      if (!si.IsGroup) {
        temp.Add( si.DisplayName, (SelectionSet)si );
        return temp;
      }

      foreach (SavedItem item in ((Autodesk.Navisworks.Api.GroupItem)si).Children) {
        foreach (var r in RecurseZoneSelections( item )) {
          temp.Add( r.Key, r.Value );
        }
      }

      return temp;
    }

    private void Button_MakeSets_Click( object sender, RoutedEventArgs e ) {

      if (selectedClashes.Count < 1) return; // No Clash results in any tests selected

      string test = selectedClashes.First().Value.Parent.DisplayName;
      MessageBox.Show( test );

      SavedItemCollection clashResults = selectedClashes.First().Value.Parent.Children;

      List<SavedItem> allResults = new List<SavedItem>();

      List<ModelItem> leftItems = new List<ModelItem>();
      List<ModelItem> rightItems = new List<ModelItem>();

      ModelItemCollection left = new ModelItemCollection();
      ModelItemCollection right = new ModelItemCollection();

      foreach (SavedItem result in clashResults) {

        if (result.IsGroup) {
          var clashResultGroup = (ClashResultGroup)result;

          foreach (ClashResult t in clashResultGroup.Children) {
            left.Add( t.Item1 );
            right.Add( t.Item2 );
          }
        }
        else {
          left.Add( ((ClashResult)result).Item1 );
          right.Add( ((ClashResult)result).Item2 );
        }
      }

      SavedItem leftSelection = new SelectionSet( left ) {
        DisplayName = test + " | Left"
      };

      SavedItem rightSelection = new SelectionSet( right ) {
        DisplayName = test + " | Right"
      };

      oDoc.SelectionSets.InsertCopy( 0, leftSelection );
      oDoc.SelectionSets.InsertCopy( 0, rightSelection );
    }


  }

  public class RadioConverter : IValueConverter {
    public object Convert( object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture ) {
      return value.Equals( parameter );
    }

    public object ConvertBack( object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture ) {
      return value.Equals( true ) ? parameter : Binding.DoNothing;
    }
  }

  public class GeometryCallback : COMApi.InwSimplePrimitivesCB {

    private readonly List<Triangle> _faces = new List<Triangle>();

    public List<Triangle> Faces {
      get {
        return _faces;
      }
    }

    public void Line( COMApi.InwSimpleVertex v1, COMApi.InwSimpleVertex v2 ) {
    }

    public void Point( COMApi.InwSimpleVertex v1 ) {
    }

    public void SnapPoint( COMApi.InwSimpleVertex v1 ) {
    }

    public void Triangle( COMApi.InwSimpleVertex v1, COMApi.InwSimpleVertex v2, COMApi.InwSimpleVertex v3 ) {
      _faces.Add( new Triangle( v1, v2, v3 ) );
    }
  }


}
