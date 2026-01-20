using HyperView.Class;

namespace HyperView.Forms
{
    public partial class CreateVMGroupForm : Form
    {
        private class VMGroupTemplate
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
        }

        public class VMGroupResult
        {
            public string GroupName { get; set; }
            public string GroupType { get; set; }
        }

        public VMGroupResult Result { get; private set; }

        private int _originalFormHeight;
        private int _expandedFormHeight;
        private VMGroupTemplate _selectedTemplate;

        public CreateVMGroupForm()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Set initial form height
            this.Height = 113;
            _originalFormHeight = this.Height;

            // Initialize combo box with group types
            if (comboboxGroupType != null)
            {
                comboboxGroupType.Items.Clear();
                comboboxGroupType.Items.Add("VMCollectionType");
                comboboxGroupType.Items.Add("ManagementCollectionType");
                comboboxGroupType.SelectedIndex = 0;
            }

            // Initially hide the recommended templates section
            if (groupboxRecommended != null)
            {
                groupboxRecommended.Visible = false;
                _expandedFormHeight = _originalFormHeight + groupboxRecommended.Height + 20;
            }

            // Initialize button states
            if (buttonUseSelectedTemplate != null)
            {
                buttonUseSelectedTemplate.Enabled = false;
            }
        }

        private List<VMGroupTemplate> GetRecommendedVMGroups()
        {
            var groups = new List<VMGroupTemplate>
            {
                new VMGroupTemplate
                {
                    Name = "Production Web Servers",
                    Type = "VMCollectionType",
                    Description = "High-priority web servers requiring maximum uptime and load balancing",
                    Category = "Production"
                },
                new VMGroupTemplate
                {
                    Name = "Development Environment",
                    Type = "VMCollectionType",
                    Description = "Development and testing virtual machines for software development",
                    Category = "Development"
                },
                new VMGroupTemplate
                {
                    Name = "Database Servers",
                    Type = "VMCollectionType",
                    Description = "Critical database servers with high resource requirements and backup needs",
                    Category = "Production"
                },
                new VMGroupTemplate
                {
                    Name = "Domain Controllers",
                    Type = "VMCollectionType",
                    Description = "Active Directory domain controllers for authentication and directory services",
                    Category = "Infrastructure"
                },
                new VMGroupTemplate
                {
                    Name = "File and Print Servers",
                    Type = "VMCollectionType",
                    Description = "File servers and print servers for document management and printing",
                    Category = "Infrastructure"
                },
                new VMGroupTemplate
                {
                    Name = "Application Servers",
                    Type = "VMCollectionType",
                    Description = "Business application servers hosting enterprise applications",
                    Category = "Production"
                },
                new VMGroupTemplate
                {
                    Name = "Test Lab VMs",
                    Type = "VMCollectionType",
                    Description = "Testing and lab virtual machines for experimentation and validation",
                    Category = "Testing"
                },
                new VMGroupTemplate
                {
                    Name = "Backup and Recovery",
                    Type = "VMCollectionType",
                    Description = "Backup servers and disaster recovery systems",
                    Category = "Infrastructure"
                },
                new VMGroupTemplate
                {
                    Name = "Monitoring Systems",
                    Type = "VMCollectionType",
                    Description = "System monitoring, logging, and management tools",
                    Category = "Management"
                },
                new VMGroupTemplate
                {
                    Name = "DMZ Servers",
                    Type = "VMCollectionType",
                    Description = "Demilitarized zone servers for external-facing services",
                    Category = "Security"
                },
                new VMGroupTemplate
                {
                    Name = "Email Servers",
                    Type = "VMCollectionType",
                    Description = "Exchange servers and email infrastructure components",
                    Category = "Infrastructure"
                },
                new VMGroupTemplate
                {
                    Name = "Terminal Services",
                    Type = "VMCollectionType",
                    Description = "Remote Desktop Services and virtual desktop infrastructure",
                    Category = "Infrastructure"
                }
            };

            FileLogger.Message($"Retrieved {groups.Count} recommended VM group templates",
                FileLogger.EventType.Information, 3000);

            return groups;
        }

        private void InitializeRecommendedVMGroupsTreeView()
        {
            try
            {
                if (treeviewTemplates == null)
                    return;

                FileLogger.Message("Starting TreeView initialization for recommended VM Groups...",
                    FileLogger.EventType.Information, 3001);

                var recommendedGroups = GetRecommendedVMGroups();
                FileLogger.Message($"Retrieved {recommendedGroups.Count} recommended groups",
                    FileLogger.EventType.Information, 3002);

                // Clear the TreeView
                treeviewTemplates.Nodes.Clear();

                // Group by Category
                var categories = new Dictionary<string, List<VMGroupTemplate>>();
                foreach (var group in recommendedGroups)
                {
                    string categoryName = string.IsNullOrEmpty(group.Category) ? "Uncategorized" : group.Category;

                    if (!categories.ContainsKey(categoryName))
                    {
                        categories[categoryName] = new List<VMGroupTemplate>();
                    }
                    categories[categoryName].Add(group);
                }

                FileLogger.Message($"Found {categories.Keys.Count} categories: {string.Join(", ", categories.Keys)}",
                    FileLogger.EventType.Information, 3003);

                treeviewTemplates.BeginUpdate();

                foreach (var category in categories)
                {
                    FileLogger.Message($"Processing category: '{category.Key}'",
                        FileLogger.EventType.Information, 3004);

                    // Create category node
                    var categoryNode = new TreeNode
                    {
                        Text = category.Key,
                        ForeColor = Color.DarkBlue
                    };

                    // Try to make category node bold
                    try
                    {
                        categoryNode.NodeFont = new Font(treeviewTemplates.Font, FontStyle.Bold);
                    }
                    catch
                    {
                        // If font setting fails, continue without bold
                    }

                    // Add child nodes for each group in this category
                    foreach (var group in category.Value)
                    {
                        FileLogger.Message($"  Adding group: {group.Name}",
                            FileLogger.EventType.Information, 3005);

                        var groupNode = new TreeNode
                        {
                            Text = group.Name,
                            Tag = group // Store the group data
                        };

                        // Add the group node as a child of the category node
                        categoryNode.Nodes.Add(groupNode);
                    }

                    // Expand the category to show its children
                    categoryNode.Expand();

                    // Add the category node to the TreeView
                    treeviewTemplates.Nodes.Add(categoryNode);
                    FileLogger.Message($"Added category: '{category.Key}' with {categoryNode.Nodes.Count} children",
                        FileLogger.EventType.Information, 3006);
                }

                treeviewTemplates.EndUpdate();

                FileLogger.Message($"TreeView initialization complete. Total nodes: {treeviewTemplates.Nodes.Count}",
                    FileLogger.EventType.Information, 3007);

                // Initial state
                if (buttonUseSelectedTemplate != null)
                    buttonUseSelectedTemplate.Enabled = false;

                if (textboxDescription != null)
                    textboxDescription.Text = "";

                if (labelSelected != null)
                    labelSelected.Text = "No template selected";

                // Force refresh
                treeviewTemplates.Refresh();
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in TreeView initialization: {ex.Message}",
                    FileLogger.EventType.Error, 3008);
                treeviewTemplates.EndUpdate();
            }
        }

        private void CreateVMGroupForm_Load(object sender, EventArgs e)
        {
            FileLogger.Message("CreateVMGroupForm loaded",
                FileLogger.EventType.Information, 3009);
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textboxGroupName?.Text))
            {
                MessageBox.Show(@"Please enter a group name.", @"Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Store result
            Result = new VMGroupResult
            {
                GroupName = textboxGroupName.Text.Trim(),
                GroupType = comboboxGroupType?.SelectedItem?.ToString() ?? "VMCollectionType"
            };

            FileLogger.Message($"VM Group created: Name='{Result.GroupName}', Type='{Result.GroupType}'",
                FileLogger.EventType.Information, 3010);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            FileLogger.Message("VM Group creation cancelled",
                FileLogger.EventType.Information, 3011);

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ButtonRecommended_Click(object sender, EventArgs e)
        {
            if (groupboxRecommended == null || buttonRecommended == null)
                return;

            // Toggle the visibility of the recommended templates section
            groupboxRecommended.Visible = !groupboxRecommended.Visible;

            // Update button text and form size based on visibility
            if (groupboxRecommended.Visible)
            {
                buttonRecommended.Text = "Hide Templates";
                // Expand the form to show templates section
                this.Height = _expandedFormHeight;

                FileLogger.Message("Showing recommended VM group templates",
                    FileLogger.EventType.Information, 3012);

                // Initialize TreeView ONLY when showing
                InitializeRecommendedVMGroupsTreeView();
            }
            else
            {
                buttonRecommended.Text = "Templates...";
                // Collapse the form to original size
                this.Height = _originalFormHeight;

                FileLogger.Message("Hiding recommended VM group templates",
                    FileLogger.EventType.Information, 3013);
            }
        }

        private void TreeviewTemplates_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNode = treeviewTemplates.SelectedNode;
            
            if (selectedNode != null && selectedNode.Tag is VMGroupTemplate group)
            {
                if (labelSelected != null)
                    labelSelected.Text = $"Selected: {group.Name}";

                if (textboxDescription != null)
                    textboxDescription.Text = group.Description;

                if (buttonUseSelectedTemplate != null)
                    buttonUseSelectedTemplate.Enabled = true;

                // Store the selected template for the Use Template button
                _selectedTemplate = group;

                FileLogger.Message($"Template selected: '{group.Name}' ({group.Category})",
                    FileLogger.EventType.Information, 3014);
            }
            else
            {
                if (labelSelected != null)
                    labelSelected.Text = "No template selected";

                if (textboxDescription != null)
                    textboxDescription.Text = "";

                if (buttonUseSelectedTemplate != null)
                    buttonUseSelectedTemplate.Enabled = false;

                _selectedTemplate = null;
            }
        }

        private void ButtonUseSelectedTemplate_Click(object sender, EventArgs e)
        {
            if (_selectedTemplate == null)
                return;

            // Populate the form fields with the selected template
            if (textboxGroupName != null)
                textboxGroupName.Text = _selectedTemplate.Name;

            // Set the group type
            if (comboboxGroupType != null)
            {
                for (int i = 0; i < comboboxGroupType.Items.Count; i++)
                {
                    if (comboboxGroupType.Items[i].ToString() == _selectedTemplate.Type)
                    {
                        comboboxGroupType.SelectedIndex = i;
                        break;
                    }
                }
            }

            FileLogger.Message($"Using template: '{_selectedTemplate.Name}'",
                FileLogger.EventType.Information, 3015);

            // Update button text
            if (buttonRecommended != null)
                buttonRecommended.Text = "Templates...";

            // Collapse the form to original size
            this.Height = _originalFormHeight;

            // Hide the templates section
            if (groupboxRecommended != null)
                groupboxRecommended.Visible = false;

            // Focus on group name
            if (textboxGroupName != null)
                textboxGroupName.Focus();
        }
    }
}
