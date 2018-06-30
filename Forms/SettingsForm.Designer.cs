namespace TwitchChatCoroutines.Forms
{
    partial class SettingsForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.twitchBox = new System.Windows.Forms.CheckBox();
            this.bttvBox = new System.Windows.Forms.CheckBox();
            this.emotesBox = new System.Windows.Forms.CheckBox();
            this.ffzBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SlateGray;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(12, 281);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(513, 32);
            this.button1.TabIndex = 0;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // twitchBox
            // 
            this.twitchBox.AutoSize = true;
            this.twitchBox.Location = new System.Drawing.Point(51, 39);
            this.twitchBox.Name = "twitchBox";
            this.twitchBox.Size = new System.Drawing.Size(159, 20);
            this.twitchBox.TabIndex = 1;
            this.twitchBox.Text = "Twitch Emote Caching";
            this.twitchBox.UseVisualStyleBackColor = true;
            // 
            // bttvBox
            // 
            this.bttvBox.AutoSize = true;
            this.bttvBox.Checked = true;
            this.bttvBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.bttvBox.Enabled = false;
            this.bttvBox.Location = new System.Drawing.Point(51, 65);
            this.bttvBox.Name = "bttvBox";
            this.bttvBox.Size = new System.Drawing.Size(157, 20);
            this.bttvBox.TabIndex = 2;
            this.bttvBox.Text = "BTTV Emote Caching";
            this.bttvBox.UseVisualStyleBackColor = true;
            // 
            // emotesBox
            // 
            this.emotesBox.AutoSize = true;
            this.emotesBox.Location = new System.Drawing.Point(13, 13);
            this.emotesBox.Name = "emotesBox";
            this.emotesBox.Size = new System.Drawing.Size(118, 20);
            this.emotesBox.TabIndex = 3;
            this.emotesBox.Text = "Emote Caching";
            this.emotesBox.UseVisualStyleBackColor = true;
            // 
            // ffzBox
            // 
            this.ffzBox.AutoSize = true;
            this.ffzBox.Checked = true;
            this.ffzBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ffzBox.Enabled = false;
            this.ffzBox.Location = new System.Drawing.Point(51, 91);
            this.ffzBox.Name = "ffzBox";
            this.ffzBox.Size = new System.Drawing.Size(145, 20);
            this.ffzBox.TabIndex = 4;
            this.ffzBox.Text = "FFZ Emote Caching";
            this.ffzBox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSlateGray;
            this.ClientSize = new System.Drawing.Size(537, 325);
            this.Controls.Add(this.ffzBox);
            this.Controls.Add(this.emotesBox);
            this.Controls.Add(this.bttvBox);
            this.Controls.Add(this.twitchBox);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox twitchBox;
        private System.Windows.Forms.CheckBox bttvBox;
        private System.Windows.Forms.CheckBox emotesBox;
        private System.Windows.Forms.CheckBox ffzBox;
    }
}