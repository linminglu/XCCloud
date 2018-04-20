using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WebKitBrowser;

namespace WebKitBrowserMain
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        WebKitBrowserUC uc;
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                uc = new WebKitBrowserUC();
                this.pBrowser.Controls.Add(uc);
                uc.Dock = DockStyle.Fill;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            uc.Navigate(this.txtUrl.Text.Trim());
        }
    }
}
