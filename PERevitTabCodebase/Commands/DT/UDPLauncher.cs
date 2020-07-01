using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SP = Microsoft.SharePoint.Client;

using PERevitTab.Commands.DT.UDP;
using PERevitTab.Data;
using System.Security;

namespace PERevitTab.Commands.DT
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class UDPLauncher : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // intialize context variables to pass on to user interface form
                Document doc = commandData.Application.ActiveUIDocument.Document;
                Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;
                SP.ClientContext context = SharepointMethods.GetContext(SharepointConstants.Links.siteUrl);

                // inject the context variables via arguments
                Forms.UDPInterface wf = new Forms.UDPInterface(doc, app, context);
                wf.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return Result.Failed;
            }
        }
    }
}
