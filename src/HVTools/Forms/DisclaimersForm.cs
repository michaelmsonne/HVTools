using HVTools.Helpers;
using static HVTools.Helpers.FileLogger;

namespace HVTools.Forms
{
    /// <summary>
    /// Form displaying disclaimers and legal information
    /// </summary>
    public partial class DisclaimersForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the DisclaimersForm
        /// </summary>
        public DisclaimersForm()
        {
            InitializeComponent();
            LoadDisclaimerContent();
            
            Message("DisclaimersForm opened", EventType.Information, 9000);
        }

        /// <summary>
        /// Loads the disclaimer content into the text box
        /// </summary>
        private void LoadDisclaimerContent()
        {
            string disclaimerText = @"DISCLAIMERS & LEGAL INFORMATION

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚠️ WARRANTY DISCLAIMER

This software is provided ""as-is"" without warranty of any kind, express or implied. 
The author is not responsible for any damages, data loss, or other issues arising from 
the use of this software. Use at your own risk.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

™️ TRADEMARK NOTICE

This is an independent project and is not affiliated with, endorsed by, or sponsored 
by Microsoft Corporation. Microsoft, Hyper-V, and Windows are registered trademarks 
of Microsoft Corporation.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

© COPYRIGHT

© 2026 Michael Morten Sonne. All rights reserved.

This software is freeware for personal and commercial use.

Open-source licensed under the terms specified in the LICENSE file. If redistribution 
is permitted, it must:
• Maintain the same open-source license
• Include proper attribution to the original author
• Reference the original project repository

Reverse engineering without express written permission is prohibited.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📝 LICENSE

For complete license terms and conditions, please visit:
" + Globals.ToolStings.UrlGitHub + @"

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔒 PRIVACY & DATA COLLECTION

This application does not collect, transmit, or store any personal information or 
usage data. All operations are performed locally on your infrastructure.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚖️ LIABILITY LIMITATION

Under no circumstances shall the author or copyright holder be liable for any claim, 
damages, or other liability arising from the use of this software, including but not 
limited to:
• Data loss or corruption
• Service interruptions
• Hardware or software failures
• Security breaches
• Financial losses

By using this software, you acknowledge and accept these terms.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";

            richTextBoxDisclaimers.Text = disclaimerText;
        }

        /// <summary>
        /// Handles the Close button click
        /// </summary>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Message("DisclaimersForm closed", EventType.Information, 9001);
            Close();
        }

        /// <summary>
        /// Handles the Visit License link click
        /// </summary>
        private void linkLabelLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Globals.ToolStings.UrlGitHub,
                    UseShellExecute = true
                });

                Message($"User opened license URL: {Globals.ToolStings.UrlGitHub}", EventType.Information, 9002);
            }
            catch (Exception ex)
            {
                Message($"Failed to open license URL: {ex.Message}", EventType.Error, 9003);
                MessageBox.Show($@"Failed to open the license URL: {ex.Message}", 
                    @"Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}
