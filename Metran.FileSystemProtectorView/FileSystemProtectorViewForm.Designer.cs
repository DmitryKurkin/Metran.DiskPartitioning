namespace Metran.FileSystemView
{
    partial class FileSystemProtectorViewForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileSystemProtectorViewForm));
            this.buttonDiskRefresh = new System.Windows.Forms.Button();
            this.bindingSourceDiskSelection = new System.Windows.Forms.BindingSource(this.components);
            this.buttonDiskLoad = new System.Windows.Forms.Button();
            this.bindingSourceDiskLoading = new System.Windows.Forms.BindingSource(this.components);
            this.comboBoxDiskList = new System.Windows.Forms.ComboBox();
            this.groupBoxDiskContents = new System.Windows.Forms.GroupBox();
            this.buttonProtect = new System.Windows.Forms.Button();
            this.bindingSourceDiskContents = new System.Windows.Forms.BindingSource(this.components);
            this.buttonUnprotect = new System.Windows.Forms.Button();
            this.treeViewDiskContents = new System.Windows.Forms.TreeView();
            this.imageListDiskContents = new System.Windows.Forms.ImageList(this.components);
            this.groupBoxDiskSelection = new System.Windows.Forms.GroupBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.groupBoxDiskLoading = new System.Windows.Forms.GroupBox();
            this.radioButtonFat32 = new System.Windows.Forms.RadioButton();
            this.radioButtonFat16 = new System.Windows.Forms.RadioButton();
            this.listBoxEventLog = new System.Windows.Forms.ListBox();
            this.bindingSourceEvents = new System.Windows.Forms.BindingSource(this.components);
            this.bindingSourceEventLog = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceDiskSelection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceDiskLoading)).BeginInit();
            this.groupBoxDiskContents.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceDiskContents)).BeginInit();
            this.groupBoxDiskSelection.SuspendLayout();
            this.groupBoxDiskLoading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceEvents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceEventLog)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonDiskRefresh
            // 
            this.buttonDiskRefresh.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskSelection, "IsRefreshEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.buttonDiskRefresh.Location = new System.Drawing.Point(183, 19);
            this.buttonDiskRefresh.Name = "buttonDiskRefresh";
            this.buttonDiskRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonDiskRefresh.TabIndex = 0;
            this.buttonDiskRefresh.Text = "Refresh";
            this.buttonDiskRefresh.UseVisualStyleBackColor = true;
            this.buttonDiskRefresh.Click += new System.EventHandler(this.buttonDiskRefresh_Click);
            // 
            // bindingSourceDiskSelection
            // 
            this.bindingSourceDiskSelection.DataSource = typeof(Metran.FileSystemViewModel.IDiskSelectionViewModel);
            // 
            // buttonDiskLoad
            // 
            this.buttonDiskLoad.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskLoading, "IsLoadEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.buttonDiskLoad.Location = new System.Drawing.Point(132, 19);
            this.buttonDiskLoad.Name = "buttonDiskLoad";
            this.buttonDiskLoad.Size = new System.Drawing.Size(75, 23);
            this.buttonDiskLoad.TabIndex = 1;
            this.buttonDiskLoad.Text = "Load";
            this.buttonDiskLoad.UseVisualStyleBackColor = true;
            this.buttonDiskLoad.Click += new System.EventHandler(this.buttonDiskLoad_Click);
            // 
            // bindingSourceDiskLoading
            // 
            this.bindingSourceDiskLoading.DataSource = typeof(Metran.FileSystemViewModel.IDiskLoadingViewModel);
            // 
            // comboBoxDiskList
            // 
            this.comboBoxDiskList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDiskList.FormattingEnabled = true;
            this.comboBoxDiskList.Location = new System.Drawing.Point(6, 21);
            this.comboBoxDiskList.Name = "comboBoxDiskList";
            this.comboBoxDiskList.Size = new System.Drawing.Size(171, 21);
            this.comboBoxDiskList.TabIndex = 2;
            // 
            // groupBoxDiskContents
            // 
            this.groupBoxDiskContents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDiskContents.Controls.Add(this.buttonProtect);
            this.groupBoxDiskContents.Controls.Add(this.buttonUnprotect);
            this.groupBoxDiskContents.Controls.Add(this.treeViewDiskContents);
            this.groupBoxDiskContents.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskContents, "IsEnabled", true));
            this.groupBoxDiskContents.Location = new System.Drawing.Point(12, 66);
            this.groupBoxDiskContents.Name = "groupBoxDiskContents";
            this.groupBoxDiskContents.Size = new System.Drawing.Size(564, 357);
            this.groupBoxDiskContents.TabIndex = 4;
            this.groupBoxDiskContents.TabStop = false;
            this.groupBoxDiskContents.Text = "Disk Contents";
            // 
            // buttonProtect
            // 
            this.buttonProtect.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskContents, "IsButtonProtectEnabled", true));
            this.buttonProtect.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonProtect.Location = new System.Drawing.Point(3, 308);
            this.buttonProtect.Name = "buttonProtect";
            this.buttonProtect.Size = new System.Drawing.Size(558, 23);
            this.buttonProtect.TabIndex = 2;
            this.buttonProtect.Text = "Protect";
            this.buttonProtect.UseVisualStyleBackColor = true;
            this.buttonProtect.Click += new System.EventHandler(this.buttonProtect_Click);
            // 
            // bindingSourceDiskContents
            // 
            this.bindingSourceDiskContents.DataSource = typeof(Metran.FileSystemViewModel.IDiskContentsViewModel);
            // 
            // buttonUnprotect
            // 
            this.buttonUnprotect.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskContents, "IsButtonUnprotectEnabled", true));
            this.buttonUnprotect.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonUnprotect.Location = new System.Drawing.Point(3, 331);
            this.buttonUnprotect.Name = "buttonUnprotect";
            this.buttonUnprotect.Size = new System.Drawing.Size(558, 23);
            this.buttonUnprotect.TabIndex = 1;
            this.buttonUnprotect.Text = "Unprotect";
            this.buttonUnprotect.UseVisualStyleBackColor = true;
            this.buttonUnprotect.Click += new System.EventHandler(this.buttonUnprotect_Click);
            // 
            // treeViewDiskContents
            // 
            this.treeViewDiskContents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewDiskContents.HideSelection = false;
            this.treeViewDiskContents.ImageIndex = 0;
            this.treeViewDiskContents.ImageList = this.imageListDiskContents;
            this.treeViewDiskContents.Location = new System.Drawing.Point(3, 16);
            this.treeViewDiskContents.Name = "treeViewDiskContents";
            this.treeViewDiskContents.SelectedImageIndex = 0;
            this.treeViewDiskContents.Size = new System.Drawing.Size(558, 286);
            this.treeViewDiskContents.TabIndex = 0;
            this.treeViewDiskContents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewDiskContents_AfterSelect);
            // 
            // imageListDiskContents
            // 
            this.imageListDiskContents.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListDiskContents.ImageStream")));
            this.imageListDiskContents.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListDiskContents.Images.SetKeyName(0, "Folder_48x48.png");
            this.imageListDiskContents.Images.SetKeyName(1, "Generic_Application.png");
            this.imageListDiskContents.Images.SetKeyName(2, "FolderOpen_48x48_72.png");
            // 
            // groupBoxDiskSelection
            // 
            this.groupBoxDiskSelection.Controls.Add(this.comboBoxDiskList);
            this.groupBoxDiskSelection.Controls.Add(this.buttonDiskRefresh);
            this.groupBoxDiskSelection.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskSelection, "IsEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.groupBoxDiskSelection.Location = new System.Drawing.Point(12, 12);
            this.groupBoxDiskSelection.Name = "groupBoxDiskSelection";
            this.groupBoxDiskSelection.Size = new System.Drawing.Size(264, 48);
            this.groupBoxDiskSelection.TabIndex = 5;
            this.groupBoxDiskSelection.TabStop = false;
            this.groupBoxDiskSelection.Text = "Disk Selection";
            // 
            // buttonClose
            // 
            this.buttonClose.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskLoading, "IsCloseEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.buttonClose.Location = new System.Drawing.Point(213, 19);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // groupBoxDiskLoading
            // 
            this.groupBoxDiskLoading.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDiskLoading.Controls.Add(this.radioButtonFat32);
            this.groupBoxDiskLoading.Controls.Add(this.radioButtonFat16);
            this.groupBoxDiskLoading.Controls.Add(this.buttonClose);
            this.groupBoxDiskLoading.Controls.Add(this.buttonDiskLoad);
            this.groupBoxDiskLoading.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskLoading, "IsEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.groupBoxDiskLoading.Location = new System.Drawing.Point(282, 12);
            this.groupBoxDiskLoading.Name = "groupBoxDiskLoading";
            this.groupBoxDiskLoading.Size = new System.Drawing.Size(294, 48);
            this.groupBoxDiskLoading.TabIndex = 6;
            this.groupBoxDiskLoading.TabStop = false;
            this.groupBoxDiskLoading.Text = "Disk Loading";
            // 
            // radioButtonFat32
            // 
            this.radioButtonFat32.AutoSize = true;
            this.radioButtonFat32.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceDiskLoading, "IsFat32Checked", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButtonFat32.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskLoading, "IsFatSelectorsEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButtonFat32.Location = new System.Drawing.Point(69, 22);
            this.radioButtonFat32.Name = "radioButtonFat32";
            this.radioButtonFat32.Size = new System.Drawing.Size(57, 17);
            this.radioButtonFat32.TabIndex = 5;
            this.radioButtonFat32.Text = "FAT32";
            this.radioButtonFat32.UseVisualStyleBackColor = true;
            // 
            // radioButtonFat16
            // 
            this.radioButtonFat16.AutoSize = true;
            this.radioButtonFat16.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceDiskLoading, "IsFat16Checked", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButtonFat16.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceDiskLoading, "IsFatSelectorsEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButtonFat16.Location = new System.Drawing.Point(6, 22);
            this.radioButtonFat16.Name = "radioButtonFat16";
            this.radioButtonFat16.Size = new System.Drawing.Size(57, 17);
            this.radioButtonFat16.TabIndex = 4;
            this.radioButtonFat16.Text = "FAT16";
            this.radioButtonFat16.UseVisualStyleBackColor = true;
            // 
            // listBoxEventLog
            // 
            this.listBoxEventLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxEventLog.DataSource = this.bindingSourceEvents;
            this.listBoxEventLog.FormattingEnabled = true;
            this.listBoxEventLog.Location = new System.Drawing.Point(12, 429);
            this.listBoxEventLog.Name = "listBoxEventLog";
            this.listBoxEventLog.Size = new System.Drawing.Size(564, 56);
            this.listBoxEventLog.TabIndex = 7;
            // 
            // bindingSourceEvents
            // 
            this.bindingSourceEvents.DataMember = "Events";
            this.bindingSourceEvents.DataSource = this.bindingSourceEventLog;
            // 
            // bindingSourceEventLog
            // 
            this.bindingSourceEventLog.DataSource = typeof(Metran.FileSystemViewModel.IEventLogViewModel);
            // 
            // FileSystemProtectorViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 501);
            this.Controls.Add(this.listBoxEventLog);
            this.Controls.Add(this.groupBoxDiskLoading);
            this.Controls.Add(this.groupBoxDiskSelection);
            this.Controls.Add(this.groupBoxDiskContents);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(604, 535);
            this.Name = "FileSystemProtectorViewForm";
            this.Text = "File System Protector v1.0.0.1";
            this.Load += new System.EventHandler(this.FileSystemViewForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceDiskSelection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceDiskLoading)).EndInit();
            this.groupBoxDiskContents.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceDiskContents)).EndInit();
            this.groupBoxDiskSelection.ResumeLayout(false);
            this.groupBoxDiskLoading.ResumeLayout(false);
            this.groupBoxDiskLoading.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceEvents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceEventLog)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonDiskRefresh;
        private System.Windows.Forms.Button buttonDiskLoad;
        private System.Windows.Forms.ComboBox comboBoxDiskList;
        private System.Windows.Forms.GroupBox groupBoxDiskContents;
        private System.Windows.Forms.GroupBox groupBoxDiskSelection;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.GroupBox groupBoxDiskLoading;
        private System.Windows.Forms.RadioButton radioButtonFat32;
        private System.Windows.Forms.RadioButton radioButtonFat16;
        private System.Windows.Forms.BindingSource bindingSourceDiskSelection;
        private System.Windows.Forms.BindingSource bindingSourceDiskLoading;
        private System.Windows.Forms.ListBox listBoxEventLog;
        private System.Windows.Forms.BindingSource bindingSourceEventLog;
        private System.Windows.Forms.BindingSource bindingSourceEvents;
        private System.Windows.Forms.TreeView treeViewDiskContents;
        private System.Windows.Forms.Button buttonUnprotect;
        private System.Windows.Forms.Button buttonProtect;
        private System.Windows.Forms.BindingSource bindingSourceDiskContents;
        private System.Windows.Forms.ImageList imageListDiskContents;
    }
}

