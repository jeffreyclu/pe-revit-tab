#region autodesk libraries
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

#region system libraries
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Excel = Microsoft.Office.Interop.Excel;
#endregion

namespace PERevitTab.Forms
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class WorksetCreator : System.Windows.Forms.Form
    {
        #region class variables
        private object[] worksetNames = new object[] { "New Workset 1", "New Workset 2", "New Workset 3" };
        private List<string> selectedWorksetNames = new List<string>();
        private ExternalCommandData extCommandData;
        #endregion
        #region main functions
        public WorksetCreator(ExternalCommandData commandData)
        {
            extCommandData = commandData;
            InitializeComponent();
        }

        private void WorksetCreator_Load(object sender, EventArgs e)
        {
            try
            {
                worksetNames = GenerateWorksetNames();
                foreach (object w in worksetNames)
                {
                    this.checkedListBox1.Items.Add(w, CheckState.Checked);
                }
            }
            catch (Exception err)
            {
                TaskDialog.Show("Error", err.ToString());
            }
        }

        private object[] GenerateWorksetNames()
        {
            // Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("pack://application:,,,/PERevitTab;component/Assets/Excel/worksets.xlsx");
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            List<string> worksetNames = new List<string>();

            for (int i = 2; i <= 100; i++)
            {
                if (xlRange.Cells[i, 1] != null && xlRange.Cells[i, 1].Value2 != null)
                {
                    worksetNames.Add(xlRange.Cells[i, 1].Value2.ToString());
                }
                else
                {
                    break;
                }
            }

            //close and release
            xlWorkbook.Close();
            //quit and release
            xlApp.Quit();
            return worksetNames.Cast<object>().ToArray();
        }
        #endregion
        #region event handlers
        private void button1_Click(object sender, EventArgs e)
        {
            Document doc = extCommandData.Application.ActiveUIDocument.Document;
            using (Transaction t = new Transaction(doc, "Making Worksets"))
            {
                try
                {
                    t.Start();
                    IList<Workset> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).Where(w => w.Name == "Workset1").ToList();
                    WorksetId workset1Id = worksets[0].Id;
                    for (int i = 0; i < selectedWorksetNames.Count; i++)
                    {
                        if (WorksetTable.IsWorksetNameUnique(doc, selectedWorksetNames[i].ToString()))
                        {
                            if (i == 0)
                            {
                                WorksetTable.RenameWorkset(doc, workset1Id, selectedWorksetNames[i].ToString());
                            }
                            else
                            {
                                Workset.Create(doc, selectedWorksetNames[i].ToString());
                            }
                        }
                        else
                        {
                            throw new Exception("Duplicate Workset Name");
                        }

                    }
                    t.Commit();
                    TaskDialog.Show("Success", "Worksets created.");

                }
                catch (Exception error)
                {
                    t.RollBack();
                    TaskDialog.Show("Error", error.ToString());
                }
            }
            this.Close();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                selectedWorksetNames.Add(checkedListBox1.Items[e.Index].ToString());
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                selectedWorksetNames.Remove(checkedListBox1.Items[e.Index].ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
