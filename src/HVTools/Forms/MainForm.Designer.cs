namespace HVTools.Forms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            datagridviewVMOverView = new DataGridView();
            tabcontrolMainForm = new TabControl();
            tabpagehvOverview = new TabPage();
            buttonExportVMvmOverviewView = new Button();
            buttonSummaryhvOverviewView = new Button();
            buttonLoadVMsrefresh = new Button();
            labelOverviewHelpText = new Label();
            tabPagehvCPU = new TabPage();
            tabPagehvMemory = new TabPage();
            tabPagehvDisks = new TabPage();
            buttonSummaryvDiskView = new Button();
            buttonLoadvDiskrefresh = new Button();
            labelvDiskOverviewText = new Label();
            datagridviewvDiskOverView = new DataGridView();
            tabpagehvNetworking = new TabPage();
            tabpagehvCheckpoints = new TabPage();
            buttonSummaryvCheckpointsView = new Button();
            buttonLoadvCheckpointsrefresh = new Button();
            labelvCheckpointsOverviewText = new Label();
            datagridviewCheckpointOverView = new DataGridView();
            tabPagehvDVD = new TabPage();
            tabpagehvHosts = new TabPage();
            buttonSummaryvHostsView = new Button();
            buttonLoadHostsrefresh = new Button();
            datagridviewhvHosts = new DataGridView();
            label1 = new Label();
            tabpagehvClusters = new TabPage();
            datagridviewClusterVMs = new DataGridView();
            labelClusterVMs = new Label();
            datagridviewClusterNodes = new DataGridView();
            labelClusterNodes = new Label();
            groupBoxClusterInfo = new GroupBox();
            labelSharedVolumesValue = new Label();
            labelSharedVolumes = new Label();
            labelClusterNetworksValue = new Label();
            labelClusterNetworks = new Label();
            labelCurrentNodeValue = new Label();
            labelCurrentNode = new Label();
            labelTotalNodesValue = new Label();
            labelTotalNodes = new Label();
            labelClusterNameValue = new Label();
            labelClusterName = new Label();
            buttonRefreshClusterInfo = new Button();
            labelClustersHelpText = new Label();
            buttonSummaryClustersOverviewView = new Button();
            tabpagehvStorage = new TabPage();
            labelvStorageOverviewText = new Label();
            tabPagehvReplication = new TabPage();
            tabpagehvResources = new TabPage();
            tabpageManageNetwork = new TabPage();
            tabpagehvSecurity = new TabPage();
            tabpagehvPerformance = new TabPage();
            tabpagehvCompliance = new TabPage();
            tabpagehvInventory = new TabPage();
            tabpageCreateVM = new TabPage();
            tabpageVMGroups = new TabPage();
            groupBox2 = new GroupBox();
            buttonManageServerMembers = new Button();
            groupBox1 = new GroupBox();
            buttonRenameSelectedVMGrou = new Button();
            buttonDeleteSelectedVMGrou = new Button();
            buttonCreateANewVMGroup = new Button();
            buttonLoadGroupsrefresh = new Button();
            labelThisViewProvidesOver = new Label();
            datagridviewVMGroups = new DataGridView();
            tabpageHealthOverview = new TabPage();
            comboBoxClusterNodeSelector = new ComboBox();
            labelClusterNodeSelector = new Label();
            buttonSummaryHealthOverviewHelp = new Button();
            datagridviewHealthOverview = new DataGridView();
            buttonExportHealthOverview = new Button();
            buttonSummaryHealthOverviewView = new Button();
            buttonLoadHealthOverview = new Button();
            labelHealthOverviewText = new Label();
            panelSearch = new Panel();
            buttonCloseSearch = new Button();
            checkBoxFilterResults = new CheckBox();
            labelSearchResults = new Label();
            buttonSearchNext = new Button();
            buttonSearchPrevious = new Button();
            labelSearchIcon = new Label();
            textBoxSearch = new TextBox();
            menuStripTopMainForm = new MenuStrip();
            menuToolStripMenuItem = new ToolStripMenuItem();
            disconnectToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            onlineToolStripMenuItem = new ToolStripMenuItem();
            myWebpageToolStripMenuItem = new ToolStripMenuItem();
            myBlogToolStripMenuItem = new ToolStripMenuItem();
            guideToolStripMenuItem = new ToolStripMenuItem();
            logsToolStripMenuItem = new ToolStripMenuItem();
            openLogFolderToolStripMenuItem = new ToolStripMenuItem();
            openLogForTodayToolStripMenuItem = new ToolStripMenuItem();
            downloadLastestReleaseFromGitHubToolStripMenuItem = new ToolStripMenuItem();
            changelogToolStripMenuItem = new ToolStripMenuItem();
            exportDataToolStripMenuItem = new ToolStripMenuItem();
            exportAllDataToolStripMenuItem = new ToolStripMenuItem();
            exportCurrentTabToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            refreshDataToolStripMenuItem = new ToolStripMenuItem();
            clearCacheToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            expandAllCollumsToolStripMenuItem = new ToolStripMenuItem();
            autoSizeAllCollumsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            autoRefreshToolStripMenuItem = new ToolStripMenuItem();
            disabledMinuteToolStripMenuItem = new ToolStripMenuItem();
            every1MinuteToolStripMenuItem = new ToolStripMenuItem();
            every5MinutesToolStripMenuItem = new ToolStripMenuItem();
            every10MinutesToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            pictureboxSupportMe = new PictureBox();
            statusStripMainForm = new StatusStrip();
            toolStripStatusLabelMainForm = new ToolStripStatusLabel();
            toolStripStatusLabelTextMainForm = new ToolStripStatusLabel();
            toolstripstatuslabelMain_CreatedBy = new Label();
            copySelectionToClipboardToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).BeginInit();
            tabcontrolMainForm.SuspendLayout();
            tabpagehvOverview.SuspendLayout();
            tabPagehvDisks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewvDiskOverView).BeginInit();
            tabpagehvCheckpoints.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewCheckpointOverView).BeginInit();
            tabpagehvHosts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewhvHosts).BeginInit();
            tabpagehvClusters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterVMs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterNodes).BeginInit();
            groupBoxClusterInfo.SuspendLayout();
            tabpagehvStorage.SuspendLayout();
            tabpageVMGroups.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewVMGroups).BeginInit();
            tabpageHealthOverview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewHealthOverview).BeginInit();
            panelSearch.SuspendLayout();
            menuStripTopMainForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureboxSupportMe).BeginInit();
            statusStripMainForm.SuspendLayout();
            SuspendLayout();
            // 
            // datagridviewVMOverView
            // 
            datagridviewVMOverView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewVMOverView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewVMOverView.Location = new Point(6, 35);
            datagridviewVMOverView.Name = "datagridviewVMOverView";
            datagridviewVMOverView.Size = new Size(1601, 774);
            datagridviewVMOverView.TabIndex = 0;
            datagridviewVMOverView.CellContentDoubleClick += datagridviewVMOverView_CellContentDoubleClick;
            datagridviewVMOverView.ColumnHeaderMouseClick += DatagridviewVMOverView_ColumnHeaderMouseClick;
            datagridviewVMOverView.DataBindingComplete += DatagridviewVMOverView_DataBindingComplete;
            // 
            // tabcontrolMainForm
            // 
            tabcontrolMainForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabcontrolMainForm.Controls.Add(tabpagehvOverview);
            tabcontrolMainForm.Controls.Add(tabPagehvCPU);
            tabcontrolMainForm.Controls.Add(tabPagehvMemory);
            tabcontrolMainForm.Controls.Add(tabPagehvDisks);
            tabcontrolMainForm.Controls.Add(tabpagehvNetworking);
            tabcontrolMainForm.Controls.Add(tabpagehvCheckpoints);
            tabcontrolMainForm.Controls.Add(tabPagehvDVD);
            tabcontrolMainForm.Controls.Add(tabpagehvHosts);
            tabcontrolMainForm.Controls.Add(tabpagehvClusters);
            tabcontrolMainForm.Controls.Add(tabpagehvStorage);
            tabcontrolMainForm.Controls.Add(tabPagehvReplication);
            tabcontrolMainForm.Controls.Add(tabpagehvResources);
            tabcontrolMainForm.Controls.Add(tabpageManageNetwork);
            tabcontrolMainForm.Controls.Add(tabpagehvSecurity);
            tabcontrolMainForm.Controls.Add(tabpagehvPerformance);
            tabcontrolMainForm.Controls.Add(tabpagehvCompliance);
            tabcontrolMainForm.Controls.Add(tabpagehvInventory);
            tabcontrolMainForm.Controls.Add(tabpageCreateVM);
            tabcontrolMainForm.Controls.Add(tabpageVMGroups);
            tabcontrolMainForm.Controls.Add(tabpageHealthOverview);
            tabcontrolMainForm.Location = new Point(12, 27);
            tabcontrolMainForm.Name = "tabcontrolMainForm";
            tabcontrolMainForm.SelectedIndex = 0;
            tabcontrolMainForm.Size = new Size(1621, 843);
            tabcontrolMainForm.TabIndex = 1;
            // 
            // tabpagehvOverview
            // 
            tabpagehvOverview.Controls.Add(buttonExportVMvmOverviewView);
            tabpagehvOverview.Controls.Add(buttonSummaryhvOverviewView);
            tabpagehvOverview.Controls.Add(buttonLoadVMsrefresh);
            tabpagehvOverview.Controls.Add(labelOverviewHelpText);
            tabpagehvOverview.Controls.Add(datagridviewVMOverView);
            tabpagehvOverview.Location = new Point(4, 24);
            tabpagehvOverview.Name = "tabpagehvOverview";
            tabpagehvOverview.Padding = new Padding(3);
            tabpagehvOverview.Size = new Size(1613, 815);
            tabpagehvOverview.TabIndex = 0;
            tabpagehvOverview.Text = "vInfo";
            tabpagehvOverview.UseVisualStyleBackColor = true;
            // 
            // buttonExportVMvmOverviewView
            // 
            buttonExportVMvmOverviewView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExportVMvmOverviewView.Location = new Point(1329, 6);
            buttonExportVMvmOverviewView.Name = "buttonExportVMvmOverviewView";
            buttonExportVMvmOverviewView.Size = new Size(81, 23);
            buttonExportVMvmOverviewView.TabIndex = 4;
            buttonExportVMvmOverviewView.Text = "Export VM´s";
            buttonExportVMvmOverviewView.UseVisualStyleBackColor = true;
            buttonExportVMvmOverviewView.Click += buttonExportVMvmOverviewView_Click;
            // 
            // buttonSummaryhvOverviewView
            // 
            buttonSummaryhvOverviewView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryhvOverviewView.Location = new Point(1416, 6);
            buttonSummaryhvOverviewView.Name = "buttonSummaryhvOverviewView";
            buttonSummaryhvOverviewView.Size = new Size(75, 23);
            buttonSummaryhvOverviewView.TabIndex = 3;
            buttonSummaryhvOverviewView.Text = "Summary";
            buttonSummaryhvOverviewView.UseVisualStyleBackColor = true;
            buttonSummaryhvOverviewView.Click += buttonSummaryhvOverviewView_Click;
            // 
            // buttonLoadVMsrefresh
            // 
            buttonLoadVMsrefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadVMsrefresh.Location = new Point(1497, 6);
            buttonLoadVMsrefresh.Name = "buttonLoadVMsrefresh";
            buttonLoadVMsrefresh.Size = new Size(110, 23);
            buttonLoadVMsrefresh.TabIndex = 2;
            buttonLoadVMsrefresh.Text = "&Load VMs/refresh";
            buttonLoadVMsrefresh.UseVisualStyleBackColor = true;
            buttonLoadVMsrefresh.Click += buttonLoadVMsrefresh_Click;
            // 
            // labelOverviewHelpText
            // 
            labelOverviewHelpText.AutoSize = true;
            labelOverviewHelpText.Location = new Point(6, 3);
            labelOverviewHelpText.Name = "labelOverviewHelpText";
            labelOverviewHelpText.Size = new Size(601, 30);
            labelOverviewHelpText.TabIndex = 1;
            labelOverviewHelpText.Text = "This view provides provides overview and core functionality within the Hyper-V space for information about VMs\r\nand other data that extends that functionality over multiple servers.";
            // 
            // tabPagehvCPU
            // 
            tabPagehvCPU.Location = new Point(4, 24);
            tabPagehvCPU.Name = "tabPagehvCPU";
            tabPagehvCPU.Size = new Size(1613, 815);
            tabPagehvCPU.TabIndex = 17;
            tabPagehvCPU.Text = "vCPU";
            tabPagehvCPU.UseVisualStyleBackColor = true;
            // 
            // tabPagehvMemory
            // 
            tabPagehvMemory.Location = new Point(4, 24);
            tabPagehvMemory.Name = "tabPagehvMemory";
            tabPagehvMemory.Size = new Size(1613, 815);
            tabPagehvMemory.TabIndex = 18;
            tabPagehvMemory.Text = "vMemory";
            tabPagehvMemory.UseVisualStyleBackColor = true;
            // 
            // tabPagehvDisks
            // 
            tabPagehvDisks.Controls.Add(buttonSummaryvDiskView);
            tabPagehvDisks.Controls.Add(buttonLoadvDiskrefresh);
            tabPagehvDisks.Controls.Add(labelvDiskOverviewText);
            tabPagehvDisks.Controls.Add(datagridviewvDiskOverView);
            tabPagehvDisks.Location = new Point(4, 24);
            tabPagehvDisks.Name = "tabPagehvDisks";
            tabPagehvDisks.Size = new Size(1613, 815);
            tabPagehvDisks.TabIndex = 16;
            tabPagehvDisks.Text = "vDisks";
            tabPagehvDisks.UseVisualStyleBackColor = true;
            // 
            // buttonSummaryvDiskView
            // 
            buttonSummaryvDiskView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryvDiskView.Location = new Point(1410, 6);
            buttonSummaryvDiskView.Name = "buttonSummaryvDiskView";
            buttonSummaryvDiskView.Size = new Size(75, 23);
            buttonSummaryvDiskView.TabIndex = 7;
            buttonSummaryvDiskView.Text = "Summary";
            buttonSummaryvDiskView.UseVisualStyleBackColor = true;
            buttonSummaryvDiskView.Click += buttonSummaryvDiskView_Click;
            // 
            // buttonLoadvDiskrefresh
            // 
            buttonLoadvDiskrefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadvDiskrefresh.Location = new Point(1491, 6);
            buttonLoadvDiskrefresh.Name = "buttonLoadvDiskrefresh";
            buttonLoadvDiskrefresh.Size = new Size(116, 23);
            buttonLoadvDiskrefresh.TabIndex = 6;
            buttonLoadvDiskrefresh.Text = "&Load vDisk/refresh";
            buttonLoadvDiskrefresh.UseVisualStyleBackColor = true;
            buttonLoadvDiskrefresh.Click += buttonLoadvDiskrefresh_Click;
            // 
            // labelvDiskOverviewText
            // 
            labelvDiskOverviewText.AutoSize = true;
            labelvDiskOverviewText.Location = new Point(6, 3);
            labelvDiskOverviewText.Name = "labelvDiskOverviewText";
            labelvDiskOverviewText.Size = new Size(486, 30);
            labelvDiskOverviewText.TabIndex = 5;
            labelvDiskOverviewText.Text = "This view provides provides overview and core functionality within the Hyper-V space for\r\ninformation about VMs and other data that extends that functionality over multiple servers.";
            // 
            // datagridviewvDiskOverView
            // 
            datagridviewvDiskOverView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewvDiskOverView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewvDiskOverView.Location = new Point(6, 35);
            datagridviewvDiskOverView.Name = "datagridviewvDiskOverView";
            datagridviewvDiskOverView.Size = new Size(1601, 774);
            datagridviewvDiskOverView.TabIndex = 4;
            // 
            // tabpagehvNetworking
            // 
            tabpagehvNetworking.Location = new Point(4, 24);
            tabpagehvNetworking.Name = "tabpagehvNetworking";
            tabpagehvNetworking.Size = new Size(1613, 815);
            tabpagehvNetworking.TabIndex = 6;
            tabpagehvNetworking.Text = "vNetwork";
            tabpagehvNetworking.UseVisualStyleBackColor = true;
            // 
            // tabpagehvCheckpoints
            // 
            tabpagehvCheckpoints.Controls.Add(buttonSummaryvCheckpointsView);
            tabpagehvCheckpoints.Controls.Add(buttonLoadvCheckpointsrefresh);
            tabpagehvCheckpoints.Controls.Add(labelvCheckpointsOverviewText);
            tabpagehvCheckpoints.Controls.Add(datagridviewCheckpointOverView);
            tabpagehvCheckpoints.Location = new Point(4, 24);
            tabpagehvCheckpoints.Name = "tabpagehvCheckpoints";
            tabpagehvCheckpoints.Size = new Size(1613, 815);
            tabpagehvCheckpoints.TabIndex = 7;
            tabpagehvCheckpoints.Text = "vCheckpoint";
            tabpagehvCheckpoints.UseVisualStyleBackColor = true;
            // 
            // buttonSummaryvCheckpointsView
            // 
            buttonSummaryvCheckpointsView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryvCheckpointsView.Location = new Point(1372, 6);
            buttonSummaryvCheckpointsView.Name = "buttonSummaryvCheckpointsView";
            buttonSummaryvCheckpointsView.Size = new Size(75, 23);
            buttonSummaryvCheckpointsView.TabIndex = 11;
            buttonSummaryvCheckpointsView.Text = "Summary";
            buttonSummaryvCheckpointsView.UseVisualStyleBackColor = true;
            buttonSummaryvCheckpointsView.Click += buttonSummaryvCheckpointsView_Click;
            // 
            // buttonLoadvCheckpointsrefresh
            // 
            buttonLoadvCheckpointsrefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadvCheckpointsrefresh.Location = new Point(1453, 6);
            buttonLoadvCheckpointsrefresh.Name = "buttonLoadvCheckpointsrefresh";
            buttonLoadvCheckpointsrefresh.Size = new Size(154, 23);
            buttonLoadvCheckpointsrefresh.TabIndex = 10;
            buttonLoadvCheckpointsrefresh.Text = "&Load Checkpoints/refresh";
            buttonLoadvCheckpointsrefresh.UseVisualStyleBackColor = true;
            buttonLoadvCheckpointsrefresh.Click += buttonLoadvCheckpointsrefresh_Click;
            // 
            // labelvCheckpointsOverviewText
            // 
            labelvCheckpointsOverviewText.AutoSize = true;
            labelvCheckpointsOverviewText.Location = new Point(6, 3);
            labelvCheckpointsOverviewText.Name = "labelvCheckpointsOverviewText";
            labelvCheckpointsOverviewText.Size = new Size(548, 30);
            labelvCheckpointsOverviewText.TabIndex = 9;
            labelvCheckpointsOverviewText.Text = "This view provides provides overview and core functionality within the Hyper-V space for\r\ninformation about VM checkpoints and other data that extends that functionality over multiple servers.";
            // 
            // datagridviewCheckpointOverView
            // 
            datagridviewCheckpointOverView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewCheckpointOverView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewCheckpointOverView.Location = new Point(6, 35);
            datagridviewCheckpointOverView.Name = "datagridviewCheckpointOverView";
            datagridviewCheckpointOverView.Size = new Size(1601, 774);
            datagridviewCheckpointOverView.TabIndex = 8;
            // 
            // tabPagehvDVD
            // 
            tabPagehvDVD.Location = new Point(4, 24);
            tabPagehvDVD.Name = "tabPagehvDVD";
            tabPagehvDVD.Size = new Size(1613, 815);
            tabPagehvDVD.TabIndex = 20;
            tabPagehvDVD.Text = "vDVD";
            tabPagehvDVD.UseVisualStyleBackColor = true;
            // 
            // tabpagehvHosts
            // 
            tabpagehvHosts.Controls.Add(buttonSummaryvHostsView);
            tabpagehvHosts.Controls.Add(buttonLoadHostsrefresh);
            tabpagehvHosts.Controls.Add(datagridviewhvHosts);
            tabpagehvHosts.Controls.Add(label1);
            tabpagehvHosts.Location = new Point(4, 24);
            tabpagehvHosts.Name = "tabpagehvHosts";
            tabpagehvHosts.Size = new Size(1613, 815);
            tabpagehvHosts.TabIndex = 3;
            tabpagehvHosts.Text = "vHosts";
            tabpagehvHosts.UseVisualStyleBackColor = true;
            // 
            // buttonSummaryvHostsView
            // 
            buttonSummaryvHostsView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryvHostsView.Location = new Point(1406, 6);
            buttonSummaryvHostsView.Name = "buttonSummaryvHostsView";
            buttonSummaryvHostsView.Size = new Size(75, 23);
            buttonSummaryvHostsView.TabIndex = 8;
            buttonSummaryvHostsView.Text = "Summary";
            buttonSummaryvHostsView.UseVisualStyleBackColor = true;
            // 
            // buttonLoadHostsrefresh
            // 
            buttonLoadHostsrefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadHostsrefresh.Location = new Point(1487, 6);
            buttonLoadHostsrefresh.Name = "buttonLoadHostsrefresh";
            buttonLoadHostsrefresh.Size = new Size(120, 23);
            buttonLoadHostsrefresh.TabIndex = 2;
            buttonLoadHostsrefresh.Text = "&Load Hosts/refresh";
            buttonLoadHostsrefresh.UseVisualStyleBackColor = true;
            buttonLoadHostsrefresh.Click += buttonLoadHostsrefresh_Click;
            // 
            // datagridviewhvHosts
            // 
            datagridviewhvHosts.AllowUserToAddRows = false;
            datagridviewhvHosts.AllowUserToDeleteRows = false;
            datagridviewhvHosts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewhvHosts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewhvHosts.Location = new Point(6, 35);
            datagridviewhvHosts.Name = "datagridviewhvHosts";
            datagridviewhvHosts.ReadOnly = true;
            datagridviewhvHosts.Size = new Size(1601, 774);
            datagridviewhvHosts.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 3);
            label1.Name = "label1";
            label1.Size = new Size(514, 15);
            label1.TabIndex = 0;
            label1.Text = "This view provides overview the Hyper-V Host space and information about hardware and more.";
            // 
            // tabpagehvClusters
            // 
            tabpagehvClusters.Controls.Add(datagridviewClusterVMs);
            tabpagehvClusters.Controls.Add(labelClusterVMs);
            tabpagehvClusters.Controls.Add(datagridviewClusterNodes);
            tabpagehvClusters.Controls.Add(labelClusterNodes);
            tabpagehvClusters.Controls.Add(groupBoxClusterInfo);
            tabpagehvClusters.Controls.Add(buttonRefreshClusterInfo);
            tabpagehvClusters.Controls.Add(labelClustersHelpText);
            tabpagehvClusters.Controls.Add(buttonSummaryClustersOverviewView);
            tabpagehvClusters.Location = new Point(4, 24);
            tabpagehvClusters.Name = "tabpagehvClusters";
            tabpagehvClusters.Size = new Size(1613, 815);
            tabpagehvClusters.TabIndex = 4;
            tabpagehvClusters.Text = "vCluster";
            tabpagehvClusters.UseVisualStyleBackColor = true;
            // 
            // datagridviewClusterVMs
            // 
            datagridviewClusterVMs.AllowUserToAddRows = false;
            datagridviewClusterVMs.AllowUserToDeleteRows = false;
            datagridviewClusterVMs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewClusterVMs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewClusterVMs.Location = new Point(6, 542);
            datagridviewClusterVMs.Name = "datagridviewClusterVMs";
            datagridviewClusterVMs.ReadOnly = true;
            datagridviewClusterVMs.Size = new Size(1601, 270);
            datagridviewClusterVMs.TabIndex = 13;
            // 
            // labelClusterVMs
            // 
            labelClusterVMs.AutoSize = true;
            labelClusterVMs.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelClusterVMs.Location = new Point(6, 524);
            labelClusterVMs.Name = "labelClusterVMs";
            labelClusterVMs.Size = new Size(167, 15);
            labelClusterVMs.TabIndex = 12;
            labelClusterVMs.Text = "Highly Available VMs (0 VMs)";
            // 
            // datagridviewClusterNodes
            // 
            datagridviewClusterNodes.AllowUserToAddRows = false;
            datagridviewClusterNodes.AllowUserToDeleteRows = false;
            datagridviewClusterNodes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewClusterNodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewClusterNodes.Location = new Point(6, 308);
            datagridviewClusterNodes.Name = "datagridviewClusterNodes";
            datagridviewClusterNodes.ReadOnly = true;
            datagridviewClusterNodes.Size = new Size(1601, 200);
            datagridviewClusterNodes.TabIndex = 11;
            // 
            // labelClusterNodes
            // 
            labelClusterNodes.AutoSize = true;
            labelClusterNodes.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelClusterNodes.Location = new Point(6, 290);
            labelClusterNodes.Name = "labelClusterNodes";
            labelClusterNodes.Size = new Size(131, 15);
            labelClusterNodes.TabIndex = 10;
            labelClusterNodes.Text = "Cluster Nodes (0 total)";
            // 
            // groupBoxClusterInfo
            // 
            groupBoxClusterInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxClusterInfo.AutoSize = true;
            groupBoxClusterInfo.Controls.Add(labelSharedVolumesValue);
            groupBoxClusterInfo.Controls.Add(labelSharedVolumes);
            groupBoxClusterInfo.Controls.Add(labelClusterNetworksValue);
            groupBoxClusterInfo.Controls.Add(labelClusterNetworks);
            groupBoxClusterInfo.Controls.Add(labelCurrentNodeValue);
            groupBoxClusterInfo.Controls.Add(labelCurrentNode);
            groupBoxClusterInfo.Controls.Add(labelTotalNodesValue);
            groupBoxClusterInfo.Controls.Add(labelTotalNodes);
            groupBoxClusterInfo.Controls.Add(labelClusterNameValue);
            groupBoxClusterInfo.Controls.Add(labelClusterName);
            groupBoxClusterInfo.Location = new Point(6, 38);
            groupBoxClusterInfo.Name = "groupBoxClusterInfo";
            groupBoxClusterInfo.Size = new Size(1601, 240);
            groupBoxClusterInfo.TabIndex = 7;
            groupBoxClusterInfo.TabStop = false;
            groupBoxClusterInfo.Text = "Cluster Information";
            // 
            // labelSharedVolumesValue
            // 
            labelSharedVolumesValue.AutoSize = true;
            labelSharedVolumesValue.Location = new Point(164, 169);
            labelSharedVolumesValue.Name = "labelSharedVolumesValue";
            labelSharedVolumesValue.Size = new Size(12, 15);
            labelSharedVolumesValue.TabIndex = 9;
            labelSharedVolumesValue.Text = "-";
            // 
            // labelSharedVolumes
            // 
            labelSharedVolumes.AutoSize = true;
            labelSharedVolumes.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelSharedVolumes.Location = new Point(16, 169);
            labelSharedVolumes.Name = "labelSharedVolumes";
            labelSharedVolumes.Size = new Size(99, 15);
            labelSharedVolumes.TabIndex = 8;
            labelSharedVolumes.Text = "Shared Volumes:";
            // 
            // labelClusterNetworksValue
            // 
            labelClusterNetworksValue.AutoSize = true;
            labelClusterNetworksValue.Location = new Point(164, 139);
            labelClusterNetworksValue.Name = "labelClusterNetworksValue";
            labelClusterNetworksValue.Size = new Size(12, 15);
            labelClusterNetworksValue.TabIndex = 7;
            labelClusterNetworksValue.Text = "-";
            // 
            // labelClusterNetworks
            // 
            labelClusterNetworks.AutoSize = true;
            labelClusterNetworks.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelClusterNetworks.Location = new Point(16, 139);
            labelClusterNetworks.Name = "labelClusterNetworks";
            labelClusterNetworks.Size = new Size(107, 15);
            labelClusterNetworks.TabIndex = 6;
            labelClusterNetworks.Text = "Cluster Networks:";
            // 
            // labelCurrentNodeValue
            // 
            labelCurrentNodeValue.AutoSize = true;
            labelCurrentNodeValue.Location = new Point(164, 109);
            labelCurrentNodeValue.Name = "labelCurrentNodeValue";
            labelCurrentNodeValue.Size = new Size(12, 15);
            labelCurrentNodeValue.TabIndex = 5;
            labelCurrentNodeValue.Text = "-";
            // 
            // labelCurrentNode
            // 
            labelCurrentNode.AutoSize = true;
            labelCurrentNode.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelCurrentNode.Location = new Point(16, 109);
            labelCurrentNode.Name = "labelCurrentNode";
            labelCurrentNode.Size = new Size(86, 15);
            labelCurrentNode.TabIndex = 4;
            labelCurrentNode.Text = "Current Node:";
            // 
            // labelTotalNodesValue
            // 
            labelTotalNodesValue.AutoSize = true;
            labelTotalNodesValue.Location = new Point(164, 79);
            labelTotalNodesValue.Name = "labelTotalNodesValue";
            labelTotalNodesValue.Size = new Size(12, 15);
            labelTotalNodesValue.TabIndex = 3;
            labelTotalNodesValue.Text = "-";
            // 
            // labelTotalNodes
            // 
            labelTotalNodes.AutoSize = true;
            labelTotalNodes.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelTotalNodes.Location = new Point(16, 79);
            labelTotalNodes.Name = "labelTotalNodes";
            labelTotalNodes.Size = new Size(75, 15);
            labelTotalNodes.TabIndex = 2;
            labelTotalNodes.Text = "Total Nodes:";
            // 
            // labelClusterNameValue
            // 
            labelClusterNameValue.AutoSize = true;
            labelClusterNameValue.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold);
            labelClusterNameValue.ForeColor = Color.DarkBlue;
            labelClusterNameValue.Location = new Point(162, 40);
            labelClusterNameValue.Name = "labelClusterNameValue";
            labelClusterNameValue.Size = new Size(150, 24);
            labelClusterNameValue.TabIndex = 1;
            labelClusterNameValue.Text = "Not Connected";
            // 
            // labelClusterName
            // 
            labelClusterName.AutoSize = true;
            labelClusterName.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold);
            labelClusterName.ForeColor = Color.DarkBlue;
            labelClusterName.Location = new Point(14, 40);
            labelClusterName.Name = "labelClusterName";
            labelClusterName.Size = new Size(142, 24);
            labelClusterName.TabIndex = 0;
            labelClusterName.Text = "Cluster Name:";
            // 
            // buttonRefreshClusterInfo
            // 
            buttonRefreshClusterInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRefreshClusterInfo.Location = new Point(1483, 6);
            buttonRefreshClusterInfo.Name = "buttonRefreshClusterInfo";
            buttonRefreshClusterInfo.Size = new Size(124, 23);
            buttonRefreshClusterInfo.TabIndex = 6;
            buttonRefreshClusterInfo.Text = "&Load Cluster/refresh";
            buttonRefreshClusterInfo.UseVisualStyleBackColor = true;
            buttonRefreshClusterInfo.Click += buttonRefreshClusterInfoUI_Click;
            // 
            // labelClustersHelpText
            // 
            labelClustersHelpText.AutoSize = true;
            labelClustersHelpText.Location = new Point(6, 3);
            labelClustersHelpText.Name = "labelClustersHelpText";
            labelClustersHelpText.Size = new Size(418, 30);
            labelClustersHelpText.TabIndex = 5;
            labelClustersHelpText.Text = "This view provides overview of Hyper-V Failover Cluster information including\r\nnodes, networks, shared volumes, and highly available virtual machines.";
            // 
            // buttonSummaryClustersOverviewView
            // 
            buttonSummaryClustersOverviewView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryClustersOverviewView.Location = new Point(1402, 6);
            buttonSummaryClustersOverviewView.Name = "buttonSummaryClustersOverviewView";
            buttonSummaryClustersOverviewView.Size = new Size(75, 23);
            buttonSummaryClustersOverviewView.TabIndex = 4;
            buttonSummaryClustersOverviewView.Text = "Summary";
            buttonSummaryClustersOverviewView.UseVisualStyleBackColor = true;
            buttonSummaryClustersOverviewView.Click += buttonSummaryClustersOverviewView_Click;
            // 
            // tabpagehvStorage
            // 
            tabpagehvStorage.Controls.Add(labelvStorageOverviewText);
            tabpagehvStorage.Location = new Point(4, 24);
            tabpagehvStorage.Name = "tabpagehvStorage";
            tabpagehvStorage.Size = new Size(1613, 815);
            tabpagehvStorage.TabIndex = 5;
            tabpagehvStorage.Text = "vStorage";
            tabpagehvStorage.UseVisualStyleBackColor = true;
            // 
            // labelvStorageOverviewText
            // 
            labelvStorageOverviewText.AutoSize = true;
            labelvStorageOverviewText.Location = new Point(6, 3);
            labelvStorageOverviewText.Name = "labelvStorageOverviewText";
            labelvStorageOverviewText.Size = new Size(623, 30);
            labelvStorageOverviewText.TabIndex = 6;
            labelvStorageOverviewText.Text = resources.GetString("labelvStorageOverviewText.Text");
            // 
            // tabPagehvReplication
            // 
            tabPagehvReplication.Location = new Point(4, 24);
            tabPagehvReplication.Name = "tabPagehvReplication";
            tabPagehvReplication.Size = new Size(1613, 815);
            tabPagehvReplication.TabIndex = 19;
            tabPagehvReplication.Text = "vReplication";
            tabPagehvReplication.UseVisualStyleBackColor = true;
            // 
            // tabpagehvResources
            // 
            tabpagehvResources.Location = new Point(4, 24);
            tabpagehvResources.Name = "tabpagehvResources";
            tabpagehvResources.Size = new Size(1613, 815);
            tabpagehvResources.TabIndex = 9;
            tabpagehvResources.Text = "vResources";
            tabpagehvResources.UseVisualStyleBackColor = true;
            // 
            // tabpageManageNetwork
            // 
            tabpageManageNetwork.Location = new Point(4, 24);
            tabpageManageNetwork.Name = "tabpageManageNetwork";
            tabpageManageNetwork.Size = new Size(1613, 815);
            tabpageManageNetwork.TabIndex = 2;
            tabpageManageNetwork.Text = "vNetwork";
            tabpageManageNetwork.UseVisualStyleBackColor = true;
            // 
            // tabpagehvSecurity
            // 
            tabpagehvSecurity.Location = new Point(4, 24);
            tabpagehvSecurity.Name = "tabpagehvSecurity";
            tabpagehvSecurity.Size = new Size(1613, 815);
            tabpagehvSecurity.TabIndex = 10;
            tabpagehvSecurity.Text = "vSecurity";
            tabpagehvSecurity.UseVisualStyleBackColor = true;
            // 
            // tabpagehvPerformance
            // 
            tabpagehvPerformance.Location = new Point(4, 24);
            tabpagehvPerformance.Name = "tabpagehvPerformance";
            tabpagehvPerformance.Size = new Size(1613, 815);
            tabpagehvPerformance.TabIndex = 11;
            tabpagehvPerformance.Text = "vPerformance";
            tabpagehvPerformance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvCompliance
            // 
            tabpagehvCompliance.Location = new Point(4, 24);
            tabpagehvCompliance.Name = "tabpagehvCompliance";
            tabpagehvCompliance.Size = new Size(1613, 815);
            tabpagehvCompliance.TabIndex = 12;
            tabpagehvCompliance.Text = "vCompliance";
            tabpagehvCompliance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvInventory
            // 
            tabpagehvInventory.Location = new Point(4, 24);
            tabpagehvInventory.Name = "tabpagehvInventory";
            tabpagehvInventory.Size = new Size(1613, 815);
            tabpagehvInventory.TabIndex = 13;
            tabpagehvInventory.Text = "vInventory";
            tabpagehvInventory.UseVisualStyleBackColor = true;
            // 
            // tabpageCreateVM
            // 
            tabpageCreateVM.Location = new Point(4, 24);
            tabpageCreateVM.Name = "tabpageCreateVM";
            tabpageCreateVM.Size = new Size(1613, 815);
            tabpageCreateVM.TabIndex = 14;
            tabpageCreateVM.Text = "Create VM";
            tabpageCreateVM.UseVisualStyleBackColor = true;
            // 
            // tabpageVMGroups
            // 
            tabpageVMGroups.Controls.Add(groupBox2);
            tabpageVMGroups.Controls.Add(groupBox1);
            tabpageVMGroups.Controls.Add(buttonLoadGroupsrefresh);
            tabpageVMGroups.Controls.Add(labelThisViewProvidesOver);
            tabpageVMGroups.Controls.Add(datagridviewVMGroups);
            tabpageVMGroups.Location = new Point(4, 24);
            tabpageVMGroups.Name = "tabpageVMGroups";
            tabpageVMGroups.Padding = new Padding(3);
            tabpageVMGroups.Size = new Size(1613, 815);
            tabpageVMGroups.TabIndex = 1;
            tabpageVMGroups.Text = "vVMGroup";
            tabpageVMGroups.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox2.Controls.Add(buttonManageServerMembers);
            groupBox2.Location = new Point(1407, 211);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(200, 73);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Manage VM Group";
            // 
            // buttonManageServerMembers
            // 
            buttonManageServerMembers.Location = new Point(6, 22);
            buttonManageServerMembers.Name = "buttonManageServerMembers";
            buttonManageServerMembers.Size = new Size(188, 43);
            buttonManageServerMembers.TabIndex = 2;
            buttonManageServerMembers.Text = "Manage server members";
            buttonManageServerMembers.UseVisualStyleBackColor = true;
            buttonManageServerMembers.Click += buttonManageServerMembers_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox1.Controls.Add(buttonRenameSelectedVMGrou);
            groupBox1.Controls.Add(buttonDeleteSelectedVMGrou);
            groupBox1.Controls.Add(buttonCreateANewVMGroup);
            groupBox1.Location = new Point(1407, 35);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(200, 170);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Group management";
            // 
            // buttonRenameSelectedVMGrou
            // 
            buttonRenameSelectedVMGrou.Location = new Point(6, 120);
            buttonRenameSelectedVMGrou.Name = "buttonRenameSelectedVMGrou";
            buttonRenameSelectedVMGrou.Size = new Size(188, 43);
            buttonRenameSelectedVMGrou.TabIndex = 2;
            buttonRenameSelectedVMGrou.Text = "Rename selected VM group";
            buttonRenameSelectedVMGrou.UseVisualStyleBackColor = true;
            buttonRenameSelectedVMGrou.Click += buttonRenameSelectedVMGrou_Click;
            // 
            // buttonDeleteSelectedVMGrou
            // 
            buttonDeleteSelectedVMGrou.BackColor = Color.LightCoral;
            buttonDeleteSelectedVMGrou.Location = new Point(6, 71);
            buttonDeleteSelectedVMGrou.Name = "buttonDeleteSelectedVMGrou";
            buttonDeleteSelectedVMGrou.Size = new Size(188, 43);
            buttonDeleteSelectedVMGrou.TabIndex = 1;
            buttonDeleteSelectedVMGrou.Text = "Delete selected VM group";
            buttonDeleteSelectedVMGrou.UseVisualStyleBackColor = false;
            buttonDeleteSelectedVMGrou.Click += buttonDeleteSelectedVMGrou_Click;
            // 
            // buttonCreateANewVMGroup
            // 
            buttonCreateANewVMGroup.BackColor = Color.LightGreen;
            buttonCreateANewVMGroup.Location = new Point(6, 22);
            buttonCreateANewVMGroup.Name = "buttonCreateANewVMGroup";
            buttonCreateANewVMGroup.Size = new Size(188, 43);
            buttonCreateANewVMGroup.TabIndex = 0;
            buttonCreateANewVMGroup.Text = "Create a new VM Group";
            buttonCreateANewVMGroup.UseVisualStyleBackColor = false;
            buttonCreateANewVMGroup.Click += buttonCreateANewVMGroup_Click;
            // 
            // buttonLoadGroupsrefresh
            // 
            buttonLoadGroupsrefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadGroupsrefresh.Location = new Point(1475, 6);
            buttonLoadGroupsrefresh.Name = "buttonLoadGroupsrefresh";
            buttonLoadGroupsrefresh.Size = new Size(132, 23);
            buttonLoadGroupsrefresh.TabIndex = 3;
            buttonLoadGroupsrefresh.Text = "&Load Groups/refresh";
            buttonLoadGroupsrefresh.UseVisualStyleBackColor = true;
            buttonLoadGroupsrefresh.Click += buttonLoadGroupsrefresh_Click;
            // 
            // labelThisViewProvidesOver
            // 
            labelThisViewProvidesOver.AutoSize = true;
            labelThisViewProvidesOver.Location = new Point(6, 3);
            labelThisViewProvidesOver.Name = "labelThisViewProvidesOver";
            labelThisViewProvidesOver.Size = new Size(501, 30);
            labelThisViewProvidesOver.TabIndex = 2;
            labelThisViewProvidesOver.Text = "This view provides overview and core functionality within the Hyper-V space for management\r\nof VM Groups for VMs that extends that functionality over multiple servers.";
            // 
            // datagridviewVMGroups
            // 
            datagridviewVMGroups.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewVMGroups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewVMGroups.Location = new Point(6, 35);
            datagridviewVMGroups.Name = "datagridviewVMGroups";
            datagridviewVMGroups.Size = new Size(1395, 774);
            datagridviewVMGroups.TabIndex = 0;
            // 
            // tabpageHealthOverview
            // 
            tabpageHealthOverview.Controls.Add(comboBoxClusterNodeSelector);
            tabpageHealthOverview.Controls.Add(labelClusterNodeSelector);
            tabpageHealthOverview.Controls.Add(buttonSummaryHealthOverviewHelp);
            tabpageHealthOverview.Controls.Add(datagridviewHealthOverview);
            tabpageHealthOverview.Controls.Add(buttonExportHealthOverview);
            tabpageHealthOverview.Controls.Add(buttonSummaryHealthOverviewView);
            tabpageHealthOverview.Controls.Add(buttonLoadHealthOverview);
            tabpageHealthOverview.Controls.Add(labelHealthOverviewText);
            tabpageHealthOverview.Location = new Point(4, 24);
            tabpageHealthOverview.Name = "tabpageHealthOverview";
            tabpageHealthOverview.Size = new Size(1613, 815);
            tabpageHealthOverview.TabIndex = 15;
            tabpageHealthOverview.Text = "Health Overview";
            tabpageHealthOverview.UseVisualStyleBackColor = true;
            // 
            // comboBoxClusterNodeSelector
            // 
            comboBoxClusterNodeSelector.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxClusterNodeSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxClusterNodeSelector.FormattingEnabled = true;
            comboBoxClusterNodeSelector.Location = new Point(983, 6);
            comboBoxClusterNodeSelector.Name = "comboBoxClusterNodeSelector";
            comboBoxClusterNodeSelector.Size = new Size(222, 23);
            comboBoxClusterNodeSelector.TabIndex = 12;
            comboBoxClusterNodeSelector.Visible = false;
            comboBoxClusterNodeSelector.SelectedIndexChanged += comboBoxClusterNodeSelector_SelectedIndexChanged;
            // 
            // labelClusterNodeSelector
            // 
            labelClusterNodeSelector.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelClusterNodeSelector.AutoSize = true;
            labelClusterNodeSelector.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelClusterNodeSelector.Location = new Point(883, 10);
            labelClusterNodeSelector.Name = "labelClusterNodeSelector";
            labelClusterNodeSelector.Size = new Size(96, 15);
            labelClusterNodeSelector.TabIndex = 11;
            labelClusterNodeSelector.Text = "View data from:";
            labelClusterNodeSelector.Visible = false;
            // 
            // buttonSummaryHealthOverviewHelp
            // 
            buttonSummaryHealthOverviewHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryHealthOverviewHelp.Location = new Point(1211, 6);
            buttonSummaryHealthOverviewHelp.Name = "buttonSummaryHealthOverviewHelp";
            buttonSummaryHealthOverviewHelp.Size = new Size(26, 23);
            buttonSummaryHealthOverviewHelp.TabIndex = 10;
            buttonSummaryHealthOverviewHelp.Text = "?";
            buttonSummaryHealthOverviewHelp.UseVisualStyleBackColor = true;
            buttonSummaryHealthOverviewHelp.Click += buttonSummaryHealthOverviewHelp_Click;
            // 
            // datagridviewHealthOverview
            // 
            datagridviewHealthOverview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewHealthOverview.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewHealthOverview.Location = new Point(6, 35);
            datagridviewHealthOverview.Name = "datagridviewHealthOverview";
            datagridviewHealthOverview.Size = new Size(1601, 774);
            datagridviewHealthOverview.TabIndex = 9;
            // 
            // buttonExportHealthOverview
            // 
            buttonExportHealthOverview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExportHealthOverview.Location = new Point(1324, 6);
            buttonExportHealthOverview.Name = "buttonExportHealthOverview";
            buttonExportHealthOverview.Size = new Size(144, 23);
            buttonExportHealthOverview.TabIndex = 8;
            buttonExportHealthOverview.Text = "&Export Health Overview";
            buttonExportHealthOverview.UseVisualStyleBackColor = true;
            // 
            // buttonSummaryHealthOverviewView
            // 
            buttonSummaryHealthOverviewView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryHealthOverviewView.Location = new Point(1243, 6);
            buttonSummaryHealthOverviewView.Name = "buttonSummaryHealthOverviewView";
            buttonSummaryHealthOverviewView.Size = new Size(75, 23);
            buttonSummaryHealthOverviewView.TabIndex = 7;
            buttonSummaryHealthOverviewView.Text = "Summary";
            buttonSummaryHealthOverviewView.UseVisualStyleBackColor = true;
            buttonSummaryHealthOverviewView.Click += buttonSummaryHealthOverview_Click;
            // 
            // buttonLoadHealthOverview
            // 
            buttonLoadHealthOverview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadHealthOverview.Location = new Point(1474, 6);
            buttonLoadHealthOverview.Name = "buttonLoadHealthOverview";
            buttonLoadHealthOverview.Size = new Size(133, 23);
            buttonLoadHealthOverview.TabIndex = 6;
            buttonLoadHealthOverview.Text = "&Load overview/refresh";
            buttonLoadHealthOverview.UseVisualStyleBackColor = true;
            buttonLoadHealthOverview.Click += buttonLoadHealthOverview_Click;
            // 
            // labelHealthOverviewText
            // 
            labelHealthOverviewText.AutoSize = true;
            labelHealthOverviewText.Location = new Point(6, 3);
            labelHealthOverviewText.Name = "labelHealthOverviewText";
            labelHealthOverviewText.Size = new Size(769, 15);
            labelHealthOverviewText.TabIndex = 5;
            labelHealthOverviewText.Text = "This view provides provides overview over core ressoruces and allocation in Hyper-V/at host level, for information about VMs and other key data.\r\n";
            // 
            // panelSearch
            // 
            panelSearch.BackColor = Color.WhiteSmoke;
            panelSearch.BorderStyle = BorderStyle.FixedSingle;
            panelSearch.Controls.Add(buttonCloseSearch);
            panelSearch.Controls.Add(checkBoxFilterResults);
            panelSearch.Controls.Add(labelSearchResults);
            panelSearch.Controls.Add(buttonSearchNext);
            panelSearch.Controls.Add(buttonSearchPrevious);
            panelSearch.Controls.Add(labelSearchIcon);
            panelSearch.Controls.Add(textBoxSearch);
            panelSearch.Dock = DockStyle.Top;
            panelSearch.Location = new Point(0, 24);
            panelSearch.Name = "panelSearch";
            panelSearch.Size = new Size(1645, 35);
            panelSearch.TabIndex = 5;
            panelSearch.Visible = false;
            // 
            // buttonCloseSearch
            // 
            buttonCloseSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCloseSearch.FlatAppearance.BorderSize = 0;
            buttonCloseSearch.FlatStyle = FlatStyle.Flat;
            buttonCloseSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonCloseSearch.ForeColor = Color.FromArgb(64, 64, 64);
            buttonCloseSearch.Location = new Point(1615, 4);
            buttonCloseSearch.Name = "buttonCloseSearch";
            buttonCloseSearch.Size = new Size(25, 25);
            buttonCloseSearch.TabIndex = 5;
            buttonCloseSearch.Text = "✕";
            buttonCloseSearch.UseVisualStyleBackColor = true;
            buttonCloseSearch.Click += ButtonCloseSearch_Click;
            // 
            // checkBoxFilterResults
            // 
            checkBoxFilterResults.AutoSize = true;
            checkBoxFilterResults.Font = new Font("Segoe UI", 9F);
            checkBoxFilterResults.ForeColor = Color.FromArgb(64, 64, 64);
            checkBoxFilterResults.Location = new Point(481, 8);
            checkBoxFilterResults.Name = "checkBoxFilterResults";
            checkBoxFilterResults.Size = new Size(107, 19);
            checkBoxFilterResults.TabIndex = 6;
            checkBoxFilterResults.Text = "List only results";
            checkBoxFilterResults.UseVisualStyleBackColor = true;
            checkBoxFilterResults.CheckedChanged += CheckBoxFilterResults_CheckedChanged;
            // 
            // labelSearchResults
            // 
            labelSearchResults.AutoSize = true;
            labelSearchResults.Font = new Font("Segoe UI", 9F);
            labelSearchResults.ForeColor = Color.FromArgb(64, 64, 64);
            labelSearchResults.Location = new Point(418, 9);
            labelSearchResults.Name = "labelSearchResults";
            labelSearchResults.Size = new Size(0, 15);
            labelSearchResults.TabIndex = 4;
            // 
            // buttonSearchNext
            // 
            buttonSearchNext.FlatStyle = FlatStyle.Flat;
            buttonSearchNext.Location = new Point(378, 5);
            buttonSearchNext.Name = "buttonSearchNext";
            buttonSearchNext.Size = new Size(30, 24);
            buttonSearchNext.TabIndex = 3;
            buttonSearchNext.Text = "▼";
            buttonSearchNext.UseVisualStyleBackColor = true;
            buttonSearchNext.Click += ButtonSearchNext_Click;
            // 
            // buttonSearchPrevious
            // 
            buttonSearchPrevious.FlatStyle = FlatStyle.Flat;
            buttonSearchPrevious.Location = new Point(345, 5);
            buttonSearchPrevious.Name = "buttonSearchPrevious";
            buttonSearchPrevious.Size = new Size(30, 24);
            buttonSearchPrevious.TabIndex = 2;
            buttonSearchPrevious.Text = "▲";
            buttonSearchPrevious.UseVisualStyleBackColor = true;
            buttonSearchPrevious.Click += ButtonSearchPrevious_Click;
            // 
            // labelSearchIcon
            // 
            labelSearchIcon.AutoSize = true;
            labelSearchIcon.Font = new Font("Segoe UI", 10F);
            labelSearchIcon.ForeColor = Color.FromArgb(64, 64, 64);
            labelSearchIcon.Location = new Point(8, 8);
            labelSearchIcon.Name = "labelSearchIcon";
            labelSearchIcon.Size = new Size(28, 19);
            labelSearchIcon.TabIndex = 1;
            labelSearchIcon.Text = "🔍";
            // 
            // textBoxSearch
            // 
            textBoxSearch.Location = new Point(35, 6);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.PlaceholderText = "Search in table...";
            textBoxSearch.Size = new Size(300, 23);
            textBoxSearch.TabIndex = 0;
            textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
            // 
            // menuStripTopMainForm
            // 
            menuStripTopMainForm.BackColor = Color.White;
            menuStripTopMainForm.Items.AddRange(new ToolStripItem[] { menuToolStripMenuItem, viewToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            menuStripTopMainForm.Location = new Point(0, 0);
            menuStripTopMainForm.Name = "menuStripTopMainForm";
            menuStripTopMainForm.Size = new Size(1645, 24);
            menuStripTopMainForm.TabIndex = 2;
            menuStripTopMainForm.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { disconnectToolStripMenuItem, toolStripSeparator2, onlineToolStripMenuItem, logsToolStripMenuItem, downloadLastestReleaseFromGitHubToolStripMenuItem, changelogToolStripMenuItem, exportDataToolStripMenuItem, aboutToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new Size(37, 20);
            menuToolStripMenuItem.Text = "File";
            // 
            // disconnectToolStripMenuItem
            // 
            disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            disconnectToolStripMenuItem.Size = new Size(273, 22);
            disconnectToolStripMenuItem.Text = "Disconnect";
            disconnectToolStripMenuItem.Click += disconnectToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(270, 6);
            // 
            // onlineToolStripMenuItem
            // 
            onlineToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { myWebpageToolStripMenuItem, myBlogToolStripMenuItem, guideToolStripMenuItem });
            onlineToolStripMenuItem.Name = "onlineToolStripMenuItem";
            onlineToolStripMenuItem.Size = new Size(273, 22);
            onlineToolStripMenuItem.Text = "Online";
            // 
            // myWebpageToolStripMenuItem
            // 
            myWebpageToolStripMenuItem.Name = "myWebpageToolStripMenuItem";
            myWebpageToolStripMenuItem.Size = new Size(142, 22);
            myWebpageToolStripMenuItem.Text = "My webpage";
            myWebpageToolStripMenuItem.Click += myWebpageToolStripMenuItem_Click;
            // 
            // myBlogToolStripMenuItem
            // 
            myBlogToolStripMenuItem.Name = "myBlogToolStripMenuItem";
            myBlogToolStripMenuItem.Size = new Size(142, 22);
            myBlogToolStripMenuItem.Text = "My blog";
            myBlogToolStripMenuItem.Click += myBlogToolStripMenuItem_Click;
            // 
            // guideToolStripMenuItem
            // 
            guideToolStripMenuItem.Name = "guideToolStripMenuItem";
            guideToolStripMenuItem.Size = new Size(142, 22);
            guideToolStripMenuItem.Text = "Guide";
            guideToolStripMenuItem.Click += guideToolStripMenuItem_Click;
            // 
            // logsToolStripMenuItem
            // 
            logsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openLogFolderToolStripMenuItem, openLogForTodayToolStripMenuItem });
            logsToolStripMenuItem.Name = "logsToolStripMenuItem";
            logsToolStripMenuItem.Size = new Size(273, 22);
            logsToolStripMenuItem.Text = "Logs";
            // 
            // openLogFolderToolStripMenuItem
            // 
            openLogFolderToolStripMenuItem.Name = "openLogFolderToolStripMenuItem";
            openLogFolderToolStripMenuItem.Size = new Size(174, 22);
            openLogFolderToolStripMenuItem.Text = "Open log folder";
            openLogFolderToolStripMenuItem.Click += openLogFolderToolStripMenuItem_Click;
            // 
            // openLogForTodayToolStripMenuItem
            // 
            openLogForTodayToolStripMenuItem.Name = "openLogForTodayToolStripMenuItem";
            openLogForTodayToolStripMenuItem.Size = new Size(174, 22);
            openLogForTodayToolStripMenuItem.Text = "Open log for today";
            openLogForTodayToolStripMenuItem.Click += openLogForTodayToolStripMenuItem_Click;
            // 
            // downloadLastestReleaseFromGitHubToolStripMenuItem
            // 
            downloadLastestReleaseFromGitHubToolStripMenuItem.Name = "downloadLastestReleaseFromGitHubToolStripMenuItem";
            downloadLastestReleaseFromGitHubToolStripMenuItem.Size = new Size(273, 22);
            downloadLastestReleaseFromGitHubToolStripMenuItem.Text = "Download lastest release from GitHub";
            downloadLastestReleaseFromGitHubToolStripMenuItem.Click += downloadLastestReleaseFromGitHubToolStripMenuItem_Click;
            // 
            // changelogToolStripMenuItem
            // 
            changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            changelogToolStripMenuItem.Size = new Size(273, 22);
            changelogToolStripMenuItem.Text = "Changelog";
            changelogToolStripMenuItem.Click += changelogToolStripMenuItem_Click;
            // 
            // exportDataToolStripMenuItem
            // 
            exportDataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exportAllDataToolStripMenuItem, exportCurrentTabToolStripMenuItem });
            exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            exportDataToolStripMenuItem.Size = new Size(273, 22);
            exportDataToolStripMenuItem.Text = "Export data";
            // 
            // exportAllDataToolStripMenuItem
            // 
            exportAllDataToolStripMenuItem.Name = "exportAllDataToolStripMenuItem";
            exportAllDataToolStripMenuItem.Size = new Size(168, 22);
            exportAllDataToolStripMenuItem.Text = "Export all data";
            // 
            // exportCurrentTabToolStripMenuItem
            // 
            exportCurrentTabToolStripMenuItem.Name = "exportCurrentTabToolStripMenuItem";
            exportCurrentTabToolStripMenuItem.Size = new Size(168, 22);
            exportCurrentTabToolStripMenuItem.Text = "Export current tab";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.ShortcutKeys = Keys.F1;
            aboutToolStripMenuItem.Size = new Size(273, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(270, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(273, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { refreshDataToolStripMenuItem, clearCacheToolStripMenuItem, toolStripSeparator3, expandAllCollumsToolStripMenuItem, autoSizeAllCollumsToolStripMenuItem, toolStripSeparator4, autoRefreshToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // refreshDataToolStripMenuItem
            // 
            refreshDataToolStripMenuItem.Name = "refreshDataToolStripMenuItem";
            refreshDataToolStripMenuItem.ShortcutKeys = Keys.F5;
            refreshDataToolStripMenuItem.Size = new Size(184, 22);
            refreshDataToolStripMenuItem.Text = "Refresh data";
            // 
            // clearCacheToolStripMenuItem
            // 
            clearCacheToolStripMenuItem.Name = "clearCacheToolStripMenuItem";
            clearCacheToolStripMenuItem.Size = new Size(184, 22);
            clearCacheToolStripMenuItem.Text = "Clear cache";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(181, 6);
            // 
            // expandAllCollumsToolStripMenuItem
            // 
            expandAllCollumsToolStripMenuItem.Name = "expandAllCollumsToolStripMenuItem";
            expandAllCollumsToolStripMenuItem.Size = new Size(184, 22);
            expandAllCollumsToolStripMenuItem.Text = "Expand all Collums";
            // 
            // autoSizeAllCollumsToolStripMenuItem
            // 
            autoSizeAllCollumsToolStripMenuItem.Name = "autoSizeAllCollumsToolStripMenuItem";
            autoSizeAllCollumsToolStripMenuItem.Size = new Size(184, 22);
            autoSizeAllCollumsToolStripMenuItem.Text = "Auto size all Collums";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(181, 6);
            // 
            // autoRefreshToolStripMenuItem
            // 
            autoRefreshToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { disabledMinuteToolStripMenuItem, every1MinuteToolStripMenuItem, every5MinutesToolStripMenuItem, every10MinutesToolStripMenuItem });
            autoRefreshToolStripMenuItem.Name = "autoRefreshToolStripMenuItem";
            autoRefreshToolStripMenuItem.Size = new Size(184, 22);
            autoRefreshToolStripMenuItem.Text = "Auto refresh";
            // 
            // disabledMinuteToolStripMenuItem
            // 
            disabledMinuteToolStripMenuItem.Name = "disabledMinuteToolStripMenuItem";
            disabledMinuteToolStripMenuItem.Size = new Size(163, 22);
            disabledMinuteToolStripMenuItem.Text = "Disabled";
            // 
            // every1MinuteToolStripMenuItem
            // 
            every1MinuteToolStripMenuItem.Name = "every1MinuteToolStripMenuItem";
            every1MinuteToolStripMenuItem.Size = new Size(163, 22);
            every1MinuteToolStripMenuItem.Text = "Every 1 minute";
            // 
            // every5MinutesToolStripMenuItem
            // 
            every5MinutesToolStripMenuItem.Name = "every5MinutesToolStripMenuItem";
            every5MinutesToolStripMenuItem.Size = new Size(163, 22);
            every5MinutesToolStripMenuItem.Text = "Every 5 minutes";
            // 
            // every10MinutesToolStripMenuItem
            // 
            every10MinutesToolStripMenuItem.Name = "every10MinutesToolStripMenuItem";
            every10MinutesToolStripMenuItem.Size = new Size(163, 22);
            every10MinutesToolStripMenuItem.Text = "Every 10 minutes";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { copySelectionToClipboardToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(47, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // pictureboxSupportMe
            // 
            pictureboxSupportMe.BackColor = Color.Transparent;
            pictureboxSupportMe.Image = (Image)resources.GetObject("pictureboxSupportMe.Image");
            pictureboxSupportMe.Location = new Point(186, 2);
            pictureboxSupportMe.Name = "pictureboxSupportMe";
            pictureboxSupportMe.Size = new Size(77, 21);
            pictureboxSupportMe.SizeMode = PictureBoxSizeMode.Zoom;
            pictureboxSupportMe.TabIndex = 1;
            pictureboxSupportMe.TabStop = false;
            pictureboxSupportMe.Click += pictureboxSupportMe_Click;
            // 
            // statusStripMainForm
            // 
            statusStripMainForm.BackColor = Color.White;
            statusStripMainForm.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelMainForm, toolStripStatusLabelTextMainForm });
            statusStripMainForm.Location = new Point(0, 884);
            statusStripMainForm.Name = "statusStripMainForm";
            statusStripMainForm.Size = new Size(1645, 22);
            statusStripMainForm.SizingGrip = false;
            statusStripMainForm.TabIndex = 3;
            statusStripMainForm.Text = "statusStrip1";
            // 
            // toolStripStatusLabelMainForm
            // 
            toolStripStatusLabelMainForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStripStatusLabelMainForm.Name = "toolStripStatusLabelMainForm";
            toolStripStatusLabelMainForm.Size = new Size(45, 17);
            toolStripStatusLabelMainForm.Text = "Status:";
            // 
            // toolStripStatusLabelTextMainForm
            // 
            toolStripStatusLabelTextMainForm.Name = "toolStripStatusLabelTextMainForm";
            toolStripStatusLabelTextMainForm.Size = new Size(67, 17);
            toolStripStatusLabelTextMainForm.Text = "%STATUS%";
            // 
            // toolstripstatuslabelMain_CreatedBy
            // 
            toolstripstatuslabelMain_CreatedBy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toolstripstatuslabelMain_CreatedBy.AutoSize = true;
            toolstripstatuslabelMain_CreatedBy.BackColor = Color.White;
            toolstripstatuslabelMain_CreatedBy.Location = new Point(1451, 4);
            toolstripstatuslabelMain_CreatedBy.Name = "toolstripstatuslabelMain_CreatedBy";
            toolstripstatuslabelMain_CreatedBy.Size = new Size(190, 15);
            toolstripstatuslabelMain_CreatedBy.TabIndex = 4;
            toolstripstatuslabelMain_CreatedBy.Text = "Created by: Michael Morten Sonne";
            // 
            // copySelectionToClipboardToolStripMenuItem
            // 
            copySelectionToClipboardToolStripMenuItem.Name = "copySelectionToClipboardToolStripMenuItem";
            copySelectionToClipboardToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copySelectionToClipboardToolStripMenuItem.Size = new Size(261, 22);
            copySelectionToClipboardToolStripMenuItem.Text = "Copy selection to clipboard";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1645, 906);
            Controls.Add(panelSearch);
            Controls.Add(pictureboxSupportMe);
            Controls.Add(toolstripstatuslabelMain_CreatedBy);
            Controls.Add(statusStripMainForm);
            Controls.Add(menuStripTopMainForm);
            Controls.Add(tabcontrolMainForm);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStripTopMainForm;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HVTools";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            KeyDown += MainForm_KeyDown;
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).EndInit();
            tabcontrolMainForm.ResumeLayout(false);
            tabpagehvOverview.ResumeLayout(false);
            tabpagehvOverview.PerformLayout();
            tabPagehvDisks.ResumeLayout(false);
            tabPagehvDisks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewvDiskOverView).EndInit();
            tabpagehvCheckpoints.ResumeLayout(false);
            tabpagehvCheckpoints.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewCheckpointOverView).EndInit();
            tabpagehvHosts.ResumeLayout(false);
            tabpagehvHosts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewhvHosts).EndInit();
            tabpagehvClusters.ResumeLayout(false);
            tabpagehvClusters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterVMs).EndInit();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterNodes).EndInit();
            groupBoxClusterInfo.ResumeLayout(false);
            groupBoxClusterInfo.PerformLayout();
            tabpagehvStorage.ResumeLayout(false);
            tabpagehvStorage.PerformLayout();
            tabpageVMGroups.ResumeLayout(false);
            tabpageVMGroups.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)datagridviewVMGroups).EndInit();
            tabpageHealthOverview.ResumeLayout(false);
            tabpageHealthOverview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewHealthOverview).EndInit();
            panelSearch.ResumeLayout(false);
            panelSearch.PerformLayout();
            menuStripTopMainForm.ResumeLayout(false);
            menuStripTopMainForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureboxSupportMe).EndInit();
            statusStripMainForm.ResumeLayout(false);
            statusStripMainForm.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView datagridviewVMOverView;
        private TabControl tabcontrolMainForm;
        private TabPage tabpagehvOverview;
        private TabPage tabpageVMGroups;
        private MenuStrip menuStripTopMainForm;
        private ToolStripMenuItem menuToolStripMenuItem;
        private ToolStripMenuItem onlineToolStripMenuItem;
        private ToolStripMenuItem myWebpageToolStripMenuItem;
        private ToolStripMenuItem myBlogToolStripMenuItem;
        private ToolStripMenuItem guideToolStripMenuItem;
        private ToolStripMenuItem logsToolStripMenuItem;
        private ToolStripMenuItem openLogFolderToolStripMenuItem;
        private ToolStripMenuItem openLogForTodayToolStripMenuItem;
        private ToolStripMenuItem downloadLastestReleaseFromGitHubToolStripMenuItem;
        private ToolStripMenuItem changelogToolStripMenuItem;
        private ToolStripMenuItem exportDataToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private TabPage tabpageManageNetwork;
        private TabPage tabpagehvHosts;
        private TabPage tabpagehvClusters;
        private TabPage tabpagehvStorage;
        private TabPage tabpagehvNetworking;
        private TabPage tabpagehvCheckpoints;
        private TabPage tabpagehvResources;
        private TabPage tabpagehvSecurity;
        private TabPage tabpagehvPerformance;
        private TabPage tabpagehvCompliance;
        private TabPage tabpagehvInventory;
        private TabPage tabpageCreateVM;
        private TabPage tabpageHealthOverview;
        private PictureBox pictureboxSupportMe;
        private StatusStrip statusStripMainForm;
        private ToolStripStatusLabel toolStripStatusLabelMainForm;
        private ToolStripStatusLabel toolStripStatusLabelTextMainForm;
        private Label labelOverviewHelpText;
        private Button buttonSummaryhvOverviewView;
        private Button buttonLoadVMsrefresh;
        private ToolStripMenuItem disconnectToolStripMenuItem;
        private Label labelThisViewProvidesOver;
        private DataGridView datagridviewVMGroups;
        private GroupBox groupBox1;
        private Button buttonRenameSelectedVMGrou;
        private Button buttonDeleteSelectedVMGrou;
        private Button buttonCreateANewVMGroup;
        private Button buttonLoadGroupsrefresh;
        private GroupBox groupBox2;
        private Button buttonManageServerMembers;
        private Label toolstripstatuslabelMain_CreatedBy;
        private Button buttonSummaryClustersOverviewView;
        private Label label1;
        private DataGridView datagridviewhvHosts;
        private Button buttonLoadHostsrefresh;
        private Label labelClustersHelpText;
        private Button buttonRefreshClusterInfo;
        private GroupBox groupBoxClusterInfo;
        private Label labelClusterName;
        private Label labelClusterNameValue;
        private Label labelTotalNodes;
        private Label labelTotalNodesValue;
        private Label labelCurrentNode;
        private Label labelCurrentNodeValue;
        private Label labelClusterNetworks;
        private Label labelClusterNetworksValue;
        private Label labelSharedVolumes;
        private Label labelSharedVolumesValue;
        private Label labelClusterNodes;
        private DataGridView datagridviewClusterNodes;
        private Label labelClusterVMs;
        private DataGridView datagridviewClusterVMs;
        private TabPage tabPagehvDisks;
        private Button buttonSummaryvDiskView;
        private Button buttonLoadvDiskrefresh;
        private Label labelvDiskOverviewText;
        private DataGridView datagridviewvDiskOverView;
        private Button buttonExportVMvmOverviewView;
        private Button buttonSummaryvCheckpointsView;
        private Button buttonLoadvCheckpointsrefresh;
        private Label labelvCheckpointsOverviewText;
        private DataGridView datagridviewCheckpointOverView;
        private Label labelvStorageOverviewText;
        private DataGridView datagridviewHealthOverview;
        private Button buttonExportHealthOverview;
        private Button buttonSummaryHealthOverviewView;
        private Button buttonLoadHealthOverview;
        private Label labelHealthOverviewText;
        private Button buttonSummaryHealthOverviewHelp;
        private ComboBox comboBoxClusterNodeSelector;
        private Label labelClusterNodeSelector;
        private Button buttonSummaryvHostsView;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem exportAllDataToolStripMenuItem;
        private ToolStripMenuItem exportCurrentTabToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem refreshDataToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem clearCacheToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem expandAllCollumsToolStripMenuItem;
        private ToolStripMenuItem autoSizeAllCollumsToolStripMenuItem;
        private Panel panelSearch;
        private TextBox textBoxSearch;
        private Button buttonSearchPrevious;
        private Button buttonSearchNext;
        private Label labelSearchResults;
        private Button buttonCloseSearch;
        private Label labelSearchIcon;
        private CheckBox checkBoxFilterResults;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem autoRefreshToolStripMenuItem;
        private ToolStripMenuItem disabledMinuteToolStripMenuItem;
        private ToolStripMenuItem every1MinuteToolStripMenuItem;
        private ToolStripMenuItem every5MinutesToolStripMenuItem;
        private ToolStripMenuItem every10MinutesToolStripMenuItem;
        private TabPage tabPagehvCPU;
        private TabPage tabPagehvMemory;
        private TabPage tabPagehvReplication;
        private TabPage tabPagehvDVD;
        private ToolStripMenuItem copySelectionToClipboardToolStripMenuItem;
    }
}
