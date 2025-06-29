using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
                button1.Text = "Cancel";
                openFilesToolStripMenuItem.Enabled = false;
                button1.Click += BTN_Cancel;
                if(!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
            }
            else if (MessageBox.Show("No files selected. Select now?", "No file selected", MessageBoxButtons.YesNo) == DialogResult.Yes) LoadFiles();
        }

        private void BTN_Cancel(object sender, EventArgs e)
        {
            if (MessageBox.Show("Operation is running. Do you want to cancel it? Note: you will lose all progress", "Cancel operation?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                backgroundWorker1.CancelAsync();
                button1.Text = "Convert";
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

            // Disable the Cancel button.
            //cancelAsyncButton.Enabled = false;
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

        void backgroundWorker1_RunWorkerCompleted(
        object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                _ = MessageBox.Show(e.Error.Message);
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
                toolStripStatusLabel1.Text = "Canceled";
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                MessageBox.Show("done");
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
            toolStripStatusLabel1.Text = e.ProgressPercentage.ToString() + " / " + val2.ToString();
        }

        private void LoadFiles()
        {
            //file1 = ""; file2 = "";
            strcount1 = 0; strcount2 = 0; trcount2 = 0; utrcount2 = 0;
            string line1 = "";
            toolStripStatusLabel1.Text = "Select Input...";
            OpenFileDialog fdiag = new OpenFileDialog
            {
                Title = "Open input file...",
                Filter = "Lang files (*.lang)|*.lang",
                InitialDirectory = "%homedir%",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            StreamReader S;
            StringBuilder P;
            if (fdiag.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = "Loading file 1...";
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
                if(textBox1.Text == "log output...")textBox1.Clear();
                textBox1.AppendText(fdiag.FileName + " has " + Convert.ToString(strcount1) + " strings" + Environment.NewLine);
                toolStripStatusLabel1.Text = "Loading file 2...";
                fdiag.Title = "Open output file...";
                fdiag.Filter = "Lang files (*.xliff)|*.xliff";
                if (fdiag.ShowDialog() == DialogResult.OK)
                {
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
                    textBox1.AppendText(fdiag.FileName + " has " + Convert.ToString(strcount2) + " strings of whom " + Convert.ToString(trcount2) + " translated and " + Convert.ToString(utrcount2) + " untranslated" + Environment.NewLine);
                }
                else MessageBox.Show("File not selected!", "Warning");
                toolStripStatusLabel1.Text = "Ready";
                textBox1.AppendText("Input length: " + Convert.ToString(file1.Length) + Environment.NewLine);
                textBox1.AppendText("Output length: " + Convert.ToString(file2.Length) + Environment.NewLine);
            }
            else MessageBox.Show("File not selected!", "Warning");
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
            if (MessageBox.Show("Are you sure you want to convert?", "ready to convert", MessageBoxButtons.YesNo) == DialogResult.Yes) {
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
                                        //MessageBox.Show("replaced " + transl.InnerXml + " with " + newTransl.InnerXml);
                                        nodes[i].ReplaceChild(transl, newTransl);
                                    }
                                    //if (newTransl.Attributes["state"].Value == "needs-translation") MessageBox.Show(line11, "we need this here");
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
