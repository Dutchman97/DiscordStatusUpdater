using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class Form1 : Form
    {
        DiscordClient client;

        public Form1(DiscordClient client)
        {
            InitializeComponent();

            this.client = client;
            textBox1.Text = CheckForVideo();
        }

        private string CheckForVideo()
        {
            string output = "";

            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                output += p.MainWindowTitle + ";" + p.ProcessName + " - ";
            }
            return output;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            client.Disconnect();
            Application.Exit();
        }
    }
}
