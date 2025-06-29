using lang_to_xliff;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace format_converter
{
    public partial class Form1 : Form
    {
        string file1 = "", file2 = "", saveloc;
        int strcount1 = 0, strcount2 = 0, trcount2 = 0, utrcount2 = 0;
        XmlDocument xliff = new XmlDocument();
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private void button1_Click(object sender, EventArgs e)
        {
            if (file1 != "" && file2 != "") {
                button1.Text = lang_to_xliff.strings.button_cancel_text;
                openFilesToolStripMenuItem.Enabled = false;
                button1.Click += BTN_Cancel;
                if(!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
            }
            else if (MessageBox.Show(lang_to_xliff.strings.popup_nofileselect_desc, lang_to_xliff.strings.popup_nofile_title, MessageBoxButtons.YesNo) == DialogResult.Yes) LoadFiles();
        }

        private void BTN_Cancel(object sender, EventArgs e)
        {
            if (MessageBox.Show(lang_to_xliff.strings.popup_opruns_desc, lang_to_xliff.strings.popup_opruns_title, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                backgroundWorker1.CancelAsync();
                button1.Text = lang_to_xliff.strings.button_convert_text;
                openFilesToolStripMenuItem.Enabled = false;
                button1.Click += button1_Click;
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork +=
                 backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted +=
                backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.ProgressChanged +=
                backgroundWorker1_ProgressChanged;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        void cancelAsyncButton_Click(object sender,
        EventArgs e)
        {
            // Cancel the asynchronous operation.
            backgroundWorker1.CancelAsync();
        }

        void backgroundWorker1_DoWork(object sender,
        DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation
            // to the Result property of the DoWorkEventArgs
            // object. This is will be available to the 
            // RunWorkerCompleted eventhandler.
            e.Result = ConvertFiles(worker, e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang_to_xliff.Properties.Settings.Default.Lang);
            Localize(this);
            DumbLocalize();
        }

        private void Localize(Control item)
        {
            string t;
            if (item.Tag != null)
            {
                t = strings.ResourceManager.GetString((string)item.Tag);
                if (t.Length > 0)
                    item.Text = t;
            }
            if (item.HasChildren) foreach (Control child in item.Controls) Localize(child);
        }
        private void DumbLocalize()
        {
            toolStripStatusLabel1.Text = lang_to_xliff.strings.text_ready;
            fileToolStripMenuItem.Text = lang_to_xliff.strings.main_toolbar_file;
            openFilesToolStripMenuItem.Text = lang_to_xliff.strings.main_file_open;
            currentModeToolStripMenuItem.Text = string.Format(lang_to_xliff.strings.main_toolbar_mode, lang_to_xliff.strings.mode_lang_xliff);
            languageToolStripMenuItem.Text = lang_to_xliff.strings.main_toolbar_lang;
        }

        private void românăToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lang_to_xliff.Properties.Settings.Default.Lang = "ro";
            lang_to_xliff.Properties.Settings.Default.Save();
            Form1_Load(this, e);
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lang_to_xliff.Properties.Settings.Default.Lang = "en";
            lang_to_xliff.Properties.Settings.Default.Save();
            Form1_Load(this, e);
        }

        void backgroundWorker1_RunWorkerCompleted(
        object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                _ = MessageBox.Show(lang_to_xliff.strings.popup_error_title, e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                toolStripProgressBar1.Value = 0;
                toolStripStatusLabel1.Text = lang_to_xliff.strings.text_cancel;
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                MessageBox.Show(lang_to_xliff.strings.popup_convdone_desc);
            }

            // Enable the UpDown control.
            fileToolStripMenuItem.Enabled = true;
        }

        void backgroundWorker1_ProgressChanged(object sender,
        ProgressChangedEventArgs e)
        {
            int prog = (int)((float)e.ProgressPercentage / strcount1 * 100);
            int val2 = strcount1;
            if (prog > 100 && strcount2 > strcount1) { prog = (int)((float)e.ProgressPercentage / strcount2 * 100); val2 = strcount2; }
            toolStripProgressBar1.Value = prog;
            toolStripStatusLabel1.Text = string.Format(lang_to_xliff.strings.text_progress, e.ProgressPercentage.ToString(), val2.ToString());
        }

        private void LoadFiles()
        {
            //file1 = ""; file2 = "";
            strcount1 = 0; strcount2 = 0; trcount2 = 0; utrcount2 = 0;
            string line1 = "";
            string[] details;
            toolStripStatusLabel1.Text = lang_to_xliff.strings.text_sel1;
            OpenFileDialog fdiag = new OpenFileDialog
            {
                Title = lang_to_xliff.strings.text_sel1_title,
                Filter = lang_to_xliff.strings.filter_lang_desc + "|*.lang",
                InitialDirectory = "%homedir%",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            StreamReader S;
            StringBuilder P;
            if (fdiag.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = lang_to_xliff.strings.text_load1;
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                toolStripProgressBar1.Value = 100;
                S = new StreamReader(fdiag.FileName);
                P = new StringBuilder(file1);
                line1 = S.ReadLine();
                while (line1 != null)
                {
                    P.AppendLine(line1);
                    if (line1.Length > 0) if (line1[0] != '#') strcount1++;
                    line1 = S.ReadLine();
                }
                file1 = P.ToString();
                toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
                toolStripProgressBar1.Value = 50;
                label3.Text = fdiag.FileName;
                if(textBox1.Text == lang_to_xliff.strings.main_text_notext)textBox1.Clear();
                details = new string[] { fdiag.FileName, Convert.ToString(strcount1) };
                textBox1.AppendText(string.Format(lang_to_xliff.strings.text_desc_lang, details) + Environment.NewLine);
                toolStripStatusLabel1.Text = lang_to_xliff.strings.text_sel2;
                fdiag.Title = lang_to_xliff.strings.text_sel2_title;
                fdiag.Filter = lang_to_xliff.strings.filter_xliff_desc + "| *.xliff";
                if (fdiag.ShowDialog() == DialogResult.OK)
                {
                    toolStripStatusLabel1.Text = lang_to_xliff.strings.text_load2;
                    toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                    toolStripProgressBar1.Value = 100;
                    saveloc = fdiag.FileName;
                    S = new StreamReader(fdiag.FileName);
                    P = new StringBuilder(file2);
                    line1 = S.ReadLine();
                    while (line1 != null)
                    {
                        P.AppendLine(line1);
                        if (line1.IndexOf("resname") != -1) strcount2++;
                        else if (line1.IndexOf("state=\"final\"") != -1 || line1.IndexOf("state=\"translated\"") != -1 || line1.IndexOf("state=\"needs - review - translation\"") != -1) trcount2++;
                        else if (line1.IndexOf("needs-translation") != -1) utrcount2++;
                        line1 = S.ReadLine();
                    }
                    xliff.Load(fdiag.FileName);
                    file2 = P.ToString();
                    label4.Text = fdiag.FileName;
                    toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
                    details = new string[] { fdiag.FileName, Convert.ToString(strcount2), Convert.ToString(trcount2), Convert.ToString(utrcount2) };
                    textBox1.AppendText(string.Format(lang_to_xliff.strings.text_desc_xliff, details) + Environment.NewLine);
                }
                else MessageBox.Show(lang_to_xliff.strings.popup_nofile_desc, lang_to_xliff.strings.popup_warn_title);
                toolStripStatusLabel1.Text = lang_to_xliff.strings.text_ready;
                toolStripProgressBar1.Value = 0;
                textBox1.AppendText(string.Format(lang_to_xliff.strings.text_desc_length1, Convert.ToString(file1.Length)) + Environment.NewLine);
                textBox1.AppendText(string.Format(lang_to_xliff.strings.text_desc_length2, Convert.ToString(file2.Length)) + Environment.NewLine);
            }
            else MessageBox.Show(lang_to_xliff.strings.popup_nofile_desc, lang_to_xliff.strings.popup_warn_title);
            toolStripStatusLabel1.Text = lang_to_xliff.strings.text_ready;
        }
        private void openFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadFiles(); 
        }

        private int ConvertFiles(BackgroundWorker worker, DoWorkEventArgs e)
        {
            string line1, line11, line12;
            int loc1 = 0, tloc = 0, tms = 0;
            long loc2 = 0;
            StringReader fconvert1 = new StringReader(file1);
            XmlNode newTransl, transl;
            XmlNodeList nodes = xliff.GetElementsByTagName("trans-unit"), nodes2;
            nodes2 = nodes;
            //textBox1.AppendText(nodes.Count.ToString() + " entries");
            if (MessageBox.Show(lang_to_xliff.strings.popup_convstart_desc, lang_to_xliff.strings.popup_convstart_title, MessageBoxButtons.YesNo) == DialogResult.Yes) {
                line1 = fconvert1.ReadLine();
                while (line1 != null)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                    }
                    else if (line1.Length > 0) if (line1[0] != '#')
                        {
                            line11 = line1.Substring(line1.IndexOf("=")+1);
                            line1 = line1.Substring(0, line1.IndexOf("="));
                            loc1++;
                            if (line11.IndexOf('&') != -1) { line11 = line11.Substring(0, line11.IndexOf('&')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&amp;" + line12; }
                            if (line11.IndexOf('<') != -1) { line11 = line11.Substring(0, line11.IndexOf('<')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&lt;" + line12; }
                            if (line11.IndexOf('>') != -1) { line11 = line11.Substring(0, line11.IndexOf('>')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&gt;" + line12; }
                            if (line11.IndexOf('\'') != -1) { line11 = line11.Substring(0, line11.IndexOf('\'')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&apos;" + line12; }
                            if (line11.IndexOf('"') != -1) { line11 = line11.Substring(0, line11.IndexOf('"')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&quot;" + line12; }
                            for (int i = 0; i < Convert.ToInt64(nodes.Count.ToString()); i++)
                                if (nodes[i].Attributes["resname"].Value == line1){ 
                                    loc2++;
                                    if(tloc == 0)foreach(XmlNode nb in nodes[i])
                                        {
                                            if(nb.Name == "target")tloc = tms;
                                            tms++;
                                        }
                                    transl = nodes[i].ChildNodes.Item(tloc);
                                    if (transl != null) {
                                        newTransl = transl;
                                        newTransl.InnerXml = line11;
                                        newTransl.Attributes["state"].Value = "translated";
                                        nodes[i].ReplaceChild(transl, newTransl);
                                    }
                                    worker.ReportProgress(loc1);
                                    break;
                                }
                        }
                    line1 = fconvert1.ReadLine();
                }
                int tll = 0;
                XmlNode Child0 = xliff.GetElementsByTagName("xliff")[0];
                foreach (XmlNode a in Child0)
                {
                    if (a.Name == "file")
                        break;
                    tll++;
                }
                XmlNode Child1 = Child0.ChildNodes.Item(tll);
                XmlNode Child2 = Child1;
                tll = 0;
                foreach (XmlNode a in Child1)
                {
                    if (a.Name == "body")
                        break;
                    tll++;
                }
                XmlNode Child3 = Child1.ChildNodes.Item(tll);
                XmlNode Child4 = Child3;
                for (int i = 0; i < Convert.ToInt64(nodes.Count.ToString()); i++)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        Child3.ReplaceChild(nodes[i], nodes2[i]);
                        worker.ReportProgress(i);
                    }
                }
                Child1.ReplaceChild(Child3, Child4);
                Child0.ReplaceChild(Child1, Child2);
                xliff.ReplaceChild(Child0, xliff.GetElementsByTagName("xliff")[0]);
                xliff.Save(saveloc);
            }
            return 0;
        }

    }
}
