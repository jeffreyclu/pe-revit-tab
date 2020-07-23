#region autodesk libraries
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
#endregion

#region system libraries
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

using PERevitTab.CommonMethods;
using PERevitTab.Data;

namespace PERevitTab.Commands.DT
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class ModelMetrics : IExternalCommand // the actual plugin "clickable" button implemented using the ExternalCommand interface
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) // IExternalCommand has a single method Execute() which returns a Result that can be failed, succeeded, or cancelled
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument; // access the UI doc
            Application app = uidoc.Application.Application; // access the application
            Document doc = uidoc.Document; // access the current doc

            try
            {
                Dictionary<string, object> modelMetrics = CollectModelData(doc, app);
                UtilityMethods.WriteCsv(modelMetrics, $"{doc.Title}-model-metrics");
                TaskDialog.Show("Success", $"Model Data Collected.");
                return Result.Succeeded; // try collecting data and write collected data to csv
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", "Error logged. Contact DT for details.");
                Dictionary<string, object> error = UtilityMethods.LogError(e);
                UtilityMethods.WriteCsv(error, $"{doc.Title}-debugging");
                return Result.Failed; // if anything failed, display the error to the user and log the error
            }
        }
        /// <summary>
        /// Collects model metrics and returns a dictionary
        /// </summary>
        /// <param name="doc">The active document</param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> CollectModelData(Document doc, Application app)
        {

            /* 
             * the basic strategy is to use filtered element collectors to collect the relevant data. 
             * some helper functions are defined in order to collect more complicated metrics that require the use of several element collectors.
             * examples: unused elements and redundant elements require helper functions 
             */

            IList<FailureMessage> warnings = doc.GetWarnings();
            IList<Element> linkedRVT = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToList();
            Dictionary<string, IList<Element>> nonRvtLinks = RevitMethods.NonRVTLinkCollector(doc);
            IList<Element> linkedCAD = nonRvtLinks[RevitConstants.Strings.linkedCAD];
            IList<Element> importedCAD = nonRvtLinks[RevitConstants.Strings.importedCAD];
            IList<Element> importedSKP = nonRvtLinks[RevitConstants.Strings.importedSKP];
            IList<Element> raster = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RasterImages).WhereElementIsNotElementType().ToList();
            IList<Element> sheets = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType().ToList();
            IList<View> views = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).Cast<View>().Where(v => !v.IsTemplate).ToList();
            IList<View> unplacedViews = RevitMethods.UnplacedViewCollector(doc, sheets);
            IList<Element> modelGroups = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSModelGroups).WhereElementIsNotElementType().ToList();
            IList<Element> detailGroups = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSDetailGroups).WhereElementIsNotElementType().ToList();
            IList<Family> inPlaceFamilies = new FilteredElementCollector(doc).OfClass(typeof(Family)).Cast<Family>().Where(f => f.IsInPlace).ToList();
            IList<Element> designOptionElements = new FilteredElementCollector(doc).WhereElementIsNotElementType().Where(e => e.DesignOption != null).ToList();
            IList<Workset> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToList();
            IList<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToList();
            IList<SpatialElement> unplacedRooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().Cast<SpatialElement>().Where(r => r.Area == 0 && r.Location == null).ToList();
            IList<SpatialElement> redundantRooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().Cast<SpatialElement>().Where(r => r.Area == 0 && r.Location != null).ToList();
            IList<Element> redundantElements = RevitMethods.RedundantElementsCollector(warnings, doc);
            int unusedElements = unplacedViews.Count + unplacedRooms.Count + RevitMethods.UnusedFamiliesCollector(doc).Count;

            // collect model specific information
            (string writeDate, string writeTime) = UtilityMethods.FormatDateTime();
            (string modelPath, string modelName, string modelSize) = RevitMethods.GetModelInfo(doc);
            string versionBuild = app.VersionBuild;

            // save collected data in a dictionary
            Dictionary<string, object> modelMetrics = new Dictionary<string, object>() 
            { 
                { "Model Name", modelName },
                { "Model Path", modelPath },
                { "Model Size", modelSize },
                { "Warnings", warnings },
                { "Linked RVT Files", linkedRVT },
                { "Linked CAD Files", linkedCAD },
                { "Imported CAD Files", importedCAD },
                { "Imported SKP Files", importedSKP },
                { "Raster Images", raster },
                { "Sheets", sheets },
                { "Views", views },
                { "Unplaced Views", unplacedViews },
                { "Model Groups", modelGroups },
                { "Detail Groups", detailGroups },
                { "In-place Families", inPlaceFamilies },
                { "Elements in Design Options", designOptionElements },
                { "Worksets", worksets },
                { "Rooms", rooms },
                { "Unplaced Rooms", unplacedRooms },
                { "Redundant and Unenclosed Rooms", redundantRooms },
                { "Redundant Elements", redundantElements },
                { "Unused Elements", unusedElements },
                { "Write Date", writeDate },
                { "Write Time", writeTime },
                { "Version Build", versionBuild },
            };

            return modelMetrics;
        }
        #region todos
        /*
        public void GetGroupsInfo(Document doc, Dictionary<string, object> data)
        {
            (string writeDate, string writeTime) = FormatDateTime();
            string csvPath = $"C:\\Users\\j.lu\\desktop\\{writeDate}_groupsdata.csv";

            foreach (Element g in modelGroups)
            {
                WorksharingTooltipInfo tooltipInfo = WorksharingUtils.GetWorksharingTooltipInfo(doc, g.Id);
                string creator = tooltipInfo.Creator;
                string owner = tooltipInfo.Owner;
                string lastChangedBy = tooltipInfo.LastChangedBy;
                LocationPoint origin = g.Location as LocationPoint;
                output.AppendLine(
                    $"{g.Id.IntegerValue}, " +
                    $"{g.Name}, " +
                    $"{origin.Point.ToString()}, " +
                    $"{creator}, " +
                    $"{lastChangedBy}"
                    );
            }

            foreach (Element g in detailGroups)
            {
                WorksharingTooltipInfo tooltipInfo = WorksharingUtils.GetWorksharingTooltipInfo(doc, g.Id);
                string creator = tooltipInfo.Creator;
                string owner = tooltipInfo.Owner;
                string lastChangedBy = tooltipInfo.LastChangedBy;
                LocationPoint origin = g.Location as LocationPoint;
                output.AppendLine(
                    $"{g.Id.IntegerValue}, " +
                    $"{g.Name}, " +
                    $"{origin.Point.ToString()}, " +
                    $"{creator}, " +
                    $"{lastChangedBy}"
                    );
            }

            string headers =
                "Element ID, " +
                "Element Name, " +
                "Creator, " +
                "Last Modified By, " +
                "Write Time, " +
                Environment.NewLine; // generate csv headers

            output.Insert(0, headers); // prepend headers to output stringbuilder
            File.WriteAllText(csvPath, output.ToString()); // write output to file
        }
        */
        /*
        public void GetAllElements(Document doc)
        {
            DateTime localDate = DateTime.Now;
            string writeDate = localDate.ToString("yyyy-MM-dd");
            string writeTime = localDate.ToString("HH:mm:ss");
            string csvPath = $"C:\\Users\\j.lu\\desktop\\{writeDate}_elementsdata.csv";
            string modelPath = doc.PathName;
            string modelName = doc.Title;
            StringBuilder output = new StringBuilder();

            var allElements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements();

            foreach(Element e in allElements)
            {
                WorksharingTooltipInfo tooltipInfo = WorksharingUtils.GetWorksharingTooltipInfo(doc, e.Id);
                string creator = tooltipInfo.Creator;
                string owner = tooltipInfo.Owner;
                string lastChangedBy = tooltipInfo.LastChangedBy;
                string location = "unknown";
                if (e.Location != null)
                {
                    LocationPoint origin = e.Location as LocationPoint;
                    if (origin != null)
                    {
                        location = origin.Point.ToString();
                    }
                }

                output.AppendLine(
                  $"{e.Id.IntegerValue}, " +
                  $"{e.Name}, " +
                  $"{location}, " +
                  $"{creator}, " +
                  $"{lastChangedBy}, " +
                  $"{e.GetType()}, " +
                  $"{e.GetType().Name}, " +
                  $"{e.GetType().FullName}"
                  );
            }
            File.WriteAllText(csvPath, output.ToString());
        }
        */
        #endregion
    }
}
