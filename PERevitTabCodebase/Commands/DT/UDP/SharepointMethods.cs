using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using SP = Microsoft.SharePoint.Client;

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
                // Do something with the fields
                Console.Write(field.InternalName);
            }
        }
        #endregion
        #region "set" methods
        public static void AddItemsToList(SP.ClientContext context, SP.List list, List<object> rooms)
        {
            foreach (Dictionary<string, string> room in rooms)
            {
                SP.ListItemCreationInformation itemCreateInfo = new SP.ListItemCreationInformation();
                SP.ListItem newItem = list.AddItem(itemCreateInfo);
                foreach (KeyValuePair<string, string> entry in room)
                {
                    newItem[entry.Key.ToString()] = entry.Value;
                }
                newItem.Update();
            }
            context.ExecuteQuery();
        }
        #endregion

        #region validate credentials
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
    }
}
