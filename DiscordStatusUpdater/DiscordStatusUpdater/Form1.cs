using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class Form1 : Form
    {
        DiscordClient client;
        string email, password;

        public Form1()
        {
            InitializeComponent();

            string[] loginInfo = File.ReadAllLines(@"C:\Users\Kevin\Desktop\login.txt");
            email = loginInfo[0]; password = loginInfo[1];

            client = new DiscordClient();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.Disconnect();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client.Connect(email, password);
        }
    }
}
