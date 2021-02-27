using System;
using System.Windows.Forms;

namespace DBconnect
{
    public partial class Authorization : Form
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {          
            MainForm form = new MainForm(textBoxLogin.Text, textBoxPassword.Text, this); 
        }
    }
}
