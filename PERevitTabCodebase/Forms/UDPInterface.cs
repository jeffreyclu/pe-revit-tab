using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SP = Microsoft.SharePoint.Client;

using PERevitTab.Commands.DT.UDP;
using PERevitTab.Data;

namespace PERevitTab.Forms
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class UDPInterface : System.Windows.Forms.Form
    {
        public static bool _isLoggedIn = false;
        private string _username { get; set; }
        private SecureString _password { get; set; }
        private Document _doc { get; set; }
        private Autodesk.Revit.ApplicationServices.Application _app { get; set; }
        SP.ClientContext _context { get; set; }
        private static Dictionary<string, ExternalDefinition> _sharedParametersList = new Dictionary<string, ExternalDefinition>();
        private static Dictionary<string, SP.ListItemCollection> _SPListItems = new Dictionary<string, SP.ListItemCollection>();
        public UDPInterface(Document extDoc, Autodesk.Revit.ApplicationServices.Application extApp, SP.ClientContext extContext)
        {
            // initialize class variables from injected arguments
            _doc = extDoc;
            _context = extContext;
            _app = extApp;
            InitializeComponent();
        }

        #region main form methods
        private void CheckLogin()
        {
            _username = SharepointConstants.Cache.username;
            _password = SharepointConstants.Cache.password;
            _isLoggedIn = (_username != null && _password != null);
            if (_isLoggedIn)
            {
                _context.Credentials = new SP.SharePointOnlineCredentials(_username, _password);
            }
        }

        private void ReloadForm()
        {
            if (_isLoggedIn == true)
            {
                this.loginButton.Visible = false;
                this.logoutButton.Visible = true;
                this.userLabel.Visible = true;
                this.userLabel.Text = _username;
                this.syncButton.Enabled = true;
                this.lastSyncedLabel.Visible = true;
                this.uploadButton.Enabled = true;
                this.viewProjectButton.Enabled = true;
            }
            else
            {
                this.loginButton.Visible = true;
                this.userLabel.Visible = false;
                this.logoutButton.Visible = false;
                this.syncButton.Enabled = false;
                this.lastSyncedLabel.Visible = false;
                this.uploadButton.Enabled = false;
                this.viewProjectButton.Enabled = false;
            }
        }
        #endregion

        #region form event handlers
        private void UDPInterface_Load(object sender, EventArgs e)
        {
            CheckLogin();
            ReloadForm();
        }

        private void SPLoginClosed (object sender, EventArgs e)
        {
            CheckLogin();
            ReloadForm();
        }
        #endregion

        #region click handlers
        private void loginButton_Click(object sender, EventArgs e)
        {
            Forms.SharepointLogin spLogin = new Forms.SharepointLogin(_context);
            spLogin.FormClosed += SPLoginClosed;
            spLogin.ShowDialog();
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            SharepointConstants.Cache.username = null;
            SharepointConstants.Cache.password = null;
            CheckLogin();
            ReloadForm();
        }

        private void syncButton_Click(object sender, EventArgs e)
        {
            // check login
            CheckLogin();
            // get lists from context
            SP.List readList = SharepointMethods.GetListFromWeb(
                _context, 
                SharepointConstants.Links.readListName);
            // get view from list by view name
            SP.View readView = SharepointMethods.GetViewFromList(
                _context, 
                readList, 
                SharepointConstants.Links.readViewName);
            // get items from list by view
            SP.ListItemCollection readListItems = SharepointMethods.GetViewItemsFromList(
                _context,
                readList,
                readView);
            // save the items in a dictionary
            _SPListItems["readListItems"] = readListItems;

            // new shared parameters
            ExternalDefinition vif_ID = RevitMethods.AddSharedParameter(_doc, _app, "vif_ID", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["vif_ID"] = vif_ID; // TODO refactor this part to be in RevitMethods.AddSharedParameter function to avoid repetition
            ExternalDefinition org1 = RevitMethods.AddSharedParameter(_doc, _app, "org1", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org1"] = org1;
            ExternalDefinition org2 = RevitMethods.AddSharedParameter(_doc, _app, "org2", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org2"] = org2;
            ExternalDefinition org3 = RevitMethods.AddSharedParameter(_doc, _app, "org3", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org3"] = org3;
            ExternalDefinition org4 = RevitMethods.AddSharedParameter(_doc, _app, "org4", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org4"] = org4;
            ExternalDefinition org5 = RevitMethods.AddSharedParameter(_doc, _app, "org6", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org5"] = org5;
            ExternalDefinition org6 = RevitMethods.AddSharedParameter(_doc, _app, "org6", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org6"] = org6;
            ExternalDefinition org7 = RevitMethods.AddSharedParameter(_doc, _app, "org7", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org7"] = org7;
            ExternalDefinition org8 = RevitMethods.AddSharedParameter(_doc, _app, "org8", Autodesk.Revit.DB.ParameterType.Text, true);
            _sharedParametersList["org8"] = org8;

            // get the latest revit phase for room creation
            Phase latestPhase = RevitMethods.GetLatestPhase(_doc);

            // generate the rooms
            bool roomsCreated = RevitMethods.GenerateRooms(_doc, latestPhase, _SPListItems, _sharedParametersList);
            if (roomsCreated)
            {
                MessageBox.Show($"Success, {readListItems.Count} rooms synced.");
                (string writeDate, string writeTime) = FormatDateTime();
                lastSyncedLabel.Text = $"Last synced {writeDate} at {writeTime}";
                ReloadForm();
            }
            else
            {
                MessageBox.Show("Error, there was a syncing issue. Please try again later.");
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {

        }

        private void viewProjectButton_Click(object sender, EventArgs e)
        {

        }

        private void downloadPowerBIButton_Click(object sender, EventArgs e)
        {

        }

        private void provideFeedback_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region utilities
        public (string date, string time) FormatDateTime()
        {
            DateTime localDate = DateTime.Now;
            string writeDate = localDate.ToString("yyyy-MM-dd");
            string writeTime = localDate.ToString("HH:mm:ss");
            return (writeDate, writeTime);
        }
        #endregion
    }
}