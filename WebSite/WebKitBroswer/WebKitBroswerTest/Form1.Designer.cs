namespace WebKitBrowserTest
{
    partial class Form1
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
            this.pBroswer = new System.Windows.Forms.Panel();
            this.pControl = new System.Windows.Forms.Panel();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.pControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // pBroswer
            // 
            this.pBroswer.Location = new System.Drawing.Point(65, 61);
            this.pBroswer.Name = "pBroswer";
            this.pBroswer.Size = new System.Drawing.Size(1111, 486);
            this.pBroswer.TabIndex = 0;
            // 
            // pControl
            // 
            this.pControl.Controls.Add(this.btnGo);
            this.pControl.Controls.Add(this.txtUrl);
            this.pControl.Location = new System.Drawing.Point(12, 12);
            this.pControl.Name = "pControl";
            this.pControl.Size = new System.Drawing.Size(940, 33);
            this.pControl.TabIndex = 1;
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(28, 9);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(330, 21);
            this.txtUrl.TabIndex = 0;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(603, 4);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 1;
            this.btnGo.Text = "button1";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1289, 609);
            this.Controls.Add(this.pControl);
            this.Controls.Add(this.pBroswer);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pControl.ResumeLayout(false);
            this.pControl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pBroswer;
        private System.Windows.Forms.Panel pControl;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.TextBox txtUrl;
    }
}

