using System;
using System.Diagnostics;
using System.Threading;
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
            updateTimerLabel.ForeColor = System.Drawing.Color.Green;
            updateTimerLabel.Text = "Status update possible";
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
            Console.WriteLine("Title: " + title);
            // Remove prefix and suffix of the player from the title.
            if (player.TitleSuffix != string.Empty)
            {
                int suffixIndex = title.ToLower().LastIndexOf(player.TitleSuffix.ToLower());
                title = title.Remove(suffixIndex);
                Console.WriteLine("Suffix: " + player.TitleSuffix + ", at: " + suffixIndex.ToString());
            }
            if (player.TitlePrefix != string.Empty)
            {
                int prefixIndex = title.ToLower().IndexOf(player.TitlePrefix.ToLower());
                title = title.Substring(prefixIndex + player.TitlePrefix.Length);
                Console.WriteLine("Prefix: " + player.TitlePrefix + ", at: " + prefixIndex.ToString());
            }

            Console.WriteLine("Title minus pre/suffix: " + title);
            // Remove the file extension from the title
            foreach (string extension in Properties.Settings.Default.Extensions)
                if (title.EndsWith(extension, true, null))
                {
                    title = title.Remove(title.Length - 1 - extension.Length);
                    Console.WriteLine("Extension: " + extension);
                    break;
                }

            Console.WriteLine("Title minus extension: " + title);
            // Remove square brackets and everything inbetween them.
            while (true)
            {
                int first = title.IndexOf('[');
                int last = title.IndexOf(']');
                if (first < 0 || last < 0)
                    break;

                title = title.Remove(first, last - first + 1);
            }

            Console.WriteLine("Title minus brackets: " + title);
            // Replace all underscores with whitespace.
            title = title.Replace('_', ' ');

            Console.WriteLine("Title minus underscores: " + title);
            // Remove all leading and trailing whitespace.
            title = title.Trim();

            Console.WriteLine("Final title: " + title);
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

            if (status == statusTextBox.Text)
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
                updateTimerLabel.Text = "No status update possible yet";
                updateTimerLabel.ForeColor = System.Drawing.Color.Red;

                statusTextBox.Text = status;
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
            if (updateTimer.Enabled && statusTextBox.Text != string.Empty)
            {
                DialogResult result = MessageBox.Show("Your current status message will stay the same if you close the program now.\nAre you sure you want to close the program?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                //e.Cancel = true;
                //ChangeStatus(string.Empty);
                //updateTimer.Tick += (sender1, e1) =>
                //{
                //    MainForm_FormClosing(sender, e);
                //};
                //return;
            }

            this.Text = "Closing...";
            client.SetGame(string.Empty);

            // Yes, a Thread.Sleep() since appearantly calling SetGame() does not wait for the new status to get sent.
            Thread.Sleep(500);
            client.Disconnect();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void setStatusButton_Click(object sender, EventArgs e)
        {
            setStatusTextBox_KeyDown(sender, new KeyEventArgs(Keys.Enter));
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Update timer ticked");
            updateTimerLabel.Text = "Status update possible";
            updateTimerLabel.ForeColor = System.Drawing.Color.Green;
            updateTimer.Stop();

            if (pendingStatus != null)
                ChangeStatus(pendingStatus);

            pendingLabel.Text = "No pending status update";
            pendingStatus = null;
        }

        private void updateLabel_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Discord only allows status updates every roughly 10 seconds.\n" +
                "Any status update less than 10 seconds after another status update will be pushed after the 10 seconds are over.",
                "Update speed limit", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void updateTimerLabel_ForeColorChanged(object sender, EventArgs e)
        {
            helpLabel.Location = new System.Drawing.Point(updateTimerLabel.Location.X + updateTimerLabel.Size.Width - 5, updateTimerLabel.Location.Y);
        }

        private void setStatusTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!manual)
                    ChangeMode();
                ChangeStatus(setStatusTextBox.Text);
            }
        }
    }
}
