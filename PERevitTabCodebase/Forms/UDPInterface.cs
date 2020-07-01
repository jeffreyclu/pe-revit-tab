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
        
        private List<Room> SyncToSharepoint()
        {
            List<Room> syncedRooms = new List<Room>();

            // get current revit rooms
            IList<Element> collectedRooms = RevitMethods.GetElements(
                _doc,
                BuiltInCategory.OST_Rooms);
            if (collectedRooms == null) return null;

            // get items from SP list
            SP.ListItemCollection spRooms = GetItemsFromSharepointList(
                    SharepointConstants.Links.spReadList,
                    SharepointConstants.Links.allItems);
            if (spRooms == null) return null;

            // get the latest phase
            Phase latestPhase = RevitMethods.GetLatestPhase(_doc);
            if (latestPhase == null) return null;

            // check if room parameters exist
            bool roomParamsExist = RevitMethods.CheckParametersExist(
                collectedRooms,
                SharepointConstants.Dictionaries.newRevitRoomParameters);

            // if they don't exist
            if (!roomParamsExist)
            {
                // make new parameters
                Dictionary<string, ExternalDefinition> roomParametersCreated = RevitMethods.CreateSharedParameters(
                        _doc,
                        _app,
                        SharepointConstants.Dictionaries.newRevitRoomParameters,
                        BuiltInCategory.OST_Rooms,
                        BuiltInParameterGroup.PG_REFERENCE);
                if (roomParametersCreated == null) return null;

                // generate the rooms
                syncedRooms = RevitMethods.CreateRooms(
                        _doc,
                        latestPhase,
                        spRooms,
                        roomParametersCreated);
            }
            else
            {
                // sync the existing rooms
                syncedRooms = RevitMethods.UpdateRooms(
                    _doc,
                    latestPhase,
                    collectedRooms,
                    spRooms,
                    SharepointConstants.Dictionaries.newRevitRoomParameters);
            }
            return syncedRooms;
        }

        private List<object> UploadToSharepoint()
        {
            // collect rooms from the model, return false if there are none
            IList<SpatialElement> placedRooms = RevitMethods.GetPlacedRooms(_doc);
            if (placedRooms == null) return null;

            // check if the custom room parameters exist
            bool roomParametersCheck = RevitMethods.CheckParametersExist(
                placedRooms,
                SharepointConstants.Dictionaries.newRevitRoomParameters);
            if (roomParametersCheck == false) return null;

            // collect the room info for writing to sharepoint
            List<object> placedRoomsData = RevitMethods.ParseRoomData(
                    placedRooms,
                    SharepointConstants.Dictionaries.newRevitRoomParameters);
            if (placedRoomsData == null) return null;

            SP.List SPWriteList = SharepointMethods.GetListFromWeb(
                _context,
                SharepointConstants.Links.spWriteList);

            // write the room data to the write list
            bool roomsUploaded = SharepointMethods.AddItemsToList(
                _context, 
                SPWriteList, 
                placedRoomsData);
            if (roomsUploaded == false) return null;
            
            // if we get to the end then return the written data
            return placedRoomsData;
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
            List<Room> syncedRooms = SyncToSharepoint();

            // check if sync method was successful
            if (syncedRooms != null && syncedRooms.Count > 0)
            {
                MessageBox.Show($"Success, {syncedRooms.Count} rooms synced.");
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

            // run upload method
            List<object> uploadedRooms = UploadToSharepoint();

            // check if upload method was successful
            if (uploadedRooms != null && uploadedRooms.Count > 0)
            {
                MessageBox.Show($"Success, {uploadedRooms.Count} rooms uploaded.");
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