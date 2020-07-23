using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PERevitTab.Data
{
    class RevitConstants
    {
        public class Strings
        {
            public static string linkedCAD = "Linked CAD";
            public static string importedCAD = "Imported CAD";
            public static string importedSKP = "Imported SKP";
        }
        public class Guids
        {
            public static Guid redundantFailMsgGuid = new Guid("b4176cef-6086-45a8-a066-c3fd424c9412");
        }
    }
}
