using System.ComponentModel;
using System.Management.Automation;
using HyperView.Class;

namespace HyperView.Forms
{
    public partial class ManageVMGroupMembers : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string GroupName { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> AllVMs { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, System.Collections.ObjectModel.Collection<PSObject>> ExecutePowerShellCommand { get; set; }

        public ManageVMGroupMembers()
        {
            InitializeComponent();
        }

        private void ManageVMGroupMembers_Load(object sender, EventArgs e)
        {
            FileLogger.Message($"ManageVMGroupMembers form loaded for group '{GroupName}'",
                FileLogger.EventType.Information, 2125);

            if (!string.IsNullOrEmpty(GroupName))
            {
                this.Text = $"Manage VM Group Members - {GroupName}";
                UpdateMemberLists();
            }
        }

        private void UpdateMemberLists()
        {
            try
            {
                FileLogger.Message("Updating member lists...",
                    FileLogger.EventType.Information, 2126);

                if (listboxAvailable == null || listboxMembers == null)
                {
                    FileLogger.Message("ListBox controls not found",
                        FileLogger.EventType.Warning, 2127);
                    return;
                }

                listboxAvailable.Items.Clear();
                listboxMembers.Items.Clear();

                // Get current group info
                var vmGroups = VMGroups.GetHyperVVMGroups(ExecutePowerShellCommand);
                var currentGroup = vmGroups.FirstOrDefault(g => g.Name == GroupName);

                if (currentGroup == null)
                {
                    MessageBox.Show($@"VM Group '{GroupName}' not found.",
                        @"Group Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                var currentMembers = currentGroup.VMMembers ?? new List<string>();

                // Populate available VMs (exclude current members)
                if (AllVMs != null)
                {
                    foreach (var vm in AllVMs)
                    {
                        if (!currentMembers.Contains(vm))
                        {
                            listboxAvailable.Items.Add(vm);
                        }
                    }
                }

                // Populate current members
                foreach (var member in currentMembers)
                {
                    listboxMembers.Items.Add(member);
                }

                FileLogger.Message($"Member lists updated - Available: {listboxAvailable.Items.Count}, Members: {listboxMembers.Items.Count}",
                    FileLogger.EventType.Information, 2128);
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error updating member lists: {ex.Message}",
                    FileLogger.EventType.Error, 2129);
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User clicked Add button",
                    FileLogger.EventType.Information, 2130);

                if (listboxAvailable.SelectedItems.Count == 0)
                {
                    MessageBox.Show(@"Please select VMs to add.",
                        @"No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var selectedVMs = new List<string>();
                foreach (var item in listboxAvailable.SelectedItems)
                {
                    selectedVMs.Add(item.ToString());
                }

                int successCount = 0;
                int errorCount = 0;

                this.Cursor = Cursors.WaitCursor;

                foreach (var vmName in selectedVMs)
                {
                    var result = VMGroups.AddVMToGroup(vmName, GroupName, ExecutePowerShellCommand);
                    if (result.Success)
                    {
                        successCount++;
                        // Optimistic UI update - add immediately
                        listboxAvailable.Items.Remove(vmName);
                        listboxMembers.Items.Add(vmName);
                    }
                    else
                    {
                        errorCount++;
                        FileLogger.Message($"Failed to add VM '{vmName}' to group: {result.Error}",
                            FileLogger.EventType.Error, 2131);
                    }
                }

                this.Cursor = Cursors.Default;

                // Refresh after a short delay to confirm (Hyper-V caching issue)
                Task.Delay(500).ContinueWith(_ =>
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke(new Action(() =>
                        {
                            UpdateMemberLists();
                        }));
                    }
                });

                if (successCount > 0)
                {
                    string message = $"Successfully added {successCount} VM(s) to group.";
                    if (errorCount > 0)
                    {
                        message += $" {errorCount} VM(s) failed to be added.";
                    }

                    MessageBox.Show(message,
                        @"Operation Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    FileLogger.Message($"Add operation complete - Success: {successCount}, Errors: {errorCount}",
                        FileLogger.EventType.Information, 2132);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;

                string errorMsg = $"Error adding VMs to group: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2133);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User clicked Remove button",
                    FileLogger.EventType.Information, 2134);

                if (listboxMembers.SelectedItems.Count == 0)
                {
                    MessageBox.Show(@"Please select VMs to remove.",
                        @"No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var confirmResult = MessageBox.Show(
                    @"Are you sure you want to remove the selected VM(s) from the group?",
                    @"Confirm Removal",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }

                var selectedVMs = new List<string>();
                foreach (var item in listboxMembers.SelectedItems)
                {
                    selectedVMs.Add(item.ToString());
                }

                int successCount = 0;
                int errorCount = 0;

                this.Cursor = Cursors.WaitCursor;

                foreach (var vmName in selectedVMs)
                {
                    var result = VMGroups.RemoveVMFromGroup(vmName, GroupName, ExecutePowerShellCommand);
                    if (result.Success)
                    {
                        successCount++;
                        // Optimistic UI update - remove immediately
                        listboxMembers.Items.Remove(vmName);
                        // Add back to available list
                        if (AllVMs != null && AllVMs.Contains(vmName))
                        {
                            listboxAvailable.Items.Add(vmName);
                        }
                    }
                    else
                    {
                        errorCount++;
                        FileLogger.Message($"Failed to remove VM '{vmName}' from group: {result.Error}",
                            FileLogger.EventType.Error, 2135);
                    }
                }

                this.Cursor = Cursors.Default;

                // Refresh after a short delay to confirm (Hyper-V caching issue)
                Task.Delay(500).ContinueWith(_ =>
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke(new Action(() =>
                        {
                            UpdateMemberLists();
                        }));
                    }
                });

                if (successCount > 0)
                {
                    string message = $"Successfully removed {successCount} VM(s) from group.";
                    if (errorCount > 0)
                    {
                        message += $" {errorCount} VM(s) failed to be removed.";
                    }

                    MessageBox.Show(message,
                        @"Operation Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    FileLogger.Message($"Remove operation complete - Success: {successCount}, Errors: {errorCount}",
                        FileLogger.EventType.Information, 2136);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;

                string errorMsg = $"Error removing VMs from group: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2137);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            FileLogger.Message("User closed ManageVMGroupMembers form",
                FileLogger.EventType.Information, 2138);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
