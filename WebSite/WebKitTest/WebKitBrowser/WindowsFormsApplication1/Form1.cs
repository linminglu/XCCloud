using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WebKitBrowser;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        WebKitBrowserUC uc;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                uc = new WebKitBrowserUC();
                //this.pBrowser.Controls.Add(uc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
