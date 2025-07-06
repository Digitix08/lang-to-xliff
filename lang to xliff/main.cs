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
    public partial class main : Form
    {
        string file1 = "", fpath1, fpath2, LangFile = "AvailableLangs.txt", SelLang = "en";
        int strcount1 = 0, strcount2 = 0, trcount2 = 0, utrcount2 = 0;
        XmlDocument xliff = new XmlDocument();
        lang_to_xliff LtX = new lang_to_xliff();
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(LangFile))
            {
                StreamReader langs = new StreamReader(LangFile);
                string line = langs.ReadLine();
                while (line != null)
                {
                    if (line.Contains("#SelectedLang:")) SelLang = line.Substring(line.IndexOf(':') + 1);
                    else if (line[0] != '#')
                    {
                        ToolStripMenuItem lang = new ToolStripMenuItem { Text = line.Substring(0, line.IndexOf(';')), Tag = (string)line.Substring(line.IndexOf(';') + 1) };
                        languageToolStripMenuItem.DropDownItems.Add(lang);
                        lang.Click += Lang_Click;
                        if (lang.Tag.ToString() == SelLang) lang.Checked = true;
                    }
                    line = langs.ReadLine();
                }
                langs.Close();
            }
            if (SelLang == "en") englishToolStripMenuItem.Checked = true;
            LoadLang();
        }

        private void Localize(Control item, ToolStripItem itm2)
        {
            string t;
            if (item != null)
            {
                if (item.Tag != null)
                {
                    t = strings.ResourceManager.GetString((string)item.Tag);
                    if (t != null) if (t.Length > 0)
                        { item.Text = t; } //MessageBox.Show(strings.ResourceManager.GetString((string)item.Tag)); }
                }
                if (item.HasChildren) foreach (Control child in (item).Controls) Localize(child, null);
                if (item.GetType() == typeof(MenuStrip)) foreach (ToolStripItem tool in ((MenuStrip)item).Items) Localize(null, tool);
            }
            else if (itm2 != null)
            {
                if (itm2.Tag != null)
                {
                    t = strings.ResourceManager.GetString((string)itm2.Tag);
                    if (t!=null) if (t.Length > 0)
                        itm2.Text = t;
                }
                if (itm2.GetType() == typeof(ToolStripMenuItem)) foreach (ToolStripItem tool in ((ToolStripMenuItem)itm2).DropDownItems) Localize(null, tool);
            }
        }

        private void LoadLang()
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(SelLang);
            Localize(this, null);
            toolStripStatusLabel1.Text = format_converter.strings.text_ready;
            /*fileToolStripMenuItem.Text = format_converter.strings.main_toolbar_file;
            openFilesToolStripMenuItem.Text = format_converter.strings.main_file_open;*/
            currentModeToolStripMenuItem.Text = string.Format(format_converter.strings.main_toolbar_mode, format_converter.strings.mode_lang_xliff);
            //languageToolStripMenuItem.Text = format_converter.strings.main_toolbar_lang;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (file1 != "" && xliff.OuterXml != "") {
                if (!backgroundWorker1.IsBusy) if (MessageBox.Show(format_converter.strings.popup_convstart_desc, format_converter.strings.popup_convstart_title, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        button1.Text = format_converter.strings.button_cancel_text;
                        openFilesToolStripMenuItem.Enabled = false;
                        button1.Click += BTN_Cancel;
                        toolStripStatusLabel1.Text = format_converter.strings.text_noprogress;
                        toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                        toolStripProgressBar1.Value = 100;
                        backgroundWorker1.RunWorkerAsync();
                    }
            }
            else if (MessageBox.Show(format_converter.strings.popup_nofileselect_desc, format_converter.strings.popup_nofile_title, MessageBoxButtons.YesNo) == DialogResult.Yes) LtX.LoadFiles();
        }

        private void BTN_Cancel(object sender, EventArgs e)
        {
            if (MessageBox.Show(string.Format(format_converter.strings.popup_opruns_desc, Environment.NewLine), format_converter.strings.popup_opruns_title, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        public main()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            InitializeExtension();
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

        private void InitializeExtension()
        {
            LtX.SendDet += LtX_SendDetails;
            LtX.SendData += LtX_SendData;
        }

        private void LtX_SendData(object sender, EventArgs e)
        {
            file1 = LtX.file1;
            fpath1 = LtX.fpath1;
            fpath2 = LtX.fpath2;
            xliff = LtX.xliff;
            strcount1 = LtX.strcount1;
            strcount2 = LtX.strcount2;
        }

        private void LtX_SendDetails(object sender, EventArgs e)
        {
            if (LtX.ClrBigText) if (textBox1.Text == format_converter.strings.main_text_notext) textBox1.Clear();
            if (LtX.TextToSend != null) toolStripStatusLabel1.Text = ((lang_to_xliff)sender).TextToSend;
            if (LtX.BigText != null) textBox1.AppendText(((lang_to_xliff)sender).BigText);
            if (LtX.Label3 != null) label3.Text = ((lang_to_xliff)sender).Label3;
            if (LtX.Label4 != null) label4.Text = ((lang_to_xliff)sender).Label4;
            if (LtX.ValToSend != -1) toolStripProgressBar1.Value = LtX.ValToSend;
            toolStripProgressBar1.Style = LtX.StyleToSend;
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 NewFile = new Form2();
            if(NewFile.ShowDialog() == DialogResult.OK) MessageBox.Show("File created succesfully");
            else MessageBox.Show("Failed to create file", format_converter.strings.popup_warn_title);
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
            LtX.GetFiles(xliff, file1, fpath2);
            e.Result = LtX.ConvertFiles(worker, e);
        }

        private void invertModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LtX.GetFiles(xliff, file1, fpath2);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists(LangFile))
            {
                StreamReader s = new StreamReader(LangFile);
                string[] file = File.ReadAllLines(LangFile);
                string lang = s.ReadLine();
                s.Close();
                lang = lang.Substring(0, lang.IndexOf(':') + 1) + SelLang;
                file[0] = lang;
                File.WriteAllLines(LangFile, file);
            }
        }

        private void Lang_Click(object sender, EventArgs e)
        {
            SelLang = ((ToolStripMenuItem)sender).Tag.ToString();
            foreach (ToolStripMenuItem itm in ((ToolStripMenuItem)sender).GetCurrentParent().Items) itm.Checked = false;
            if (Directory.Exists(SelLang))
            {
                ((ToolStripMenuItem)sender).Checked = true;
            }
            else
            {
                SelLang = "en";
                englishToolStripMenuItem.Checked = true;
            }
            LoadLang();
        }

        void backgroundWorker1_RunWorkerCompleted(
        object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                _ = MessageBox.Show(e.Error.Message, format_converter.strings.popup_error_title);
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
                toolStripStatusLabel1.Text = format_converter.strings.text_cancel;
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                MessageBox.Show(format_converter.strings.popup_convdone_desc);
                textBox1.AppendText(string.Format(format_converter.strings.text_done, e.Result.ToString()));
            }
            //set stuff for conversion
            button1.Text = format_converter.strings.button_convert_text;
            openFilesToolStripMenuItem.Enabled = true;
            button1.Click += button1_Click;
        }

        void backgroundWorker1_ProgressChanged(object sender,
        ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            int prog = (int)((float)e.ProgressPercentage / strcount1 * 100);
            int val2 = strcount1;
            if (prog > 100 && strcount2 > strcount1) { prog = (int)((float)e.ProgressPercentage / strcount2 * 100); val2 = strcount2; }
            if (prog >= toolStripProgressBar1.Minimum && prog <= toolStripProgressBar1.Maximum)toolStripProgressBar1.Value = prog;
            toolStripStatusLabel1.Text = string.Format(format_converter.strings.text_progress, e.ProgressPercentage.ToString(), val2.ToString());
        }

        

        private void openFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LtX.LoadFiles(); 
        }
    }
}
