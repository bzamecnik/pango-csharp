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
            this.timeValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.gameStateValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.moneyValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.healthValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.livesValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
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
            this.mapLabel.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mapLabel.Location = new System.Drawing.Point(0, 0);
            this.mapLabel.Name = "mapLabel";
            this.mapLabel.Size = new System.Drawing.Size(345, 312);
            this.mapLabel.TabIndex = 1;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.timeLabel,
            this.timeValueLabel,
            this.gameStateLabel,
            this.gameStateValueLabel,
            this.moneyLabel,
            this.moneyValueLabel,
            this.healthLabel,
            this.healthValueLabel,
            this.livesLabel,
            this.livesValueLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 290);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(345, 22);
            this.statusStrip.TabIndex = 2;
            // 
            // timeValueLabel
            // 
            this.timeValueLabel.Name = "timeValueLabel";
            this.timeValueLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // gameStateValueLabel
            // 
            this.gameStateValueLabel.Name = "gameStateValueLabel";
            this.gameStateValueLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // moneyValueLabel
            // 
            this.moneyValueLabel.Name = "moneyValueLabel";
            this.moneyValueLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // healthValueLabel
            // 
            this.healthValueLabel.Name = "healthValueLabel";
            this.healthValueLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // livesValueLabel
            // 
            this.livesValueLabel.Name = "livesValueLabel";
            this.livesValueLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // timeLabel
            // 
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(31, 17);
            this.timeLabel.Text = "time:";
            // 
            // gameStateLabel
            // 
            this.gameStateLabel.Name = "gameStateLabel";
            this.gameStateLabel.Size = new System.Drawing.Size(36, 17);
            this.gameStateLabel.Text = "state:";
            // 
            // moneyLabel
            // 
            this.moneyLabel.Name = "moneyLabel";
            this.moneyLabel.Size = new System.Drawing.Size(43, 17);
            this.moneyLabel.Text = "money:";
            // 
            // healthLabel
            // 
            this.healthLabel.Name = "healthLabel";
            this.healthLabel.Size = new System.Drawing.Size(41, 17);
            this.healthLabel.Text = "health:";
            // 
            // livesLabel
            // 
            this.livesLabel.Name = "livesLabel";
            this.livesLabel.Size = new System.Drawing.Size(32, 17);
            this.livesLabel.Text = "lives:";
            // 
            // PangoGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 312);
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
        private System.Windows.Forms.ToolStripStatusLabel timeValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel gameStateValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel moneyValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel healthValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel livesValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel timeLabel;
        private System.Windows.Forms.ToolStripStatusLabel gameStateLabel;
        private System.Windows.Forms.ToolStripStatusLabel moneyLabel;
        private System.Windows.Forms.ToolStripStatusLabel healthLabel;
        private System.Windows.Forms.ToolStripStatusLabel livesLabel;

    }
}

