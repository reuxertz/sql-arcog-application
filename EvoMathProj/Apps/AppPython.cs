using EvolutionTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EvoMathProj
{
    public class AppPython : AppForm.AppBase
    {
        public string[] PythonKeyWords = new string[] 
        { "and", "del", "from", "not", "while", "as", "elif", "global", "or", "with", 
            "assert", "else", "if", "pass", "yield", "break", "except", "import", "print", 
            "class", "exec", "in", "raise", "continue", "finally", "is", "return", "def", 
            "for", "lambda", "try" };

        //Constructor
        public AppPython(RichTextBox screen, AppForm a)
            : base(screen, a)
        {
            screen.TextChanged += screen_TextChanged;
        }

        void screen_TextChanged(object sender, EventArgs e)
        {
            if (this._screen.SelectionStart == 0)
                return;

            var lastChar = this._screen.Text[this._screen.SelectionStart - 1];
            var match = "";

            for (int i = 0; i < PythonKeyWords.Length; i++)
            {
                var cnt = false;
                var w = " " + PythonKeyWords[i] + " ";
                for (int j = 0; j < w.Length; j++)
                {
                    if (this._screen.SelectionStart - (j + 1) < 0 || w[w.Length - (j + 1)] != this._screen.Text[this._screen.SelectionStart - (j + 1)])
                    {
                        cnt = true;
                        break;
                    }
                }

                if (cnt)
                    continue;

                match = PythonKeyWords[i];
                break;
            }

            if (match == "")
                return;

            var lastSel = this._screen.SelectionStart;
            this._screen.SelectionStart = lastSel - match.Length - 1;
            this._screen.SelectionLength = match.Length;
            var c = this._screen.SelectionColor;
            this._screen.SelectionColor = System.Drawing.Color.Blue;

            this._screen.SelectionLength = 0;
            this._screen.SelectionStart = lastSel;
            this._screen.SelectionColor = c;
                        
        }
    }
}
