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
        private string currPath;

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
            
            var myFiles = Directory.EnumerateFiles(currPath, "*.vcxitems", SearchOption.AllDirectories);
            foreach (string item in myFiles)
                References.Items.Add(Path.GetFileNameWithoutExtension(item));
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
