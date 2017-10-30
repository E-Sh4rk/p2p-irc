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
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        bool restartCommunications = true;
        public bool RestartCommunications
        {
            get
            {
                bool r = restartCommunications;
                restartCommunications = false;
                return r;
            }
        }
        public int Port
        {
            get
            {
                return Convert.ToInt32(portNumber.Value);
            }
        }
        public string Username
        {
            get
            {
                return username.Text;
            }
        }
        public string[] Neighbors
        {
            get
            {
                return neighbors.Text.Split(new string[] { "\r\n", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private void close_Click(object sender, EventArgs e) {  this.Close(); }

        private void button2_Click(object sender, EventArgs e)
        {
            restartCommunications = true;
        }
    }
}
