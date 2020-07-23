#region autodesk libraries
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PERevitTab.Data;
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
        private Dictionary<string, List<object>> worksetDictionary = new Dictionary<string, List<object>>();
        private readonly List<string> selectedWorksetNames = new List<string>();
        private readonly Document doc;
        private string filePath;
        private readonly List<string> illegalChars = new List<string>()
        {
            "\\", ":", "{", "}", "[", "]", "|", ";", "<", ">", "?", "`", "~"
        };
        #endregion
        #region main functions
        public WorksetCreator(Document extDoc)
        {
            doc = extDoc;
            InitializeComponent();
        }

        private void WorksetCreator_Load(object sender, EventArgs e)
        {
            // MessageBox.Show("Select a workset list", "Workset Creator");
            label3.Visible = false;
            listBox1.Visible = false;
            openFileDialog1.InitialDirectory =WorksetCreatorConstants.worksetTemplatesLocation;
            openFileDialog1.Title = "Select workset template:";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (ProgressDialog pd = new ProgressDialog("Workset Creator", 5))
                    {
                        pd.Show();
                        // get file path from the file dialog
                        pd.StartTask("Opening excel file.");
                        filePath = openFileDialog1.FileName;
                        if (filePath == null)
                        {
                            TaskDialog.Show("Error", "Illegal filepath.");
                            pd.Close();
                            Close();
                            return;
                        }
                        pd.Increment();

                        // open the excel file and extract workset names
                        pd.StartTask("Getting workset names.");
                        worksetDictionary = GenerateWorksetNames(filePath);
                        if (worksetDictionary[WorksetCreatorConstants.newWorksets].Count == 0 && worksetDictionary[WorksetCreatorConstants.existingWorksets].Count == 0)
                        {
                            TaskDialog.Show("Error", "No workset names found. Aborting.");
                            pd.Close();
                            Close();
                            return;
                        }
                        else if (worksetDictionary[WorksetCreatorConstants.newWorksets].Count == 0 && worksetDictionary[WorksetCreatorConstants.existingWorksets].Count > 0)
                        {
                            TaskDialog.Show("Error", "No new workset names found. Aborting.");
                            pd.Close();
                            Close();
                            return;
                        }
                        pd.Increment();

                        pd.StartTask("Checking for existing worksets");
                        if (worksetDictionary[WorksetCreatorConstants.existingWorksets].Count > 0)
                        {
                            TaskDialog.Show("Warning", $"{worksetDictionary[WorksetCreatorConstants.existingWorksets].Count} worksets already exist. These will be ignored.");
                            checkedListBox1.Height = 364;
                            label3.Visible = true;
                            listBox1.Visible = true;
                            foreach (object w in worksetDictionary[WorksetCreatorConstants.existingWorksets])
                            {
                                listBox1.Items.Add(w);
                            }
                        }
                        pd.Increment();

                        pd.StartTask("Checking workset names.");
                        foreach (object w in worksetDictionary[WorksetCreatorConstants.newWorksets])
                        {
                            foreach (string c in illegalChars)
                            {
                                if (w.ToString().Contains(c)) 
                                {
                                    TaskDialog.Show("Error", $"Illegal character detected in workset name '{w}'. Workset name cannot contain any of the following chracters: \\ : {{ }} [ ] | ; < > ? ` ~");
                                    pd.Close();
                                    Close();
                                    return;
                                }
                            }
                            checkedListBox1.Items.Add(w, CheckState.Checked);
                        }
                        pd.Increment();
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.ToString(), "Error:");
                }
            }
            else
            {
                this.Close();
            }
        }

        private Dictionary<string, List<object>> GenerateWorksetNames(string path)
        {
            // Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            // initialize a dictionary of object[] to hold workset names
            Dictionary<string, List<object>> worksetDictionary = new Dictionary<string, List<object>>()
            {
                { WorksetCreatorConstants.newWorksets, new List<object>() },
                { WorksetCreatorConstants.existingWorksets, new List<object>() },
            };

            // collect all user created worksets from the revit document
            FilteredWorksetCollector revitWorksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset);

            // iterate through the excel sheet
            for (int i = 2; i <= 100; i++)
            {
                // check that we don't have a null
                if (xlRange.Cells[i, 1] != null && xlRange.Cells[i, 1].Value2 != null)
                {
                    // save the workset name as a variable
                    string worksetName = xlRange.Cells[i, 1].Value2.ToString();

                    // see if the excel workset already exists in revit
                    if (revitWorksets.Where(w => w.Name == worksetName).FirstOrDefault() == null)
                    {
                        worksetDictionary[WorksetCreatorConstants.newWorksets].Add(worksetName);
                    }
                    else
                    {
                        worksetDictionary[WorksetCreatorConstants.existingWorksets].Add(worksetName);
                    }
                }
                else 
                {
                    // break if there's a null
                    break;
                }
            }

            // close workbook and release
            xlWorkbook.Close();
            // quit excel and release
            xlApp.Quit();

            return worksetDictionary;
        }
        #endregion
        #region event handlers
        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedWorksetNames.Count == 0)
            {
                TaskDialog.Show("Error", "No worksets selected.");
                return;
            }
            // Document doc = extCommandData.Application.ActiveUIDocument.Document;
            using (Transaction t = new Transaction(doc, "Making Worksets"))
            {
                try
                {
                    t.Start();
                    // Workset workset1 = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).Where(w => w.Name == "Workset1").FirstOrDefault();
                    // WorksetId workset1Id = workset1?.Id;
                    for (int i = 0; i < selectedWorksetNames.Count; i++)
                    {
                        if (WorksetTable.IsWorksetNameUnique(doc, selectedWorksetNames[i].ToString()))
                        {
                            /*
                            if (i == 0 && workset1Id != null)
                            {
                                WorksetTable.RenameWorkset(doc, workset1Id, selectedWorksetNames[i].ToString());
                            }
                            else
                            {
                                Workset.Create(doc, selectedWorksetNames[i].ToString());
                            }
                            */
                            Workset.Create(doc, selectedWorksetNames[i].ToString());
                        }
                        else
                        {
                            TaskDialog.Show("Error", $"Duplicate workset name already exists in project: {selectedWorksetNames[i]}");
                        }

                    }
                    t.Commit();
                    TaskDialog.Show("Success", $"{selectedWorksetNames.Count} worksets created.");

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
