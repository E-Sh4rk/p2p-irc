using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace chat_ui
{
    public partial class Form1 : Form
    {
        Settings settings = new Settings();
        public Form1()
        {
            InitializeComponent();
            settings.ShowDialog();
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            settings.ShowDialog();
        }
    }
}
