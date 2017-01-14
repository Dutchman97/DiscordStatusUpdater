namespace DiscordStatusUpdater
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.modeButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.checkTimer = new System.Windows.Forms.Timer(this.components);
            this.setStatusTextBox = new System.Windows.Forms.TextBox();
            this.setStatusButton = new System.Windows.Forms.Button();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.updateTimerLabel = new System.Windows.Forms.Label();
            this.helpLabel = new System.Windows.Forms.Label();
            this.statusTextBox = new System.Windows.Forms.RichTextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.usernameLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // modeButton
            // 
            this.modeButton.Location = new System.Drawing.Point(12, 84);
            this.modeButton.Name = "modeButton";
            this.modeButton.Size = new System.Drawing.Size(258, 49);
            this.modeButton.TabIndex = 0;
            this.modeButton.Text = "Click to change mode\r\nCurrently automatic";
            this.modeButton.UseVisualStyleBackColor = true;
            this.modeButton.Click += new System.EventHandler(this.modeButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(12, 9);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(101, 17);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "Current status:";
            // 
            // checkTimer
            // 
            this.checkTimer.Interval = 20500;
            this.checkTimer.Tick += new System.EventHandler(this.checkTimer_Tick);
            // 
            // setStatusTextBox
            // 
            this.setStatusTextBox.Location = new System.Drawing.Point(12, 139);
            this.setStatusTextBox.Name = "setStatusTextBox";
            this.setStatusTextBox.Size = new System.Drawing.Size(170, 22);
            this.setStatusTextBox.TabIndex = 4;
            this.setStatusTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.setStatusTextBox_KeyDown);
            // 
            // setStatusButton
            // 
            this.setStatusButton.Location = new System.Drawing.Point(188, 138);
            this.setStatusButton.Name = "setStatusButton";
            this.setStatusButton.Size = new System.Drawing.Size(82, 24);
            this.setStatusButton.TabIndex = 5;
            this.setStatusButton.Text = "Set status";
            this.setStatusButton.UseVisualStyleBackColor = true;
            this.setStatusButton.Click += new System.EventHandler(this.setStatusButton_Click);
            // 
            // updateTimerLabel
            // 
            this.updateTimerLabel.AutoSize = true;
            this.updateTimerLabel.Cursor = System.Windows.Forms.Cursors.Help;
            this.updateTimerLabel.Location = new System.Drawing.Point(12, 54);
            this.updateTimerLabel.Name = "updateTimerLabel";
            this.updateTimerLabel.Size = new System.Drawing.Size(152, 17);
            this.updateTimerLabel.TabIndex = 8;
            this.updateTimerLabel.Text = "Status update possible";
            this.toolTip1.SetToolTip(this.updateTimerLabel, "Discord only allows status updates every roughly 10 seconds.\r\nAny status update l" +
        "ess than 10 seconds after another status update will be pushed after the 10 seco" +
        "nds are over.");
            // 
            // helpLabel
            // 
            this.helpLabel.AutoSize = true;
            this.helpLabel.Cursor = System.Windows.Forms.Cursors.Help;
            this.helpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpLabel.Location = new System.Drawing.Point(170, 54);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(16, 12);
            this.helpLabel.TabIndex = 9;
            this.helpLabel.Text = "(?)";
            this.toolTip1.SetToolTip(this.helpLabel, "Discord only allows status updates every roughly 10 seconds.\r\nAny status update l" +
        "ess than 10 seconds after another status update will be pushed after the 10 seco" +
        "nds are over.");
            // 
            // statusTextBox
            // 
            this.statusTextBox.Location = new System.Drawing.Point(12, 29);
            this.statusTextBox.Multiline = false;
            this.statusTextBox.Name = "statusTextBox";
            this.statusTextBox.ReadOnly = true;
            this.statusTextBox.Size = new System.Drawing.Size(258, 22);
            this.statusTextBox.TabIndex = 11;
            this.statusTextBox.Text = "";
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.usernameLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 174);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(282, 25);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 12;
            this.statusStrip.Text = "statusStrip1";
            // 
            // usernameLabel
            // 
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(151, 20);
            this.usernameLabel.Text = "toolStripStatusLabel1";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 15000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 100;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(282, 199);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.statusTextBox);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.updateTimerLabel);
            this.Controls.Add(this.setStatusButton);
            this.Controls.Add(this.setStatusTextBox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.modeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DiscordStatusUpdater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Move += new System.EventHandler(this.MainForm_Move);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button modeButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Timer checkTimer;
        private System.Windows.Forms.TextBox setStatusTextBox;
        private System.Windows.Forms.Button setStatusButton;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Label updateTimerLabel;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.RichTextBox statusTextBox;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel usernameLabel;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

