#region revit libraries
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

#region dynamo libraries
using Dynamo.Applications;
#endregion

#region system libraries
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace PERevitTab.Commands.DU
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class RunDynamo1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                #region main code
                // get application and documnet objects and start transaction
                UIApplication uiapp = commandData.Application;
                Document doc = uiapp.ActiveUIDocument.Document;

                string Journal_Dynamo_Path = @"\\d-peapcny.net\enterprise\G_Gen-Admin\Committees\Data Unit\_SCRIPTS LIBRARY\DYNAMO\_Ready for the S drive\Dynamo Player\View_Renumber on Multiple Sheets - Dynamo Player.dyn";
                // hello world
                // initialize a new dynamo instance
                DynamoRevit dynamoRevit = new DynamoRevit();

                DynamoRevitCommandData dynamoRevitCommandData = new DynamoRevitCommandData();
                dynamoRevitCommandData.Application = commandData.Application;

                IDictionary<string, string> journalData = new Dictionary<string, string>
                {
                    { Dynamo.Applications.JournalKeys.ShowUiKey, false.ToString() }, // don't show DynamoUI at runtime
                    { Dynamo.Applications.JournalKeys.DynPathKey, Journal_Dynamo_Path }, //run node at this file path
                    { Dynamo.Applications.JournalKeys.DynPathExecuteKey, true.ToString() }, // The journal file can specify if the Dynamo workspace opened from DynPathKey will be executed or not. If we are in automation mode the workspace will be executed regardless of this key.
                    { Dynamo.Applications.JournalKeys.ForceManualRunKey, true.ToString() }, // force runs in manual mode
                    { Dynamo.Applications.JournalKeys.ModelShutDownKey, true.ToString() },
                    { Dynamo.Applications.JournalKeys.ModelNodesInfo, false.ToString() }

                };

                dynamoRevitCommandData.JournalData = journalData;
                Result externalCommandResult = dynamoRevit.ExecuteCommand(dynamoRevitCommandData);
                return externalCommandResult;
                #endregion
            }
            catch (Exception err)
            {
                // TODO: add better error handling
                TaskDialog.Show("Error", err.Message.ToString());
                return Result.Failed;
            }
        }
    }
}
