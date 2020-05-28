using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PERevitTab.Data
{
    public class Credentials 
    { 
        public static class Cache
        {
            public static string username { get; set; }
            public static SecureString password { get; set; }
        }
    }
}
