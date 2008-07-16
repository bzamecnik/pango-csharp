namespace gui
{
    partial class PangoGameForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.mapLabel = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.timeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.gameStateLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.moneyLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.healthLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.livesLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mapLabel
            // 
            this.mapLabel.AllowDrop = true;
            this.mapLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mapLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapLabel.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.mapLabel.Location = new System.Drawing.Point(0, 0);
            this.mapLabel.Name = "mapLabel";
            this.mapLabel.Size = new System.Drawing.Size(292, 266);
            this.mapLabel.TabIndex = 1;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.timeLabel,
            this.gameStateLabel,
            this.moneyLabel,
            this.healthLabel,
            this.livesLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 244);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(292, 22);
            this.statusStrip.TabIndex = 2;
            // 
            // timeLabel
            // 
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(27, 17);
            this.timeLabel.Text = "time";
            // 
            // gameStateLabel
            // 
            this.gameStateLabel.Name = "gameStateLabel";
            this.gameStateLabel.Size = new System.Drawing.Size(61, 17);
            this.gameStateLabel.Text = "game state";
            // 
            // moneyLabel
            // 
            this.moneyLabel.Name = "moneyLabel";
            this.moneyLabel.Size = new System.Drawing.Size(39, 17);
            this.moneyLabel.Text = "money";
            // 
            // healthLabel
            // 
            this.healthLabel.Name = "healthLabel";
            this.healthLabel.Size = new System.Drawing.Size(37, 17);
            this.healthLabel.Text = "health";
            // 
            // livesLabel
            // 
            this.livesLabel.Name = "livesLabel";
            this.livesLabel.Size = new System.Drawing.Size(28, 17);
            this.livesLabel.Text = "lives";
            // 
            // PangoGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mapLabel);
            this.Name = "PangoGameForm";
            this.Text = "Pango";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.processKeyboardInput);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mapLabel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel timeLabel;
        private System.Windows.Forms.ToolStripStatusLabel gameStateLabel;
        private System.Windows.Forms.ToolStripStatusLabel moneyLabel;
        private System.Windows.Forms.ToolStripStatusLabel healthLabel;
        private System.Windows.Forms.ToolStripStatusLabel livesLabel;

    }
}

