using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PERevitTab.Forms
{
    public partial class ProgressDialog : Form
    {
        private int _increment { get; set; }
        public ProgressDialog(string name, int totalTasks)
        {
            Text = name;
            _increment = (int)Math.Floor(100 / (double)totalTasks);
            InitializeComponent();
        }

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            // set progressbar initial values
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            // set label initial values
            progressLabel.Text = $"{progressBar.Value}%";
            taskLabel.Text = "Current Task:";
        }

        public void Increment()
        {
            // increment the progress bar value
            progressBar.Value += _increment;

            // update the progress percentage 
            progressLabel.Text = $"{progressBar.Value}%";
            progressLabel.Refresh();
        }

        public void StartTask(string taskName)
        {
            // check if taskName is greater than 35 characters and slice it if it is
            string taskLabelText = taskName.Length > 35 ? $"{taskName.Substring(0, 31)} ..." : taskName;

            // update tasklabel
            taskLabel.Text = $"Current Task: {taskLabelText}";
            taskLabel.Refresh();
        }
    }
}
