using System.Management.Automation;

namespace HVTools.Helpers
{
    public class VmGroupInfo
    {
        public string Name { get; set; }
        public string GroupType { get; set; }
        public string GroupTypeDisplay { get; set; }
        public int VmCount { get; set; }
        public string VmList { get; set; }
        public List<string> VmMembers { get; set; } = new List<string>();
        public int GroupCount { get; set; }
        public string GroupList { get; set; }
        public List<string> GroupMembers { get; set; } = new List<string>();
        public int TotalMembers { get; set; }
        public string ComputerName { get; set; }
        public string Id { get; set; }
    }

    public class VmGroupDeletionResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public bool CanForce { get; set; }
        public int VmCount { get; set; }
        public List<string> VmNames { get; set; } = new List<string>();
    }

    public class VmGroupCreationResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class VmGroupRenameResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class VmGroupMemberResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public static class VmGroups
    {
        public static List<VmGroupInfo> GetHyperVvmGroups(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Retrieving VM Groups from '{SessionContext.ServerName}'...",
                    FileLogger.EventType.Information, 2061);

                // Build PowerShell script to get VM Groups
                string script = @"
                    $groups = Get-VMGroup -ErrorAction SilentlyContinue
                    $result = @()
                    
                    foreach ($group in $groups) {
                        $vmMembers = @()
                        $groupMembers = @()
                        
                        # Process VM members
                        if ($group.VMMembers -and $group.VMMembers.Count -gt 0) {
                            foreach ($vmMember in $group.VMMembers) {
                                if ($vmMember -and $vmMember.Name) {
                                    $vmMembers += $vmMember.Name
                                }
                            }
                        }
                        
                        # Process group members (nested groups)
                        if ($group.VMGroupMembers -and $group.VMGroupMembers.Count -gt 0) {
                            foreach ($groupMember in $group.VMGroupMembers) {
                                if ($groupMember -and $groupMember.Name) {
                                    $groupMembers += $groupMember.Name
                                }
                            }
                        }
                        
                        $groupObject = [PSCustomObject]@{
                            Name = $group.Name
                            GroupType = $group.GroupType.ToString()
                            VMMembers = $vmMembers
                            VMCount = $vmMembers.Count
                            VMList = if ($vmMembers.Count -gt 0) { $vmMembers -join ', ' } else { 'No VMs' }
                            GroupMembers = $groupMembers
                            GroupCount = $groupMembers.Count
                            GroupList = if ($groupMembers.Count -gt 0) { $groupMembers -join ', ' } else { 'No Nested Groups' }
                            TotalMembers = $vmMembers.Count + $groupMembers.Count
                            ComputerName = $group.ComputerName
                            Id = $group.Id
                        }
                        
                        $result += $groupObject
                    }
                    
                    return $result
                ";

                var results = executePowerShellCommand(script);

                if (results == null || results.Count == 0)
                {
                    FileLogger.Message("No VM Groups found",
                        FileLogger.EventType.Information, 2062);
                    return new List<VmGroupInfo>();
                }

                var vmGroups = new List<VmGroupInfo>();

                foreach (var result in results)
                {
                    try
                    {
                        var groupInfo = new VmGroupInfo
                        {
                            Name = result.Properties["Name"]?.Value?.ToString() ?? "Unknown",
                            GroupType = result.Properties["GroupType"]?.Value?.ToString() ?? "Unknown",
                            VmCount = Convert.ToInt32(result.Properties["VMCount"]?.Value ?? 0),
                            VmList = result.Properties["VMList"]?.Value?.ToString() ?? "",
                            GroupCount = Convert.ToInt32(result.Properties["GroupCount"]?.Value ?? 0),
                            GroupList = result.Properties["GroupList"]?.Value?.ToString() ?? "",
                            TotalMembers = Convert.ToInt32(result.Properties["TotalMembers"]?.Value ?? 0),
                            ComputerName = result.Properties["ComputerName"]?.Value?.ToString() ?? SessionContext.ServerName,
                            Id = result.Properties["Id"]?.Value?.ToString() ?? ""
                        };

                        // Process VM Members array
                        var vmMembersProperty = result.Properties["VMMembers"]?.Value;
#if DEBUG
                        FileLogger.Message($"VM Members property type for group '{groupInfo.Name}'", //: {vmMembersProperty?.GetType().FullName ?? "null"}",
                            FileLogger.EventType.Information, 2072);
#endif
                        if (vmMembersProperty != null)
                        {
                            // Unwrap PSObject if needed
                            object actualValue = vmMembersProperty;
                            if (vmMembersProperty is PSObject psObj)
                            {
                                actualValue = psObj.BaseObject;
#if debug
                                FileLogger.Message($"Unwrapped PSObject - BaseObject type: {actualValue?.GetType().FullName ?? "null"}",
                                    FileLogger.EventType.Information, 2080);
#endif
                            }

                            // Handle different types that might be returned
                            if (actualValue is System.Collections.IEnumerable enumerable && !(actualValue is string))
                            {
                                foreach (var item in enumerable)
                                {
                                    if (item != null)
                                    {
                                        // Unwrap PSObject items if needed
                                        object actualItem = item;
                                        if (item is PSObject psItem)
                                        {
                                            actualItem = psItem.BaseObject;
                                        }
                                        
                                        string vmName = actualItem.ToString();
                                        if (!string.IsNullOrWhiteSpace(vmName))
                                        {
                                            groupInfo.VmMembers.Add(vmName);
                                            FileLogger.Message($"Added VM member: '{vmName}' to group '{groupInfo.Name}'",
                                                FileLogger.EventType.Information, 2073);
                                        }
                                    }
                                }
                            }
                            else if (actualValue is string strValue && !string.IsNullOrWhiteSpace(strValue))
                            {
                                // Handle case where it's returned as a single string
                                groupInfo.VmMembers.Add(strValue);
                                FileLogger.Message($"Added VM member (string): '{strValue}' to group '{groupInfo.Name}'",
                                    FileLogger.EventType.Information, 2074);
                            }
                        }
                        
                        FileLogger.Message($"Total VM members parsed for group '{groupInfo.Name}': {groupInfo.VmMembers.Count}",
                            FileLogger.EventType.Information, 2075);

                        // Process Group Members array
                        var groupMembersProperty = result.Properties["GroupMembers"]?.Value;
#if DEBUG
                        FileLogger.Message($"Group Members property type for group '{groupInfo.Name}': {groupMembersProperty?.GetType().FullName ?? "null"}",
                            FileLogger.EventType.Information, 2076);
#endif
                        if (groupMembersProperty != null)
                        {
                            // Unwrap PSObject if needed
                            object actualValue = groupMembersProperty;
                            if (groupMembersProperty is PSObject psObj)
                            {
                                actualValue = psObj.BaseObject;
#if debug
                                FileLogger.Message($"Unwrapped PSObject - BaseObject type: {actualValue?.GetType().FullName ?? "null"}",
                                    FileLogger.EventType.Information, 2081);
#endif
                            }
                            
                            // Handle different types that might be returned
                            if (actualValue is System.Collections.IEnumerable groupEnum && !(actualValue is string))
                            {
                                foreach (var item in groupEnum)
                                {
                                    if (item != null)
                                    {
                                        // Unwrap PSObject items if needed
                                        object actualItem = item;
                                        if (item is PSObject psItem)
                                        {
                                            actualItem = psItem.BaseObject;
                                        }
                                        
                                        string groupName = actualItem.ToString();
                                        if (!string.IsNullOrWhiteSpace(groupName))
                                        {
                                            groupInfo.GroupMembers.Add(groupName);
                                            FileLogger.Message($"Added Group member: '{groupName}' to group '{groupInfo.Name}'",
                                                FileLogger.EventType.Information, 2077);
                                        }
                                    }
                                }
                            }
                            else if (actualValue is string strValue && !string.IsNullOrWhiteSpace(strValue))
                            {
                                // Handle case where it's returned as a single string
                                groupInfo.GroupMembers.Add(strValue);
                                FileLogger.Message($"Added Group member (string): '{strValue}' to group '{groupInfo.Name}'",
                                    FileLogger.EventType.Information, 2078);
                            }
                        }
                        
                        FileLogger.Message($"Total Group members parsed for group '{groupInfo.Name}': {groupInfo.GroupMembers.Count}",
                            FileLogger.EventType.Information, 2079);

                        // Set display name for group type
                        groupInfo.GroupTypeDisplay = groupInfo.GroupType switch
                        {
                            "VMCollectionType" => "Collection",
                            "ManagementCollectionType" => "Management",
                            _ => groupInfo.GroupType
                        };

                        vmGroups.Add(groupInfo);

                        FileLogger.Message($"Processed VM Group: '{groupInfo.Name}' ({groupInfo.GroupTypeDisplay}) - {groupInfo.VmCount} VMs",
                            FileLogger.EventType.Information, 2063);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Message($"Error processing VM Group result: {ex.Message}",
                            FileLogger.EventType.Warning, 2064);
                    }
                }

                FileLogger.Message($"Successfully retrieved {vmGroups.Count} VM Groups",
                    FileLogger.EventType.Information, 2065);

                return vmGroups;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error retrieving VM Groups: {ex.Message}",
                    FileLogger.EventType.Error, 2066);
                return new List<VmGroupInfo>();
            }
        }

        public static VmGroupDeletionResult RemoveHyperVvmGroup(
            string groupName,
            bool force,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Checking VM Group '{groupName}' before removal...",
                    FileLogger.EventType.Information, 2050);

                // Get the VM group and check if it contains VMs
                var checkScript = $@"
                    $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                    $vmMembers = $group.VMMembers
                    $vmNames = @()
                    foreach ($vm in $vmMembers) {{
                        if ($vm.Name) {{
                            $vmNames += $vm.Name
                        }}
                    }}
                    @{{ 
                        VMCount = $vmMembers.Count
                        VMNames = $vmNames
                        GroupExists = $true
                    }}
                ";

                var checkResults = executePowerShellCommand(checkScript);

                if (checkResults == null || checkResults.Count == 0)
                {
                    return new VmGroupDeletionResult
                    {
                        Success = false,
                        Error = $"VM Group '{groupName}' does not exist"
                    };
                }

                int vmCount = 0;
                List<string> vmNames = new List<string>();

                var result = checkResults[0];
                if (result.BaseObject is System.Collections.Hashtable hashtable)
                {
                    vmCount = Convert.ToInt32(hashtable["VMCount"] ?? 0);

                    if (hashtable["VMNames"] != null)
                    {
                        // Unwrap PSObject if present (common in remote connections)
                        object vmNamesValue = hashtable["VMNames"];
                        if (vmNamesValue is PSObject psObj)
                        {
                            vmNamesValue = psObj.BaseObject;
#if debug
                            FileLogger.Message($"Unwrapped PSObject for VMNames - BaseObject type: {vmNamesValue?.GetType().FullName ?? "null"}",
                                FileLogger.EventType.Debug, 2082);
#endif
                        }

                        // Handle different collection types
                        if (vmNamesValue is System.Collections.IEnumerable enumerable && !(vmNamesValue is string))
                        {
                            foreach (var item in enumerable)
                            {
                                if (item != null)
                                {
                                    // Unwrap PSObject items if needed
                                    object actualItem = item;
                                    if (item is PSObject psItem)
                                    {
                                        actualItem = psItem.BaseObject;
                                    }

                                    string vmName = actualItem.ToString();
                                    if (!string.IsNullOrWhiteSpace(vmName))
                                    {
                                        vmNames.Add(vmName);
                                        FileLogger.Message($"Extracted VM name from group check: '{vmName}'",
                                            FileLogger.EventType.Information, 2083);
                                    }
                                }
                            }
                        }
                        else if (vmNamesValue is string strValue && !string.IsNullOrWhiteSpace(strValue))
                        {
                            // Handle single string value
                            vmNames.Add(strValue);
                            FileLogger.Message($"Extracted single VM name from group check: '{strValue}'",
                                FileLogger.EventType.Information, 2084);
                        }
                    }
                }

                FileLogger.Message($"VM Group '{groupName}' contains {vmCount} VM(s): {string.Join(", ", vmNames)}",
                    FileLogger.EventType.Information, 2051);

                // If group contains VMs and not forcing, return error
                if (vmCount > 0 && !force)
                {
                    FileLogger.Message($"VM Group '{groupName}' cannot be deleted without force - contains {vmCount} VM(s)",
                        FileLogger.EventType.Warning, 2052);

                    return new VmGroupDeletionResult
                    {
                        Success = false,
                        Error = $"Cannot delete VM Group '{groupName}' because it contains {vmCount} VM(s). Use Force to delete anyway.",
                        CanForce = true,
                        VmCount = vmCount,
                        VmNames = vmNames
                    };
                }

                // If force deletion and group has VMs, remove VMs from group first
                if (force && vmCount > 0)
                {
                    FileLogger.Message($"Force deletion - removing {vmCount} VM(s) from group '{groupName}' first...",
                        FileLogger.EventType.Information, 2072);

                    foreach (var vmName in vmNames)
                    {
                        try
                        {
                            FileLogger.Message($"Removing VM '{vmName}' from group '{groupName}'...",
                                FileLogger.EventType.Information, 2073);

                            // Use correct Remove-VMGroupMember format
                            var removeVmScript = $@"
                                $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                                $vm = Get-VM -Name '{vmName}' -ErrorAction Stop
                                Remove-VMGroupMember -VMGroup $group -VM $vm -ErrorAction Stop
                            ";

                            var removeVmResult = executePowerShellCommand(removeVmScript);

                            FileLogger.Message($"Successfully removed VM '{vmName}' from group '{groupName}'",
                                FileLogger.EventType.Information, 2074);
                        }
                        catch (Exception ex)
                        {
                            FileLogger.Message($"Error removing VM '{vmName}' from group '{groupName}': {ex.Message}",
                                FileLogger.EventType.Error, 2076);
                        }
                    }

                    FileLogger.Message($"Finished removing VMs from group '{groupName}'",
                        FileLogger.EventType.Information, 2077);
                }

                // Remove the VM group (it should now be empty)
                FileLogger.Message($"Deleting VM Group '{groupName}'...",
                    FileLogger.EventType.Information, 2078);

                var removeGroupScript = $"Remove-VMGroup -Name '{groupName}' -Force -ErrorAction Stop";
                var removeGroupResult = executePowerShellCommand(removeGroupScript);

                if (removeGroupResult == null)
                {
                    return new VmGroupDeletionResult
                    {
                        Success = false,
                        Error = "Failed to remove VM Group. Check logs for details."
                    };
                }

                FileLogger.Message($"VM Group '{groupName}' removed successfully",
                    FileLogger.EventType.Information, 2054);

                return new VmGroupDeletionResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception removing VM Group '{groupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2055);

                return new VmGroupDeletionResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public static void RefreshVmGroupsView(
            string reason,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand,
            Action<List<VmGroupInfo>> updateDataGridView)
        {
            try
            {
                FileLogger.Message($"Refreshing VM Groups view - Reason: {reason}",
                    FileLogger.EventType.Information, 2079);

                // Get fresh VM Groups data
                var vmGroups = GetHyperVvmGroups(executePowerShellCommand);

                if (vmGroups != null && updateDataGridView != null)
                {
                    FileLogger.Message($"VM Groups view refreshed with {vmGroups.Count} groups",
                        FileLogger.EventType.Information, 2080);
                    
                    // Update the DataGridView
                    updateDataGridView(vmGroups);
                }
                else
                {
                    FileLogger.Message("DataGridView update callback not available",
                        FileLogger.EventType.Information, 2081);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to refresh VM Groups view: {ex.Message}",
                    FileLogger.EventType.Warning, 2082);
            }
        }

        public static VmGroupCreationResult CreateHyperVvmGroup(
            string groupName,
            string groupType,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Executing New-VMGroup command for '{groupName}'...",
                    FileLogger.EventType.Information, 2035);

                // Build PowerShell command
                string command = $"New-VMGroup -Name '{groupName}' -GroupType {groupType}";

                var results = executePowerShellCommand(command);

                if (results != null && results.Count > 0)
                {
                    FileLogger.Message($"VM Group '{groupName}' created successfully via PowerShell",
                        FileLogger.EventType.Information, 2036);

                    return new VmGroupCreationResult
                    {
                        Success = true
                    };
                }
                else
                {
                    string error = "No results returned from New-VMGroup command";
                    FileLogger.Message($"VM Group creation returned no results: {error}",
                        FileLogger.EventType.Warning, 2037);

                    return new VmGroupCreationResult
                    {
                        Success = false,
                        Error = error
                    };
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception creating VM Group: {ex.Message}",
                    FileLogger.EventType.Error, 2038);

                return new VmGroupCreationResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public static VmGroupRenameResult RenameHyperVvmGroup(
            string oldGroupName,
            string newGroupName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Renaming VM Group from '{oldGroupName}' to '{newGroupName}'...",
                    FileLogger.EventType.Information, 2090);

                // Build PowerShell command
                string command = $"Rename-VMGroup -Name '{oldGroupName}' -NewName '{newGroupName}' -ErrorAction Stop";

                var results = executePowerShellCommand(command);

                if (results == null)
                {
                    string error = "Failed to rename VM Group. Check logs for details.";
                    FileLogger.Message($"VM Group rename failed: {error}",
                        FileLogger.EventType.Error, 2091);

                    return new VmGroupRenameResult
                    {
                        Success = false,
                        Error = error
                    };
                }

                FileLogger.Message($"VM Group renamed successfully from '{oldGroupName}' to '{newGroupName}'",
                    FileLogger.EventType.Information, 2092);

                return new VmGroupRenameResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception renaming VM Group '{oldGroupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2093);

                return new VmGroupRenameResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public static VmGroupMemberResult AddVmToGroup(
            string vmName,
            string groupName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Adding VM '{vmName}' to group '{groupName}'...",
                    FileLogger.EventType.Information, 2117);

                // Build PowerShell command
                string command = $@"
                    $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                    $vm = Get-VM -Name '{vmName}' -ErrorAction Stop
                    Add-VMGroupMember -VMGroup $group -VM $vm -ErrorAction Stop
                ";

                var results = executePowerShellCommand(command);

                // Add-VMGroupMember doesn't return output, so if we get here without exception, it succeeded
                FileLogger.Message($"Successfully added VM '{vmName}' to group '{groupName}'",
                    FileLogger.EventType.Information, 2119);

                return new VmGroupMemberResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception adding VM '{vmName}' to group '{groupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2120);

                return new VmGroupMemberResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public static VmGroupMemberResult RemoveVmFromGroup(
            string vmName,
            string groupName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Removing VM '{vmName}' from group '{groupName}'...",
                    FileLogger.EventType.Information, 2121);

                // Build PowerShell command
                string command = $@"
                    $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                    $vm = Get-VM -Name '{vmName}' -ErrorAction Stop
                    Remove-VMGroupMember -VMGroup $group -VM $vm -ErrorAction Stop
                ";

                var results = executePowerShellCommand(command);

                // Remove-VMGroupMember doesn't return output, so if we get here without exception, it succeeded
                FileLogger.Message($"Successfully removed VM '{vmName}' from group '{groupName}'",
                    FileLogger.EventType.Information, 2123);

                return new VmGroupMemberResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception removing VM '{vmName}' from group '{groupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2124);

                return new VmGroupMemberResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets the VM groups that a specific VM belongs to
        /// </summary>
        /// <param name="vmName">The name of the VM</param>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <returns>Comma-separated list of group names, or "N/A" if not in any groups</returns>
        public static string GetVmGroups(string vmName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>?> executePowerShellCommand)
        {
            try
            {
                var results = executePowerShellCommand($"Get-VMGroup | Where-Object {{ $_.VMMembers.Name -contains '{vmName}' }} | Select-Object -ExpandProperty Name");

                if (results != null && results.Count > 0)
                {
                    return string.Join(", ", results.Select(g => g.ToString()));
                }

                return "N/A";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Exports VM data and VM Groups to a JSON file
        /// </summary>
        /// <param name="filePath">The file path to export to</param>
        /// <param name="vmGroups">The VM groups to export</param>
        /// <param name="vmData">The VM data to export</param>
        /// <param name="vmCount">Total VM count</param>
        /// <returns>True if export was successful, false otherwise</returns>
        public static bool ExportVmGroupsToJson(string filePath, List<VmGroupInfo> vmGroups,
            List<Dictionary<string, string>> vmData, int vmCount)
        {
            try
            {
                FileLogger.Message("Exporting as JSON format",
                    FileLogger.EventType.Information, 2108);

                var exportData = new
                {
                    ExportInfo = new
                    {
                        ExportDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        ExportedBy = Environment.UserName,
                        HyperVHost = SessionContext.ServerName,
                        ConnectionType = SessionContext.IsLocal ? "Local" : "Remote",
                        TotalVMs = vmCount,
                        ApplicationVersion = Globals.ToolName.FullName + " " + Globals.ToolProperties.ToolVersion
                    },
                    VMData = vmData,
                    VMGroups = vmGroups
                };

                string jsonData = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, jsonData, System.Text.Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error exporting to JSON: {ex.Message}",
                    FileLogger.EventType.Error, 2109);
                return false;
            }
        }
    }
}
