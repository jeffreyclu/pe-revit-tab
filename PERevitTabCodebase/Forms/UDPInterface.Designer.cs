namespace PERevitTab.Forms
{
    partial class UDPInterface
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
            this.loginPanel = new System.Windows.Forms.Panel();
            this.logoutButton = new System.Windows.Forms.Button();
            this.userLabel = new System.Windows.Forms.Label();
            this.loginButton = new System.Windows.Forms.Button();
            this.tbdPanel = new System.Windows.Forms.Panel();
            this.tbdLabel = new System.Windows.Forms.Label();
            this.syncPanel = new System.Windows.Forms.Panel();
            this.lastSyncedLabel = new System.Windows.Forms.Label();
            this.syncButton = new System.Windows.Forms.Button();
            this.uploadPanel = new System.Windows.Forms.Panel();
            this.uploadButton = new System.Windows.Forms.Button();
            this.viewProjectPanel = new System.Windows.Forms.Panel();
            this.viewProjectButton = new System.Windows.Forms.Button();
            this.downloadPowerBIPanel = new System.Windows.Forms.Panel();
            this.downloadPowerBIButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.provideFeedback = new System.Windows.Forms.Button();
            this.loginPanel.SuspendLayout();
            this.tbdPanel.SuspendLayout();
            this.syncPanel.SuspendLayout();
            this.uploadPanel.SuspendLayout();
            this.viewProjectPanel.SuspendLayout();
            this.downloadPowerBIPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // loginPanel
            // 
            this.loginPanel.BackColor = System.Drawing.Color.LightPink;
            this.loginPanel.Controls.Add(this.logoutButton);
            this.loginPanel.Controls.Add(this.userLabel);
            this.loginPanel.Controls.Add(this.loginButton);
            this.loginPanel.Location = new System.Drawing.Point(20, 20);
            this.loginPanel.Name = "loginPanel";
            this.loginPanel.Size = new System.Drawing.Size(200, 200);
            this.loginPanel.TabIndex = 0;
            // 
            // logoutButton
            // 
            this.logoutButton.AutoSize = true;
            this.logoutButton.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logoutButton.Location = new System.Drawing.Point(0, 168);
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(200, 32);
            this.logoutButton.TabIndex = 2;
            this.logoutButton.Text = "Logout";
            this.logoutButton.UseVisualStyleBackColor = true;
            this.logoutButton.Click += new System.EventHandler(this.logoutButton_Click);
            // 
            // userLabel
            // 
            this.userLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userLabel.Location = new System.Drawing.Point(0, 142);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(200, 23);
            this.userLabel.TabIndex = 1;
            this.userLabel.Text = "User";
            this.userLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // loginButton
            // 
            this.loginButton.AutoSize = true;
            this.loginButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.loginButton.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loginButton.Location = new System.Drawing.Point(0, 168);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(200, 32);
            this.loginButton.TabIndex = 1;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // tbdPanel
            // 
            this.tbdPanel.BackColor = System.Drawing.Color.LightPink;
            this.tbdPanel.Controls.Add(this.tbdLabel);
            this.tbdPanel.Location = new System.Drawing.Point(240, 20);
            this.tbdPanel.Name = "tbdPanel";
            this.tbdPanel.Size = new System.Drawing.Size(200, 200);
            this.tbdPanel.TabIndex = 1;
            // 
            // tbdLabel
            // 
            this.tbdLabel.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbdLabel.Location = new System.Drawing.Point(0, 0);
            this.tbdLabel.Name = "tbdLabel";
            this.tbdLabel.Size = new System.Drawing.Size(200, 200);
            this.tbdLabel.TabIndex = 0;
            this.tbdLabel.Text = "TBD";
            this.tbdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // syncPanel
            // 
            this.syncPanel.BackColor = System.Drawing.Color.LightSkyBlue;
            this.syncPanel.Controls.Add(this.lastSyncedLabel);
            this.syncPanel.Controls.Add(this.syncButton);
            this.syncPanel.Location = new System.Drawing.Point(460, 20);
            this.syncPanel.Name = "syncPanel";
            this.syncPanel.Size = new System.Drawing.Size(200, 130);
            this.syncPanel.TabIndex = 2;
            // 
            // lastSyncedLabel
            // 
            this.lastSyncedLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lastSyncedLabel.Location = new System.Drawing.Point(0, 0);
            this.lastSyncedLabel.Name = "lastSyncedLabel";
            this.lastSyncedLabel.Size = new System.Drawing.Size(200, 100);
            this.lastSyncedLabel.TabIndex = 1;
            this.lastSyncedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // syncButton
            // 
            this.syncButton.AutoSize = true;
            this.syncButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.syncButton.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.syncButton.Location = new System.Drawing.Point(0, 98);
            this.syncButton.Name = "syncButton";
            this.syncButton.Size = new System.Drawing.Size(200, 32);
            this.syncButton.TabIndex = 0;
            this.syncButton.Text = "Sync";
            this.syncButton.UseVisualStyleBackColor = true;
            this.syncButton.Click += new System.EventHandler(this.syncButton_Click);
            // 
            // uploadPanel
            // 
            this.uploadPanel.BackColor = System.Drawing.Color.LightSkyBlue;
            this.uploadPanel.Controls.Add(this.uploadButton);
            this.uploadPanel.Location = new System.Drawing.Point(460, 170);
            this.uploadPanel.Name = "uploadPanel";
            this.uploadPanel.Size = new System.Drawing.Size(200, 50);
            this.uploadPanel.TabIndex = 3;
            // 
            // uploadButton
            // 
            this.uploadButton.AutoSize = true;
            this.uploadButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uploadButton.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uploadButton.Location = new System.Drawing.Point(0, 18);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(200, 32);
            this.uploadButton.TabIndex = 7;
            this.uploadButton.Text = "Upload";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // viewProjectPanel
            // 
            this.viewProjectPanel.BackColor = System.Drawing.Color.LightSalmon;
            this.viewProjectPanel.Controls.Add(this.viewProjectButton);
            this.viewProjectPanel.Location = new System.Drawing.Point(680, 20);
            this.viewProjectPanel.Name = "viewProjectPanel";
            this.viewProjectPanel.Size = new System.Drawing.Size(200, 50);
            this.viewProjectPanel.TabIndex = 4;
            // 
            // viewProjectButton
            // 
            this.viewProjectButton.AutoSize = true;
            this.viewProjectButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.viewProjectButton.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.viewProjectButton.Location = new System.Drawing.Point(0, 18);
            this.viewProjectButton.Name = "viewProjectButton";
            this.viewProjectButton.Size = new System.Drawing.Size(200, 32);
            this.viewProjectButton.TabIndex = 8;
            this.viewProjectButton.Text = "View Project";
            this.viewProjectButton.UseVisualStyleBackColor = true;
            this.viewProjectButton.Click += new System.EventHandler(this.viewProjectButton_Click);
            // 
            // downloadPowerBIPanel
            // 
            this.downloadPowerBIPanel.BackColor = System.Drawing.Color.LightSalmon;
            this.downloadPowerBIPanel.Controls.Add(this.downloadPowerBIButton);
            this.downloadPowerBIPanel.Location = new System.Drawing.Point(680, 100);
            this.downloadPowerBIPanel.Name = "downloadPowerBIPanel";
            this.downloadPowerBIPanel.Size = new System.Drawing.Size(200, 50);
            this.downloadPowerBIPanel.TabIndex = 5;
            // 
            // downloadPowerBIButton
            // 
            this.downloadPowerBIButton.AutoSize = true;
            this.downloadPowerBIButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.downloadPowerBIButton.Location = new System.Drawing.Point(0, 18);
            this.downloadPowerBIButton.Name = "downloadPowerBIButton";
            this.downloadPowerBIButton.Size = new System.Drawing.Size(200, 32);
            this.downloadPowerBIButton.TabIndex = 9;
            this.downloadPowerBIButton.Text = "Download PowerBI Template";
            this.downloadPowerBIButton.UseVisualStyleBackColor = true;
            this.downloadPowerBIButton.Click += new System.EventHandler(this.downloadPowerBIButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSalmon;
            this.panel1.Controls.Add(this.provideFeedback);
            this.panel1.Location = new System.Drawing.Point(680, 170);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 50);
            this.panel1.TabIndex = 6;
            // 
            // provideFeedback
            // 
            this.provideFeedback.AutoSize = true;
            this.provideFeedback.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.provideFeedback.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.provideFeedback.Location = new System.Drawing.Point(0, 18);
            this.provideFeedback.Name = "provideFeedback";
            this.provideFeedback.Size = new System.Drawing.Size(200, 32);
            this.provideFeedback.TabIndex = 9;
            this.provideFeedback.Text = "Provide Feedback";
            this.provideFeedback.UseVisualStyleBackColor = true;
            this.provideFeedback.Click += new System.EventHandler(this.provideFeedback_Click);
            // 
            // UDPInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 241);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.downloadPowerBIPanel);
            this.Controls.Add(this.viewProjectPanel);
            this.Controls.Add(this.uploadPanel);
            this.Controls.Add(this.syncPanel);
            this.Controls.Add(this.tbdPanel);
            this.Controls.Add(this.loginPanel);
            this.Name = "UDPInterface";
            this.Text = "UDPInterface";
            this.Load += new System.EventHandler(this.UDPInterface_Load);
            this.loginPanel.ResumeLayout(false);
            this.loginPanel.PerformLayout();
            this.tbdPanel.ResumeLayout(false);
            this.syncPanel.ResumeLayout(false);
            this.syncPanel.PerformLayout();
            this.uploadPanel.ResumeLayout(false);
            this.uploadPanel.PerformLayout();
            this.viewProjectPanel.ResumeLayout(false);
            this.viewProjectPanel.PerformLayout();
            this.downloadPowerBIPanel.ResumeLayout(false);
            this.downloadPowerBIPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel loginPanel;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.Panel tbdPanel;
        private System.Windows.Forms.Label tbdLabel;
        private System.Windows.Forms.Panel syncPanel;
        private System.Windows.Forms.Panel uploadPanel;
        private System.Windows.Forms.Panel viewProjectPanel;
        private System.Windows.Forms.Panel downloadPowerBIPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lastSyncedLabel;
        private System.Windows.Forms.Button syncButton;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.Button viewProjectButton;
        private System.Windows.Forms.Button downloadPowerBIButton;
        private System.Windows.Forms.Button provideFeedback;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.Button logoutButton;
    }
}