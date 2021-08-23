using Syncfusion.WinForms.Controls;
using Controller.Codes;
using System.Drawing;

namespace Controller.Popups
{
    public partial class TaskPreviewer : SfForm
    {
        public TaskPreviewer(Point StartLocation)
        {
            InitializeComponent();
            EnforceExtraDesigns();
            Location = StartLocation;
            Tasker.PendingTasks.SetDatagridView(dgATasks);
            Tasker.CompletedTasks.SetDatagridView(dgCTasks);
        }

        private void EnforceExtraDesigns()
        {
            dgCTasks.Columns[1].MinimumWidth += 12;
            dgATasks.Columns[1].MinimumWidth += 12;
            tcMain.InActiveTabForeColor = Color.White;
            tcMain.InactiveTabColor = Color.FromArgb(24, 24, 0);
            tcMain.ActiveTabColor = Color.FromArgb(128, 64, 64);
            tcMain.TabPanelBackColor = Color.FromArgb(255, 255, 222);
            tcMain.ActiveTabForeColor = Color.FromArgb(255, 224, 192);

            // Tasker.EnqueueTask("L|P", true); await Task.Delay(500);
        }
    }
}
