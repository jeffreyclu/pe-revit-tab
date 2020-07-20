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
using WF = System.Windows.Forms;
#endregion

#region microsoft libraries
using SP = Microsoft.SharePoint.Client;
#endregion

using PERevitTab.Data;
using Microsoft.SharePoint.Client;

namespace PERevitTab.Commands.DT.UDP
{
    class RevitMethods
    {
        #region read (non-transactions)
        public static IList<Element> GetElements(Document doc, BuiltInCategory category)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .OfCategory(category)
                    .WhereElementIsNotElementType()
                    .ToList();
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.GetElements: {e}");
                return null;
            }
        }
        public static IList<SpatialElement> GetPlacedRooms(Document doc)
        {
            try
            {
                // retrieve only placed rooms (i.e. where area > 0 and location is not null)
                IList<SpatialElement> placedRooms = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .Cast<SpatialElement>()
                    .Where(r => r.Area != 0 && r.Location != null)
                    .ToList();

                return placedRooms;
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.GetPlacedRooms: {e}");
                return null;
            }
        }
        public static IList<SpatialElement> GetUnplacedRooms(Document doc)
        {
            try
            {
                // retrieve only unplaced rooms (i.e. where area is 0 and location is null)
                IList<SpatialElement> placedRooms = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .Cast<SpatialElement>()
                    .Where(r => r.Area == 0 && r.Location == null)
                    .ToList();

                return placedRooms;
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.GetPlacedRooms: {e}");
                return null;
            }
        }
        public static PhaseArray GetPhases(Document doc)
        {
            try
            {
                return doc.Phases;
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.GetPhases: {e}");
                return null;
            }
        }
        public static Phase GetLatestPhase(Document doc)
        {
            try
            {
                PhaseArray allPhases = doc.Phases;
                return allPhases.get_Item(allPhases.Size - 1);
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.GetLatestPhase: {e}");
                return null;
            }
        }
        public static List<Dictionary<string, string>> ParseRoomData(IList<SpatialElement> rooms, Dictionary<string, ParameterType> parameterTypeMappings)
        {
            try
            {
                // instantiate a list of objects to hold our room information (which will be a Dictionary<string, string>)
                List<Dictionary<string, string>> roomsList = new List<Dictionary<string, string>>();

                // loop through the collected rooms
                foreach (SpatialElement r in rooms)
                {
                    // instantiate a dictionary to hold the SP column headers and the associated values
                    Dictionary<string, string> roomsInfo = new Dictionary<string, string>();

                    // first, deal with the custom parameters
                    foreach (string paramName in parameterTypeMappings.Keys)
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

                    // next, the built-in parameters
                    roomsInfo[SharepointConstants.ColumnHeaders.vol_Title] = r.Name;
                    roomsInfo[SharepointConstants.ColumnHeaders.revit_room_number] = r.Number;
                    roomsInfo[SharepointConstants.ColumnHeaders.revit_room_element_id] = r.Id.ToString();
                    roomsInfo[SharepointConstants.ColumnHeaders.comments] = r.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();

                    // finally, the built-in spatial parameters
                    Room ro = (Room)r;
                    roomsInfo["revit_room_volume"] = ro.Volume.ToString();
                    roomsInfo["revit_room_height"] = ro.UnboundedHeight.ToString();
                    roomsInfo["area_net"] = ro.Area.ToString();

                    // add the parsed info to the return list
                    roomsList.Add(roomsInfo);
                }
                return roomsList;
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.ParseRoomData': {e}");
                return null;
            }
        }
        public static bool CheckParametersExist(IList<Element> elements, Dictionary<string, ParameterType> parameterTypeMappings)
        {
            if (elements.Count == 0) return false;
            Element first = elements.First();
            foreach (string pName in parameterTypeMappings.Keys)
            {
                if (first.LookupParameter(pName) == null) return false;
            }
            return true;
        }
        public static bool CheckParametersExist(IList<SpatialElement> elements, Dictionary<string, ParameterType> parameterTypeMappings)
        {
            if (elements.Count == 0) return false;
            Element first = elements.First();
            foreach (string pName in parameterTypeMappings.Keys)
            {
                if (first.LookupParameter(pName) == null) return false;
            }
            return true;
        }
        public static bool CheckParametersExist(IList<Element> elements, List<string> parameterNames)
        {
            if (elements.Count == 0) return false;
            Element first = elements.First();
            foreach (string pName in parameterNames)
            {
                if (first.LookupParameter(pName) == null) return false;
            }
            return true;
        }
        #endregion

        #region create (transactions)
        public static ExternalDefinition CreateSharedParameter(Document doc, Application app, string name, ParameterType type, BuiltInCategory cat, BuiltInParameterGroup group, bool instance)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start($"Add shared parameter {name}");
                try
                {
                    // save the path to the current shared parameters file
                    string oriFile = app.SharedParametersFilename;
                    // initialize a temporary shared param file
                    string tempFile = Path.GetTempFileName() + ".txt";

                    // assign the temporary shared param file to the current application
                    using (System.IO.File.Create(tempFile)) { }
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
                    System.IO.File.Delete(tempFile);

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
                        WF.MessageBox.Show($"Error in RevitMethods.AddSharedParameter: parameter '{name}' was not binded successfully.");
                        t.RollBack();
                    }

                    // return the external definition
                    t.Commit();
                    return def;
                }
                catch (Exception e)
                {
                    WF.MessageBox.Show($"Error in RevitMethods.AddSharedParameter, parameter '{name}': {e}");
                    t.RollBack();
                    return null;
                }
            }
        }
        public static Dictionary<string, ExternalDefinition> CreateSharedParameters(Document doc, Application app, Dictionary<string, ParameterType> parameterTypeMappings, BuiltInCategory category, BuiltInParameterGroup parameterGroup)
        {
            // initialize a dictionary to hold our parameter name and definitions
            Dictionary<string, ExternalDefinition> parameterDefinitions = new Dictionary<string, ExternalDefinition>();

            // iterate through the parameter type mappings dictionary
            foreach (KeyValuePair<string, ParameterType> paramDef in parameterTypeMappings)
            {

                // call add shared parameter method for each key/value pair in our dictionary, returning an external definition
                ExternalDefinition extDef = CreateSharedParameter(
                    doc,
                    app,
                    $"{paramDef.Key}", // pass in the parameter name
                    paramDef.Value, // parameter type
                    category, // revit category to apply parameter to
                    parameterGroup, // parameter group where the parameter will be displayed (in properties bar)
                    true); // instance parameter (will be type parameter if false)

                if (extDef != null)
                {
                    // save returned external definition to our dictionary with a key of the parameter name
                    parameterDefinitions[$"{paramDef.Key}"] = extDef;
                }
                else return null;
            }
            return parameterDefinitions;
        }
        public static Room CreateRoom(Document doc, Phase phase)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create new Room");
                try
                {
                    Room r = doc.Create.NewRoom(phase);
                    t.Commit();
                    return r;
                }
                catch (Exception e)
                {
                    WF.MessageBox.Show($"Error in RevitMethods.CreateRoom: {e}");
                    t.RollBack();
                    return null;
                }
            }
        }
        #endregion

        #region update (transactions)
        #endregion

        #region delete (transactions)
        #endregion

        #region UDP specific
        public static bool SetRoomParameters(Room r, SP.ListItem listItem, Dictionary<string, ExternalDefinition> parameterList)
        {
            try
            {
                // loop through shared parameter definitions we created
                foreach (KeyValuePair<string, ExternalDefinition> paramDef in parameterList)
                {
                    // check if the parameter name exists as a column header in sharepoint data
                    if (listItem.FieldValues.ContainsKey(paramDef.Key))
                    {
                        // get the matching value of the column header, or null
                        var spValue = listItem[paramDef.Key] ?? null;

                        // retrieve the matching parameter in the room object
                        Parameter p = r.get_Parameter(paramDef.Value);

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
                    else return false;
                }

                // set the room's comment parameter value to the value of the Comments column if it exists
                string comments = SharepointConstants.ColumnHeaders.comments;
                if (listItem.FieldValues.ContainsKey(comments))
                {
                    if (listItem[comments] != null) r.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(listItem[comments].ToString());
                }
                else return false;

                // lastly, set the room's name to the value of the Title column if it exists
                string Title = SharepointConstants.ColumnHeaders.Title;
                if (listItem.FieldValues.ContainsKey(Title))
                {
                    if (listItem[Title] != null) r.Name = listItem[Title].ToString();
                }
                else return false;

                // if we don't have any issues return true
                return true;
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.SetRoomParameters: {e}");
                return false;
            }
        }
        public static bool SetRoomParameters(Room r, SP.ListItem listItem, Dictionary<string, ParameterType> parameterTypeMappings)
        {
            try
            {
                // loop through parameter type mappings dictionary
                foreach (string pName in parameterTypeMappings.Keys)
                {
                    // check if the parameter name exists as a column header in sharepoint data
                    if (listItem.FieldValues.ContainsKey(pName))
                    {
                        // get the matching value of the column header, or null
                        var spValue = listItem[pName] ?? null;

                        // retrieve the matching parameter in the room object
                        Parameter p = r.LookupParameter(pName);

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
                    else return false;
                }

                // set the room's comment parameter value to the value of the Comments column if it exists
                string comments = SharepointConstants.ColumnHeaders.comments;
                if (listItem.FieldValues.ContainsKey(comments))
                {
                    if (listItem[comments] != null) r.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(listItem[comments].ToString());
                }
                else return false;

                // lastly, set the room's name to the value of the Title column if it exists
                string Title = SharepointConstants.ColumnHeaders.Title;
                if (listItem.FieldValues.ContainsKey(Title))
                {
                    if (listItem[Title] != null) r.Name = listItem[Title].ToString();
                }
                else return false;

                // if we don't have any issues, return true
                return true;
            }
            catch (Exception e)
            {
                WF.MessageBox.Show($"Error in RevitMethods.SetRoomParameters: {e}");
                return false;
            }
        }
        public static List<Room> CreateRooms(Document doc, Phase phase, IEnumerable<SP.ListItem> spListItems, Dictionary<string, ExternalDefinition> parameterList)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Creating rooms from sharepoint list");
                try
                {
                    // initialize a list to hold the rooms
                    List<Room> createdRooms = new List<Room>();

                    // iterate over items from the sp list items
                    foreach (SP.ListItem listItem in spListItems)
                    {
                        // make a new room for each item
                        Room newRoom = doc.Create.NewRoom(phase);

                        // set the parameters of the new room
                        bool synced = SetRoomParameters(
                            newRoom,
                            listItem,
                            parameterList);

                        // if successfully added, add the new room to the list of created rooms
                        if (synced) createdRooms.Add(newRoom);
                        else
                        {
                            WF.MessageBox.Show($"Error in RevitMethods.CreateRooms: could not set parameters for {newRoom.Name}");
                            t.RollBack();
                            return null;
                        }
                    }
                    t.Commit();
                    return createdRooms;
                }
                catch (Exception e)
                {
                    WF.MessageBox.Show($"Error in RevitMethods.CreateRooms: {e}");
                    t.RollBack();
                    return null;
                }
            }
        }
        public static List<Room> UpdateRooms(Document doc, Phase phase, IList<Element> rooms, IEnumerable<SP.ListItem> spListItems, Dictionary<string, ParameterType> parameterTypeMappings)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Sync rooms from sharepoint list");
                try
                {
                    // initialize a list to hold the rooms
                    List<Room> syncedRooms = new List<Room>();

                    // iterate through sharepoint list
                    foreach (ListItem listItem in spListItems)
                    {
                        string vif_id = SharepointConstants.ColumnHeaders.vif_id;
                        foreach (Room r in rooms)
                        {
                            string test = r.LookupParameter(vif_id).AsString();
                            string test2 = r.LookupParameter(vif_id).AsValueString();
                            Parameter test3 = r.LookupParameter(vif_id);
                        }
                        object test4 = listItem[vif_id];
                        string test5 = listItem[vif_id].ToString();
                        // check if the SP list does has a column header "vif_id"
                        if (listItem.FieldValues.ContainsKey(vif_id))
                        {
                            // check if the revit room's vif_id matches the SP list item's vif_id
                            Room foundRoom = rooms.Where(r => r.LookupParameter(vif_id).AsString() == listItem[vif_id].ToString()).FirstOrDefault() as Room;
                            // if no match
                            if (foundRoom == null)
                            {
                                // create a new room
                                Room newRoom = doc.Create.NewRoom(phase);

                                // set the new room's parameters
                                bool synced = SetRoomParameters(
                                    newRoom,
                                    listItem,
                                    parameterTypeMappings);

                                // if successfully added, add the room to the list of synced rooms
                                if (synced) syncedRooms.Add(newRoom);
                                else
                                {
                                    WF.MessageBox.Show($"Error in RevitMethods.UpdateRooms: couldn't set parameters for new room {newRoom.Id}");
                                    t.RollBack();
                                    return null;
                                }
                            }
                            else
                            {
                                // set the room's parameters
                                bool synced = SetRoomParameters(
                                    foundRoom,
                                    listItem,
                                    parameterTypeMappings);

                                // if successfully added, add the room to the list of synced rooms
                                if (synced) syncedRooms.Add(foundRoom);
                                else
                                {
                                    WF.MessageBox.Show($"Error in RevitMethods.UpdateRooms: couldn't set parameters for existing room {foundRoom.Id}");
                                    t.RollBack();
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            WF.MessageBox.Show("Error, column 'vif_id' does not exist. Check the sharepoint list and try again.");
                            t.RollBack();
                            return null;
                        }
                    }
                    // commit the transaction and return the list of synced rooms
                    t.Commit();
                    return syncedRooms;
                }
                catch (Exception e)
                {
                    WF.MessageBox.Show($"Error creating new rooms: {e}");
                    t.RollBack();
                    return null;
                }
            }
        }
        #endregion

        #region todo
        public static bool CheckSharepointPhases(Document doc, SP.ListItemCollection spPhases)
        {
            // TODO finish this
            return true;
        }
        public static bool CheckSPSync(Document doc)
        {
            // TODO add method to check if document has been synced to SP or not by looking up a "synced" shared parameter of the category project_info
            ProjectInfo pInfo = doc.ProjectInformation;
            TaskDialog.Show("Test", pInfo.Name);
            return true;
        }
        #endregion
    }
}
