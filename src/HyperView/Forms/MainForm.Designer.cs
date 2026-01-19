namespace HyperView
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
            buttonSummaryhvOverviewView = new Button();
            buttonLoadVMsrefresh = new Button();
            labelOverviewHelpText = new Label();
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
            tabpagehvHosts = new TabPage();
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
            tabPagehvDisks = new TabPage();
            buttonSummaryvDiskView = new Button();
            buttonLoadvDiskrefresh = new Button();
            labelvDiskOverviewText = new Label();
            datagridviewvDiskOverView = new DataGridView();
            tabpagehvCheckpoints = new TabPage();
            tabpagehvReplica = new TabPage();
            tabpagehvResources = new TabPage();
            tabpageManageNetwork = new TabPage();
            tabpagehvNetworking = new TabPage();
            tabpagehvSecurity = new TabPage();
            tabpagehvPerformance = new TabPage();
            tabpagehvCompliance = new TabPage();
            tabpagehvInventory = new TabPage();
            tabpageCreateVM = new TabPage();
            tabpageHealthOverview = new TabPage();
            menuStripTopMainForm = new MenuStrip();
            menuToolStripMenuItem = new ToolStripMenuItem();
            disconnectToolStripMenuItem = new ToolStripMenuItem();
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
            allVMDataToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            pictureboxSupportMe = new PictureBox();
            statusStripMainForm = new StatusStrip();
            toolStripStatusLabelMainForm = new ToolStripStatusLabel();
            toolStripStatusLabelTextMainForm = new ToolStripStatusLabel();
            groupBoxMainFormServerDetails = new GroupBox();
            toolstripstatuslabelMain_CreatedBy = new Label();
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).BeginInit();
            tabcontrolMainForm.SuspendLayout();
            tabpagehvOverview.SuspendLayout();
            tabpageVMGroups.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewVMGroups).BeginInit();
            tabpagehvHosts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewhvHosts).BeginInit();
            tabpagehvClusters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterVMs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterNodes).BeginInit();
            groupBoxClusterInfo.SuspendLayout();
            tabPagehvDisks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewvDiskOverView).BeginInit();
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
            datagridviewVMOverView.Size = new Size(1601, 803);
            datagridviewVMOverView.TabIndex = 0;
            datagridviewVMOverView.CellContentDoubleClick += datagridviewVMOverView_CellContentDoubleClick;
            // 
            // tabcontrolMainForm
            // 
            tabcontrolMainForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabcontrolMainForm.Controls.Add(tabpagehvOverview);
            tabcontrolMainForm.Controls.Add(tabpageVMGroups);
            tabcontrolMainForm.Controls.Add(tabpagehvHosts);
            tabcontrolMainForm.Controls.Add(tabpagehvClusters);
            tabcontrolMainForm.Controls.Add(tabpagehvStorage);
            tabcontrolMainForm.Controls.Add(tabPagehvDisks);
            tabcontrolMainForm.Controls.Add(tabpagehvCheckpoints);
            tabcontrolMainForm.Controls.Add(tabpagehvReplica);
            tabcontrolMainForm.Controls.Add(tabpagehvResources);
            tabcontrolMainForm.Controls.Add(tabpageManageNetwork);
            tabcontrolMainForm.Controls.Add(tabpagehvNetworking);
            tabcontrolMainForm.Controls.Add(tabpagehvSecurity);
            tabcontrolMainForm.Controls.Add(tabpagehvPerformance);
            tabcontrolMainForm.Controls.Add(tabpagehvCompliance);
            tabcontrolMainForm.Controls.Add(tabpagehvInventory);
            tabcontrolMainForm.Controls.Add(tabpageCreateVM);
            tabcontrolMainForm.Controls.Add(tabpageHealthOverview);
            tabcontrolMainForm.Location = new Point(12, 27);
            tabcontrolMainForm.Name = "tabcontrolMainForm";
            tabcontrolMainForm.SelectedIndex = 0;
            tabcontrolMainForm.Size = new Size(1621, 872);
            tabcontrolMainForm.TabIndex = 1;
            // 
            // tabpagehvOverview
            // 
            tabpagehvOverview.Controls.Add(buttonSummaryhvOverviewView);
            tabpagehvOverview.Controls.Add(buttonLoadVMsrefresh);
            tabpagehvOverview.Controls.Add(labelOverviewHelpText);
            tabpagehvOverview.Controls.Add(datagridviewVMOverView);
            tabpagehvOverview.Location = new Point(4, 24);
            tabpagehvOverview.Name = "tabpagehvOverview";
            tabpagehvOverview.Padding = new Padding(3);
            tabpagehvOverview.Size = new Size(1613, 844);
            tabpagehvOverview.TabIndex = 0;
            tabpagehvOverview.Text = "hvView";
            tabpagehvOverview.UseVisualStyleBackColor = true;
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
            labelOverviewHelpText.Size = new Size(705, 30);
            labelOverviewHelpText.TabIndex = 1;
            labelOverviewHelpText.Text = "This view provides provides overview and core functionality within the Hyper-V space for information about VMs and other data that\r\nextends that functionality over multiple servers.";
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
            tabpageVMGroups.Size = new Size(1613, 844);
            tabpageVMGroups.TabIndex = 1;
            tabpageVMGroups.Text = "hvVMGroups";
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
            labelThisViewProvidesOver.Size = new Size(712, 30);
            labelThisViewProvidesOver.TabIndex = 2;
            labelThisViewProvidesOver.Text = "This view provides overview and core functionality within the Hyper-V space for management of VM Groups for VMs that extends that\r\nfunctionality over multiple servers.";
            // 
            // datagridviewVMGroups
            // 
            datagridviewVMGroups.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewVMGroups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewVMGroups.Location = new Point(6, 35);
            datagridviewVMGroups.Name = "datagridviewVMGroups";
            datagridviewVMGroups.Size = new Size(1395, 803);
            datagridviewVMGroups.TabIndex = 0;
            // 
            // tabpagehvHosts
            // 
            tabpagehvHosts.Controls.Add(buttonLoadHostsrefresh);
            tabpagehvHosts.Controls.Add(datagridviewhvHosts);
            tabpagehvHosts.Controls.Add(label1);
            tabpagehvHosts.Location = new Point(4, 24);
            tabpagehvHosts.Name = "tabpagehvHosts";
            tabpagehvHosts.Size = new Size(1613, 844);
            tabpagehvHosts.TabIndex = 3;
            tabpagehvHosts.Text = "hvHosts";
            tabpagehvHosts.UseVisualStyleBackColor = true;
            // 
            // buttonLoadHostsrefresh
            // 
            buttonLoadHostsrefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadHostsrefresh.Location = new Point(1490, 8);
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
            datagridviewhvHosts.Location = new Point(9, 38);
            datagridviewhvHosts.Name = "datagridviewhvHosts";
            datagridviewhvHosts.ReadOnly = true;
            datagridviewhvHosts.Size = new Size(1601, 803);
            datagridviewhvHosts.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 3);
            label1.Name = "label1";
            label1.Size = new Size(457, 15);
            label1.TabIndex = 0;
            label1.Text = "This view provides overview the Hyper-V Host space and information about hardware";
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
            tabpagehvClusters.Size = new Size(1613, 844);
            tabpagehvClusters.TabIndex = 4;
            tabpagehvClusters.Text = "hvClusters";
            tabpagehvClusters.UseVisualStyleBackColor = true;
            // 
            // datagridviewClusterVMs
            // 
            datagridviewClusterVMs.AllowUserToAddRows = false;
            datagridviewClusterVMs.AllowUserToDeleteRows = false;
            datagridviewClusterVMs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewClusterVMs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewClusterVMs.Location = new Point(6, 545);
            datagridviewClusterVMs.Name = "datagridviewClusterVMs";
            datagridviewClusterVMs.ReadOnly = true;
            datagridviewClusterVMs.Size = new Size(1604, 296);
            datagridviewClusterVMs.TabIndex = 13;
            // 
            // labelClusterVMs
            // 
            labelClusterVMs.AutoSize = true;
            labelClusterVMs.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelClusterVMs.Location = new Point(6, 527);
            labelClusterVMs.Name = "labelClusterVMs";
            labelClusterVMs.Size = new Size(167, 15);
            labelClusterVMs.TabIndex = 12;
            labelClusterVMs.Text = "Highly Available VMs (0 VMs)";
            // 
            // datagridviewClusterNodes
            // 
            datagridviewClusterNodes.AllowUserToAddRows = false;
            datagridviewClusterNodes.AllowUserToDeleteRows = false;
            datagridviewClusterNodes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewClusterNodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewClusterNodes.Location = new Point(6, 308);
            datagridviewClusterNodes.Name = "datagridviewClusterNodes";
            datagridviewClusterNodes.ReadOnly = true;
            datagridviewClusterNodes.Size = new Size(1604, 200);
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
            groupBoxClusterInfo.Size = new Size(1604, 240);
            groupBoxClusterInfo.TabIndex = 7;
            groupBoxClusterInfo.TabStop = false;
            groupBoxClusterInfo.Text = "Cluster Information";
            // 
            // labelSharedVolumesValue
            // 
            labelSharedVolumesValue.AutoSize = true;
            labelSharedVolumesValue.Location = new Point(150, 169);
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
            labelClusterNetworksValue.Location = new Point(150, 139);
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
            labelCurrentNodeValue.Location = new Point(150, 109);
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
            labelTotalNodesValue.Location = new Point(150, 79);
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
            labelClusterNameValue.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelClusterNameValue.ForeColor = Color.DarkBlue;
            labelClusterNameValue.Location = new Point(150, 40);
            labelClusterNameValue.Name = "labelClusterNameValue";
            labelClusterNameValue.Size = new Size(125, 21);
            labelClusterNameValue.TabIndex = 1;
            labelClusterNameValue.Text = "Not Connected";
            // 
            // labelClusterName
            // 
            labelClusterName.AutoSize = true;
            labelClusterName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelClusterName.ForeColor = Color.DarkBlue;
            labelClusterName.Location = new Point(16, 44);
            labelClusterName.Name = "labelClusterName";
            labelClusterName.Size = new Size(85, 15);
            labelClusterName.TabIndex = 0;
            labelClusterName.Text = "Cluster Name:";
            // 
            // buttonRefreshClusterInfo
            // 
            buttonRefreshClusterInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRefreshClusterInfo.Location = new Point(1486, 3);
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
            labelClustersHelpText.Size = new Size(800, 15);
            labelClustersHelpText.TabIndex = 5;
            labelClustersHelpText.Text = "This view provides overview of Hyper-V Failover Cluster information including nodes, networks, shared volumes, and highly available virtual machines.";
            // 
            // buttonSummaryClustersOverviewView
            // 
            buttonSummaryClustersOverviewView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryClustersOverviewView.Location = new Point(1405, 3);
            buttonSummaryClustersOverviewView.Name = "buttonSummaryClustersOverviewView";
            buttonSummaryClustersOverviewView.Size = new Size(75, 23);
            buttonSummaryClustersOverviewView.TabIndex = 4;
            buttonSummaryClustersOverviewView.Text = "Summary";
            buttonSummaryClustersOverviewView.UseVisualStyleBackColor = true;
            buttonSummaryClustersOverviewView.Click += buttonSummaryClustersOverviewView_Click;
            // 
            // tabpagehvStorage
            // 
            tabpagehvStorage.Location = new Point(4, 24);
            tabpagehvStorage.Name = "tabpagehvStorage";
            tabpagehvStorage.Size = new Size(1613, 844);
            tabpagehvStorage.TabIndex = 5;
            tabpagehvStorage.Text = "hvStorage";
            tabpagehvStorage.UseVisualStyleBackColor = true;
            // 
            // tabPagehvDisks
            // 
            tabPagehvDisks.Controls.Add(buttonSummaryvDiskView);
            tabPagehvDisks.Controls.Add(buttonLoadvDiskrefresh);
            tabPagehvDisks.Controls.Add(labelvDiskOverviewText);
            tabPagehvDisks.Controls.Add(datagridviewvDiskOverView);
            tabPagehvDisks.Location = new Point(4, 24);
            tabPagehvDisks.Name = "tabPagehvDisks";
            tabPagehvDisks.Size = new Size(1613, 844);
            tabPagehvDisks.TabIndex = 16;
            tabPagehvDisks.Text = "hvDisks";
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
            labelvDiskOverviewText.Size = new Size(705, 30);
            labelvDiskOverviewText.TabIndex = 5;
            labelvDiskOverviewText.Text = "This view provides provides overview and core functionality within the Hyper-V space for information about VMs and other data that\r\nextends that functionality over multiple servers.";
            // 
            // datagridviewvDiskOverView
            // 
            datagridviewvDiskOverView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewvDiskOverView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewvDiskOverView.Location = new Point(6, 35);
            datagridviewvDiskOverView.Name = "datagridviewvDiskOverView";
            datagridviewvDiskOverView.Size = new Size(1601, 803);
            datagridviewvDiskOverView.TabIndex = 4;
            // 
            // tabpagehvCheckpoints
            // 
            tabpagehvCheckpoints.Location = new Point(4, 24);
            tabpagehvCheckpoints.Name = "tabpagehvCheckpoints";
            tabpagehvCheckpoints.Size = new Size(1613, 844);
            tabpagehvCheckpoints.TabIndex = 7;
            tabpagehvCheckpoints.Text = "hvCheckpoints";
            tabpagehvCheckpoints.UseVisualStyleBackColor = true;
            // 
            // tabpagehvReplica
            // 
            tabpagehvReplica.Location = new Point(4, 24);
            tabpagehvReplica.Name = "tabpagehvReplica";
            tabpagehvReplica.Size = new Size(1613, 844);
            tabpagehvReplica.TabIndex = 8;
            tabpagehvReplica.Text = "hvReplica";
            tabpagehvReplica.UseVisualStyleBackColor = true;
            // 
            // tabpagehvResources
            // 
            tabpagehvResources.Location = new Point(4, 24);
            tabpagehvResources.Name = "tabpagehvResources";
            tabpagehvResources.Size = new Size(1613, 844);
            tabpagehvResources.TabIndex = 9;
            tabpagehvResources.Text = "hvResources";
            tabpagehvResources.UseVisualStyleBackColor = true;
            // 
            // tabpageManageNetwork
            // 
            tabpageManageNetwork.Location = new Point(4, 24);
            tabpageManageNetwork.Name = "tabpageManageNetwork";
            tabpageManageNetwork.Size = new Size(1613, 844);
            tabpageManageNetwork.TabIndex = 2;
            tabpageManageNetwork.Text = "hvNetwork";
            tabpageManageNetwork.UseVisualStyleBackColor = true;
            // 
            // tabpagehvNetworking
            // 
            tabpagehvNetworking.Location = new Point(4, 24);
            tabpagehvNetworking.Name = "tabpagehvNetworking";
            tabpagehvNetworking.Size = new Size(1613, 844);
            tabpagehvNetworking.TabIndex = 6;
            tabpagehvNetworking.Text = "hvNetworking";
            tabpagehvNetworking.UseVisualStyleBackColor = true;
            // 
            // tabpagehvSecurity
            // 
            tabpagehvSecurity.Location = new Point(4, 24);
            tabpagehvSecurity.Name = "tabpagehvSecurity";
            tabpagehvSecurity.Size = new Size(1613, 844);
            tabpagehvSecurity.TabIndex = 10;
            tabpagehvSecurity.Text = "hvSecurity";
            tabpagehvSecurity.UseVisualStyleBackColor = true;
            // 
            // tabpagehvPerformance
            // 
            tabpagehvPerformance.Location = new Point(4, 24);
            tabpagehvPerformance.Name = "tabpagehvPerformance";
            tabpagehvPerformance.Size = new Size(1613, 844);
            tabpagehvPerformance.TabIndex = 11;
            tabpagehvPerformance.Text = "hvPerformance";
            tabpagehvPerformance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvCompliance
            // 
            tabpagehvCompliance.Location = new Point(4, 24);
            tabpagehvCompliance.Name = "tabpagehvCompliance";
            tabpagehvCompliance.Size = new Size(1613, 844);
            tabpagehvCompliance.TabIndex = 12;
            tabpagehvCompliance.Text = "hvCompliance";
            tabpagehvCompliance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvInventory
            // 
            tabpagehvInventory.Location = new Point(4, 24);
            tabpagehvInventory.Name = "tabpagehvInventory";
            tabpagehvInventory.Size = new Size(1613, 844);
            tabpagehvInventory.TabIndex = 13;
            tabpagehvInventory.Text = "hvInventory";
            tabpagehvInventory.UseVisualStyleBackColor = true;
            // 
            // tabpageCreateVM
            // 
            tabpageCreateVM.Location = new Point(4, 24);
            tabpageCreateVM.Name = "tabpageCreateVM";
            tabpageCreateVM.Size = new Size(1613, 844);
            tabpageCreateVM.TabIndex = 14;
            tabpageCreateVM.Text = "Create VM´s";
            tabpageCreateVM.UseVisualStyleBackColor = true;
            // 
            // tabpageHealthOverview
            // 
            tabpageHealthOverview.Location = new Point(4, 24);
            tabpageHealthOverview.Name = "tabpageHealthOverview";
            tabpageHealthOverview.Size = new Size(1613, 844);
            tabpageHealthOverview.TabIndex = 15;
            tabpageHealthOverview.Text = "Health Overview";
            tabpageHealthOverview.UseVisualStyleBackColor = true;
            // 
            // menuStripTopMainForm
            // 
            menuStripTopMainForm.Items.AddRange(new ToolStripItem[] { menuToolStripMenuItem });
            menuStripTopMainForm.Location = new Point(0, 0);
            menuStripTopMainForm.Name = "menuStripTopMainForm";
            menuStripTopMainForm.Size = new Size(1645, 24);
            menuStripTopMainForm.TabIndex = 2;
            menuStripTopMainForm.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { disconnectToolStripMenuItem, onlineToolStripMenuItem, logsToolStripMenuItem, downloadLastestReleaseFromGitHubToolStripMenuItem, changelogToolStripMenuItem, exportDataToolStripMenuItem, aboutToolStripMenuItem, exitToolStripMenuItem });
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new Size(50, 20);
            menuToolStripMenuItem.Text = "Menu";
            // 
            // disconnectToolStripMenuItem
            // 
            disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            disconnectToolStripMenuItem.Size = new Size(273, 22);
            disconnectToolStripMenuItem.Text = "Disconnect";
            disconnectToolStripMenuItem.Click += disconnectToolStripMenuItem_Click;
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
            exportDataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { allVMDataToolStripMenuItem });
            exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            exportDataToolStripMenuItem.Size = new Size(273, 22);
            exportDataToolStripMenuItem.Text = "Export data";
            // 
            // allVMDataToolStripMenuItem
            // 
            allVMDataToolStripMenuItem.Name = "allVMDataToolStripMenuItem";
            allVMDataToolStripMenuItem.Size = new Size(135, 22);
            allVMDataToolStripMenuItem.Text = "All VM data";
            allVMDataToolStripMenuItem.Click += allVMDataToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(273, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(273, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // pictureboxSupportMe
            // 
            pictureboxSupportMe.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureboxSupportMe.BackColor = Color.Transparent;
            pictureboxSupportMe.Image = (Image)resources.GetObject("pictureboxSupportMe.Image");
            pictureboxSupportMe.Location = new Point(1520, 5);
            pictureboxSupportMe.Name = "pictureboxSupportMe";
            pictureboxSupportMe.Size = new Size(119, 36);
            pictureboxSupportMe.SizeMode = PictureBoxSizeMode.Zoom;
            pictureboxSupportMe.TabIndex = 1;
            pictureboxSupportMe.TabStop = false;
            pictureboxSupportMe.Click += pictureboxSupportMe_Click;
            // 
            // statusStripMainForm
            // 
            statusStripMainForm.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelMainForm, toolStripStatusLabelTextMainForm });
            statusStripMainForm.Location = new Point(0, 977);
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
            // groupBoxMainFormServerDetails
            // 
            groupBoxMainFormServerDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            groupBoxMainFormServerDetails.Location = new Point(12, 905);
            groupBoxMainFormServerDetails.Name = "groupBoxMainFormServerDetails";
            groupBoxMainFormServerDetails.Size = new Size(449, 58);
            groupBoxMainFormServerDetails.TabIndex = 4;
            groupBoxMainFormServerDetails.TabStop = false;
            groupBoxMainFormServerDetails.Text = "Details";
            // 
            // toolstripstatuslabelMain_CreatedBy
            // 
            toolstripstatuslabelMain_CreatedBy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toolstripstatuslabelMain_CreatedBy.AutoSize = true;
            toolstripstatuslabelMain_CreatedBy.BackColor = Color.White;
            toolstripstatuslabelMain_CreatedBy.Location = new Point(1327, 4);
            toolstripstatuslabelMain_CreatedBy.Name = "toolstripstatuslabelMain_CreatedBy";
            toolstripstatuslabelMain_CreatedBy.Size = new Size(190, 15);
            toolstripstatuslabelMain_CreatedBy.TabIndex = 4;
            toolstripstatuslabelMain_CreatedBy.Text = "Created by: Michael Morten Sonne";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1645, 999);
            Controls.Add(toolstripstatuslabelMain_CreatedBy);
            Controls.Add(groupBoxMainFormServerDetails);
            Controls.Add(statusStripMainForm);
            Controls.Add(pictureboxSupportMe);
            Controls.Add(menuStripTopMainForm);
            Controls.Add(tabcontrolMainForm);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripTopMainForm;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            FormClosing += MainForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).EndInit();
            tabcontrolMainForm.ResumeLayout(false);
            tabpagehvOverview.ResumeLayout(false);
            tabpagehvOverview.PerformLayout();
            tabpageVMGroups.ResumeLayout(false);
            tabpageVMGroups.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)datagridviewVMGroups).EndInit();
            tabpagehvHosts.ResumeLayout(false);
            tabpagehvHosts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewhvHosts).EndInit();
            tabpagehvClusters.ResumeLayout(false);
            tabpagehvClusters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterVMs).EndInit();
            ((System.ComponentModel.ISupportInitialize)datagridviewClusterNodes).EndInit();
            groupBoxClusterInfo.ResumeLayout(false);
            groupBoxClusterInfo.PerformLayout();
            tabPagehvDisks.ResumeLayout(false);
            tabPagehvDisks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewvDiskOverView).EndInit();
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
        private TabPage tabpagehvReplica;
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
        private GroupBox groupBoxMainFormServerDetails;
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
        private ToolStripMenuItem allVMDataToolStripMenuItem;
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
    }
}
