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
        DiscordClient client;
        bool manual = false;
        const int CHECK_INTERVAL = 15000;
        const string PLAYING_TEXT = "Playing";
        bool logout = false;

        StatusUpdater statusUpdater;
        Players.PlayerManager playerManager;

        public MainForm(DiscordClient client)
        {
            InitializeComponent();
            this.client = client;

            Debug.WriteLine(XmlDownloader.DownloadFiles() ? "Succesfully downloaded xml files" : "Failed to download xml files");
            playerManager = new Players.PlayerManager();

            checkTimer.Interval = CHECK_INTERVAL;
            checkTimer.Start();
            updateTimerLabel.Text = "Status update possible";
            updateTimerLabel.ForeColor = System.Drawing.Color.Green;
            SetHelpLabel();
            usernameLabel.Text = "Logged in as " + client.CurrentUser.Name;
            Console.WriteLine(usernameLabel.Text);

            statusUpdater = new StatusUpdater(updateTimer, client);
            statusUpdater.StatusSetAttempted += StatusSetAttempted;
        }

        private string GetVideoTitle()
        {
            if (playerManager == null)
                return string.Empty;
            return playerManager.GetVideoTitle();
        }

        private void ChangeStatus()
        {
            string videoTitle = GetVideoTitle();
            ChangeStatus(videoTitle);
        }

        private void ChangeStatus(string status)
        {
            statusUpdater.ChangeStatus(status);
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
            checkTimer.Interval = CHECK_INTERVAL;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client.State == ConnectionState.Disconnected)
                return;

            if (!statusUpdater.StatusUpdatePossible)
            {
                // StatusUpdater has to be removed, because if it's still active it will
                // still use updateTimer, which somehow forces this form to close after this method
                statusUpdater.Dispose();
                statusUpdater = null;

                DialogResult result = MessageBox.Show("Your current status message will stay the same if you close the program now.\nAre you sure you want to close the program?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;

                    statusUpdater = new StatusUpdater(updateTimer, client);
                    statusUpdater.StatusSetAttempted += StatusSetAttempted;
                    return;
                }
            }

            Debug.WriteLine("Closing...");
            this.Text = "Closing...";
            client.SetGame(null);

            // Yes, a Thread.Sleep() since appearantly calling client.SetGame() does not wait for the new status to get sent.
            Thread.Sleep(300);
            client.Disconnect().Wait();
            client.Dispose();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Debug.WriteLine("Finishing closing...");
            if (this.logout)
            {
                this.Owner.Show();
                ((LoginForm)this.Owner).Logout();
            }
            else
                Application.Exit();
        }

        private void StatusSetAttempted(object sender, StatusUpdater.StatusSetAttemptedEventArgs e)
        {
            if (e.CurrentStatus == string.Empty)
                statusTextBox.Text = string.Empty;
            else
            {
                string rtf = @"{\rtf1\ansi{\colortbl;\red0\green0\blue0;}\cf1  " + PLAYING_TEXT + @" \b\cf0 " + e.CurrentStatus + @"\b0 }";
                if (statusTextBox.Text != " " + PLAYING_TEXT + " " + e.CurrentStatus)
                {
                    statusTextBox.Rtf = rtf;
                    toolTip1.SetToolTip(statusTextBox, statusTextBox.Text.Substring(1));
                }
            }

            if (e.Timer.Enabled)
            {
                if (e.NewStatus == null)
                {
                    updateTimerLabel.ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xCC, 0x84, 0x00);
                    updateTimerLabel.Text = "No status update possible yet";
                }
                else
                {
                    updateTimerLabel.ForeColor = System.Drawing.Color.Red;
                    if (e.NewStatus == string.Empty)
                        updateTimerLabel.Text = "Pending status removal";
                    else
                        updateTimerLabel.Text = "Pending status update: " + e.NewStatus;
                }
            }
            else
            {
                updateTimerLabel.ForeColor = System.Drawing.Color.Green;
                updateTimerLabel.Text = "Status update possible";
            }

            SetHelpLabel();
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
        }

        private void setStatusButton_Click(object sender, EventArgs e)
        {
            setStatusTextBox_KeyDown(sender, new KeyEventArgs(Keys.Enter));
        }

        private void setStatusTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (setStatusTextBox.Text == "HERE BE DRAGONS")
                {
                    new DebugForm().Show();
                    return;
                }
                if (!manual)
                    ChangeMode();
                ChangeStatus(setStatusTextBox.Text);
            }
        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.logout = true;
            this.Close();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(XmlDownloader.DownloadFiles() ? "Succesfully downloaded xml files" : "Failed to download xml files");
            playerManager.Reload();
        }
    }
}
