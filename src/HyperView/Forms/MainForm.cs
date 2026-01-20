using HyperView.Class;
using HyperView.Forms;
using System.Data;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using static HyperView.Class.FileLogger;

namespace HyperView
{
    public partial class MainForm : Form
    {
        private PSObject _psSession = null;
        private Runspace _persistentRunspace = null;

        public MainForm()
        {
            InitializeComponent();
            InitializeSession();
            LoadVMOverview();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set initial status (make dynamic based on context if needed when shown later)
            toolStripStatusLabelTextMainForm.Text = "Ready";
            // Any additional initialization on form load
        }

        /// <summary>
        /// Handles the DataBindingComplete event to apply color coding after data is fully bound
        /// </summary>
        private void DatagridviewVMOverView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Only apply color coding if binding was successful
            if (e.ListChangedType == System.ComponentModel.ListChangedType.Reset)
            {
                ApplyColorCoding();
            }
        }

        private void InitializeSession()
        {
            try
            {
                // Check if we have an active session
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active session found. Please login again.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                // For remote connections, create a persistent runspace and PS session
                if (!SessionContext.IsLocal)
                {
                    // Create persistent runspace for remote operations
                    _persistentRunspace = RunspaceFactory.CreateRunspace();
                    _persistentRunspace.Open();

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _persistentRunspace;

                        // Build New-PSSession command
                        ps.AddCommand("New-PSSession")
                          .AddParameter("ComputerName", SessionContext.ServerName)
                          .AddParameter("ErrorAction", "Stop");

                        if (SessionContext.Credentials != null)
                        {
                            ps.AddParameter("Credential", SessionContext.Credentials);
                        }

                        var sessionResult = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var error = ps.Streams.Error[0];
                            MessageBox.Show($"Failed to create PowerShell session: {error.Exception.Message}",
                                "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.DialogResult = DialogResult.Cancel;
                            this.Close();
                            return;
                        }

                        if (sessionResult != null && sessionResult.Count > 0)
                        {
                            _psSession = sessionResult[0];
                            Message($"Remote PowerShell session created for '{SessionContext.ServerName}'",
                                EventType.Information, 2002);
                        }
                    }
                }

                // Update form title with connection info
                this.Text = $"{Globals.ToolName.ShortName} - Connected to {SessionContext.ServerName} ({SessionContext.ConnectionType})";
            }
            catch (Exception ex)
            {
                Message($"Failed to initialize session: {ex.Message}",
                    EventType.Error, 2003);
                MessageBox.Show($"Failed to initialize session: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Only perform cleanup if the form is actually closing (not cancelled by user)
            if (e.Cancel)
            {
                Message("Form closing cancelled - skipping cleanup",
                    EventType.Information, 2014);
                return;
            }

            // Clean up remote session if exists
            if (_psSession != null && _persistentRunspace != null)
            {
                try
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _persistentRunspace;
                        ps.AddCommand("Remove-PSSession")
                          .AddParameter("Session", _psSession);
                        ps.Invoke();
                    }
                    Message("Remote PowerShell session closed",
                        EventType.Information, 2004);
                }
                catch (Exception ex)
                {
                    Message($"Error closing PS session: {ex.Message}",
                        EventType.Warning, 2005);
                }
            }

            // Dispose persistent runspace
            if (_persistentRunspace != null)
            {
                try
                {
                    _persistentRunspace.Close();
                    _persistentRunspace.Dispose();
                    Message("Persistent runspace closed",
                        EventType.Information, 2009);
                }
                catch (Exception ex)
                {
                    Message($"Error closing persistent runspace: {ex.Message}",
                        EventType.Warning, 2010);
                }
            }
        }

        /// <summary>
        /// Executes a long-running operation with a progress form
        /// </summary>
        /// <param name="operation">The operation to execute in background</param>
        /// <param name="operationName">Name of the operation for logging</param>
        private async void ExecuteWithProgressForm(Action operation, string operationName)
        {
            ValidationProgressForm progressForm = null;

            try
            {
                // Show progress form
                progressForm = new ValidationProgressForm();
                progressForm.StartPosition = FormStartPosition.CenterScreen;
                progressForm.Show(this);
                progressForm.Refresh();
                Application.DoEvents();

                Message($"Starting background operation: {operationName}", EventType.Information, 6001);

                Exception taskException = null;

                // Run operation in background task
                await Task.Run(() =>
                {
                    try
                    {
                        operation();
                    }
                    catch (Exception ex)
                    {
                        taskException = ex;
                        Message($"Error in background operation '{operationName}': {ex.Message}",
                            EventType.Error, 6002);
                    }
                });

                // Close progress form on UI thread (we're back on UI thread after await)
                try
                {
                    if (progressForm != null && !progressForm.IsDisposed)
                    {
                        progressForm.Close();
                        progressForm.Dispose();
                        progressForm = null;
                    }

                    if (taskException != null)
                    {
                        MessageBox.Show($"Error during {operationName}:\n\n{taskException.Message}",
                            "Operation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    Message($"Error closing progress form: {ex.Message}",
                        EventType.Warning, 6003);
                }
            }
            catch (Exception ex)
            {
                Message($"Error setting up progress form for '{operationName}': {ex.Message}",
                    EventType.Error, 6005);

                // Clean up progress form if there was an error
                if (progressForm != null && !progressForm.IsDisposed)
                {
                    try
                    {
                        progressForm.Close();
                        progressForm.Dispose();
                    }
                    catch { }
                }

                MessageBox.Show($"Error during {operationName}:\n\n{ex.Message}",
                    "Operation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Executes a long-running operation with a progress form and returns a result
        /// </summary>
        /// <typeparam name="T">The type of result to return</typeparam>
        /// <param name="operation">The operation to execute in background</param>
        /// <param name="onComplete">Action to execute with the result on UI thread</param>
        /// <param name="operationName">Name of the operation for logging</param>
        private async void ExecuteWithProgressForm<T>(Func<T> operation, Action<T> onComplete, string operationName)
        {
            ValidationProgressForm progressForm = null;

            try
            {
                // Show progress form
                progressForm = new ValidationProgressForm();
                progressForm.StartPosition = FormStartPosition.CenterScreen;
                progressForm.Show(this);
                progressForm.Refresh();
                Application.DoEvents();

                Message($"Starting background operation: {operationName}", EventType.Information, 6006);

                T result = default(T);
                Exception taskException = null;

                // Run operation in background task
                await Task.Run(() =>
                {
                    try
                    {
                        result = operation();
                    }
                    catch (Exception ex)
                    {
                        taskException = ex;
                        Message($"Error in background operation '{operationName}': {ex.Message}",
                            EventType.Error, 6007);
                    }
                });

                // Execute completion action on UI thread (we're back on UI thread after await)
                try
                {
                    if (progressForm != null && !progressForm.IsDisposed)
                    {
                        progressForm.Close();
                        progressForm.Dispose();
                        progressForm = null;
                    }

                    if (taskException != null)
                    {
                        MessageBox.Show($"Error during {operationName}:\n\n{taskException.Message}",
                            "Operation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else if (onComplete != null)
                    {
                        onComplete(result);
                    }
                }
                catch (Exception ex)
                {
                    Message($"Error in completion handler for '{operationName}': {ex.Message}",
                        EventType.Error, 6008);
                }
            }
            catch (Exception ex)
            {
                Message($"Error setting up progress form for '{operationName}': {ex.Message}",
                    EventType.Error, 6010);

                // Clean up progress form if there was an error
                if (progressForm != null && !progressForm.IsDisposed)
                {
                    try
                    {
                        progressForm.Close();
                        progressForm.Dispose();
                    }
                    catch { }
                }

                MessageBox.Show($"Error during {operationName}:\n\n{ex.Message}",
                    "Operation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadVMOverview()
        {
            try
            {
                Message($"Loading VM overview from '{SessionContext.ServerName}' (IsCluster: {SessionContext.IsCluster}, IsLocal: {SessionContext.IsLocal})",
                    EventType.Information, 2170);

                // Clear existing data
                datagridviewVMOverView.DataSource = null;
                datagridviewVMOverView.Rows.Clear();
                datagridviewVMOverView.Columns.Clear();

                System.Collections.ObjectModel.Collection<PSObject> results;
                bool isClusterCollection = false;

                // Check if connected to a cluster - use node iteration approach
                if (SessionContext.IsCluster && !SessionContext.IsLocal)
                {
                    Message("Using cluster node iteration to retrieve VMs from all nodes...",
                        EventType.Information, 2171);

                    results = GetClusterVMs();
                    isClusterCollection = true;
                }
                else
                {
                    // Standard VM retrieval for standalone or local
                    results = ExecutePowerShellCommand("Get-VM");
                }

                if (results == null || results.Count == 0)
                {
                    MessageBox.Show("No VMs found.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Message($"Retrieved {results.Count} VMs, processing...",
                    EventType.Information, 2172);

                // Create DataTable with enhanced columns
                var dataTable = new DataTable();
                dataTable.Columns.Add("VM Name");
                dataTable.Columns.Add("VM Id");
                dataTable.Columns.Add("State");
                dataTable.Columns.Add("Enabled");
                dataTable.Columns.Add("CPU Count");
                dataTable.Columns.Add("CPU Usage %");
                dataTable.Columns.Add("Memory Assigned (MB)");
                dataTable.Columns.Add("Memory Demand (MB)");
                dataTable.Columns.Add("Memory Startup (MB)");
                dataTable.Columns.Add("Dynamic Memory");
                dataTable.Columns.Add("Total Disk (GB)");
                dataTable.Columns.Add("Network Adapters");
                dataTable.Columns.Add("Generation");
                dataTable.Columns.Add("Uptime");
                dataTable.Columns.Add("Heartbeat");
                dataTable.Columns.Add("Integration Services");
                dataTable.Columns.Add("Auto Start");
                dataTable.Columns.Add("Auto Stop");
                dataTable.Columns.Add("VM Groups");
                dataTable.Columns.Add("Checkpoint Type");
                dataTable.Columns.Add("Checkpoints");
                dataTable.Columns.Add("Replication");
                dataTable.Columns.Add("Created");
                dataTable.Columns.Add("Is Clustered");
                dataTable.Columns.Add("Is Deleted");
                dataTable.Columns.Add("Owner Node");
                dataTable.Columns.Add("Categories");

                foreach (var vm in results)
                {
                    var row = dataTable.NewRow();
                    var vmName = vm.Properties["Name"]?.Value?.ToString() ?? "";
                    row["VM Name"] = vmName;

                    // VM Id (GUID)
                    var vmId = vm.Properties["VMId"]?.Value ?? vm.Properties["Id"]?.Value;
                    row["VM Id"] = vmId?.ToString() ?? "";

                    row["State"] = vm.Properties["State"]?.Value?.ToString() ?? "";

                    // Enabled property (some VMs may not have this, default to Yes if not present)
                    var enabled = vm.Properties["Enabled"]?.Value;
                    if (enabled != null)
                    {
                        row["Enabled"] = (bool)enabled ? "Yes" : "No";
                    }
                    else
                    {
                        // If property doesn't exist, assume enabled
                        row["Enabled"] = "Yes";
                    }

                    row["CPU Count"] = vm.Properties["ProcessorCount"]?.Value?.ToString() ?? "";
                    row["CPU Usage %"] = vm.Properties["CPUUsage"]?.Value?.ToString() ?? "";

                    // Memory values - convert from bytes to MB
                    var memAssigned = vm.Properties["MemoryAssigned"]?.Value;
                    row["Memory Assigned (MB)"] = memAssigned != null ? ((long)memAssigned / 1048576).ToString() : "";

                    var memDemand = vm.Properties["MemoryDemand"]?.Value;
                    row["Memory Demand (MB)"] = memDemand != null ? ((long)memDemand / 1048576).ToString() : "";

                    var memStartup = vm.Properties["MemoryStartup"]?.Value;
                    row["Memory Startup (MB)"] = memStartup != null ? ((long)memStartup / 1048576).ToString() : "";

                    var dynamicMem = vm.Properties["DynamicMemoryEnabled"]?.Value;
                    row["Dynamic Memory"] = dynamicMem != null && (bool)dynamicMem ? "Yes" : "No";

                    // Check if this VM has pre-collected detailed properties (from cluster collection)
                    bool hasDetailedProperties = vm.Properties["TotalDiskSizeGB"] != null &&
                                                  vm.Properties["NetworkAdapterCount"] != null &&
                                                  vm.Properties["CheckpointCount"] != null;

                    if (hasDetailedProperties && isClusterCollection)
                    {
                        // Use pre-collected data from cluster node collection
                        var totalDiskGB = vm.Properties["TotalDiskSizeGB"]?.Value;
                        row["Total Disk (GB)"] = totalDiskGB != null ? Convert.ToDouble(totalDiskGB).ToString("F2") : "";

                        var networkAdapterCount = vm.Properties["NetworkAdapterCount"]?.Value;
                        row["Network Adapters"] = networkAdapterCount?.ToString() ?? "0";

                        var checkpointCount = vm.Properties["CheckpointCount"]?.Value;
                        row["Checkpoints"] = checkpointCount?.ToString() ?? "0";

                        // Use pre-collected integration services display
                        var integrationServicesDisplay = vm.Properties["IntegrationServicesDisplay"]?.Value?.ToString();
                        row["Integration Services"] = !string.IsNullOrEmpty(integrationServicesDisplay) ? integrationServicesDisplay : "N/A";
                    }
                    else
                    {
                        // Standard approach - make additional calls (for standalone/local)
                        if (!string.IsNullOrEmpty(vmName))
                        {
                            var totalDiskGB = GetVMTotalDiskSize(vmName);
                            row["Total Disk (GB)"] = totalDiskGB > 0 ? totalDiskGB.ToString("F2") : "";

                            var networkAdapterCount = GetVMNetworkAdapterCount(vmName);
                            row["Network Adapters"] = networkAdapterCount.ToString();

                            var integrationServicesInfo = GetVMIntegrationServices(vmName);
                            row["Integration Services"] = integrationServicesInfo;

                            var checkpointCount = GetVMCheckpointCount(vmName);
                            row["Checkpoints"] = checkpointCount.ToString();
                        }
                        else
                        {
                            row["Total Disk (GB)"] = "";
                            row["Network Adapters"] = "";
                            row["Integration Services"] = "N/A";
                            row["Checkpoints"] = "";
                        }
                    }

                    row["Generation"] = vm.Properties["Generation"]?.Value?.ToString() ?? "";

                    // Format uptime
                    var uptime = vm.Properties["Uptime"]?.Value;
                    if (uptime != null)
                    {
                        if (uptime is TimeSpan ts)
                        {
                            row["Uptime"] = FormatTimeSpan(ts);
                        }
                        else
                        {
                            // Try to parse string representation
                            if (TimeSpan.TryParse(uptime.ToString(), out TimeSpan parsedTs))
                            {
                                row["Uptime"] = FormatTimeSpan(parsedTs);
                            }
                            else
                            {
                                row["Uptime"] = "";
                            }
                        }
                    }
                    else
                    {
                        row["Uptime"] = "";
                    }

                    row["Heartbeat"] = vm.Properties["Heartbeat"]?.Value?.ToString() ?? "";

                    row["Auto Start"] = vm.Properties["AutomaticStartAction"]?.Value?.ToString() ?? "";
                    row["Auto Stop"] = vm.Properties["AutomaticStopAction"]?.Value?.ToString() ?? "";

                    // Get VM groups - always need to query this as it's not node-specific
                    if (!string.IsNullOrEmpty(vmName))
                    {
                        var groups = GetVMGroups(vmName);
                        row["VM Groups"] = groups;
                    }
                    else
                    {
                        row["VM Groups"] = "N/A";
                    }

                    row["Checkpoint Type"] = vm.Properties["CheckpointType"]?.Value?.ToString() ?? "";

                    var replicationState = vm.Properties["ReplicationState"]?.Value?.ToString();
                    row["Replication"] = !string.IsNullOrEmpty(replicationState) ? replicationState : "Not Configured";

                    var creationTime = vm.Properties["CreationTime"]?.Value;
                    row["Created"] = creationTime != null ? ((DateTime)creationTime).ToString("yyyy-MM-dd HH:mm") : "";

                    var isClustered = vm.Properties["IsClustered"]?.Value;
                    row["Is Clustered"] = isClustered != null && (bool)isClustered ? "Yes" : "No";

                    // Is Deleted property
                    var isDeleted = vm.Properties["IsDeleted"]?.Value;
                    if (isDeleted != null)
                    {
                        row["Is Deleted"] = (bool)isDeleted ? "Yes" : "No";
                    }
                    else
                    {
                        // If property doesn't exist, assume not deleted
                        row["Is Deleted"] = "No";
                    }

                    // Owner Node (for cluster VMs)
                    var ownerNode = vm.Properties["ComputerName"]?.Value?.ToString();
                    row["Owner Node"] = !string.IsNullOrEmpty(ownerNode) ? ownerNode : SessionContext.ServerName;

                    row["Categories"] = "";

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewVMOverView.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewVMOverView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewVMOverView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewVMOverView.MultiSelect = false;
                datagridviewVMOverView.ReadOnly = true;
                datagridviewVMOverView.AllowUserToAddRows = false;
                datagridviewVMOverView.AllowUserToDeleteRows = false;
                datagridviewVMOverView.RowHeadersVisible = false;

                // Color coding will be applied automatically via DataBindingComplete event

                Message($"VM overview loaded successfully with {results.Count} VMs",
                    EventType.Information, 2173);
            }
            catch (Exception ex)
            {
                Message($"Error loading VM overview: {ex.Message}",
                    EventType.Error, 2006);
                MessageBox.Show($"Error loading VM overview: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets VMs from all cluster nodes by iterating through each node
        /// Retrieves detailed VM properties at collection time to avoid cross-node issues
        /// </summary>
        private System.Collections.ObjectModel.Collection<PSObject> GetClusterVMs()
        {
            var allVMs = new System.Collections.ObjectModel.Collection<PSObject>();

            try
            {
                Message("Getting cluster nodes...",
                    EventType.Information, 2174);

                // First, get the list of cluster nodes
                string getNodesScript = @"
                    Get-ClusterNode -ErrorAction Stop | Select-Object -ExpandProperty Name
                ";

                var nodesResult = ExecutePowerShellCommand(getNodesScript);

                if (nodesResult == null || nodesResult.Count == 0)
                {
                    Message("No cluster nodes found, falling back to standard Get-VM",
                        EventType.Warning, 2175);
                    return ExecutePowerShellCommand("Get-VM");
                }

                // Build list of cluster nodes
                var clusterNodes = new List<string>();
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

                Message($"Found {clusterNodes.Count} cluster nodes: {string.Join(", ", clusterNodes)}",
                    EventType.Information, 2176);

                // Now get VMs with full details from each node
                int nodeIndex = 0;
                foreach (var node in clusterNodes)
                {
                    nodeIndex++;
                    try
                    {
                        Message($"Getting VMs from cluster node {nodeIndex} of {clusterNodes.Count}: {node}",
                            EventType.Information, 2177);

                        // Get VMs with all detailed properties in one call per node
                        // This avoids cross-node VM lookup issues
                        string getDetailedVMsScript = @"
                            $vmList = Get-VM -ErrorAction SilentlyContinue
                            foreach ($vm in $vmList) {
                                # Get all detailed properties on this node where the VM exists
                                $vmProcessor = Get-VMProcessor -VMName $vm.Name -ErrorAction SilentlyContinue
                                $vmMemory = Get-VMMemory -VMName $vm.Name -ErrorAction SilentlyContinue
                                $vmNetworkAdapters = @(Get-VMNetworkAdapter -VMName $vm.Name -ErrorAction SilentlyContinue)
                                $vmHardDrives = @(Get-VMHardDiskDrive -VMName $vm.Name -ErrorAction SilentlyContinue)
                                $vmCheckpoints = @(Get-VMSnapshot -VMName $vm.Name -ErrorAction SilentlyContinue)
                                $vmIntegrationServices = @(Get-VMIntegrationService -VMName $vm.Name -ErrorAction SilentlyContinue)

                                # Calculate total disk size
                                $totalDiskSizeGB = 0
                                foreach ($drive in $vmHardDrives) {
                                    if ($drive.Path -and (Test-Path $drive.Path -ErrorAction SilentlyContinue)) {
                                        $diskInfo = Get-Item $drive.Path -ErrorAction SilentlyContinue
                                        if ($diskInfo) {
                                            $totalDiskSizeGB += [Math]::Round($diskInfo.Length / 1GB, 2)
                                        }
                                    }
                                }

                                # Format integration services
                                $enabledServices = @()
                                $totalServiceCount = 0
                                foreach ($svc in $vmIntegrationServices) {
                                    $totalServiceCount++
                                    if ($svc.Enabled) {
                                        $displayName = $svc.Name -replace 'Guest Service Interface', 'Guest Svc' -replace 'Key-Value Pair Exchange', 'KVP' -replace 'Time Synchronization', 'Time Sync'
                                        $enabledServices += $displayName
                                    }
                                }
                                $integrationServicesDisplay = if ($totalServiceCount -gt 0) {
                                    ""$($enabledServices.Count)/$totalServiceCount enabled""
                                    if ($enabledServices.Count -gt 0) {
                                        ""$($enabledServices.Count)/$totalServiceCount enabled ($($enabledServices -join ', '))""
                                    } else {
                                        ""$($enabledServices.Count)/$totalServiceCount enabled (All disabled)""
                                    }
                                } else { 'No services' }

                                # Create custom object with all details
                                [PSCustomObject]@{
                                    Name = $vm.Name
                                    VMId = $vm.VMId
                                    Id = $vm.Id
                                    State = $vm.State
                                    Enabled = if ($vm.PSObject.Properties['Enabled']) { $vm.Enabled } else { $true }
                                    ProcessorCount = if ($vmProcessor) { $vmProcessor.Count } else { 1 }
                                    CPUUsage = $vm.CPUUsage
                                    MemoryAssigned = $vm.MemoryAssigned
                                    MemoryDemand = $vm.MemoryDemand
                                    MemoryStartup = if ($vmMemory) { $vmMemory.Startup } else { $vm.MemoryStartup }
                                    DynamicMemoryEnabled = $vm.DynamicMemoryEnabled
                                    Generation = $vm.Generation
                                    Uptime = $vm.Uptime
                                    Heartbeat = $vm.Heartbeat
                                    IntegrationServicesDisplay = $integrationServicesDisplay
                                    AutomaticStartAction = $vm.AutomaticStartAction
                                    AutomaticStopAction = $vm.AutomaticStopAction
                                    CheckpointType = $vm.CheckpointType
                                    ReplicationState = $vm.ReplicationState
                                    CreationTime = $vm.CreationTime
                                    IsClustered = $vm.IsClustered
                                    IsDeleted = if ($vm.PSObject.Properties['IsDeleted']) { $vm.IsDeleted } else { $false }
                                    TotalDiskSizeGB = $totalDiskSizeGB
                                    NetworkAdapterCount = $vmNetworkAdapters.Count
                                    CheckpointCount = $vmCheckpoints.Count
                                    ComputerName = $env:COMPUTERNAME
                                }
                            }
                        ";

                        var nodeVMs = ExecutePowerShellCommandOnNode(node, getDetailedVMsScript);

                        if (nodeVMs != null && nodeVMs.Count > 0)
                        {
                            foreach (var vm in nodeVMs)
                            {
                                allVMs.Add(vm);
                            }
                            Message($"Added {nodeVMs.Count} VMs from cluster node: {node}",
                                EventType.Information, 2178);
                        }
                        else
                        {
                            Message($"No VMs found on cluster node: {node}",
                                EventType.Information, 2179);
                        }
                    }
                    catch (Exception ex)
                    {
                        Message($"Failed to get VMs from cluster node {node}: {ex.Message}",
                            EventType.Warning, 2180);
                        // Continue processing other nodes
                    }
                }

                Message($"Total VMs collected from all cluster nodes: {allVMs.Count}",
                    EventType.Information, 2181);

                return allVMs;
            }
            catch (Exception ex)
            {
                Message($"Error getting cluster VMs: {ex.Message}",
                    EventType.Error, 2182);

                // Fall back to standard Get-VM
                Message("Falling back to standard Get-VM",
                    EventType.Warning, 2183);
                return ExecutePowerShellCommand("Get-VM");
            }
        }

        /// <summary>
        /// Executes a PowerShell command directly on a specific cluster node
        /// </summary>
        private System.Collections.ObjectModel.Collection<PSObject> ExecutePowerShellCommandOnNode(string nodeName, string command)
        {
            try
            {
                if (_persistentRunspace == null || _persistentRunspace.RunspaceStateInfo.State != RunspaceState.Opened)
                {
                    Message($"Persistent runspace not available for node command execution",
                        EventType.Error, 2184);
                    return null;
                }

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = _persistentRunspace;

                    // Use Invoke-Command to execute on the specific node
                    string script = $@"
                        Invoke-Command -ComputerName '{nodeName}' -ScriptBlock {{
                            {command}
                        }} -ErrorAction Stop
                    ";

                    // Add credentials if available
                    if (SessionContext.Credentials != null)
                    {
                        script = $@"
                            $cred = $args[0]
                            Invoke-Command -ComputerName '{nodeName}' -Credential $cred -ScriptBlock {{
                                {command}
                            }} -ErrorAction Stop
                        ";
                        ps.AddScript(script);
                        ps.AddArgument(SessionContext.Credentials);
                    }
                    else
                    {
                        ps.AddScript(script);
                    }

                    var results = ps.Invoke();

                    if (ps.HadErrors)
                    {
                        var errors = string.Join(Environment.NewLine,
                            ps.Streams.Error.Select(e => e.ToString()));
                        Message($"PowerShell errors on node '{nodeName}': {errors}",
                            EventType.Warning, 2185);
                    }

                    return results;
                }
            }
            catch (Exception ex)
            {
                Message($"Error executing command on node '{nodeName}': {ex.Message}",
                    EventType.Error, 2186);
                return null;
            }
        }

        private System.Collections.ObjectModel.Collection<PSObject> ExecutePowerShellCommand(string command, Dictionary<string, object> parameters = null)
        {
            try
            {
                if (SessionContext.IsLocal)
                {
                    // Local execution - create new runspace for each command
                    using (Runspace runspace = RunspaceFactory.CreateRunspace())
                    {
                        runspace.Open();

                        using (PowerShell ps = PowerShell.Create())
                        {
                            ps.Runspace = runspace;
                            ps.AddScript(command);

                            var results = ps.Invoke();

                            if (ps.HadErrors)
                            {
                                var errors = string.Join(Environment.NewLine,
                                    ps.Streams.Error.Select(e => e.ToString()));
                                Message($"PowerShell command '{command}' errors: {errors}",
                                    EventType.Error, 2007);
                                return null;
                            }

                            return results;
                        }
                    }
                }
                else
                {
                    // Remote execution - use persistent runspace with the session
                    if (_persistentRunspace == null || _persistentRunspace.RunspaceStateInfo.State != RunspaceState.Opened)
                    {
                        Message("Persistent runspace is not available or not opened",
                            EventType.Error, 2011);
                        return null;
                    }

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _persistentRunspace;

                        // Remote execution via Invoke-Command
                        ps.AddCommand("Invoke-Command")
                          .AddParameter("Session", _psSession)
                          .AddParameter("ScriptBlock", ScriptBlock.Create(command));

                        var results = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var errors = string.Join(Environment.NewLine,
                                ps.Streams.Error.Select(e => e.ToString()));
                            Message($"PowerShell command '{command}' errors: {errors}",
                                EventType.Error, 2007);
                            return null;
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                Message($"Error executing PowerShell command '{command}': {ex.Message}",
                    EventType.Error, 2008);
                return null;
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            else if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            else
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        private double GetVMTotalDiskSize(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMHardDiskDrive -VMName '{vmName}'");

                if (results == null || results.Count == 0)
                    return 0;

                double totalGB = 0;
                foreach (var hdd in results)
                {
                    var path = hdd.Properties["Path"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(path))
                    {
                        // For remote, we need to get file size through PS
                        if (!SessionContext.IsLocal)
                        {
                            var sizeResult = ExecutePowerShellCommand($"(Get-Item '{path}').Length");
                            if (sizeResult != null && sizeResult.Count > 0)
                            {
                                var size = Convert.ToInt64(sizeResult[0].BaseObject);
                                totalGB += size / (1024.0 * 1024.0 * 1024.0);
                            }
                        }
                        else if (File.Exists(path))
                        {
                            var fileInfo = new System.IO.FileInfo(path);
                            totalGB += fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
                        }
                    }
                }

                return totalGB;
            }
            catch
            {
                return 0;
            }
        }

        private int GetVMNetworkAdapterCount(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMNetworkAdapter -VMName '{vmName}'");
                return results?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetVMGroups(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMGroup | Where-Object {{ $_.VMMembers.Name -contains '{vmName}' }} | Select-Object -ExpandProperty Name");

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

        private int GetVMCheckpointCount(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMSnapshot -VMName '{vmName}'");
                return results?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetVMIntegrationServices(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMIntegrationService -VMName '{vmName}'");

                if (results == null || results.Count == 0)
                    return "No services";

                var enabledServicesList = new List<string>();
                int totalServices = 0;

                foreach (var service in results)
                {
                    totalServices++;

                    var nameObj = service.Properties["Name"]?.Value;
                    var enabledObj = service.Properties["Enabled"]?.Value;

                    string name = nameObj?.ToString();

                    // More robust boolean checking - handle both bool and string "True"/"False"
                    bool isEnabled = false;
                    if (enabledObj != null)
                    {
                        if (enabledObj is bool boolValue)
                        {
                            isEnabled = boolValue;
                        }
                        else if (enabledObj is string stringValue)
                        {
                            isEnabled = stringValue.Equals("True", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            // Try to parse as bool
                            bool.TryParse(enabledObj.ToString(), out isEnabled);
                        }
                    }

                    if (!string.IsNullOrEmpty(name) && isEnabled)
                    {
                        // Shorten service names for display
                        string displayName = name
                            .Replace("Guest Service Interface", "Guest Svc")
                            .Replace("Key-Value Pair Exchange", "KVP")
                            .Replace("Time Synchronization", "Time Sync");

                        enabledServicesList.Add(displayName);
                    }
                }

                // Format output: show count and enabled service names
                if (totalServices > 0)
                {
                    string displayText = $"{enabledServicesList.Count}/{totalServices} enabled";

                    if (enabledServicesList.Count > 0)
                    {
                        /* Show first 3 enabled services
                        var servicesToShow = enabledServicesList.Take(3).ToList();
                        displayText += $" ({string.Join(", ", servicesToShow)}";

                        if (enabledServicesList.Count > 3)
                        {
                            displayText += $", +{enabledServicesList.Count - 3}";
                        }

                        displayText += ")";*/

                        // Show all enabled services
                        displayText += $" ({string.Join(", ", enabledServicesList)})";
                    }
                    else
                    {
                        displayText += " (All disabled)";
                    }

                    return displayText;
                }
                else
                {
                    return "No services";
                }
            }
            catch (Exception ex)
            {
                Message($"Error getting VM integration services for '{vmName}': {ex.Message}",
                    EventType.Warning, 2150);
                return "N/A";
            }
        }

        private void ApplyColorCoding()
        {
            foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
            {
                // Color code VM State
                if (row.Cells["State"] != null && row.Cells["State"].Value != null)
                {
                    var state = row.Cells["State"].Value.ToString();
                    switch (state)
                    {
                        case "Running":
                            row.Cells["State"].Style.BackColor = Color.LightGreen;
                            row.Cells["State"].Style.ForeColor = Color.DarkGreen;
                            break;
                        case "Off":
                            row.Cells["State"].Style.BackColor = Color.LightCoral;
                            row.Cells["State"].Style.ForeColor = Color.DarkRed;
                            break;
                        case "Paused":
                            row.Cells["State"].Style.BackColor = Color.LightYellow;
                            row.Cells["State"].Style.ForeColor = Color.DarkOrange;
                            break;
                        case "Saved":
                            row.Cells["State"].Style.BackColor = Color.LightBlue;
                            row.Cells["State"].Style.ForeColor = Color.DarkBlue;
                            break;
                    }
                }

                // Color code Heartbeat status
                if (row.Cells["Heartbeat"] != null && row.Cells["Heartbeat"].Value != null)
                {
                    var heartbeat = row.Cells["Heartbeat"].Value.ToString();
                    if (heartbeat.Contains("Ok"))
                    {
                        row.Cells["Heartbeat"].Style.BackColor = Color.LightGreen;
                    }
                    else if (heartbeat == "Unknown")
                    {
                        row.Cells["Heartbeat"].Style.BackColor = Color.LightYellow;
                    }
                    else
                    {
                        row.Cells["Heartbeat"].Style.BackColor = Color.LightCoral;
                    }
                }

                // Color code Dynamic Memory
                if (row.Cells["Dynamic Memory"] != null && row.Cells["Dynamic Memory"].Value != null)
                {
                    var dynMem = row.Cells["Dynamic Memory"].Value.ToString();
                    if (dynMem == "Yes")
                    {
                        row.Cells["Dynamic Memory"].Style.BackColor = Color.LightBlue;
                    }
                }

                // Color code Replication status
                if (row.Cells["Replication"] != null && row.Cells["Replication"].Value != null)
                {
                    var replication = row.Cells["Replication"].Value.ToString();
                    if (replication != "Not Configured")
                    {
                        row.Cells["Replication"].Style.BackColor = Color.LightGreen;
                    }
                }
            }
        }

        private bool ConfirmDisconnectAndExit()
        {
            // Check if there's an active Hyper-V connection
            if (SessionContext.IsSessionActive())
            {
                Message("User is attempting to close the application with active connection",
                    EventType.Information, 2012);

                // Show confirmation dialog
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to disconnect from Hyper-V (server: '{SessionContext.ServerName}') and close the application?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    Message($"User confirmed exit - disconnecting from Hyper-V server '{SessionContext.ServerName}'...",
                        EventType.Information, 2013);

                    // Cleanup will be handled by OnFormClosing
                    return true;
                }
                else
                {
                    // User cancelled
                    Message("User cancelled exit - keeping application open",
                        EventType.Information, 2015);
                    return false;
                }
            }
            else
            {
                // No active connection - allow close
                Message("No active connection - proceeding with exit",
                    EventType.Information, 2016);
                return true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Use the shared confirmation function
            if (!ConfirmDisconnectAndExit())
            {
                e.Cancel = true;
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Message($"User initiated disconnect from '{SessionContext.ServerName}'",
                    EventType.Information, 2017);

                // Check if there's an active connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active connection to disconnect.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show confirmation dialog
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to disconnect from Hyper-V (server: '{SessionContext.ServerName}')?",
                    "Confirm Disconnect",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    Message($"User confirmed disconnect from Hyper-V server '{SessionContext.ServerName}'",
                        EventType.Information, 2018);

                    // Disable disconnect menu item during operation to prevent double-clicks
                    disconnectToolStripMenuItem.Enabled = false;

                    // Store server name before clearing
                    string disconnectedServer = SessionContext.ServerName;

                    // Clean up remote session if exists
                    if (_psSession != null && _persistentRunspace != null)
                    {
                        try
                        {
                            using (PowerShell ps = PowerShell.Create())
                            {
                                ps.Runspace = _persistentRunspace;
                                ps.AddCommand("Remove-PSSession")
                                  .AddParameter("Session", _psSession);
                                ps.Invoke();
                            }
                            Message("Remote PowerShell session closed during disconnect",
                                EventType.Information, 2019);
                        }
                        catch (Exception ex)
                        {
                            Message($"Error closing PS session during disconnect: {ex.Message}",
                                EventType.Warning, 2020);
                        }
                    }

                    // Dispose persistent runspace
                    if (_persistentRunspace != null)
                    {
                        try
                        {
                            _persistentRunspace.Close();
                            _persistentRunspace.Dispose();
                            _persistentRunspace = null;
                            Message("Persistent runspace closed during disconnect",
                                EventType.Information, 2021);
                        }
                        catch (Exception ex)
                        {
                            Message($"Error closing persistent runspace during disconnect: {ex.Message}",
                                EventType.Warning, 2022);
                        }
                    }

                    // Clear PS session reference
                    _psSession = null;

                    // Clear global connection data (SessionContext)
                    SessionContext.Clear();

                    Message($"Successfully disconnected from Hyper-V server '{disconnectedServer}'",
                        EventType.Information, 2023);

                    // Hide main form temporarily
                    this.Hide();

                    Message("Showing login form for reconnection...",
                        EventType.Information, 2024);

                    // Show login form for reconnection
                    using (Forms.LoginForm loginForm = new Forms.LoginForm())
                    {
                        var loginResult = loginForm.ShowDialog();

                        if (loginResult == DialogResult.OK && loginForm.Result != null && loginForm.Result.Success)
                        {
                            // Authentication successful - close current form and open new MainForm
                            Message($"Reconnected successfully to '{loginForm.Result.ServerName}'",
                                EventType.Information, 2025);

                            // Close current MainForm (will trigger cleanup)
                            this.DialogResult = DialogResult.OK;
                            this.Close();

                            // The LoginForm will handle showing the new MainForm
                        }
                        else
                        {
                            // Authentication failed or cancelled - close application
                            Message("Authentication cancelled after disconnect - closing application",
                                EventType.Information, 2026);

                            this.DialogResult = DialogResult.Cancel;
                            this.Close();
                        }
                    }
                }
                else
                {
                    // User cancelled disconnect
                    Message("User cancelled disconnect operation",
                        EventType.Information, 2027);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error during disconnect: {ex.Message}";
                Message(errorMsg, EventType.Error, 2028);

                // Re-enable disconnect menu item so user can try again
                disconnectToolStripMenuItem.Enabled = true;

                // Show error to user
                MessageBox.Show($"Failed to disconnect from Hyper-V:\n\n{ex.Message}",
                    "Disconnect Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // Show the form again if it was hidden
                this.Show();
            }
        }

        private void buttonCreateANewVMGroup_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User initiated VM Group creation",
                    EventType.Information, 2029);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Show the CreateVMGroupForm
                using (Forms.CreateVMGroupForm createGroupForm = new Forms.CreateVMGroupForm())
                {
                    var result = createGroupForm.ShowDialog();

                    if (result == DialogResult.OK && createGroupForm.Result != null)
                    {
                        string groupName = createGroupForm.Result.GroupName;
                        string groupType = createGroupForm.Result.GroupType;

                        Message($"Creating VM Group '{groupName}' of type '{groupType}'...",
                            EventType.Information, 2030);

                        // Create the VM Group using PowerShell
                        var createResult = VMGroups.CreateHyperVVMGroup(groupName, groupType, cmd => ExecutePowerShellCommand(cmd));

                        if (createResult.Success)
                        {
                            Message($"VM Group '{groupName}' created successfully",
                                EventType.Information, 2031);

                            MessageBox.Show($"VM Group '{groupName}' created successfully.",
                                "Group Created",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Refresh VM Groups view
                            VMGroups.RefreshVMGroupsView(
                                $"New group created: {groupName}",
                                cmd => ExecutePowerShellCommand(cmd),
                                groups => UpdateVMGroupsDataGridView(groups));
                        }
                        else
                        {
                            Message($"Failed to create VM Group '{groupName}': {createResult.Error}",
                                EventType.Error, 2032);

                            MessageBox.Show($"Failed to create VM Group:\n\n{createResult.Error}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Message("VM Group creation cancelled",
                            EventType.Information, 2033);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error creating VM Group: {ex.Message}";
                Message(errorMsg, EventType.Error, 2034);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonDeleteSelectedVMGrou_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User initiated VM Group deletion",
                    EventType.Information, 2039);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group (assuming you have a datagridviewVMGroups control)
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a VM Group to delete.",
                        "No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string selectedGroupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(selectedGroupName))
                {
                    MessageBox.Show("Invalid VM Group selection.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Message($"User selected VM Group '{selectedGroupName}' for deletion",
                    EventType.Information, 2040);

                // First confirmation
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to delete VM Group '{selectedGroupName}'?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    Message("VM Group deletion cancelled by user",
                        EventType.Information, 2041);
                    return;
                }

                // Try to remove without force first
                var result = VMGroups.RemoveHyperVVMGroup(selectedGroupName, false, cmd => ExecutePowerShellCommand(cmd));

                if (result.Success)
                {
                    Message($"VM Group '{selectedGroupName}' deleted successfully",
                        EventType.Information, 2042);

                    MessageBox.Show($"VM Group '{selectedGroupName}' deleted successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Refresh the VM Groups view
                    VMGroups.RefreshVMGroupsView(
                        $"Group deleted: {selectedGroupName}",
                        cmd => ExecutePowerShellCommand(cmd),
                        groups => UpdateVMGroupsDataGridView(groups));
                }
                else
                {
                    // Check if it's because the group contains VMs and can be forced
                    if (result.CanForce)
                    {
                        Message($"VM Group '{selectedGroupName}' contains {result.VMCount} VM(s), asking for force deletion",
                            EventType.Information, 2043);

                        string vmList = string.Join("\n• ", result.VMNames);
                        string forceMessage = $"VM Group '{selectedGroupName}' contains {result.VMCount} VM(s):\n\n• {vmList}\n\n" +
                                            "The VMs will remain but will be removed from this group.\n\n" +
                                            "Do you want to force delete the VM Group anyway?";

                        var forceResult = MessageBox.Show(forceMessage,
                            "Force Delete VM Group?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation);

                        if (forceResult == DialogResult.Yes)
                        {
                            Message($"User confirmed force deletion of VM Group '{selectedGroupName}'",
                                EventType.Information, 2044);

                            // Try again with force
                            var forceDeleteResult = VMGroups.RemoveHyperVVMGroup(selectedGroupName, true, cmd => ExecutePowerShellCommand(cmd));

                            if (forceDeleteResult.Success)
                            {
                                Message($"VM Group '{selectedGroupName}' force deleted successfully",
                                    EventType.Information, 2045);

                                MessageBox.Show($"VM Group '{selectedGroupName}' force deleted successfully. " +
                                              "The VMs remain but are no longer part of this group.",
                                    "Success",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                                // Refresh the VM Groups view
                                VMGroups.RefreshVMGroupsView(
                                    $"Group force deleted: {selectedGroupName}",
                                    cmd => ExecutePowerShellCommand(cmd),
                                    groups => UpdateVMGroupsDataGridView(groups));
                            }
                            else
                            {
                                Message($"Failed to force delete VM Group '{selectedGroupName}': {forceDeleteResult.Error}",
                                    EventType.Error, 2046);

                                MessageBox.Show($"Failed to force delete VM Group:\n\n{forceDeleteResult.Error}",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            Message("User cancelled force deletion",
                                EventType.Information, 2047);
                        }
                    }
                    else
                    {
                        // Other error
                        Message($"Failed to delete VM Group '{selectedGroupName}': {result.Error}",
                            EventType.Error, 2048);

                        MessageBox.Show($"Failed to delete VM Group:\n\n{result.Error}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error deleting VM Group: {ex.Message}";
                Message(errorMsg, EventType.Error, 2049);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonRenameSelectedVMGrou_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User initiated VM Group rename",
                    EventType.Information, 2094);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a VM Group to rename.",
                        "No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string currentGroupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(currentGroupName))
                {
                    MessageBox.Show("Invalid VM Group selection.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Message($"User selected VM Group '{currentGroupName}' for rename",
                    EventType.Information, 2095);

                // Show rename form
                using (Forms.RenameVMGroupForm renameForm = new Forms.RenameVMGroupForm())
                {
                    renameForm.CurrentGroupName = currentGroupName;

                    var result = renameForm.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrEmpty(renameForm.NewGroupName))
                    {
                        string newGroupName = renameForm.NewGroupName;

                        Message($"Renaming VM Group from '{currentGroupName}' to '{newGroupName}'...",
                            EventType.Information, 2096);

                        // Rename the VM Group using PowerShell
                        var renameResult = VMGroups.RenameHyperVVMGroup(
                            currentGroupName,
                            newGroupName,
                            cmd => ExecutePowerShellCommand(cmd));

                        if (renameResult.Success)
                        {
                            Message($"VM Group renamed successfully from '{currentGroupName}' to '{newGroupName}'",
                                EventType.Information, 2097);

                            MessageBox.Show($"VM Group renamed successfully from '{currentGroupName}' to '{newGroupName}'.",
                                "Group Renamed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Refresh VM Groups view
                            VMGroups.RefreshVMGroupsView(
                                $"Group renamed: {currentGroupName} -> {newGroupName}",
                                cmd => ExecutePowerShellCommand(cmd),
                                groups => UpdateVMGroupsDataGridView(groups));
                        }
                        else
                        {
                            Message($"Failed to rename VM Group '{currentGroupName}': {renameResult.Error}",
                                EventType.Error, 2098);

                            MessageBox.Show($"Failed to rename VM Group:\n\n{renameResult.Error}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Message("VM Group rename cancelled",
                            EventType.Information, 2099);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error renaming VM Group: {ex.Message}";
                Message(errorMsg, EventType.Error, 2100);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonLoadGroupsrefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested VM Groups refresh",
                    EventType.Information, 2056);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Show progress
                this.Cursor = Cursors.WaitCursor;

                Message("Retrieving VM Groups from server...",
                    EventType.Information, 2057);

                // Get VM Groups
                var vmGroups = VMGroups.GetHyperVVMGroups(cmd => ExecutePowerShellCommand(cmd));

                if (vmGroups != null)
                {
                    Message($"Retrieved {vmGroups.Count} VM Groups, updating DataGridView",
                        EventType.Information, 2058);

                    // Update DataGridView
                    UpdateVMGroupsDataGridView(vmGroups);

                    // Update status strip
                    toolStripStatusLabelTextMainForm.Text = $"VM Groups refreshed successfully: {vmGroups.Count} group(s) loaded";

                    /*MessageBox.Show($"VM Groups refreshed successfully.\n\nFound {vmGroups.Count} group(s).",
                        "Refresh Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);*/
                }
                else
                {
                    Message("No VM Groups retrieved",
                        EventType.Warning, 2059);

                    MessageBox.Show("No VM Groups found or error retrieving groups.",
                        "Refresh Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing VM Groups: {ex.Message}";
                Message(errorMsg, EventType.Error, 2060);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateVMGroupsDataGridView(List<VMGroupInfo> vmGroups)
        {
            try
            {
                if (datagridviewVMGroups == null)
                {
                    Message("datagridviewVMGroups control not found",
                        EventType.Warning, 2067);
                    return;
                }

                Message($"Updating VM Groups DataGridView with {vmGroups.Count} groups",
                    EventType.Information, 2068);

                // Clear existing data
                datagridviewVMGroups.DataSource = null;
                datagridviewVMGroups.Rows.Clear();
                datagridviewVMGroups.Columns.Clear();

                if (vmGroups == null || vmGroups.Count == 0)
                {
                    Message("No VM Groups to display",
                        EventType.Information, 2069);
                    return;
                }

                // Create DataTable
                var dataTable = new DataTable();
                dataTable.Columns.Add("Group Name", typeof(string));
                dataTable.Columns.Add("Group Type", typeof(string));
                dataTable.Columns.Add("VM Count", typeof(string));
                dataTable.Columns.Add("VM Members", typeof(string));
                dataTable.Columns.Add("Computer Name", typeof(string));

                // Add rows
                foreach (var group in vmGroups)
                {
                    var row = dataTable.NewRow();
                    row["Group Name"] = group.Name;
                    row["Group Type"] = group.GroupTypeDisplay;
                    row["VM Count"] = group.VMCount.ToString();

                    // Truncate long VM lists
                    string vmMembers = group.VMList;
                    if (!string.IsNullOrEmpty(vmMembers) && vmMembers.Length > 100)
                    {
                        vmMembers = vmMembers.Substring(0, 100) + "...";
                    }
                    row["VM Members"] = vmMembers;

                    row["Computer Name"] = group.ComputerName;

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewVMGroups.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewVMGroups.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewVMGroups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewVMGroups.MultiSelect = false;
                datagridviewVMGroups.ReadOnly = true;
                datagridviewVMGroups.AllowUserToAddRows = false;
                datagridviewVMGroups.AllowUserToDeleteRows = false;
                datagridviewVMGroups.RowHeadersVisible = false;

                Message($"VM Groups DataGridView updated successfully with {vmGroups.Count} groups",
                    EventType.Information, 2070);
            }
            catch (Exception ex)
            {
                Message($"Error updating VM Groups DataGridView: {ex.Message}",
                    EventType.Error, 2071);
            }
        }

        private void UpdateVMGroupsDataGridView()
        {
            try
            {
                Message("Refreshing VM Groups DataGridView (silent refresh)",
                    EventType.Information, 2083);

                // Get VM Groups without showing message boxes
                var vmGroups = VMGroups.GetHyperVVMGroups(cmd => ExecutePowerShellCommand(cmd));

                if (vmGroups != null)
                {
                    UpdateVMGroupsDataGridView(vmGroups);
                }
                else
                {
                    Message("No VM Groups retrieved during silent refresh",
                        EventType.Information, 2084);
                }
            }
            catch (Exception ex)
            {
                Message($"Error during silent VM Groups refresh: {ex.Message}",
                    EventType.Error, 2085);
            }
        }

        private void allVMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested VM data export",
                    EventType.Information, 2101);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have VM data
                if (datagridviewVMOverView == null || datagridviewVMOverView.Rows.Count == 0)
                {
                    MessageBox.Show("No VM data available. Please load VMs first.",
                        "No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Message($"Export All VM Data requested - {datagridviewVMOverView.Rows.Count} VMs available",
                    EventType.Information, 2102);

                // Show SaveFileDialog with format options
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = "Export All VM Data";
                    saveFileDialog.FileName = $"HyperV_AllVMData_{SessionContext.ServerName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    saveFileDialog.Filter = "JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;
                        string fileExtension = Path.GetExtension(filePath).ToLower();

                        Message($"Exporting VM data to: {filePath} (Format: {fileExtension})",
                            EventType.Information, 2103);

                        // Show progress cursor
                        this.Cursor = Cursors.WaitCursor;

                        try
                        {
                            // Get VM Groups data
                            var vmGroups = VMGroups.GetHyperVVMGroups(cmd => ExecutePowerShellCommand(cmd));

                            // Export based on file extension
                            bool success = false;
                            switch (fileExtension)
                            {
                                case ".json":
                                    success = ExportToJson(filePath, vmGroups);
                                    break;

                                case ".csv":
                                    success = ExportToCsv(filePath, vmGroups);
                                    break;

                                case ".xml":
                                    success = ExportToXml(filePath, vmGroups);
                                    break;

                                case ".txt":
                                    success = ExportToText(filePath, vmGroups);
                                    break;

                                default:
                                    success = ExportToJson(filePath, vmGroups);
                                    break;
                            }

                            if (success)
                            {
                                Message($"VM data export completed successfully: {filePath}",
                                    EventType.Information, 2104);

                                // Show success message with option to open file location
                                var result = MessageBox.Show(
                                    $"VM data exported successfully to:\n{filePath}\n\nWould you like to open the file location?",
                                    "Export Complete",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Information);

                                if (result == DialogResult.Yes)
                                {
                                    try
                                    {
                                        Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                                    }
                                    catch (Exception ex)
                                    {
                                        Message($"Could not open file location: {ex.Message}",
                                            EventType.Warning, 2105);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            this.Cursor = Cursors.Default;
                        }
                    }
                    else
                    {
                        Message("Export dialog cancelled by user",
                            EventType.Information, 2106);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error exporting VM data: {ex.Message}";
                Message(errorMsg, EventType.Error, 2107);

                MessageBox.Show(errorMsg,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool ExportToJson(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                Message("Exporting as JSON format",
                    EventType.Information, 2108);

                var exportData = new
                {
                    ExportInfo = new
                    {
                        ExportDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        ExportedBy = Environment.UserName,
                        HyperVHost = SessionContext.ServerName,
                        ConnectionType = SessionContext.IsLocal ? "Local" : "Remote",
                        TotalVMs = datagridviewVMOverView.Rows.Count,
                        ApplicationVersion = "HyperView v1.0.0.0"
                    },
                    VMData = GetVMDataFromGrid(),
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
                Message($"Error exporting to JSON: {ex.Message}",
                    EventType.Error, 2109);
                return false;
            }
        }

        private bool ExportToCsv(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                Message("Exporting as CSV format",
                    EventType.Information, 2110);

                var vmData = GetVMDataFromGrid();
                var csv = new System.Text.StringBuilder();

                // Write header
                var headers = new List<string>();
                foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                {
                    headers.Add($"\"{column.HeaderText}\"");
                }
                csv.AppendLine(string.Join(",", headers));

                // Write data rows
                foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                {
                    var values = new List<string>();
                    foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                    {
                        var cellValue = row.Cells[column.Index].Value?.ToString() ?? "";
                        values.Add($"\"{cellValue.Replace("\"", "\"\"")}\"");
                    }
                    csv.AppendLine(string.Join(",", values));
                }

                File.WriteAllText(filePath, csv.ToString(), System.Text.Encoding.UTF8);

                // Also create a separate CSV for VM Groups if available
                if (vmGroups != null && vmGroups.Count > 0)
                {
                    string groupsCsvPath = filePath.Replace(".csv", "_VMGroups.csv");
                    var groupsCsv = new System.Text.StringBuilder();

                    groupsCsv.AppendLine("\"Group Name\",\"Group Type\",\"VM Count\",\"VM Members\",\"Computer Name\"");

                    foreach (var group in vmGroups)
                    {
                        groupsCsv.AppendLine($"\"{group.Name}\",\"{group.GroupTypeDisplay}\",\"{group.VMCount}\",\"{group.VMList}\",\"{group.ComputerName}\"");
                    }

                    File.WriteAllText(groupsCsvPath, groupsCsv.ToString(), System.Text.Encoding.UTF8);

                    Message($"VM Groups data also exported to: {groupsCsvPath}",
                        EventType.Information, 2111);
                }

                return true;
            }
            catch (Exception ex)
            {
                Message($"Error exporting to CSV: {ex.Message}",
                    EventType.Error, 2112);
                return false;
            }
        }

        private bool ExportToXml(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                Message("Exporting as XML format",
                    EventType.Information, 2113);

                var vmData = GetVMDataFromGrid();

                using (var writer = System.Xml.XmlWriter.Create(filePath, new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    Encoding = System.Text.Encoding.UTF8
                }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("HyperVExport");

                    // Export Info
                    writer.WriteStartElement("ExportInfo");
                    writer.WriteElementString("ExportDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    writer.WriteElementString("ExportedBy", Environment.UserName);
                    writer.WriteElementString("HyperVHost", SessionContext.ServerName);
                    writer.WriteElementString("ConnectionType", SessionContext.IsLocal ? "Local" : "Remote");
                    writer.WriteElementString("TotalVMs", datagridviewVMOverView.Rows.Count.ToString());
                    writer.WriteEndElement();

                    // VM Data
                    writer.WriteStartElement("VMData");
                    foreach (var vm in vmData)
                    {
                        writer.WriteStartElement("VM");
                        foreach (var kvp in vm)
                        {
                            writer.WriteElementString(kvp.Key.Replace(" ", ""), kvp.Value);
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // VM Groups
                    if (vmGroups != null && vmGroups.Count > 0)
                    {
                        writer.WriteStartElement("VMGroups");
                        foreach (var group in vmGroups)
                        {
                            writer.WriteStartElement("VMGroup");
                            writer.WriteElementString("Name", group.Name);
                            writer.WriteElementString("GroupType", group.GroupTypeDisplay);
                            writer.WriteElementString("VMCount", group.VMCount.ToString());
                            writer.WriteElementString("VMMembers", group.VMList);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                return true;
            }
            catch (Exception ex)
            {
                Message($"Error exporting to XML: {ex.Message}",
                    EventType.Error, 2114);
                return false;
            }
        }

        private bool ExportToText(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                Message("Exporting as formatted text",
                    EventType.Information, 2115);

                var vmData = GetVMDataFromGrid();
                var textOutput = new List<string>();

                textOutput.Add(new string('=', 80));
                textOutput.Add("HYPERVIEW - ALL VM DATA EXPORT");
                textOutput.Add(new string('=', 80));
                textOutput.Add($"Export Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                textOutput.Add($"Exported By: {Environment.UserName}");
                textOutput.Add($"Hyper-V Host: {SessionContext.ServerName}");
                textOutput.Add($"Connection Type: {(SessionContext.IsLocal ? "Local" : "Remote")}");
                textOutput.Add($"Total VMs: {datagridviewVMOverView.Rows.Count}");
                textOutput.Add("");

                // VM Data
                textOutput.Add("VIRTUAL MACHINES DATA");
                textOutput.Add(new string('-', 80));

                foreach (var vm in vmData)
                {
                    textOutput.Add("");
                    foreach (var kvp in vm)
                    {
                        if (!string.IsNullOrEmpty(kvp.Value))
                            textOutput.Add($"  {kvp.Key}: {kvp.Value}");
                    }
                }

                // VM Groups Data
                if (vmGroups != null && vmGroups.Count > 0)
                {
                    textOutput.Add("");
                    textOutput.Add("");
                    textOutput.Add("VM GROUPS DATA");
                    textOutput.Add(new string('-', 80));

                    foreach (var group in vmGroups)
                    {
                        textOutput.Add("");
                        textOutput.Add($"Group Name: {group.Name}");
                        textOutput.Add($"  Type: {group.GroupTypeDisplay}");
                        textOutput.Add($"  VM Count: {group.VMCount}");
                        textOutput.Add($"  VM Members: {group.VMList}");
                    }
                }

                textOutput.Add("");
                textOutput.Add(new string('=', 80));
                textOutput.Add("END OF EXPORT");
                textOutput.Add(new string('=', 80));

                File.WriteAllLines(filePath, textOutput, System.Text.Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                Message($"Error exporting to text: {ex.Message}",
                    EventType.Error, 2116);
                return false;
            }
        }

        private List<Dictionary<string, string>> GetVMDataFromGrid()
        {
            var vmData = new List<Dictionary<string, string>>();

            foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
            {
                var vmInfo = new Dictionary<string, string>();

                foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                {
                    var value = row.Cells[column.Index].Value?.ToString() ?? "";
                    vmInfo[column.HeaderText] = value;
                }

                vmData.Add(vmInfo);
            }

            return vmData;
        }

        private void buttonManageServerMembers_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User initiated VM Group member management",
                    EventType.Information, 2139);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a VM Group to manage.",
                        "No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string groupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(groupName))
                {
                    MessageBox.Show("Invalid VM Group selection.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Message($"User selected VM Group '{groupName}' for member management",
                    EventType.Information, 2140);

                // Get list of all available VMs from the overview grid
                var allVMs = new List<string>();
                if (datagridviewVMOverView != null)
                {
                    foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                    {
                        var vmName = row.Cells["VM Name"].Value?.ToString();
                        if (!string.IsNullOrEmpty(vmName))
                        {
                            allVMs.Add(vmName);
                        }
                    }
                }

                // Show manage members form
                using (Forms.ManageVMGroupMembers manageForm = new Forms.ManageVMGroupMembers())
                {
                    manageForm.GroupName = groupName;
                    manageForm.AllVMs = allVMs;
                    manageForm.ExecutePowerShellCommand = cmd => ExecutePowerShellCommand(cmd);

                    var result = manageForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        Message($"Member management completed for group '{groupName}', refreshing view...",
                            EventType.Information, 2141);

                        // Refresh the VM Groups view
                        VMGroups.RefreshVMGroupsView(
                            $"Group members updated: {groupName}",
                            cmd => ExecutePowerShellCommand(cmd),
                            groups => UpdateVMGroupsDataGridView(groups));
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error managing VM Group members: {ex.Message}";
                Message(errorMsg, EventType.Error, 2142);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Use the shared confirmation function
            if (ConfirmDisconnectAndExit())
            {
                this.Close();
            }
        }

        private void myWebpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.URLMyWebPage,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.URLMyWebPage + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.URLMyWebPage + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1041);
            }
        }

        private void myBlogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.URLMyBlog,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.URLMyBlog + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.URLMyBlog + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1041);
            }
        }

        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Log the user's action to open the Changelog form
            Message("User clicked the 'Changelog' menu item to open the Changelog form", EventType.Information, 1057);

            // Open the Changelog form
            ChangelogForm f2 = new ChangelogForm();
            f2.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Log the user's action to open the About form
            Message("User clicked the 'About' menu item to open the About form", EventType.Information, 1056);

            // Open the About form
            AboutForm f2 = new AboutForm();
            f2.ShowDialog();
        }

        private void openLogForTodayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open the log file for today
            try
            {
                var logFilePath = FileManager.LogFilePath;
                logFilePath = logFilePath + "\\" + Globals.ToolName.ShortName + " Log " + DateTime.Today.ToString("dd-MM-yyyy") + "." + "log";
                Process.Start(logFilePath);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void openLogFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open the log folder
            try
            {
                var logFolderPath = FileManager.LogFilePath;
                Process.Start("explorer.exe", logFolderPath);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void pictureboxSupportMe_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.URLBuyMeaCoffie,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'Buy me a coffie' picture in MainForm to open the URL: '" + Globals.ToolStings.URLBuyMeaCoffie + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.URLBuyMeaCoffie + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1041);
            }
        }

        private void buttonLoadVMsrefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested VM overview refresh",
                    EventType.Information, 2151);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Message("Starting VM overview refresh...",
                    EventType.Information, 2152);

                // Execute with progress form
                ExecuteWithProgressForm(() =>
                {
                    // Reload VM overview data (runs in background thread, but UI operations need invoke)
                    LoadVMOverview();

                    // Count running VMs from the grid  
                    int totalVMs = 0;
                    int runningVMs = 0;

                    if (datagridviewVMOverView.InvokeRequired)
                    {
                        datagridviewVMOverView.Invoke((Action)(() =>
                        {
                            totalVMs = datagridviewVMOverView.Rows.Count;

                            foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                            {
                                var state = row.Cells["State"].Value?.ToString();
                                if (state == "Running")
                                {
                                    runningVMs++;
                                }
                            }
                        }));
                    }
                    else
                    {
                        totalVMs = datagridviewVMOverView.Rows.Count;

                        foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                        {
                            var state = row.Cells["State"].Value?.ToString();
                            if (state == "Running")
                            {
                                runningVMs++;
                            }
                        }
                    }

                    Message($"VM overview refresh completed - Total: {totalVMs}, Running: {runningVMs}",
                        EventType.Information, 2153);

                    // Show success message (will be invoked on UI thread automatically)
                    if (this.InvokeRequired)
                    {
                        this.Invoke((Action)(() =>
                        {
                            toolStripStatusLabelTextMainForm.Text = "VM overview refreshed successfully. Total VMs: " + totalVMs + ", Running VMs: " + runningVMs;

                            /*MessageBox.Show($"VM overview refreshed successfully.\n\nTotal VMs: {totalVMs}\nRunning VMs: {runningVMs}",
                                "Refresh Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);*/
                        }));
                    }
                    else
                    {
                        // Update statusstrip directly
                        toolStripStatusLabelTextMainForm.Text = "VM overview refreshed successfully. Total VMs: " + totalVMs + ", Running VMs: " + runningVMs;

                        /*MessageBox.Show($"VM overview refreshed successfully.\n\nTotal VMs: {totalVMs}\nRunning VMs: {runningVMs}",
                            "Refresh Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);*/
                    }

                }, "VM Overview Refresh");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing VM overview: {ex.Message}";
                Message(errorMsg, EventType.Error, 2154);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonSummaryhvOverviewView_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested VM summary",
                    EventType.Information, 2155);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have VM data
                if (datagridviewVMOverView == null || datagridviewVMOverView.Rows.Count == 0)
                {
                    MessageBox.Show("No VM data available. Please load VMs first.",
                        "No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                // Calculate VM statistics
                int totalVMs = datagridviewVMOverView.Rows.Count;
                int runningVMs = 0;
                int stoppedVMs = 0;
                int gen1VMs = 0;
                int gen2VMs = 0;
                int totalProcessors = 0;
                long totalMemoryAssignedMB = 0;
                double totalDiskSpaceGB = 0;

                foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                {
                    // Count by state
                    var state = row.Cells["State"].Value?.ToString();
                    if (state == "Running")
                        runningVMs++;
                    else if (state == "Off")
                        stoppedVMs++;

                    // Count by generation
                    var generation = row.Cells["Generation"].Value?.ToString();
                    if (generation == "1")
                        gen1VMs++;
                    else if (generation == "2")
                        gen2VMs++;

                    // Sum processors
                    var cpuCount = row.Cells["CPU Count"].Value?.ToString();
                    if (!string.IsNullOrEmpty(cpuCount) && int.TryParse(cpuCount, out int cpus))
                        totalProcessors += cpus;

                    // Sum memory assigned
                    var memAssigned = row.Cells["Memory Assigned (MB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(memAssigned) && long.TryParse(memAssigned, out long memory))
                        totalMemoryAssignedMB += memory;

                    // Sum disk space
                    var diskSpace = row.Cells["Total Disk (GB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(diskSpace) && double.TryParse(diskSpace, out double disk))
                        totalDiskSpaceGB += disk;
                }

                // Get VM Groups statistics
                Message("Retrieving VM Groups for summary...",
                    EventType.Information, 2156);

                var vmGroups = VMGroups.GetHyperVVMGroups(cmd => ExecutePowerShellCommand(cmd));
                int totalGroups = vmGroups?.Count ?? 0;

                // Count grouped VMs
                var groupedVMs = new HashSet<string>();
                if (vmGroups != null)
                {
                    foreach (var group in vmGroups)
                    {
                        foreach (var vmName in group.VMMembers)
                        {
                            groupedVMs.Add(vmName);
                        }
                    }
                }

                int groupedVMCount = groupedVMs.Count;
                int ungroupedVMCount = totalVMs - groupedVMCount;

                // Get cluster information if connected to a cluster
                string clusterSection = "";
                if (SessionContext.IsCluster)
                {
                    Message("Retrieving cluster information for summary...",
                        EventType.Information, 2163);

                    var clusterInfo = Cluster.GetClusterInformation(cmd => ExecutePowerShellCommand(cmd));

                    if (clusterInfo != null)
                    {
                        clusterSection = $@"

🖥️ Cluster Information:
• Cluster Name: {clusterInfo.ClusterName}
• Current Node: {clusterInfo.CurrentNode}
• Total Nodes: {clusterInfo.Nodes.Count}
• Clustered VMs: {clusterInfo.VirtualMachines.Count}
• Cluster Networks: {clusterInfo.Networks.Count}
• Shared Volumes: {clusterInfo.SharedStorage.Count}";

                        // List all nodes with their states
                        if (clusterInfo.Nodes.Count > 0)
                        {
                            clusterSection += "\n• Cluster Nodes:";
                            foreach (var node in clusterInfo.Nodes)
                            {
                                clusterSection += $"\n  - {node.Name} ({node.State})";
                            }
                        }

                        Message($"Cluster information added to summary - {clusterInfo.Nodes.Count} nodes",
                            EventType.Information, 2164);
                    }
                }

                this.Cursor = Cursors.Default;

                // Create summary message
                string summaryText = $@"VM Overview Summary - {SessionContext.ServerName}:

📊 VM Statistics:
• Total VMs: {totalVMs}
• Running: {runningVMs} | Stopped: {stoppedVMs}
• Generation 1: {gen1VMs} | Generation 2: {gen2VMs}

💾 Resource Allocation:
• Total Processors: {totalProcessors}
• Memory Assigned: {Math.Round(totalMemoryAssignedMB / 1024.0, 1)} GB
• Total Disk Space: {Math.Round(totalDiskSpaceGB, 1)} GB

🗂️ VM Groups:
• Total Groups: {totalGroups}
• Grouped VMs: {groupedVMCount}
• Ungrouped VMs: {ungroupedVMCount}{clusterSection}";

                Message($"VM summary generated - Total VMs: {totalVMs}, Running: {runningVMs}",
                    EventType.Information, 2157);


                // Update status strip
                toolStripStatusLabelTextMainForm.Text = "VM Overview Loaded";

                // Show summary message
                /*MessageBox.Show(summaryText,
                    "VM Overview Loaded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);*/
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;

                string errorMsg = $"Error generating VM summary: {ex.Message}";
                Message(errorMsg, EventType.Error, 2158);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void datagridviewVMOverView_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if a valid row was clicked (not header)
                if (e.RowIndex < 0)
                    return;

                Message($"User double-clicked VM row at index {e.RowIndex}",
                    EventType.Information, 2159);

                // Get the selected row
                DataGridViewRow selectedRow = datagridviewVMOverView.Rows[e.RowIndex];

                // Extract VM data from the row
                string vmName = selectedRow.Cells["VM Name"].Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(vmName))
                {
                    MessageBox.Show("Could not retrieve VM name.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Message($"Showing details for VM: {vmName}",
                    EventType.Information, 2160);

                // Build detailed information string
                string details = $@"VM Details - {vmName}

Basic Information:
• Name: {vmName}
• State: {selectedRow.Cells["State"].Value}
• Generation: {selectedRow.Cells["Generation"].Value}
• Created: {selectedRow.Cells["Created"].Value}
• Is Clustered: {selectedRow.Cells["Is Clustered"].Value}

Performance & Health:
• CPU Count: {selectedRow.Cells["CPU Count"].Value}
• CPU Usage: {selectedRow.Cells["CPU Usage %"].Value}%
• Memory Assigned: {selectedRow.Cells["Memory Assigned (MB)"].Value} MB
• Memory Demand: {selectedRow.Cells["Memory Demand (MB)"].Value} MB
• Memory Startup: {selectedRow.Cells["Memory Startup (MB)"].Value} MB
• Dynamic Memory: {selectedRow.Cells["Dynamic Memory"].Value}
• Heartbeat: {selectedRow.Cells["Heartbeat"].Value}

Storage & Network:
• Total Disk Space: {selectedRow.Cells["Total Disk (GB)"].Value} GB
• Network Adapters: {selectedRow.Cells["Network Adapters"].Value}

Automation & Backup:
• Auto Start: {selectedRow.Cells["Auto Start"].Value}
• Auto Stop: {selectedRow.Cells["Auto Stop"].Value}
• Checkpoint Type: {selectedRow.Cells["Checkpoint Type"].Value}
• Checkpoints: {selectedRow.Cells["Checkpoints"].Value}
• Replication: {selectedRow.Cells["Replication"].Value}

Management:
• Integration Services: {selectedRow.Cells["Integration Services"].Value}
• VM Group(s): {selectedRow.Cells["VM Groups"].Value}
• Categories: {selectedRow.Cells["Categories"].Value}
• Uptime: {selectedRow.Cells["Uptime"].Value}";

                Message($"Displaying VM details dialog for {vmName}",
                    EventType.Information, 2161);

                // Show details in a message box
                MessageBox.Show(details,
                    "VM Details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error showing VM details: {ex.Message}";
                Message(errorMsg, EventType.Error, 2162);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shows detailed cluster information dialog
        /// </summary>
        private void ShowClusterInformation()
        {
            try
            {
                Message("User requested cluster information",
                    EventType.Information, 2165);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if connected to a cluster
                if (!SessionContext.IsCluster)
                {
                    MessageBox.Show($"The connected host '{SessionContext.ServerName}' is not part of a cluster.\n\n" +
                                  "This is a standalone Hyper-V host.",
                        "Not a Cluster",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                Message("Retrieving detailed cluster information...",
                    EventType.Information, 2166);

                // Get cluster information
                var clusterInfo = Cluster.GetClusterInformation(cmd => ExecutePowerShellCommand(cmd));

                this.Cursor = Cursors.Default;

                if (clusterInfo == null)
                {
                    MessageBox.Show("Failed to retrieve cluster information.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Build detailed cluster information message
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Cluster Details - {clusterInfo.ClusterName}");
                details.AppendLine();

                // Basic Information
                details.AppendLine("🖥️ Cluster Overview:");
                details.AppendLine($"• Cluster Name: {clusterInfo.ClusterName}");
                details.AppendLine($"• Current Node: {clusterInfo.CurrentNode}");
                details.AppendLine($"• Total Nodes: {clusterInfo.Nodes.Count}");
                details.AppendLine($"• Clustered VMs: {clusterInfo.VirtualMachines.Count}");
                details.AppendLine();

                // Cluster Nodes
                if (clusterInfo.Nodes.Count > 0)
                {
                    details.AppendLine("📡 Cluster Nodes:");
                    foreach (var node in clusterInfo.Nodes)
                    {
                        details.AppendLine($"• {node.Name}");
                        details.AppendLine($"  - State: {node.State}");
                        details.AppendLine($"  - Node Weight: {node.NodeWeight}");
                        details.AppendLine($"  - Dynamic Weight: {node.DynamicWeight}");
                        if (!string.IsNullOrEmpty(node.FaultDomain))
                            details.AppendLine($"  - Fault Domain: {node.FaultDomain}");
                        details.AppendLine($"  - Drain Status: {node.DrainStatus}");
                        details.AppendLine();
                    }
                }

                // Cluster Networks
                if (clusterInfo.Networks.Count > 0)
                {
                    details.AppendLine("🌐 Cluster Networks:");
                    foreach (var network in clusterInfo.Networks)
                    {
                        details.AppendLine($"• {network.Name}");
                        details.AppendLine($"  - Address: {network.Address}/{network.AddressMask}");
                        details.AppendLine($"  - Role: {network.Role}");
                        details.AppendLine($"  - State: {network.State}");
                        details.AppendLine();
                    }
                }
                else
                {
                    details.AppendLine("🌐 Cluster Networks: None found");
                    details.AppendLine();
                }

                // Shared Storage
                if (clusterInfo.SharedStorage.Count > 0)
                {
                    details.AppendLine("💾 Cluster Shared Volumes:");
                    foreach (var storage in clusterInfo.SharedStorage)
                    {
                        details.AppendLine($"• {storage.Name}");
                        details.AppendLine($"  - Owner Node: {storage.OwnerNode}");
                        details.AppendLine($"  - State: {storage.State}");
                        details.AppendLine();
                    }
                }
                else
                {
                    details.AppendLine("💾 Cluster Shared Volumes: None found");
                    details.AppendLine();
                }

                // Virtual Machines
                if (clusterInfo.VirtualMachines.Count > 0)
                {
                    details.AppendLine("🖥️ Highly Available Virtual Machines:");

                    // Group VMs by owner node
                    var vmsByNode = clusterInfo.VirtualMachines
                        .GroupBy(vm => vm.OwnerNode)
                        .OrderBy(g => g.Key);

                    foreach (var nodeGroup in vmsByNode)
                    {
                        details.AppendLine($"• Node: {nodeGroup.Key} ({nodeGroup.Count()} VMs)");
                        foreach (var vm in nodeGroup.OrderBy(v => v.Name))
                        {
                            details.AppendLine($"  - {vm.Name} ({vm.State})");
                            if (vm.Priority > 0)
                                details.AppendLine($"    Priority: {vm.Priority}");
                            if (!string.IsNullOrEmpty(vm.PreferredOwners))
                                details.AppendLine($"    Preferred Owners: {vm.PreferredOwners}");
                        }
                        details.AppendLine();
                    }
                }
                else
                {
                    details.AppendLine("🖥️ Highly Available Virtual Machines: None found");
                    details.AppendLine();
                }

                Message($"Displaying cluster information for '{clusterInfo.ClusterName}'",
                    EventType.Information, 2167);

                // Show the detailed information
                MessageBox.Show(details.ToString(),
                    $"Cluster Information - {clusterInfo.ClusterName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;

                string errorMsg = $"Error displaying cluster information: {ex.Message}";
                Message(errorMsg, EventType.Error, 2168);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonSummaryClustersOverviewView_Click(object sender, EventArgs e)
        {
            // Show detailed cluster information
            ShowClusterInformation();
        }

        /// <summary>
        /// Handles the Load Hosts/refresh button click event
        /// </summary>
        private void buttonLoadHostsrefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested host details refresh",
                    EventType.Information, 4010);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Message("Retrieving Hyper-V host details...",
                    EventType.Information, 4011);

                // Execute with progress form
                ExecuteWithProgressForm<List<HostDetailsInfo>>(() =>
                {
                    // Get host details (runs in background)
                    return HostDetails.GetHyperVHostDetails(
                        cmd => ExecutePowerShellCommand(cmd),
                        (node, cmd) => ExecutePowerShellCommandOnNode(node, cmd));

                }, (hostDetails) =>
                {
                    // Handle result on UI thread
                    if (hostDetails != null && hostDetails.Count > 0)
                    {
                        Message($"Retrieved details for {hostDetails.Count} host(s), updating DataGridView",
                            EventType.Information, 4012);

                        // Update the DataGridView
                        UpdateHostsDataGridView(hostDetails);

                        string message = hostDetails.Count == 1 ? "Ready - 1 host loaded" : $"Ready - {hostDetails.Count} hosts loaded";
                        toolStripStatusLabelTextMainForm.Text = message;

                        /*MessageBox.Show($"Host details refreshed successfully.\n\nFound {hostDetails.Count} host(s).",
                            "Refresh Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);*/
                    }
                    else
                    {
                        Message("No host details retrieved",
                            EventType.Warning, 4013);
                        toolStripStatusLabelTextMainForm.Text = "No host data available";

                        MessageBox.Show("No host details found or error retrieving details.",
                            "Refresh Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                }, "Host Details Refresh");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing host details: {ex.Message}";
                Message(errorMsg, EventType.Error, 4014);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = "Error loading host details";
            }
        }

        /// <summary>
        /// Updates the hvHosts DataGridView with host details
        /// </summary>
        private void UpdateHostsDataGridView(List<HostDetailsInfo> hostDetails)
        {
            try
            {
                if (datagridviewhvHosts == null)
                {
                    Message("datagridviewhvHosts control not found",
                        EventType.Warning, 4015);
                    return;
                }

                // Clear existing data
                datagridviewhvHosts.DataSource = null;
                datagridviewhvHosts.Rows.Clear();
                datagridviewhvHosts.Columns.Clear();

                if (hostDetails == null || hostDetails.Count == 0)
                {
                    Message("No host details to display",
                        EventType.Information, 4016);
                    return;
                }

                Message($"Updating hvHosts DataGridView with {hostDetails.Count} host(s)",
                    EventType.Information, 4017);

                // Create DataTable with ALL host detail fields
                var dataTable = new DataTable();
                dataTable.Columns.Add("Host Name", typeof(string));
                dataTable.Columns.Add("Cluster Name", typeof(string));
                dataTable.Columns.Add("Node State", typeof(string));
                dataTable.Columns.Add("Domain", typeof(string));
                dataTable.Columns.Add("OS", typeof(string));
                dataTable.Columns.Add("Version", typeof(string));
                dataTable.Columns.Add("Build", typeof(string));
                dataTable.Columns.Add("Boot Time", typeof(string));
                dataTable.Columns.Add("Uptime", typeof(string));
                dataTable.Columns.Add("Time Zone", typeof(string));
                dataTable.Columns.Add("NTP Servers", typeof(string));
                dataTable.Columns.Add("NTP Status", typeof(string));
                dataTable.Columns.Add("License Status", typeof(string));
                dataTable.Columns.Add("License Type", typeof(string));
                dataTable.Columns.Add("Product Key", typeof(string));
                dataTable.Columns.Add("Grace Period", typeof(string));
                dataTable.Columns.Add("License Description", typeof(string));
                dataTable.Columns.Add("Manufacturer", typeof(string));
                dataTable.Columns.Add("Model", typeof(string));
                dataTable.Columns.Add("Serial Number", typeof(string));
                dataTable.Columns.Add("Processor", typeof(string));
                dataTable.Columns.Add("Sockets", typeof(string));
                dataTable.Columns.Add("Cores", typeof(string));
                dataTable.Columns.Add("Logical CPUs", typeof(string));
                dataTable.Columns.Add("Hyper-Threading", typeof(string));
                dataTable.Columns.Add("SLAT Support", typeof(string));
                dataTable.Columns.Add("Total RAM (GB)", typeof(string));
                dataTable.Columns.Add("Used RAM (GB)", typeof(string));
                dataTable.Columns.Add("Free RAM (GB)", typeof(string));
                dataTable.Columns.Add("RAM Usage %", typeof(string));
                dataTable.Columns.Add("Total VMs", typeof(string));
                dataTable.Columns.Add("Running VMs", typeof(string));
                dataTable.Columns.Add("Stopped VMs", typeof(string));
                dataTable.Columns.Add("Virtual Switches", typeof(string));
                dataTable.Columns.Add("External Switches", typeof(string));
                dataTable.Columns.Add("IP Addresses", typeof(string));
                dataTable.Columns.Add("Live Migration", typeof(string));
                dataTable.Columns.Add("Enhanced Session", typeof(string));
                dataTable.Columns.Add("NUMA Spanning", typeof(string));
                dataTable.Columns.Add("VHD Path", typeof(string));
                dataTable.Columns.Add("VM Config Path", typeof(string));

                // Add rows with ALL data
                foreach (var host in hostDetails)
                {
                    var row = dataTable.NewRow();
                    row["Host Name"] = host.HostName;
                    row["Cluster Name"] = host.ClusterName;
                    row["Node State"] = host.NodeState;
                    row["Domain"] = host.Domain;
                    row["OS"] = host.OperatingSystem;
                    row["Version"] = host.OSVersion;
                    row["Build"] = host.BuildNumber;
                    row["Boot Time"] = host.BootTime;
                    row["Uptime"] = host.Uptime;
                    row["Time Zone"] = host.TimeZone;
                    row["NTP Servers"] = host.NtpServers;
                    row["NTP Status"] = host.NtpStatus;
                    row["License Status"] = host.LicenseStatus;
                    row["License Type"] = host.LicenseType;
                    row["Product Key"] = host.ProductKey;
                    row["Grace Period"] = host.GracePeriod;
                    row["License Description"] = host.LicenseDescription;
                    row["Manufacturer"] = host.Manufacturer;
                    row["Model"] = host.Model;
                    row["Serial Number"] = host.SerialNumber;
                    row["Processor"] = host.Processor;
                    row["Sockets"] = host.Sockets.ToString();
                    row["Cores"] = host.Cores.ToString();
                    row["Logical CPUs"] = host.LogicalCPUs.ToString();
                    row["Hyper-Threading"] = host.HyperThreading;
                    row["SLAT Support"] = host.SLATSupport;
                    row["Total RAM (GB)"] = host.TotalMemoryGB.ToString("F2");
                    row["Used RAM (GB)"] = host.UsedMemoryGB.ToString("F2");
                    row["Free RAM (GB)"] = host.FreeMemoryGB.ToString("F2");
                    row["RAM Usage %"] = host.MemoryUsagePercent.ToString("F1");
                    row["Total VMs"] = host.TotalVMs.ToString();
                    row["Running VMs"] = host.RunningVMs.ToString();
                    row["Stopped VMs"] = host.StoppedVMs.ToString();
                    row["Virtual Switches"] = host.VirtualSwitches.ToString();
                    row["External Switches"] = host.ExternalSwitches.ToString();
                    row["IP Addresses"] = host.IPAddresses;
                    row["Live Migration"] = host.LiveMigration;
                    row["Enhanced Session"] = host.EnhancedSession;
                    row["NUMA Spanning"] = host.NUMASpanning;
                    row["VHD Path"] = host.VHDPath;
                    row["VM Config Path"] = host.VMConfigPath;

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewhvHosts.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewhvHosts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewhvHosts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewhvHosts.MultiSelect = false;
                datagridviewhvHosts.ReadOnly = true;
                datagridviewhvHosts.AllowUserToAddRows = false;
                datagridviewhvHosts.AllowUserToDeleteRows = false;
                datagridviewhvHosts.RowHeadersVisible = false;
                datagridviewhvHosts.AllowUserToResizeRows = false;

                // Apply alternating row colors
                foreach (DataGridViewRow row in datagridviewhvHosts.Rows)
                {
                    if (row.Index % 2 == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.AliceBlue;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                    }
                }

                // Set minimum column widths for key columns
                if (datagridviewhvHosts.Columns.Contains("Host Name"))
                    datagridviewhvHosts.Columns["Host Name"].MinimumWidth = 120;
                if (datagridviewhvHosts.Columns.Contains("Cluster Name"))
                    datagridviewhvHosts.Columns["Cluster Name"].MinimumWidth = 100;
                if (datagridviewhvHosts.Columns.Contains("Processor"))
                    datagridviewhvHosts.Columns["Processor"].MinimumWidth = 200;

                Message($"hvHosts DataGridView updated successfully with {hostDetails.Count} host(s)",
                    EventType.Information, 4018);
            }
            catch (Exception ex)
            {
                Message($"Error updating hvHosts DataGridView: {ex.Message}",
                    EventType.Error, 4019);
            }
        }

        /// <summary>
        /// Loads and displays cluster information in the hvClusters tab
        /// </summary>
        private void LoadClusterInformationView()
        {
            try
            {
                Message("User requested cluster information view refresh",
                    EventType.Information, 4020);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if connected to a cluster
                if (!SessionContext.IsCluster)
                {
                    labelClusterNameValue.Text = "Not a Cluster";
                    labelClusterNameValue.ForeColor = Color.Gray;
                    labelTotalNodesValue.Text = "-";
                    labelCurrentNodeValue.Text = "-";
                    labelClusterNetworksValue.Text = "-";
                    labelSharedVolumesValue.Text = "-";
                    labelClusterNodes.Text = "Cluster Nodes (0 total)";
                    labelClusterVMs.Text = "Highly Available VMs (0 VMs)";

                    MessageBox.Show($"The connected host '{SessionContext.ServerName}' is not part of a cluster.\n\n" +
                                  "This is a standalone Hyper-V host.",
                        "Not a Cluster",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                toolStripStatusLabelTextMainForm.Text = "Loading cluster information...";

                Message("Retrieving detailed cluster information...",
                    EventType.Information, 4021);

                // Get cluster information
                var clusterInfo = Cluster.GetClusterInformation(cmd => ExecutePowerShellCommand(cmd));

                if (clusterInfo == null)
                {
                    MessageBox.Show("Failed to retrieve cluster information.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Update cluster info labels
                labelClusterNameValue.Text = clusterInfo.ClusterName;
                labelClusterNameValue.ForeColor = Color.DarkBlue;
                labelTotalNodesValue.Text = clusterInfo.Nodes.Count.ToString();
                labelCurrentNodeValue.Text = clusterInfo.CurrentNode;
                labelClusterNetworksValue.Text = clusterInfo.Networks.Count.ToString();
                labelSharedVolumesValue.Text = clusterInfo.SharedStorage.Count.ToString();

                // Update Cluster Nodes DataGridView
                labelClusterNodes.Text = $"Cluster Nodes ({clusterInfo.Nodes.Count} total)";
                UpdateClusterNodesDataGridView(clusterInfo.Nodes);

                // Update Cluster VMs DataGridView
                labelClusterVMs.Text = $"Highly Available VMs ({clusterInfo.VirtualMachines.Count} VMs)";
                UpdateClusterVMsDataGridView(clusterInfo.VirtualMachines);

                toolStripStatusLabelTextMainForm.Text = $"Cluster info loaded - {clusterInfo.Nodes.Count} nodes, {clusterInfo.VirtualMachines.Count} VMs";

                Message($"Cluster information loaded successfully - {clusterInfo.Nodes.Count} nodes, {clusterInfo.VirtualMachines.Count} VMs",
                    EventType.Information, 4022);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading cluster information: {ex.Message}";
                Message(errorMsg, EventType.Error, 4023);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = "Error loading cluster information";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Updates the Cluster Nodes DataGridView
        /// </summary>
        private void UpdateClusterNodesDataGridView(List<ClusterNodeInfo> nodes)
        {
            try
            {
                // Clear existing data
                datagridviewClusterNodes.DataSource = null;
                datagridviewClusterNodes.Rows.Clear();
                datagridviewClusterNodes.Columns.Clear();

                if (nodes == null || nodes.Count == 0)
                {
                    return;
                }

                // Create DataTable
                var dataTable = new DataTable();
                dataTable.Columns.Add("Node Name", typeof(string));
                dataTable.Columns.Add("State", typeof(string));
                dataTable.Columns.Add("Node Weight", typeof(string));
                dataTable.Columns.Add("Dynamic Weight", typeof(string));
                dataTable.Columns.Add("Drain Status", typeof(string));
                dataTable.Columns.Add("Fault Domain", typeof(string));

                // Add rows
                foreach (var node in nodes)
                {
                    var row = dataTable.NewRow();
                    row["Node Name"] = node.Name;
                    row["State"] = node.State;
                    row["Node Weight"] = node.NodeWeight.ToString();
                    row["Dynamic Weight"] = node.DynamicWeight.ToString();
                    row["Drain Status"] = node.DrainStatus;
                    row["Fault Domain"] = string.IsNullOrEmpty(node.FaultDomain) ? "N/A" : node.FaultDomain;

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewClusterNodes.DataSource = dataTable;
                datagridviewClusterNodes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewClusterNodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewClusterNodes.RowHeadersVisible = false;

                // Color code node state
                foreach (DataGridViewRow row in datagridviewClusterNodes.Rows)
                {
                    var state = row.Cells["State"].Value?.ToString();
                    if (state == "Up")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightGreen;
                        row.Cells["State"].Style.ForeColor = Color.DarkGreen;
                    }
                    else if (state == "Down")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightCoral;
                        row.Cells["State"].Style.ForeColor = Color.DarkRed;
                    }
                    else if (state == "Paused")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightYellow;
                        row.Cells["State"].Style.ForeColor = Color.DarkOrange;
                    }
                }
            }
            catch (Exception ex)
            {
                Message($"Error updating cluster nodes DataGridView: {ex.Message}",
                    EventType.Error, 4024);
            }
        }

        /// <summary>
        /// Updates the Cluster VMs DataGridView
        /// </summary>
        private void UpdateClusterVMsDataGridView(List<ClusterGroupInfo> virtualMachines)
        {
            try
            {
                // Clear existing data
                datagridviewClusterVMs.DataSource = null;
                datagridviewClusterVMs.Rows.Clear();
                datagridviewClusterVMs.Columns.Clear();

                if (virtualMachines == null || virtualMachines.Count == 0)
                {
                    return;
                }

                // Create DataTable
                var dataTable = new DataTable();
                dataTable.Columns.Add("VM Name", typeof(string));
                dataTable.Columns.Add("Owner Node", typeof(string));
                dataTable.Columns.Add("State", typeof(string));
                dataTable.Columns.Add("Priority", typeof(string));
                dataTable.Columns.Add("Preferred Owners", typeof(string));

                // Add rows
                foreach (var vm in virtualMachines.OrderBy(v => v.OwnerNode).ThenBy(v => v.Name))
                {
                    var row = dataTable.NewRow();
                    row["VM Name"] = vm.Name;
                    row["Owner Node"] = vm.OwnerNode;
                    row["State"] = vm.State;
                    row["Priority"] = vm.Priority > 0 ? vm.Priority.ToString() : "Default";
                    row["Preferred Owners"] = string.IsNullOrEmpty(vm.PreferredOwners) ? "Any" : vm.PreferredOwners;

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewClusterVMs.DataSource = dataTable;
                datagridviewClusterVMs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewClusterVMs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewClusterVMs.RowHeadersVisible = false;

                // Color code VM state
                foreach (DataGridViewRow row in datagridviewClusterVMs.Rows)
                {
                    var state = row.Cells["State"].Value?.ToString();
                    if (state == "Online")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightGreen;
                        row.Cells["State"].Style.ForeColor = Color.DarkGreen;
                    }
                    else if (state == "Offline")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightCoral;
                        row.Cells["State"].Style.ForeColor = Color.DarkRed;
                    }
                    else if (state == "Pending")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightYellow;
                        row.Cells["State"].Style.ForeColor = Color.DarkOrange;
                    }
                }

                // Set VM Name column wider
                if (datagridviewClusterVMs.Columns.Contains("VM Name"))
                    datagridviewClusterVMs.Columns["VM Name"].MinimumWidth = 200;
            }
            catch (Exception ex)
            {
                Message($"Error updating cluster VMs DataGridView: {ex.Message}",
                    EventType.Error, 4025);
            }
        }

        /// <summary>
        /// Handles the Load Cluster/refresh button click event
        /// </summary>
        private void buttonRefreshClusterInfoUI_Click(object sender, EventArgs e)
        {
            LoadClusterInformationView();
        }

        /// <summary>
        /// Loads and displays virtual disk information in the hvDisks tab
        /// </summary>
        public void LoadVirtualDiskOverview()
        {
            try
            {
                Message("User requested virtual disk overview refresh",
                    EventType.Information, 5030);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                toolStripStatusLabelTextMainForm.Text = "Loading virtual disk information...";

                Message("Retrieving virtual disk details...",
                    EventType.Information, 5031);

                // Get virtual disk details
                List<VirtualDiskInfo> diskDetails;

                if (SessionContext.IsCluster && !SessionContext.IsLocal)
                {
                    // Cluster environment - get disks from all nodes
                    diskDetails = VirtualDisks.GetVirtualDiskDetails(
                        cmd => ExecutePowerShellCommand(cmd),
                        (node, cmd) => ExecutePowerShellCommandOnNode(node, cmd));
                }
                else
                {
                    // Single host or local
                    diskDetails = VirtualDisks.GetVirtualDiskDetails(
                        cmd => ExecutePowerShellCommand(cmd));
                }

                if (diskDetails != null && diskDetails.Count > 0)
                {
                    Message($"Retrieved {diskDetails.Count} virtual disk(s), updating DataGridView",
                        EventType.Information, 5032);

                    UpdateVirtualDisksDataGridView(diskDetails);

                    toolStripStatusLabelTextMainForm.Text = $"Loaded {diskDetails.Count} virtual disk(s)";

                    Message($"Virtual disk overview loaded successfully with {diskDetails.Count} disk(s)",
                        EventType.Information, 5033);
                }
                else
                {
                    Message("No virtual disks found",
                        EventType.Warning, 5034);

                    toolStripStatusLabelTextMainForm.Text = "No virtual disks found";
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading virtual disk overview: {ex.Message}";
                Message(errorMsg, EventType.Error, 5035);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = "Error loading virtual disk overview";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Updates the datagridviewvDiskOverView DataGridView with virtual disk details
        /// </summary>
        private void UpdateVirtualDisksDataGridView(List<VirtualDiskInfo> diskDetails)
        {
            try
            {
                if (datagridviewvDiskOverView == null)
                {
                    Message("datagridviewvDiskOverView control not found",
                        EventType.Warning, 5036);
                    return;
                }

                // Clear existing data
                datagridviewvDiskOverView.DataSource = null;
                datagridviewvDiskOverView.Rows.Clear();
                datagridviewvDiskOverView.Columns.Clear();

                if (diskDetails == null || diskDetails.Count == 0)
                {
                    Message("No virtual disk details to display",
                        EventType.Information, 5037);
                    return;
                }

                Message($"Updating datagridviewvDiskOverView with {diskDetails.Count} virtual disk(s)",
                    EventType.Information, 5038);

                // Create DataTable with comprehensive columns (Hyper-V equivalents of VMware disk info)
                var dataTable = new DataTable();

                // VM Information
                dataTable.Columns.Add("VM Name", typeof(string));
                dataTable.Columns.Add("VM State", typeof(string));
                dataTable.Columns.Add("VM Generation", typeof(string));
                dataTable.Columns.Add("VM ID", typeof(string));
                dataTable.Columns.Add("VM Notes", typeof(string));

                // Disk Information
                dataTable.Columns.Add("Disk Name", typeof(string));
                dataTable.Columns.Add("Disk Path", typeof(string));
                dataTable.Columns.Add("Disk Type", typeof(string)); // Dynamic/Fixed/Differencing
                dataTable.Columns.Add("Disk Format", typeof(string)); // VHD/VHDX
                dataTable.Columns.Add("Max Size (GB)", typeof(string));
                dataTable.Columns.Add("File Size (GB)", typeof(string));
                dataTable.Columns.Add("Used Space (GB)", typeof(string));
                dataTable.Columns.Add("Fragmentation %", typeof(string));
                dataTable.Columns.Add("Physical Sector Size", typeof(string));
                dataTable.Columns.Add("Logical Sector Size", typeof(string));
                dataTable.Columns.Add("Block Size", typeof(string));

                // Controller Information
                dataTable.Columns.Add("Controller Type", typeof(string)); // IDE/SCSI
                dataTable.Columns.Add("Controller #", typeof(string));
                dataTable.Columns.Add("Controller Location", typeof(string));
                dataTable.Columns.Add("Attachment Type", typeof(string)); // VHD/Physical

                // Advanced Disk Properties
                dataTable.Columns.Add("Shared", typeof(string));
                dataTable.Columns.Add("Read Only", typeof(string));
                dataTable.Columns.Add("Clustered", typeof(string));
                dataTable.Columns.Add("Persistent Reservations", typeof(string));

                // QoS Information
                dataTable.Columns.Add("QoS Policy ID", typeof(string));
                dataTable.Columns.Add("QoS Min IOPS", typeof(string));
                dataTable.Columns.Add("QoS Max IOPS", typeof(string));

                // Differencing Disk
                dataTable.Columns.Add("Parent Path", typeof(string));
                dataTable.Columns.Add("Disk Identifier", typeof(string));

                // Environment Information
                dataTable.Columns.Add("Cluster Name", typeof(string));
                dataTable.Columns.Add("Current Host", typeof(string));
                dataTable.Columns.Add("VM Path", typeof(string));
                dataTable.Columns.Add("Config Location", typeof(string));
                dataTable.Columns.Add("Snapshot Location", typeof(string));
                dataTable.Columns.Add("Smart Paging Path", typeof(string));

                // OS Information
                dataTable.Columns.Add("Guest OS Type", typeof(string));

                // Add rows with all disk data
                foreach (var disk in diskDetails)
                {
                    var row = dataTable.NewRow();

                    // VM Information
                    row["VM Name"] = disk.VMName;
                    row["VM State"] = disk.VMState;
                    row["VM Generation"] = disk.VMGeneration;
                    row["VM ID"] = disk.VMId;
                    row["VM Notes"] = disk.VMNotes;

                    // Disk Information
                    row["Disk Name"] = disk.DiskName;
                    row["Disk Path"] = disk.DiskPath;
                    row["Disk Type"] = disk.DiskType;
                    row["Disk Format"] = disk.DiskFormat;
                    row["Max Size (GB)"] = disk.MaxSizeGB > 0 ? disk.MaxSizeGB.ToString("F2") : "";
                    row["File Size (GB)"] = disk.FileSizeGB > 0 ? disk.FileSizeGB.ToString("F2") : "";
                    row["Used Space (GB)"] = disk.UsedSpaceGB > 0 ? disk.UsedSpaceGB.ToString("F2") : "";
                    row["Fragmentation %"] = disk.FragmentationPercent;
                    row["Physical Sector Size"] = disk.PhysicalSectorSizeBytes > 0 ? disk.PhysicalSectorSizeBytes.ToString() : "";
                    row["Logical Sector Size"] = disk.LogicalSectorSizeBytes > 0 ? disk.LogicalSectorSizeBytes.ToString() : "";
                    row["Block Size"] = disk.BlockSizeBytes > 0 ? disk.BlockSizeBytes.ToString() : "";

                    // Controller Information
                    row["Controller Type"] = disk.ControllerType;
                    row["Controller #"] = disk.ControllerNumber.ToString();
                    row["Controller Location"] = disk.ControllerLocation.ToString();
                    row["Attachment Type"] = disk.AttachmentType;

                    // Advanced Disk Properties
                    row["Shared"] = disk.IsShared ? "Yes" : "No";
                    row["Read Only"] = disk.IsReadOnly ? "Yes" : "No";
                    row["Clustered"] = disk.IsClustered ? "Yes" : "No";
                    row["Persistent Reservations"] = disk.SupportPersistentReservations;

                    // QoS Information
                    row["QoS Policy ID"] = disk.QoSPolicyId;
                    row["QoS Min IOPS"] = disk.QoSMinimumIOPS;
                    row["QoS Max IOPS"] = disk.QoSMaximumIOPS;

                    // Differencing Disk
                    row["Parent Path"] = disk.ParentPath;
                    row["Disk Identifier"] = disk.DiskIdentifier;

                    // Environment Information
                    row["Cluster Name"] = disk.ClusterName;
                    row["Current Host"] = disk.CurrentHost;
                    row["VM Path"] = disk.VMPath;
                    row["Config Location"] = disk.ConfigurationLocation;
                    row["Snapshot Location"] = disk.SnapshotFileLocation;
                    row["Smart Paging Path"] = disk.SmartPagingFilePath;

                    // OS Information
                    row["Guest OS Type"] = disk.GuestOSType;

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewvDiskOverView.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewvDiskOverView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewvDiskOverView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewvDiskOverView.MultiSelect = false;
                datagridviewvDiskOverView.ReadOnly = true;
                datagridviewvDiskOverView.AllowUserToAddRows = false;
                datagridviewvDiskOverView.AllowUserToDeleteRows = false;
                datagridviewvDiskOverView.RowHeadersVisible = false;
                datagridviewvDiskOverView.AllowUserToResizeRows = false;

                // Apply alternating row colors
                foreach (DataGridViewRow row in datagridviewvDiskOverView.Rows)
                {
                    if (row.Index % 2 == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.AliceBlue;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                    }
                }

                // Color code VM State
                foreach (DataGridViewRow row in datagridviewvDiskOverView.Rows)
                {
                    var state = row.Cells["VM State"].Value?.ToString();
                    if (state == "Running")
                    {
                        row.Cells["VM State"].Style.BackColor = Color.LightGreen;
                        row.Cells["VM State"].Style.ForeColor = Color.DarkGreen;
                    }
                    else if (state == "Off")
                    {
                        row.Cells["VM State"].Style.BackColor = Color.LightGray;
                        row.Cells["VM State"].Style.ForeColor = Color.DarkSlateGray;
                    }
                    else if (state == "Paused" || state == "Saved")
                    {
                        row.Cells["VM State"].Style.BackColor = Color.LightYellow;
                        row.Cells["VM State"].Style.ForeColor = Color.DarkOrange;
                    }
                }

                // Color code Disk Type
                foreach (DataGridViewRow row in datagridviewvDiskOverView.Rows)
                {
                    var diskType = row.Cells["Disk Type"].Value?.ToString();
                    if (diskType == "Dynamic")
                    {
                        row.Cells["Disk Type"].Style.BackColor = Color.LightGreen;
                    }
                    else if (diskType == "Fixed")
                    {
                        row.Cells["Disk Type"].Style.BackColor = Color.LightBlue;
                    }
                    else if (diskType == "Differencing")
                    {
                        row.Cells["Disk Type"].Style.BackColor = Color.LightYellow;
                    }
                    else if (diskType == "PassThrough")
                    {
                        row.Cells["Disk Type"].Style.BackColor = Color.LightCoral;
                    }
                }

                // Set minimum column widths for key columns
                if (datagridviewvDiskOverView.Columns.Contains("VM Name"))
                    datagridviewvDiskOverView.Columns["VM Name"].MinimumWidth = 150;
                if (datagridviewvDiskOverView.Columns.Contains("Disk Name"))
                    datagridviewvDiskOverView.Columns["Disk Name"].MinimumWidth = 200;
                if (datagridviewvDiskOverView.Columns.Contains("Disk Path"))
                    datagridviewvDiskOverView.Columns["Disk Path"].MinimumWidth = 300;

                Message($"datagridviewvDiskOverView updated successfully with {diskDetails.Count} virtual disk(s)",
                    EventType.Information, 5039);
            }
            catch (Exception ex)
            {
                Message($"Error updating datagridviewvDiskOverView: {ex.Message}",
                    EventType.Error, 5040);
            }
        }

        private void buttonLoadvDiskrefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested virtual disk overview refresh",
                    EventType.Information, 5041);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Update status label on UI thread before starting
                toolStripStatusLabelTextMainForm.Text = "Loading virtual disk information...";

                // Execute with progress form
                ExecuteWithProgressForm<List<VirtualDiskInfo>>(() =>
                {
                    // Get virtual disk details (runs in background thread)
                    Message("Retrieving virtual disk details...",
                        EventType.Information, 5031);

                    List<VirtualDiskInfo> diskDetails;

                    if (SessionContext.IsCluster && !SessionContext.IsLocal)
                    {
                        // Cluster environment - get disks from all nodes
                        diskDetails = VirtualDisks.GetVirtualDiskDetails(
                            cmd => ExecutePowerShellCommand(cmd),
                            (node, cmd) => ExecutePowerShellCommandOnNode(node, cmd));
                    }
                    else
                    {
                        // Single host or local
                        diskDetails = VirtualDisks.GetVirtualDiskDetails(
                            cmd => ExecutePowerShellCommand(cmd));
                    }

                    return diskDetails;

                }, (diskDetails) =>
                {
                    // Handle result on UI thread
                    try
                    {
                        if (diskDetails != null && diskDetails.Count > 0)
                        {
                            Message($"Retrieved {diskDetails.Count} virtual disk(s), updating DataGridView",
                                EventType.Information, 5032);

                            // Update DataGridView on UI thread
                            UpdateVirtualDisksDataGridView(diskDetails);

                            toolStripStatusLabelTextMainForm.Text = $"Loaded {diskDetails.Count} virtual disk(s)";

                            Message($"Virtual disk overview loaded successfully with {diskDetails.Count} disk(s)",
                                EventType.Information, 5033);

                            // Show success message
                            int diskCount = diskDetails.Count;

                            // Update status label
                            toolStripStatusLabelTextMainForm.Text = $"Virtual disk overview refreshed - {diskCount} disk(s) found";

                            /*MessageBox.Show($"Virtual disk overview refreshed successfully.\n\nFound {diskCount} virtual disk(s).",
                                "Refresh Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);*/
                        }
                        else
                        {
                            Message("No virtual disks found",
                                EventType.Warning, 5034);

                            toolStripStatusLabelTextMainForm.Text = "No virtual disks found";

                            MessageBox.Show("No virtual disks found.",
                                "Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Error updating virtual disk display: {ex.Message}";
                        Message(errorMsg, EventType.Error, 5046);

                        MessageBox.Show(errorMsg,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        toolStripStatusLabelTextMainForm.Text = "Error displaying virtual disk data";
                    }

                }, "Virtual Disk Overview Refresh");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing virtual disk overview: {ex.Message}";
                Message(errorMsg, EventType.Error, 5042);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = "Error loading virtual disks";
            }
        }

        private void buttonSummaryvDiskView_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested virtual disk summary",
                    EventType.Information, 5043);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have disk data
                if (datagridviewvDiskOverView == null || datagridviewvDiskOverView.Rows.Count == 0)
                {
                    MessageBox.Show("No virtual disk data available. Please load disks first.",
                        "No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                // Calculate disk statistics
                int totalDisks = datagridviewvDiskOverView.Rows.Count;
                double totalMaxSizeGB = 0;
                double totalFileSizeGB = 0;
                double totalUsedSpaceGB = 0;

                // Disk type breakdown
                int dynamicDisks = 0;
                int fixedDisks = 0;
                int differencingDisks = 0;
                int passThroughDisks = 0;

                // Disk format breakdown
                int vhdDisks = 0;
                int vhdxDisks = 0;
                int physicalDisks = 0;

                // Controller breakdown
                int scsiDisks = 0;
                int ideDisks = 0;

                // Advanced features
                int sharedDisks = 0;
                int readOnlyDisks = 0;
                int disksWithQoS = 0;
                int clusteredDisks = 0;

                // VM state breakdown
                int disksOnRunningVMs = 0;
                int disksOnStoppedVMs = 0;

                // Fragmentation tracking
                var fragmentationValues = new List<double>();

                // Unique VMs with disks
                var uniqueVMs = new HashSet<string>();
                var uniqueHosts = new HashSet<string>();

                foreach (DataGridViewRow row in datagridviewvDiskOverView.Rows)
                {
                    // VM tracking
                    var vmName = row.Cells["VM Name"].Value?.ToString();
                    if (!string.IsNullOrEmpty(vmName))
                    {
                        uniqueVMs.Add(vmName);
                    }

                    // Host tracking
                    var hostName = row.Cells["Current Host"].Value?.ToString();
                    if (!string.IsNullOrEmpty(hostName))
                    {
                        uniqueHosts.Add(hostName);
                    }

                    // VM State
                    var vmState = row.Cells["VM State"].Value?.ToString();
                    if (vmState == "Running")
                        disksOnRunningVMs++;
                    else if (vmState == "Off")
                        disksOnStoppedVMs++;

                    // Disk sizes
                    var maxSize = row.Cells["Max Size (GB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(maxSize) && double.TryParse(maxSize, out double maxGB))
                        totalMaxSizeGB += maxGB;

                    var fileSize = row.Cells["File Size (GB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(fileSize) && double.TryParse(fileSize, out double filGB))
                        totalFileSizeGB += filGB;

                    var usedSpace = row.Cells["Used Space (GB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(usedSpace) && double.TryParse(usedSpace, out double usedGB))
                        totalUsedSpaceGB += usedGB;

                    // Disk Type
                    var diskType = row.Cells["Disk Type"].Value?.ToString();
                    switch (diskType)
                    {
                        case "Dynamic":
                            dynamicDisks++;
                            break;
                        case "Fixed":
                            fixedDisks++;
                            break;
                        case "Differencing":
                            differencingDisks++;
                            break;
                        case "PassThrough":
                            passThroughDisks++;
                            break;
                    }

                    // Disk Format
                    var diskFormat = row.Cells["Disk Format"].Value?.ToString();
                    switch (diskFormat)
                    {
                        case "VHD":
                            vhdDisks++;
                            break;
                        case "VHDX":
                            vhdxDisks++;
                            break;
                        case "Physical":
                            physicalDisks++;
                            break;
                    }

                    // Controller Type
                    var controllerType = row.Cells["Controller Type"].Value?.ToString();
                    if (controllerType == "SCSI")
                        scsiDisks++;
                    else if (controllerType == "IDE")
                        ideDisks++;

                    // Advanced features
                    var shared = row.Cells["Shared"].Value?.ToString();
                    if (shared == "Yes")
                        sharedDisks++;

                    var readOnly = row.Cells["Read Only"].Value?.ToString();
                    if (readOnly == "Yes")
                        readOnlyDisks++;

                    var clustered = row.Cells["Clustered"].Value?.ToString();
                    if (clustered == "Yes")
                        clusteredDisks++;

                    var qosMin = row.Cells["QoS Min IOPS"].Value?.ToString();
                    var qosMax = row.Cells["QoS Max IOPS"].Value?.ToString();
                    if (!string.IsNullOrEmpty(qosMin) || !string.IsNullOrEmpty(qosMax))
                        disksWithQoS++;

                    // Fragmentation
                    var fragmentation = row.Cells["Fragmentation %"].Value?.ToString();
                    if (!string.IsNullOrEmpty(fragmentation) && fragmentation != "N/A")
                    {
                        var fragStr = fragmentation.Replace("%", "").Trim();
                        if (double.TryParse(fragStr, out double fragValue))
                        {
                            fragmentationValues.Add(fragValue);
                        }
                    }
                }

                // Calculate averages and percentages
                double avgFragmentation = fragmentationValues.Count > 0
                    ? Math.Round(fragmentationValues.Average(), 1)
                    : 0;

                double spaceEfficiency = totalMaxSizeGB > 0
                    ? Math.Round((totalFileSizeGB / totalMaxSizeGB) * 100, 1)
                    : 0;

                // Get cluster information if available
                string clusterSection = "";
                string clusterName = "N/A";

                // Get cluster name from first row if available
                if (datagridviewvDiskOverView.Rows.Count > 0)
                {
                    var firstRow = datagridviewvDiskOverView.Rows[0];
                    var clusterNameValue = firstRow.Cells["Cluster Name"].Value?.ToString();
                    if (!string.IsNullOrEmpty(clusterNameValue) && clusterNameValue != "N/A")
                    {
                        clusterName = clusterNameValue;
                    }
                }

                if (SessionContext.IsCluster && uniqueHosts.Count > 1)
                {
                    clusterSection = $@"

🖥️ Environment:
• Cluster Name: {clusterName}
• Hosts with Disks: {uniqueHosts.Count}
• Clustered Disks: {clusteredDisks}";
                }

                this.Cursor = Cursors.Default;

                // Create summary message
                string summaryText = $@"Virtual Disk Overview Summary - {SessionContext.ServerName}

📊 Disk Statistics:
• Total Virtual Disks: {totalDisks}
• Unique VMs: {uniqueVMs.Count}
• Disks on Running VMs: {disksOnRunningVMs}
• Disks on Stopped VMs: {disksOnStoppedVMs}

💾 Storage Capacity:
• Total Allocated Space: {Math.Round(totalMaxSizeGB, 1):N1} GB
• Actual File Size: {Math.Round(totalFileSizeGB, 1):N1} GB
• Space Efficiency: {spaceEfficiency}%
• Potential Savings: {Math.Round(totalMaxSizeGB - totalFileSizeGB, 1):N1} GB

📁 Disk Type Breakdown:
• Dynamic: {dynamicDisks}
• Fixed: {fixedDisks}
• Differencing: {differencingDisks}
• PassThrough/Physical: {passThroughDisks}

💿 Disk Format:
• VHDX: {vhdxDisks}
• VHD: {vhdDisks}
• Physical: {physicalDisks}

🔌 Controller Type:
• SCSI: {scsiDisks}
• IDE: {ideDisks}

⚙️ Advanced Features:
• Shared Disks: {sharedDisks}
• Read-Only Disks: {readOnlyDisks}
• Disks with QoS: {disksWithQoS}
• Avg Fragmentation: {avgFragmentation}%{clusterSection}

💡 Recommendations:
{GetDiskRecommendations(dynamicDisks, fixedDisks, spaceEfficiency, avgFragmentation, vhdDisks)}";

                Message($"Virtual disk summary generated - Total Disks: {totalDisks}, Total Size: {totalMaxSizeGB:F1} GB",
                    EventType.Information, 5044);

                // Show summary message
                MessageBox.Show(summaryText,
                    "Virtual Disk Overview Summary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;

                string errorMsg = $"Error generating virtual disk summary: {ex.Message}";
                Message(errorMsg, EventType.Error, 5045);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Generates recommendations based on disk statistics
        /// </summary>
        private string GetDiskRecommendations(int dynamicDisks, int fixedDisks, double spaceEfficiency,
            double avgFragmentation, int vhdDisks)
        {
            var recommendations = new List<string>();

            // Dynamic vs Fixed recommendation
            if (dynamicDisks > fixedDisks * 3)
            {
                recommendations.Add("• Consider using Fixed disks for production VMs for better performance");
            }

            // Space efficiency
            if (spaceEfficiency < 50)
            {
                recommendations.Add($"• Low space efficiency ({spaceEfficiency}%) - consider compacting dynamic disks");
            }

            // Fragmentation
            if (avgFragmentation > 15)
            {
                recommendations.Add($"• High fragmentation detected ({avgFragmentation}%) - consider defragmenting disks");
            }

            // VHD vs VHDX
            if (vhdDisks > 0)
            {
                recommendations.Add($"• {vhdDisks} VHD disk(s) detected - consider migrating to VHDX format");
            }

            return recommendations.Count > 0
                ? string.Join("\n", recommendations)
                : "• Disk configuration looks optimal";
        }        
    }
}
