
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using PERevitTab.Data;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SP = Microsoft.SharePoint.Client;

namespace PERevitTab.Commands.DT
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SharepointConnector : IExternalCommand
    {
        // TODO STORE LOGIN CREDENTIALS LOCALLY, SWITCH LOGGEDIN TO TRUE AND USE CACHED CREDENTIALS
        private static string siteUrl = @"https://perkinseastman.sharepoint.com/sites/Grove";
        private static string listName = @"cpVolumes";
        private static string listView = @"All Items";
        private static string username { get; set; }
        private static SecureString password { get; set; }
        private static SP.ClientContext context { get; set; }
        private static UIApplication uiApp { get; set; }
        private static Document doc { get; set; }
        private static Autodesk.Revit.ApplicationServices.Application app { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            app = commandData.Application.Application;
            uiApp = commandData.Application;
            doc = uiApp.ActiveUIDocument.Document;
            try
            {
                Forms.SharepointLogin wf = new Forms.SharepointLogin();
                // check if username & password have not been set
                if (Credentials.Cache.username == null && Credentials.Cache.password == null)
                {
                    wf.ShowDialog();
                    wf.Close();
                }
                //see if user cancelled
                if (wf.DialogResult == DialogResult.Cancel)
                {
                    return Result.Cancelled;
                }

                username = Credentials.Cache.username;
                password = Credentials.Cache.password;
                context = new SP.ClientContext(siteUrl);
                try
                {
                    // validate the format of the username and password
                    context.Credentials = new SP.SharePointOnlineCredentials(username, password);
                    try
                    {
                        // validate the username and password
                        SP.Web web = context.Web;
                        context.ExecuteQuery();
                        try
                        {
                            SP.List testList = GetListFromWeb();
                            SP.View testView = GetViewFromList(testList);
                            SP.ListItemCollection testListItems = GetViewItemsFromList(testView, testList);
                            TaskDialog.Show($"{testList.Title}: {testView.Title}", $"{testListItems.Count}");
                            AddSharedParameter("_ID", Autodesk.Revit.DB.ParameterType.Text, true);
                            GenerateRooms(testListItems);
                            /*
                            foreach (SP.ListItem li in testListItems)
                            {
                                foreach(KeyValuePair<string, object> entry in li.FieldValues)
                                {
                                    Console.WriteLine(entry.Key);
                                    
                                }
                                TaskDialog.Show($"{testList.Title}: {testView.Title}", $"{li["ID"]}");
                            }
                            */
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error, username or password is incorrect");
                    }
                }
                catch
                {
                    MessageBox.Show("Error, the email you inputted is not in the correct format.");
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.ToString());
                return Result.Failed;
            }
            #region previous
            /*
            try
            {
                if (Credentials.Cache.username == null && Credentials.Cache.password == null)
                {
                    
                    wf.ShowDialog();
                    wf.Close();
                }

                string username = Credentials.Cache.username;
                SecureString password = Credentials.Cache.password;
                context = new SP.ClientContext(siteUrl);

                
                {
                    try
                    {
                        context.Credentials = new SP.SharePointOnlineCredentials(username, password);
                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("Error", e.ToString());
                        return Result.Failed;
                    }

                    try
                    {
                        SP.List testList = GetListFromWeb();
                        SP.View testView = GetViewFromList(testList);
                        SP.ListItemCollection testListItems = GetViewItemsFromList(testView, testList);

                        TaskDialog.Show("Success", $"List Name: {testList.Title}, View Name: {testView.Title}, Number of Items: {testListItems.Count}");
                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("Error", "Username or Password is incorrect. Please try again.");
                        return Result.Failed;
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.ToString());
                return Result.Failed;
            } */
            #endregion
        }
        #region sharepoint methods
        public static SP.List GetListFromWeb()
        {
            SP.List list = context.Web.Lists.GetByTitle(listName);
            context.Load(list);
            context.ExecuteQuery();
            return list;
        }

        public static SP.View GetViewFromList(SP.List list)
        {
            SP.View view = list.Views.GetByTitle(listView);
            context.Load(view);
            context.ExecuteQuery();
            return view;
        }

        public static SP.ListItemCollection GetViewItemsFromList(SP.View view, SP.List list)
        {
            SP.ListItemCollection items = null;
            SP.CamlQuery query = new SP.CamlQuery();
            query.ViewXml = view.ViewQuery;
            items = list.GetItems(query);
            context.Load(items);
            context.ExecuteQuery();
            return items;
        }
        #endregion
        #region revit methods
        private void GenerateRooms(SP.ListItemCollection items)
        {
            Phase latestPhase = GetLatestPhase();
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Make Sharepoint Rooms");
                try
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        Room newRoom = doc.Create.NewRoom(latestPhase);
                        newRoom.Name = "Test Room";
                        newRoom.Number = $"{i}";
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Error", e.ToString());
                    t.RollBack();
                }
                t.Commit();
            }
        }

        private Phase GetLatestPhase()
        {
            PhaseArray allPhases = doc.Phases;
            return allPhases.get_Item(allPhases.Size - 1);
        }

        public static void AddSharedParameter(string name, ParameterType type, bool instance)
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
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Error", e.ToString());
                    t.RollBack();
                }
                t.Commit();
            }
        }
        #endregion
    }
}
