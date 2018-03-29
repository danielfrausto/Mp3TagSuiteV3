using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mp3TagSuite
{
    public partial class iSearch_Form : Form
    {
        public string Filename
        {
            get { return LBL_Filename.Text; }
            set { LBL_Filename.Text = value; }
        }
        public iSearch_Form()
        {
            InitializeComponent();
        }

        private void BTN_Search_Click(object sender, EventArgs e)
        {
            Main_Form.SearchQuery = TB_Search.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
