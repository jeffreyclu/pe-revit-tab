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

using SP = Microsoft.SharePoint.Client;

using PERevitTab.Data;
using PERevitTab.Commands.DT.UDP;

namespace PERevitTab.Forms
{
    public partial class SharepointLogin : System.Windows.Forms.Form
    {
        SP.ClientContext _context { get; set; }
        private bool _isValidated = false;
        private bool _isLoggedIn = false;
        public SharepointLogin(SP.ClientContext extContext)
        {
            _context = extContext;
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string username = this.textBox1.Text;
            SecureString password = new NetworkCredential("", this.textBox2.Text).SecurePassword;
            _isValidated = SharepointMethods.ValidateCredentials(_context, username, password);
            if (_isValidated == false)
            {
                MessageBox.Show("Error, invalid username or password format. Please try again.");
            }
            else
            {
                _isLoggedIn = SharepointMethods.TestCredentials(_context, username, password);
                if (_isLoggedIn == false)
                {
                    MessageBox.Show("Error, username or password is incorrect. Please try again.");
                }
                else
                {
                    SharepointConstants.Cache.username = this.textBox1.Text;
                    SharepointConstants.Cache.password = new NetworkCredential("", this.textBox2.Text).SecurePassword;
                    MessageBox.Show("Successfully logged in.");
                    Close();
                }
            }
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
