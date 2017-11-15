namespace chat_ui
{
    partial class Settings
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
            this.close = new System.Windows.Forms.Button();
            this.restartConnections = new System.Windows.Forms.Button();
            this.portNumber = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.username = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.neighbors = new System.Windows.Forms.TextBox();
            this.showIDs = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.showDebug = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.portNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(195, 235);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 0;
            this.close.Text = "OK";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // restartConnections
            // 
            this.restartConnections.Location = new System.Drawing.Point(76, 235);
            this.restartConnections.Name = "restartConnections";
            this.restartConnections.Size = new System.Drawing.Size(113, 23);
            this.restartConnections.TabIndex = 1;
            this.restartConnections.Text = "Restart connections";
            this.restartConnections.UseVisualStyleBackColor = true;
            this.restartConnections.Click += new System.EventHandler(this.button2_Click);
            // 
            // portNumber
            // 
            this.portNumber.Location = new System.Drawing.Point(150, 96);
            this.portNumber.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.portNumber.Name = "portNumber";
            this.portNumber.Size = new System.Drawing.Size(120, 20);
            this.portNumber.TabIndex = 2;
            this.portNumber.Value = new decimal(new int[] {
            1212,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Port number (need restart)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Username";
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(150, 12);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(120, 20);
            this.username.TabIndex = 5;
            this.username.Text = "Kadoc";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(249, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Initial potential neighbors (one per line, need restart)";
            // 
            // neighbors
            // 
            this.neighbors.Location = new System.Drawing.Point(12, 145);
            this.neighbors.Multiline = true;
            this.neighbors.Name = "neighbors";
            this.neighbors.Size = new System.Drawing.Size(258, 84);
            this.neighbors.TabIndex = 7;
            this.neighbors.Text = "jch.irif.fr:1212";
            // 
            // showIDs
            // 
            this.showIDs.AutoSize = true;
            this.showIDs.Location = new System.Drawing.Point(150, 38);
            this.showIDs.Name = "showIDs";
            this.showIDs.Size = new System.Drawing.Size(72, 17);
            this.showIDs.TabIndex = 8;
            this.showIDs.Text = "Show IDs";
            this.showIDs.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Display";
            // 
            // showDebug
            // 
            this.showDebug.AutoSize = true;
            this.showDebug.Checked = true;
            this.showDebug.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDebug.Location = new System.Drawing.Point(150, 61);
            this.showDebug.Name = "showDebug";
            this.showDebug.Size = new System.Drawing.Size(129, 17);
            this.showDebug.TabIndex = 10;
            this.showDebug.Text = "Show errors/warnings";
            this.showDebug.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 270);
            this.Controls.Add(this.showDebug);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.showIDs);
            this.Controls.Add(this.neighbors);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.username);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.portNumber);
            this.Controls.Add(this.restartConnections);
            this.Controls.Add(this.close);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Settings";
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.portNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button close;
        private System.Windows.Forms.Button restartConnections;
        private System.Windows.Forms.NumericUpDown portNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox username;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox neighbors;
        private System.Windows.Forms.CheckBox showIDs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox showDebug;
    }
}