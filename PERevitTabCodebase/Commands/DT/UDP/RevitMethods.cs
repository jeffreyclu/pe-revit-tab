#region autodesk libraries
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
#endregion

#region system libraries
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region microsoft libraries
using SP = Microsoft.SharePoint.Client;
#endregion

using PERevitTab.Data;

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
            // instantiate a list of objects to hold our room information (which will be a Dictionary<string, string>)
            List<object> roomsList = new List<object>();

            // loop through the collected rooms
            foreach (SpatialElement r in rooms)
            {
                // instantiate a dictionary to hold the SP column headers and the associated values
                Dictionary<string, string> roomsInfo = new Dictionary<string, string>();

                // first deal with the built-in parameters
                roomsInfo["vol_Title"] = r.Name;
                roomsInfo["revit_room_number"] = r.Number;
                roomsInfo["revit_room_element_id"] = r.Id.ToString();
                roomsInfo["comments"] = r.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();

                // next deal with the built-in spatial parameters
                Room ro = (Room)r;
                roomsInfo["revit_room_volume"] = ro.Volume.ToString();
                roomsInfo["revit_room_height"] = ro.UnboundedHeight.ToString();
                roomsInfo["area_net"] = r.Area.ToString();
                ro.Area.ToString();

                // lastly, deal with the custom parameters
                foreach (string paramName in SharepointConstants.Dictionaries.newRevitRoomParameters.Keys)
                {
                    Parameter p = r.LookupParameter(paramName);
                    switch (p.Definition.ParameterType)
                    {
                        case ParameterType.Text:
                            roomsInfo[paramName] = r.LookupParameter(paramName).AsString();
                            break;
                        case ParameterType.MultilineText:
                            roomsInfo[paramName] = r.LookupParameter(paramName).AsString();
                            break;
                        case ParameterType.YesNo:
                            roomsInfo[paramName] = r.LookupParameter(paramName).AsValueString();
                            break;
                        case ParameterType.Number:
                            roomsInfo[paramName] = r.LookupParameter(paramName).AsValueString();
                            break;
                        case ParameterType.Integer:
                            roomsInfo[paramName] = r.LookupParameter(paramName).AsValueString();
                            break;
                        default:
                            break;
                    }
                }
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
        public static Dictionary<string, ExternalDefinition> AddSharedParametersFromList(Document doc, Application app, Dictionary<string, ParameterType> parameterTypeMappings, BuiltInCategory category, BuiltInParameterGroup parameterGroup)
        {
            try
            {
                // initialize a dictionary to hold our parameter name and definitions
                Dictionary<string, ExternalDefinition> parameterDefinitions = new Dictionary<string, ExternalDefinition>();

                // iterate through the parameter type mappings dictionary
                foreach (KeyValuePair<string, ParameterType> paramDef in parameterTypeMappings)
                {

                    // call add shared parameter method for each key/value pair in our dictionary, returning an external definition
                    ExternalDefinition extDef = RevitMethods.AddSharedParameter(
                        doc,
                        app,
                        $"{paramDef.Key}", // pass in the parameter name
                        paramDef.Value, // parameter type
                        category, // revit category to apply parameter to
                        parameterGroup, // parameter group where the parameter will be displayed (in properties bar)
                        true); // instance parameter (will be type parameter if false)

                    // save returned external definition to our dictionary with a key of the parameter name
                    parameterDefinitions[$"{paramDef.Key}"] = extDef;
                }
                return parameterDefinitions;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error:", e.ToString());
                return null;
            }
        }
        public static bool CheckRoomParameters(IList<Element> rooms, Dictionary<string, ParameterType> parameterTypeMappings)
        {
            Element room = rooms.First();
            foreach (string pName in parameterTypeMappings.Keys)
            {
                if (room.LookupParameter(pName) == null) return false;
            }
            return true;
        }
        public static void CheckSPSync(Document doc)
        {
            // TODO add method to check if document has been synced to SP or not by looking up a "synced" shared parameter of the category project_info
            ProjectInfo pInfo = doc.ProjectInformation;
            TaskDialog.Show("Test", pInfo.Name);
        }
        public static List<Room> GenerateRooms(Document doc, Phase phase, SP.ListItemCollection SPListItems, Dictionary<string, ExternalDefinition> parameterList)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Make Sharepoint Rooms");
                try
                {
                    // initialize a list to hold the rooms
                    List<Room> createdRooms = new List<Room>();

                    // iterate over items from the sp list items
                    foreach (SP.ListItem listItem in SPListItems)
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
                                var spValue = listItem[paramDef.Key] ?? null;

                                // retrieve the matching parameter in the room object
                                Parameter p = newRoom.get_Parameter(paramDef.Value);

                                // check the parameter type of the retrieved parameter and set the parameter value appropriately
                                switch (p.Definition.ParameterType)
                                {
                                    case ParameterType.Text:
                                        if (spValue != null) p.Set(spValue.ToString());
                                        break;
                                    case ParameterType.MultilineText:
                                        if (spValue != null) p.Set(spValue.ToString());
                                        break;
                                    case ParameterType.YesNo:
                                        if (spValue.ToString() == "Yes") p.Set(1);
                                        else if (spValue.ToString() == "No") p.Set(0);
                                        break;
                                    case ParameterType.Number:
                                        if (spValue != null) p.Set(Convert.ToDouble(spValue));
                                        break;
                                    case ParameterType.Integer:
                                        if (spValue != null) p.Set(Convert.ToInt32(spValue));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        // set the room's comment parameter value to the value of the Comments column if it exists
                        if (listItem.FieldValues.ContainsKey("Comments")) newRoom.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(listItem["Comments"].ToString());

                        // lastly, set the room's name to the value of the Title column if it exists
                        if (listItem.FieldValues.ContainsKey("Title")) newRoom.Name = listItem["Title"].ToString();

                        // add the room to the list of created rooms
                        createdRooms.Add(newRoom);
                    }
                    t.Commit();
                    return createdRooms;
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Error", e.ToString());
                    t.RollBack();
                    return null;
                }
            }
        }
        public static Phase GetLatestPhase(Document doc)
        {
            PhaseArray allPhases = doc.Phases;
            return allPhases.get_Item(allPhases.Size - 1);
        }
    }
}
