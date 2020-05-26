using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Text_Editor
{
    public partial class AboutForm : Form
    {
        string currentFilePath;
        public AboutForm(string filePath = "")
        {
            InitializeComponent();
            this.currentFilePath = filePath;
        }

        // display current file information
        private void AboutForm_Load(object sender, EventArgs e)
        {
            if(this.currentFilePath.Trim().Length > 0)
            {
                filePathLabel.Text += this.currentFilePath;
            }
            else
            {
                filePathLabel.Text += "File not saved yet.";
            }
        }
    }
}
