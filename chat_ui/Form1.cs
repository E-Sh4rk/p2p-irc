using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace chat_ui
{
    public partial class Form1 : Form
    {
        Settings settings;
        p2p_irc.Protocol protocol;

        public Form1()
        {
            InitializeComponent();
            p2p_irc.Utils.debug_action = new p2p_irc.Utils.NewDebugMessage(newDebugMessage);
            settings = new Settings();
            showSettings();
        }

        void showSettings()
        {
            settings.ShowDialog(this);
            if (!settings.ClosedProperly)
                Close();
            else if (settings.RestartCommunications)
                restartCommunications();
        }

        Thread t;
        public void restartCommunications()
        {
            if (protocol != null)
            {
                protocol.Dispose();
                while (t.IsAlive) { Thread.Sleep(10); }
            }

            List<p2p_irc.PeerAddress> neighbors = new List<p2p_irc.PeerAddress>();
            foreach (string a in settings.Neighbors)
            {
                try
                {
                    string[] parts = a.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        p2p_irc.PeerAddress pa = new p2p_irc.PeerAddress();
                        pa.port = Convert.ToInt32(parts[1]);
                        pa.ip = Dns.GetHostEntry(parts[0]).AddressList[0];
                        neighbors.Add(pa);
                    }
                }
                catch { }
            }
            protocol = new p2p_irc.Protocol(settings.Port, neighbors.ToArray(), new p2p_irc.Chat.NewMessage(newMessageArrived));
            t = new Thread(new ThreadStart(protocol.Run));
            t.Start();
        }

        public void newMessageArrived(ulong sender, string msg)
        {
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate { newMessageArrived(sender, msg); });
            else
            {
                if (settings.ShowIDs)
                    chatTextBox.AppendText("[" + sender.ToString() + "] ");
                chatTextBox.AppendText(msg + Environment.NewLine);
            }
        }
        public void newDebugMessage(string msg, ulong? sender)
        {
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate { newDebugMessage(msg, sender); });
            else
            {
                if (settings.ShowDebug)
                    chatTextBox.AppendText(msg + Environment.NewLine);
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            showSettings();
        }

        private string interpretCommand(string command)
        {
            string cmd = command;
            string tail = "";
            int index = command.IndexOf(' ');
            if (index >= 0)
            {
                cmd = command.Substring(0, index);
                tail = command.Substring(index);
            }
            switch (cmd)
            {
                case "/me":
                    return "* " + settings.Username + " " + tail;
                case "/exit":
                    Close();
                    break;
            }
            return null;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            string msg = messageTextBox.Text;
            messageTextBox.Clear();
            if (msg.StartsWith("/"))
                msg = interpretCommand(msg);
            else
                msg = settings.Username + ": " + msg;
            if (!String.IsNullOrEmpty(msg))
                protocol.SendMessage(msg);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (protocol != null)
                protocol.Dispose();
        }
    }
}
