#region autodesk libraries
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

#region system libraries
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
#endregion

#region microsoft libraries
using SP = Microsoft.SharePoint.Client;
#endregion

using PERevitTab.Commands.DT.UDP;
using PERevitTab.Data;

namespace PERevitTab.Forms
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class UDPInterface : System.Windows.Forms.Form
    {
        #region class variables
        public static bool _isLoggedIn = false;
        public static bool _isProjectChosen = false;
        private string _username { get; set; }
        private SecureString _password { get; set; }
        private string _user { get; set; }
        private string _userId { get; set; }
        private string _activeProject { get; set; }
        private string _activeProjectId { get; set; }
        private List<string> _allProjectIds { get; set; }
        private List<string> _userProjectIds { get; set; }
        private Dictionary<string, string> _userProjects = new Dictionary<string, string>();
        private Document _doc { get; set; }
        private Autodesk.Revit.ApplicationServices.Application _app { get; set; }
        private string _lastSynced { get; set; }
        SP.ClientContext _context { get; set; }
        private static Dictionary<string, ExternalDefinition> _sharedParametersList = new Dictionary<string, ExternalDefinition>();
        private static Dictionary<string, SP.ListItemCollection> _SPListItems = new Dictionary<string, SP.ListItemCollection>();
        #endregion
        public UDPInterface(Document extDoc, Autodesk.Revit.ApplicationServices.Application extApp, SP.ClientContext extContext)
        {
            // initialize class variables from injected arguments
            _doc = extDoc;
            _context = extContext;
            _app = extApp;
            InitializeComponent();
        }

        #region main form methods
        /// <summary>
        /// 
        /// </summary>
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

        private void GetItemsFromSharepointList(string listName, string viewName, string dictionaryKey)
        {
            try
            {
                // get lists from context
                SP.List readList = SharepointMethods.GetListFromWeb(
                    _context,
                    listName);
                // get view from list by view name
                SP.View readView = SharepointMethods.GetViewFromList(
                    _context,
                    readList,
                    viewName);
                // get items from list by view
                SP.ListItemCollection readListItems = SharepointMethods.GetViewItemsFromList(
                    _context,
                    readList,
                    readView);
                // save the items in a dictionary
                Dictionary<string, SP.ListItemCollection> SPListItems = new Dictionary<string, SP.ListItemCollection>();
                _SPListItems[dictionaryKey] = readListItems;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        
        private void CreateSharedParametersFromList(Dictionary<string, ParameterType> parameterList)
        {
            try
            {
                // iterate through the parameter list dictionary
                foreach(KeyValuePair<string, ParameterType> paramDef in parameterList) {
                    // call add shared parameter method
                    ExternalDefinition extDef = RevitMethods.AddSharedParameter(
                        _doc, 
                        _app, 
                        $"{paramDef.Key}",
                        paramDef.Value,
                        BuiltInCategory.OST_Rooms,
                        BuiltInParameterGroup.PG_REFERENCE,
                        true);
                    // save returned extDef to list
                    _sharedParametersList[$"{paramDef.Key}"] = extDef;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
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

            // get items from SP list
            GetItemsFromSharepointList(
                SharepointConstants.Links.readListName, 
                SharepointConstants.Links.readViewName, 
                "readListItems");

            // create new shared parameters
            CreateSharedParametersFromList(SharepointConstants.Dictionaries.spToRevitParameterTypes);

            // get the latest revit phase for room creation
            Phase latestPhase = RevitMethods.GetLatestPhase(_doc);

            // generate the rooms
            bool roomsCreated = RevitMethods.GenerateRooms(
                _doc, 
                latestPhase, 
                _SPListItems, 
                _sharedParametersList);

            if (roomsCreated)
            {
                MessageBox.Show($"Success, {_SPListItems["readListItems"].Count} rooms synced.");
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
            // check login
            CheckLogin();

            // get list from context
            SP.List writeList = SharepointMethods.GetListFromWeb(
                _context,
                SharepointConstants.Links.writeListName);

            // collect room data
            IList<Element> rooms = RevitMethods.CollectRooms(_doc);
            List<object> roomsData = RevitMethods.ParseRoomData(rooms);

            // write to sharepoint
            bool roomsUploaded = SharepointMethods.AddItemsToList(_context, writeList, roomsData);

            if (roomsUploaded)
            {
                MessageBox.Show($"Success, {roomsData.Count} rooms uploaded.");
            }
            else
            {
                MessageBox.Show("Error, there was a publishing issue. Please try again later.");
            }
        }

        private void viewProjectButton_Click(object sender, EventArgs e)
        {
            // TODO add link or form to display project info
        }

        private void downloadPowerBIButton_Click(object sender, EventArgs e)
        {
            // TODO add link to power bi template
        }

        private void provideFeedback_Click(object sender, EventArgs e)
        {
            // TODO add link or form to provide feedback
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