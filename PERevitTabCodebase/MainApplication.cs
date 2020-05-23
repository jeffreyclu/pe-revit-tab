#region autodesk libraries
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
#endregion
#region system libraries
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Reflection;
#endregion

namespace PERevitTab
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MainApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                #region initial setup
                // retrieve assembly path
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // create the ribbon tab
                string tabName = "PE Revit Tools";
                application.CreateRibbonTab(tabName);

                // create the DT panel
                string dtPanelName = "DT Tools";
                RibbonPanel dtPanel = application.CreateRibbonPanel(tabName, dtPanelName);

                //create the DU panel
                string duPanelName = "DU Tools";
                RibbonPanel duPanel = application.CreateRibbonPanel(tabName, duPanelName);
                #endregion

                #region DT panel
                // button 1 image
                BitmapImage modelMetricsImg = new BitmapImage(new Uri(@"\\d-peapcny.net\enterprise\G_Gen-Admin\Committees\Data Unit\01_TEAMS\AUTOMATION\PE Revit Tab\PERevitTabCodebase\PERevitTabCodebase\Assets\Images\modelMetricsBtn.png")); // add the uri

                // button 2 image
                BitmapImage createWorksetsImg = new BitmapImage(new Uri(@"\\d-peapcny.net\enterprise\G_Gen-Admin\Committees\Data Unit\01_TEAMS\AUTOMATION\PE Revit Tab\PERevitTabCodebase\PERevitTabCodebase\Assets\Images\worksetCreatorBtn.png")); // add the uri

                // button 1
                PushButtonData modelMetricsData = new PushButtonData("Model Metrics", "Run \n" + "Model Metrics", assemblyPath, "PERevitTab.Commands.DT.ModelMetrics");
                PushButton modelMetrics = dtPanel.AddItem(modelMetricsData) as PushButton;
                modelMetrics.LargeImage = modelMetricsImg;
                modelMetrics.ToolTip = "Collects model metrics";

                // button 2
                PushButtonData createWorksetsData = new PushButtonData("Create Worksets", "Create \n" + "Worksets", assemblyPath, "PERevitTab.Commands.DT.CreateWorksets");
                PushButton createWorksets = dtPanel.AddItem(createWorksetsData) as PushButton;
                createWorksets.LargeImage = createWorksetsImg;
                createWorksets.ToolTip = "Creates worksets";
                #endregion

                #region DU panel
                // button 1 image
                BitmapImage runDynamo1Img = new BitmapImage(new Uri(@"\\d-peapcny.net\enterprise\G_Gen-Admin\Committees\Data Unit\01_TEAMS\AUTOMATION\PE Revit Tab\PERevitTabCodebase\PERevitTabCodebase\Assets\Images\duBtn.png")); // add the uri

                // button 2 image
                BitmapImage runDynamo2Img = new BitmapImage(new Uri(@"\\\\d-peapcny.net\enterprise\G_Gen-Admin\Committees\Data Unit\01_TEAMS\AUTOMATION\PE Revit Tab\PERevitTabCodebase\PERevitTabCodebase\Assets\Images\duBtn.png")); // add the uri

                // button 3 image
                BitmapImage runDynamo3Img = new BitmapImage(new Uri(@"\\\\d-peapcny.net\enterprise\G_Gen-Admin\Committees\Data Unit\01_TEAMS\AUTOMATION\PE Revit Tab\PERevitTabCodebase\PERevitTabCodebase\Assets\Images\duBtn.png")); // add the uri

                // button 1
                PushButtonData runDynamo1Data = new PushButtonData("Renumber Views", "Renumber \n" + "Views on Sheets", assemblyPath, "PERevitTab.Commands.DU.RunDynamo1");
                PushButton runDynamo1 = duPanel.AddItem(runDynamo1Data) as PushButton;
                runDynamo1.LargeImage = runDynamo1Img;
                runDynamo1.ToolTip = "Renumbers viewports on multiple sheets. The bottom left corners of Viewports within an inch in height are counted as a row. Numbers start from the bottom and goes right to left.";

                // button 2
                PushButtonData runDynamo2Data = new PushButtonData("Align Views", "Align \n" + "Views", assemblyPath, "PERevitTab.Commands.DU.RunDynamo2");
                PushButton runDynamo2 = duPanel.AddItem(runDynamo2Data) as PushButton;
                runDynamo2.LargeImage = runDynamo2Img;
                runDynamo2.ToolTip = "Aligns Views on multiple sheets to the location of the referenced view. Choose whether to align by the corner or the center of the views.";

                //button 3
                PushButtonData runDynamo3Data = new PushButtonData("Add Views to Sheets", "Add Views to \n" + "Sheets - Selection", assemblyPath, "PERevitTab.Commands.DU.RunDynamo3");
                PushButton runDynamo3 = duPanel.AddItem(runDynamo3Data) as PushButton;
                runDynamo3.LargeImage = runDynamo3Img;
                runDynamo3.ToolTip = "Choose a Sheet, then select view references (elevation and section tags, callouts) to be added to the sheet.";
                #endregion
            }
            catch (Exception err)
            {
                // TODO: add better error handling
                TaskDialog.Show("Error", err.Message.ToString());
            }
            return Result.Succeeded;
        }
    }
}
