namespace EvoMathProj
{
    partial class AppWorking
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppWorking));
            this.labelMsg = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.labelPercent = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonAddServer = new System.Windows.Forms.Button();
            this.comboBoxDatabases = new System.Windows.Forms.ComboBox();
            this.buttonTestConnection = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxSavePassword = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelMsg
            // 
            this.labelMsg.AutoSize = true;
            this.labelMsg.Location = new System.Drawing.Point(12, 17);
            this.labelMsg.Name = "labelMsg";
            this.labelMsg.Size = new System.Drawing.Size(35, 13);
            this.labelMsg.TabIndex = 0;
            this.labelMsg.Text = "label1";
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 20;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 33);
            this.progressBar1.Maximum = 1000;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(387, 16);
            this.progressBar1.TabIndex = 1;
            this.progressBar1.Visible = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.labelPercent);
            this.splitContainer1.Panel1.Controls.Add(this.labelMsg);
            this.splitContainer1.Panel1.Controls.Add(this.progressBar1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(414, 274);
            this.splitContainer1.SplitterDistance = 66;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 2;
            // 
            // labelPercent
            // 
            this.labelPercent.AutoSize = true;
            this.labelPercent.Location = new System.Drawing.Point(331, 17);
            this.labelPercent.Name = "labelPercent";
            this.labelPercent.Size = new System.Drawing.Size(35, 13);
            this.labelPercent.TabIndex = 2;
            this.labelPercent.Text = "label6";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonAddServer);
            this.panel1.Controls.Add(this.comboBoxDatabases);
            this.panel1.Controls.Add(this.buttonTestConnection);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBoxUsername);
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.checkBoxSavePassword);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.textBox4);
            this.panel1.Controls.Add(this.textBoxPassword);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, -20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(414, 227);
            this.panel1.TabIndex = 17;
            // 
            // buttonAddServer
            // 
            this.buttonAddServer.Location = new System.Drawing.Point(260, 186);
            this.buttonAddServer.Name = "buttonAddServer";
            this.buttonAddServer.Size = new System.Drawing.Size(68, 28);
            this.buttonAddServer.TabIndex = 14;
            this.buttonAddServer.Text = "Add";
            this.buttonAddServer.UseVisualStyleBackColor = true;
            this.buttonAddServer.Click += new System.EventHandler(this.buttonAddServer_Click);
            // 
            // comboBoxDatabases
            // 
            this.comboBoxDatabases.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuPopup;
            this.comboBoxDatabases.Location = new System.Drawing.Point(103, 130);
            this.comboBoxDatabases.Name = "comboBoxDatabases";
            this.comboBoxDatabases.Size = new System.Drawing.Size(299, 21);
            this.comboBoxDatabases.TabIndex = 10;
            this.comboBoxDatabases.DropDown += new System.EventHandler(this.comboBoxDatabases_DropDown);
            // 
            // buttonTestConnection
            // 
            this.buttonTestConnection.Location = new System.Drawing.Point(12, 186);
            this.buttonTestConnection.Name = "buttonTestConnection";
            this.buttonTestConnection.Size = new System.Drawing.Size(125, 28);
            this.buttonTestConnection.TabIndex = 12;
            this.buttonTestConnection.Text = "Test Connection";
            this.buttonTestConnection.UseVisualStyleBackColor = true;
            this.buttonTestConnection.Click += new System.EventHandler(this.buttonTestConnection_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "label2";
            this.label2.Visible = false;
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxUsername.Location = new System.Drawing.Point(103, 21);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(299, 20);
            this.textBoxUsername.TabIndex = 0;
            this.textBoxUsername.Text = "admin";
            this.textBoxUsername.Visible = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(334, 186);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(68, 28);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "label3";
            this.label3.Visible = false;
            // 
            // checkBoxSavePassword
            // 
            this.checkBoxSavePassword.AutoSize = true;
            this.checkBoxSavePassword.Enabled = false;
            this.checkBoxSavePassword.Location = new System.Drawing.Point(103, 107);
            this.checkBoxSavePassword.Name = "checkBoxSavePassword";
            this.checkBoxSavePassword.Size = new System.Drawing.Size(99, 17);
            this.checkBoxSavePassword.TabIndex = 9;
            this.checkBoxSavePassword.Text = "Save password";
            this.checkBoxSavePassword.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "label5";
            this.label5.Visible = false;
            // 
            // textBox4
            // 
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox4.Location = new System.Drawing.Point(103, 81);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(299, 20);
            this.textBox4.TabIndex = 8;
            this.textBox4.Text = "localhost";
            this.textBox4.Visible = false;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPassword.Location = new System.Drawing.Point(103, 51);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '•';
            this.textBoxPassword.Size = new System.Drawing.Size(299, 20);
            this.textBoxPassword.TabIndex = 6;
            this.textBoxPassword.Text = "bios455";
            this.textBoxPassword.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "label4";
            this.label4.Visible = false;
            // 
            // AppWorking
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(414, 274);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppWorking";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LoadingForm_FormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelMsg;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonAddServer;
        private System.Windows.Forms.ComboBox comboBoxDatabases;
        private System.Windows.Forms.Button buttonTestConnection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxSavePassword;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelPercent;
    }
}