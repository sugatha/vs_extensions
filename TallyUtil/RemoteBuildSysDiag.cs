using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TallyUtil
{
    public struct tHosts{
        public string id;
        public string hostname;
        public string port;
        public string username;
    };

    public partial class RemoteBuildSysDiag : Form
    {

        private List<tHosts> list;

        public int selectedIndex;
        public RemoteBuildSysDiag()
        {
            InitializeComponent();
        }

        public RemoteBuildSysDiag(List<tHosts> hosts)
        {
            InitializeComponent();
            list = hosts;
        }

        private void RemoteBuildSysDiag_Load(object sender, EventArgs e)
        {
            Hosts.SelectedText = "---Select a host---";
            foreach (tHosts item in list)
                Hosts.Items.Add(item.hostname + ":" + item.port + "@" + item.username);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int index = Hosts.SelectedIndex;
            if (index >= 0)
            {
                string entry_id = list[index].id;
                string hostname = list[index].hostname;

                selectedIndex = index;

                this.Close();
            }
        }
    }
}
