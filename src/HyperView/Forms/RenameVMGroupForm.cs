using System.ComponentModel;
using HyperView.Class;

namespace HyperView.Forms
{
    public partial class RenameVMGroupForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CurrentGroupName { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NewGroupName { get; private set; }

        public RenameVMGroupForm()
        {
            InitializeComponent();
        }

        private void RenameVMGroupForm_Load(object sender, EventArgs e)
        {
            FileLogger.Message("RenameVMGroupForm loaded",
                FileLogger.EventType.Information, 2086);

            // Display current name and set it in textbox
            if (!string.IsNullOrEmpty(CurrentGroupName))
            {
                if (labelCurrentNameValue != null)
                    labelCurrentNameValue.Text = CurrentGroupName;

                if (textboxNewName != null)
                {
                    textboxNewName.Text = CurrentGroupName;
                    textboxNewName.SelectAll();
                    textboxNewName.Focus();
                }

                FileLogger.Message($"Rename form loaded for VM Group '{CurrentGroupName}'",
                    FileLogger.EventType.Information, 2087);
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textboxNewName?.Text))
            {
                MessageBox.Show(@"Please enter a new group name.",
                    @"Invalid Input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string newName = textboxNewName.Text.Trim();

            if (newName == CurrentGroupName)
            {
                MessageBox.Show(@"The new name must be different from the current name.",
                    @"Invalid Input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Store result
            NewGroupName = newName;

            FileLogger.Message($"VM Group rename confirmed: '{CurrentGroupName}' -> '{NewGroupName}'",
                FileLogger.EventType.Information, 2088);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            FileLogger.Message("VM Group rename cancelled",
                FileLogger.EventType.Information, 2089);

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
