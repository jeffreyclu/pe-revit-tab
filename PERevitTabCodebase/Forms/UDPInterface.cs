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

using PERevitTab.CommonMethods;
using PERevitTab.Data;

namespace PERevitTab.Forms
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class UDPInterface : System.Windows.Forms.Form
    {
        #region form state
        private string _username { get; set; }
        private SecureString _password { get; set; }
        private SP.ListItem _user { get; set; }
        private SP.ListItem _project { get; set; }
        private SP.ListItemCollection _projects { get; set; }
        private bool _isLoggedIn = false;
        private bool _isUserSelected = false;
        private bool _isProjectSelected = false;
        private bool _isProjectsFetched = false;
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

        #region form state methods
        private void CheckLogin()
        {
            // grab username and password from the cache
            _username = SharepointConstants.Cache.username;
            _password = SharepointConstants.Cache.password;

            // if they have been previously set, then the user is logged in
            _isLoggedIn = (_username != null && _password != null);
            if (_isLoggedIn == false)
            {
                // if not logged in, do nothing
                return;
            }

            // set the context credentials
            _context.Credentials = new SP.SharePointOnlineCredentials(_username, _password);
        }
        private void CheckUser()
        {
            // grab the user from the cache
            _user = SharepointConstants.Cache.user;
            _isUserSelected = (_user != null);
        }
        private void CheckProjectSelected()
        {
            // grab the project from the cache
            _project = SharepointConstants.Cache.project;
            _isProjectSelected = (_project != null);
        }
        private void CheckState()
        {
            CheckLogin();
            CheckUser();
            CheckProjectSelected();
        }
        private void ReloadForm()
        {
            // check the state
            CheckState();

            // check if we are logged in and set visibility/text accordingly
            if (_isLoggedIn == true)
            {
                loginButton.Visible = false;
                logoutButton.Visible = true;
                userLabel.Visible = true;
                userLabel.Text = _user == null
                    ? _username
                    : _user[SharepointConstants.ColumnHeaders.nickname].ToString();

                // check if a project has been selected or not
                if (_isProjectSelected == true)
                {
                    projectList.Visible = false;
                    linkProjectButton.Visible = false;
                    linkedProjectLabel.Visible = true;
                    linkedProjectLabel.Text = _project[SharepointConstants.ColumnHeaders.Title].ToString();
                    syncButton.Enabled = true;
                    lastSyncedLabel.Visible = true;
                    uploadButton.Enabled = true;
                    viewProjectButton.Enabled = true;
                }
                else
                {
                    projectList.Visible = true;
                    linkProjectButton.Visible = true;
                    linkedProjectLabel.Visible = false;
                    syncButton.Enabled = false;
                    lastSyncedLabel.Visible = false;
                    uploadButton.Enabled = false;
                    viewProjectButton.Enabled = false;
                }
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
                projectList.Visible = false;
                linkProjectButton.Visible = false;
                linkedProjectLabel.Visible = false;
            }
        }
        private void DisableForm()
        {
            // TODO prompt user to request access
            loginButton.Visible = false;
            logoutButton.Visible = true;
            userLabel.Visible = true;
            userLabel.Text = _user == null
                ? _username
                : _user[SharepointConstants.ColumnHeaders.nickname].ToString();
            syncButton.Enabled = false;
            lastSyncedLabel.Visible = false;
            uploadButton.Enabled = false;
            viewProjectButton.Enabled = false;
        }
        private void DisplayProjects(SP.ListItemCollection allProjects)
        {
            projectList.Items.Clear();
            projectList.SelectedItem = null;
            projectList.SelectedIndex = -1;
            projectList.MaxDropDownItems = allProjects.Count;
            foreach (SP.ListItem p in allProjects)
            {
                string projectLabel = $"" +
                    $"{p[SharepointConstants.ColumnHeaders.projectNumber]} " +
                    $"{p[SharepointConstants.ColumnHeaders.Title]}";
                projectList.Items.Add(projectLabel);
            }
        }
        #endregion

        #region fetch from sharepoint
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
        private SP.ListItem GetCurrentUser()
        {
            // get all users from all users list
            SP.ListItemCollection allUsers = GetItemsFromSharepointList(
                SharepointConstants.Links.userList,
                SharepointConstants.Views.allItems);
            if (allUsers == null)
            {
                return null;
            }

            // get the user matching the logged in user's email address
            SP.ListItem currentUser = allUsers.Where(u => u.FieldValues["emailAddress"].ToString().ToLower() == _username.ToLower()).FirstOrDefault();
            if (currentUser == null)
            {
                return null;
            }

            // store the user in cache
            SharepointConstants.Cache.user = currentUser;
            return currentUser;
        }
        private SP.ListItemCollection GetProjects()
        {
            // get projects from sharepoint
            SP.ListItemCollection projects = GetItemsFromSharepointList(
                SharepointConstants.Links.projectList,
                SharepointConstants.Views.allItems
                );

            if (projects == null)
            {
                MessageBox.Show("Error: no projects available in Sharepoint.");
                return null;
            }

            // store the projects in cache
            SharepointConstants.Cache.projects = projects;
            return projects;
        }
        private SP.ListItem GetLinkedProject()
        {
            // get all linked projects
            SP.ListItemCollection linkedProjects = GetItemsFromSharepointList(
                SharepointConstants.Links.linkedProjectList,
                SharepointConstants.Views.allItems
                );
            if (linkedProjects.Count == 0)
            {
                //return null;
            }

            // see if the current project is linked
            SP.ListItem linkedProject = SharepointMethods.GetItem(
                linkedProjects,
                SharepointConstants.ColumnHeaders.Title,
                _doc.Title
                );
            return linkedProject;
        }
        #endregion

        #region write to sharepoint
        private SP.ListItem LogTransaction(SP.ListItem project, string transactionTypeId)
        {
            Dictionary<string, string> transactionInfo = ParseTransactionInfo(project, "19");
            SP.List transactionsList = SharepointMethods.GetListFromWeb(
                _context,
                SharepointConstants.Links.transactionsList);
            SP.ListItem transaction = SharepointMethods.AddItemToList(
                _context,
                transactionsList,
                transactionInfo
                );

            return transaction;
        }
        private SP.ListItem LogProject(SP.ListItem selectedProject, SP.ListItem transaction)
        {
            // parse the project info
            Dictionary<string, string> projectInfo = ParseProjectInfo(selectedProject, transaction);

            // get write list
            SP.List linkedProjectsList = SharepointMethods.GetListFromWeb(
                _context,
                SharepointConstants.Links.linkedProjectList);
            
            // write the project info to sharepoint
            SP.ListItem loggedProject = SharepointMethods.AddItemToList(
                _context,
                linkedProjectsList,
                projectInfo
                );

            return loggedProject;
        }

        #endregion

        #region convert revit data to sharepoint data
        public Dictionary<string, string> ParseProjectInfo(SP.ListItem selectedProject, SP.ListItem transaction)
        {
            string title = _doc.Title;
            string projectId = selectedProject[SharepointConstants.ColumnHeaders.ID].ToString();
            string version = _app.VersionNumber;
            bool isCentral = _doc.IsWorkshared;
            ModelPath centralPath = isCentral 
                ? _doc.GetWorksharingCentralModelPath() 
                : null;
            string path = isCentral 
                ? ModelPathUtils.ConvertModelPathToUserVisiblePath(centralPath) 
                : _doc.PathName;
            string transactionId = transaction[SharepointConstants.ColumnHeaders.ID].ToString();

            Dictionary<string, string> projectInfo = new Dictionary<string, string>()
            {
                {
                    SharepointConstants.ColumnHeaders.Title,
                    title
                },
                {
                    SharepointConstants.ColumnHeaders.project_id,
                    projectId
                },
                {
                    SharepointConstants.ColumnHeaders.revit_version,
                    version
                },
                {
                    SharepointConstants.ColumnHeaders.is_central_model,
                    isCentral.ToString()
                },
                {
                    SharepointConstants.ColumnHeaders.central_model_path,
                    path
                },
                {
                    SharepointConstants.ColumnHeaders.transaction_id,
                    transactionId
                },
            };

            return projectInfo;
        }
        private Dictionary<string, string> ParseTransactionInfo(SP.ListItem selectedProject, string transactionTypeId)
        {
            string projectId = selectedProject[SharepointConstants.ColumnHeaders.ID].ToString();
            Dictionary<string, string> transactionInfo = new Dictionary<string, string>()
            {
                {
                    SharepointConstants.ColumnHeaders.project_id,
                    projectId
                },
                {
                    SharepointConstants.ColumnHeaders.transaction_type_id,
                    transactionTypeId
                }
            };
            return transactionInfo;
        }
        #endregion
        
        #endregion

        #region event handlers
        private void UDPInterface_Load(object sender, EventArgs e)
        {
            HandleState();
        }
        private void LoginModalClosed (object sender, EventArgs e)
        {
            // cast sender as a form
            System.Windows.Forms.Form form = sender as System.Windows.Forms.Form;

            // check if user cancelled login
            if (form.DialogResult == DialogResult.Cancel)
            {
                return;
            }

            HandleState();
        }
        private void HandleState()
        {
            // check user credentials
            CheckLogin();
            if (_isLoggedIn)
            {
                using (ProgressDialog pd = new ProgressDialog("Loading data.", 4))
                {
                    pd.Show();

                    // get current user from sharepoint
                    pd.StartTask("Looking up user.");
                    SP.ListItem user = GetCurrentUser();
                    if (user == null)
                    {
                        // disable the form if the user doesn't exist
                        SharepointConstants.Cache.user = null;
                        DisableForm();
                        pd.Close();
                        return;
                    }
                    SharepointConstants.Cache.user = user;
                    // CheckUser();
                    pd.Increment();

                    // get all available projects from sharepoint
                    pd.StartTask("Fetching sharepoint projects.");
                    SP.ListItemCollection projects = GetProjects();
                    if (projects == null)
                    {
                        SharepointConstants.Cache.projects = null;
                        pd.Close();
                        return;
                    }
                    SharepointConstants.Cache.projects = projects;
                    pd.Increment();

                    // check if the project is linked
                    pd.StartTask("Checking Revit model link status.");
                    SP.ListItem linkedProject = GetLinkedProject();
                    if (linkedProject == null)
                    {
                        // display the available projects in the dropdown box
                        SharepointConstants.Cache.project = null;
                        DisplayProjects(projects);
                        ReloadForm();
                        pd.Close();
                        return;
                    }
                    pd.Increment();

                    // get the sharepoint project associated with the linked project
                    pd.StartTask("Establishing Revit model to Sharepoint link.");
                    SP.ListItem spProject = SharepointMethods.GetItem(
                        projects,
                        SharepointConstants.ColumnHeaders.ID,
                        linkedProject[SharepointConstants.ColumnHeaders.project_id].ToString()
                        );
                    SharepointConstants.Cache.project = spProject;
                    pd.Increment();
                }
            }
            // refresh the form
            ReloadForm();
        }
        private void HandleLogout()
        {
            // clear the cache
            SharepointConstants.Cache.ClearCache();
            ReloadForm();
        }
        private SP.ListItem HandleLinkProject()
        {
            using (ProgressDialog pd = new ProgressDialog("Link Project", 4))
            {
                pd.Show();

                // get the index of the selected project
                pd.StartTask("Grabbing selected project index.");
                int selectedIndex = projectList.SelectedIndex;
                if (selectedIndex < 0) 
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // get the selected project
                pd.StartTask("Retrieving UDP project from sharepoint.");
                SP.ListItem selectedProject = _projects[selectedIndex];
                if (selectedProject == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // generate a transaction
                pd.StartTask("Logging the transaction.");
                SP.ListItem transaction = LogTransaction(
                    selectedProject, 
                    "19");
                if (transaction == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // log the project
                pd.StartTask("Writing the revit project to sharepoint.");
                SP.ListItem project = LogProject(
                    selectedProject, 
                    transaction);
                if (project == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                return selectedProject;
            }
        }
        private List<Room> HandleSync()
        {
            using (ProgressDialog pd = new ProgressDialog("Sync", 6))
            {
                pd.Show();
                List<Room> syncedRooms = new List<Room>();

                // get current revit rooms
                pd.StartTask("Collecting Revit rooms");
                IList<Element> collectedRooms = RevitMethods.GetElements(
                    _doc,
                    BuiltInCategory.OST_Rooms);
                if (collectedRooms == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // get items from SP list
                pd.StartTask("Fetching Sharepoint data");
                SP.ListItemCollection allSpRooms = GetItemsFromSharepointList(
                        SharepointConstants.Links.spReadList,
                        SharepointConstants.Views.allItems);
                if (allSpRooms == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // filter by current project
                pd.StartTask("Retrieving rooms from current project");
                IEnumerable<SP.ListItem> spRooms = SharepointMethods.GetItems(
                    allSpRooms,
                    SharepointConstants.ColumnHeaders.project_id,
                    _project[SharepointConstants.ColumnHeaders.ID].ToString());
                if (spRooms == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // get the latest phase
                pd.StartTask("Getting latest Revit phase");
                Phase latestPhase = RevitMethods.GetLatestPhase(_doc);
                if (latestPhase == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // check if room parameters exist
                pd.StartTask("Checking Revit parameters");
                bool roomParamsExist = RevitMethods.CheckParametersExist(
                    collectedRooms,
                    SharepointConstants.Dictionaries.newRevitRoomParameters);
                pd.Increment();

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
                    if (roomParametersCreated == null)
                    {
                        pd.Close();
                        return null;
                    }

                    // generate the rooms
                    pd.StartTask("Creating revit rooms");
                    syncedRooms = RevitMethods.CreateRooms(
                            _doc,
                            latestPhase,
                            spRooms,
                            roomParametersCreated);
                    pd.Increment();
                }
                else
                {
                    // sync the existing rooms
                    pd.StartTask("Syncing revit rooms");
                    syncedRooms = RevitMethods.UpdateRooms(
                        _doc,
                        latestPhase,
                        collectedRooms,
                        spRooms,
                        SharepointConstants.Dictionaries.newRevitRoomParameters);
                    pd.Increment();
                }
                return syncedRooms;
            }
        }
        private List<Dictionary<string, string>> HandleUpload()
        {
            using (ProgressDialog pd = new ProgressDialog("Upload", 5))
            {
                pd.Show();
                // collect rooms from the model, return false if there are none
                pd.StartTask("Collecting placed rooms");
                IList<SpatialElement> placedRooms = RevitMethods.GetPlacedRooms(_doc);
                if (placedRooms == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // check if the custom room parameters exist
                pd.StartTask("Checking room parameters");
                bool roomParametersCheck = RevitMethods.CheckParametersExist(
                    placedRooms,
                    SharepointConstants.Dictionaries.newRevitRoomParameters);
                if (roomParametersCheck == false)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // collect the room info for writing to sharepoint
                pd.StartTask("Parsing room data");
                List<Dictionary<string, string>> placedRoomsData = RevitMethods.ParseRoomData(
                        placedRooms,
                        SharepointConstants.Dictionaries.newRevitRoomParameters);
                if (placedRoomsData == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // retrieve write list from sharepoint
                pd.StartTask("Connecting to Sharepoint");
                SP.List SPWriteList = SharepointMethods.GetListFromWeb(
                    _context,
                    SharepointConstants.Links.spWriteList);
                if (SPWriteList == null)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // write the room data to the write list
                pd.StartTask("Writing data to Sharepoint");
                bool roomsUploaded = SharepointMethods.AddItemsToList(
                    _context,
                    SPWriteList,
                    placedRoomsData);
                if (roomsUploaded == false)
                {
                    pd.Close();
                    return null;
                }
                pd.Increment();

                // if we get to the end then return the written data
                return placedRoomsData;
            }
        }
        #endregion

        #region click handlers
        private void loginButton_Click(object sender, EventArgs e)
        {
            // initialize a new login form
            Forms.SharepointLogin spLogin = new Forms.SharepointLogin(_context);

            // add an event handler to handle the login form closing
            spLogin.FormClosed += LoginModalClosed;

            // show the form
            spLogin.ShowDialog();
        }
        private void logoutButton_Click(object sender, EventArgs e)
        {
            HandleLogout();
        }
        private void linkProjectButton_Click(object sender, EventArgs e)
        {
            // run link project method
            SP.ListItem linkedProject = HandleLinkProject();
            
            // cache the logged project
            SharepointConstants.Cache.project = linkedProject;

            if (linkedProject == null)
            {
                MessageBox.Show($"Error: there was a linking issue. Please try again later.");
            }
            else
            {
                MessageBox.Show($"Success, the model was successfully linked.");
                linkedProjectLabel.Text = linkedProject[SharepointConstants.ColumnHeaders.Title].ToString();
                ReloadForm();
            }
        }

        private void syncButton_Click(object sender, EventArgs e)
        {
            // check state
            CheckState();

            // run sync method
            List<Room> syncedRooms = HandleSync();

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
                MessageBox.Show("Error: there was a syncing issue. Please try again later.");
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            // check state
            CheckState();

            // run upload method
            List<Dictionary<string, string>> uploadedRooms = HandleUpload();

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