using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public class StatusUpdater
    {
        public event EventHandler<StatusSetAttemptedEventArgs> StatusSetAttempted;

        DiscordClient client;
        Timer timer;
        string curStatus = null;
        string newStatus = null;

        public StatusUpdater(Timer timer, DiscordClient client)
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
            timer.Tick += new EventHandler(AttemptSetStatus);
        }

        public void ChangeStatus(string status)
        {
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("!!Trying to change status to " + status);

            if (status == null)
                throw new NullReferenceException("Status can not be set to null.");

            newStatus = status;

            AttemptSetStatus(this, null);
        }

        void AttemptSetStatus(object sender, EventArgs e1)
        {
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(sender.ToString());
            Debug.WriteLine("Attempting to set new status...");

            if (sender == timer)
                timer.Stop();

            if (newStatus == null)
            {
                Debug.WriteLine("No new status");
                //return;
            }
            else if (newStatus == curStatus)
            {
                Debug.WriteLine("Same status, aborting");
                //return;
            }
            else
            {
                if (!timer.Enabled)
                {
                    Debug.WriteLine("Timer is disabled, setting status to " + newStatus);
                    client.SetGame(newStatus == string.Empty ? null : newStatus);
                    curStatus = newStatus;
                    newStatus = null;

                    timer.Interval = 10000;
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
