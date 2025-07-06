using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace format_converter
{
    class lang_to_xliff
    {
        public XmlDocument xliff;
        public string TextToSend, BigText, Label3, Label4, fpath1, fpath2, file1;
        public int strcount1, strcount2, ValToSend;
        public bool ClrBigText = false;
        public ProgressBarStyle StyleToSend;
        public event EventHandler SendDet = delegate { };
        public event EventHandler SendData = delegate { };

        public void GetFiles(XmlDocument xml, string inp1, string savl)
        {
            file1 = inp1;
            fpath2 = savl;
            xliff = xml;
        }

        private void SendDetails()
        {
            SendDet(this, EventArgs.Empty);
            BigText = Label3 = Label4 = TextToSend = null;
            ClrBigText = false;
            ValToSend = -1;
        }
        public void LoadFiles()
        {
            file1 = "";
            xliff = new XmlDocument();
            int trcount2 = 0, utrcount2 = 0;
            strcount1 = 0; strcount2 = 0;
            string line1;
            string[] details;
            TextToSend = format_converter.strings.text_sel1;
            SendDetails();
            OpenFileDialog fdiag = new OpenFileDialog
            {
                Title = format_converter.strings.text_sel1_title,
                Filter = format_converter.strings.filter_lang_desc + "|*.lang",
                InitialDirectory = "%homedir%",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            StreamReader S;
            StringBuilder P;
            if (fdiag.ShowDialog() == DialogResult.OK)
            {
                TextToSend = format_converter.strings.text_load1;
                StyleToSend = ProgressBarStyle.Marquee;
                ValToSend = 100;
                SendDetails();
                S = new StreamReader(fdiag.FileName);
                P = new StringBuilder(file1);
                line1 = S.ReadLine();
                while (line1 != null)
                {
                    P.AppendLine(line1);
                    if (line1.Length > 0) if (line1[0] != '#') strcount1++;
                    line1 = S.ReadLine();
                }
                S.Close();
                file1 = P.ToString();
                StyleToSend = ProgressBarStyle.Blocks;
                ValToSend = 50;
                fpath1 = fdiag.FileName;
                Label3 = fpath1;
                ClrBigText = true;
                details = new string[] { fpath1, Convert.ToString(strcount1) };
                BigText = string.Format(format_converter.strings.text_desc_lang, details) + Environment.NewLine;
                TextToSend = format_converter.strings.text_sel2;
                SendDetails();
                fdiag.Title = format_converter.strings.text_sel2_title;
                fdiag.Filter = format_converter.strings.filter_xliff_desc + "| *.xliff";
                if (fdiag.ShowDialog() == DialogResult.OK)
                {
                    TextToSend = format_converter.strings.text_load2;
                    StyleToSend = ProgressBarStyle.Marquee;
                    ValToSend = 100;
                    SendDetails();
                    S = new StreamReader(fdiag.FileName);
                    line1 = S.ReadLine();
                    while (line1 != null)
                    {
                        P.AppendLine(line1);
                        if (line1.IndexOf("resname") != -1) strcount2++;
                        else if (line1.IndexOf("state=\"final\"") != -1 || line1.IndexOf("state=\"translated\"") != -1 || line1.IndexOf("state=\"needs - review - translation\"") != -1) trcount2++;
                        else if (line1.IndexOf("needs-translation") != -1) utrcount2++;
                        line1 = S.ReadLine();
                    }
                    S.Close();
                    try
                    {
                        fpath2 = fdiag.FileName;
                        xliff.Load(fpath2);
                        Label4 = fpath2;
                        StyleToSend = ProgressBarStyle.Blocks;
                        details = new string[] { fpath2, Convert.ToString(strcount2), Convert.ToString(trcount2), Convert.ToString(utrcount2) };
                        BigText = string.Format(format_converter.strings.text_desc_xliff, details) + Environment.NewLine;
                        SendDetails();
                    }
                    catch (System.Xml.XmlException)
                    {
                        MessageBox.Show(string.Format(format_converter.strings.popup_badxml_desc, Environment.NewLine), format_converter.strings.popup_error_title);
                        StyleToSend = ProgressBarStyle.Blocks;
                        ValToSend = 0;
                    }
                }
                else MessageBox.Show(format_converter.strings.popup_nofile_desc, format_converter.strings.popup_warn_title);
                BigText = string.Format(format_converter.strings.text_desc_length1, Convert.ToString(file1.Length)) + Environment.NewLine + string.Format(format_converter.strings.text_desc_length2, Convert.ToString(xliff.OuterXml.Length)) + Environment.NewLine;
                TextToSend = format_converter.strings.text_ready;
                ValToSend = 0;
                SendDetails();
            }
            else MessageBox.Show(format_converter.strings.popup_nofile_desc, format_converter.strings.popup_warn_title);
            TextToSend = format_converter.strings.text_ready;
            SendDetails();
            SendData(this, EventArgs.Empty);
        }

        public int ConvertFiles(BackgroundWorker worker, DoWorkEventArgs e)
        {
            string line1, line11, line12;
            int loc1 = 0, tloc = 0, tms = 0, loc2 = 0;
            StringReader fconvert1 = new StringReader(file1);
            XmlNode newTransl, transl;
            XmlNodeList nodes = xliff.GetElementsByTagName("trans-unit"), nodes2;
            nodes2 = nodes;
            line1 = fconvert1.ReadLine();
            while (line1 != null)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else if (line1.Length > 0) if (line1[0] != '#')
                    {
                        line11 = line1.Substring(line1.IndexOf("=") + 1);
                        line1 = line1.Substring(0, line1.IndexOf("="));
                        loc1++;
                        if (line11.IndexOf('&') != -1) { line11 = line11.Substring(0, line11.IndexOf('&')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&amp;" + line12; }
                        if (line11.IndexOf('<') != -1) { line11 = line11.Substring(0, line11.IndexOf('<')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&lt;" + line12; }
                        if (line11.IndexOf('>') != -1) { line11 = line11.Substring(0, line11.IndexOf('>')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&gt;" + line12; }
                        if (line11.IndexOf('\'') != -1) { line11 = line11.Substring(0, line11.IndexOf('\'')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&apos;" + line12; }
                        if (line11.IndexOf('"') != -1) { line11 = line11.Substring(0, line11.IndexOf('"')); line12 = line11.Substring(line11.IndexOf('&') + 1); line11 = line11 + "&quot;" + line12; }
                        for (int i = 0; i < Convert.ToInt64(nodes.Count.ToString()); i++)
                            if (nodes[i].Attributes["resname"].Value == line1)
                            {
                                loc2++;
                                if (tloc == 0) foreach (XmlNode nb in nodes[i])
                                    {
                                        if (nb.Name == "target") tloc = tms;
                                        tms++;
                                    }
                                transl = nodes[i].ChildNodes.Item(tloc);
                                if (transl != null)
                                {
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
            xliff.Save(fpath2);
            return loc2;
        }
    }
}
