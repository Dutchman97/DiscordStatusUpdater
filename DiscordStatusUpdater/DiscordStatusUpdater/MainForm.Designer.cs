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
            this.statusTextBox = new System.Windows.Forms.TextBox();
            this.checkTimer = new System.Windows.Forms.Timer(this.components);
            this.setStatusTextBox = new System.Windows.Forms.TextBox();
            this.setStatusButton = new System.Windows.Forms.Button();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.pendingLabel = new System.Windows.Forms.Label();
            this.updateTimerLabel = new System.Windows.Forms.Label();
            this.helpLabel = new System.Windows.Forms.Label();
            this.settingsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // modeButton
            // 
            this.modeButton.Location = new System.Drawing.Point(12, 101);
            this.modeButton.Name = "modeButton";
            this.modeButton.Size = new System.Drawing.Size(170, 49);
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
            // statusTextBox
            // 
            this.statusTextBox.Location = new System.Drawing.Point(12, 29);
            this.statusTextBox.Name = "statusTextBox";
            this.statusTextBox.ReadOnly = true;
            this.statusTextBox.Size = new System.Drawing.Size(258, 22);
            this.statusTextBox.TabIndex = 3;
            // 
            // checkTimer
            // 
            this.checkTimer.Interval = 20500;
            this.checkTimer.Tick += new System.EventHandler(this.checkTimer_Tick);
            // 
            // setStatusTextBox
            // 
            this.setStatusTextBox.Location = new System.Drawing.Point(12, 156);
            this.setStatusTextBox.Name = "setStatusTextBox";
            this.setStatusTextBox.Size = new System.Drawing.Size(170, 22);
            this.setStatusTextBox.TabIndex = 4;
            this.setStatusTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.setStatusTextBox_KeyDown);
            // 
            // setStatusButton
            // 
            this.setStatusButton.Location = new System.Drawing.Point(188, 155);
            this.setStatusButton.Name = "setStatusButton";
            this.setStatusButton.Size = new System.Drawing.Size(82, 22);
            this.setStatusButton.TabIndex = 5;
            this.setStatusButton.Text = "Set status";
            this.setStatusButton.UseVisualStyleBackColor = true;
            this.setStatusButton.Click += new System.EventHandler(this.setStatusButton_Click);
            // 
            // updateTimer
            // 
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // pendingLabel
            // 
            this.pendingLabel.AutoSize = true;
            this.pendingLabel.Cursor = System.Windows.Forms.Cursors.Help;
            this.pendingLabel.Location = new System.Drawing.Point(12, 71);
            this.pendingLabel.Name = "pendingLabel";
            this.pendingLabel.Size = new System.Drawing.Size(171, 17);
            this.pendingLabel.TabIndex = 7;
            this.pendingLabel.Text = "No pending status update";
            this.pendingLabel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.updateLabel_MouseClick);
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
            this.updateTimerLabel.ForeColorChanged += new System.EventHandler(this.updateTimerLabel_ForeColorChanged);
            this.updateTimerLabel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.updateLabel_MouseClick);
            // 
            // helpLabel
            // 
            this.helpLabel.AutoSize = true;
            this.helpLabel.Cursor = System.Windows.Forms.Cursors.Help;
            this.helpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 4.4F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpLabel.Location = new System.Drawing.Point(170, 54);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(15, 9);
            this.helpLabel.TabIndex = 9;
            this.helpLabel.Text = "(?)";
            this.helpLabel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.updateLabel_MouseClick);
            // 
            // settingsButton
            // 
            this.settingsButton.Location = new System.Drawing.Point(188, 101);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(82, 49);
            this.settingsButton.TabIndex = 10;
            this.settingsButton.Text = "Settings";
            this.settingsButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(282, 190);
            this.Controls.Add(this.settingsButton);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.updateTimerLabel);
            this.Controls.Add(this.pendingLabel);
            this.Controls.Add(this.setStatusButton);
            this.Controls.Add(this.setStatusTextBox);
            this.Controls.Add(this.statusTextBox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.modeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DiscordStatusUpdater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button modeButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.TextBox statusTextBox;
        private System.Windows.Forms.Timer checkTimer;
        private System.Windows.Forms.TextBox setStatusTextBox;
        private System.Windows.Forms.Button setStatusButton;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Label pendingLabel;
        private System.Windows.Forms.Label updateTimerLabel;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.Button settingsButton;
    }
}

