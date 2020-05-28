
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.SharePoint.Client;
using PERevitTab.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using SP = Microsoft.SharePoint.Client;

namespace PERevitTab.Commands.DT
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SharepointConnector : IExternalCommand
    {
        // TODO STORE LOGIN CREDENTIALS LOCALLY, SWITCH LOGGEDIN TO TRUE AND USE CACHED CREDENTIALS
        private static readonly string siteUrl = @"https://perkinseastman.sharepoint.com/sites/Grove";
        private static readonly string listName = @"translated_database";
        private static readonly string listView = @"dynamoDataView";
        private static bool loggedIn = false;
        public static ClientContext Context { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Forms.SharepointLogin wf = new Forms.SharepointLogin();
                wf.ShowDialog();
                wf.Close();

                string username = Credentials.Cache.username;
                SecureString password = Credentials.Cache.password;
                Context = new SP.ClientContext(siteUrl);

                if (username != null && password != null)
                {
                    try
                    {
                        Context.Credentials = new SP.SharePointOnlineCredentials(username, password);
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
            }
        }
        #region methods
        private static bool ValidateCredentials(string url, string username, SecureString password)
        {
            try
            {
                SP.ClientContext context = new SP.ClientContext(url);
                context.Credentials = new SP.SharePointOnlineCredentials(username, password);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static SP.List GetListFromWeb()
        {
            SP.List list = Context.Web.Lists.GetByTitle(listName);
            Context.Load(list);
            Context.ExecuteQuery();
            return list;
        }

        public static SP.View GetViewFromList(SP.List list)
        {
            SP.View view = list.Views.GetByTitle(listView);
            Context.Load(view);
            Context.ExecuteQuery();
            return view;
        }

        public static SP.ListItemCollection GetViewItemsFromList(SP.View view, SP.List list)
        {
            SP.ListItemCollection items = null;
            SP.CamlQuery query = new SP.CamlQuery();
            query.ViewXml = view.ViewQuery;
            items = list.GetItems(query);
            Context.Load(items);
            Context.ExecuteQuery();
            return items;
        }
        #endregion
    }
}
