using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using EvolutionTools;

using System.Runtime.InteropServices;


namespace EvoMathProj
{
    static class Program
    {
        public static Random R = new Random();
        public static AppForm MainForm;
        public static bool RestartOnExit = false;

        //string refTypes, ArgClass[] cls, bool init, string initDir, string[] initStrings
        public static string MasterDir, ExeDir, ResourceDir;

        static void LoadSettings()
        {
            if (!Directory.Exists(Properties.Settings.Default.ConsoleWorkingDirectory))
            {
                Properties.Settings.Default.ConsoleWorkingDirectory = Environment.CurrentDirectory;
                Properties.Settings.Default.Save();
            }
        }

        //Temp Settings
        public static string SelectedDatabase, SelectedTable;

        //Settings
        public static string ConsoleWorkingDirectory
        {
            get
            {
                if (!Directory.Exists(Properties.Settings.Default.ConsoleWorkingDirectory))
                {
                    var cd = Environment.CurrentDirectory;
                    var spcd = cd.Split(new string[] { "\\" } , 4, StringSplitOptions.RemoveEmptyEntries);
                    Properties.Settings.Default.ConsoleWorkingDirectory = spcd[0] + "\\" + spcd[1] + "\\" + spcd[2];
                    Properties.Settings.Default.Save();
                }

                return Properties.Settings.Default.ConsoleWorkingDirectory;
            }
            set
            {
                if (!Directory.Exists(value))
                    return;

                Properties.Settings.Default.ConsoleWorkingDirectory = value;
                Properties.Settings.Default.Save();
            }
        }
        public static string NCBIExecutablesDirectory
        {
            get
            {
                if (!Directory.Exists(Properties.Settings.Default.NCBIExecutablesDirectory))
                {
                    var cd = Environment.CurrentDirectory;
                    var spcd = cd.Split(new string[] { "\\" }, 4, StringSplitOptions.RemoveEmptyEntries);
                    Properties.Settings.Default.NCBIExecutablesDirectory = spcd[0] + "\\" + spcd[1] + "\\" + spcd[2];
                    Properties.Settings.Default.Save();
                }

                return Properties.Settings.Default.NCBIExecutablesDirectory;
            }
            set
            {
                if (!Directory.Exists(value))
                    return;

                if (value.Substring(value.Length - 1, 1) != @"\")
                    value += @"\";

                Properties.Settings.Default.NCBIExecutablesDirectory = value;
                Properties.Settings.Default.Save();
            }
        }
        public static string LocalSequenceFilePath
        {
            get
            {
                if (!File.Exists(Properties.Settings.Default.LocalSequenceFilePath))
                {
                    var cd = Environment.CurrentDirectory;
                    var spcd = cd.Split(new string[] { "\\" }, 4, StringSplitOptions.RemoveEmptyEntries);
                    Properties.Settings.Default.LocalSequenceFilePath = spcd[0] + "\\" + spcd[1] + "\\" + spcd[2];
                    Properties.Settings.Default.Save();
                }

                return Properties.Settings.Default.LocalSequenceFilePath;
            }
            set
            {
                if (!File.Exists(value))
                    return;

                Properties.Settings.Default.LocalSequenceFilePath = value;
                Properties.Settings.Default.Save();
            }
        }
        public static string LocalDatabaseDirectory
        {
            get
            {
                if (!Directory.Exists(Properties.Settings.Default.LocalDatabaseDirectory))
                {
                    var cd = Environment.CurrentDirectory;
                    var spcd = cd.Split(new string[] { "\\" }, 4, StringSplitOptions.RemoveEmptyEntries);
                    Properties.Settings.Default.LocalSequenceFilePath = spcd[0] + "\\" + spcd[1] + "\\" + spcd[2];
                    Properties.Settings.Default.Save();
                }

                return Properties.Settings.Default.LocalDatabaseDirectory;
            }
            set
            {
                if (!Directory.Exists(value))
                    return;

                if (value.Substring(value.Length - 1, 1) != @"\")
                    value += @"\";

                Properties.Settings.Default.LocalDatabaseDirectory = value;
                Properties.Settings.Default.Save();
            }
        }
        public static List<string> ConnectionStrings
        {
            get
            {
                var r = new List<string>();

                if (Properties.Settings.Default.ConnectionStrings == null)
                    Properties.Settings.Default.ConnectionStrings = new System.Collections.Specialized.StringCollection();

                for (int i = 0; i < Properties.Settings.Default.ConnectionStrings.Count; i++)
                    r.Add(Properties.Settings.Default.ConnectionStrings[i]);

                return r;
            }            
            set
            {
                Properties.Settings.Default.ConnectionStrings.Clear();

                for (int i = 0; i < value.Count; i++)
                    if (!Properties.Settings.Default.ConnectionStrings.Contains(value[i]))
                        Properties.Settings.Default.ConnectionStrings.Add(value[i]);

                //Properties.Settings.Default.ConnectionStrings.AddRange(value);
                Properties.Settings.Default.Save();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            /*
            var s = "and       del       from      not       while " +
"as        elif      global    or        with " +
"assert    else      if        pass      yield " +
"break     except    import    print " +
"class     exec      in        raise " +
"continue  finally   is        return " +
"def       for       lambda    try";


            var ss = s.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var sss = "";
            foreach (string si in ss)
                sss += "\"" + si + "\"" + ", ";

            var ssss = sss.Remove(sss.Length - 2, 2);
            while (ssss.Contains("\\"))
                ssss = ssss.Remove(ssss.IndexOf("\\"));

            var cd = Environment.CurrentDirectory;

            if (Program.HandleArgs(args))
                return;
            /*
            //Expression e = new Expression();
            string[] orfs = Management.GetTextFromFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\ORFs.txt");
            string[] engl = Management.GetTextFromFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\English.txt");
            //string[] engl = Management.GetTextFromFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\Decameron.txt");

            string[] fsts = new string[(int)(orfs.Length / 2.0)], seqs = new string[(int)(orfs.Length / 2.0)];
            for (int i = 0; i < orfs.Length; i++)
                if ((i + 2) % 2 == 0)
                    fsts[(int)(i / 2.0)] = orfs[i];
                else
                    seqs[(int)(i / 2.0)] = orfs[i];


            String dnaA = "ACGT";
            for (int i = 0; false && 
                i < seqs.Length; i++)
            {
                var s = "";
                for (int j = 0; j < 1000; j++)
                {
                    s += dnaA[Program.R.Next(dnaA.Length)];
                }
                seqs[i] = s;
            }

            seqs = InfoMath.PrepareSample(seqs, true, true, true, true);
            engl = InfoMath.PrepareSample(engl, false, true, false, false);

            var seqA = new Alphabet(seqs, dnaA);
            //var englA = new Alphabet(engl, " ABCDEFGHIJKLMNOPQRSTUVWXYZ.");
            //var engl2A = Alphabet.AlphabetByOrder(engl, " ABCDEFGHIJKLMNOPQRSTUVWXYZ.", 2);

            //var seq2A = Alphabet.AlphabetByOrder(seqs, "ACGT", 2);
            //var seq3A = Alphabet.AlphabetByOrder(seqs, "ACGT", 3);

            //var p1 = seqA.MyInfo.GetStateProbability("ATG");
            //var p2 = seq2A.MyInfo.GetStateProbability("ATG");
            //var p3 = seq3A.MyInfo.GetStateProbability("ATG");
           // var p4 = seq3A.MyInfo.GetStateProbability("A");

            */
            var t = Management.GetTextFromFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj-1.fasta");
            for (int i = 0; i < t.Length; i++)
            {
                while(t[i].Contains(","))
                {
                    t[i] = t[i].Replace(",", ";");
                }
            }
            Management.WriteTextToFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj-1.fasta", t, false);

            var l = CommandLibrary.StateSequence.FastaToCSV(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj-1.fasta");
            Management.WriteTextToFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj-1.csv", l.ToArray<string>(), false);

            //Program.TEST(">Contig100.revised.gene1.mrna", @"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\ORFs.fasta");
            //Program.TEST("31543380", @"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj1.fasta");
            //var l2 = Program.CreateFastaCSV(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj1.fasta");
            //Management.WriteTextToFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\CHOPPED-dj-1.csv", l2.ToArray<string>(), false);

            Application.ApplicationExit += Application_ApplicationExit;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Program.MainForm = new AppForm();


            Program.HandleArgs(args, Program.MainForm);

            Application.Run(Program.MainForm);

            return;
        }

        private static void CreateFastaCSV(string fastaPath, out List<string> csv)
        {
            //var l = CommandLibrary.StateSequence.FastaToCSV
            var text = Management.GetTextFromFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj-1.fasta");

            List<string> heads = new List<string>(), seqs = new List<string>();
            CommandLibrary.StateSequence.FastaFileToArrays(text, out heads, out seqs);

            List<List<string>> final = new List<List<string>> { heads, seqs, 
                //id field          id value            refType             revVal
                new List<string>(), new List<string>(), new List<string>(), new List<string>(),
                //prespecies        species             post species             
                new List<string>(), new List<string>(), new List<string>() };

            //">gi|14480676|gb|AAE62638.1| Sequence 1 from patent US 6197940"

            for (int i = 0; i < heads.Count; i++)
            {
                var s = heads[i].Split('|');


                final[2].Add(s[0]);
                final[3].Add(s[1]);
                final[4].Add(s[2]);
                final[5].Add(s[3]);

                var p1 = s[4].IndexOfAny(new char[] { '[' });
                var p2 = s[4].LastIndexOf(']');

                string preSpecies = s[4], species = "", postSpecies= "";

                if (p1 != -1 && p2 != -1)
                {
                    preSpecies = s[4].Substring(0, p1);
                    species = s[4].Substring(p1 + 1, p2 - (p1 + 1)).Replace("'", "");
                    postSpecies = s[4].Substring(p2 + 1, s[4].Length - (p2 + 1));
                }

                final[6].Add(preSpecies);
                final[7].Add(species);
                final[8].Add(postSpecies);

                continue;
            }

            csv = new List<string>();
            List<List<int>> counts = new List<List<int>>();
            List<string> specTypes = new List<string>();
            List<int> specTypeCounts = new List<int>();
            List<string> protTypes = new List<string>();
            List<int> protTypeCounts = new List<int>();
            for (int i = 0; i < final[0].Count; i++)
            {
                var s = "";

                for (int j = 0; j < final.Count; j++)
                {
                    s += final[j][i];

                    if (j < final.Count - 1)
                        s += ",";

                    if (j == 7)
                    {
                        if (!protTypes.Contains(final[j][i]))
                        {
                            protTypes.Add(final[j][i]);
                            protTypeCounts.Add(1);
                        }
                        else
                            protTypeCounts[protTypes.IndexOf(final[j][i])]++;
                    } 
                    if (j == 8)
                    {
                        if (!specTypes.Contains(final[j][i]))
                        {
                            specTypes.Add(final[j][i]);
                            specTypeCounts.Add(1);
                        }
                        else
                            specTypeCounts[specTypes.IndexOf(final[j][i])]++;
                    }
                }

                csv.Add(s);
            }

            //out csv;
        }
        private static void TEST(string id, string f)
        {
            var testSeqStateLength = 2;
            var avgWindowLength = 5;

            //Base Sequences
            var text = Management.GetTextFromFile(f);
            List<string> heads, seqs;
            //var f = Management.GetTextFromFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\ORFs.fasta");
            //var f = Management.GetTextFromFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\dj1.fasta");
            CommandLibrary.StateSequence.FastaFileToArrays(text, out heads, out seqs);
            var baseAlpha = new Alphabet(new int[] { 1, 2, 3 });
            baseAlpha.AddMessageSamples(seqs, null);
            baseAlpha.UpdateInfo();

            //Test seq
            var testSeqIndex = -1;
            for (int i = 0; i < heads.Count; i++)
                if (heads[i].Contains(id))
                {
                    testSeqIndex = i;
                    break;
                }
            if (testSeqIndex == -1)
                return;
            var testSeq = seqs[testSeqIndex];
            var testSeqFinal = new List<double>();
            var outputFileText = new List<string>();
            //var testSeqAlpha = new Alphabet(new int[] { 2 });
            for (int i = 0; i < testSeq.Length - (testSeqStateLength - 1); i += 1)
            {
                var curTestWord = "";
                var curTestSeqInd = new List<double>();
                var curTestSeqIndCum = 1.0;
                for (int j = 0; j < testSeqStateLength; j++)
                { 
                    curTestWord += testSeq[j + i];
                    curTestSeqInd.Add(baseAlpha.GetStateProbability("" + testSeq[j + i]));
                    curTestSeqIndCum *= curTestSeqInd[curTestSeqInd.Count - 1];
                }

                var probOfWord = baseAlpha.GetStateProbability(curTestWord);

                var y1 = (curTestSeqIndCum - probOfWord);
                var y2 = Math.Sqrt(y1 * y1);

                testSeqFinal.Add(y2);

                var valPI = 0.0;
                var valER = 0.0;
                if (testSeqFinal.Count >= avgWindowLength)
                {
                    valPI = 1.0;
                    var count = 0;
                    for (int j = testSeqFinal.Count - (avgWindowLength); j < testSeqFinal.Count; j++)
                    {
                        valPI *= testSeqFinal[j];
                        valER += testSeqFinal[j];
                        count++;
                    }

                    Math.Pow(valPI, 1.0 / count);
                    Math.Sqrt(valER / count);
                }

                outputFileText.Add(
                    (i + 1) + "," +             testSeq[i] + "," +          curTestWord + "," + 
                    probOfWord + "," +    curTestSeqIndCum + "," +    y1 + "," + 
                    y2 + "," +                  valER);
                //valPI);

                continue;
            }

            if (File.Exists(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\InfoOut.csv"))
                File.Delete(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\InfoOut.csv");
            Management.WriteTextToFile(@"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj\resources\data\InfoOut.csv", outputFileText.ToArray<string>(), false);

            return;
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            //Save Fields
            Program.ConnectionStrings = Program.MainForm.AppSQL.GetConnectionStrings();

            /*
            if (Program.RestartOnExit)
            {
                Program.RestartOnExit = false;

                Program.MainForm = new AppForm();
                Application.Run(Program.MainForm);
            }*/
            
        }

        private static void HandleArgs(string[] args, AppForm f)
        {
            //f.AppConsole.CLF.RunCommandLine("$a(orfs.txt) /literal /DNA", @"C:\Users\Ryan\Desktop\VSSD\vs projects\EvoMathProj");
            //while (!f.AppConsole.CLF.IsDone)
            //{

            //}
            //f.AppConsole.CLF.RunCommandLine("$a.stats()", @"C:\Ryan\ProteinProj\ProjectFiles");

            //throw new NotImplementedException();
        }
    }
}
