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
        const int CHECKINTERVAL = 10000;
        const string PLAYINGTEXT = "Playing";

        StatusUpdater statusUpdater;
        Players.PlayerManager playerManager;

        public MainForm(DiscordClient client)
        {
            InitializeComponent();
            this.client = client;

            checkTimer.Interval = 1;
            checkTimer.Start();
            updateTimerLabel.Text = "Status update possible";
            updateTimerLabel.ForeColor = System.Drawing.Color.Green;
            SetHelpLabel();
            usernameLabel.Text = "Logged in as " + client.CurrentUser.Name;
            Console.WriteLine(usernameLabel.Text);

            statusUpdater = new StatusUpdater(updateTimer, client);
            statusUpdater.StatusSetAttempted += StatusSetAttempted;

            playerManager = new Players.PlayerManager();
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
            checkTimer.Interval = CHECKINTERVAL;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            
            this.Text = "Closing...";
            client.SetGame(null);

            // Yes, a Thread.Sleep() since appearantly calling client.SetGame() does not wait for the new status to get sent.
            Thread.Sleep(300);
            client.Disconnect();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Debug.WriteLine("Finishing closing...");
            Application.Exit();
        }

        private void StatusSetAttempted(object sender, StatusUpdater.StatusSetAttemptedEventArgs e)
        {
            if (e.CurrentStatus == string.Empty)
                statusTextBox.Text = string.Empty;
            else
                statusTextBox.Rtf = @"{\rtf1\ansi {\colortbl;\red0\green0\blue0;}\cf1  " + PLAYINGTEXT + @" \b\cf0 " + e.CurrentStatus + @"\b0 }";

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

        private void setStatusButton_Click(object sender, EventArgs e)
        {
            setStatusTextBox_KeyDown(sender, new KeyEventArgs(Keys.Enter));
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
