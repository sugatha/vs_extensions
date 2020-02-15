using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TallyUtil
{
    public partial class NewProject : Form
    {
        public string currPath;
        public string sharedItemName;
        public string newProjectname;
        public string projectType;

        public IEnumerable<string> sharedItemFiles;

        public NewProject()
        {
            InitializeComponent();
        }

        public NewProject(string v)
        {
            InitializeComponent();
            this.currPath = v;

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

       
        private void NewProject_Load(object sender, EventArgs e)
        {
            References.SelectedText = "--- Select a Shared Item ---";
            sharedItemFiles = Directory.EnumerateFiles(currPath, "*.vcxitems", SearchOption.AllDirectories);
            foreach (string item in sharedItemFiles)
                References.Items.Add(Path.GetFileNameWithoutExtension(item));

            Project_Type.SelectedText = "--- Select a Type ---";
            References.Enabled = false;
        }



        private void button1_Click(object sender, EventArgs e)
        {
            int indexReference = References.SelectedIndex;
            if(indexReference != -1)
                sharedItemName = Path.GetFileName(sharedItemFiles.ElementAt(indexReference));

            int indexType = Project_Type.SelectedIndex;
            if(indexType != -1)
                projectType = Project_Type.SelectedItem.ToString();

            newProjectname = Project_Name.Text;

            if((indexType != -1) && newProjectname.Length != 0)
                this.Close();
        }

        private void Project_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Project_Type.SelectedIndex != 0)
            {
                References.Enabled = true;
            }
        }
    }
}
