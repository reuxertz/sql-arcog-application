using EvolutionTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
namespace EvoMathProj
{
    public class AppConsole : AppForm.AppBase
    {
        public CommandPromptWrapper CLF;
        public RichTextBox Screen;
        public static string[] InitStrings
        {
            get
            {
                string lt = DateTime.Now.ToLongTimeString();
                return new string[]
				{
					AppForm.ProgName + " Console Interface - MS Command Line Wrapper",
					DateTime.Now.ToShortDateString() + " " + lt,
					""
				};
            }
        }
        public int RequestStartIndex
        {
            get
            {
                RichTextBox tb = this._parentForm.ConsoleTextBox;
                int i = tb.Lines[tb.Lines.Length - 2].IndexOf(">");
                int end = this.RequestEndIndex;
                return end - tb.Lines[tb.Lines.Length - 2].Length + i + 1;
            }
        }
        public int RequestEndIndex
        {
            get
            {
                return this.TextLength(this._parentForm.ConsoleTextBox.Lines) - 1;
            }
        }
        public int TextLength(string[] ss)
        {
            int sum = 0;
            int c = -1;
            for (int j = 0; j < ss.Length; j++)
            {
                string s = ss[j];
                int i = -2;
                while (i != -1)
                {
                    if (i < 0)
                    {
                        i = -1;
                    }
                    i = s.IndexOf("\\", i + 1);
                    if (i > -1)
                    {
                        c++;
                    }
                }
                sum += s.Length;
            }
            return sum + (ss.Length - 2);
        }
       
        public AppConsole(RichTextBox screen, AppForm a)
            : base(screen, a)
        {
            string arg_31_0 = "@#$B";
            CommandConsole.ArgClass[] array = new CommandConsole.ArgClass[4];
            array[2] = new CommandLibrary.StateSequence(new char?('$'));
            this.CLF = new CommandConsole(arg_31_0, array, true, Program.ConsoleWorkingDirectory, AppConsole.InitStrings);
            this.CLF.OnWorkingDirectoryChanged += new CommandPromptWrapper.DirectoryChangedEvent(this.CLF_OnWorkingDirectoryChanged);
            a.ConsoleTextBox.Lines = new string[]
			{
				" "
			};
            a.ConsoleTextBox.KeyDown += new KeyEventHandler(this.richTextBox1_KeyDown);
            a.ConsoleTextBox.KeyUp += new KeyEventHandler(this.richTextBox1_KeyUp);
            a.ConsoleTextBox.MouseUp += new MouseEventHandler(this.richTextBox1_MouseUp);
        }
        
        public bool updateSelection()
        {
            return this.updateSelection(true);
        }
        public bool updateSelection(bool startOrEnd)
        {
            RichTextBox tb = this._parentForm.ConsoleTextBox;
            int end = this.RequestEndIndex;
            int start = this.RequestStartIndex;
            bool set = false;
            if (tb.SelectionStart - 1 < start || tb.SelectionStart > end)
            {
                set = true;
            }
            if (set)
            {
                if (startOrEnd)
                {
                    tb.Select(start, 0);
                }
                else
                {
                    tb.Select(end, 0);
                }

                //this._parentForm.AppForm_MouseClick(null, null);

                if (tb.SelectionStart == 0)
                    return false;
            }
            return set;
        }
        private void CLF_OnWorkingDirectoryChanged(CommandPromptWrapper clf, string newDir)
        {
            Program.ConsoleWorkingDirectory = newDir;
        }
        public void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            RichTextBox tb = this._parentForm.ConsoleTextBox;
            Keys i = e.KeyData;
            if (i == Keys.Up || i == Keys.Left || i == Keys.Right || i == Keys.Down || i == Keys.Delete || i == Keys.Back)
            {
                //this.updateSelection(false);
            }
        }
        public void richTextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.updateSelection();
        }
        public void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            this.updateSelection(false);
            RichTextBox tb = this._parentForm.ConsoleTextBox;
            Keys i = e.KeyCode;
            if (tb.ReadOnly)
            {
                e.Handled = true;
            }
            else if (i == Keys.Return || i == Keys.Enter)
            {
                List<string> ls = tb.Lines.ToList<string>();
                if (ls[ls.Count - 2] == "")
                {
                    ls.RemoveAt(ls.Count - 2);
                }
                string reqL = ls[ls.Count - 2];
                int j = reqL.IndexOf(">") + 1;
                string req = reqL.Substring(j, reqL.Length - j);
                string wd = reqL.Substring(0, j - 1);
                tb.Lines = ls.ToArray<string>();
                if (this.CLF.IsDone)
                {
                    this.CLF.RunCommandLine(req, wd);
                }
                this.updateSelection(false);
                this._parentForm.ConsoleTextBox.ReadOnly = true;
                e.Handled = true;
            }
            else if (i == Keys.Up || i == Keys.Left || i == Keys.Right || i == Keys.Down || i == Keys.Delete || i == Keys.Back)
            {
                int start = this.RequestStartIndex;
                int end = this.RequestEndIndex;
                if ((i == Keys.Back || i == Keys.Left) && tb.SelectionStart - 1 < start)
                {
                    e.Handled = true;
                }
                if (i == Keys.Up && tb.SelectionStart - 1 < start)
                {
                    e.Handled = true;
                }
                if ((i == Keys.Delete || i == Keys.Right || i == Keys.Down) && tb.SelectionStart + 1 > end)
                {
                    e.Handled = true;
                }
                if (i == Keys.Up)
                {
                    int d = 1;
                    if (i == Keys.Down)
                    {
                        d = -1;
                    }
                    int ci = tb.Lines[tb.Lines.Length - 2].IndexOf(">");
                    string t = tb.Lines[tb.Lines.Length - 2];
                    string r;
                    this.CLF.GetPreviousRequest(d, out r);
                    string[] k = tb.Lines;
                    k[tb.Lines.Length - 2] = t.Substring(0, ci + 1) + r;
                    tb.Lines = k;
                    this.updateSelection(false);
                    e.Handled = true;
                }
            }
        }
    }
}
