using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using PERevitTab.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SP = Microsoft.SharePoint.Client;

namespace PERevitTab.Commands.DT.UDP
{
    class RevitMethods
    {
        public static IList<Element> CollectRooms(Document doc)
        {
            IList<Element> rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .ToElements();

            return rooms;
        }
        public static List<object> ParseRoomData(IList<Element> rooms)
        {
            List<object> roomsList = new List<object>();
            foreach (SpatialElement r in rooms)
            {
                Dictionary<string, string> roomsInfo = new Dictionary<string, string>();
                roomsInfo["RoomName"] = r.Name;
                roomsInfo["RoomNumber"] = r.Number;
                roomsInfo["RoomArea"] = r.Area.ToString();
                Room ro = (Room)r;
                roomsInfo["RoomVolume"] = ro.Volume.ToString();
                roomsList.Add(roomsInfo);
            }
            return roomsList;
        }
        public static ExternalDefinition AddSharedParameter(Document doc, Application app, string name, ParameterType type, BuiltInCategory cat, BuiltInParameterGroup group, bool instance)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Add shared parameter");
                try
                {
                    // save the path to the current shared parameters file
                    string oriFile = app.SharedParametersFilename;
                    // initialize a temporary shared param file
                    string tempFile = Path.GetTempFileName() + ".txt";

                    // assign the temporary shared param file to the current application
                    using (File.Create(tempFile)) { }
                    app.SharedParametersFilename = tempFile;

                    // initialize some external definition creation options, passing in our param name and type
                    var defOptions = new ExternalDefinitionCreationOptions(name, type)
                    {
                        Visible = true,
                    };

                    // add the shared parameter to the temporary shared parameter file
                    ExternalDefinition def = app
                        .OpenSharedParameterFile()
                        .Groups
                        .Create("UDP Parameters") // create a new shared parameter group called UDP parameters
                        .Definitions
                        .Create(defOptions) as ExternalDefinition; // add the shared param using the options specified above

                    // reset the application's shared parameter file to the original
                    app.SharedParametersFilename = oriFile;
                    // delete the temporary shared param file
                    File.Delete(tempFile);

                    // create a new category set and add our category to the category set
                    CategorySet categorySet = app.Create.NewCategorySet();
                    Category category = doc.Settings.Categories.get_Item(cat);
                    categorySet.Insert(category);

                    // initialize a binding to our category set
                    Autodesk.Revit.DB.Binding binding = app.Create.NewTypeBinding(categorySet);
                    if (instance) binding = app.Create.NewInstanceBinding(categorySet); // initialize an instance binding if necessary

                    // bind our new shared parameter to the category set
                    BindingMap map = (doc.ParameterBindings);
                    
                    // check if the binding is successful
                    if (!map.Insert(def, binding, group))
                    {
                        TaskDialog.Show("Error", $"Failed to create Project parameter '{name}' :(");
                        t.RollBack();
                    }
                    t.Commit();
                    return def; // return the external definition
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Error", e.ToString());
                    t.RollBack();
                    return null;
                }
            }
        }

        public static void CheckSPSync(Document doc)
        {
            // TODO add method to check if document has been synced to SP or not by looking up a "synced" shared parameter of the category project_info
            ProjectInfo pInfo = doc.ProjectInformation;
            TaskDialog.Show("Test", pInfo.Name);
        }

        public static Phase GetLatestPhase(Document doc)
        {
            PhaseArray allPhases = doc.Phases;
            return allPhases.get_Item(allPhases.Size - 1);
        }
        public static bool GenerateRooms(Document doc, Phase phase, Dictionary<string, SP.ListItemCollection> SPListItems, Dictionary<string, ExternalDefinition> parameterList)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Make Sharepoint Rooms");
                try
                {
                    // iterate over items
                    foreach (SP.ListItem listItem in SPListItems["readListItems"])
                    {
                        // make a new room for each item
                        Room newRoom = doc.Create.NewRoom(phase);

                        // loop through shared parameter definitions we created
                        foreach (KeyValuePair<string, ExternalDefinition> paramDef in parameterList)
                        {
                            // check if the parameter name exists as a column header in sharepoint data
                            if (listItem.FieldValues.ContainsKey(paramDef.Key))
                            {
                                // get the matching value of the column header, or null
                                var pValue = listItem[paramDef.Key] ?? null;

                                // retrieve the matching parameter in the room object
                                Parameter p = newRoom.get_Parameter(paramDef.Value);

                                // check the parameter type of the retrieved parameter and set the parameter value appropriately
                                switch (p.Definition.ParameterType)
                                {
                                    case ParameterType.Text:
                                        if (pValue != null) p.Set(pValue.ToString());
                                        break;
                                    case ParameterType.MultilineText:
                                        if (pValue != null) p.Set(pValue.ToString());
                                        break;
                                    case ParameterType.YesNo:
                                        if (pValue.ToString() == "Yes") p.Set(1);
                                        else if (pValue.ToString() == "No") p.Set(0);
                                        break;
                                    case ParameterType.Number:
                                        if (pValue != null) p.Set(Convert.ToDouble(pValue));
                                        break;
                                    case ParameterType.Integer:
                                        if (pValue != null) p.Set(Convert.ToInt32(pValue));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        // lastly, set the room's name to the value of the Title column if it exists
                        if (listItem.FieldValues.ContainsKey("Title")) newRoom.Name = listItem["Title"].ToString();
                    }
                    t.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Error", e.ToString());
                    t.RollBack();
                    return false;
                }
            }
        }
    }
}
