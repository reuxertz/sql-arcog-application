using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MySql.Data.MySqlClient; 

using EvolutionTools;

namespace EvoMathProj
{
    using Color = System.Drawing.Color;
    
    public partial class AppForm : Form
    {
        public class AppBGWorker : BackgroundWorker
        {
            public List<string> Tags = new List<string>();
        }

        //Form Move Functions
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
         );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     // variables for box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        private const int WM_NCHITTEST = 0x84;          // variables for dragging the form
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        public class AppBase
        {
            protected AppForm _parentForm;
            protected RichTextBox _screen;

            public AppBase(RichTextBox screen, AppForm a)
            {
                this._parentForm = a;
                this._screen = screen;
            }
        }

        //Static
        public static Control FindFocusedControl(Control control)
        {
            ContainerControl container = control as ContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as ContainerControl;
            }
            return control;
        }
        public static string ProgName = "GEANNSEQ", ProgVersion = "1.1";
        public static int Timeout = 15;

        //Fields
        public string TitleText;
        protected AppWorking _loadingForm = null;
        protected TabControl _storedTabs = new TabControl();
        protected DateTime? _lastLoadCheck = null;
        protected int _waitThresh = 1000, _modernPadding = 0;
        protected bool _displayLoaded = false;
        protected System.Drawing.Point? _formSelLastPoint = null;

        //Apps
        public AppConsole AppConsole;
        public AppPython AppPython;
        public AppSQL AppSQL;

        //Properties
        public RichTextBox ConsoleTextBox
        {
            get
            {
                return this.rtbConsole;
            }
        }

        //Setters
        public void ClearLoadingForm()
        {
            this._loadingForm = null;
        }

        //Constructor
        public AppForm()
        {
            this.SetStyle(
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer |
              ControlStyles.ResizeRedraw, true);

            InitializeComponent();

            this.Visible = false;
            this.Hide();

            this.MaximumSize = new Size(Screen.FromControl(this).Bounds.Width, Screen.FromControl(this).Bounds.Height - 40);
            this.CenterToScreen();
            this.WindowState = FormWindowState.Normal;
            this._waitThresh = 1000;
            this.formTimer_Tick(null, null);
            this.modernToolStripMenuItem_Click(null, null);
            this.Text = ProgName + " v" + ProgVersion;
            this.TitleText = this.Text;
            this.statusToolStripMenuItem_Click(null, null);
            this.pictureBox1.Image = null;
            this.pictureBox2.Image = null;
            this.Text += " - " + Environment.UserName;
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                this.Text += " (Administrator)";
            this.labelMainFormTitle.Text = this.Text;


            //if (this.modernToolStripMenuItem.Checked)
            //    this.WindowState = FormWindowState.Maximized;


            this.AppConsole = new AppConsole(this.rtbConsole, this);
            this.AppPython = new AppPython(this.rtbPythonEditor, this);
            this.AppSQL = new AppSQL(this.rtbSQLEditor, this);

            this.InitAll();
            this.Show();
            this.Visible = true;

            //this.tabBlast.Show();
            //this.tabBlast.Focus();

            //this.LocalBlast(true);
        }
        public void InitAll()
        {
            this.InitView();
            this.InitBlastGUI();
            this.InitConnections();
        }
        public void InitBlastGUI()
        {
            var fp = Properties.Settings.Default.LocalSequenceFilePath;

            if (fp != "")
            {
                var fdl = fp.Split('\\');
                var fd = fp.Remove(fp.Length - fdl[fdl.Length - 1].Length);
                this.textBoxLBDWD.Text = Properties.Settings.Default.LocalDatabaseDirectory;
                this.textBoxLBDFP.Text = fp;
                this.textBoxLBEXED.Text = Properties.Settings.Default.NCBIExecutablesDirectory;
            }
            else
            {
                this.textBoxLBDWD.Text = Environment.CurrentDirectory;
                this.textBoxLBDFP.Text = Environment.CurrentDirectory;
                this.textBoxLBEXED.Text = Environment.CurrentDirectory;
            }
            this.tabBlastControl.SelectedIndex = 0;
            this.toolStripBlastButton.Text = "Perform Blast - " + tabBlastControl.SelectedTab.Text;
            this.proteinBlastToolStripMenuItem.PerformClick();
            this.explorerToolStripMenuItem_Click(null, null);
        }
        public void InitView()
        {
            //this.explorerToolStripMenuItem.Checked = !this.splitTreeView.Panel1Collapsed;
            //this.projectToolStripMenuItem.Checked = !this.splitProject.Panel2Collapsed;

            for (int i = 0; i < VIEWToolStripMenuItem.DropDownItems.Count; i++)
            {
                var c = VIEWToolStripMenuItem.DropDownItems[i];

                if (!(c is ToolStripMenuItem))
                    continue;

                this.ViewToolStripMenuItem_Click((ToolStripMenuItem)c, null);
            }
        }
        public void InitConnections()
        {
            var l = Program.ConnectionStrings.ToList<string>();
            for (int i = 0; i < l.Count; i++)
            {
                //AppLoading
                var tc = this.AppSQL.TestConnection(l[i], true);
            }
        }

        //Form Event Functions
        private void formTimer_Tick(object sender, EventArgs e)
        {
            if (this.AppConsole != null)
            {
                var nl = this.AppConsole.CLF.NextLines;
                if (nl != null)
                {
                    var l = this.rtbConsole.Lines.ToList<string>();
                    l.InsertRange(l.Count - 1, nl);
                    this.rtbConsole.Lines = l.ToArray<string>();
                    this.AppConsole.updateSelection();
                }
                else if (this.rtbConsole.ReadOnly)
                {
                    this.rtbConsole.ReadOnly = false;
                }
            }
            if (this.PostEventFire)
            {
                this.PostEventFire = false;
                if (this.PostEventWait > 0)
                {
                    this.PostEventWait--;
                }
                
                if (this.PostEventWait == 0)
                {
                    this.Enabled = true;
                    //this.InitAll();
                    this.Show();
                    this.Focus();
                    if (this.PostEnable != null && this.PostEnable(sender, e))
                    {
                        this.PostEnable = null;
                    }
                }
            }
        }

        private void resetConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.rtbConsole.Clear();
        }
        private void clearScreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = this.rtbConsole.Lines.ToList<string>().GetRange(0, 3);
            l.AddRange(this.rtbConsole.Lines.ToList<string>().GetRange(this.rtbConsole.Lines.Length - 2, 2));
            this.rtbConsole.Lines = l.ToArray<string>();
        }
        private void contextConsole_Opening(object sender, CancelEventArgs e)
        {
            var b = !this.rtbConsole.ReadOnly;

            this.clearScreeToolStripMenuItem.Enabled = b;
            this.resetConsoleToolStripMenuItem.Enabled = b;
        }

        //Blast Controls  
        private void buttonSaveBlast_Click(object sender, EventArgs e)
        {
            if (this.richTextBoxLocalBlast.Lines.Length == 0)
                return;

            var f = new SaveFileDialog();
            f.Filter = "Text|*.txt";
            var r = f.ShowDialog();

            if (f.FileName == "")
                return;

            Management.WriteTextToFile(f.FileName, this.richTextBoxLocalBlast.Lines, false);
        }  
        private void tabBlastControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripBlastButton.Text = "Perform Blast - " + this.tabBlastControl.SelectedTab.Text;
        }       
        private void blastTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tsmi = (ToolStripMenuItem)sender;

            foreach (ToolStripMenuItem c in toolStripBlastTypeButton.DropDownItems)
                ((ToolStripMenuItem)c).Checked = false;
            tsmi.Checked = true;
            toolStripBlastTypeButton.Text = "Blast Type - " + tsmi.Text.Split(' ')[0];
        }     
        public void tabBlastControl_SelectedTabIndexChanged(object sender, EventArgs e)
        {
            buttonArCOGBlast.Enabled = false;
            if (tabBlastControl.SelectedTab.Name == "tabSQL")
            {
                buttonArCOGBlast.Enabled = true;
                
                var ss = this.AppSQL.GetDatabaseFromConnectionStrings();

                //Check if match
                var match = true;
                for (int i = 0; i < ss.Length; i++)
                {
                    if (!listBoxBlastSQLDB.Items.Contains(ss[i]))
                    {
                        match = false;
                        this.listBoxBlastSQLDB.Items.Clear();
                        this.labelDBField.Text = "-";
                        this.labelDBOutput.Text = "-";
                        this.listBoxBlastSQLTable_SelectedIndexChanged(null, null);
                        break;
                    }
                }

                //If a match in the database connection strings doesn't exist in sql box, refresh to servers
                if (!match)
                {
                    for (int i = 0; i < ss.Length; i++)
                    {
                        listBoxBlastSQLDB.Items.Add(ss[i]);
                    }
                }

                //If none, return
                if (ss.Length == 0)
                    return;

                //Add all
            }
        }
        private void buttonBlastConnectDB_Click(object sender, EventArgs e)
        {
            this.toolStripAddSQLDB.PerformClick();
        }
        private void buttonBlastRemoveDB_Click(object sender, EventArgs e)
        {
            if (this.listBoxBlastSQLDB.SelectedItem != null)
            {
                this.AppSQL.RemoveDBConnectionString((string)this.listBoxBlastSQLDB.SelectedItem);
                this.listBoxBlastSQLDB.Items.Remove(this.listBoxBlastSQLDB.SelectedItem);
                this.listBoxBlastSQLTable.Items.Clear();
                this.labelTableField.Text = "-";
                this.labelTableOutput.Text = "-";
                this.labelDBField.Text = "-";
                this.labelDBOutput.Text = "-";
            }
        }
        private void buttonBlastRefreshDB_Click(object sender, EventArgs e)
        {
            this.listBoxBlastSQLDB.Items.Clear();
            this.listBoxBlastSQLTable.Items.Clear();
            this.labelTableField.Text = "-";
            this.labelTableOutput.Text = "-";
            this.labelDBField.Text = "-";
            this.labelDBOutput.Text = "-";

            this.InitConnections();
            this.tabBlastControl_SelectedTabIndexChanged(sender, e);
        }
        private void listBoxBlastSQL_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (this.listBoxBlastSQLDB.SelectedItem != null && (string)this.listBoxBlastSQLDB.SelectedItem != "" &&
                !this.labelDBField.Text.Contains("-") && !this.labelDBOutput.Text.Contains("-"))
            {
                var DBFields = this.labelDBField.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                var DBOutputs = this.labelDBOutput.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                var ind = DBFields.IndexOf("Database:");
                var lastLoaded = DBOutputs[ind];

                if ((string)this.listBoxBlastSQLDB.SelectedItem == lastLoaded)
                {
                    
                    return;
                }

            }
            else
            {*/
                this.labelDBField.Text = "-";
                this.labelDBOutput.Text = "-";

                if (this.listBoxBlastSQLDB.SelectedItem == null || (string)this.listBoxBlastSQLDB.SelectedItem == "")
                    return;

            //}

            var s = this.AppSQL.GetConnectionStringFromDBName((string)this.listBoxBlastSQLDB.SelectedItem);

            if (s.ToLower().Contains("password"))
            {
                int startI = s.ToLower().IndexOf("password");
                int endI = s.IndexOf(";", startI);

                if (endI != -1)
                    s = s.Remove(startI, endI - startI);
                else
                    s = s.Substring(0, startI);
            }

            s = s.Replace(";", "=");
            //s = s.Replace("=", ":\t\t");

            var ss = s.Split('=');

            this.labelDBField.Text = "FIELD:\n";
            this.labelDBOutput.Text = "VALUE:\n";
            for (int i = 0; i < ss.Length; i++)
            {
                if (ss[i] == "")
                {
                    continue;
                }

                if ((i + 2) % 2 == 0)
                    this.labelDBField.Text += ("" + ss[i][0]).ToUpper() + ss[i].Substring(1, ss[i].Length - 1) + ":\n";
                else
                    this.labelDBOutput.Text += ss[i] + "\n";
            }
        }
        private void listBoxBlastSQLTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.labelTableField.Text = " -";
            this.labelTableOutput.Text = " -";

            if (this.listBoxBlastSQLDB.SelectedItem == null || this.listBoxBlastSQLTable.SelectedItem == null)
            {
                if (this.listBoxBlastSQLDB.SelectedItem == null)
                    this.listBoxBlastSQLTable.Items.Clear();

                return;
            }


            var rowCount = this.AppSQL.GetTotalRowsInTable((string)this.listBoxBlastSQLDB.SelectedItem, (string)this.listBoxBlastSQLTable.SelectedItem);

            this.labelTableField.Text = "FIELD:\n";
            this.labelTableOutput.Text = "VALUE:\n";

            this.labelTableField.Text += "Row Count:\n";
            this.labelTableOutput.Text += rowCount + "\n";
        }
        private void listBoxBlastSQLDB_Click(object sender, EventArgs e)
        {
            var dbName = (string)this.listBoxBlastSQLDB.SelectedItem;

            if (dbName == null)
                return;

            if (this.listBoxBlastSQLDB.SelectedItem != null && (string)this.listBoxBlastSQLDB.SelectedItem != "" &&
                !this.labelDBField.Text.Contains("-") && !this.labelDBOutput.Text.Contains("-") && this.listBoxBlastSQLTable.Items.Count > 0)
            {
                var DBFields = this.labelDBField.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                var DBOutputs = this.labelDBOutput.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                var ind = DBFields.IndexOf("Database:");
                var lastLoaded = DBOutputs[ind];

                if ((string)this.listBoxBlastSQLDB.SelectedItem == lastLoaded)
                    return;
            }
            //else
            {

                this.labelTableField.Text = " -";
                this.labelTableOutput.Text = " -";
                this.listBoxBlastSQLTable.Items.Clear();

                if (this.listBoxBlastSQLDB.SelectedItem == null || (string)this.listBoxBlastSQLDB.SelectedItem == "")
                    return;
            }

            var tables = this.AppSQL.GetTables(dbName);

            for (int i = 0; i < tables.Count; i++)
            {
                var cols = this.AppSQL.GetColumnsInfoFromTable(true, dbName, tables[i]);

                if (cols == null || cols.Count == 0)
                    continue;

                bool hasFasta = false, hasSequence = false;
                for (int j = 0; j < cols.Count; j++)
                {
                    if (cols[j].ToLower() == "fasta header")
                        hasFasta = true;

                    if (cols[j].ToLower() == "fasta sequence")
                        hasSequence = true;

                    if (hasFasta && hasSequence)
                        break;
                }

                if (!hasFasta || !hasSequence)
                    continue; ;

                listBoxBlastSQLTable.Items.Add(tables[i]);
                var x = this.listBoxBlastSQLDB.SelectedIndex;
                this.listBoxBlastSQLDB.SelectedIndex = -1;
                this.listBoxBlastSQLDB.SelectedIndex = x;
            }
        }
        private void searchArcogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int res = -1;
            var b = int.TryParse(this.richTextBoxLocalBlast.SelectedText, out res);

            if (b)
                this.SearchReturnArcogs(res, sender != null);
        }
        private void toolStripBlastButton_ButtonClick(object sender, EventArgs e)
        {
            this.AssignArcogs = false;

            if (this.toolStripBlastButton.Text.Contains("Local"))
                this.LocalBlast(true);

            if (this.toolStripBlastButton.Text.Contains("URL"))
                this.URLBlast();

            if (this.toolStripBlastButton.Text.Contains("SQL"))
                this.SQLBlast(true);
        }
        private void buttonArCOGBlast_ButtonClick(object sender, EventArgs e)
        {
            //Perform clicks on form, basic automation

            if (!listBoxBlastSQLDB.Items.Contains((object)"arcogs"))
                return;

            listBoxBlastSQLDB.SelectedItem = listBoxBlastSQLDB.Items[listBoxBlastSQLDB.Items.IndexOf((object)"arcogs")];
            listBoxBlastSQL_SelectedIndexChanged(null, null);
            listBoxBlastSQLDB_Click(null, null);

            if (!listBoxBlastSQLTable.Items.Contains((object)"aaseq"))
                return;

            listBoxBlastSQLTable.SelectedItem = listBoxBlastSQLTable.Items[listBoxBlastSQLTable.Items.IndexOf((object)"aaseq")];
            listBoxBlastSQLTable_SelectedIndexChanged(null, null);

            tabControl.SelectedTab = tabBlast;
            tabBlast.Show();

            toolStripBlastButton_ButtonClick(null, null);
            this.AssignArcogs = true;

            return;
        }
        private void menuItemAutoArCOGBlast_Click(object sender, EventArgs e)
        {
            //var fastas = new List<string>();
            //var seqs = new List<string>();

            /*for (int j = 0; false && j < this.textBoxBlastSeq.Lines.Length; j++)
            {
                var l = this.textBoxBlastSeq.Lines[j];
                if (l.Contains(">"))
                {
                    fastas.Add(l);
                    seqs.Add("");
                }
                else
                    seqs[seqs.Count - 1] += l;

                this.textBoxBlastSeq.Lines = new string[] { fastas[0], seqs[0] };
                fastas.RemoveAt(0);
                seqs.RemoveAt(0);
                this.queveFastas = fastas;
                this.queveSeqs = seqs;

                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(.1 + .1 * (j / this.textBoxBlastSeq.Lines.Length));
            }*/

            this.AssignArcogs = true;
            this.buttonArCOGBlast_ButtonClick(null, null);

            //this.PostEventWait = 2;
            //this.PostEnable = new PostEvent(false, null);
        }

        private bool menuItemAutoArCOGBlast_Click_Post(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                var databasePath = this.textBoxLBDWD.Text;
                var outBlastFilePath = databasePath + @"~tempBlast.txt";
                var t = Management.GetTextFromFile(outBlastFilePath);
                var res = this.SearchReturnBlastDetails(t);

                var r = new List<List<string>>();

                for (int i = 0; i < res.Count; i++)
                {


                    var eThresA = 0.0;
                    var eThresB = 0.0;
                    if (this.toolStripTextBoxEThresh.Text.Contains("e"))
                    {
                        var splE = this.toolStripTextBoxEThresh.Text.Split('e');
                        eThresA = double.Parse(splE[0]);
                        eThresB = double.Parse(splE[1]);
                    }
                    else
                        eThresA = double.Parse(this.toolStripTextBoxEThresh.Text);

                    var cpE = this.toolStripTextBoxCover.Text.Replace("%", "");
                    var covPerc = double.Parse(cpE);

                    var eRA = 0.0;
                    var eRB = 0.0;
                    if (res[i][1].Contains("e"))
                    {
                        var splR = res[i][1].Split('e');
                        eRA = double.Parse(splR[0]);
                        eRB = double.Parse(splR[1]);
                    }
                    else
                        eThresA = double.Parse(res[i][1]);
                    var cpR = res[i][2].Replace("%", "");
                    var covPercR = double.Parse(cpR);

                    bool covSat = covPercR > covPerc;
                    bool eSat = eRB < eThresB || (eRB == eThresB && eRA < eThresA);

                    if (covSat && eSat && (res[i][3] != "" || demoModeToolStripMenuItem.Checked))
                        r.Add(res[i]);
                }

                if (r.Count > 0)
                {
                    var s = "";
                    var nl = "";
                    var title = "Prot-id:\t\tE-value:\t\tCover%:\t\tArCOG-id:\t\n";
                    for (int i = 0; i < r.Count; i++)
                    {
                        if (i != 0)
                            continue;

                        s += "Prot-id:\t\tE-value:\t\tCover%:\t\tArCOG-id:\t\n" +
                             r[i][0] + "\t\t" + r[i][1] + "\t\t" + r[i][2] + "\t\t" + r[i][3] + "\n";
                        nl = r[i][0] + "," + r[i][1] + "," + r[i][2] + "," + r[i][3];
                    }

                    MessageBox.Show("The following arcogs passed required thersholds and can be assigned to the query submitted:\n\n" + s, this.TitleText);

                    var wd = Program.LocalDatabaseDirectory;
                    var arcOut = wd + @"ArCOG-Output.txt";
                    var text = new string[] { "" };
                    if (File.Exists(arcOut))
                        text = Management.GetTextFromFile(arcOut);
                    else
                        text = new string[] { title };

                    if (text.Length == 0 || (text.Length == 1 && text[0] == ""))
                        text = new string[] { title };

                    var l = text.ToList<string>();
                    l.Add(nl);
                    Management.WriteTextToFile(arcOut, l.ToArray<string>(), false);                         

                }


                return true;
            }

            return this.Enabled;
        }

        //Blast Functions
        public List<string> queveFastas, queveSeqs;
        public delegate bool PostEvent(object sender, EventArgs e);
        public PostEvent PostEnable;
        public bool AssignArcogs = false, PostEventFire = false;
        public int PostEventWait = 0;
        private bool LocalBlast(bool perform)
        {

            var filePath = this.textBoxLBDFP.Text;
            var databasePath = this.textBoxLBDWD.Text;
            var exePath = this.textBoxLBEXED.Text;
            var outBlastFilePath = databasePath + @"~tempLocalBlast.txt";

            this.pictureBox1.Image = null;
            this.pictureBox2.Image = null;
            this.pictureBox3.Image = null;
            this.pictureBox4.Image = null;
            this.richTextBoxLocalBlast.Text = "";
            this.BlastSplitContainer.Panel2.Enabled = false;
            if (!File.Exists(filePath))
            {
                this.pictureBox1.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox2.Image")));
                MessageBox.Show("Invalid sequence file path for database construction.", this.TitleText + " - Error");
                return false;
            }
            else
                this.pictureBox1.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox1.Image")));

            if (!Directory.Exists(databasePath))
            {
                this.pictureBox2.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox2.Image")));
                MessageBox.Show("Invalid path for local blast database.", this.TitleText + " - Error");
                return false;
            }
            else
                this.pictureBox2.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox1.Image")));

            if (!Directory.Exists(exePath) || !File.Exists(exePath + @"\makeblastdb.exe"))
            {
                this.pictureBox4.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox2.Image")));
                MessageBox.Show("Invalid path for NCBI executables.", this.TitleText + " - Error");
                return false;
            }
            else
                this.pictureBox4.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox1.Image")));

            if (this.textBoxBlastSeq.Text == "")
            {
                this.pictureBox3.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox2.Image")));
                MessageBox.Show("Invalid sequence for blast.", this.TitleText + " - Error");
                return false;
            }
            else
                this.pictureBox3.Image = ((System.Drawing.Image)
                    (new System.ComponentModel.ComponentResourceManager(typeof(AppForm)).GetObject("pictureBox1.Image")));

            if (File.Exists(outBlastFilePath))
                File.Delete(outBlastFilePath);
            var t = this.toolStripBlastButton.Text.Split(' ');
            var tt = t[t.Length - 1];

            /*if (tt == "Local")
            else if (tt == "URL")
                bgw.DoWork += bgw_DoWork_LocalBlast;
            else
            {
                MessageBox.Show("Invalid blast source.", "Error");
                return false;
            }*/

            if (!perform)
                return true;

            //Create the loading form, which runs the blast in the background.
            if (tt == "SQL")
                tt = "local " + tt;

            tt = tt.Replace("Local", "local");

            var bgw = new BackgroundWorker();
            bgw.DoWork += bgw_DoWork_LocalBlast;
            bgw.RunWorkerCompleted += bgw_RunWorkerCompleted_LocalBlast;
            this._loadingForm = new AppWorking("Performing " + tt + " blast...", this, bgw);
            this._loadingForm.RunWorker();
            this.Enabled = false;

            return true;
        }
        private void URLBlast()
        {
            //ht tp://www.ncbi.nlm.nih.gov/blast/Blast.cgi?CMD=Put\&QUERY=SSWWAHVEMGPPDPILGVTEAYKRDTNSKK&PROGRAM=blastp&FILTER=L\&DATABASE=nr&ALIGNMENTS=100&DESCRIPTIONS=100";
            var r = @"http://www.ncbi.nlm.nih.gov/blast/Blast.cgi?" +
                @"CMD=Put\" +
                @"&QUERY=SSWWAHVEMGPPDPILGVTEAYKRDTNSKK&PROGRAM=blastp" +
                @"&FILTER=L\" +
                @"&DATABASE=nr" +
                @"&ALIGNMENTS=100" +
                @"&DESCRIPTIONS=100";
            var client = new WebClient();
            var content = client.DownloadString(r);

            Thread.Sleep(1000);

            //ht tp://www.ncbi.nlm.nih.gov/blast/Blast.cgi?CMD=Get&RID=UWDA524901S&FORMAT_OBJECT=Alignment&DESCRIPTIONS=200&ALIGNMENTS=200
            r = @"http://www.ncbi.nlm.nih.gov/blast/Blast.cgi?" +
                "CMD=Get" +
                "&RID=UWDA524901S" +
                "&FORMAT_OBJECT=Alignment" +
                "&DESCRIPTIONS=200" +
                "&ALIGNMENTS=200";
            content = client.DownloadString(r);
            return;

        }
        private bool SQLBlast(bool perform)
        {
            var db = (string)this.listBoxBlastSQLDB.SelectedItem;
            var table = (string)this.listBoxBlastSQLTable.SelectedItem;

            if (db == null || db == "" || table == null || table == "")
            {
                if (this.listBoxBlastSQLDB.Items.Count == 0)
                    MessageBox.Show("A database containing a fasta table is required.", this.TitleText + " - Error");
                else
                    MessageBox.Show("Invalid database query parameters.", this.TitleText + " - Error");
                return false;
            }

            if (!this.LocalBlast(false))
                return false;

            if (!perform)
                return true;

            //Create the loading form, which runs the blast in the background.
            var bgw = new BackgroundWorker();
            Program.SelectedDatabase = db;
            Program.SelectedTable = table;
            bgw.DoWork += bgw_DoWork_SQLBlast;
            bgw.RunWorkerCompleted += bgw_RunWorkerCompleted_SQLBlast;
            this._loadingForm = new AppWorking("Downloading SQL database for local blast...", this, bgw);
            this._loadingForm.RunWorker();
            this.Enabled = false;

            return true;

            //var cols = this.AppSQL.GetColumnsInTable
        }
        private List<List<string>> SearchReturnArcogs(int domainid, bool promptUser)
        {
            var res = domainid;
            var ll = (List<List<string>>)null;
            List<List<string>> r1 = null, r2 = null;

            r1 = this.AppSQL.GetRowsInTable("arcogs", "arcogs", "domain-id", "" + res, null);
            r2 = this.AppSQL.GetRowsInTable("arcogs", "arcogs", null);

            var y = new List<List<string>>();
            //var x = (from string[] sa in r2
            //         where sa[0] == ("" + res)
            //         select sa).ToList<List<string>>();

            for (int i = 0; i < r2.Count; i++)
            {
                var r2i = r2[i];
                if (r2i[0] == ("" + res))
                    y.Add(r2i);
            }


            //var y = x.ToList<string[]>();         
            if (y.Count > 0 && r1.Count == 0)
            {
                ll = new List<List<string>>();
                var s = "";// y.Count + " match found:\n\n";

                for (int i = 0; i < y.Count; i++)
                {
                    if (y[i][6] == "")
                        continue;

                    if (promptUser)
                        s += "domain-id: " + y[i][0] + "\t\tarcogId: " + y[i][6];
                    else
                        ll.Add(new List<string> { y[i][0], y[i][6] });
                }

                if (s == "")
                    s = "No match was found\t\t\t\t";

                if (promptUser)
                    MessageBox.Show(s, "ArCOG Database Search");
            }

            return ll;
        }
        private List<List<string>> SearchReturnBlastDetails(string[] t)
        {
            var state = "QueryStart>";

            var r = new List<List<string>>();

            for (int i = 0; i < t.Length; i++)
            {
                if (state == "QueryStart>" || t[i].IndexOf(">") != -1)
                {
                    var nextGTindex = t[i].IndexOf(">");

                    if (nextGTindex == -1)
                        continue;

                    var nextUPindex1 = t[i].IndexOf("|", nextGTindex + 1);
                    var nextUPindex2 = t[i].IndexOf("|", nextUPindex1 + 1);

                    var domainID = t[i].Substring(nextUPindex1 + 1, (nextUPindex2 - nextUPindex1) - 1);

                    state = "QueryStats=";
                    r.Add(new List<string> { domainID });

                    continue;
                }

                int eqCount = 0;
                if (state == "QueryStats=")
                {
                    var startIndex = 0;
                    var nextEQindex = t[i].IndexOf("=", startIndex);

                    while (nextEQindex != -1)
                    {
                        var nextCommaIndex = t[i].IndexOf(",", nextEQindex + 1);

                        if (nextCommaIndex == -1)
                            nextCommaIndex = t[i].Length - 1;

                        var ssName = t[i].Substring(startIndex, (nextEQindex - startIndex)).Trim();
                        var ssVal = t[i].Substring(nextEQindex + 1, (nextCommaIndex - nextEQindex) - 1).Trim();

                        if (ssName == "Expect")
                        {
                            r[r.Count - 1].Add(ssVal);
                        }

                        if (ssName == "Identities")
                        {
                            var pi1 = ssVal.IndexOf("(");
                            var pi2 = ssVal.IndexOf(")");
                            r[r.Count - 1].Add(ssVal.Substring(pi1 + 1, (pi2 - pi1) - 2).Trim());
                        }

                        startIndex = nextCommaIndex + 1;
                        nextEQindex = t[i].IndexOf("=", startIndex);
                    }
                }
            }

            for (int i = 0; i < r.Count; i++)
            {
                int x = -1;
                int.TryParse(r[i][0], out x);
                var ll = this.SearchReturnArcogs(x,  false);

                if (ll.Count == 0)
                    r[i].Add("");
                else
                    r[i].Add(ll[0][1]);
            }

            return r;

        }
        private void AssignArcog(string sampleFasta, string[] txt, bool autoClose)
        {            
            var res = this.SearchReturnBlastDetails(txt);

            var r = new List<List<string>>();

            for (int i = 0; i < res.Count; i++)
            {
                var eThresA = 0.0;
                var eThresB = 0.0;
                if (this.toolStripTextBoxEThresh.Text.Contains("e"))
                {
                    var splE = this.toolStripTextBoxEThresh.Text.Split('e');
                    eThresA = double.Parse(splE[0]);
                    eThresB = double.Parse(splE[1]);
                }
                else
                    eThresA = double.Parse(this.toolStripTextBoxEThresh.Text);

                var cpE = this.toolStripTextBoxCover.Text.Replace("%", "");
                var covPerc = double.Parse(cpE);

                var eRA = 0.0;
                var eRB = 0.0;
                if (res[i][1].Contains("e"))
                {
                    var splR = res[i][1].Split('e');
                    eRA = double.Parse(splR[0]);
                    eRB = double.Parse(splR[1]);
                }
                else
                    eThresA = double.Parse(res[i][1]);
                var cpR = res[i][2].Replace("%", "");
                var covPercR = double.Parse(cpR);

                bool covSat = covPercR > covPerc;
                bool eSat = eRB < eThresB || (eRB == eThresB && eRA < eThresA);

                if (covSat && eSat && (res[i][3] != "" || demoModeToolStripMenuItem.Checked))
                    r.Add(res[i]);
            }

            if (r.Count > 0)
            {
                var s = "";
                var nl = "";
                var title = "Date,Prot-id,E-value,Cover%,ArCOG-id";
                for (int i = 0; i < r.Count; i++)
                {
                    if (i != 0)
                        continue;

                    s += "Prot-id:\t\tE-value:\t\tCover%:\t\tArCOG-id:\t\n" +
                            r[i][0] + "\t\t" + r[i][1] + "\t\t" + r[i][2] + "\t\t" + r[i][3] + "\n";
                    nl = DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString() + "," + r[i][0] + "," + r[i][1] + "," + r[i][2] + "," + r[i][3];
                }

                var in1 = sampleFasta.IndexOf("|", 0);
                var in2 = sampleFasta.IndexOf("|", in1 + 1);
                var smpID = sampleFasta.Substring(in1 + 1, (in2 - in1) - 1);
                var ot = "The query " + smpID + " passed required thresholds of e-value: " + this.toolStripTextBoxEThresh.Text + " and cover: " + this.toolStripTextBoxCover.Text +
                    " for the following arcog(s):\n\n" + s;
                //if (autoClose)
                //    AutoClosingMessageBox.Show(ot, this.TitleText, 10000);
                //else
                    AutoClosingMessageBox.Show(ot, this.TitleText, 3000);

                var wd = Program.LocalDatabaseDirectory;
                var arcOut = wd + @"ArCOG-Output.txt";
                var text = new string[] { "" };
                if (File.Exists(arcOut))
                    text = Management.GetTextFromFile(arcOut);
                else
                    text = new string[] { title };

                if (text.Length == 0 || (text.Length == 1 && text[0] == ""))
                    text = new string[] { title };

                var l = text.ToList<string>();
                l.Add(nl);
                Management.WriteTextToFile(arcOut, l.ToArray<string>(), false);
            }
        }

        //Background Blast Worker
        void bgw_RunWorkerCompleted_LocalBlast(object sender, RunWorkerCompletedEventArgs e)
        {
            var backgroundWorker = (BackgroundWorker)sender;
            if (backgroundWorker.CancellationPending)
                return;

            if (this._loadingForm == null)
                return;

            this._loadingForm.Close();

            if (this._loadingForm == null)
                return;

            if (!this._loadingForm.IsProgressComplete())
            {
                this._loadingForm = null;
                return;
            }
            this._loadingForm = null;

            var dbp = Program.LocalDatabaseDirectory;// this.textBoxLBDWD.Text;
            var outBlastFilePath = dbp + @"~tempBlast.txt";
            var b = File.Exists(outBlastFilePath);

            var tt = Management.GetTextFromFile(outBlastFilePath);
            this.richTextBoxLocalBlast.Lines = tt;
            this.BlastSplitContainer.Panel2.Enabled = true;
            //this.Show();
            //this.Invalidate();
        }
        void bgw_DoWork_LocalBlast(object sender, DoWorkEventArgs e)
        {
            //This c# event (function) performs the blast functions from NCBI executeables.
            //It is ran in the background to remove interference with the form

            //Background worker - C# object that runs a function on a separate thread and notifies when ready
            var backgroundWorker = (BackgroundWorker)sender;

            //Gathers all the necessary stringd containing paths
            var filePath = Program.LocalSequenceFilePath;// this.textBoxLBDFP.Text;
            var databasePath = Program.LocalDatabaseDirectory;// this.textBoxLBDWD.Text;
            var exePath = Program.NCBIExecutablesDirectory;// this.textBoxLBEXED.Text;

            //Program.LocalSequenceFilePath = fp;
            //Program. = dbp;
            //Program.NCBIExecutablesDirectory = exp;
            //makeblastdb -in NveProt.fas -dbtype 'prot' -out NveProt -name -NveProt

            //Name for local db
            var outFilePath = databasePath + @"~tempLocalBlastDB";

            //Check for part of db, if exists, remove to force new db creation
            if (File.Exists(outFilePath + ".pin"))
                File.Delete(outFilePath + ".pin");
            
            //Create new Database through c# command line simulator/wrapper. Wait for completion
            this.AppConsole.CLF.RunCommandLine("makeblastdb -in " + filePath + " -dbtype prot -out " + outFilePath, exePath);
            this._loadingForm.SetProgressPercent(.1);

            if (backgroundWorker.CancellationPending)
                return;
            //Thread.Sleep(500);

            //Get blast fasta sequence from textbox
            /*
            var fasta = "";
            var i = 1;
            if (this.textBoxBlastSeq.Lines[0].Contains('>'))
                fasta = this.textBoxBlastSeq.Lines[0];
            else
                i = 0;
            var seq = "";
            for (i = i; i < this.textBoxBlastSeq.Lines.Length; i++)
            {
                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(.1 + .1 * (i / this.textBoxBlastSeq.Lines.Length));
                seq += this.textBoxBlastSeq.Lines[i];
            }*/

            var fastas = new List<string>();
            var seqs = new List<string>();

            for (int j = 0; j < this.textBoxBlastSeq.Lines.Length; j++)
            {
                var l = this.textBoxBlastSeq.Lines[j];
                if (l.Contains(">"))
                {
                    fastas.Add(l);
                    seqs.Add("");
                }
                else
                    seqs[seqs.Count - 1] += l;

                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(.1 + .1 * (j / this.textBoxBlastSeq.Lines.Length));
            }

            //Verify sequence existd
            if (fastas.Count == 0 || seqs.Count == 0 || fastas.Count != seqs.Count)
                return;

            //Create txt file of blast query
            var outQueryFilePath = databasePath + @"~tempBlastQuery.txt";

            //Get blast type from form
            var t = "";
            if (this.proteinBlastToolStripMenuItem.Checked)
                t = "blastp";
            if (this.nucleotideBlastToolStripMenuItem.Checked)
                t = "blastn";
            if (t == "")
            {
                MessageBox.Show("Invalid blast type.", this.TitleText + " - Error");
                return;
            }

            //Code that checks and waits for the database creation
            var dt = DateTime.Now;
            while ((from x in Directory.GetFiles(databasePath)
                    where x.Contains(outFilePath + ".pin")
                    select x).ToArray<string>().Length == 0)
            {
                if (DateTime.Now.Subtract(dt).TotalSeconds > AppForm.Timeout)
                    return;

                continue;
            }

            for (int i = 0; i < fastas.Count; i++)
            {//////////////
                this._loadingForm.SetProgressPercent(.2);
                //Management.WriteTextToFile(outQueryFilePath, new string[] { fasta, seq }, false);
                Management.WriteTextToFile(outQueryFilePath, new string[] { fastas[i], seqs[i] }, false);
                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(.3);

                //blastn –query text_query.txt –db refseq_rna.00 –out output.txt
                //blastp -db C:\Ryan\ProteinProj\ProjectFiles\~tempLocalBlastDB -query C:\Ryan\ProteinProj\ProjectFiles\tempLocalBlastQuery.txt -out C:\Ryan\ProteinProj\ProjectFiles\~tempLocalBlast.txt
                //  blastp 
                //  -db C:\Ryan\ProteinProj\ProjectFiles\~tempLocalBlastDB
                //  -query C:\Ryan\ProteinProj\ProjectFiles\tempLocalBlastQuery.txt 
                //  -out C:\Ryan\ProteinProj\ProjectFiles\~tempLocalBlast.txt
                var outBlastFilePath = databasePath + @"~tempBlast.txt";
                //Program.CLF.RunCommandLine(r, NCBIExs);

                if (backgroundWorker.CancellationPending)
                    return;

                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(.5);
                //Thread.Sleep(500);
                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(.7);

                //Run Command line wrapper to perform blast
                this.AppConsole.CLF.RunCommandLine(t +
                    @" -db " + outFilePath + //"C:\Ryan\ProteinProj\ProjectFiles\~tempLocalBlastDB " + 
                    @" -query " + outQueryFilePath + //" C:\Ryan\ProteinProj\ProjectFiles\tempLocalBlastQuery.txt " + 
                    @" -out " + outBlastFilePath, exePath); //C:\Ryan\ProteinProj\ProjectFiles\~tempLocalBlast.txt", NCBIExs);

                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(.9);
                dt = DateTime.Now;

                //Wait for blast output text file to be created
                while (!File.Exists(outBlastFilePath))
                {
                    if (DateTime.Now.Subtract(dt).TotalSeconds > AppForm.Timeout)
                        return;

                    continue;
                }

                //Leave adequete time (2 secs roughly) for command line to complete and close
                Thread.Sleep(2000);
                if (this._loadingForm != null)
                    this._loadingForm.SetProgressPercent(1);

                //Do Arcog/Multi-Fasta
                if (this.AssignArcogs)
                {
                    if (fastas.Count > 1)
                        this.AssignArcog(fastas[i], Management.GetTextFromFile(outBlastFilePath), true);
                    else
                        this.AssignArcog(fastas[i], Management.GetTextFromFile(outBlastFilePath), false);
                }


            }/////////////

            return;
        }

        private void bgw_RunWorkerCompleted_SQLBlast(object sender, RunWorkerCompletedEventArgs e)
        {
            var backgroundWorker = (BackgroundWorker)sender;

            if (this._loadingForm == null)
                return;

            this._loadingForm.Close();
            if (!this._loadingForm.IsProgressComplete())
            {
                this._loadingForm = null;
                return;
            }
            this._loadingForm = null;

            if (!backgroundWorker.CancellationPending)
                this.LocalBlast(true);
        }
        private void bgw_DoWork_SQLBlast(object sender, DoWorkEventArgs e)
        {
            var filePath = Program.LocalSequenceFilePath;
            var dbName = Program.SelectedDatabase;
            var tblName = Program.SelectedTable;

            var rows = this.AppSQL.GetRowsInTable(dbName, tblName, this._loadingForm);
            var dbp = Program.LocalDatabaseDirectory;// this.textBoxLBDWD.Text;
            var tempSQLFasta = dbp + @"~tempSQLDB" + "_" + dbName + "_" + tblName + ".fa";

            if (rows != null && rows.Count > 1 && rows[0][0].Contains('>'))
            {
                if (File.Exists(tempSQLFasta))
                    File.Delete(tempSQLFasta);

                var f = new List<string>();
                foreach (var s in rows)
                    f.AddRange(s);

                Management.WriteTextToFile(tempSQLFasta, f.ToArray<string>(), false);
            }

            Program.LocalSequenceFilePath = tempSQLFasta;

            if (Program.LocalSequenceFilePath != tempSQLFasta)
            {
                MessageBox.Show("Temporary SQL file unable to be constructed.", this.TitleText + " - Error");
                return;
            }

            return;
        }

        //Local File Functions
        private void buttonSeqFileDialog_Click(object sender, EventArgs e)
        {
            var f = new OpenFileDialog();
            f.Filter = "FASTA|*.fa;*.fasta|Genbank|*.gbk";
            var r = f.ShowDialog();

            if (f.FileName == "")
                return;

            this.textBoxLBDFP.Text = f.FileName;
        }
        private void buttonBlastFileDialog_Click(object sender, EventArgs e)
        {
            var f = new OpenFileDialog();
            f.Filter = "FASTA|*.fa;*.fasta|Genbank|*.gbk";
            var r = f.ShowDialog();

            if (f.FileName == "")
                return;

            var s = Management.GetTextFromFile(f.FileName);
            this.textBoxBlastSeq.Lines = s;
        }
        private void buttonWorkDirDialog_Click(object sender, EventArgs e)
        {
            var f = new FolderBrowserDialog();
            if (Directory.Exists(this.textBoxLBDWD.Text))
                f.SelectedPath = this.textBoxLBDWD.Text;
            var r = f.ShowDialog();

            if (f.SelectedPath == "")
                return;

            this.textBoxLBDWD.Text = f.SelectedPath + @"\";
        }
        private void buttonNCBIEXEDir_Click(object sender, EventArgs e)
        {
            var f = new FolderBrowserDialog();
            if (Directory.Exists(this.textBoxLBEXED.Text))
                f.SelectedPath = this.textBoxLBEXED.Text;
            var r = f.ShowDialog();

            if (f.SelectedPath == "")
                return;

            this.textBoxLBEXED.Text = f.SelectedPath + @"\";
        }

        //Text Change Events
        private void textBoxLBDFP_TextChanged(object sender, EventArgs e)
        {
            Program.LocalSequenceFilePath = this.textBoxLBDFP.Text;
            this.textBoxLBDFP.Text = this.textBoxLBDFP.Text;
            this.pictureBox1.Image = null;
        }
        private void textBoxLBDWD_TextChanged(object sender, EventArgs e)
        {
            Program.LocalDatabaseDirectory = this.textBoxLBDWD.Text;
            this.pictureBox2.Image = null;

            //if (this.textBoxLBDWD.Text != Program.LocalDatabaseDirectory)
            //    this.textBoxLBDWD.Text = Program.LocalDatabaseDirectory;
        }
        private void textBoxLBEXED_TextChanged(object sender, EventArgs e)
        {
            Program.NCBIExecutablesDirectory = this.textBoxLBEXED.Text;
            this.pictureBox4.Image = null;

            //if (this.textBoxLBEXED.Text != Program.NCBIExecutablesDirectory)
            //    this.textBoxLBEXED.Text = Program.NCBIExecutablesDirectory;
        }
        private void textBoxBS_TextChanged(object sender, EventArgs e)
        {
            this.pictureBox3.Image = null;
        }

        //Form Functions
        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void ViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = ((ToolStripMenuItem)sender);

            if (!t.Checked)
            {
                for (int i = 0; i < tabControl.TabPages.Count; i++)
                {
                    var tab = tabControl.TabPages[i];
                    if (t.Name.Remove(0, 8) == tab.Name.Remove(0, 3))
                    {
                        this.tabControl.Controls.Remove(tab);
                        this._storedTabs.Controls.Add(tab);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _storedTabs.TabPages.Count; i++)
                {
                    var tab = _storedTabs.TabPages[i];
                    if (t.Name.Remove(0, 8) == tab.Name.Remove(0, 3))
                    {
                        this._storedTabs.Controls.Remove(tab);
                        this.tabControl.Controls.Add(tab);
                    }
                }
            }

            tabControl.Visible = tabControl.TabPages.Count != 0;
        }
        private void modernToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.menuStripMainForm.Visible = this.modernToolStripMenuItem.Checked;
            var x = this.WindowState;
            Thread.Sleep(100);
            this.Hide();

            if (this.modernToolStripMenuItem.Checked)
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.panelMain.BorderStyle = BorderStyle.FixedSingle;
                this.panelMain.Padding = new Padding(this._modernPadding);
            }
            else
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                this.panelMain.BorderStyle = BorderStyle.None;
                this.panelMain.Padding = new Padding(0);
            }

            this.Show();

        }
        private void buttonCloseApp_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void buttonMaxApp_Click(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Maximized;
                this.buttonMaxApp.Image = global::EvoMathProj.Properties.Resources.menu_maximize_multi;
                this.panelMain.Padding = new Padding(0);
                //buttonMaxApp.Image = Application.Se
            }
            else
            {
                this.panelMain.Padding = new Padding(this._modernPadding);
                this.WindowState = FormWindowState.Normal;
                this.buttonMaxApp.Image = global::EvoMathProj.Properties.Resources.menu_maximize_single;
            }
        }
        private void buttonMinApp_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void menuStripMainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

            if (e.Clicks != 2)
                return; 

            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                if (this.WindowState != FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Maximized;

            return;

        }
        private void buttonExtend_Click(object sender, EventArgs e)
        {
            this.splitContainerBlastQuery.Panel1Collapsed = !this.splitContainerBlastQuery.Panel1Collapsed;
            this.buttonExtend.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
        private void explorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.splitContainerExplorer.Panel1Collapsed = !this.explorerToolStripMenuItem.Checked;
        }
        private void projectToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void toolStripButtonPythonFont_Click(object sender, EventArgs e)
        {
            var f = new FontDialog();
            f.MinSize = 6;
            f.MaxSize = 18;
            var r = f.ShowDialog();
            this.rtbPythonEditor.Font = f.Font;
        }
        private void stripButtonSQLRefresh_Click(object sender, EventArgs e)
        {
            this.AppSQL.RefreshServers();

            return;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var bgw = new BackgroundWorker();

            //if (tt == "Local")
            //    bgw.DoWork += bgw_DoWork_LocalBlast;
            //else if (tt == "URL")
            //    bgw.DoWork += bgw_DoWork_LocalBlast;
            //else
            //{
            //    MessageBox.Show("Invalid blast source.", "Error");
            //    return;
            //}

            //bgw.RunWorkerCompleted += bgw_RunWorkerCompleted_LocalBlast;
            this._loadingForm = new AppWorking(AppWorking.Type.LoadMySQLDatabase, "Provide the information below to connect to a SQL database.", this, bgw);
            this.Enabled = false;

            return;


            /*SqlConnection myConnection = new SqlConnection("user id=root;" +
                                       "password=rjw30181;server=localhost:3306;" +
                                       "Trusted_Connection=yes;" +
                                       "database=main; " +
                                       "connection timeout=3");*/

            var cs = "uid = admin;" +
                "pwd = bios455;" +
                "server = 127.0.0.1;" +
                "database = localhost; " +
                "connection timeout = 3";

            //Server=myServerAddress;Port=1234;Database=myDataBase;Uid=myUsername;
            //Pwd = myPassword;
            var server = "localhost";// "localhost";
            var database = "arcogs";
            var uid = "admin";
            var password = "bios455";
            var connectionString = "server=" + server + ";" + "database=" +
                database + ";" + "userid=" + uid + ";" + "password=" + password + ";" +
                "Connection Timeout=3";

            var sc = new MySqlConnection(connectionString);
            while (true)
            {
                var os = "";
                try
                {
                    using (sc)
                    {
                        sc.Open();
                        os += "ServerVersion: " + sc.ServerVersion + "\n";
                        os += "Database: " + sc.Database;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The connection to the database has timed out with the given parameters.\n\n" + ex.ToString(), this.Text);
                    break;
                }

                MessageBox.Show("User " + uid + " successfully connected to databse " + database + " on server " + server + ".\n\n" + os, this.Text);
                this.AppSQL.AddDatabase(uid, password, server, database);

                break;
            }

            try
            {
                sc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The connection to the database was unable to succesfully close.\n\n" + ex.ToString(), this.Text);
            }   

            return;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            /*
            SqlDataSourceEnumerator sqldatasourceenumerator1 = SqlDataSourceEnumerator.Instance;
            DataTable datatable1 = sqldatasourceenumerator1.GetDataSources();
            foreach (DataRow row in datatable1.Rows)
            {
                Console.WriteLine("****************************************");
                Console.WriteLine("Server Name:" + row["ServerName"]);
                Console.WriteLine("Instance Name:" + row["InstanceName"]);
                Console.WriteLine("Is Clustered:" + row["IsClustered"]);
                Console.WriteLine("Version:" + row["Version"]);
                Console.WriteLine("****************************************");
            }*/

            return;
        }

        private void AppForm_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var tv = (TreeView)sender;
            var n = tv.SelectedNode;
            var checkNodes = false;
            string[] curNodeNames = null;
            var imgIndex = 0;

            //if (n.IsExpanded)
            //{
            //    n.Collapse();
            //    return;
            //}

            //var n = (TreeNode)sender;

            if (n.Nodes.Count > 0 && !n.IsExpanded)
                return;


            if (n.Parent == null)
            {
                if (n.Name == "Computer")
                {
                    checkNodes = true;
                    curNodeNames = Directory.GetLogicalDrives();
                    imgIndex = 15;
                    /*curNodeNames
                    var drives = Directory.GetLogicalDrives().ToList<string>();

                    //n.Nodes.Clear();
                   
                    var ba = new bool[n.Nodes.Count];
                    for (int i = 0; i < drives.Count; i++)
                    {
                        var c = (from TreeNode tn in n.Nodes
                                 where tn.Name == drives[i] + @"\"
                                 select tn).ToArray<TreeNode>();
                        if (c.Length == 0)
                        {
                            //DirectoryInfo di = new DirectoryInfo(drives[i]);
                            var nn = new TreeNode();

                            nn.Name = drives[i] + @"\";
                            nn.Text = drives[i];
                            nn.ImageIndex = 15;
                            nn.SelectedImageIndex = 15;

                            n.Nodes.Add(nn);
                        }
                    } 
                    for (int i = 0; i < n.Nodes.Count; i++)
                    {
                        var name = n.Nodes[i].Name;
                        var nm = name.Substring(0, name.Length - 1);
                        if (!drives.Contains(nm))
                        {
                            n.Nodes.RemoveAt(i);
                            i--;
                        }
                    }
                    n.Expand();*/

                }
                else if (n.Name == "Network")
                {
                    checkNodes = true;
                    curNodeNames = AppSQL.GetServersFromConnectionStrings();
                    imgIndex = 17;
                }
            }
            else if (n.Parent.Name == "Network")
            {
                checkNodes = true;

                var servers = (from string s in this.AppSQL.GetConnectionStrings()
                               where s.Contains(n.Text)
                               select s).FirstOrDefault<string>();

                if (servers == null || servers == "")
                    return;

                curNodeNames = AppSQL.SQL_GetDatabasesFromServer(servers, false).ToArray<string>();
                imgIndex = 5;
            }
            else if (n.Parent.Parent != null && n.Parent.Parent.Name == "Network")
            {
                checkNodes = true;

                var dbs = this.AppSQL.GetTables(n.Text);

                if (dbs == null || dbs.Count == 0)
                    return;

                curNodeNames = dbs.ToArray<string>();
                imgIndex = 19;
            }
            else if (n.Parent.Parent.Parent != null && n.Parent.Parent.Parent.Name == "Network")
            {
                checkNodes = true;

                var cols = this.AppSQL.GetColumnsInfoFromTable(true, n.Parent.Text, n.Text);

                if (cols == null || cols.Count == 0)
                    return;

                curNodeNames = cols.ToArray<string>();
                imgIndex = 20;
            }
            else if (n.Parent.Text == "Computer" || Directory.Exists(n.Parent.Text))
            {
                string[] dirs = null;
                try
                {
                    dirs = Directory.GetDirectories(n.Name);
                }
                catch (Exception ex)
                {
                    return;
                }

                if (dirs == null)
                    return;

                checkNodes = true;
                curNodeNames = dirs;

                /*
                n.Nodes.Clear();
                for (int i = 0; i < dirs.Length; i++)
                {
                    var nn = new TreeNode();
                    var sn = dirs[i].Split('\\');

                    nn.Text = sn[sn.Length - 1];
                    nn.Name = dirs[i];

                    n.Nodes.Add(nn);
                }
                n.Expand();*/
            }

            if (checkNodes)
            {
                var ba = new bool[n.Nodes.Count];
                for (int i = 0; i < curNodeNames.Length; i++)
                {
                    var c = (from TreeNode tn in n.Nodes
                             where tn.Name == curNodeNames[i] + @"\"
                             select tn).ToArray<TreeNode>();
                    if (c.Length == 0)
                    {
                        //DirectoryInfo di = new DirectoryInfo(drives[i]);
                        var nn = new TreeNode();

                        nn.Name = curNodeNames[i] + @"\";
                        nn.Text = curNodeNames[i];
                        nn.ImageIndex = imgIndex;
                        nn.SelectedImageIndex = imgIndex;

                        var sn = nn.Name.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                        nn.Text = sn[sn.Length - 1];

                        n.Nodes.Add(nn);
                    }
                }
                for (int i = 0; i < n.Nodes.Count; i++)
                {
                    var name = n.Nodes[i].Name;
                    var nm = name.Substring(0, name.Length - 1);
                    if (!curNodeNames.Contains(nm))
                    {
                        n.Nodes.RemoveAt(i);
                        i--;
                    }
                }
                n.Expand();
            }

        }

        private void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            return;
            if (e.Clicks < 2)
            {
                return;
            }

            var tv = (TreeView)sender;
            var n = tv.SelectedNode;
            treeView1_AfterSelect(sender, new TreeViewEventArgs(n));
        }



        private System.Drawing.Point? _lastMP = null;
        private void panelMain_MouseHover(object sender, EventArgs e)
        {
            var mp = new EvolutionTools.Point(MousePosition.X, MousePosition.Y);
            if (this.Cursor == Cursors.Default)
            {
                if (this.modernToolStripMenuItem.Checked)
                {
                    var formCenter = new EvolutionTools.Point(this.Location).Add(new EvolutionTools.Point(this.Size).Divide(2));
                    var corner = new EvolutionTools.Point(this.Location.X + this.Size.Width, this.Location.Y);
                    var mpvect = mp.Subtract(formCenter);
                    var cnvect = mp.Subtract(formCenter);

                    //Angle mpa = 

                    if (mpvect.X >= formCenter.X)
                    {
                        //if (mpvect.Y >= formCenter.Y)
                        //this.Cursor = Cursors.

                    }
                    else
                    {


                    }

                    //if (cos >)
                    this.Cursor = Cursors.SizeAll;
                }
            }

            var nl = new System.Drawing.Point((int)mp.X, (int)mp.Y);

            if (this.Cursor != Cursors.Default && _lastMP != null)
            {
                this.Width += (nl.X - _lastMP.Value.X);
            }

            this._lastMP = nl;
            return;

        }
        private void panelMain_MouseLeave(object sender, EventArgs e)
        {
            if (this.Cursor == Cursors.SizeAll)
            {
                this.Cursor = Cursors.Default;
                this._lastMP = null;
            }
        }
        private void panelMain_MouseMove(object sender, MouseEventArgs e)
        {

        }

        public void AppForm_MouseClick(object sender, MouseEventArgs e)
        {
            var c = AppForm.FindFocusedControl(this);

            if (c is RichTextBox)
            {
                var rct = (RichTextBox)c;

                var charIndex = rct.GetCharIndexFromPosition(Cursor.Position);
                var lineIndex = rct.GetLineFromCharIndex(charIndex);
                
                var sum = 0;
                for (int i = 0; i < lineIndex; i++)
                    sum += rct.Lines[i].Length;

                var col = charIndex - sum;
                var pad = 12;
                this.toolStripStatusLabel1.Text = 
                    ("Ln " + lineIndex).PadRight(pad) + 
                    ("Col " + col).PadRight(pad) + 
                    ("Chr " + charIndex).PadRight(pad);

                //var curLine = rct.Lines[];

                return;
            }
        }

        private void statusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.statusStrip1.Visible = this.statusToolStripMenuItem.Checked;
        }

        private void contextBlastOutput_Opening(object sender, CancelEventArgs e)
        {
            var selText = this.richTextBoxLocalBlast.SelectedText;

            if (selText == "")
            {
                e.Cancel = true;
                return;
            }

            var num = Convert.ToInt32(selText);
        }

        private void promptErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.AppConsole.CLF.PromptErrorLines = this.promptErrorsToolStripMenuItem.Checked;
        }









    }
}
