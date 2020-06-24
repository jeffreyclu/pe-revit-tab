#region autodesk libraries
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
        private string _lastSynced { get; set; }
        private Document _doc { get; set; }
        private Autodesk.Revit.ApplicationServices.Application _app { get; set; }
        SP.ClientContext _context { get; set; }
        private Phase _latestPhase { get; set; }
        private List<Room> _createdRooms { get; set; }
        private Dictionary<string, ExternalDefinition> _roomParameters { get; set; }
        private IList<SpatialElement> _collectedRooms { get; set; }
        private bool _roomParametersChecked { get; set; }
        private List<object> _collectedRoomsData { get; set; }
        private bool _roomsUploaded { get; set; }
        private SP.ListItemCollection _SPListItems { get; set; }
        private SP.List _SPWriteList { get; set; }
        #endregion

        #region main form method
        public UDPInterface(Document extDoc, Autodesk.Revit.ApplicationServices.Application extApp, SP.ClientContext extContext)
        {
            // initialize class variables from injected arguments
            _doc = extDoc;
            _context = extContext;
            _app = extApp;
            InitializeComponent();
        }
        #endregion

        #region helper methods
        private void CheckLogin()
        {
            // grab username and password from the cache
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
            // check if we are logged in and set visibility/text accordingly
            if (_isLoggedIn == true)
            {
                loginButton.Visible = false;
                logoutButton.Visible = true;
                userLabel.Visible = true;
                userLabel.Text = _username;
                syncButton.Enabled = true;
                lastSyncedLabel.Visible = true;
                uploadButton.Enabled = true;
                viewProjectButton.Enabled = true;
            }
            else
            {
                loginButton.Visible = true;
                userLabel.Visible = false;
                logoutButton.Visible = false;
                syncButton.Enabled = false;
                lastSyncedLabel.Visible = false;
                uploadButton.Enabled = false;
                viewProjectButton.Enabled = false;
            }
        }

        private SP.ListItemCollection GetItemsFromSharepointList(string listName, string viewName)
        {
            try
            {
                // get the list from context
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
                return readListItems;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }
        
        private bool SyncToSharepoint()
        {
            // get items from SP list
            try
            {
                _SPListItems = GetItemsFromSharepointList(
                    SharepointConstants.Links.spReadList,
                    SharepointConstants.Links.allItems);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // create new room shared parameters
            try
            {
                _roomParameters = RevitMethods.AddSharedParametersFromList(
                    _doc,
                    _app,
                    SharepointConstants.Dictionaries.newRevitRoomParameters,
                    BuiltInCategory.OST_Rooms,
                    BuiltInParameterGroup.PG_REFERENCE);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // get the latest revit phase for room creation
            try
            {
                _latestPhase = RevitMethods.GetLatestPhase(_doc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // generate the rooms
            try
            {
                _createdRooms = RevitMethods.GenerateRooms(
                    _doc,
                    _latestPhase,
                    _SPListItems,
                    _roomParameters);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // if we get to the end then return true
            return true;
        }

        private bool UploadToSharepoint()
        {
            // collect rooms from the model, return false if there are none
            try
            {
                _collectedRooms = RevitMethods.CollectRooms(_doc);
                if (_collectedRooms == null)
                {
                    MessageBox.Show("Error: no rooms in model.");
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // check if the custom room parameters exist
            try
            {
                _roomParametersChecked = RevitMethods.CheckRoomParameters(
                    _collectedRooms,
                    SharepointConstants.Dictionaries.newRevitRoomParameters);
                if (_roomParametersChecked == false) {
                    MessageBox.Show("Error, project has not been synced. Sync project and try again.");
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // collect the room info for writing to sharepoint
            try
            {
                _collectedRoomsData = RevitMethods.ParseRoomData(_collectedRooms);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // get the write list from sharepoint
            try
            {
               _SPWriteList = SharepointMethods.GetListFromWeb(
                _context,
                SharepointConstants.Links.spWriteList);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            // write the room data to the write list
            try { 
                _roomsUploaded = SharepointMethods.AddItemsToList(
                    _context, 
                    _SPWriteList, 
                    _collectedRoomsData);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
            
            // if we get to the end then return true
            return true;
        }
        #endregion

        #region event handlers
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
            // initialize a new login form
            Forms.SharepointLogin spLogin = new Forms.SharepointLogin(_context);

            // add an event handler to handle the login form closing
            spLogin.FormClosed += SPLoginClosed;

            // show the form
            spLogin.ShowDialog();
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            // reset cached username and password variables
            SharepointConstants.Cache.username = null;
            SharepointConstants.Cache.password = null;
            CheckLogin();
            ReloadForm();
        }

        private void syncButton_Click(object sender, EventArgs e)
        {
            // check login
            CheckLogin();

            // run sync method
            bool synced = SyncToSharepoint();

            // check if sync method was successful
            if (synced)
            {
                MessageBox.Show($"Success, {_createdRooms.Count} rooms synced.");
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
            bool roomsUploaded = UploadToSharepoint();

            if (roomsUploaded)
            {
                MessageBox.Show($"Success, {_collectedRoomsData.Count} rooms uploaded.");
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