namespace QvCapDisconnect
{
    partial class Disconnect
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
            this.server = new System.Windows.Forms.ComboBox();
            this.DGV_Sessions = new System.Windows.Forms.DataGridView();
            this.SessionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SessionUserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SessionId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SessionStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExitButton = new System.Windows.Forms.Button();
            this.KillButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Sessions)).BeginInit();
            this.SuspendLayout();
            // 
            // server
            // 
            this.server.FormattingEnabled = true;
            this.server.Items.AddRange(new object[] {
            "localhost",
            "qtfrancetest"});
            this.server.Location = new System.Drawing.Point(12, 12);
            this.server.Name = "server";
            this.server.Size = new System.Drawing.Size(343, 21);
            this.server.TabIndex = 0;
            // 
            // DGV_Sessions
            // 
            this.DGV_Sessions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_Sessions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SessionName,
            this.SessionUserName,
            this.SessionId,
            this.SessionStatus});
            this.DGV_Sessions.Location = new System.Drawing.Point(12, 44);
            this.DGV_Sessions.Name = "DGV_Sessions";
            this.DGV_Sessions.Size = new System.Drawing.Size(343, 182);
            this.DGV_Sessions.TabIndex = 1;
            // 
            // SessionName
            // 
            this.SessionName.HeaderText = "Name";
            this.SessionName.Name = "SessionName";
            // 
            // SessionUserName
            // 
            this.SessionUserName.HeaderText = "User";
            this.SessionUserName.Name = "SessionUserName";
            // 
            // SessionId
            // 
            this.SessionId.HeaderText = "Id";
            this.SessionId.Name = "SessionId";
            // 
            // SessionStatus
            // 
            this.SessionStatus.HeaderText = "Status";
            this.SessionStatus.Name = "SessionStatus";
            // 
            // ExitButton
            // 
            this.ExitButton.Location = new System.Drawing.Point(188, 232);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(91, 32);
            this.ExitButton.TabIndex = 2;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // KillButton
            // 
            this.KillButton.Location = new System.Drawing.Point(91, 232);
            this.KillButton.Name = "KillButton";
            this.KillButton.Size = new System.Drawing.Size(91, 32);
            this.KillButton.TabIndex = 2;
            this.KillButton.Text = "Kill";
            this.KillButton.UseVisualStyleBackColor = true;
            this.KillButton.Click += new System.EventHandler(this.KillButton_Click);
            // 
            // Disconnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 276);
            this.Controls.Add(this.KillButton);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.DGV_Sessions);
            this.Controls.Add(this.server);
            this.MaximumSize = new System.Drawing.Size(375, 303);
            this.MinimumSize = new System.Drawing.Size(375, 303);
            this.Name = "Disconnect";
            this.Text = "Disconnect";
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Sessions)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox server;
        private System.Windows.Forms.DataGridView DGV_Sessions;
        private System.Windows.Forms.DataGridViewTextBoxColumn SessionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SessionUserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SessionId;
        private System.Windows.Forms.DataGridViewTextBoxColumn SessionStatus;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button KillButton;
    }
}

