#region system libraries
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#endregion

#region microsoft variables
using SP = Microsoft.SharePoint.Client;
#endregion

namespace PERevitTab.Commands.DT.UDP
{
    class SharepointMethods
    {
        #region "get" methods
        public static SP.ClientContext GetContext(string siteurl)
        {
            SP.ClientContext context = new SP.ClientContext(siteurl);
            return context;
        }
        public static SP.List GetListFromWeb(SP.ClientContext context, string listTitle)
        {
            SP.List list = context.Web.Lists.GetByTitle(listTitle);
            context.Load(list);
            context.ExecuteQuery();
            return list;
        }

        public static SP.View GetViewFromList(SP.ClientContext context, SP.List list, string viewName)
        {
            SP.View view = list.Views.GetByTitle(viewName);
            context.Load(view);
            context.ExecuteQuery();
            return view;
        }

        public static SP.ListItemCollection GetViewItemsFromList(SP.ClientContext context, SP.List list, SP.View view)
        {
            SP.ListItemCollection items = null;
            SP.CamlQuery query = new SP.CamlQuery();
            query.ViewXml = view.ViewQuery;
            items = list.GetItems(query);
            context.Load(items);
            context.ExecuteQuery();
            return items;
        }
        public static void GetFieldsFromList(SP.ClientContext context, SP.List list)
        {
            context.Load(list.Fields);
            context.ExecuteQuery();
            foreach (SP.Field field in list.Fields)
            {
                // TODO do something with the fields
                Console.Write(field.InternalName);
            }
        }
        public static SP.ListItem GetItem(SP.ListItemCollection items, string columnHeader, string searchValue)
        {
            if (!items.First().FieldValues.ContainsKey(columnHeader))
            {
                return null;
            }
            return items.Where(i => i[columnHeader].ToString() == searchValue).FirstOrDefault();
        }
        public static IEnumerable<SP.ListItem> GetItems(SP.ListItemCollection items, string columnHeader, string searchValue)
        {
            if (!items.First().FieldValues.ContainsKey(columnHeader))
            {
                return null;
            }
            return items.Where(i => i[columnHeader].ToString() == searchValue);
        }
        #endregion

        #region "set" methods
        public static SP.ListItem AddItemToList(SP.ClientContext context, SP.List list, Dictionary<string, string> item)
        {
            try
            {

                SP.ListItemCreationInformation itemCreationInfo = new SP.ListItemCreationInformation();
                SP.ListItem newItem = list.AddItem(itemCreationInfo);
                foreach (KeyValuePair<string, string> entry in item)
                {
                    newItem[entry.Key] = entry.Value;
                }
                newItem.Update();
                context.Load(newItem);
                context.ExecuteQuery();
                return newItem;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error in SharepointMethods.AddItemToList: {e}");
                return null;
            }
        }
        public static bool AddItemsToList(SP.ClientContext context, SP.List list, List<Dictionary<string, string>> items)
        {
            try
            {
                foreach (Dictionary<string, string> item in items)
                {
                    SP.ListItemCreationInformation itemCreationInfo = new SP.ListItemCreationInformation();
                    SP.ListItem newItem = list.AddItem(itemCreationInfo);
                    foreach (KeyValuePair<string, string> entry in item)
                    {
                        newItem[entry.Key] = entry.Value;
                    }
                    newItem.Update();
                }
                context.ExecuteQuery();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error in SharepointMethods.AddItemsToList: {e}");
                return false;
            }
        }
        #endregion

        #region credentials validation
        public static bool ValidateCredentials(SP.ClientContext context, string username, SecureString password)
        {
            try
            {
                context.Credentials = new SP.SharePointOnlineCredentials(username, password);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TestCredentials (SP.ClientContext context, string username, SecureString password)
        {
            try
            {
                SP.Web web = context.Web;
                context.ExecuteQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region UDP specific
        
        #endregion
    }
}
