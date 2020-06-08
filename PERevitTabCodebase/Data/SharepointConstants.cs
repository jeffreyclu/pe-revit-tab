using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PERevitTab.Data
{
    public class SharepointConstants
    { 
        public static class Cache
        {
            public static string username { get; set; }
            public static SecureString password { get; set; }
        }
        public static class Links
        {
            public static string siteUrl = @"https://perkinseastman.sharepoint.com/sites/Grove";
            public static string readListName = @"cp_vi_framework";
            public static string readViewName = "All Items";
            public static string writeListName = @"cp_revit_volumes";
            public static string writeViewName = "All Items";
        }
    }
}
