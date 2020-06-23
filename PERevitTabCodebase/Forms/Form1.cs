using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PERevitTab.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void SayHello()
        {
            MessageBox.Show("Hello!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // do something here
            SayHello();
        }
    }
}
