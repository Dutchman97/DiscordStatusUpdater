using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public class StatusUpdater
    {
        public const string NO_STATUS = "[[REMOVE STATUS]]";
        public event EventHandler<StatusSetAttemptedEventArgs> StatusSetAttempted;

        DiscordClient client;
        Timer timer;
        string status = null;
        string newStatus = null;

        public StatusUpdater(Timer timer, DiscordClient client)
        {
            if (timer == null)
                throw new NullReferenceException("Timer may not be null.");
            if (client == null)
                throw new NullReferenceException("DiscordClient may not be null.");
            
            this.client = client;
            this.timer = timer;
            timer.Tick += 
        }

        public void ChangeStatus(string status)
        {
            if (status == null)
                throw new NullReferenceException("Status can not be set to null.");
        }

        void AttemptSetStatus()
        {
            if (newStatus == null)
                return;

            StatusSetAttemptedEventArgs e = new StatusSetAttemptedEventArgs();

            if (!timer.Enabled)
            {
                status = newStatus;
                newStatus = null;
                client.SetGame(status);

                timer.Interval = 10000;
                timer.Start();
            }

            StatusSetAttempted(this, e);
        }

        public class StatusSetAttemptedEventArgs : EventArgs
        {
            public string Status { get; set; }
            public string NewStatus { get; set; }
        }
    }
}
