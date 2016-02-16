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
        }

        private string CheckForVideo()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process proc in processes)
                if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    foreach (Player player in Properties.Settings.Default.Players)
                        if (proc.ProcessName.ToLower() == player.FileName.ToLower())
                            return GetVideoTitle(player, proc.MainWindowTitle);
            return string.Empty;
        }

        private string GetVideoTitle(Player player, string title)
        {
            // Remove prefix and suffix of the player from the title.
            int prefixIndex = title.ToLower().IndexOf(player.TitlePrefix.ToLower());
            int suffixIndex = title.ToLower().LastIndexOf(player.TitleSuffix.ToLower());
            title = title.Remove(suffixIndex);
            title = title.Substring(prefixIndex + player.TitlePrefix.Length);

            // Remove the file extension from the title
            foreach (string extension in Properties.Settings.Default.Extensions)
                if (title.EndsWith(extension, true, null))
                {
                    title = title.Remove(title.Length - 1 - extension.Length);
                    break;
                }

            // Remove square brackets and everything inbetween them.
            while (true)
            {
                int first = title.IndexOf('[');
                int last = title.IndexOf(']');
                if (first < 0 || last < 0)
                    break;

                title = title.Remove(first, last - first + 1);
            }

            // Replace all underscores with whitespace.
            title = title.Replace('_', ' ');

            // Remove all leading and trailing whitespace.
            title = title.Trim();

            return title;
        }

        async private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            client.SetGame(string.Empty);
            await client.Disconnect();
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string videoTitle = CheckForVideo();
            textBox1.Text = videoTitle;
            client.SetGame(videoTitle);
        }
    }
}
