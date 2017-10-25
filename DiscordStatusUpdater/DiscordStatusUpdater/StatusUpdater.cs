using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;

namespace DiscordStatusUpdater
{
    public class StatusUpdater
    {
        public event EventHandler<StatusSetAttemptedEventArgs> StatusSetAttempted;

        const int UPDATE_INTERVAL = 11000;
        DiscordSocketClient client;
        Timer timer;
        string curStatus = string.Empty;
        string newStatus = null;

        public StatusUpdater(Timer timer, DiscordSocketClient client)
        {
            Debug.WriteLine("Loading StatusUpdater");

            if (timer == null)
                throw new NullReferenceException("Timer may not be null.");
            if (client == null)
                throw new NullReferenceException("DiscordClient may not be null.");
            
            this.client = client;
            this.timer = timer;
            timer.Interval = 1000;
            timer.Start();
            timer.Tick += new EventHandler(AttemptToSetStatus);
        }

        public void ChangeStatus(string status)
        {
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("!!Trying to change status to " + status);

            if (status == null)
                throw new NullReferenceException("Status can not be set to null.");

            newStatus = status;

            AttemptToSetStatus(this, null);
        }

        void AttemptToSetStatus(object sender, EventArgs e1)
        {
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(sender.ToString());
            Debug.WriteLine("Attempting to set new status...");

            if (sender == timer)
                timer.Stop();

            if (newStatus == null)
            {
                Debug.WriteLine("No new status");
            }
            else if (newStatus == curStatus)
            {
                newStatus = null;
                Debug.WriteLine("Same status, aborting");
            }
            else
            {
                if (!timer.Enabled)
                {
                    Debug.WriteLine("Timer is disabled, setting status to " + newStatus);
                    client.SetGameAsync(newStatus == string.Empty ? null : newStatus).Wait();
                    curStatus = newStatus;
                    newStatus = null;

                    timer.Interval = UPDATE_INTERVAL;
                    timer.Start();
                }
                else
                {
                    Debug.WriteLine("Timer is enabled");
                }
            }

            if (StatusSetAttempted != null)
            {
                StatusSetAttemptedEventArgs e = new StatusSetAttemptedEventArgs();
                e.CurrentStatus = curStatus;
                e.NewStatus = newStatus;
                e.Timer = timer;
                StatusSetAttempted(this, e);
            }
        }

        public void Dispose()
        {
            timer.Tick -= AttemptToSetStatus;
        }

        public bool StatusUpdatePossible
        { get { return newStatus == null && !timer.Enabled; } }

        public class StatusSetAttemptedEventArgs : EventArgs
        {
            public string CurrentStatus { get; set; }
            public string NewStatus { get; set; }
            public Timer Timer { get; set; }
        }
    }
}
