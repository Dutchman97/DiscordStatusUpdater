using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class MainForm : Form
    {
        DiscordClient client;
        bool manual = false;
        // The String class instead of the string struct, because now 'null' can represent no status change
        String pendingStatus = null;
        const int CHECKINTERVAL = 10000, UPDATEINTERVAL = 10000;

        public MainForm(DiscordClient client)
        {
            InitializeComponent();
            this.client = client;

            checkTimer.Interval = 1;
            checkTimer.Start();
            updateTimer.Stop();
        }

        private string GetVideoTitle()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process proc in processes)
                if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    foreach (Player player in Properties.Settings.Default.Players)
                        if (proc.ProcessName.ToLower() == player.FileName.ToLower())
                            return ParseVideoTitle(player, proc.MainWindowTitle);
            return string.Empty;
        }

        private string ParseVideoTitle(Player player, string title)
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

        private void ChangeStatus()
        {
            string videoTitle = GetVideoTitle();
            ChangeStatus(videoTitle);
        }

        private void ChangeStatus(string status)
        {
            Console.WriteLine("Trying to change status to " + status);

            if (status == currentStatusTextBox.Text)
                return;

            Console.WriteLine("New status not equal to old status");

            if (updateTimer.Enabled)
            {
                Console.WriteLine("Update timer enabled");
                pendingStatus = status;
                if (status == string.Empty)
                    pendingLabel.Text = "Pending status removal";
                else
                    pendingLabel.Text = "Pending status update: " + status;
            }
            else
            {
                Console.WriteLine("Update timer disabled");
                Console.WriteLine("Changed status to " + status);
                updateTimer.Interval = UPDATEINTERVAL;
                updateTimer.Start();

                currentStatusTextBox.Text = status;
                client.SetGame(status);
            }
        }

        private void ChangeMode()
        {
            manual = !manual;
            modeButton.Text = "Click to change mode." + Environment.NewLine + "Currently set to ";

            if (manual)
            {
                modeButton.Text += "manual.";
                checkTimer.Stop();
            }
            else
            {
                modeButton.Text += "automatic.";
                checkTimer.Interval = 1;
                checkTimer.Start();
            }
        }

        private void modeButton_Click(object sender, EventArgs e)
        {
            ChangeMode();
        }

        private void checkTimer_Tick(object sender, EventArgs e)
        {
            ChangeStatus();
            checkTimer.Interval = CHECKINTERVAL;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (updateTimer.Enabled)
            {
                //DialogResult result = MessageBox.Show("Your current status message will stay the same if you close the program now.\nAre you sure you want to close the program?",
                //    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                //if (result == DialogResult.No)
                //{
                //    e.Cancel = true;
                //    return;
                //}
                e.Cancel = true;
                ChangeStatus(string.Empty);
                updateTimer.Tick += (sender1, e1) =>
                {
                    MainForm_FormClosing(sender, e);
                };
                return;
            }
            
            client.SetGame(string.Empty);

            // Yes, a Thread.Sleep() since appearantly calling SetGame() does not wait for the new status to get sent.
            Thread.Sleep(500);
            client.Disconnect();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void statusButton_Click(object sender, EventArgs e)
        {
            statusTextBox_KeyDown(sender, new KeyEventArgs(Keys.Enter));
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Update timer ticked");
            updateTimer.Stop();

            if (pendingStatus != null)
                ChangeStatus(pendingStatus);

            pendingLabel.Text = "No pending status update";
            pendingStatus = null;
        }

        private void pendingLabel_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Discord only allows status updates every roughly 10 seconds.\n" +
                "Any status update less than 10 seconds after another status update will be pushed after the 10 seconds are over.",
                "Update speed limit", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void statusTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!manual)
                    ChangeMode();
                ChangeStatus(statusTextBox.Text);
            }
        }
    }
}
