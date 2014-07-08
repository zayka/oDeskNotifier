namespace oDeskNotifier
{
    partial class SlidePopUp
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
            this.label_budget = new System.Windows.Forms.Label();
            this.linkLabel_Title = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label_budget
            // 
            this.label_budget.AutoSize = true;
            this.label_budget.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label_budget.Location = new System.Drawing.Point(12, 73);
            this.label_budget.Name = "label_budget";
            this.label_budget.Size = new System.Drawing.Size(77, 13);
            this.label_budget.TabIndex = 0;
            this.label_budget.Text = "Budget: Hourly";
            this.label_budget.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SlidePopUp_MouseClick);
            // 
            // linkLabel_Title
            // 
            this.linkLabel_Title.Location = new System.Drawing.Point(12, 9);
            this.linkLabel_Title.Name = "linkLabel_Title";
            this.linkLabel_Title.Size = new System.Drawing.Size(193, 61);
            this.linkLabel_Title.TabIndex = 2;
            this.linkLabel_Title.TabStop = true;
            this.linkLabel_Title.Text = "linkLabel1";
            this.linkLabel_Title.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_Title_LinkClicked);
            this.linkLabel_Title.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SlidePopUp_MouseClick);
            // 
            // SlidePopUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(219, 93);
            this.ControlBox = false;
            this.Controls.Add(this.linkLabel_Title);
            this.Controls.Add(this.label_budget);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SlidePopUp";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SlidePopUp_MouseClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_budget;
        private System.Windows.Forms.LinkLabel linkLabel_Title;
    }
}