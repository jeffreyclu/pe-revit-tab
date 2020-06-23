#region system libraries
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Windows.Forms;
#endregion

#region microsoft libraries
using SP = Microsoft.SharePoint.Client;
#endregion

using PERevitTab.Data;
using PERevitTab.Commands.DT.UDP;

namespace PERevitTab.Forms
{
    public partial class SharepointLogin : System.Windows.Forms.Form
    {
        #region class variables
        SP.ClientContext _context { get; set; }
        private bool _isValidated = false;
        private bool _isLoggedIn = false;
        #endregion

        #region main form method
        public SharepointLogin(SP.ClientContext extContext)
        {
            // grab the context from the injected argument
            _context = extContext;
            InitializeComponent();
        }
        #endregion

        #region click handlers
        private void loginButton_Click(object sender, EventArgs e)
        {
            // grab the username from the text box
            string username = this.textBox1.Text;

            // grab the password from the text box and convert it to a secure string
            SecureString password = new NetworkCredential("", this.textBox2.Text).SecurePassword;

            // validate the format of the credentials
            _isValidated = SharepointMethods.ValidateCredentials(_context, username, password);
            if (_isValidated == false)
            {
                MessageBox.Show("Error, invalid username or password format. Please try again.");
            }
            else
            {
                // test the validated credentials
                _isLoggedIn = SharepointMethods.TestCredentials(_context, username, password);
                if (_isLoggedIn == false)
                {
                    MessageBox.Show("Error, username or password is incorrect. Please try again.");
                }
                else
                {
                    // store the validated and tested credentials in the cache
                    SharepointConstants.Cache.username = this.textBox1.Text;
                    SharepointConstants.Cache.password = new NetworkCredential("", this.textBox2.Text).SecurePassword;
                    MessageBox.Show("Successfully logged in.");
                    Close();
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
