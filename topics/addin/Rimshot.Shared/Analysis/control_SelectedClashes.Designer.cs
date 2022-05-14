using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;


namespace Rimshot.Shared.Analysis {

  partial class Control_SelectedClashes {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    protected override CreateParams CreateParams {
      get {
        var cp = base.CreateParams;
        cp.ExStyle |= 0x02000000;    // Turn on WS_EX_COMPOSITED
        return cp;
      }
    }

    #region Component Designer generated code

    class ListViewItemComparer : IComparer {
      private int col;
      private SortOrder order;
      public ListViewItemComparer() {
        col = 0;
        order = SortOrder.Ascending;
      }
      public ListViewItemComparer(int column, SortOrder order) {
        col = column;
        this.order = order;
      }
      public int Compare(object x, object y) {
        int returnVal;
        // Determine whether the type being compared is a date type.
        try {
          // Parse the two objects passed as a parameter as a DateTime.
          System.DateTime firstDate =
                  DateTime.Parse(((ListViewItem)x).SubItems[col].Text);
          System.DateTime secondDate =
                  DateTime.Parse(((ListViewItem)y).SubItems[col].Text);
          // Compare the two dates.
          returnVal = DateTime.Compare(firstDate, secondDate);
        }
        // If neither compared object has a valid date format, compare
        // as a string.
        catch {
          // Compare the two items as a string.
          returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                      ((ListViewItem)y).SubItems[col].Text);
        }
        // Determine whether the sort order is descending.
        if (order == SortOrder.Descending)
          // Invert the value returned by String.Compare.
          returnVal *= -1;
        return returnVal;
      }
    }

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    /// 
    private void InitializeComponent() {
      this.button2 = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.selectAllCheck = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // button2
      // 
      this.button2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button2.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.button2.Location = new System.Drawing.Point(10, 262);
      this.button2.Margin = new System.Windows.Forms.Padding(10);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 5;
      this.button2.Text = "Refresh";
      this.button2.UseVisualStyleBackColor = false;
      this.button2.Click += new System.EventHandler(this.RefreshClashTestList);
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button1.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.button1.Location = new System.Drawing.Point(91, 262);
      this.button1.Margin = new System.Windows.Forms.Padding(10);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 4;
      this.button1.Text = "Run";
      this.button1.UseVisualStyleBackColor = false;
      this.button1.Click += new System.EventHandler(this.RunSelectedClashTests);
      // 
      // listView1
      // 
      this.listView1.Alignment = System.Windows.Forms.ListViewAlignment.Default;
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
      | System.Windows.Forms.AnchorStyles.Left)
      | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listView1.CheckBoxes = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
      this.listView1.GridLines = true;
      this.listView1.Location = new System.Drawing.Point(10, 36);
      this.listView1.Margin = new System.Windows.Forms.Padding(10);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(702, 206);
      this.listView1.TabIndex = 3;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      //this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
      this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.ListView1_ItemCheck1);
      this.listView1.SelectedIndexChanged += new System.EventHandler(this.ListView1_SelectedIndexChanged);

      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Enabled";
      this.columnHeader1.Width = 51;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Clash Test";
      this.columnHeader2.Width = 62;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Type";
      this.columnHeader3.Width = 36;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Tolerance";
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Last Run";
      this.columnHeader5.Width = 493;
      // 
      // selectAllCheck
      // 
      this.selectAllCheck.AutoSize = true;
      this.selectAllCheck.FlatAppearance.BorderColor = System.Drawing.Color.White;
      this.selectAllCheck.Location = new System.Drawing.Point(17, 10);
      this.selectAllCheck.Name = "selectAllCheck";
      this.selectAllCheck.Size = new System.Drawing.Size(70, 17);
      this.selectAllCheck.TabIndex = 6;
      this.selectAllCheck.Text = "Select All";
      this.selectAllCheck.UseVisualStyleBackColor = true;
      this.selectAllCheck.CheckedChanged += new System.EventHandler(this.SelectAllCheck_CheckedChanged);
      // 
      // control_SelectedClashes
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScroll = true;
      this.AutoSize = true;
      this.Controls.Add(this.selectAllCheck);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.listView1);
      this.MinimumSize = new System.Drawing.Size(722, 210);
      this.Name = "control_SelectedClashes";
      this.Size = new System.Drawing.Size(722, 305);
      this.Load += new System.EventHandler(this.Control_SelectedClashes_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.CheckBox selectAllCheck;

  }
}
