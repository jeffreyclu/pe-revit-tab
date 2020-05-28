using PERevitTab.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PERevitTab.Forms
{
    public partial class SharepointLogin : System.Windows.Forms.Form
    {
        public SharepointLogin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Credentials.Cache.username = this.textBox1.Text;
            Credentials.Cache.password = new NetworkCredential("", this.textBox2.Text).SecurePassword;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
