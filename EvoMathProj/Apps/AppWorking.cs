using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace EvoMathProj
{
    public partial class AppWorking : Form
    {
        public enum Type { Base, LoadMySQLDatabase };

        private BackgroundWorker worker;
        private AppForm parent;
        private double? percentCompl = 0;
        private string loadMsg;
        private string tempConnectString;

        public void SetProgressPercent(string msg, double perc)
        {
            this.percentCompl = perc;

            if (msg != null)
                this.loadMsg = msg;

            if (this.percentCompl > 1)
                this.percentCompl = 1;
            //this.progressBar1.Value = (int)(perc * 1000);
        }
        public void AddProgressPercent(string msg, double perc)
        {
            this.percentCompl += perc;
            if (msg != null)
                this.loadMsg = msg;

            if (this.percentCompl > 1)
                this.percentCompl = 1;

            //this.progressBar1.Value = (int)(perc * 1000);
        }
        public bool IsProgressComplete()
        {
            return percentCompl >= 1;
        }
        public void SetProgressPercent(double perc)
        {
            this.SetProgressPercent(null, perc);
        }

        public AppWorking(String msg, AppForm par, BackgroundWorker bgw)
            : this(Type.Base, msg, par, bgw)
        {
        }
        public AppWorking(Type t, String msg, AppForm par, BackgroundWorker bgw)
        {
            InitializeComponent();

            parent = par;
            worker = bgw;
            this.loadMsg = msg;
            this.labelMsg.Text = msg;
            this.labelPercent.Text = "0%";
            worker.WorkerSupportsCancellation = true;

            this.Init(t);

            par.Enabled = false;
            this.CenterToScreen();
            this.Show();
            this.BringToFront();
            this.Focus();
        }

        public void RunWorker()
        {
            if (worker != null)
                worker.RunWorkerAsync(this);
        }
        public void Init(Type t)
        {
            this.Text = this.parent.TitleText;

            switch (t)
            {
                case Type.Base:
                    {
                        this.progressBar1.Visible = true;
                        this.splitContainer1.Panel2Collapsed = true;
                        //worker.RunWorkerAsync(this);
                        this.Size = new Size(this.Size.Width, 100);
                        this.Text += " - BLAST";
                        return;
                    }
                case Type.LoadMySQLDatabase:
                    {
                        this.progressBar1.Visible = false;
                        this.splitContainer1.Panel2Collapsed = false;
                        this.Size = new Size(this.Size.Width, 308);
                        this.Text += " - Connect to Database";

                        this.label4.Text = "Server name:";
                        this.label2.Text = "User name:";
                        this.label3.Text = "Password";
                        this.label5.Text = "Database:";

                        this.label2.Visible = true;
                        this.label3.Visible = true;
                        this.label4.Visible = true;
                        this.label5.Visible = true;

                        this.textBoxUsername.Visible = true;
                        this.textBoxPassword.Visible = true;
                        this.textBox4.Visible = true;
                        this.comboBoxDatabases.Visible = true;

                        this.textBoxUsername.Select();
                        this.textBoxUsername.Focus();

                        return;
                    }
                    
            }
        }
        public static void TestConnection(AppWorking f, bool alertUser, bool addToServers)
        {            
            var server = f.textBox4.Text;// "localhost";
            var db = (string)(f.comboBoxDatabases.SelectedItem);
            var uid = f.textBoxUsername.Text;
            var pwd = f.textBoxPassword.Text;

            AppWorking.TestConnection(f, server, db, uid, pwd, alertUser, addToServers);
        }
        private static void TestConnection(AppWorking f, string server, string db, string uid, string pwd, bool alertUser, bool addToServers)
        {
            var outpt = f.parent.AppSQL.TestConnection(server, db, uid, pwd, addToServers);

            if (outpt == null && alertUser)
            {
                MessageBox.Show("The connection to the database could not be made with given parameters.\n\n",
                    f.parent.TitleText + " - Test Connection Fail");
                return;
            }

            if (alertUser)
                MessageBox.Show(outpt, f.parent.TitleText + " - Test Connection Success");

            return;
            /*
            var connectionString = "server=" + server + ";" + "database=" +
                db + ";" + "userid=" + uid + ";" + "password=" + pwd + ";" +
                "Connection Timeout=3";

            var sqlConnection = new MySqlConnection(connectionString);
            while (true)
            {
                var os = "";
                try
                {
                    using (sqlConnection)
                    {
                        sqlConnection.Open();
                        os += "User:\t\t" + uid + "\n";
                        if (sqlConnection.Database == "")
                            db = "(none selected)";

                        os += "Database:\t\t" + db + "\n";

                        os += "Server:\t\t" + server + "\n";
                        os += "ServerVersion:\t" + sqlConnection.ServerVersion + "\n";
                    }
                }
                catch (Exception ex)
                {
                    if (alertUser)
                        MessageBox.Show("The connection to the database could not be made.\n\n" + ex.ToString(),
                            f.parent.TitleText + " - Test Connection Fail");
                    break;
                }

                f.tempConnectString = connectionString;

                if (addToServers)
                {
                    if (db != "(none selected)")
                    {
                        if (addToServers)
                            f.parent.AppSQL.AddDatabase(uid, pwd, server, db);

                        if (alertUser)
                            MessageBox.Show("Database added successfully.\n\n" + os, f.parent.TitleText + " - Database Added");
                        f.Close();
                    }
                    else
                    {
                        if (alertUser)
                            MessageBox.Show("Select database to add",f.parent.TitleText);
                    }

                }
                else
                {
                    if (alertUser)
                        MessageBox.Show("User " + uid + " successfully connected to database " + db + " on server " + server + ".\n\n" + os,
                            f.parent.TitleText + " - Test Connection Success");
                }

                break;
            }

            try
            {
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                if (alertUser)
                    MessageBox.Show("The connection to the database was unable to succesfully close.\n\n" + ex.ToString(), f.Text);
            }

            return;*/
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var v = (int)(this.percentCompl * 1000);
            if (this.percentCompl * 1000 > 1000)
                v = 1000;

            if (this.percentCompl > 0)
            {
                var percent = "" + Math.Round(this.percentCompl.Value * 100, 1) + "%";
                this.labelMsg.Text = this.loadMsg;
                this.labelPercent.Text = percent;
            }
            this.progressBar1.Value = v;
        }
        private void LoadingForm_FormClosed(object sender, FormClosedEventArgs e)
        {

            if (this.percentCompl < 1 && this.worker != null)
            {
                this.worker.CancelAsync();
                this.parent.ClearLoadingForm();
            }

            this.Enabled = false;
            this.Visible = false;
            this.Hide();

            //parent.InitAll();
            //parent.PostEventFire = true;

            parent.Enabled = true;
            parent.Show();
            parent.BringToFront();
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void buttonTestConnection_Click(object sender, EventArgs e)
        {
            AppWorking.TestConnection(this, true, false);
        }
        private void buttonAddServer_Click(object sender, EventArgs e)
        {
            AppWorking.TestConnection(this, true, true);
            this.parent.tabBlastControl_SelectedTabIndexChanged(null, null);
        }

        private void comboBoxDatabases_DropDown(object sender, EventArgs e)
        {
            if (this.textBox4.Text == "" || this.textBoxUsername.Text == "" || this.textBoxPassword.Text == "")
                return;

            string myConnectionString = "SERVER=" + this.textBox4.Text + ";UID=" + this.textBoxUsername.Text + ";PASSWORD=" + this.textBoxPassword.Text + ";";

            var sa = this.parent.AppSQL.SQL_GetDatabasesFromServer(myConnectionString, true);

            this.comboBoxDatabases.Items.Clear();

            if (sa == null || sa.Count == 0)
                return;

            foreach (string s in sa)
                this.comboBoxDatabases.Items.Add(s);

            return;
        }



    }
}
