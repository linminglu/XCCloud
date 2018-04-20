namespace WebKitBrowserMain
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pControl = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.pBrowser = new System.Windows.Forms.Panel();
            this.pControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // pControl
            // 
            this.pControl.Controls.Add(this.button1);
            this.pControl.Controls.Add(this.txtUrl);
            this.pControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.pControl.Location = new System.Drawing.Point(0, 0);
            this.pControl.Name = "pControl";
            this.pControl.Size = new System.Drawing.Size(1081, 45);
            this.pControl.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(809, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "btnGo";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(34, 12);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(706, 21);
            this.txtUrl.TabIndex = 0;
            this.txtUrl.Text = "http://localhost:3288/Test/webkittest.html";
            // 
            // pBrowser
            // 
            this.pBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pBrowser.Location = new System.Drawing.Point(0, 45);
            this.pBrowser.Name = "pBrowser";
            this.pBrowser.Size = new System.Drawing.Size(1081, 479);
            this.pBrowser.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1081, 524);
            this.Controls.Add(this.pBrowser);
            this.Controls.Add(this.pControl);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.pControl.ResumeLayout(false);
            this.pControl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pControl;
        private System.Windows.Forms.Panel pBrowser;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Button button1;
    }
}

