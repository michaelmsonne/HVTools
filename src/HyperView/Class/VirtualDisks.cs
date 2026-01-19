using System.Management.Automation;

namespace HyperView.Class
{
    /// <summary>
    /// Represents detailed information about a virtual disk (VHD/VHDX)
    /// </summary>
    public class VirtualDiskInfo
    {
        public string VMName { get; set; } = "";
        public string VMState { get; set; } = "";
        public string VMId { get; set; } = "";
        public string VMGeneration { get; set; } = "";
        public string VMNotes { get; set; } = "";
        public string DiskName { get; set; } = "";
        public string DiskPath { get; set; } = "";
        public string DiskType { get; set; } = ""; // Dynamic, Fixed, Differencing
        public string DiskFormat { get; set; } = ""; // VHD, VHDX
        public double MaxSizeGB { get; set; }
        public double FileSizeGB { get; set; }
        public double UsedSpaceGB { get; set; }
        public string FragmentationPercent { get; set; } = "";
        public int PhysicalSectorSizeBytes { get; set; }
        public int LogicalSectorSizeBytes { get; set; }
        public int BlockSizeBytes { get; set; }
        public string ControllerType { get; set; } = ""; // IDE, SCSI
        public int ControllerNumber { get; set; }
        public int ControllerLocation { get; set; }
        public string AttachmentType { get; set; } = ""; // VHD, Physical, None
        public bool IsShared { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsClustered { get; set; }
        public string SupportPersistentReservations { get; set; } = "";
        public string QoSPolicyId { get; set; } = "";
        public string QoSMinimumIOPS { get; set; } = "";
        public string QoSMaximumIOPS { get; set; } = "";
        public string ParentPath { get; set; } = "";
        public string DiskIdentifier { get; set; } = "";
        public string ClusterName { get; set; } = "";
        public string CurrentHost { get; set; } = "";
        public string HostCluster { get; set; } = "";
        public string VMPath { get; set; } = "";
        public string ConfigurationLocation { get; set; } = "";
        public string SnapshotFileLocation { get; set; } = "";
        public string SmartPagingFilePath { get; set; } = "";
        public string GuestOSType { get; set; } = "";
    }

    /// <summary>
    /// Provides functionality to retrieve virtual disk information
    /// </summary>
    public static class VirtualDisks
    {
        /// <summary>
        /// Gets detailed information about all virtual disks from all VMs
        /// </summary>
        public static List<VirtualDiskInfo> GetVirtualDiskDetails(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand,
            Func<string, string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommandOnNode = null)
        {
            var allDisks = new List<VirtualDiskInfo>();

            try
            {
                FileLogger.Message("Getting virtual disk details...",
                    FileLogger.EventType.Information, 5001);

                // Check if connected to a cluster
                bool isCluster = SessionContext.IsCluster;
                var clusterNodes = new List<string>();

                if (isCluster && !SessionContext.IsLocal)
                {
                    // Get cluster nodes
                    FileLogger.Message("Detected cluster environment, getting cluster nodes for disk enumeration...",
                        FileLogger.EventType.Information, 5002);

                    string getNodesScript = @"Get-ClusterNode -ErrorAction Stop | Select-Object -ExpandProperty Name";
                    var nodesResult = executePowerShellCommand(getNodesScript);

                    if (nodesResult != null && nodesResult.Count > 0)
                    {
                        foreach (var nodeObj in nodesResult)
                        {
                            string nodeName = nodeObj.BaseObject?.ToString();
                            if (!string.IsNullOrEmpty(nodeName))
                            {
                                // If the original connection used FQDN, construct FQDNs for cluster nodes
                                if (SessionContext.ServerName.Contains('.') && !nodeName.Contains('.'))
                                {
                                    string domain = SessionContext.ServerName.Substring(SessionContext.ServerName.IndexOf('.'));
                                    nodeName = nodeName + domain;
                                }
                                clusterNodes.Add(nodeName);
                            }
                        }

                        FileLogger.Message($"Found {clusterNodes.Count} cluster nodes for disk enumeration: {string.Join(", ", clusterNodes)}",
                            FileLogger.EventType.Information, 5003);
                    }
                }

                if (clusterNodes.Count > 0 && executePowerShellCommandOnNode != null)
                {
                    // Get disk details from all cluster nodes
                    int nodeIndex = 0;
                    foreach (var node in clusterNodes)
                    {
                        nodeIndex++;
                        try
                        {
                            FileLogger.Message($"Getting disk details from cluster node {nodeIndex} of {clusterNodes.Count}: {node}",
                                FileLogger.EventType.Information, 5004);

                            var nodeDisks = GetVirtualDisksFromNode(node, executePowerShellCommandOnNode);
                            if (nodeDisks != null && nodeDisks.Count > 0)
                            {
                                allDisks.AddRange(nodeDisks);
                                FileLogger.Message($"Successfully retrieved {nodeDisks.Count} disk(s) from node: {node}",
                                    FileLogger.EventType.Information, 5005);
                            }
                        }
                        catch (Exception ex)
                        {
                            FileLogger.Message($"Failed to get disk details from cluster node {node}: {ex.Message}",
                                FileLogger.EventType.Warning, 5006);
                        }
                    }
                }
                else
                {
                    // Single host
                    FileLogger.Message("Getting disk details from single host...",
                        FileLogger.EventType.Information, 5007);

                    var disks = GetVirtualDisksFromSession(executePowerShellCommand);
                    if (disks != null && disks.Count > 0)
                    {
                        allDisks.AddRange(disks);
                    }
                }

                FileLogger.Message($"Successfully retrieved details for {allDisks.Count} virtual disk(s)",
                    FileLogger.EventType.Information, 5008);

                return allDisks;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting virtual disk details: {ex.Message}",
                    FileLogger.EventType.Error, 5009);
                return allDisks;
            }
        }

        /// <summary>
        /// Gets virtual disk details from a specific cluster node
        /// </summary>
        private static List<VirtualDiskInfo> GetVirtualDisksFromNode(string nodeName,
            Func<string, string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommandOnNode)
        {
            try
            {
                string script = GetVirtualDiskScript();

                FileLogger.Message($"Executing virtual disk script on node '{nodeName}'...",
                    FileLogger.EventType.Information, 5010);

                var result = executePowerShellCommandOnNode(nodeName, script);

                if (result == null || result.Count == 0)
                {
                    FileLogger.Message($"Virtual disk script returned no results for node '{nodeName}'",
                        FileLogger.EventType.Warning, 5011);
                    return new List<VirtualDiskInfo>();
                }

                FileLogger.Message($"Virtual disk script for node '{nodeName}' returned {result.Count} result(s), parsing...",
                    FileLogger.EventType.Information, 5012);

                var disks = ParseVirtualDiskDetails(result);

                FileLogger.Message($"Successfully parsed {disks.Count} virtual disk(s) from node '{nodeName}'",
                    FileLogger.EventType.Information, 5013);

                return disks;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting virtual disks from node '{nodeName}': {ex.Message}",
                    FileLogger.EventType.Error, 5014);
                return new List<VirtualDiskInfo>();
            }
        }

        /// <summary>
        /// Gets virtual disk details from the current session
        /// </summary>
        private static List<VirtualDiskInfo> GetVirtualDisksFromSession(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                string script = GetVirtualDiskScript();

                FileLogger.Message("Executing virtual disk script...",
                    FileLogger.EventType.Information, 5015);

                var result = executePowerShellCommand(script);

                if (result == null || result.Count == 0)
                {
                    FileLogger.Message("Virtual disk script returned no results",
                        FileLogger.EventType.Warning, 5016);
                    return new List<VirtualDiskInfo>();
                }

                FileLogger.Message($"Virtual disk script returned {result.Count} result(s), parsing...",
                    FileLogger.EventType.Information, 5017);

                var disks = ParseVirtualDiskDetails(result);

                FileLogger.Message($"Successfully parsed {disks.Count} virtual disk(s)",
                    FileLogger.EventType.Information, 5018);

                return disks;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in GetVirtualDisksFromSession: {ex.Message}",
                    FileLogger.EventType.Error, 5019);
                return new List<VirtualDiskInfo>();
            }
        }

        /// <summary>
        /// Gets the PowerShell script to retrieve virtual disk details
        /// </summary>
        private static string GetVirtualDiskScript()
        {
            return @"
                $ErrorActionPreference = 'SilentlyContinue'
                try {
                    # Get cluster name if clustered
                    $clusterName = 'N/A'
                    $isClustered = $false
                    try {
                        $cluster = Get-Cluster -ErrorAction SilentlyContinue
                        if ($cluster) {
                            $clusterName = $cluster.Name
                            $isClustered = $true
                        }
                    } catch { }

                    # Get all VMs
                    $vms = Get-VM -ErrorAction SilentlyContinue

                    $diskDetails = @()

                    foreach ($vm in $vms) {
                        try {
                            # Get VM details
                            $vmName = $vm.Name
                            $vmState = $vm.State.ToString()
                            $vmId = $vm.Id.ToString()
                            $vmGeneration = $vm.Generation.ToString()
                            $vmNotes = if ($vm.Notes) { $vm.Notes } else { '' }
                            $vmPath = $vm.Path
                            $configLocation = $vm.ConfigurationLocation
                            $snapshotLocation = $vm.SnapshotFileLocation
                            $smartPagingPath = $vm.SmartPagingFilePath
                            $currentHost = $env:COMPUTERNAME

                            # Get all hard drives
                            $hardDrives = Get-VMHardDiskDrive -VMName $vmName -ErrorAction SilentlyContinue

                            foreach ($drive in $hardDrives) {
                                try {
                                    $diskInfo = [PSCustomObject]@{
                                        VMName = $vmName
                                        VMState = $vmState
                                        VMId = $vmId
                                        VMGeneration = $vmGeneration
                                        VMNotes = $vmNotes
                                        DiskName = [System.IO.Path]::GetFileName($drive.Path)
                                        DiskPath = $drive.Path
                                        DiskType = ''
                                        DiskFormat = ''
                                        MaxSizeGB = 0
                                        FileSizeGB = 0
                                        UsedSpaceGB = 0
                                        FragmentationPercent = 'N/A'
                                        PhysicalSectorSizeBytes = 0
                                        LogicalSectorSizeBytes = 0
                                        BlockSizeBytes = 0
                                        ControllerType = $drive.ControllerType.ToString()
                                        ControllerNumber = $drive.ControllerNumber
                                        ControllerLocation = $drive.ControllerLocation
                                        AttachmentType = 'VHD'
                                        IsShared = $false
                                        IsReadOnly = $false
                                        IsClustered = $isClustered
                                        SupportPersistentReservations = 'No'
                                        QoSPolicyId = ''
                                        QoSMinimumIOPS = ''
                                        QoSMaximumIOPS = ''
                                        ParentPath = ''
                                        DiskIdentifier = ''
                                        ClusterName = $clusterName
                                        CurrentHost = $currentHost
                                        HostCluster = $clusterName
                                        VMPath = $vmPath
                                        ConfigurationLocation = $configLocation
                                        SnapshotFileLocation = $snapshotLocation
                                        SmartPagingFilePath = $smartPagingPath
                                        GuestOSType = $vm.GuestStateIsolationType.ToString()
                                    }

                                    # Get VHD details if path exists
                                    if ($drive.Path -and (Test-Path $drive.Path -ErrorAction SilentlyContinue)) {
                                        try {
                                            $vhd = Get-VHD -Path $drive.Path -ErrorAction SilentlyContinue
                                            if ($vhd) {
                                                $diskInfo.DiskType = $vhd.VhdType.ToString()
                                                $diskInfo.DiskFormat = $vhd.VhdFormat.ToString()
                                                $diskInfo.MaxSizeGB = [Math]::Round($vhd.Size / 1GB, 2)
                                                $diskInfo.FileSizeGB = [Math]::Round($vhd.FileSize / 1GB, 2)
                                                $diskInfo.PhysicalSectorSizeBytes = $vhd.PhysicalSectorSize
                                                $diskInfo.LogicalSectorSizeBytes = $vhd.LogicalSectorSize
                                                $diskInfo.BlockSizeBytes = $vhd.BlockSize
                                                $diskInfo.ParentPath = if ($vhd.ParentPath) { $vhd.ParentPath } else { '' }
                                                $diskInfo.DiskIdentifier = if ($vhd.DiskIdentifier) { $vhd.DiskIdentifier } else { '' }
                                                $diskInfo.FragmentationPercent = if ($vhd.FragmentationPercentage) { ""$($vhd.FragmentationPercentage)%"" } else { 'N/A' }
                                            }
                                        } catch { }
                                    }

                                    # Check if shared
                                    try {
                                        if ($drive.SupportPersistentReservations) {
                                            $diskInfo.IsShared = $true
                                            $diskInfo.SupportPersistentReservations = 'Yes'
                                        }
                                    } catch { }

                                    # Get QoS settings
                                    try {
                                        $qos = Get-VMHardDiskDrive -VMName $vmName -ControllerType $drive.ControllerType -ControllerNumber $drive.ControllerNumber -ControllerLocation $drive.ControllerLocation -ErrorAction SilentlyContinue
                                        if ($qos) {
                                            if ($qos.MinimumIOPS) { $diskInfo.QoSMinimumIOPS = $qos.MinimumIOPS.ToString() }
                                            if ($qos.MaximumIOPS) { $diskInfo.QoSMaximumIOPS = $qos.MaximumIOPS.ToString() }
                                        }
                                    } catch { }

                                    $diskDetails += $diskInfo
                                } catch {
                                    # Error processing specific drive - continue
                                }
                            }

                            # Get physical/passthrough disks
                            try {
                                $physicalDisks = Get-VMHardDiskDrive -VMName $vmName -ErrorAction SilentlyContinue | Where-Object { $_.Path -like 'Disk*' }
                                foreach ($pDisk in $physicalDisks) {
                                    $diskInfo = [PSCustomObject]@{
                                        VMName = $vmName
                                        VMState = $vmState
                                        VMId = $vmId
                                        VMGeneration = $vmGeneration
                                        VMNotes = $vmNotes
                                        DiskName = 'Physical Disk'
                                        DiskPath = $pDisk.Path
                                        DiskType = 'PassThrough'
                                        DiskFormat = 'Physical'
                                        MaxSizeGB = 0
                                        FileSizeGB = 0
                                        UsedSpaceGB = 0
                                        FragmentationPercent = 'N/A'
                                        PhysicalSectorSizeBytes = 0
                                        LogicalSectorSizeBytes = 0
                                        BlockSizeBytes = 0
                                        ControllerType = $pDisk.ControllerType.ToString()
                                        ControllerNumber = $pDisk.ControllerNumber
                                        ControllerLocation = $pDisk.ControllerLocation
                                        AttachmentType = 'Physical'
                                        IsShared = $false
                                        IsReadOnly = $false
                                        IsClustered = $isClustered
                                        SupportPersistentReservations = 'No'
                                        QoSPolicyId = ''
                                        QoSMinimumIOPS = ''
                                        QoSMaximumIOPS = ''
                                        ParentPath = ''
                                        DiskIdentifier = $pDisk.Path
                                        ClusterName = $clusterName
                                        CurrentHost = $currentHost
                                        HostCluster = $clusterName
                                        VMPath = $vmPath
                                        ConfigurationLocation = $configLocation
                                        SnapshotFileLocation = $snapshotLocation
                                        SmartPagingFilePath = $smartPagingPath
                                        GuestOSType = $vm.GuestStateIsolationType.ToString()
                                    }
                                    $diskDetails += $diskInfo
                                }
                            } catch { }

                        } catch {
                            # Error processing VM - continue with next
                        }
                    }

                    # Return all disk details
                    return $diskDetails
                } catch {
                    # Return empty array on error
                    return @()
                }
            ";
        }

        /// <summary>
        /// Parses PSObject collection into VirtualDiskInfo list
        /// </summary>
        private static List<VirtualDiskInfo> ParseVirtualDiskDetails(System.Collections.ObjectModel.Collection<PSObject> psObjects)
        {
            var disks = new List<VirtualDiskInfo>();

            try
            {
                foreach (var psObject in psObjects)
                {
                    try
                    {
                        var disk = new VirtualDiskInfo
                        {
                            VMName = GetStringProperty(psObject, "VMName"),
                            VMState = GetStringProperty(psObject, "VMState"),
                            VMId = GetStringProperty(psObject, "VMId"),
                            VMGeneration = GetStringProperty(psObject, "VMGeneration"),
                            VMNotes = GetStringProperty(psObject, "VMNotes"),
                            DiskName = GetStringProperty(psObject, "DiskName"),
                            DiskPath = GetStringProperty(psObject, "DiskPath"),
                            DiskType = GetStringProperty(psObject, "DiskType"),
                            DiskFormat = GetStringProperty(psObject, "DiskFormat"),
                            MaxSizeGB = GetDoubleProperty(psObject, "MaxSizeGB"),
                            FileSizeGB = GetDoubleProperty(psObject, "FileSizeGB"),
                            UsedSpaceGB = GetDoubleProperty(psObject, "UsedSpaceGB"),
                            FragmentationPercent = GetStringProperty(psObject, "FragmentationPercent"),
                            PhysicalSectorSizeBytes = GetIntProperty(psObject, "PhysicalSectorSizeBytes"),
                            LogicalSectorSizeBytes = GetIntProperty(psObject, "LogicalSectorSizeBytes"),
                            BlockSizeBytes = GetIntProperty(psObject, "BlockSizeBytes"),
                            ControllerType = GetStringProperty(psObject, "ControllerType"),
                            ControllerNumber = GetIntProperty(psObject, "ControllerNumber"),
                            ControllerLocation = GetIntProperty(psObject, "ControllerLocation"),
                            AttachmentType = GetStringProperty(psObject, "AttachmentType"),
                            IsShared = GetBoolProperty(psObject, "IsShared"),
                            IsReadOnly = GetBoolProperty(psObject, "IsReadOnly"),
                            IsClustered = GetBoolProperty(psObject, "IsClustered"),
                            SupportPersistentReservations = GetStringProperty(psObject, "SupportPersistentReservations"),
                            QoSPolicyId = GetStringProperty(psObject, "QoSPolicyId"),
                            QoSMinimumIOPS = GetStringProperty(psObject, "QoSMinimumIOPS"),
                            QoSMaximumIOPS = GetStringProperty(psObject, "QoSMaximumIOPS"),
                            ParentPath = GetStringProperty(psObject, "ParentPath"),
                            DiskIdentifier = GetStringProperty(psObject, "DiskIdentifier"),
                            ClusterName = GetStringProperty(psObject, "ClusterName"),
                            CurrentHost = GetStringProperty(psObject, "CurrentHost"),
                            HostCluster = GetStringProperty(psObject, "HostCluster"),
                            VMPath = GetStringProperty(psObject, "VMPath"),
                            ConfigurationLocation = GetStringProperty(psObject, "ConfigurationLocation"),
                            SnapshotFileLocation = GetStringProperty(psObject, "SnapshotFileLocation"),
                            SmartPagingFilePath = GetStringProperty(psObject, "SmartPagingFilePath"),
                            GuestOSType = GetStringProperty(psObject, "GuestOSType")
                        };

                        disks.Add(disk);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Message($"Error parsing virtual disk object: {ex.Message}",
                            FileLogger.EventType.Warning, 5020);
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in ParseVirtualDiskDetails: {ex.Message}",
                    FileLogger.EventType.Error, 5021);
            }

            return disks;
        }

        private static string GetStringProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                return prop?.Value?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static int GetIntProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return Convert.ToInt32(prop.Value);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetDoubleProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return Convert.ToDouble(prop.Value);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static bool GetBoolProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return Convert.ToBoolean(prop.Value);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
