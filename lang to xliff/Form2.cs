using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace format_converter
{
    public partial class Form2 : Form
    {
        string loadFile;
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog() {
                Title = "Open file...",
                InitialDirectory = "%homedir%",
                Filter = ".lang files (*.lang)|*.lang|.resx files (*.resx)|*.resx|Text files (*.txt)|*.txt|.xliff files (*.xliff)|*.xliff",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (open.ShowDialog() == DialogResult.OK) { 
                loadFile = open.FileName;
                label2.Text = loadFile;
                button1.Text = "Browse...";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == 0) { checkBox1.Checked = false; checkBox1.Enabled = false; }
            else checkBox1.Enabled = true;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string FType = comboBox1.SelectedItem.ToString();
            SaveFileDialog save = new SaveFileDialog()
            {
                Title = "Save file...",
                InitialDirectory = "%homedir%",
                Filter = FType + " file (*" + FType + ")|*" + FType,
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (save.ShowDialog() == DialogResult.OK)
            {
                string saveFile = save.FileName, saveType = Path.GetExtension(saveFile);
                MessageBox.Show(saveFile, saveType);
                MessageBox.Show(loadFile);
                MessageBox.Show(checkBox1.Checked.ToString());
                if (checkBox1.Checked && saveType != ".lang")
                {
                    switch (saveType) {
                        case (".resx"): SetUpRESX(saveFile); break;
                        case (".xliff"): MessageBox.Show(".xliff"); break;
                    }
                }
                MessageBox.Show(checkBox2.Checked.ToString());
                if (checkBox2.Checked && loadFile != null)
                {
                    MessageBox.Show(saveFile, saveType);
                    MessageBox.Show(loadFile);
                    switch (saveType)
                    {
                        case (".lang"): MessageBox.Show(".lang"); LoadLANG(saveFile, loadFile); break;
                        case (".resx"): MessageBox.Show(".resx"); LoadRESX(saveFile, loadFile); MessageBox.Show("pass12"); break;
                        case (".xliff"): MessageBox.Show(".xliff"); break;
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void LoadLANG(string saveFile, string loadFile)
        {
            string LoadType = Path.GetExtension(loadFile);
            StreamWriter lang = new StreamWriter(saveFile);
            switch (LoadType)
            {
                case (".lang"):
                    {
                        string line1;
                        StreamReader S = new StreamReader(loadFile);
                        line1 = S.ReadLine();
                        while (line1 != null)
                        {
                            if (line1.Length > 0) if (line1[0] != '#')
                                {
                                    lang.WriteLine(line1);
                                }
                            line1 = S.ReadLine();
                        }
                        S.Close();
                        break;
                    }
                case (".resx"):
                    {
                        ResXResourceReader S = new ResXResourceReader(loadFile);
                        foreach (DictionaryEntry entry in S)
                        {
                            lang.WriteLine(entry.Key.ToString() + '=' + entry.Value.ToString());
                        }
                        break;
                    }
            }
            lang.Close();
        }

        private void LoadRESX(string saveFile, string loadFile)
        {
            string LoadType = Path.GetExtension(loadFile);
            int p = 0, strcount1 = 0;
            ResXResourceWriter resx = new ResXResourceWriter(saveFile);
            switch (LoadType)
            {
                case (".lang"):
                    {
                        StreamReader S = new StreamReader(loadFile); string line1, line11;
                        line1 = S.ReadLine();
                        while (line1 != null)
                        {
                            if (line1.Length > 0) if (line1[0] != '#') strcount1++;
                            line1 = S.ReadLine();
                        }
                        S = new StreamReader(loadFile);
                        line1 = S.ReadLine();
                        while (line1 != null)
                        {
                            if (line1.Length > 0) if (line1[0] != '#') {
                                    line11 = line1.Substring(line1.IndexOf("=") + 1);
                                    line1 = line1.Substring(0, line1.IndexOf("="));
                                    resx.AddResource(line1, line11);
                                    p++;
                                }
                            line1 = S.ReadLine();
                            int prog = (int)((float)p / strcount1 * 100);
                            if (prog >= progressBar1.Minimum && prog <= progressBar1.Maximum) progressBar1.Value = prog;
                        }
                        S.Close();
                        break;
                    }
                case (".resx"):
                    {
                        ResXResourceReader S = new ResXResourceReader(loadFile);
                        foreach (DictionaryEntry entry in S)
                        {
                            resx.AddResource(entry.Key.ToString(), entry.Value.ToString());
                        }
                        break;
                    }
            }
            resx.Close();
        }

        private void SetUpRESX(string saveFile)
        {
            MessageBox.Show("resx1");
            ResXResourceWriter resx = new ResXResourceWriter(saveFile);
            resx.Close();
            MessageBox.Show("resx2");
        }
    }
}
