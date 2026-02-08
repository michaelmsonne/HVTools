using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using HVTools.Helpers;
using static HVTools.Helpers.FileLogger;

namespace HVTools.Forms
{
    public partial class MainForm : Form
    {
        private PSObject _psSession;
        private Runspace? _persistentRunspace;

        private bool _initialLoadComplete;
        private bool _exitConfirmed;

        // Store current health inventory for node switching
        private bool _isLoadingNodeData;
        private string? _currentlyDisplayedNodeName; // Track which node's data is displayed

        public MainForm()
        {
            InitializeComponent();
            InitializeSession();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set initial status (make dynamic based on context if needed when shown later)
            toolStripStatusLabelTextMainForm.Text = @"Loading...";
            // Initial data load will happen in OnShown
        }

        /// <summary>
        /// Performs initial data load with progress form when MainForm is first shown
        /// </summary>
        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Only perform initial load once
            if (_initialLoadComplete)
                return;

            ValidationProgressForm? progressForm = null;
            ManualResetEvent progressFormReady = new ManualResetEvent(false);

            try
            {
                // Create progress form on a separate UI thread to keep it responsive
                var progressThread = new Thread(() =>
                {
                    progressForm = new ValidationProgressForm();
                    progressForm.StartPosition = FormStartPosition.CenterScreen;
                    progressForm.TopMost = true;

                    // Signal that the form is created
                    progressFormReady.Set();

                    // Run message loop for this form on this thread
                    Application.Run(progressForm);
                });

                progressThread.SetApartmentState(ApartmentState.STA);
                progressThread.IsBackground = true;
                progressThread.Start();

                // Wait for progress form to be created and shown
                progressFormReady.WaitOne();
                await Task.Delay(100); // Give it time to render

                Message("Starting initial data load for MainForm", EventType.Information, 2200);

                Exception loadException = null;

                // Load VM Overview in background
                Message("Loading VM Overview data...", EventType.Information, 2201);
                await Task.Run(() =>
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            LoadVmOverview();
                        });
                    }
                    catch (Exception ex)
                    {
                        loadException = ex;
                        Message($"Error loading VM Overview: {ex.Message}", EventType.Error, 2203);
                    }
                });

                // Small delay between operations
                await Task.Delay(100);

                // Load VM Groups in background if no error occurred
                if (loadException == null)
                {
                    Message("Loading VM Groups data...", EventType.Information, 2202);
                    await Task.Run(() =>
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                UpdateVmGroupsDataGridView();
                            });
                        }
                        catch (Exception ex)
                        {
                            loadException = ex;
                            Message($"Error loading VM Groups: {ex.Message}", EventType.Error, 2204);
                        }
                    });
                }

                // Check for errors
                if (loadException != null)
                {
                    MessageBox.Show(
                        $@"Error loading initial data:

{loadException.Message}",
                        @"Data Load Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                _initialLoadComplete = true;
                toolStripStatusLabelTextMainForm.Text = @"Ready";
                Message("Initial data load completed successfully", EventType.Information, 2205);
            }
            catch (Exception ex)
            {
                Message($"Error in OnShown initial load: {ex.Message}", EventType.Error, 2206);
                MessageBox.Show(
                    $@"Error initializing form:

{ex.Message}",
                    @"Initialization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                toolStripStatusLabelTextMainForm.Text = @"Error";
            }
            finally
            {
                // Close the progress form on its own thread
                if (progressForm != null)
                {
                    try
                    {
                        // Use Invoke to close the form on its own thread
                        progressForm.Invoke((MethodInvoker)delegate
                        {
                            progressForm.Close();
                        });
                    }
                    catch
                    {
                        // Ignore disposal errors
                    }
                }

                // Clean up the event
                progressFormReady?.Dispose();
            }
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
                    MessageBox.Show(@"No active session found. Please login again.", @"Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.Cancel;
                    Close();
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
                            MessageBox.Show($@"Failed to create PowerShell session: {error.Exception.Message}",
                                @"Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            DialogResult = DialogResult.Cancel;
                            Close();
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
                string username;
                if (SessionContext.Credentials != null)
                {
                    username = SessionContext.Credentials.UserName;
                    // Ensure domain is included if not already present
                    if (!username.Contains("\\") && !username.Contains("@"))
                    {
                        // For remote connections, get the domain from the remote machine
                        try
                        {
                            Message("Retrieving domain information from remote machine...",
                                EventType.Information, 2240);

                            string getDomainScript = @"
                                $computerSystem = Get-WmiObject -Class Win32_ComputerSystem
                                if ($computerSystem.PartOfDomain) {
                                    # Machine is domain-joined
                                    $computerSystem.Domain
                                } else {
                                    # Machine is in workgroup - use computer name
                                    $env:COMPUTERNAME
                                }
                            ";

                            var domainResult = ExecutePowerShellCommand(getDomainScript);

                            if (domainResult != null && domainResult.Count > 0)
                            {
                                string remoteDomain = domainResult[0].BaseObject?.ToString();
                                if (!string.IsNullOrEmpty(remoteDomain))
                                {
                                    username = $"{remoteDomain}\\{username}";
                                    Message($"Using remote domain/workgroup: {remoteDomain}",
                                        EventType.Information, 2241);
                                }
                                else
                                {
                                    // Fallback: use just the username
                                    Message("Could not determine remote domain, using username only",
                                        EventType.Warning, 2242);
                                }
                            }
                            else
                            {
                                // Fallback: use just the username
                                Message("No domain result from remote machine, using username only",
                                    EventType.Warning, 2243);
                            }
                        }
                        catch (Exception ex)
                        {
                            Message($"Error retrieving remote domain: {ex.Message}",
                                EventType.Warning, 2244);
                            // Fallback: use just the username without domain
                        }
                    }
                }
                else
                {
                    // Local connection - use current user with domain
                    string currentUser = Environment.UserName;
                    string domain = Environment.UserDomainName;

                    if (!string.IsNullOrEmpty(domain) && domain != Environment.MachineName)
                    {
                        username = $"{domain}\\{currentUser}";
                    }
                    else
                    {
                        username = currentUser;
                    }
                }

                string clusterInfo = SessionContext.IsCluster ? " - Cluster" : " - Standalone";

                // Get hostname if connected via IP address
                string? serverDisplay = SessionContext.ServerName;
                if (System.Net.IPAddress.TryParse(SessionContext.ServerName?.Split('.')[0], out _))
                {
                    // Connected via IP - try to get hostname
                    try
                    {
                        Message("Detected IP address connection, retrieving hostname...",
                            EventType.Information, 2237);

                        string getHostnameScript = "$env:COMPUTERNAME";
                        var hostnameResult = ExecutePowerShellCommand(getHostnameScript);

                        if (hostnameResult != null && hostnameResult.Count > 0)
                        {
                            string hostname = hostnameResult[0].BaseObject?.ToString();
                            if (!string.IsNullOrEmpty(hostname))
                            {
                                serverDisplay = $"{SessionContext.ServerName} ({hostname})";
                                Message($"Resolved hostname: {hostname} for IP: {SessionContext.ServerName}",
                                    EventType.Information, 2238);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Message($"Could not retrieve hostname for IP {SessionContext.ServerName}: {ex.Message}",
                            EventType.Warning, 2239);
                    }
                }

                Text = $@"{Globals.ToolName.ShortName} - Connected to {serverDisplay} as {username} ({SessionContext.ConnectionType}{clusterInfo})";
            }
            catch (Exception ex)
            {
                Message($"Failed to initialize session: {ex.Message}",
                    EventType.Error, 2003);
                MessageBox.Show($@"Failed to initialize session: {ex.Message}", @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
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
                        MessageBox.Show($@"Error during {operationName}:

{taskException.Message}",
                            @"Operation Error",
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
                    catch
                    {
                        // ignored
                    }
                }

                MessageBox.Show($@"Error during {operationName}:

{ex.Message}",
                    @"Operation Error",
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
#if DEBUG
                Message($"Starting background operation: {operationName}", EventType.Information, 6006);
#endif
                T result = default;
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
                    if (taskException != null)
                    {
                        MessageBox.Show($@"Error during {operationName}:

{taskException.Message}",
                            @"Operation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else if (onComplete != null)
                    {
                        // Execute completion handler BEFORE closing progress form
                        onComplete(result);
                    }
                }
                catch (Exception ex)
                {
                    Message($"Error in completion handler for '{operationName}': {ex.Message}",
                        EventType.Error, 6008);
                }
                finally
                {
                    // Close progress form AFTER completion handler finishes
                    if (progressForm != null && !progressForm.IsDisposed)
                    {
                        progressForm.Close();
                        progressForm.Dispose();
                        progressForm = null;
                    }
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
                    catch
                    {
                        // ignored
                    }
                }

                MessageBox.Show($@"Error during {operationName}:

{ex.Message}",
                    @"Operation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadVmOverview()
        {
            try
            {
                Message($"Loading VM overview from '{SessionContext.ServerName}' (IsCluster: {SessionContext.IsCluster}, IsLocal: {SessionContext.IsLocal})",
                    EventType.Information, 2170);

                System.Collections.ObjectModel.Collection<PSObject> results;

                // Check if connected to a cluster - use node iteration approach
                if (SessionContext.IsCluster && !SessionContext.IsLocal)
                {
                    Message("Using cluster node iteration to retrieve VMs from all nodes...",
                        EventType.Information, 2171);

                    results = GetClusterVMs();
                }
                else
                {
                    // Standard VM retrieval for standalone or local
                    results = ExecutePowerShellCommand("Get-VM");
                }

                if (results == null || results.Count == 0)
                {
                    MessageBox.Show(@"No VMs found.", @"Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Message($"Retrieved {results.Count} VMs, processing...",
                    EventType.Information, 2172);

                // Update the DataGridView
                UpdateVmOverviewDataGridView(results);

                Message($"VM overview loaded successfully with {results.Count} VMs",
                    EventType.Information, 2173);
            }
            catch (Exception ex)
            {
                Message($"Error loading VM overview: {ex.Message}",
                    EventType.Error, 2006);
                MessageBox.Show($@"Error loading VM overview: {ex.Message}", @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the VM Overview DataGridView with PowerShell results
        /// </summary>
        private void UpdateVmOverviewDataGridView(System.Collections.ObjectModel.Collection<PSObject> results)
        {
            try
            {
                if (results == null || results.Count == 0)
                {
                    Message("No VM results to display",
                        EventType.Warning, 2187);
                    return;
                }

                // Determine if this is cluster collection data
                bool isClusterCollection = results.Count > 0 &&
                    results[0].Properties["TotalDiskSizeGB"] != null &&
                    results[0].Properties["NetworkAdapterCount"] != null &&
                    results[0].Properties["CheckpointCount"] != null;

                // Clear existing data
                datagridviewVMOverView.DataSource = null;
                datagridviewVMOverView.Rows.Clear();
                datagridviewVMOverView.Columns.Clear();

                // Create DataTable with enhanced columns
                var dataTable = new DataTable();
                // Add checkbox column for export selection (first column)
                dataTable.Columns.Add("Export", typeof(bool));
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
                    // Set checkbox to checked by default for all VMs
                    row["Export"] = true;
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
                        var totalDiskGb = vm.Properties["TotalDiskSizeGB"]?.Value;
                        row["Total Disk (GB)"] = totalDiskGb != null ? Convert.ToDouble(totalDiskGb).ToString("F2") : "";

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
                            var totalDiskGb = GetVmTotalDiskSize(vmName);
                            row["Total Disk (GB)"] = totalDiskGb > 0 ? totalDiskGb.ToString("F2") : "";

                            var networkAdapterCount = GetVmNetworkAdapterCount(vmName);
                            row["Network Adapters"] = networkAdapterCount.ToString();

                            var integrationServicesInfo = GetVmIntegrationServices(vmName);
                            row["Integration Services"] = integrationServicesInfo;

                            var checkpointCount = GetVmCheckpointCount(vmName);
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
                        var groups = GetVmGroups(vmName);
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
                datagridviewVMOverView.ReadOnly = false; // Allow checkbox editing
                datagridviewVMOverView.AllowUserToAddRows = false;
                datagridviewVMOverView.AllowUserToDeleteRows = false;
                datagridviewVMOverView.RowHeadersVisible = false;

                // Replace the auto-generated Export column with a proper checkbox column
                int exportColumnIndex = -1;
                for (int i = 0; i < datagridviewVMOverView.Columns.Count; i++)
                {
                    var col = datagridviewVMOverView.Columns[i];
                    if (col.Name == "Export" || col.DataPropertyName == "Export")
                    {
                        exportColumnIndex = i;
                        break;
                    }
                }

                if (exportColumnIndex >= 0)
                {
                    // Remove the auto-generated column
                    datagridviewVMOverView.Columns.RemoveAt(exportColumnIndex);

                    // Create a proper checkbox column with all required properties
                    DataGridViewCheckBoxColumn checkboxColumn = new DataGridViewCheckBoxColumn
                    {
                        Name = "Export",
                        DataPropertyName = "Export",
                        HeaderText = @"☑",
                        Width = 40,
                        ReadOnly = false,
                        TrueValue = true,
                        FalseValue = false,
                        IndeterminateValue = false,
                        ValueType = typeof(bool),
                        SortMode = DataGridViewColumnSortMode.NotSortable,
                        Frozen = false,
                        Resizable = DataGridViewTriState.False
                    };

                    // Insert at the beginning
                    datagridviewVMOverView.Columns.Insert(0, checkboxColumn);
#if DEBUG
                    Message("Export checkbox column created and inserted successfully",
                        EventType.Debug, 2174);
#endif
                }
                else
                {
                    Message("Warning: Export column not found in DataGridView",
                        EventType.Warning, 2175);
                }

                // Make all columns except "Export" read-only
                foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                {
                    if (column.Name != "Export" && column.DataPropertyName != "Export")
                    {
                        column.ReadOnly = true;
                    }
                }

                // Color coding will be applied automatically via DataBindingComplete event
            }
            catch (Exception ex)
            {
                Message($"Error updating VM overview DataGridView: {ex.Message}",
                    EventType.Error, 2188);
                throw;
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
                        Message($"Getting VMs from cluster node {nodeIndex} of {clusterNodes.Count}: '{node}'",
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
                            Message($"Added {nodeVMs.Count} VMs from cluster node: '{node}'",
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
                                // Only log actual error messages (not empty errors from SilentlyContinue)
                                var actualErrors = ps.Streams.Error.Where(e =>
                                    e != null &&
                                    e.Exception != null &&
                                    !string.IsNullOrWhiteSpace(e.Exception.Message))
                                    .ToList();

                                if (actualErrors.Any())
                                {
                                    var errors = string.Join(Environment.NewLine,
                                        actualErrors.Select(e => e.Exception.Message));
                                    Message($"PowerShell command errors: {errors}",
                                        EventType.Error, 2007);
                                    return null;
                                }
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
                            // Only log actual error messages (not empty errors from SilentlyContinue)
                            var actualErrors = ps.Streams.Error.Where(e =>
                                e != null &&
                                e.Exception != null &&
                                !string.IsNullOrWhiteSpace(e.Exception.Message))
                                .ToList();

                            if (actualErrors.Any())
                            {
                                var errors = string.Join(Environment.NewLine,
                                    actualErrors.Select(e => e.Exception.Message));
                                Message($"PowerShell command errors: {errors}",
                                    EventType.Error, 2007);
                                return null;
                            }
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

        private double GetVmTotalDiskSize(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMHardDiskDrive -VMName '{vmName}'");

                if (results == null || results.Count == 0)
                    return 0;

                double totalGb = 0;
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
                                totalGb += size / (1024.0 * 1024.0 * 1024.0);
                            }
                        }
                        else if (File.Exists(path))
                        {
                            var fileInfo = new FileInfo(path);
                            totalGb += fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
                        }
                    }
                }

                return totalGb;
            }
            catch
            {
                return 0;
            }
        }

        private int GetVmNetworkAdapterCount(string vmName)
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

        private string GetVmGroups(string vmName)
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

        private int GetVmCheckpointCount(string vmName)
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

        private string GetVmIntegrationServices(string vmName)
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
                    $@"Are you sure you want to disconnect from Hyper-V (server: '{SessionContext.ServerName}') and close the application?",
                    @"Confirm Exit",
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
            // If exit was already confirmed (e.g., from exit menu), skip confirmation
            if (_exitConfirmed)
            {
                return;
            }

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
                    MessageBox.Show(@"No active connection to disconnect.", @"Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show confirmation dialog
                var confirmResult = MessageBox.Show(
                    $@"Are you sure you want to disconnect from Hyper-V (server: '{SessionContext.ServerName}')?",
                    @"Confirm Disconnect",
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
                    Hide();

                    Message("Showing login form for reconnection...",
                        EventType.Information, 2024);

                    // Show login form for reconnection
                    using (LoginForm loginForm = new LoginForm())
                    {
                        var loginResult = loginForm.ShowDialog();

                        if (loginResult == DialogResult.OK && loginForm.Result != null && loginForm.Result.Success)
                        {
                            // Authentication successful - close current form and open new MainForm
                            Message($"Reconnected successfully to '{loginForm.Result.ServerName}'",
                                EventType.Information, 2025);

                            // Close current MainForm (will trigger cleanup)
                            DialogResult = DialogResult.OK;
                            Close();

                            // The LoginForm will handle showing the new MainForm
                        }
                        else
                        {
                            // Authentication failed or cancelled - close application
                            Message("Authentication cancelled after disconnect - closing application",
                                EventType.Information, 2026);

                            DialogResult = DialogResult.Cancel;
                            Close();
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
                MessageBox.Show($@"Failed to disconnect from Hyper-V:

{ex.Message}",
                    @"Disconnect Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // Show the form again if it was hidden
                Show();
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
                    MessageBox.Show(@"Please connect to a Hyper-V server first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Show the CreateVMGroupForm
                using (CreateVmGroupForm createGroupForm = new CreateVmGroupForm())
                {
                    var result = createGroupForm.ShowDialog();

                    if (result == DialogResult.OK && createGroupForm.Result != null)
                    {
                        string groupName = createGroupForm.Result.GroupName;
                        string groupType = createGroupForm.Result.GroupType;

                        Message($"Creating VM Group '{groupName}' of type '{groupType}'...",
                            EventType.Information, 2030);

                        // Create the VM Group using PowerShell
                        var createResult = VmGroups.CreateHyperVvmGroup(groupName, groupType, cmd => ExecutePowerShellCommand(cmd));

                        if (createResult.Success)
                        {
                            Message($"VM Group '{groupName}' created successfully",
                                EventType.Information, 2031);

                            MessageBox.Show($@"VM Group '{groupName}' created successfully.",
                                @"Group Created",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Refresh VM Groups view
                            VmGroups.RefreshVmGroupsView(
                                $"New group created: {groupName}",
                                cmd => ExecutePowerShellCommand(cmd),
                                groups => UpdateVmGroupsDataGridView(groups));
                        }
                        else
                        {
                            Message($"Failed to create VM Group '{groupName}': {createResult.Error}",
                                EventType.Error, 2032);

                            MessageBox.Show($@"Failed to create VM Group:

{createResult.Error}",
                                @"Error",
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
                    @"Error",
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
                    MessageBox.Show(@"Please connect to a Hyper-V server first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group (assuming you have a datagridviewVMGroups control)
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show(@"Please select a VM Group to delete.",
                        @"No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string selectedGroupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(selectedGroupName))
                {
                    MessageBox.Show(@"Invalid VM Group selection.",
                        @"Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Message($"User selected VM Group '{selectedGroupName}' for deletion",
                    EventType.Information, 2040);

                // First confirmation
                var confirmResult = MessageBox.Show(
                    $@"Are you sure you want to delete VM Group '{selectedGroupName}'?",
                    @"Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    Message("VM Group deletion cancelled by user",
                        EventType.Information, 2041);
                    return;
                }

                // Try to remove without force first
                var result = VmGroups.RemoveHyperVvmGroup(selectedGroupName, false, cmd => ExecutePowerShellCommand(cmd));

                if (result.Success)
                {
                    Message($"VM Group '{selectedGroupName}' deleted successfully",
                        EventType.Information, 2042);

                    MessageBox.Show($@"VM Group '{selectedGroupName}' deleted successfully.",
                        @"Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Refresh the VM Groups view
                    VmGroups.RefreshVmGroupsView(
                        $"Group deleted: {selectedGroupName}",
                        cmd => ExecutePowerShellCommand(cmd),
                        groups => UpdateVmGroupsDataGridView(groups));
                }
                else
                {
                    // Check if it's because the group contains VMs and can be forced
                    if (result.CanForce)
                    {
                        Message($"VM Group '{selectedGroupName}' contains {result.VmCount} VM(s), asking for force deletion",
                            EventType.Information, 2043);

                        string vmList = string.Join("\n• ", result.VmNames);
                        string forceMessage = $"VM Group '{selectedGroupName}' contains {result.VmCount} VM(s):\n\n• {vmList}\n\n" +
                                            "The VMs will remain but will be removed from this group.\n\n" +
                                            "Do you want to force delete the VM Group anyway?";

                        var forceResult = MessageBox.Show(forceMessage,
                            @"Force Delete VM Group?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation);

                        if (forceResult == DialogResult.Yes)
                        {
                            Message($"User confirmed force deletion of VM Group '{selectedGroupName}'",
                                EventType.Information, 2044);

                            // Try again with force
                            var forceDeleteResult = VmGroups.RemoveHyperVvmGroup(selectedGroupName, true, cmd => ExecutePowerShellCommand(cmd));

                            if (forceDeleteResult.Success)
                            {
                                Message($"VM Group '{selectedGroupName}' force deleted successfully",
                                    EventType.Information, 2045);

                                MessageBox.Show($"VM Group '{selectedGroupName}' force deleted successfully. " +
                                              "The VMs remain but are no longer part of this group.",
                                    @"Success",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                                // Refresh the VM Groups view
                                VmGroups.RefreshVmGroupsView(
                                    $"Group force deleted: {selectedGroupName}",
                                    cmd => ExecutePowerShellCommand(cmd),
                                    groups => UpdateVmGroupsDataGridView(groups));
                            }
                            else
                            {
                                Message($"Failed to force delete VM Group '{selectedGroupName}': {forceDeleteResult.Error}",
                                    EventType.Error, 2046);

                                MessageBox.Show($@"Failed to force delete VM Group:

{forceDeleteResult.Error}",
                                    @"Error",
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

                        MessageBox.Show($@"Failed to delete VM Group:

{result.Error}",
                            @"Error",
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
                    @"Error",
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
                    MessageBox.Show(@"Please connect to a Hyper-V server first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show(@"Please select a VM Group to rename.",
                        @"No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string currentGroupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(currentGroupName))
                {
                    MessageBox.Show(@"Invalid VM Group selection.",
                        @"Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Message($"User selected VM Group '{currentGroupName}' for rename",
                    EventType.Information, 2095);

                // Show rename form
                using (RenameVmGroupForm renameForm = new RenameVmGroupForm())
                {
                    renameForm.CurrentGroupName = currentGroupName;

                    var result = renameForm.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrEmpty(renameForm.NewGroupName))
                    {
                        string newGroupName = renameForm.NewGroupName;

                        Message($"Renaming VM Group from '{currentGroupName}' to '{newGroupName}'...",
                            EventType.Information, 2096);

                        // Rename the VM Group using PowerShell
                        var renameResult = VmGroups.RenameHyperVvmGroup(
                            currentGroupName,
                            newGroupName,
                            cmd => ExecutePowerShellCommand(cmd));

                        if (renameResult.Success)
                        {
                            Message($"VM Group renamed successfully from '{currentGroupName}' to '{newGroupName}'",
                                EventType.Information, 2097);

                            MessageBox.Show($@"VM Group renamed successfully from '{currentGroupName}' to '{newGroupName}'.",
                                @"Group Renamed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Refresh VM Groups view
                            VmGroups.RefreshVmGroupsView(
                                $"Group renamed: {currentGroupName} -> {newGroupName}",
                                cmd => ExecutePowerShellCommand(cmd),
                                groups => UpdateVmGroupsDataGridView(groups));
                        }
                        else
                        {
                            Message($"Failed to rename VM Group '{currentGroupName}': {renameResult.Error}",
                                EventType.Error, 2098);

                            MessageBox.Show($@"Failed to rename VM Group:

{renameResult.Error}",
                                @"Error",
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
                    @"Error",
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
                    MessageBox.Show(@"Please connect to a Hyper-V server first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Show progress
                Cursor = Cursors.WaitCursor;

                Message("Retrieving VM Groups from server...",
                    EventType.Information, 2057);

                // Get VM Groups
                var vmGroups = VmGroups.GetHyperVvmGroups(cmd => ExecutePowerShellCommand(cmd));

                if (vmGroups != null)
                {
                    Message($"Retrieved {vmGroups.Count} VM Groups, updating DataGridView",
                        EventType.Information, 2058);

                    // Update DataGridView
                    UpdateVmGroupsDataGridView(vmGroups);

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

                    MessageBox.Show(@"No VM Groups found or error retrieving groups.",
                        @"Refresh Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing VM Groups: {ex.Message}";
                Message(errorMsg, EventType.Error, 2060);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateVmGroupsDataGridView(List<VmGroupInfo> vmGroups)
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
                    row["VM Count"] = group.VmCount.ToString();

                    // Truncate long VM lists
                    string vmMembers = group.VmList;
                    if (!string.IsNullOrEmpty(vmMembers) && vmMembers.Length > 250)
                    {
                        vmMembers = vmMembers.Substring(0, 250) + "...";
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

                // Enforce alphabetic sorting on "Group Name" column
                if (datagridviewVMGroups.Columns.Contains("Group Name"))
                {
                    datagridviewVMGroups.Sort(datagridviewVMGroups.Columns["Group Name"], System.ComponentModel.ListSortDirection.Ascending);
                }

                Message($"VM Groups DataGridView updated successfully with '{vmGroups.Count}' groups",
                    EventType.Information, 2070);
            }
            catch (Exception ex)
            {
                Message($"Error updating VM Groups DataGridView: {ex.Message}",
                    EventType.Error, 2071);
            }
        }

        private void UpdateVmGroupsDataGridView()
        {
            try
            {
                Message("Refreshing VM Groups DataGridView (silent refresh)",
                    EventType.Information, 2083);

                // Get VM Groups without showing message boxes
                var vmGroups = VmGroups.GetHyperVvmGroups(cmd => ExecutePowerShellCommand(cmd));

                if (vmGroups != null)
                {
                    UpdateVmGroupsDataGridView(vmGroups);
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

        }

        private bool ExportToJson(string filePath, List<VmGroupInfo> vmGroups)
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
                        ApplicationVersion = "HVTools v1.0.0.0"
                    },
                    VMData = GetVmDataFromGrid(),
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

        private bool ExportToCsv(string filePath, List<VmGroupInfo> vmGroups)
        {
            try
            {
                Message("Exporting as CSV format",
                    EventType.Information, 2110);

                var vmData = GetVmDataFromGrid();
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
                        groupsCsv.AppendLine($"\"{group.Name}\",\"{group.GroupTypeDisplay}\",\"{group.VmCount}\",\"{group.VmList}\",\"{group.ComputerName}\"");
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

        private bool ExportToXml(string filePath, List<VmGroupInfo> vmGroups)
        {
            try
            {
                Message("Exporting as XML format",
                    EventType.Information, 2113);

                var vmData = GetVmDataFromGrid();

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
                            writer.WriteElementString("VMCount", group.VmCount.ToString());
                            writer.WriteElementString("VMMembers", group.VmList);
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

        private bool ExportToText(string filePath, List<VmGroupInfo> vmGroups)
        {
            try
            {
                Message("Exporting as formatted text",
                    EventType.Information, 2115);

                var vmData = GetVmDataFromGrid();
                var textOutput = new List<string>();

                textOutput.Add(new string('=', 80));
                textOutput.Add("HVTools - ALL VM DATA EXPORT");
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
                        textOutput.Add($"  VM Count: {group.VmCount}");
                        textOutput.Add($"  VM Members: {group.VmList}");
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

        private List<Dictionary<string, string>> GetVmDataFromGrid()
        {
            var vmData = new List<Dictionary<string, string>>();

            // Find the Export column index
            int exportColumnIndex = -1;
            for (int i = 0; i < datagridviewVMOverView.Columns.Count; i++)
            {
                var col = datagridviewVMOverView.Columns[i];
                if (col.Name == "Export" || col.DataPropertyName == "Export" || col.HeaderText == "☑" || col.HeaderText == "☐")
                {
                    exportColumnIndex = i;
#if DEBUG
                    Message($"Validation: Found Export column at index {i}", EventType.Debug, 2232);
#endif
                    break;
                }
            }

            if (exportColumnIndex < 0)
            {
                Message("Export column not found - exporting all VMs", EventType.Warning, 2227);
            }

            int rowIndex = 0;
            foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
            {
                // Check if this row is selected for export
                bool isSelected = true; // Default to true if column doesn't exist

                // Try to find and check the Export column value
                if (exportColumnIndex >= 0 && exportColumnIndex < row.Cells.Count)
                {
                    var cell = row.Cells[exportColumnIndex];
                    var vmName = row.Cells["VM Name"]?.Value?.ToString() ?? $"Row{rowIndex}";

#if DEBUG
                    Message($"Row {rowIndex} ({vmName}): Cell.Value = {cell.Value ?? "NULL"}, Cell.Value Type = {cell.Value?.GetType().Name ?? "NULL"}",
                        EventType.Debug, 2228);
#endif
                    if (cell.Value != null)
                    {
                        // Try to parse the value as boolean
                        if (cell.Value is bool boolValue)
                        {
                            isSelected = boolValue;
                        }
                        else if (cell.Value is int intValue)
                        {
                            // Sometimes checkboxes store 0/1 instead of bool
                            isSelected = intValue != 0;
                        }
                        else if (bool.TryParse(cell.Value.ToString(), out bool parsedValue))
                        {
                            isSelected = parsedValue;
                        }
                        else
                        {
                            // Unknown value type - log it and default to true
                            Message($"Unknown checkbox value type for row {rowIndex}: {cell.Value} ({cell.Value.GetType().Name})",
                                EventType.Warning, 2229);
                        }
                    }
                    else
                    {
                        // Null value - treat as unchecked
                        isSelected = false;
                        Message($"Row {rowIndex} ({vmName}): NULL value - treating as unchecked",
                            EventType.Information, 2230);
                    }

                    Message($"Row {rowIndex} ({vmName}): Export checkbox is {(isSelected ? "CHECKED" : "UNCHECKED")}",
                        EventType.Information, 2224);
                }

                // Skip rows that are not selected for export
                if (!isSelected)
                {
                    Message($"Skipping row {rowIndex} - checkbox unchecked - not selected for export", EventType.Information, 2231);
                    rowIndex++;
                    continue;
                }

                var vmInfo = new Dictionary<string, string>();

                foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                {
                    // Skip the Export checkbox column in the exported data
                    if (column.Name == "Export" || column.DataPropertyName == "Export" || column.HeaderText == "☑" || column.HeaderText == "☐")
                        continue;

                    var value = row.Cells[column.Index].Value?.ToString() ?? "";
                    vmInfo[column.HeaderText] = value;
                }

                vmData.Add(vmInfo);
                rowIndex++;
            }

            Message($"Total VMs selected for export: {vmData.Count} out of {datagridviewVMOverView.Rows.Count} total rows", EventType.Information, 2225);

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
                    MessageBox.Show(@"Please connect to a Hyper-V server first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show(@"Please select a VM Group to manage.",
                        @"No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string groupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(groupName))
                {
                    MessageBox.Show(@"Invalid VM Group selection.",
                        @"Error",
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
                using (ManageVmGroupMembers manageForm = new ManageVmGroupMembers())
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
                        VmGroups.RefreshVmGroupsView(
                            $"Group members updated: {groupName}",
                            cmd => ExecutePowerShellCommand(cmd),
                            groups => UpdateVmGroupsDataGridView(groups));
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error managing VM Group members: {ex.Message}";
                Message(errorMsg, EventType.Error, 2142);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Use the shared confirmation function
            if (ConfirmDisconnectAndExit())
            {
                // Set flag to prevent duplicate confirmation in FormClosing event
                _exitConfirmed = true;
                Close();
            }
        }

        private void myWebpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.UrlMyWebPage,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.UrlMyWebPage + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.UrlMyWebPage + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                    FileName = Globals.ToolStings.UrlMyBlog,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.UrlMyBlog + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.UrlMyBlog + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                    FileName = Globals.ToolStings.UrlBuyMeaCoffie,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'Buy me a coffie' picture in MainForm to open the URL: '" + Globals.ToolStings.UrlBuyMeaCoffie + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.UrlBuyMeaCoffie + @"'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                toolStripStatusLabelTextMainForm.Text = @"Starting VM overview refresh...";

                Message("Starting VM overview refresh...",
                    EventType.Information, 2152);

                // Execute with progress form - use generic version to keep PowerShell work in background
                ExecuteWithProgressForm<System.Collections.ObjectModel.Collection<PSObject>>(() =>
                {
                    // Get VM data with ALL details in background thread (PowerShell work - no UI blocking)
                    Message("Retrieving VM data from server...",
                        EventType.Information, 2152);

                    System.Collections.ObjectModel.Collection<PSObject> results;

                    // Always use GetClusterVMs for data collection (it works for standalone too)
                    // This ensures all PowerShell work is done in the background
                    if (SessionContext.IsCluster && !SessionContext.IsLocal)
                    {
                        Message("Using cluster node iteration to retrieve VMs from all nodes...",
                            EventType.Information, 2171);
                        results = GetClusterVMs();
                    }
                    else
                    {
                        Message("Retrieving VMs with detailed properties...",
                            EventType.Information, 2189);
                        // Use the same detailed collection approach for standalone hosts
                        results = GetDetailedVMsForStandalone();
                    }

                    return results;

                }, (results) =>
                {
                    // Process results on UI thread (UI updates only - NO PowerShell calls)
                    try
                    {
                        if (results == null || results.Count == 0)
                        {
                            MessageBox.Show(@"No VMs found.",
                                @"Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            toolStripStatusLabelTextMainForm.Text = @"No VMs found";
                            return;
                        }

                        Message($"Retrieved {results.Count} VMs, updating UI...",
                            EventType.Information, 2172);

                        // Clear and rebuild the DataGridView on UI thread
                        // This now only updates UI - no PowerShell calls
                        UpdateVmOverviewDataGridView(results);

                        // Count running VMs
                        int totalVMs = datagridviewVMOverView.Rows.Count;
                        int runningVMs = 0;

                        foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                        {
                            var state = row.Cells["State"].Value?.ToString();
                            if (state == "Running")
                            {
                                runningVMs++;
                            }
                        }

                        Message($"VM overview refresh completed - Total: {totalVMs}, Running: {runningVMs}",
                            EventType.Information, 2153);

                        toolStripStatusLabelTextMainForm.Text = $"VM overview refreshed successfully. Total VMs: {totalVMs}, Running VMs: {runningVMs}";
                    }
                    catch (Exception ex)
                    {
                        Message($"Error updating VM overview UI: {ex.Message}",
                            EventType.Error, 2154);

                        MessageBox.Show($@"Error updating VM overview: {ex.Message}",
                            @"Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }

                }, "VM Overview Refresh");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing VM overview: {ex.Message}";
                Message(errorMsg, EventType.Error, 2154);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets detailed VM information for standalone/local hosts
        /// Collects all properties in one pass to avoid multiple PowerShell calls later
        /// </summary>
        private System.Collections.ObjectModel.Collection<PSObject> GetDetailedVMsForStandalone()
        {
            try
            {
                Message("Getting detailed VM information for standalone host...",
                    EventType.Information, 2190);

                // Use the same script as cluster nodes to get all details at once
                string getDetailedVMsScript = @"
                    $vmList = Get-VM -ErrorAction SilentlyContinue
                    foreach ($vm in $vmList) {
# Get all detailed properties
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

                var results = ExecutePowerShellCommand(getDetailedVMsScript);

                Message($"Retrieved {results?.Count ?? 0} VMs with detailed properties",
                    EventType.Information, 2191);

                return results;
            }
            catch (Exception ex)
            {
                Message($"Error getting detailed VMs for standalone host: {ex.Message}",
                    EventType.Error, 2192);
                return null;
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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have VM data
                if (datagridviewVMOverView == null || datagridviewVMOverView.Rows.Count == 0)
                {
                    MessageBox.Show(@"No VM data available. Please load VMs first.",
                        @"No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Calculate VM statistics
                int totalVMs = datagridviewVMOverView.Rows.Count;
                int runningVMs = 0;
                int stoppedVMs = 0;
                int gen1VMs = 0;
                int gen2VMs = 0;
                int totalProcessors = 0;
                long totalMemoryAssignedMb = 0;
                double totalDiskSpaceGb = 0;

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
                        totalMemoryAssignedMb += memory;

                    // Sum disk space
                    var diskSpace = row.Cells["Total Disk (GB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(diskSpace) && double.TryParse(diskSpace, out double disk))
                        totalDiskSpaceGb += disk;
                }

                // Get VM Groups statistics
                Message("Retrieving VM Groups for summary...",
                    EventType.Information, 2156);

                var vmGroups = VmGroups.GetHyperVvmGroups(cmd => ExecutePowerShellCommand(cmd));
                int totalGroups = vmGroups?.Count ?? 0;

                // Count grouped VMs
                var groupedVMs = new HashSet<string>();
                if (vmGroups != null)
                {
                    foreach (var group in vmGroups)
                    {
                        foreach (var vmName in group.VmMembers)
                        {
                            groupedVMs.Add(vmName);
                        }
                    }
                }

                int groupedVmCount = groupedVMs.Count;
                int ungroupedVmCount = totalVMs - groupedVmCount;

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

                Cursor = Cursors.Default;

                // Create summary message
                string summaryText = $@"VM Overview Summary - {SessionContext.ServerName}:

📊 VM Statistics:
• Total VMs: {totalVMs}
• Running: {runningVMs} | Stopped: {stoppedVMs}
• Generation 1: {gen1VMs} | Generation 2: {gen2VMs}

💾 Resource Allocation:
• Total Processors: {totalProcessors}
• Memory Assigned: {Math.Round(totalMemoryAssignedMb / 1024.0, 1)} GB
• Total Disk Space: {Math.Round(totalDiskSpaceGb, 1)} GB

🗂️ VM Groups:
• Total Groups: {totalGroups}
• Grouped VMs: {groupedVmCount}
• Ungrouped VMs: {ungroupedVmCount}{clusterSection}";

                Message($"VM summary generated - Total VMs: {totalVMs}, Running: {runningVMs}",
                    EventType.Information, 2157);


                // Update status strip
                toolStripStatusLabelTextMainForm.Text = @"VM Overview Loaded";

                // Show summary message
                MessageBox.Show(summaryText,
                    @"VM Overview Loaded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;

                string errorMsg = $"Error generating VM summary: {ex.Message}";
                Message(errorMsg, EventType.Error, 2158);

                MessageBox.Show(errorMsg,
                    @"Error",
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
                    MessageBox.Show(@"Could not retrieve VM name.",
                        @"Error",
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
                    $@"VM Details - {vmName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Message($"Error showing VM details: {ex.Message}",
                    EventType.Error, 2162);

                MessageBox.Show($"Error showing VM details: {ex.Message}",
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles clicking on column headers to toggle all Export checkboxes
        /// </summary>
        private void DatagridviewVMOverView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0) return;

                var clickedColumn = datagridviewVMOverView.Columns[e.ColumnIndex];

                // Check if the Export column header was clicked
                if (clickedColumn.Name == "Export" || clickedColumn.DataPropertyName == "Export" || clickedColumn.HeaderText == "☑" || clickedColumn.HeaderText == "☐")
                {
                    Message("User clicked Export column header to toggle all checkboxes",
                        EventType.Information, 2220);

                    // End any pending edits first
                    datagridviewVMOverView.EndEdit();

                    // Determine if we should check or uncheck all
                    // If any are unchecked, check all. If all are checked, uncheck all.
                    int checkedCount = 0;
                    int totalRows = datagridviewVMOverView.Rows.Count;

                    foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                    {
                        var cell = row.Cells[e.ColumnIndex];
                        if (cell.Value != null && cell.Value is bool boolValue && boolValue)
                        {
                            checkedCount++;
                        }
                    }

                    // If all are checked, uncheck all. Otherwise, check all.
                    bool newValue = checkedCount < totalRows;

                    foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                    {
                        row.Cells[e.ColumnIndex].Value = newValue;
                    }

                    // Update header text to reflect state
                    clickedColumn.HeaderText = newValue ? "☑" : "☐";

                    Message($"All VM export checkboxes set to: {(newValue ? "checked" : "unchecked")}",
                        EventType.Information, 2221);

                    // Commit all changes
                    datagridviewVMOverView.EndEdit();
                    datagridviewVMOverView.CurrentCell = null;

                    // Refresh the grid to show changes
                    datagridviewVMOverView.Refresh();
                }
            }
            catch (Exception ex)
            {
                Message($"Error toggling export checkboxes: {ex.Message}",
                    EventType.Error, 2222);
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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if connected to a cluster
                if (!SessionContext.IsCluster)
                {
                    MessageBox.Show($@"The connected host '{SessionContext.ServerName}' is not part of a cluster.

" +
                                  @"This is a standalone Hyper-V host.",
                        @"Not a Cluster",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                Message("Retrieving detailed cluster information...",
                    EventType.Information, 2166);

                // Get cluster information
                var clusterInfo = Cluster.GetClusterInformation(cmd => ExecutePowerShellCommand(cmd));

                if (clusterInfo == null)
                {
                    MessageBox.Show(@"Failed to retrieve cluster information.",
                        @"Error",
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
                    $@"Cluster Information - {clusterInfo.ClusterName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;

                string errorMsg = $"Error displaying cluster information: {ex.Message}";
                Message(errorMsg, EventType.Error, 2168);

                MessageBox.Show(errorMsg,
                    @"Error",
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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                //this.Cursor = Cursors.WaitCursor;
                toolStripStatusLabelTextMainForm.Text = @"Loading Hyper-V host information...";

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

                        toolStripStatusLabelTextMainForm.Text = $@"Host details refreshed - {hostDetails.Count} host(s)";

                        /*MessageBox.Show($"Host details refreshed successfully.\n\nFound {hostDetails.Count} host(s).",
                            "Refresh Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);*/
                    }
                    else
                    {
                        Message("No host details retrieved",
                            EventType.Warning, 4013);
                        toolStripStatusLabelTextMainForm.Text = @"No host data available";

                        MessageBox.Show(@"No host details found or error retrieving details.",
                            @"Refresh Complete",
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
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = @"Error loading host details";
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
                    row["Version"] = host.OsVersion;
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
                    row["Logical CPUs"] = host.LogicalCpUs.ToString();
                    row["Hyper-Threading"] = host.HyperThreading;
                    row["SLAT Support"] = host.SlatSupport;
                    row["Total RAM (GB)"] = host.TotalMemoryGb.ToString("F2");
                    row["Used RAM (GB)"] = host.UsedMemoryGb.ToString("F2");
                    row["Free RAM (GB)"] = host.FreeMemoryGb.ToString("F2");
                    row["RAM Usage %"] = host.MemoryUsagePercent.ToString("F1");
                    row["Total VMs"] = host.TotalVMs.ToString();
                    row["Running VMs"] = host.RunningVMs.ToString();
                    row["Stopped VMs"] = host.StoppedVMs.ToString();
                    row["Virtual Switches"] = host.VirtualSwitches.ToString();
                    row["External Switches"] = host.ExternalSwitches.ToString();
                    row["IP Addresses"] = host.IpAddresses;
                    row["Live Migration"] = host.LiveMigration;
                    row["Enhanced Session"] = host.EnhancedSession;
                    row["NUMA Spanning"] = host.NumaSpanning;
                    row["VHD Path"] = host.VhdPath;
                    row["VM Config Path"] = host.VmConfigPath;

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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if connected to a cluster
                if (!SessionContext.IsCluster)
                {
                    labelClusterNameValue.Text = @"Not a Cluster";
                    labelClusterNameValue.ForeColor = Color.Gray;
                    labelTotalNodesValue.Text = @"-";
                    labelCurrentNodeValue.Text = @"-";
                    labelClusterNetworksValue.Text = @"-";
                    labelSharedVolumesValue.Text = @"-";
                    labelClusterNodes.Text = @"Cluster Nodes (0 total)";
                    labelClusterVMs.Text = @"Highly Available VMs (0 VMs)";

                    MessageBox.Show($@"The connected host '{SessionContext.ServerName}' is not part of a cluster.

" +
                                  @"This is a standalone Hyper-V host.",
                        @"Not a Cluster",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                toolStripStatusLabelTextMainForm.Text = @"Loading cluster information...";

                Message("Retrieving detailed cluster information...",
                    EventType.Information, 4021);

                // Get cluster information
                var clusterInfo = Cluster.GetClusterInformation(cmd => ExecutePowerShellCommand(cmd));

                if (clusterInfo == null)
                {
                    MessageBox.Show(@"Failed to retrieve cluster information.",
                        @"Error",
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
                labelClusterNodes.Text = $@"Cluster Nodes ({clusterInfo.Nodes.Count} total)";
                UpdateClusterNodesDataGridView(clusterInfo.Nodes);

                // Update Cluster VMs DataGridView
                labelClusterVMs.Text = $@"Highly Available VMs ({clusterInfo.VirtualMachines.Count} VMs)";
                UpdateClusterVMsDataGridView(clusterInfo.VirtualMachines);

                toolStripStatusLabelTextMainForm.Text = $@"Cluster info loaded - {clusterInfo.Nodes.Count} nodes, {clusterInfo.VirtualMachines.Count} VMs";

                Message($"Cluster information loaded successfully - {clusterInfo.Nodes.Count} nodes, {clusterInfo.VirtualMachines.Count} VMs",
                    EventType.Information, 4022);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading cluster information: {ex.Message}";
                Message(errorMsg, EventType.Error, 4023);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = @"Error loading cluster information";
            }
            finally
            {
                Cursor = Cursors.Default;
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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                toolStripStatusLabelTextMainForm.Text = @"Loading virtual disk information...";

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

                    toolStripStatusLabelTextMainForm.Text = $@"Loaded {diskDetails.Count} virtual disk(s)";

                    Message($"Virtual disk overview loaded successfully with {diskDetails.Count} disk(s)",
                        EventType.Information, 5033);
                }
                else
                {
                    Message("No virtual disks found",
                        EventType.Warning, 5034);

                    toolStripStatusLabelTextMainForm.Text = @"No virtual disks found";
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading virtual disk overview: {ex.Message}";
                Message(errorMsg, EventType.Error, 5035);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = @"Error loading virtual disk overview";
            }
            finally
            {
                Cursor = Cursors.Default;
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
                    row["VM Name"] = disk.VmName;
                    row["VM State"] = disk.VmState;
                    row["VM Generation"] = disk.VmGeneration;
                    row["VM ID"] = disk.VmId;
                    row["VM Notes"] = disk.VmNotes;

                    // Disk Information
                    row["Disk Name"] = disk.DiskName;
                    row["Disk Path"] = disk.DiskPath;
                    row["Disk Type"] = disk.DiskType;
                    row["Disk Format"] = disk.DiskFormat;
                    row["Max Size (GB)"] = disk.MaxSizeGb > 0 ? disk.MaxSizeGb.ToString("F2") : "";
                    row["File Size (GB)"] = disk.FileSizeGb > 0 ? disk.FileSizeGb.ToString("F2") : "";
                    row["Used Space (GB)"] = disk.UsedSpaceGb > 0 ? disk.UsedSpaceGb.ToString("F2") : "";
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
                    row["QoS Min IOPS"] = disk.QoSMinimumIops;
                    row["QoS Max IOPS"] = disk.QoSMaximumIops;

                    // Differencing Disk
                    row["Parent Path"] = disk.ParentPath;
                    row["Disk Identifier"] = disk.DiskIdentifier;

                    // Environment Information
                    row["Cluster Name"] = disk.ClusterName;
                    row["Current Host"] = disk.CurrentHost;
                    row["VM Path"] = disk.VmPath;
                    row["Config Location"] = disk.ConfigurationLocation;
                    row["Snapshot Location"] = disk.SnapshotFileLocation;
                    row["Smart Paging Path"] = disk.SmartPagingFilePath;

                    // OS Information
                    row["Guest OS Type"] = disk.GuestOsType;

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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Update status label on UI thread before starting
                toolStripStatusLabelTextMainForm.Text = @"Loading virtual disk information...";

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

                            toolStripStatusLabelTextMainForm.Text = $@"Loaded {diskDetails.Count} virtual disk(s)";

                            Message($"Virtual disk overview loaded successfully with {diskDetails.Count} disk(s)",
                                EventType.Information, 5033);

                            // Show success message
                            int diskCount = diskDetails.Count;

                            // Update status label
                            toolStripStatusLabelTextMainForm.Text = $@"Virtual disk overview refreshed - {diskCount} disk(s) found";

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

                            MessageBox.Show(@"No virtual disks found.",
                                @"Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Error updating virtual disk display: {ex.Message}";
                        Message(errorMsg, EventType.Error, 5046);

                        MessageBox.Show(errorMsg,
                            @"Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        toolStripStatusLabelTextMainForm.Text = @"Error displaying virtual disk data";
                    }

                }, "Virtual Disk Overview Refresh");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing virtual disk overview: {ex.Message}";
                Message(errorMsg, EventType.Error, 5042);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = @"Error loading virtual disks";
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
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have disk data
                if (datagridviewvDiskOverView == null || datagridviewvDiskOverView.Rows.Count == 0)
                {
                    MessageBox.Show(@"No virtual disk data available. Please load disks first.",
                        @"No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Calculate disk statistics
                int totalDisks = datagridviewvDiskOverView.Rows.Count;
                double totalMaxSizeGb = 0;
                double totalFileSizeGb = 0;
                double totalUsedSpaceGb = 0;

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
                    if (!string.IsNullOrEmpty(maxSize) && double.TryParse(maxSize, out double maxGb))
                        totalMaxSizeGb += maxGb;

                    var fileSize = row.Cells["File Size (GB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(fileSize) && double.TryParse(fileSize, out double filGb))
                        totalFileSizeGb += filGb;

                    var usedSpace = row.Cells["Used Space (GB)"].Value?.ToString();
                    if (!string.IsNullOrEmpty(usedSpace) && double.TryParse(usedSpace, out double usedGb))
                        totalUsedSpaceGb += usedGb;

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

                double spaceEfficiency = totalMaxSizeGb > 0
                    ? Math.Round((totalFileSizeGb / totalMaxSizeGb) * 100, 1)
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

                Cursor = Cursors.Default;

                // Create summary message
                string summaryText = $@"Virtual Disk Overview Summary - {SessionContext.ServerName}

📊 Disk Statistics:
• Total Virtual Disks: {totalDisks}
• Unique VMs: {uniqueVMs.Count}
• Disks on Running VMs: {disksOnRunningVMs}
• Disks on Stopped VMs: {disksOnStoppedVMs}

💾 Storage Capacity:
• Total Allocated Space: {Math.Round(totalMaxSizeGb, 1):N1} GB
• Actual File Size: {Math.Round(totalFileSizeGb, 1):N1} GB
• Space Efficiency: {spaceEfficiency}%
• Potential Savings: {Math.Round(totalMaxSizeGb - totalFileSizeGb, 1):N1} GB

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

                Message($"Virtual disk summary generated - Total Disks: {totalDisks}, Total Size: {totalMaxSizeGb:F1} GB",
                    EventType.Information, 5044);

                // Show summary message
                MessageBox.Show(summaryText,
                    @"Virtual Disk Overview Summary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;

                string errorMsg = $"Error generating virtual disk summary: {ex.Message}";
                Message(errorMsg, EventType.Error, 5045);

                MessageBox.Show(errorMsg,
                    @"Error",
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

        private void buttonExportVMvmOverviewView_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested VM data export",
                    EventType.Information, 2101);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show(@"Please connect to a Hyper-V server first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have VM data
                if (datagridviewVMOverView == null || datagridviewVMOverView.Rows.Count == 0)
                {
                    MessageBox.Show(@"No VM data available. Please load VMs first.",
                        @"No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Force DataGridView to commit any pending edits (checkbox changes)
                // This ensures that checkbox changes are written to the underlying data before we read them
                datagridviewVMOverView.EndEdit();
                datagridviewVMOverView.CurrentCell = null; // Clear current cell to commit changes
                Application.DoEvents(); // Process pending UI events
#if DEBUG
                Message("Forcing DataGridView to commit pending checkbox changes", EventType.Debug, 2234);
#endif
                // Count how many VMs are selected for export
                int selectedCount = 0;

                // Find the Export column index
                int exportColumnIndex = -1;
                for (int i = 0; i < datagridviewVMOverView.Columns.Count; i++)
                {
                    var col = datagridviewVMOverView.Columns[i];
                    if (col.Name == "Export" || col.DataPropertyName == "Export" || col.HeaderText == @"☑" || col.HeaderText == @"☐")
                    {
                        exportColumnIndex = i;
#if DEBUG
                        Message($"Validation: Found Export column at index {i}", EventType.Debug, 2232);
#endif
                        break;
                    }
                }

                if (exportColumnIndex >= 0)
                {
                    foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                    {
                        var cell = row.Cells[exportColumnIndex];
                        bool isChecked = false;

                        if (cell.Value != null)
                        {
                            if (cell.Value is bool boolValue)
                            {
                                isChecked = boolValue;
                            }
                            else if (cell.Value is int intValue)
                            {
                                isChecked = intValue != 0;
                            }
                            else if (bool.TryParse(cell.Value.ToString(), out bool parsedValue))
                            {
                                isChecked = parsedValue;
                            }
                        }

                        if (isChecked)
                        {
                            selectedCount++;
                        }
                    }

                    Message($"Validation: {selectedCount} of {datagridviewVMOverView.Rows.Count} VMs are checked for export",
                        EventType.Information, 2233);
                }
                else
                {
                    // If Export column not found, export all VMs
                    selectedCount = datagridviewVMOverView.Rows.Count;
                    Message("Validation: Export column not found - will export all VMs",
                        EventType.Warning, 2226);
                }

                if (selectedCount == 0)
                {
                    MessageBox.Show(@"No VMs selected for export. Please check at least one VM in the Export column.",
                        @"No VMs Selected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Message($"Export VM Data requested - {selectedCount} of {datagridviewVMOverView.Rows.Count} VMs selected for export",
                    EventType.Information, 2102);

                // Show SaveFileDialog with format options
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = @"Export data for selected VM´s";
                    saveFileDialog.FileName = $"Exported_VMData_{SessionContext.ServerName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    saveFileDialog.Filter = @"JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;
                        string fileExtension = Path.GetExtension(filePath).ToLower();

                        Message($"Exporting VM data to: '{filePath}', (Format: {fileExtension})",
                            EventType.Information, 2103);

                        // Show progress cursor
                        Cursor = Cursors.WaitCursor;

                        try
                        {
                            // Get VM Groups data
                            var vmGroups = VmGroups.GetHyperVvmGroups(cmd => ExecutePowerShellCommand(cmd));

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
                                Message($"VM data export completed successfully: '{filePath}'",
                                    EventType.Information, 2104);

                                // Show success message with option to open file location
                                var result = MessageBox.Show(
                                    $@"VM data exported successfully to:
{filePath}

Would you like to open the file location?",
                                    @"Export Complete",
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
                            Cursor = Cursors.Default;
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
                    @"Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonLoadvCheckpointsrefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested VM checkpoints refresh",
                    EventType.Information, 6050);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                toolStripStatusLabelTextMainForm.Text = @"Loading VM checkpoints...";

                Message("Retrieving VM checkpoints...",
                    EventType.Information, 6051);

                // Execute with progress form
                ExecuteWithProgressForm(() =>
                {
                    // Load checkpoint data on UI thread (must invoke because it manipulates DataGridView)
                    Invoke((MethodInvoker)delegate
                    {
                        LoadVmCheckpoints();
                    });

                    // Count checkpoints from the grid on UI thread
                    int totalCheckpoints;
                    int standardCheckpoints = 0;
                    int productionCheckpoints = 0;

                    Invoke((MethodInvoker)delegate
                    {
                        totalCheckpoints = datagridviewCheckpointOverView.Rows.Count;

                        foreach (DataGridViewRow row in datagridviewCheckpointOverView.Rows)
                        {
                            var type = row.Cells["Checkpoint Type"]?.Value?.ToString();
                            if (type == "Standard")
                                standardCheckpoints++;
                            else if (type == "Production")
                                productionCheckpoints++;
                        }

                        Message($"VM checkpoints refresh completed - Total: {totalCheckpoints}, Standard: {standardCheckpoints}, Production: {productionCheckpoints}",
                            EventType.Information, 6052);

                        toolStripStatusLabelTextMainForm.Text = $@"VM checkpoints loaded - Total: {totalCheckpoints}, Standard: {standardCheckpoints}, Production: {productionCheckpoints}";
                    });

                }, "VM Checkpoints Refresh");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing VM checkpoints: {ex.Message}";
                Message(errorMsg, EventType.Error, 6053);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonSummaryvCheckpointsView_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested VM checkpoints summary",
                    EventType.Information, 6054);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have checkpoint data
                if (datagridviewCheckpointOverView == null || datagridviewCheckpointOverView.Rows.Count == 0)
                {
                    MessageBox.Show(@"No checkpoint data available. Please load checkpoints first.",
                        @"No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Calculate checkpoint statistics
                int totalCheckpoints = datagridviewCheckpointOverView.Rows.Count;
                int standardCheckpoints = 0;
                int productionCheckpoints = 0;
                double totalSizeMb = 0;
                var uniqueVMs = new HashSet<string>();
                var oldestCheckpoint = DateTime.MaxValue;
                var newestCheckpoint = DateTime.MinValue;
                var checkpointsByVm = new Dictionary<string, int>();

                foreach (DataGridViewRow row in datagridviewCheckpointOverView.Rows)
                {
                    // VM tracking
                    var vmName = row.Cells["VM Name"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(vmName))
                    {
                        uniqueVMs.Add(vmName);

                        if (!checkpointsByVm.ContainsKey(vmName))
                            checkpointsByVm[vmName] = 0;
                        checkpointsByVm[vmName]++;
                    }

                    // Checkpoint type
                    var type = row.Cells["Checkpoint Type"]?.Value?.ToString();
                    if (type == "Standard")
                        standardCheckpoints++;
                    else if (type == "Production")
                        productionCheckpoints++;

                    // Size (stored as decimal string like "50.25")
                    var sizeStr = row.Cells["Size (MB)"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(sizeStr) && double.TryParse(sizeStr, out double sizeMb))
                        totalSizeMb += sizeMb;

                    // Dates
                    var createdStr = row.Cells["Created"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(createdStr) && DateTime.TryParse(createdStr, out DateTime created))
                    {
                        if (created < oldestCheckpoint)
                            oldestCheckpoint = created;
                        if (created > newestCheckpoint)
                            newestCheckpoint = created;
                    }
                }

                // Find VM with most checkpoints
                string vmWithMostCheckpoints = "";
                int maxCheckpoints = 0;
                foreach (var kvp in checkpointsByVm)
                {
                    if (kvp.Value > maxCheckpoints)
                    {
                        maxCheckpoints = kvp.Value;
                        vmWithMostCheckpoints = kvp.Key;
                    }
                }

                Cursor = Cursors.Default;

                // Create summary message
                string summaryText = $@"VM Checkpoints Summary - {SessionContext.ServerName}

📊 Checkpoint Statistics:
• Total Checkpoints: {totalCheckpoints}
• Standard Checkpoints: {standardCheckpoints}
• Production Checkpoints: {productionCheckpoints}
• VMs with Checkpoints: {uniqueVMs.Count}

💾 Storage Usage:
• Total Size: {Math.Round(totalSizeMb / 1024.0, 2):N2} GB ({totalSizeMb:N0} MB)
• Average per Checkpoint: {(totalCheckpoints > 0 ? Math.Round(totalSizeMb / (double)totalCheckpoints, 2) : 0):N2} MB

📅 Age Information:
• Oldest Checkpoint: {(oldestCheckpoint != DateTime.MaxValue ? oldestCheckpoint.ToString("yyyy-MM-dd HH:mm") : "N/A")}
• Newest Checkpoint: {(newestCheckpoint != DateTime.MinValue ? newestCheckpoint.ToString("yyyy-MM-dd HH:mm") : "N/A")}

🔝 Top VM:
• Most Checkpoints: {vmWithMostCheckpoints} ({maxCheckpoints} checkpoint{(maxCheckpoints != 1 ? "s" : "")})

💡 Recommendations:
{GetCheckpointRecommendations(totalCheckpoints, standardCheckpoints, productionCheckpoints, totalSizeMb, oldestCheckpoint)}";

                Message($"VM checkpoints summary generated - Total: {totalCheckpoints}",
                    EventType.Information, 6055);

                // Show summary message
                MessageBox.Show(summaryText,
                    @"VM Checkpoints Summary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;

                string errorMsg = $"Error generating VM checkpoints summary: {ex.Message}";
                Message(errorMsg, EventType.Error, 6056);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads VM checkpoints from all VMs and displays them in the DataGridView
        /// </summary>
        private void LoadVmCheckpoints()
        {
            try
            {
                Message($"Loading VM checkpoints from '{SessionContext.ServerName}'",
                    EventType.Information, 6057);

                // Clear existing data
                datagridviewCheckpointOverView.DataSource = null;
                datagridviewCheckpointOverView.Rows.Clear();
                datagridviewCheckpointOverView.Columns.Clear();

                // Get all checkpoints
                string getCheckpointsScript = @"
                    $ErrorActionPreference = 'SilentlyContinue'
                    $vms = Get-VM
                    
                    if (-not $vms) {
# No VMs found, return empty
                        return
                    }
                    
                    foreach ($vm in $vms) {
                        try {
                            $checkpoints = Get-VMSnapshot -VMName $vm.Name -ErrorAction SilentlyContinue
                            
                            if (-not $checkpoints) {
# No checkpoints for this VM
                                continue
                            }
                            
                            foreach ($checkpoint in $checkpoints) {
# Get checkpoint file size if available
                                $sizeBytes = 0
                                try {
                                    if ($checkpoint.Path -and (Test-Path $checkpoint.Path -ErrorAction SilentlyContinue)) {
                                        $files = Get-ChildItem -Path $checkpoint.Path -Recurse -File -ErrorAction SilentlyContinue
                                        if ($files) {
                                            $sum = ($files | Measure-Object -Property Length -Sum -ErrorAction SilentlyContinue).Sum
                                            if ($sum) {
                                                $sizeBytes = $sum
                                            }
                                        }
                                    }
                                } catch {
# Silently continue if size calculation fails
                                    $sizeBytes = 0
                                }
                                
# Count complex arrays
                                $hardDrivesCount = if ($checkpoint.HardDrives) { @($checkpoint.HardDrives).Count } else { 0 }
                                $networkAdaptersCount = if ($checkpoint.NetworkAdapters) { @($checkpoint.NetworkAdapters).Count } else { 0 }
                                $dvdDrivesCount = if ($checkpoint.DVDDrives) { @($checkpoint.DVDDrives).Count } else { 0 }

# Output the object with ALL detailed properties from Get-VMSnapshot
                                [PSCustomObject]@{
# VM Information
                                    VMName = $vm.Name
                                    VMId = $vm.VMId
                                    VMState = $vm.State
                                    VMGeneration = $vm.Generation
                                    
# Checkpoint Basic Information
                                    CheckpointName = $checkpoint.Name
                                    CheckpointId = $checkpoint.Id
                                    CheckpointType = if ($checkpoint.PSObject.Properties['CheckpointType']) { $checkpoint.CheckpointType } else { $checkpoint.SnapshotType }
                                    SnapshotType = $checkpoint.SnapshotType
                                    IsAutomaticCheckpoint = if ($checkpoint.PSObject.Properties['IsAutomaticCheckpoint']) { $checkpoint.IsAutomaticCheckpoint } else { $false }
                                    State = if ($checkpoint.PSObject.Properties['State']) { $checkpoint.State } else { 'Unknown' }
                                    CreationTime = $checkpoint.CreationTime
                                    
# Hierarchy
                                    ParentCheckpointName = $checkpoint.ParentSnapshotName
                                    ParentCheckpointId = $checkpoint.ParentSnapshotId
                                    
# Storage
                                    Path = $checkpoint.Path
                                    SizeBytes = $sizeBytes
                                    SizeOfSystemFiles = if ($checkpoint.PSObject.Properties['SizeOfSystemFiles']) { $checkpoint.SizeOfSystemFiles } else { 0 }
                                    
# Version
                                    Version = if ($checkpoint.PSObject.Properties['Version']) { $checkpoint.Version } else { 'N/A' }
                                    
# Notes
                                    Notes = $checkpoint.Notes
                                    
# Processor Configuration
                                    ProcessorCount = if ($checkpoint.PSObject.Properties['ProcessorCount']) { $checkpoint.ProcessorCount } else { 0 }
                                    
# Memory Configuration
                                    MemoryStartup = if ($checkpoint.PSObject.Properties['MemoryStartup']) { $checkpoint.MemoryStartup } else { 0 }
                                    MemoryMinimum = if ($checkpoint.PSObject.Properties['MemoryMinimum']) { $checkpoint.MemoryMinimum } else { 0 }
                                    MemoryMaximum = if ($checkpoint.PSObject.Properties['MemoryMaximum']) { $checkpoint.MemoryMaximum } else { 0 }
                                    DynamicMemoryEnabled = if ($checkpoint.PSObject.Properties['DynamicMemoryEnabled']) { $checkpoint.DynamicMemoryEnabled } else { $false }
                                    
# Hardware Configuration
                                    HardDrivesCount = $hardDrivesCount
                                    NetworkAdaptersCount = $networkAdaptersCount
                                    DVDDrivesCount = $dvdDrivesCount
                                    
# Advanced Properties
                                    BatteryPassthroughEnabled = if ($checkpoint.PSObject.Properties['BatteryPassthroughEnabled']) { $checkpoint.BatteryPassthroughEnabled } else { $false }
                                    IsClustered = if ($checkpoint.PSObject.Properties['IsClustered']) { $checkpoint.IsClustered } else { $false }
                                    IsDeleted = if ($checkpoint.PSObject.Properties['IsDeleted']) { $checkpoint.IsDeleted } else { $false }
                                    LockOnDisconnect = if ($checkpoint.PSObject.Properties['LockOnDisconnect']) { $checkpoint.LockOnDisconnect } else { 'Off' }
                                    
# Memory Mapped IO (Advanced)
                                    LowMemoryMappedIoSpace = if ($checkpoint.PSObject.Properties['LowMemoryMappedIoSpace']) { $checkpoint.LowMemoryMappedIoSpace } else { 0 }
                                    HighMemoryMappedIoSpace = if ($checkpoint.PSObject.Properties['HighMemoryMappedIoSpace']) { $checkpoint.HighMemoryMappedIoSpace } else { 0 }
                                    HighMemoryMappedIoBaseAddress = if ($checkpoint.PSObject.Properties['HighMemoryMappedIoBaseAddress']) { $checkpoint.HighMemoryMappedIoBaseAddress } else { 0 }
                                    GuestControlledCacheTypes = if ($checkpoint.PSObject.Properties['GuestControlledCacheTypes']) { $checkpoint.GuestControlledCacheTypes } else { $false }
                                    
# Host Information
                                    ComputerName = $env:COMPUTERNAME
                                }
                            }
                        } catch {
# Skip VMs that error
                            continue
                        }
                    }
                ";

                System.Collections.ObjectModel.Collection<PSObject> results;

                if (SessionContext.IsCluster && !SessionContext.IsLocal)
                {
                    Message("Getting checkpoints from cluster nodes...",
                        EventType.Information, 6058);

                    // Get cluster nodes
                    string getNodesScript = @"
                        Get-ClusterNode -ErrorAction Stop | Select-Object -ExpandProperty Name
                    ";

                    var nodesResult = ExecutePowerShellCommand(getNodesScript);

                    if (nodesResult == null || nodesResult.Count == 0)
                    {
                        Message("No cluster nodes found, falling back to standard checkpoint retrieval",
                            EventType.Warning, 6070);
                        results = ExecutePowerShellCommand(getCheckpointsScript);
                    }
                    else
                    {
                        var allCheckpoints = new System.Collections.ObjectModel.Collection<PSObject>();

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
                            EventType.Information, 6071);

                        // Get checkpoints from each node
                        int nodeIndex = 0;
                        foreach (var node in clusterNodes)
                        {
                            nodeIndex++;
                            try
                            {
                                Message($"Getting checkpoints from cluster node {nodeIndex} of {clusterNodes.Count}: '{node}'",
                                    EventType.Information, 6072);

                                var nodeCheckpoints = ExecutePowerShellCommandOnNode(node, getCheckpointsScript);

                                if (nodeCheckpoints != null && nodeCheckpoints.Count > 0)
                                {
                                    foreach (var checkpoint in nodeCheckpoints)
                                    {
                                        allCheckpoints.Add(checkpoint);
                                    }
                                    Message($"Added {nodeCheckpoints.Count} checkpoint(s) from cluster node: '{node}'",
                                        EventType.Information, 6073);
                                }
                                else
                                {
                                    Message($"No checkpoints found on cluster node: {node}",
                                        EventType.Information, 6074);
                                }
                            }
                            catch (Exception ex)
                            {
                                Message($"Failed to get checkpoints from cluster node {node}: {ex.Message}",
                                    EventType.Warning, 6075);
                                // Continue processing other nodes
                            }
                        }

                        Message($"Total checkpoints collected from all cluster nodes: {allCheckpoints.Count}",
                            EventType.Information, 6076);

                        results = allCheckpoints;
                    }
                }
                else
                {
                    // Standard retrieval for standalone or local
                    results = ExecutePowerShellCommand(getCheckpointsScript);
                }

                if (results == null || results.Count == 0)
                {
                    MessageBox.Show(@"No VM checkpoints found.",
                        @"Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Message($"Retrieved {results.Count} checkpoints, processing...",
                    EventType.Information, 6060);

                // Create DataTable with comprehensive checkpoint columns
                var dataTable = new DataTable();

                // VM Information
                dataTable.Columns.Add("VM Name", typeof(string));
                dataTable.Columns.Add("VM State", typeof(string));
                dataTable.Columns.Add("VM Generation", typeof(string));
                dataTable.Columns.Add("VM ID", typeof(string));

                // Checkpoint Basic Information
                dataTable.Columns.Add("Checkpoint Name", typeof(string));
                dataTable.Columns.Add("Checkpoint Type", typeof(string));
                dataTable.Columns.Add("State", typeof(string));
                dataTable.Columns.Add("Is Automatic", typeof(string));
                dataTable.Columns.Add("Created", typeof(string));
                dataTable.Columns.Add("Age (Days)", typeof(string));

                // Hierarchy
                dataTable.Columns.Add("Parent Checkpoint", typeof(string));

                // Storage
                dataTable.Columns.Add("Size (MB)", typeof(string));
                dataTable.Columns.Add("System Files (MB)", typeof(string));
                dataTable.Columns.Add("Path", typeof(string));

                // Version & Notes
                dataTable.Columns.Add("Version", typeof(string));
                dataTable.Columns.Add("Notes", typeof(string));

                // Processor Configuration
                dataTable.Columns.Add("Processor Count", typeof(string));

                // Memory Configuration
                dataTable.Columns.Add("Memory Startup (MB)", typeof(string));
                dataTable.Columns.Add("Memory Minimum (MB)", typeof(string));
                dataTable.Columns.Add("Memory Maximum (MB)", typeof(string));
                dataTable.Columns.Add("Dynamic Memory", typeof(string));

                // Hardware Configuration
                dataTable.Columns.Add("Hard Drives", typeof(string));
                dataTable.Columns.Add("Network Adapters", typeof(string));
                dataTable.Columns.Add("DVD Drives", typeof(string));

                // Advanced Properties
                dataTable.Columns.Add("Battery Passthrough", typeof(string));
                dataTable.Columns.Add("Is Clustered", typeof(string));
                dataTable.Columns.Add("Is Deleted", typeof(string));
                dataTable.Columns.Add("Lock On Disconnect", typeof(string));

                // Advanced Memory Mapped IO
                dataTable.Columns.Add("Low MMIO Space", typeof(string));
                dataTable.Columns.Add("High MMIO Space", typeof(string));
                dataTable.Columns.Add("High MMIO Base Address", typeof(string));
                dataTable.Columns.Add("Guest Controlled Cache", typeof(string));

                // Host Information
                dataTable.Columns.Add("Host", typeof(string));
                dataTable.Columns.Add("Checkpoint ID", typeof(string));

                foreach (var checkpoint in results)
                {
                    var row = dataTable.NewRow();

                    // VM Information
                    row["VM Name"] = checkpoint.Properties["VMName"]?.Value?.ToString() ?? "";
                    row["VM State"] = checkpoint.Properties["VMState"]?.Value?.ToString() ?? "";
                    row["VM Generation"] = checkpoint.Properties["VMGeneration"]?.Value?.ToString() ?? "";
                    row["VM ID"] = checkpoint.Properties["VMId"]?.Value?.ToString() ?? "";

                    // Checkpoint Basic Information
                    row["Checkpoint Name"] = checkpoint.Properties["CheckpointName"]?.Value?.ToString() ?? "";
                    row["Checkpoint Type"] = checkpoint.Properties["SnapshotType"]?.Value?.ToString() ?? "";
                    row["State"] = checkpoint.Properties["State"]?.Value?.ToString() ?? "";

                    var isAutomatic = checkpoint.Properties["IsAutomaticCheckpoint"]?.Value;
                    row["Is Automatic"] = (isAutomatic != null && (bool)isAutomatic) ? "Yes" : "No";

                    // Creation Time and Age
                    var creationTime = checkpoint.Properties["CreationTime"]?.Value;
                    if (creationTime != null && creationTime is DateTime dt)
                    {
                        row["Created"] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        row["Age (Days)"] = Math.Round((DateTime.Now - dt).TotalDays, 1).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        row["Created"] = "";
                        row["Age (Days)"] = "";
                    }

                    // Hierarchy
                    row["Parent Checkpoint"] = checkpoint.Properties["ParentCheckpointName"]?.Value?.ToString() ?? "None";

                    // Storage - Size Information
                    var sizeBytes = checkpoint.Properties["SizeBytes"]?.Value;
                    if (sizeBytes != null && long.TryParse(sizeBytes.ToString(), out long bytes))
                    {
                        row["Size (MB)"] = Math.Round(bytes / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        row["Size (MB)"] = "0";
                    }

                    var sizeOfSystemFiles = checkpoint.Properties["SizeOfSystemFiles"]?.Value;
                    if (sizeOfSystemFiles != null && long.TryParse(sizeOfSystemFiles.ToString(), out long systemFilesBytes))
                    {
                        row["System Files (MB)"] = Math.Round(systemFilesBytes / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        row["System Files (MB)"] = "0";
                    }

                    row["Path"] = checkpoint.Properties["Path"]?.Value?.ToString() ?? "";

                    // Version & Notes
                    row["Version"] = checkpoint.Properties["Version"]?.Value?.ToString() ?? "N/A";
                    row["Notes"] = checkpoint.Properties["Notes"]?.Value?.ToString() ?? "";

                    // Processor Configuration
                    row["Processor Count"] = checkpoint.Properties["ProcessorCount"]?.Value?.ToString() ?? "0";

                    // Memory Configuration
                    var memoryStartup = checkpoint.Properties["MemoryStartup"]?.Value;
                    if (memoryStartup != null && long.TryParse(memoryStartup.ToString(), out long memStartupBytes))
                    {
                        row["Memory Startup (MB)"] = Math.Round(memStartupBytes / (1024.0 * 1024.0), 0).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        row["Memory Startup (MB)"] = "0";
                    }

                    var memoryMinimum = checkpoint.Properties["MemoryMinimum"]?.Value;
                    if (memoryMinimum != null && long.TryParse(memoryMinimum.ToString(), out long memMinBytes))
                    {
                        row["Memory Minimum (MB)"] = Math.Round(memMinBytes / (1024.0 * 1024.0), 0).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        row["Memory Minimum (MB)"] = "0";
                    }

                    var memoryMaximum = checkpoint.Properties["MemoryMaximum"]?.Value;
                    if (memoryMaximum != null && long.TryParse(memoryMaximum.ToString(), out long memMaxBytes))
                    {
                        row["Memory Maximum (MB)"] = Math.Round(memMaxBytes / (1024.0 * 1024.0), 0).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        row["Memory Maximum (MB)"] = "0";
                    }

                    var dynamicMemory = checkpoint.Properties["DynamicMemoryEnabled"]?.Value;
                    row["Dynamic Memory"] = (dynamicMemory != null && (bool)dynamicMemory) ? "Yes" : "No";

                    // Hardware Configuration
                    row["Hard Drives"] = checkpoint.Properties["HardDrivesCount"]?.Value?.ToString() ?? "0";
                    row["Network Adapters"] = checkpoint.Properties["NetworkAdaptersCount"]?.Value?.ToString() ?? "0";
                    row["DVD Drives"] = checkpoint.Properties["DVDDrivesCount"]?.Value?.ToString() ?? "0";

                    // Advanced Properties
                    var batteryPassthrough = checkpoint.Properties["BatteryPassthroughEnabled"]?.Value;
                    row["Battery Passthrough"] = (batteryPassthrough != null && (bool)batteryPassthrough) ? "Yes" : "No";

                    var isClustered = checkpoint.Properties["IsClustered"]?.Value;
                    row["Is Clustered"] = (isClustered != null && (bool)isClustered) ? "Yes" : "No";

                    var isDeleted = checkpoint.Properties["IsDeleted"]?.Value;
                    row["Is Deleted"] = (isDeleted != null && (bool)isDeleted) ? "Yes" : "No";

                    row["Lock On Disconnect"] = checkpoint.Properties["LockOnDisconnect"]?.Value?.ToString() ?? "Off";

                    // Advanced Memory Mapped IO
                    var lowMmio = checkpoint.Properties["LowMemoryMappedIoSpace"]?.Value;
                    row["Low MMIO Space"] = lowMmio != null ? lowMmio.ToString() : "0";

                    var highMmio = checkpoint.Properties["HighMemoryMappedIoSpace"]?.Value;
                    row["High MMIO Space"] = highMmio != null ? highMmio.ToString() : "0";

                    var highMmioBase = checkpoint.Properties["HighMemoryMappedIoBaseAddress"]?.Value;
                    row["High MMIO Base Address"] = highMmioBase != null ? highMmioBase.ToString() : "0";

                    var guestCache = checkpoint.Properties["GuestControlledCacheTypes"]?.Value;
                    row["Guest Controlled Cache"] = (guestCache != null && (bool)guestCache) ? "Yes" : "No";

                    // Host Information
                    row["Host"] = checkpoint.Properties["ComputerName"]?.Value?.ToString() ?? SessionContext.ServerName;
                    row["Checkpoint ID"] = checkpoint.Properties["CheckpointId"]?.Value?.ToString() ?? "";

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewCheckpointOverView.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewCheckpointOverView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewCheckpointOverView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewCheckpointOverView.MultiSelect = false;
                datagridviewCheckpointOverView.ReadOnly = true;
                datagridviewCheckpointOverView.AllowUserToAddRows = false;
                datagridviewCheckpointOverView.AllowUserToDeleteRows = false;
                datagridviewCheckpointOverView.RowHeadersVisible = false;

                // Add context menu for right-click
                var contextMenu = new ContextMenuStrip();
                var deleteMenuItem = new ToolStripMenuItem("Delete Checkpoint");
                deleteMenuItem.Click += DeleteCheckpointMenuItem_Click;
                contextMenu.Items.Add(deleteMenuItem);

                var viewDetailsMenuItem = new ToolStripMenuItem("View Details");
                viewDetailsMenuItem.Click += ViewCheckpointDetailsMenuItem_Click;
                contextMenu.Items.Add(viewDetailsMenuItem);

                datagridviewCheckpointOverView.ContextMenuStrip = contextMenu;

                // Apply color coding
                foreach (DataGridViewRow row in datagridviewCheckpointOverView.Rows)
                {
                    // Color code checkpoint type
                    var type = row.Cells["Checkpoint Type"]?.Value?.ToString();
                    if (type == "Standard")
                    {
                        row.Cells["Checkpoint Type"].Style.BackColor = Color.LightBlue;
                    }
                    else if (type == "Production")
                    {
                        row.Cells["Checkpoint Type"].Style.BackColor = Color.LightGreen;
                    }

                    // Color code automatic checkpoints
                    var isAutomatic = row.Cells["Is Automatic"]?.Value?.ToString();
                    if (isAutomatic == "Yes")
                    {
                        row.Cells["Is Automatic"].Style.BackColor = Color.LightYellow;
                        row.Cells["Is Automatic"].Style.ForeColor = Color.DarkOrange;
                    }
                    else
                    {
                        row.Cells["Is Automatic"].Style.BackColor = Color.LightGreen;
                        row.Cells["Is Automatic"].Style.ForeColor = Color.DarkGreen;
                    }

                    // Color code age - highlight old checkpoints
                    var ageStr = row.Cells["Age (Days)"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(ageStr) && double.TryParse(ageStr, out double age))
                    {
                        if (age > 90)
                        {
                            row.Cells["Age (Days)"].Style.BackColor = Color.LightCoral;
                            row.Cells["Age (Days)"].Style.ForeColor = Color.DarkRed;
                        }
                        else if (age > 30)
                        {
                            row.Cells["Age (Days)"].Style.BackColor = Color.LightYellow;
                            row.Cells["Age (Days)"].Style.ForeColor = Color.DarkOrange;
                        }
                    }

                    // Color code VM state
                    var state = row.Cells["VM State"]?.Value?.ToString();
                    if (state == "Running")
                    {
                        row.Cells["VM State"].Style.BackColor = Color.LightGreen;
                    }
                    else if (state == "Off")
                    {
                        row.Cells["VM State"].Style.BackColor = Color.LightGray;
                    }

                    // Color code checkpoint state
                    var checkpointState = row.Cells["State"]?.Value?.ToString();
                    if (checkpointState == "Off")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightGray;
                    }
                    else if (checkpointState == "Running")
                    {
                        row.Cells["State"].Style.BackColor = Color.LightGreen;
                    }

                    // Color code VM Generation
                    var generation = row.Cells["VM Generation"]?.Value?.ToString();
                    if (generation == "1")
                    {
                        row.Cells["VM Generation"].Style.BackColor = Color.LightBlue;
                    }
                    else if (generation == "2")
                    {
                        row.Cells["VM Generation"].Style.BackColor = Color.LightCyan;
                    }

                    // Color code Dynamic Memory
                    var dynamicMemory = row.Cells["Dynamic Memory"]?.Value?.ToString();
                    if (dynamicMemory == "Yes")
                    {
                        row.Cells["Dynamic Memory"].Style.BackColor = Color.LightBlue;
                    }

                    // Color code Is Deleted
                    var isDeleted = row.Cells["Is Deleted"]?.Value?.ToString();
                    if (isDeleted == "Yes")
                    {
                        row.Cells["Is Deleted"].Style.BackColor = Color.LightCoral;
                        row.Cells["Is Deleted"].Style.ForeColor = Color.DarkRed;
                    }
                }

                Message($"VM checkpoints loaded successfully with {results.Count} checkpoint(s)",
                    EventType.Information, 6061);
            }
            catch (Exception ex)
            {
                Message($"Error loading VM checkpoints: {ex.Message}",
                    EventType.Error, 6062);
                MessageBox.Show($@"Error loading VM checkpoints: {ex.Message}",
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the delete checkpoint context menu click
        /// </summary>
        private void DeleteCheckpointMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (datagridviewCheckpointOverView.SelectedRows.Count == 0)
                {
                    MessageBox.Show(@"Please select a checkpoint to delete.",
                        @"No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var selectedRow = datagridviewCheckpointOverView.SelectedRows[0];
                string vmName = selectedRow.Cells["VM Name"]?.Value?.ToString();
                string checkpointName = selectedRow.Cells["Checkpoint Name"]?.Value?.ToString();
                string checkpointType = selectedRow.Cells["Checkpoint Type"]?.Value?.ToString();

                if (string.IsNullOrEmpty(vmName) || string.IsNullOrEmpty(checkpointName))
                {
                    MessageBox.Show(@"Invalid checkpoint selection.",
                        @"Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Message($"User requested deletion of checkpoint '{checkpointName}' on VM '{vmName}'",
                    EventType.Information, 6063);

                // Confirmation dialog
                var confirmResult = MessageBox.Show(
                    $@"Are you sure you want to delete this checkpoint?

VM: {vmName}
Checkpoint: {checkpointName}
Type: {checkpointType}

This action cannot be undone!",
                    @"Confirm Checkpoint Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmResult != DialogResult.Yes)
                {
                    Message("Checkpoint deletion cancelled by user",
                        EventType.Information, 6064);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Delete the checkpoint
                string deleteScript = $@"
                    Remove-VMSnapshot -VMName '{vmName}' -Name '{checkpointName}' -ErrorAction Stop
                ";

                Message($"Deleting checkpoint '{checkpointName}' on VM '{vmName}'...",
                    EventType.Information, 6065);

                var result = ExecutePowerShellCommand(deleteScript);

                if (result != null)
                {
                    Message($"Checkpoint '{checkpointName}' deleted successfully",
                        EventType.Information, 6066);

                    MessageBox.Show($@"Checkpoint '{checkpointName}' deleted successfully.

Note: The checkpoint files will be merged in the background, which may take some time depending on the size.",
                        @"Checkpoint Deleted",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Refresh the checkpoint view
                    LoadVmCheckpoints();
                }
                else
                {
                    Message($"Failed to delete checkpoint '{checkpointName}'",
                        EventType.Error, 6067);

                    MessageBox.Show(@"Failed to delete checkpoint. Check the logs for details.",
                        @"Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error deleting checkpoint: {ex.Message}";
                Message(errorMsg, EventType.Error, 6068);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the view checkpoint details context menu click
        /// </summary>
        private void ViewCheckpointDetailsMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (datagridviewCheckpointOverView.SelectedRows.Count == 0)
                {
                    MessageBox.Show(@"Please select a checkpoint to view details.",
                        @"No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var selectedRow = datagridviewCheckpointOverView.SelectedRows[0];

                string details = $@"Checkpoint Details

VM Information:
• VM Name: {selectedRow.Cells["VM Name"]?.Value}
• VM State: {selectedRow.Cells["VM State"]?.Value}
• VM Generation: {selectedRow.Cells["VM Generation"]?.Value}
• VM ID: {selectedRow.Cells["VM ID"]?.Value}

Checkpoint Information:
• Name: {selectedRow.Cells["Checkpoint Name"]?.Value}
• Type: {selectedRow.Cells["Checkpoint Type"]?.Value}
• State: {selectedRow.Cells["State"]?.Value}
• Is Automatic: {selectedRow.Cells["Is Automatic"]?.Value}
• Version: {selectedRow.Cells["Version"]?.Value}
• Created: {selectedRow.Cells["Created"]?.Value}
• Age: {selectedRow.Cells["Age (Days)"]?.Value} days

Size Information:
• Total Size: {selectedRow.Cells["Size (MB)"]?.Value} MB
• System Files: {selectedRow.Cells["System Files (MB)"]?.Value} MB

VM Configuration at Checkpoint Time:
• Processor Count: {selectedRow.Cells["Processor Count"]?.Value}
• Memory Startup: {selectedRow.Cells["Memory Startup (MB)"]?.Value} MB
• Memory Minimum: {selectedRow.Cells["Memory Minimum (MB)"]?.Value} MB
• Memory Maximum: {selectedRow.Cells["Memory Maximum (MB)"]?.Value} MB
• Dynamic Memory: {selectedRow.Cells["Dynamic Memory"]?.Value}

Hardware Configuration:
• Hard Drives: {selectedRow.Cells["Hard Drives"]?.Value}
• Network Adapters: {selectedRow.Cells["Network Adapters"]?.Value}
• DVD Drives: {selectedRow.Cells["DVD Drives"]?.Value}

Advanced Properties:
• Battery Passthrough: {selectedRow.Cells["Battery Passthrough"]?.Value}
• Is Clustered: {selectedRow.Cells["Is Clustered"]?.Value}
• Is Deleted: {selectedRow.Cells["Is Deleted"]?.Value}
• Lock On Disconnect: {selectedRow.Cells["Lock On Disconnect"]?.Value}
• Guest Controlled Cache: {selectedRow.Cells["Guest Controlled Cache"]?.Value}

Advanced Memory Configuration:
• Low MMIO Space: {selectedRow.Cells["Low MMIO Space"]?.Value}
• High MMIO Space: {selectedRow.Cells["High MMIO Space"]?.Value}
• High MMIO Base Address: {selectedRow.Cells["High MMIO Base Address"]?.Value}

Hierarchy:
• Parent Checkpoint: {selectedRow.Cells["Parent Checkpoint"]?.Value}

Storage:
• Path: {selectedRow.Cells["Path"]?.Value}
• Host: {selectedRow.Cells["Host"]?.Value}

Notes:
{selectedRow.Cells["Notes"]?.Value}";

                MessageBox.Show(details,
                    @"Checkpoint Details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Message($"Error showing checkpoint details: {ex.Message}",
                    EventType.Error, 6069);
            }
        }

        /// <summary>
        /// Generates recommendations based on checkpoint statistics
        /// </summary>
        private string GetCheckpointRecommendations(int totalCheckpoints, int standardCheckpoints,
            int productionCheckpoints, double totalSizeMb, DateTime oldestCheckpoint)
        {
            var recommendations = new List<string>();

            // Too many checkpoints
            if (totalCheckpoints > 50)
            {
                recommendations.Add($"• High number of checkpoints ({totalCheckpoints}) - consider removing old or unnecessary ones");
            }

            // Old checkpoints
            if (oldestCheckpoint != DateTime.MaxValue)
            {
                var age = (DateTime.Now - oldestCheckpoint).TotalDays;
                if (age > 90)
                {
                    recommendations.Add($"• Oldest checkpoint is {Math.Round(age, 0)} days old - review and remove if no longer needed");
                }
            }

            // Storage usage
            double totalSizeGb = totalSizeMb / 1024.0;
            if (totalSizeGb > 100)
            {
                recommendations.Add($"• High storage usage ({Math.Round(totalSizeGb, 2)} GB) - checkpoints consume significant disk space");
            }

            // Standard vs Production
            if (standardCheckpoints > productionCheckpoints * 3 && productionCheckpoints > 0)
            {
                recommendations.Add("• Consider using Production checkpoints for better backup consistency");
            }

            return recommendations.Count > 0
                ? string.Join("\n", recommendations)
                : "• Checkpoint management looks good - no immediate actions needed";
        }

        private void buttonLoadHealthOverview_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested host health/inventory overview",
                    EventType.Information, 7100);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                toolStripStatusLabelTextMainForm.Text = @"Loading host health inventory...";


                Message("Retrieving host health inventory...",
                    EventType.Information, 7101);

                // Execute with progress form
                ExecuteWithProgressForm<HostHealthInfo?>(() =>
                {
#if DEBUG
                    // Get host inventory (runs in background thread)
                    Message("Calling HostInventory.GetHyperVHostInventory...",
                        EventType.Information, 7102);
#endif
                    return HostHealth.GetHyperVHostHealth(
                        cmd => ExecutePowerShellCommand(cmd),
                        (node, cmd) => ExecutePowerShellCommandOnNode(node, cmd),
                        includeDetailedVMs: true);

                }, (inventory) =>
                {
                    // Handle result on UI thread
                    try
                    {
                        if (inventory != null)
                        {
                            // Store inventory for potential node switching

                            // Track which node's data we're displaying
                            _currentlyDisplayedNodeName = inventory.HostInfo.ComputerName;

                            Message($"Retrieved host inventory for '{inventory.HostInfo.ComputerName}', updating DataGridView",
                                EventType.Information, 7103);

                            // Update the DataGridView
                            UpdateHealthOverviewDataGridView(inventory);

                            // Update node selector for clusters
                            UpdateClusterNodeSelector(inventory);

                            // Update status with summary
                            int totalVMs = inventory.WorkloadAnalysis.TotalVMs;
                            int runningVMs = inventory.WorkloadAnalysis.RunningVMs;
                            double cpuOvercommit = inventory.ResourceAllocation.CpuOvercommitRatio;
                            double memOvercommit = inventory.ResourceAllocation.MemoryOvercommitRatio;

                            toolStripStatusLabelTextMainForm.Text = $@"Health inventory loaded - VMs: {totalVMs} ({runningVMs} running), CPU Overcommit: {cpuOvercommit:F2}:1, Memory Overcommit: {memOvercommit:F2}:1";

                            Message($"Host health inventory loaded successfully",
                                EventType.Information, 7104);
                        }
                        else
                        {
                            Message("No host inventory data retrieved",
                                EventType.Warning, 7105);

                            toolStripStatusLabelTextMainForm.Text = @"No health data available";

                            MessageBox.Show(@"No host inventory data found or error retrieving data.",
                                @"No Data",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        Message($"Error updating health overview UI: {ex.Message}",
                            EventType.Error, 7106);

                        MessageBox.Show($@"Error updating health overview: {ex.Message}",
                            @"Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        toolStripStatusLabelTextMainForm.Text = @"Error loading health data";
                    }

                }, "Host Health Inventory");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading host health inventory: {ex.Message}";
                Message(errorMsg, EventType.Error, 7107);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabelTextMainForm.Text = @"Error loading health inventory";
            }
        }

        /// <summary>
        /// Updates the cluster node selector dropdown for cluster environments
        /// </summary>
        private void UpdateClusterNodeSelector(HostHealthInfo inventory)
        {
            try
            {
                bool isCluster = !string.IsNullOrEmpty(inventory.HostInfo.ClusterName) &&
                                 inventory.HostInfo.ClusterName != "N/A" &&
                                 inventory.HostInfo.ClusterNodes.Count > 0;

                if (isCluster)
                {
                    // Show the node selector
                    labelClusterNodeSelector.Visible = true;
                    comboBoxClusterNodeSelector.Visible = true;

                    // Prevent triggering SelectedIndexChanged while populating
                    _isLoadingNodeData = true;

                    // Clear and populate the dropdown
                    comboBoxClusterNodeSelector.Items.Clear();

                    foreach (var node in inventory.HostInfo.ClusterNodes)
                    {
                        string displayText = node.IsCurrentNode
                            ? $"➤ {node.Name} ({node.State}) - Current session"
                            : $"{node.Name} ({node.State})";
                        comboBoxClusterNodeSelector.Items.Add(new ClusterNodeComboItem(node.Name, node.Fqdn, displayText, node.IsCurrentNode));

                        // Select the current node
                        if (node.IsCurrentNode)
                        {
                            comboBoxClusterNodeSelector.SelectedIndex = comboBoxClusterNodeSelector.Items.Count - 1;
                        }
                    }

                    _isLoadingNodeData = false;

                    Message($"Cluster node selector populated with {inventory.HostInfo.ClusterNodes.Count} nodes",
                        EventType.Information, 7120);
                }
                else
                {
                    // Hide the node selector for standalone hosts
                    labelClusterNodeSelector.Visible = false;
                    comboBoxClusterNodeSelector.Visible = false;
                    comboBoxClusterNodeSelector.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                Message($"Error updating cluster node selector: {ex.Message}",
                    EventType.Warning, 7121);
            }
        }

        /// <summary>
        /// Handles node selection change in the cluster node dropdown
        /// </summary>
        private void comboBoxClusterNodeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Skip if we're loading data or no item selected
            if (_isLoadingNodeData || comboBoxClusterNodeSelector.SelectedItem == null)
                return;

            var selectedItem = comboBoxClusterNodeSelector.SelectedItem as ClusterNodeComboItem;
            if (selectedItem == null)
                return;

            // Check if we're already displaying this node's data (by comparing node names)
            bool isAlreadyDisplayed = !string.IsNullOrEmpty(_currentlyDisplayedNodeName) &&
                                      _currentlyDisplayedNodeName.Equals(selectedItem.NodeName, StringComparison.OrdinalIgnoreCase);

            if (isAlreadyDisplayed)
            {
                Message($"Already viewing data from node: {selectedItem.NodeName}",
                    EventType.Information, 7122);
                return;
            }

            // Show message about switching nodes
            var result = MessageBox.Show(
                $@"Switch to view health data from node '{selectedItem.NodeName}'?

" +
                @"Note: This will execute a remote PowerShell connection to the selected node to gather its health data. " +
                @"The current connection will remain on the original node.",
                @"Switch Cluster Node View",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Use FQDN for remote connection, fall back to short name if FQDN is not available
                string connectionName = !string.IsNullOrEmpty(selectedItem.NodeFqdn)
                    ? selectedItem.NodeFqdn
                    : selectedItem.NodeName;
                LoadHealthDataFromNode(connectionName, selectedItem.NodeName);
            }
            else
            {
                // Reset selection to the currently displayed node
                _isLoadingNodeData = true;
                for (int i = 0; i < comboBoxClusterNodeSelector.Items.Count; i++)
                {
                    if (comboBoxClusterNodeSelector.Items[i] is ClusterNodeComboItem item &&
                        item.NodeName.Equals(_currentlyDisplayedNodeName, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxClusterNodeSelector.SelectedIndex = i;
                        break;
                    }
                }
                _isLoadingNodeData = false;
            }
        }

        /// <summary>
        /// Loads health data from a specific cluster node
        /// </summary>
        /// <param name="connectionName">The FQDN or connection string to use for the remote connection</param>
        /// <param name="shortNodeName">The short node name for display and tracking (optional, extracted from connectionName if not provided)</param>
        private void LoadHealthDataFromNode(string connectionName, string? shortNodeName = null)
        {
            // Extract short name from connection name if not provided
            shortNodeName ??= connectionName.Contains('.')
                ? connectionName[..connectionName.IndexOf('.')]
                : connectionName;

            try
            {
                Message($"User requested health data from cluster node: {shortNodeName} (connection: {connectionName})",
                    EventType.Information, 7123);

                toolStripStatusLabelTextMainForm.Text = $@"Loading health data from node '{shortNodeName}'...";

                // Execute with progress form
                ExecuteWithProgressForm<HostHealthInfo?>(() =>
                {
                    // Execute the health script on the specific node
                    Message($"Executing health script on node '{connectionName}'...",
                        EventType.Information, 7124);

                    return HostHealth.GetHyperVHostHealth(
                        cmd => ExecutePowerShellCommandOnNode(connectionName, cmd),
                        (node, cmd) => ExecutePowerShellCommandOnNode(node, cmd),
                        includeDetailedVMs: true);

                }, (inventory) =>
                {
                    try
                    {
                        if (inventory != null)
                        {
                            Message($"Retrieved health inventory from node '{shortNodeName}'",
                                EventType.Information, 7125);

                            // Track which node's data we're now displaying
                            _currentlyDisplayedNodeName = inventory.HostInfo.ComputerName;

                            // Update the DataGridView with data from the selected node
                            UpdateHealthOverviewDataGridView(inventory);

                            // Update status
                            int totalVMs = inventory.WorkloadAnalysis.TotalVMs;
                            int runningVMs = inventory.WorkloadAnalysis.RunningVMs;

                            toolStripStatusLabelTextMainForm.Text = $@"Health data from '{shortNodeName}' - VMs: {totalVMs} ({runningVMs} running)";
                        }
                        else
                        {
                            Message($"Failed to retrieve health data from node '{shortNodeName}'",
                                EventType.Warning, 7126);

                            MessageBox.Show($@"Could not retrieve health data from node '{shortNodeName}'.

" +
                                @"The node may be offline or inaccessible.",
                                @"Node Data Unavailable",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            toolStripStatusLabelTextMainForm.Text = $@"Failed to load data from '{shortNodeName}'";

                            // Reset selection to the currently displayed node
                            _isLoadingNodeData = true;
                            for (int i = 0; i < comboBoxClusterNodeSelector.Items.Count; i++)
                            {
                                if (comboBoxClusterNodeSelector.Items[i] is ClusterNodeComboItem item &&
                                    item.NodeName.Equals(_currentlyDisplayedNodeName, StringComparison.OrdinalIgnoreCase))
                                {
                                    comboBoxClusterNodeSelector.SelectedIndex = i;
                                    break;
                                }
                            }
                            _isLoadingNodeData = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Message($"Error processing health data from node '{shortNodeName}': {ex.Message}",
                            EventType.Error, 7127);
                    }

                }, $"Loading health data from {shortNodeName}");
            }
            catch (Exception ex)
            {
                Message($"Error loading health data from node '{shortNodeName}': {ex.Message}",
                    EventType.Error, 7128);

                toolStripStatusLabelTextMainForm.Text = $@"Error loading data from '{shortNodeName}'";
            }
        }


        /// <summary>
        /// Helper class for cluster node combobox items
        /// </summary>
        private class ClusterNodeComboItem
        {
            public string NodeName { get; }
            public string NodeFqdn { get; }
            public string DisplayText { get; }

            public ClusterNodeComboItem(string nodeName, string nodeFqdn, string displayText, bool isCurrentNode)
            {
                NodeName = nodeName;
                NodeFqdn = nodeFqdn;
                DisplayText = displayText;
            }

            public override string ToString() => DisplayText;
        }

        /// <summary>
        /// Updates the datagridviewHealthOverview DataGridView with host inventory details
        /// </summary>
        private void UpdateHealthOverviewDataGridView(HostHealthInfo inventory)
        {
            try
            {
                if (datagridviewHealthOverview == null)
                {
                    Message("datagridviewHealthOverview control not found",
                        EventType.Warning, 7110);
                    return;
                }

                // Clear existing data
                datagridviewHealthOverview.DataSource = null;
                datagridviewHealthOverview.Rows.Clear();
                datagridviewHealthOverview.Columns.Clear();

                if (inventory == null)
                {
                    Message("No host inventory to display",
                        EventType.Information, 7111);
                    return;
                }


                Message($"Updating datagridviewHealthOverview with host inventory data",
                    EventType.Information, 7112);

                // Create DataTable with category and value columns for a property grid-like view
                var dataTable = new DataTable();
                dataTable.Columns.Add("Category", typeof(string));
                dataTable.Columns.Add("Property", typeof(string));
                dataTable.Columns.Add("Value", typeof(string));
                dataTable.Columns.Add("Details", typeof(string));
                dataTable.Columns.Add("Status", typeof(string));
                dataTable.Columns.Add("Help", typeof(string));

                // Host Information section - simplified to just hostname and cluster info
                bool isCluster = !string.IsNullOrEmpty(inventory.HostInfo.ClusterName) &&
                                 inventory.HostInfo.ClusterName != "N/A";

                if (isCluster)
                {
                    // Cluster mode - show cluster overview with node counts
                    int nodeCount = inventory.HostInfo.ClusterNodeCount;
                    int nodesOnline = inventory.HostInfo.ClusterNodesOnline;
                    int nodesOffline = inventory.HostInfo.ClusterNodesOffline;
                    int nodesPaused = inventory.HostInfo.ClusterNodesPaused;

                    // Determine cluster health status
                    string clusterStatus = nodesOffline > 0 ? "Critical" : nodesPaused > 0 ? "Warning" : "Good";
                    string nodeStatusText = $"{nodesOnline} online";
                    if (nodesOffline > 0) nodeStatusText += $", {nodesOffline} offline";
                    if (nodesPaused > 0) nodeStatusText += $", {nodesPaused} paused";

                    // Build node list for details
                    string nodeList = "";
                    if (inventory.HostInfo.ClusterNodes.Count > 0)
                    {
                        var nodeNames = inventory.HostInfo.ClusterNodes
                            .Select(n => n.IsCurrentNode ? $"➤ {n.Name} ({n.State})" : $"{n.Name} ({n.State})");
                        nodeList = string.Join(" | ", nodeNames);
                    }

                    AddInventoryRow(dataTable, "🖥️ Cluster", "Cluster Name", inventory.HostInfo.ClusterName,
                        $"{nodeCount} nodes: {nodeStatusText}",
                        clusterStatus,
                        "Failover cluster overview. ➤ indicates current connected node.");

                    AddInventoryRow(dataTable, "🖥️ Cluster", "Cluster Nodes", nodeList,
                        $"Current Node: {inventory.HostInfo.ComputerName}",
                        GetNodeStateStatus(inventory.HostInfo.NodeState),
                        "All cluster nodes and their states. Data shown is from the current connected node only.");
                }
                else
                {
                    // Standalone mode - show hostname
                    AddInventoryRow(dataTable, "🖥️ Host", "Hostname", inventory.HostInfo.ComputerName,
                        $"Standalone Hyper-V Host | {inventory.HostInfo.HyperVVersion}", "",
                        "Standalone Hyper-V host server (not part of a cluster)");
                }

                // Resource Allocation section
                double memoryAllocatedGb = inventory.ResourceAllocation.TotalVmMemoryMb / 1024.0;
                string cpuGuidance = inventory.ResourceAllocation.CpuOvercommitRatio > 4 ? "⚠️ High overcommit" :
                                     inventory.ResourceAllocation.CpuOvercommitRatio > 2 ? "⚡ Moderate" : "✅ Good";
                string memGuidance = inventory.ResourceAllocation.MemoryOvercommitRatio > 1.5 ? "⚠️ High overcommit" :
                                     inventory.ResourceAllocation.MemoryOvercommitRatio > 1.2 ? "⚡ Moderate" : "✅ Good";

                // Show resource context (note if cluster - data is from current node only)
                string resourceContext = isCluster ? " (Current Node)" : "";
                AddInventoryRow(dataTable, "📊 Resource Allocation", $"Physical Resources{resourceContext}",
                    $"{inventory.HostInfo.PhysicalProcessors} cores | {inventory.HostInfo.TotalMemoryGb:F1} GB RAM",
                    $"Logical CPUs: {inventory.HostInfo.LogicalProcessors} | Sockets: {inventory.HostInfo.ProcessorSockets}", "",
                    isCluster ? "Physical resources on the current connected node only" : "Physical CPU cores and RAM available on this host");
                AddInventoryRow(dataTable, "📊 Resource Allocation", "VM Processors Allocated", $"{inventory.ResourceAllocation.TotalVmProcessors} vCPUs",
                    $"{cpuGuidance} - Overcommit Ratio: {inventory.ResourceAllocation.CpuOvercommitRatio:F2}:1",
                    GetOvercommitStatus(inventory.ResourceAllocation.CpuOvercommitRatio, "cpu"),
                    "CPU Overcommit = Total vCPUs ÷ Physical Cores. Ratio >4:1 may cause contention. Reduce VM CPU counts or add physical cores.");
                AddInventoryRow(dataTable, "📊 Resource Allocation", "VM Memory Allocated", $"{memoryAllocatedGb:F1} GB",
                    $"{memGuidance} - Overcommit Ratio: {inventory.ResourceAllocation.MemoryOvercommitRatio:F2}:1",
                    GetOvercommitStatus(inventory.ResourceAllocation.MemoryOvercommitRatio, "memory"),
                    "Memory Overcommit = VM Memory ÷ Physical Memory. Ratio >1.2:1 requires Dynamic Memory. Enable Dynamic Memory on VMs or add physical RAM.");

                // Performance Data section
                if (inventory.PerformanceData.DataAvailable)
                {
                    string cpuStatus = inventory.PerformanceData.CpuUsagePercent > 80 ? "⚠️ High" :
                                       inventory.PerformanceData.CpuUsagePercent > 60 ? "⚡ Moderate" : "✅ Normal";
                    string memStatus = inventory.PerformanceData.MemoryUsagePercent > 85 ? "⚠️ High" :
                                       inventory.PerformanceData.MemoryUsagePercent > 70 ? "⚡ Moderate" : "✅ Normal";


                    AddInventoryRow(dataTable, "⚡ Performance", "CPU Usage", $"{inventory.PerformanceData.CpuUsagePercent:F1}%",
                        $"{cpuStatus} usage level",
                        GetPerformanceStatus(inventory.PerformanceData.CpuUsagePercent, "cpu"),
                        "Current CPU utilization. >80% sustained may indicate need for more CPU cores or VM CPU reduction.");
                    AddInventoryRow(dataTable, "⚡ Performance", "Memory Usage", $"{inventory.PerformanceData.MemoryUsagePercent:F1}%",
                        $"{memStatus} - Available: {inventory.PerformanceData.AvailableMemoryMb:F0} MB",
                        GetPerformanceStatus(inventory.PerformanceData.MemoryUsagePercent, "memory"),
                        "Current memory utilization. >85% may cause performance issues. Consider adding RAM or enabling Dynamic Memory.");
                }
                else
                {
                    AddInventoryRow(dataTable, "⚡ Performance", "Performance Data", "Not Available",
                        "Performance counters could not be retrieved", "Warning",
                        "Performance counters could not be retrieved from the host");
                }

                // Workload Analysis section - consolidated view
                AddInventoryRow(dataTable, "🖥️ VM Workload", "Total VMs", inventory.WorkloadAnalysis.TotalVMs.ToString(),
                    $"Running: {inventory.WorkloadAnalysis.RunningVMs} | Stopped: {inventory.WorkloadAnalysis.StoppedVMs}", "",
                    "Summary of all virtual machines on this host");

                // Only show Paused/Saved if there are any
                if (inventory.WorkloadAnalysis.PausedVMs > 0 || inventory.WorkloadAnalysis.SavedVMs > 0)
                {
                    AddInventoryRow(dataTable, "🖥️ VM Workload", "Other VM States",
                        $"Paused: {inventory.WorkloadAnalysis.PausedVMs} | Saved: {inventory.WorkloadAnalysis.SavedVMs}",
                        inventory.WorkloadAnalysis.PausedVMs > 0 ? "⚠️ Paused VMs may indicate issues" : "",
                        inventory.WorkloadAnalysis.PausedVMs > 0 ? "Warning" : "Info",
                        "VMs in paused or saved state. Paused VMs may indicate resource issues.");
                }

                AddInventoryRow(dataTable, "🖥️ VM Workload", "VM Generations",
                    $"Gen 1: {inventory.WorkloadAnalysis.Generation1VMs} | Gen 2: {inventory.WorkloadAnalysis.Generation2VMs}",
                    $"Replicated: {inventory.WorkloadAnalysis.ReplicatedVMs} | With Checkpoints: {inventory.WorkloadAnalysis.CheckpointedVMs}",
                    inventory.WorkloadAnalysis.CheckpointedVMs > 5 ? "Warning" : "",
                    "Gen2 VMs offer better performance and security. Consider upgrading Gen1 VMs when possible.");

                // Storage Information section - consolidated per drive
                foreach (var storage in inventory.StorageInfo)
                {
                    string driveStatus = storage.UsedPercent > 90 ? "Critical" : storage.UsedPercent > 75 ? "Warning" : "Good";
                    string storageGuidance = storage.UsedPercent > 90 ? "⚠️ Critical - Clean up space immediately" :
                                             storage.UsedPercent > 80 ? "⚡ High usage - Monitor closely" :
                                             storage.UsedPercent > 70 ? "⚡ Growing - Plan for expansion" : "✅ Healthy";

                    AddInventoryRow(dataTable, "💾 Storage", $"Drive {storage.DriveLetter}",
                        $"{storage.UsedGb:F1} GB / {storage.TotalGb:F1} GB ({storage.UsedPercent:F1}%)",
                        $"{storageGuidance} - Free: {storage.FreeGb:F1} GB | VM Files: {storage.VmFileCount}",
                        driveStatus,
                        $"Storage usage on drive {storage.DriveLetter}. >90% usage can cause VM performance issues and prevent snapshots.");
                }

                // Network Information section
                foreach (var network in inventory.NetworkInfo)
                {
                    string virtualSwitchStatus = network.VirtualSwitches == 0 ? "Warning" : "Good";
                    string netGuidance = network.VirtualSwitches == 0 ? "⚠️ Unused adapter" : "✅ In use";

                    AddInventoryRow(dataTable, "🌐 Network", network.Name, network.InterfaceDescription,
                        $"{netGuidance} - Virtual Switches: {network.VirtualSwitches}",
                        virtualSwitchStatus,
                        "Physical network adapter. Unused adapters can be configured for VM networking or failover.");
                }

                // Idle Resources section
                int idleVmCount = inventory.IdleResources.IdleVmNames.Count;
                int unusedAdapterCount = inventory.IdleResources.UnusedNetworkAdapterNames.Count;

                if (idleVmCount > 0)
                {
                    string idleVmList = idleVmCount <= 5
                        ? string.Join(", ", inventory.IdleResources.IdleVmNames)
                        : string.Join(", ", inventory.IdleResources.IdleVmNames.Take(5)) + $"... (+{idleVmCount - 5} more)";
                    AddInventoryRow(dataTable, "💤 Idle Resources", "Idle VMs", idleVmCount.ToString(),
                        $"⚠️ VMs stopped for >30 days: {idleVmList}", "Warning",
                        "VMs that have been powered off for over 30 days. These may be candidates for deletion to reclaim storage and licensing.");
                }

                if (unusedAdapterCount > 0)
                {
                    string unusedAdapterList = string.Join(", ", inventory.IdleResources.UnusedNetworkAdapterNames);
                    AddInventoryRow(dataTable, "💤 Idle Resources", "Unused Network Adapters", unusedAdapterCount.ToString(),
                        $"💡 Available for VM networking or teaming: {unusedAdapterList}", "Info",
                        "Physical network adapters not assigned to virtual switches. Can be used for additional VM networks or NIC teaming.");
                }

                // Timestamp
                AddInventoryRow(dataTable, "📅 Collection Info", "Data Collected At", inventory.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    "", "",
                    "Timestamp when this inventory data was collected");

                // Bind to DataGridView
                datagridviewHealthOverview.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewHealthOverview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewHealthOverview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewHealthOverview.MultiSelect = false;
                datagridviewHealthOverview.ReadOnly = true;
                datagridviewHealthOverview.AllowUserToAddRows = false;
                datagridviewHealthOverview.AllowUserToDeleteRows = false;
                datagridviewHealthOverview.RowHeadersVisible = false;
                datagridviewHealthOverview.AllowUserToResizeRows = false;

                // Set column widths
                if (datagridviewHealthOverview.Columns.Contains("Category"))
                    datagridviewHealthOverview.Columns["Category"].MinimumWidth = 150;
                if (datagridviewHealthOverview.Columns.Contains("Property"))
                    datagridviewHealthOverview.Columns["Property"].MinimumWidth = 180;
                if (datagridviewHealthOverview.Columns.Contains("Value"))
                    datagridviewHealthOverview.Columns["Value"].MinimumWidth = 200;
                if (datagridviewHealthOverview.Columns.Contains("Details"))
                    datagridviewHealthOverview.Columns["Details"].MinimumWidth = 300;
                if (datagridviewHealthOverview.Columns.Contains("Status"))
                    datagridviewHealthOverview.Columns["Status"].MinimumWidth = 80;
                if (datagridviewHealthOverview.Columns.Contains("Help"))
                    datagridviewHealthOverview.Columns["Help"].MinimumWidth = 350;

                // Apply color coding based on Status column
                ApplyHealthOverviewColorCoding();
#if DEBUG
                Message($"datagridviewHealthOverview updated successfully",
                    EventType.Information, 7113);
#endif
            }
            catch (Exception ex)
            {
                Message($"Error updating datagridviewHealthOverview: {ex.Message}",
                    EventType.Error, 7114);
            }
        }

        /// <summary>
        /// Adds a row to the inventory DataTable
        /// </summary>
        private void AddInventoryRow(DataTable dataTable, string category, string property, string value, string details, string status, string help = "")
        {
            var row = dataTable.NewRow();
            row["Category"] = category;
            row["Property"] = property;
            row["Value"] = value;
            row["Details"] = details;
            row["Status"] = status;
            row["Help"] = help;
            dataTable.Rows.Add(row);
        }

        /// <summary>
        /// Gets the status indicator for node state
        /// </summary>
        private string GetNodeStateStatus(string nodeState)
        {
            return nodeState switch
            {
                "Up" or "Online" => "Good",
                "Down" or "Offline" => "Critical",
                "Paused" => "Warning",
                "Standalone" => "Info",
                _ => ""
            };
        }

        /// <summary>
        /// Gets the status indicator for overcommit ratios
        /// </summary>
        private string GetOvercommitStatus(double ratio, string type)
        {
            if (type == "cpu")
            {
                if (ratio > 8) return "Critical";
                if (ratio > 4) return "Warning";
                return "Good";
            }
            else // memory
            {
                if (ratio > 1.5) return "Critical";
                if (ratio > 1.0) return "Warning";
                return "Good";
            }
        }

        /// <summary>
        /// Gets the status indicator for performance metrics
        /// </summary>
        private string GetPerformanceStatus(double value, string type)
        {
            if (type == "cpu" || type == "memory")
            {
                if (value > 90) return "Critical";
                if (value > 75) return "Warning";
                return "Good";
            }
            return "";
        }

        /// <summary>
        /// Formats link speed from bytes/sec to human-readable format
        /// </summary>
        private string FormatLinkSpeed(long linkSpeed)
        {
            if (linkSpeed >= 1_000_000_000_000)
                return $"{linkSpeed / 1_000_000_000_000.0:F1} Tbps";
            if (linkSpeed >= 1_000_000_000)
                return $"{linkSpeed / 1_000_000_000.0:F1} Gbps";
            if (linkSpeed >= 1_000_000)
                return $"{linkSpeed / 1_000_000.0:F1} Mbps";
            if (linkSpeed >= 1_000)
                return $"{linkSpeed / 1_000.0:F1} Kbps";
            return $"{linkSpeed} bps";
        }

        /// <summary>
        /// Applies color coding to the health overview DataGridView based on status
        /// </summary>
        private void ApplyHealthOverviewColorCoding()
        {
            foreach (DataGridViewRow row in datagridviewHealthOverview.Rows)
            {
                var status = row.Cells["Status"]?.Value?.ToString();
                var category = row.Cells["Category"]?.Value?.ToString();

                // Apply row background color based on category for grouping effect
                if (category?.Contains("Host Information") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.AliceBlue;
                }
                else if (category?.Contains("Resource Allocation") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.Honeydew;
                }
                else if (category?.Contains("Performance") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.LavenderBlush;
                }
                else if (category?.Contains("Workload") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.LightCyan;
                }
                else if (category?.Contains("Storage") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.Cornsilk;
                }
                else if (category?.Contains("Network") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.Lavender;
                }
                else if (category?.Contains("Idle") == true)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                }

                // Apply status cell color coding
                if (!string.IsNullOrEmpty(status))
                {
                    switch (status)
                    {
                        case "Good":
                            row.Cells["Status"].Style.BackColor = Color.LightGreen;
                            row.Cells["Status"].Style.ForeColor = Color.DarkGreen;
                            break;
                        case "Warning":
                            row.Cells["Status"].Style.BackColor = Color.LightYellow;
                            row.Cells["Status"].Style.ForeColor = Color.DarkOrange;
                            break;
                        case "Critical":
                            row.Cells["Status"].Style.BackColor = Color.LightCoral;
                            row.Cells["Status"].Style.ForeColor = Color.DarkRed;
                            break;
                        case "Info":
                            row.Cells["Status"].Style.BackColor = Color.LightBlue;
                            row.Cells["Status"].Style.ForeColor = Color.DarkBlue;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a summary of the health overview data
        /// </summary>
        private void buttonSummaryHealthOverview_Click(object sender, EventArgs e)
        {
            try
            {
                Message("User requested health overview summary",
                    EventType.Information, 7120);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show(@"No active Hyper-V connection. Please connect to a Hyper-V host first.",
                        @"Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have health data
                if (datagridviewHealthOverview == null || datagridviewHealthOverview.Rows.Count == 0)
                {
                    MessageBox.Show(@"No health data available. Please load health overview first.",
                        @"No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Count status indicators
                int goodCount = 0;
                int warningCount = 0;
                int criticalCount = 0;
                int infoCount = 0;

                foreach (DataGridViewRow row in datagridviewHealthOverview.Rows)
                {
                    var status = row.Cells["Status"]?.Value?.ToString();
                    switch (status)
                    {
                        case "Good": goodCount++; break;
                        case "Warning": warningCount++; break;
                        case "Critical": criticalCount++; break;
                        case "Info": infoCount++; break;
                    }
                }

                // Get key metrics from the grid
                string computerName = GetHealthGridValue("Computer Name");
                string totalVMs = GetHealthGridValue("Total VMs");
                string runningVMs = GetHealthGridValue("Running VMs");
                string cpuOvercommit = GetHealthGridValue("CPU Overcommit Ratio");
                string memoryOvercommit = GetHealthGridValue("Memory Overcommit Ratio");
                string cpuUsage = GetHealthGridValue("CPU Usage %");
                string memoryUsage = GetHealthGridValue("Memory Usage %");

                Cursor = Cursors.Default;

                // Determine overall health status
                string overallHealth;
                string healthEmoji;
                if (criticalCount > 0)
                {
                    overallHealth = "CRITICAL";
                    healthEmoji = "🔴";
                }
                else if (warningCount > 0)
                {
                    overallHealth = "WARNING";
                    healthEmoji = "🟡";
                }
                else
                {
                    overallHealth = "HEALTHY";
                    healthEmoji = "🟢";
                }

                string summaryText = $@"Host Health Summary - {computerName}

{healthEmoji} Overall Status: {overallHealth}

📊 Health Indicators:
• Good: {goodCount}
• Warning: {warningCount}
• Critical: {criticalCount}
• Info: {infoCount}

🖥️ VM Status:
• Total VMs: {totalVMs}
• Running VMs: {runningVMs}

📈 Resource Utilization:
• CPU Overcommit: {cpuOvercommit}
• Memory Overcommit: {memoryOvercommit}
• Current CPU Usage: {cpuUsage}
• Current Memory Usage: {memoryUsage}

💡 Recommendations:
{GetHealthRecommendations(criticalCount, warningCount, cpuOvercommit, memoryOvercommit)}";

                Message($"Health summary generated - Status: {overallHealth}",
                    EventType.Information, 7121);

                MessageBox.Show(summaryText,
                    @"Host Health Summary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;

                string errorMsg = $"Error generating health summary: {ex.Message}";
                Message(errorMsg, EventType.Error, 7122);

                MessageBox.Show(errorMsg,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets a value from the health overview grid by property name
        /// </summary>
        private string GetHealthGridValue(string propertyName)
        {
            foreach (DataGridViewRow row in datagridviewHealthOverview.Rows)
            {
                var property = row.Cells["Property"]?.Value?.ToString();
                if (property == propertyName)
                {
                    return row.Cells["Value"]?.Value?.ToString() ?? "N/A";
                }
            }
            return "N/A";
        }

        /// <summary>
        /// Generates health recommendations based on metrics
        /// </summary>
        private string GetHealthRecommendations(int criticalCount, int warningCount, string cpuOvercommit, string memoryOvercommit)
        {
            var recommendations = new List<string>();

            if (criticalCount > 0)
            {
                recommendations.Add("• ⚠️ Critical issues detected - investigate immediately");
            }

            if (warningCount > 0)
            {
                recommendations.Add($"• ⚡ {warningCount} warning(s) detected - review and address");
            }

            // Parse overcommit ratios
            if (double.TryParse(cpuOvercommit?.Replace(":1", ""), out double cpuRatio))
            {
                if (cpuRatio > 4)
                {
                    recommendations.Add($"• CPU overcommit ratio ({cpuRatio:F1}:1) is high - consider adding processors or reducing VM count");
                }
            }

            if (double.TryParse(memoryOvercommit?.Replace(":1", ""), out double memRatio))
            {
                if (memRatio > 1.0)
                {
                    recommendations.Add($"• Memory overcommit ratio ({memRatio:F1}:1) exceeds 1:1 - monitor for memory pressure");
                }
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("• ✅ Host appears healthy - no immediate actions required");
            }

            return string.Join("\n", recommendations);
        }

        private void buttonSummaryHealthOverviewHelp_Click(object sender, EventArgs e)
        {
            string helpText = @"🔍 Hyper-V Host Overview - Field Guide

📊 RESOURCE ALLOCATION RATIOS:

CPU Overcommit Ratio (vCPUs ÷ Physical Cores):
• 1:1 to 2:1   ✅ Excellent - Low contention risk
• 2:1 to 4:1   ⚡ Good - Monitor for performance
• 4:1 to 8:1   ⚠️ High - May cause CPU contention
• >8:1         🚨 Critical - Reduce VM CPUs or add cores

Memory Overcommit Ratio (VM Memory ÷ Physical Memory):
• Up to 1.2:1  ✅ Safe - Physical memory covers VMs
• 1.2:1 to 1.5:1 ⚡ Caution - Enable Dynamic Memory
• >1.5:1       ⚠️ Risk - Memory pressure likely

📈 PERFORMANCE THRESHOLDS:

CPU Usage:
• 0-60%   ✅ Normal operation
• 60-80%  ⚡ Moderate - monitor workloads
• >80%    ⚠️ High - consider CPU upgrade

Memory Usage:
• 0-70%   ✅ Healthy
• 70-85%  ⚡ Moderate - monitor closely  
• >85%    ⚠️ High - risk of performance issues

💾 STORAGE GUIDELINES:

• 0-70%   ✅ Healthy space
• 70-80%  ⚡ Plan expansion
• 80-90%  ⚠️ High usage - cleanup needed
• >90%    🚨 Critical - immediate action required

🔧 OPTIMIZATION RECOMMENDATIONS:

High CPU Overcommit:
• Reduce VM CPU counts for idle VMs
• Use CPU limits for non-critical VMs  
• Add physical CPU cores/sockets

High Memory Overcommit:
• Enable Dynamic Memory on VMs
• Set appropriate memory ranges
• Add physical RAM

High Storage Usage:
• Delete old checkpoints/snapshots
• Move VMs to other storage
• Expand storage capacity

Unused Resources:
• Remove idle VMs (stopped >30 days)
• Configure unused network adapters
• Consider VM consolidation";

            Message("User requested Health Overview help",
                EventType.Information, 7130);

            MessageBox.Show(helpText,
                @"Health Overview - Field Guide",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void downloadLastestReleaseFromGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Start GitHub repo
            string repoUrl = Globals.ToolStings.UrlGitHubDownload;

            try
            {
                Message("User requested to open GitHub repository",
                    EventType.Information, 7140);
                Process.Start(new ProcessStartInfo
                {
                    FileName = repoUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Message($"Error opening GitHub repository: {ex.Message}",
                    EventType.Error, 7141);
                MessageBox.Show($@"Unable to open GitHub repository. Please visit: {repoUrl}",
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
