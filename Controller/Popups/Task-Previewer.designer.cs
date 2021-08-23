
namespace Controller.Popups
{
    partial class TaskPreviewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn1 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn2 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn3 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn4 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn5 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn6 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn7 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn8 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            Syncfusion.WinForms.DataGrid.GridTextColumn gridTextColumn9 = new Syncfusion.WinForms.DataGrid.GridTextColumn();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskPreviewer));
            this.tcMain = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            this.tpOngoing = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.dgATasks = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            this.tpCompleted = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.dgCTasks = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            ((System.ComponentModel.ISupportInitialize)(this.tcMain)).BeginInit();
            this.tcMain.SuspendLayout();
            this.tpOngoing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgATasks)).BeginInit();
            this.tpCompleted.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCTasks)).BeginInit();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.ActiveTabFont = new System.Drawing.Font("Palatino Linotype", 12F, System.Drawing.FontStyle.Bold);
            this.tcMain.BackColor = System.Drawing.Color.White;
            this.tcMain.BeforeTouchSize = new System.Drawing.Size(594, 445);
            this.tcMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tcMain.CloseButtonBackColor = System.Drawing.Color.Empty;
            this.tcMain.Controls.Add(this.tpOngoing);
            this.tcMain.Controls.Add(this.tpCompleted);
            this.tcMain.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.FocusOnTabClick = false;
            this.tcMain.Location = new System.Drawing.Point(2, 2);
            this.tcMain.Name = "tcMain";
            this.tcMain.ShowToolTips = true;
            this.tcMain.Size = new System.Drawing.Size(594, 445);
            this.tcMain.TabIndex = 0;
            this.tcMain.TabStop = false;
            this.tcMain.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererDockingWhidbeyBeta);
            this.tcMain.ThemeName = "TabRendererDockingWhidbeyBeta";
            this.tcMain.ThemesEnabled = true;
            // 
            // tpOngoing
            // 
            this.tpOngoing.Controls.Add(this.dgATasks);
            this.tpOngoing.Cursor = System.Windows.Forms.Cursors.Default;
            this.tpOngoing.Image = null;
            this.tpOngoing.ImageSize = new System.Drawing.Size(16, 16);
            this.tpOngoing.Location = new System.Drawing.Point(3, 41);
            this.tpOngoing.Name = "tpOngoing";
            this.tpOngoing.ShowCloseButton = true;
            this.tpOngoing.Size = new System.Drawing.Size(588, 401);
            this.tpOngoing.TabIndex = 3;
            this.tpOngoing.Text = "Ongoing (Active) Tasks";
            this.tpOngoing.ThemesEnabled = true;
            // 
            // dgATasks
            // 
            this.dgATasks.AccessibleName = "Table";
            this.dgATasks.AllowEditing = false;
            this.dgATasks.AllowGrouping = false;
            this.dgATasks.AllowResizingColumns = true;
            this.dgATasks.AllowTriStateSorting = true;
            this.dgATasks.AutoExpandGroups = true;
            this.dgATasks.AutoFitGroupDropAreaItem = true;
            this.dgATasks.AutoGenerateColumns = false;
            this.dgATasks.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.Fill;
            gridTextColumn1.AllowEditing = false;
            gridTextColumn1.AllowGrouping = false;
            gridTextColumn1.AllowResizing = true;
            gridTextColumn1.HeaderText = "ID";
            gridTextColumn1.MappingName = "ID";
            gridTextColumn1.MaximumWidth = 50D;
            gridTextColumn1.MinimumWidth = 50D;
            gridTextColumn2.AllowEditing = false;
            gridTextColumn2.AllowGrouping = false;
            gridTextColumn2.AllowResizing = true;
            gridTextColumn2.HeaderText = "Task Name";
            gridTextColumn2.MappingName = "Name";
            gridTextColumn2.MaximumWidth = 350D;
            gridTextColumn2.MinimumWidth = 250D;
            gridTextColumn3.AllowEditing = false;
            gridTextColumn3.AllowGrouping = false;
            gridTextColumn3.AllowResizing = true;
            gridTextColumn3.HeaderText = "Initiated At";
            gridTextColumn3.MappingName = "Initiated";
            gridTextColumn3.MaximumWidth = 150D;
            gridTextColumn3.MinimumWidth = 150D;
            gridTextColumn4.AllowEditing = false;
            gridTextColumn4.AllowGrouping = false;
            gridTextColumn4.AllowResizing = true;
            gridTextColumn4.HeaderText = "Status";
            gridTextColumn4.MappingName = "Status";
            gridTextColumn4.MaximumWidth = 135D;
            gridTextColumn4.MinimumWidth = 135D;
            this.dgATasks.Columns.Add(gridTextColumn1);
            this.dgATasks.Columns.Add(gridTextColumn2);
            this.dgATasks.Columns.Add(gridTextColumn3);
            this.dgATasks.Columns.Add(gridTextColumn4);
            this.dgATasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgATasks.Location = new System.Drawing.Point(0, 0);
            this.dgATasks.Name = "dgATasks";
            this.dgATasks.Size = new System.Drawing.Size(588, 401);
            this.dgATasks.Style.CellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.dgATasks.Style.CellStyle.Font.Bold = true;
            this.dgATasks.Style.CellStyle.Font.Facename = "Palatino Linotype";
            this.dgATasks.Style.CellStyle.Font.Size = 10F;
            this.dgATasks.Style.CellStyle.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.dgATasks.Style.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(200)))));
            this.dgATasks.Style.HeaderStyle.FilterIconColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(29)))), ((int)(((byte)(29)))));
            this.dgATasks.Style.HeaderStyle.Font.Bold = true;
            this.dgATasks.Style.HeaderStyle.Font.Facename = "Palatino Linotype";
            this.dgATasks.Style.HeaderStyle.Font.Size = 12F;
            this.dgATasks.Style.HeaderStyle.Font.Strikeout = false;
            this.dgATasks.Style.HeaderStyle.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(190)))));
            this.dgATasks.TabIndex = 1;
            // 
            // tpCompleted
            // 
            this.tpCompleted.Controls.Add(this.dgCTasks);
            this.tpCompleted.Cursor = System.Windows.Forms.Cursors.Default;
            this.tpCompleted.Image = null;
            this.tpCompleted.ImageSize = new System.Drawing.Size(16, 16);
            this.tpCompleted.Location = new System.Drawing.Point(3, 41);
            this.tpCompleted.Name = "tpCompleted";
            this.tpCompleted.ShowCloseButton = true;
            this.tpCompleted.Size = new System.Drawing.Size(588, 403);
            this.tpCompleted.TabIndex = 4;
            this.tpCompleted.Text = "Completed Tasks";
            this.tpCompleted.ThemesEnabled = true;
            // 
            // dgCTasks
            // 
            this.dgCTasks.AccessibleName = "Table";
            this.dgCTasks.AllowEditing = false;
            this.dgCTasks.AllowGrouping = false;
            this.dgCTasks.AllowResizingColumns = true;
            this.dgCTasks.AllowTriStateSorting = true;
            this.dgCTasks.AutoExpandGroups = true;
            this.dgCTasks.AutoFitGroupDropAreaItem = true;
            this.dgCTasks.AutoGenerateColumns = false;
            this.dgCTasks.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.Fill;
            gridTextColumn5.AllowEditing = false;
            gridTextColumn5.AllowGrouping = false;
            gridTextColumn5.AllowResizing = true;
            gridTextColumn5.HeaderText = "ID";
            gridTextColumn5.MappingName = "ID";
            gridTextColumn5.MaximumWidth = 50D;
            gridTextColumn5.MinimumWidth = 50D;
            gridTextColumn6.AllowEditing = false;
            gridTextColumn6.AllowGrouping = false;
            gridTextColumn6.AllowResizing = true;
            gridTextColumn6.HeaderText = "Task Name";
            gridTextColumn6.MappingName = "Name";
            gridTextColumn6.MaximumWidth = 300D;
            gridTextColumn6.MinimumWidth = 200D;
            gridTextColumn7.AllowEditing = false;
            gridTextColumn7.AllowGrouping = false;
            gridTextColumn7.AllowResizing = true;
            gridTextColumn7.HeaderText = "Initiated At";
            gridTextColumn7.MappingName = "Initiated";
            gridTextColumn7.MaximumWidth = 120D;
            gridTextColumn7.MinimumWidth = 120D;
            gridTextColumn8.AllowEditing = false;
            gridTextColumn8.AllowGrouping = false;
            gridTextColumn8.AllowResizing = true;
            gridTextColumn8.HeaderText = "Ended At";
            gridTextColumn8.MappingName = "Ended";
            gridTextColumn8.MaximumWidth = 120D;
            gridTextColumn8.MinimumWidth = 120D;
            gridTextColumn9.AllowEditing = false;
            gridTextColumn9.AllowGrouping = false;
            gridTextColumn9.AllowResizing = true;
            gridTextColumn9.HeaderText = "Status";
            gridTextColumn9.MappingName = "Status";
            gridTextColumn9.MaximumWidth = 96D;
            gridTextColumn9.MinimumWidth = 96D;
            this.dgCTasks.Columns.Add(gridTextColumn5);
            this.dgCTasks.Columns.Add(gridTextColumn6);
            this.dgCTasks.Columns.Add(gridTextColumn7);
            this.dgCTasks.Columns.Add(gridTextColumn8);
            this.dgCTasks.Columns.Add(gridTextColumn9);
            this.dgCTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgCTasks.Location = new System.Drawing.Point(0, 0);
            this.dgCTasks.Name = "dgCTasks";
            this.dgCTasks.Size = new System.Drawing.Size(588, 403);
            this.dgCTasks.Style.CellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.dgCTasks.Style.CellStyle.Font.Bold = true;
            this.dgCTasks.Style.CellStyle.Font.Facename = "Palatino Linotype";
            this.dgCTasks.Style.CellStyle.Font.Size = 10F;
            this.dgCTasks.Style.CellStyle.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.dgCTasks.Style.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(200)))));
            this.dgCTasks.Style.HeaderStyle.FilterIconColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(29)))), ((int)(((byte)(29)))));
            this.dgCTasks.Style.HeaderStyle.Font.Bold = true;
            this.dgCTasks.Style.HeaderStyle.Font.Facename = "Palatino Linotype";
            this.dgCTasks.Style.HeaderStyle.Font.Size = 12F;
            this.dgCTasks.Style.HeaderStyle.Font.Strikeout = false;
            this.dgCTasks.Style.HeaderStyle.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(190)))));
            this.dgCTasks.TabIndex = 0;
            // 
            // TaskPreviewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(222)))));
            this.ClientSize = new System.Drawing.Size(598, 449);
            this.Controls.Add(this.tcMain);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Palatino Linotype", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IconSize = new System.Drawing.Size(30, 30);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.MaximizeBox = false;
            this.Name = "TaskPreviewer";
            this.ShowToolTip = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Style.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(222)))));
            this.Style.InactiveShadowOpacity = ((byte)(20));
            this.Style.MdiChild.IconHorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.Style.MdiChild.IconVerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
            this.Style.ShadowOpacity = ((byte)(120));
            this.Style.TitleBar.BackColor = System.Drawing.Color.Khaki;
            this.Style.TitleBar.BottomBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.Style.TitleBar.CloseButtonForeColor = System.Drawing.Color.Black;
            this.Style.TitleBar.CloseButtonHoverBackColor = System.Drawing.Color.Red;
            this.Style.TitleBar.CloseButtonHoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.Style.TitleBar.CloseButtonPressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Style.TitleBar.CloseButtonPressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.Style.TitleBar.CloseButtonSize = new System.Drawing.Size(45, 30);
            this.Style.TitleBar.Font = new System.Drawing.Font("Palatino Linotype", 14F, System.Drawing.FontStyle.Bold);
            this.Style.TitleBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.Style.TitleBar.Height = 35;
            this.Style.TitleBar.MinimizeButtonSize = new System.Drawing.Size(45, 30);
            this.Style.TitleBar.TextHorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.Text = "Command & Task Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.tcMain)).EndInit();
            this.tcMain.ResumeLayout(false);
            this.tpOngoing.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgATasks)).EndInit();
            this.tpCompleted.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgCTasks)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TabControlAdv tcMain;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tpOngoing;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tpCompleted;
        private Syncfusion.WinForms.DataGrid.SfDataGrid dgCTasks;
        private Syncfusion.WinForms.DataGrid.SfDataGrid dgATasks;
    }
}