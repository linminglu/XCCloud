using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebKitBrowser;

namespace WebKitBrowserTest
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
            uc = new WebKitBrowserUC();
            this.pBroswer.Controls.Add(uc);
            
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            uc.Navigate("www.baidu.com");
        }
    }
}
