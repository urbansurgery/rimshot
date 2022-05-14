using System;
using System.Windows.Forms;

using Autodesk.Navisworks.Api.Clash;

namespace Rimshot.Shared.Analysis {


  public partial class Control_SelectedClashes : UserControl {

    private bool shouldIgnoreCheckEvent;
    private bool shouldIgnoreSelectAllEvent;

    public Control_SelectedClashes() {
      InitializeComponent();
    }

    private void RefreshClashTestList(object sender, EventArgs e) {
      listView1.Items.Clear();

      DocumentClash document_clash = Autodesk.Navisworks.Api.Application.ActiveDocument.GetClash();
      Autodesk.Navisworks.Api.Units u = Autodesk.Navisworks.Api.Application.ActiveDocument.Units;
      var sF = Autodesk.Navisworks.Api.UnitConversion.ScaleFactor(u, Autodesk.Navisworks.Api.Units.Millimeters);

      listView1.BeginUpdate();
      foreach (ClashTest cTest in document_clash.TestsData.Tests) {

        string[] arr = new string[6];
        ListViewItem itm;

        //Add first item
        arr[0] = "";
        arr[1] = cTest.DisplayName;
        arr[2] = cTest.TestType.ToString();
        arr[3] = (cTest.Tolerance * sF).ToString() + " mm";
        arr[4] = cTest.LastRun.ToString().Split(' ')[0];
        arr[5] = cTest.Guid.ToString();
        itm = new ListViewItem(arr);

        listView1.Items.Add(itm);
      }

      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      listView1.AutoResizeColumn(4, ColumnHeaderAutoResizeStyle.ColumnContent);

      selectAllCheck.CheckState = CheckState.Unchecked;
      listView1.EndUpdate();
    }

    private void RunSelectedClashTests(object sender, EventArgs e) {
      foreach (ListViewItem row in listView1.CheckedItems) {
        string selGuid = row.SubItems[5].Text;
        DocumentClashTests tests = Autodesk.Navisworks.Api.Application.ActiveDocument.GetClash().TestsData;

        foreach (ClashTest test in tests.Tests) {
          if (test.Guid.ToString() == selGuid) {
            tests.TestsRunTest(test);
          }
        }
      }

      string run_count = listView1.CheckedItems.Count.ToString();

      RefreshClashTestList(sender, e);
      MessageBox.Show(run_count + " Tests Updated");
    }

    private void ListView1_ItemCheck1(object sender, System.Windows.Forms.ItemCheckedEventArgs e) {
      if (!shouldIgnoreCheckEvent) {
        shouldIgnoreSelectAllEvent = true;

        if (listView1.Items.Count == listView1.CheckedItems.Count) {
          selectAllCheck.CheckState = CheckState.Checked;
        }
        else if (listView1.CheckedItems.Count == 0) {
          selectAllCheck.CheckState = CheckState.Unchecked;
        }
        else {
          selectAllCheck.CheckState = CheckState.Indeterminate;
        }

        shouldIgnoreSelectAllEvent = false;
      }
    }

    private void ListView1_SelectedIndexChanged(object sender, EventArgs e) {

    }


    private void Control_SelectedClashes_Load(object sender, EventArgs e) {

    }

    private int sortColumn = -1;

    private void SelectAllCheck_CheckedChanged(object sender, EventArgs e) {
      if (!shouldIgnoreSelectAllEvent) {
        shouldIgnoreCheckEvent = true;

        foreach (ListViewItem row in listView1.Items) {
          row.Checked = selectAllCheck.Checked;
        }
        shouldIgnoreCheckEvent = false;
      }
    }

    private void ListView1_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e) {

      // Determine whether the column is the same as the last column clicked.
      if (e.Column != sortColumn) {
        // Set the sort column to the new column.
        sortColumn = e.Column;
        // Set the sort order to ascending by default.
        listView1.Sorting = SortOrder.Ascending;
      }
      else {
        // Determine what the last sort order was and change it.
        if (listView1.Sorting == SortOrder.Ascending)
          listView1.Sorting = SortOrder.Descending;
        else
          listView1.Sorting = SortOrder.Ascending;
      }

      // Call the sort method to manually sort.
      listView1.BeginUpdate();
      listView1.Sort();
      // Set the ListViewItemSorter property to a new ListViewItemComparer
      // object.
      this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column,
                                                        listView1.Sorting);
      listView1.EndUpdate();
    }


  }
}
