namespace HVTools.Forms
{
    partial class DisclaimersForm
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
            richTextBoxDisclaimers = new RichTextBox();
            buttonClose = new Button();
            linkLabelLicense = new LinkLabel();
            labelTitle = new Label();
            panelBottom = new Panel();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // richTextBoxDisclaimers
            // 
            richTextBoxDisclaimers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxDisclaimers.BackColor = Color.White;
            richTextBoxDisclaimers.BorderStyle = BorderStyle.FixedSingle;
            richTextBoxDisclaimers.Font = new Font("Consolas", 9F);
            richTextBoxDisclaimers.Location = new Point(12, 45);
            richTextBoxDisclaimers.Name = "richTextBoxDisclaimers";
            richTextBoxDisclaimers.ReadOnly = true;
            richTextBoxDisclaimers.Size = new Size(760, 485);
            richTextBoxDisclaimers.TabIndex = 0;
            richTextBoxDisclaimers.Text = "";
            // 
            // buttonClose
            // 
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.Location = new Point(665, 10);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(95, 30);
            buttonClose.TabIndex = 1;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // linkLabelLicense
            // 
            linkLabelLicense.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelLicense.AutoSize = true;
            linkLabelLicense.Location = new Point(10, 17);
            linkLabelLicense.Name = "linkLabelLicense";
            linkLabelLicense.Size = new Size(166, 15);
            linkLabelLicense.TabIndex = 2;
            linkLabelLicense.TabStop = true;
            linkLabelLicense.Text = "View Full License on GitHub ↗";
            linkLabelLicense.LinkClicked += linkLabelLicense_LinkClicked;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelTitle.Location = new Point(12, 12);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(284, 21);
            labelTitle.TabIndex = 3;
            labelTitle.Text = "⚖️ Disclaimers && Legal Information";
            // 
            // panelBottom
            // 
            panelBottom.BackColor = Color.White;
            panelBottom.BorderStyle = BorderStyle.FixedSingle;
            panelBottom.Controls.Add(linkLabelLicense);
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 536);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(784, 50);
            panelBottom.TabIndex = 4;
            // 
            // DisclaimersForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(784, 586);
            Controls.Add(panelBottom);
            Controls.Add(labelTitle);
            Controls.Add(richTextBoxDisclaimers);
            MinimumSize = new Size(600, 400);
            Name = "DisclaimersForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Disclaimers & Legal Information";
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBoxDisclaimers;
        private Button buttonClose;
        private LinkLabel linkLabelLicense;
        private Label labelTitle;
        private Panel panelBottom;
    }
}
