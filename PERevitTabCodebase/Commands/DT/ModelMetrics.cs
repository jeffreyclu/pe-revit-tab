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

namespace PERevitTab.Commands.DT
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class ModelMetrics : IExternalCommand // the actual plugin "clickable" button implemented using the ExternalCommand interface
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) // IExternalCommand has a single method Execute() which returns a Result that can be failed, succeeded, or cancelled
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument; // access the UI doc
            Application app = uidoc.Application.Application;
            Document doc = uidoc.Document; // access the current doc

            try
            {
                Dictionary<string, object> modelMetrics = CollectModelData(doc, app);
                WriteCsv(modelMetrics, $"{doc.Title}-model-metrics");
                TaskDialog.Show("Success", $"Model Data Collected.");
                return Result.Succeeded; // try collecting data and write collected data to csv
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", "Error logged. Contact DT for details.");
                Dictionary<string, object> error = LogError(e);
                WriteCsv(error, $"{doc.Title}-debugging");
                return Result.Failed; // if anything failed, display the error to the user and log the error
            }
        }

        #region main function
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
            IList<Element> linkedCAD = NonRVTLinkCollector(doc)["Linked CAD"];
            IList<Element> importedCAD = NonRVTLinkCollector(doc)["Imported CAD"];
            IList<Element> importedSKP = NonRVTLinkCollector(doc)["Imported SKP"];
            IList<Element> raster = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RasterImages).WhereElementIsNotElementType().ToList();
            IList<Element> sheets = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType().ToList();
            IList<View> views = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).Cast<View>().Where(v => !v.IsTemplate).ToList();
            IList<View> unplacedViews = UnplacedViewCollector(doc, sheets);
            IList<Element> modelGroups = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSModelGroups).WhereElementIsNotElementType().ToList();
            IList<Element> detailGroups = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSDetailGroups).WhereElementIsNotElementType().ToList();
            IList<Family> inPlaceFamilies = new FilteredElementCollector(doc).OfClass(typeof(Family)).Cast<Family>().Where(f => f.IsInPlace).ToList();
            IList<Element> designOptionElements = new FilteredElementCollector(doc).WhereElementIsNotElementType().Where(e => e.DesignOption != null).ToList();
            IList<Workset> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToList();
            IList<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToList();
            IList<SpatialElement> unplacedRooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().Cast<SpatialElement>().Where(r => r.Area == 0 && r.Location == null).ToList();
            IList<SpatialElement> redundantRooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().Cast<SpatialElement>().Where(r => r.Area == 0 && r.Location != null).ToList();
            IList<Element> redundantElements = RedundantElementsCollector(warnings, doc);
            int unusedElements = unplacedViews.Count + unplacedRooms.Count + UnusedFamiliesCollector(doc).Count;

            // collect model specific information
            (string writeDate, string writeTime) = FormatDateTime();
            (string modelPath, string modelName, string modelSize) = GetModelInfo(doc);
            string versionBuild = app.VersionBuild;

            // save collected data in a dictionary
            Dictionary<string, object> modelMetrics = new Dictionary<string, object>();
            modelMetrics.Add("Model Name", modelName);
            modelMetrics.Add("Model Path", modelPath);
            modelMetrics.Add("Model Size", modelSize);
            modelMetrics.Add("Warnings", warnings);
            modelMetrics.Add("Linked RVT Files", linkedRVT);
            modelMetrics.Add("Linked CAD Files", linkedCAD);
            modelMetrics.Add("Imported CAD Files", importedCAD);
            modelMetrics.Add("Imported SKP Files", importedSKP);
            modelMetrics.Add("Raster Images", raster);
            modelMetrics.Add("Sheets", sheets);
            modelMetrics.Add("Views", views);
            modelMetrics.Add("Unplaced Views", unplacedViews);
            modelMetrics.Add("Model Groups", modelGroups);
            modelMetrics.Add("Detail Groups", detailGroups);
            modelMetrics.Add("In-place Families", inPlaceFamilies);
            modelMetrics.Add("Elements in Design Options", designOptionElements);
            modelMetrics.Add("Worksets", worksets);
            modelMetrics.Add("Rooms", rooms);
            modelMetrics.Add("Unplaced Rooms", unplacedRooms);
            modelMetrics.Add("Redundant and Unenclosed Rooms", redundantRooms);
            modelMetrics.Add("Redundant Elements", redundantElements);
            modelMetrics.Add("Unused Elements", unusedElements);
            modelMetrics.Add("Write Date", writeDate);
            modelMetrics.Add("Write Time", writeTime);
            modelMetrics.Add("Version Build", versionBuild);

            return modelMetrics;
        }
        #endregion
        #region helper functions
        /// <summary>
        /// Collects non-RVT link and imports
        /// </summary>
        /// <param name="doc">The active document</param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, IList<Element>> NonRVTLinkCollector(Document doc)
        {
            IList<Element> linkedCAD = new List<Element>();
            IList<Element> importedCAD = new List<Element>();
            IList<Element> importedSKP = new List<Element>();
            Dictionary<string, IList<Element>> linkDictionary = new Dictionary<string, IList<Element>>();
            linkDictionary.Add("Linked CAD", linkedCAD);
            linkDictionary.Add("Imported CAD", importedCAD);
            linkDictionary.Add("Imported SKP", importedSKP);

            IList<Element> importInstances = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)).WhereElementIsNotElementType().ToList();
            foreach (Element i in importInstances)
            {
                CADLinkType linkType = doc.GetElement(i.GetTypeId()) as CADLinkType;
                if (linkType.IsExternalFileReference())
                {
                    linkDictionary["Linked CAD"].Add(i);
                }
                else if (!linkType.IsExternalFileReference() && linkType.Name.Contains(".skp"))
                {
                    linkDictionary["Imported SKP"].Add(i);
                }
                else if (!linkType.IsExternalFileReference() && !linkType.Name.Contains(".skp"))
                {
                    linkDictionary["Imported CAD"].Add(i);
                }
            }
            return linkDictionary;
        }
        /// <summary>
        /// Collects unplaced views by cross-referencing all views against views that are on sheets
        /// </summary>
        /// <param name="doc">The active document</param>
        /// <param name="sheets">List of sheets</param>
        /// <returns>IList</returns>
        public IList<View> UnplacedViewCollector(Document doc, IList<Element> sheets)
        {
            ICollection<ElementId> placedViewIds = new List<ElementId>(); // initiliaze an empty collection to hold element ids
            foreach (ViewSheet sheet in sheets)
            {
                ISet<ElementId> currentPlacedViewIds = sheet.GetAllPlacedViews();
                foreach (ElementId eId in currentPlacedViewIds)
                {
                    placedViewIds.Add(eId); // add all placed ids of views placed on sheets to element ids collection
                }
            }
            ExclusionFilter placedViewsFilter = new ExclusionFilter(placedViewIds); // generate exclusion filter using placed view ids
            IList<View> unplacedViews = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WherePasses(placedViewsFilter).Cast<View>().Where(v => !v.IsTemplate).ToList(); // add exclusion filter to filtered element collector of views

            return unplacedViews;
        }

        /// <summary>
        /// Collects redundant elements based on the redundant elements warning
        /// </summary>
        /// <param name="warnings">List of warnings</param>
        /// <param name="doc">The active document</param>
        /// <returns>IList</returns>
        public IList<Element> RedundantElementsCollector(IList<FailureMessage> warnings, Document doc)
        {
            IList<Element> redundantElements = new List<Element>();
            ICollection<ElementId> redundantElementIds = new List<ElementId>(); // initalize empty list to hold redundant elements
            Guid redundantFailMsgGuid = new Guid("b4176cef-6086-45a8-a066-c3fd424c9412"); // guid corresponding to redundant element failure message
            foreach (FailureMessage failMsg in warnings)
            {
                if (failMsg.GetFailureDefinitionId().Guid == redundantFailMsgGuid) // check if elements causing warnings match the redundant element warning
                {
                    ICollection<ElementId> eleIds = failMsg.GetFailingElements();
                    foreach (ElementId eId in eleIds)
                    {
                        Element redundantElement = doc.GetElement(eId); // retrieve element based on its ID
                        redundantElements.Add(redundantElement); // add each element to the redundant elements list
                    }
                }
            }
            return redundantElements;
        }

        /// <summary>
        /// Gets unused family types (symbols)
        /// </summary>
        /// <param name="doc">The active document</param>
        /// <returns>IList</returns>
        public IList<Element> UnusedFamiliesCollector(Document doc)
        {
            IList<Element> familyInstances = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToList(); // get all family instances
            Dictionary<ElementId, IList<Element>> usedFamilyTypesDictionary = new Dictionary<ElementId, IList<Element>>(); // create a dictionary of type ids
            foreach (Element f in familyInstances)
            {
                ElementId key = f.GetTypeId(); // use the typeid as the key and check if the key exists in the dictionary
                if (!usedFamilyTypesDictionary.ContainsKey(key))
                {
                    IList<Element> elements = new List<Element>();
                    usedFamilyTypesDictionary.Add(key, elements); // initialize an entry with an empty list of elements as the value
                }
                usedFamilyTypesDictionary[key].Add(f); // add element to the list of elements at the key of the element's type id
            }
            IList<Element> unusedFamilySymbols = new List<Element>(); // initalize empty list of elements to hold unused family symbols
            IList<Element> familySymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).WhereElementIsElementType().ToList(); // get all family symbols
            foreach (Element f in familySymbols) // loop through family symbol types and retrieve the id for each family symbol
            {
                ElementId key = f.Id; // set the family symbol type id as the key to search for
                if (!usedFamilyTypesDictionary.ContainsKey(key)) // check the dictionary of placed family types contains the key
                {
                    unusedFamilySymbols.Add(f); // if no, add it to the list to return
                }
            }
            return unusedFamilySymbols;
        }
        #endregion
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
        #region utility functions
        /// <summary>
        /// Formats the current date and time in a human-readable format
        /// </summary>
        /// <returns>Tuple</returns>
        public (string date, string time) FormatDateTime()
        {
            DateTime localDate = DateTime.Now;
            string writeDate = localDate.ToString("yyyy-MM-dd");
            string writeTime = localDate.ToString("HH:mm:ss");
            return (writeDate, writeTime); // this function 
        }
        /// <summary>
        /// Attempts to retrieve model info such as model name, model path, and model size
        /// </summary>
        /// <param name="doc">The active document</param>
        /// <returns>Tuple</returns>
        public (string path, string name, string size) GetModelInfo(Document doc)
        {

            string modelPath = doc.PathName;
            if (doc.IsWorkshared)
            {
                modelPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath());
            }
            string modelName = doc.Title;
            string modelSize = "unknown";
            StringBuilder output = new StringBuilder();
            if (File.Exists(modelPath))
            {
                var f = new FileInfo(modelPath);
                modelSize = (f.Length / 1024 / 1024).ToString("#.##");
            }
            return (modelPath, modelName, modelSize);
        }
        /// <summary>
        /// Stores the error message, write date, and write time in a dictionary
        /// </summary>
        /// <param name="e">The error</param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> LogError(Exception e)
        {
            (string writeDate, string writeTime) = FormatDateTime();
            Dictionary<string, object> error = new Dictionary<string, object>();
            error.Add("Error", e.Message);
            error.Add("Write Date", writeDate);
            error.Add("Write Time", writeTime);
            return error; // this function logs the error message, write date, and write time to a CSV.
        }
        /// <summary>
        /// Writes a dictionary to a CSV file
        /// </summary>
        /// <param name="data">The data as a dictionary</param>
        /// <param name="fileName">The desired filename (without .csv)</param>
        public void WriteCsv(Dictionary<string, object> data, string fileName)
        {
            (string writeDate, string writeTime) = FormatDateTime();
            string csvPath = $"\\\\d-peapcny.net\\enterprise\\G_Gen-Admin\\Committees\\Design-Technology_Digital-Practice\\Revit Model Health\\_Database\\{writeDate}_{fileName}.csv";
            StringBuilder output = new StringBuilder();

            if (!File.Exists(csvPath))
            {
                string header = "";
                foreach (string key in data.Keys)
                {
                    header += $"{key}, ";
                }
                output.AppendLine(header.TrimEnd(','));
            }

            string currentLine = "";
            foreach (object value in data.Values)
            {

                if (value is IList<Element>)
                {
                    IList<Element> valueList = value as IList<Element>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<ElementId>)
                {
                    IList<ElementId> valueList = value as IList<ElementId>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is ICollection<Element>)
                {
                    ICollection<Element> valueCollection = value as ICollection<Element>;
                    currentLine += $"{valueCollection.Count}, ";
                }
                else if (value is IList<FailureMessage>)
                {
                    IList<FailureMessage> valueList = value as IList<FailureMessage>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<Family>)
                {
                    IList<Family> valueList = value as IList<Family>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<View>)
                {
                    IList<View> valueList = value as IList<View>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<SpatialElement>)
                {
                    IList<SpatialElement> valueList = value as IList<SpatialElement>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<Workset>)
                {
                    IList<Workset> valueList = value as IList<Workset>;
                    currentLine += $"{valueList.Count}, ";
                }
                else
                {
                    currentLine += $"{value}, ";
                }
            }
            output.AppendLine(currentLine.TrimEnd(','));
            File.AppendAllText(csvPath, output.ToString());
        }
        #endregion
    }
}
