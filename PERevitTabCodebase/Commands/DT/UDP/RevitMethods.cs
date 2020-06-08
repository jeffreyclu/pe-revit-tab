using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
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
        public static ExternalDefinition AddSharedParameter(Document doc, Application app, string name, ParameterType type, bool instance)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Add shared parameter");
                try
                {
                    string oriFile = app.SharedParametersFilename;
                    string tempFile = Path.GetTempFileName() + ".txt";
                    using (File.Create(tempFile)) { }
                    app.SharedParametersFilename = tempFile;

                    var defOptions = new ExternalDefinitionCreationOptions(name, type)
                    {
                        Visible = true,
                    };
                    ExternalDefinition def = app.OpenSharedParameterFile().Groups.Create("UDP Parameters").Definitions.Create(defOptions) as ExternalDefinition;

                    app.SharedParametersFilename = oriFile;
                    File.Delete(tempFile);

                    CategorySet categorySet = app.Create.NewCategorySet();
                    Category roomCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Rooms);
                    categorySet.Insert(roomCategory);

                    BuiltInParameterGroup group = BuiltInParameterGroup.PG_IDENTITY_DATA;
                    Autodesk.Revit.DB.Binding binding = app.Create.NewTypeBinding(categorySet);
                    if (instance) binding = app.Create.NewInstanceBinding(categorySet);

                    BindingMap map = (doc.ParameterBindings);
                    if (!map.Insert(def, binding, group))
                    {
                        TaskDialog.Show("Error", $"Failed to create Project parameter '{name}' :(");
                        t.RollBack();
                    }
                    t.Commit();
                    return def;
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
        public static bool GenerateRooms(Document doc, Phase phase, Dictionary<string, SP.ListItemCollection> SPListItems, Dictionary<string, ExternalDefinition> sharedParametersList)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Make Sharepoint Rooms");
                try
                {
                    // iterate over items
                    for (int i = 0; i < SPListItems["readListItems"].Count; i++) // TODO decide which list to use as the main
                    {
                        Room newRoom = doc.Create.NewRoom(phase);
                        newRoom.Name = "Test Room";
                        newRoom.Number = $"{i + 1}";
                        newRoom.get_Parameter(sharedParametersList["vif_ID"]).Set(SPListItems["readListItems"][i]["ID"].ToString());
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
