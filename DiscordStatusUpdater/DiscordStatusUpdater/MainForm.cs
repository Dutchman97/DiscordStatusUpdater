using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class MainForm : Form
    {
        SettingsForm settingsForm;
        DiscordClient client;
        bool manual = false;
        // The String class instead of the string struct, because now 'null' can represent no status change
        String pendingStatus = null;
        const int CHECKINTERVAL = 10000, UPDATEINTERVAL = 10000;
        const string PLAYINGTEXT = " Playing ";

        public MainForm(DiscordClient client)
        {
            InitializeComponent();
            this.client = client;

            checkTimer.Interval = 1;
            checkTimer.Start();
            updateTimer.Stop();
            updateTimerLabel.Text = "Status update possible";
            updateTimerLabel.ForeColor = System.Drawing.Color.Green;
            SetHelpLabel();
            usernameLabel.Text = "Logged in as " + client.CurrentUser.Name;
            Console.WriteLine(usernameLabel.Text);
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
            title -= player.Title;
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
            
            if ((status == string.Empty && statusTextBox.Text == string.Empty) || (statusTextBox.Text != string.Empty && status == statusTextBox.Text.Substring(PLAYINGTEXT.Length)))
                return;

            Console.WriteLine("New status not equal to old status");

            if (updateTimer.Enabled)
            {
                Console.WriteLine("Update timer enabled");
                pendingStatus = status;

                if (status == string.Empty)
                    updateTimerLabel.Text = "Pending status removal";
                else
                    updateTimerLabel.Text = "Pending status update: " + status;

                updateTimerLabel.ForeColor = System.Drawing.Color.Red;

                SetHelpLabel();
            }
            else
            {
                Console.WriteLine("Update timer disabled");
                Console.WriteLine("Changed status to " + status);
                updateTimer.Interval = UPDATEINTERVAL;
                updateTimer.Start();
                updateTimerLabel.Text = "No status update possible yet";
                updateTimerLabel.ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xCC, 0x84, 0x00);
                SetHelpLabel();

                if (status == string.Empty)
                {
                    statusTextBox.Text = "";
                    client.SetGame(null);
                }
                else
                {
                    client.SetGame(status);
                    statusTextBox.Rtf = @"{\rtf1\ansi {\colortbl;\red0\green0\blue0;}\cf1 " + PLAYINGTEXT + @"\b\cf0 " + status + @"\b0 }";
                }
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

        private void SetHelpLabel()
        {
            helpLabel.Location = new Point(updateTimerLabel.Location.X + updateTimerLabel.Size.Width - 3, updateTimerLabel.Location.Y + 1);
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
            if (updateTimer.Enabled && statusTextBox.Text.Length > 0 && statusTextBox.Text.Substring(PLAYINGTEXT.Length) != string.Empty)
            {
                DialogResult result = MessageBox.Show("Your current status message will stay the same if you close the program now.\nAre you sure you want to close the program?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            updateTimer.Stop();
            this.Text = "Closing...";

            if (statusTextBox.Text != string.Empty)
            {
                client.SetGame(string.Empty);
                statusTextBox.Text = string.Empty;

                // Yes, a Thread.Sleep() since appearantly calling client.SetGame() does not wait for the new status to get sent.
                Thread.Sleep(300);
            }

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
            SetHelpLabel();
            updateTimer.Stop();

            if (pendingStatus != null)
                ChangeStatus(pendingStatus);
            
            pendingStatus = null;
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            if (settingsForm == null)
            {
                settingsForm = new SettingsForm();
                settingsForm.StartPosition = FormStartPosition.Manual;
            }

            if (settingsForm.Visible)
            {
                settingsForm.Hide();
            }
            else
            {
                MainForm_Move(sender, e);
                settingsForm.Show(this);
            }
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            if (settingsForm != null)
                settingsForm.Location = new Point(this.DesktopLocation.X + this.Width - 15, this.DesktopLocation.Y);
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
