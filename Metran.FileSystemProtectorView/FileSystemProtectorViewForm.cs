// -----------------------------------------------------------------------
// <copyright file="FileSystemProtectorViewForm.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Gets.Utils;
using Metran.FileSystemViewModel;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Metran.FileSystemView
{
    public partial class FileSystemProtectorViewForm : Form
    {
        private static void LoadContentsRecursive(TreeNodeCollection nodes, IDirectoryViewModel directoryViewModel)
        {
            foreach (var d in directoryViewModel.Directories)
            {
                var newDirectoryNode = nodes.Add(d.Name);
                newDirectoryNode.Tag = d;
                newDirectoryNode.ImageIndex = 0;
                newDirectoryNode.SelectedImageIndex = 2;

                LoadContentsRecursive(newDirectoryNode.Nodes, d);
            }

            foreach (var f in directoryViewModel.Files)
            {
                var newFileNode = nodes.Add(f.Name);
                newFileNode.Tag = f;
                newFileNode.ImageIndex = 1;
                newFileNode.SelectedImageIndex = 1;
            }
        }

        private readonly IDiskSelectionViewModel _selectionViewModel;

        private readonly IDiskLoadingViewModel _loadingViewModel;

        private readonly IDiskContentsViewModel _contentsViewModel;

        public FileSystemProtectorViewForm(
            IDiskSelectionViewModel selectionViewModel,
            IDiskLoadingViewModel loadingViewModel,
            IDiskContentsViewModel contentsViewModel,
            IEventLogViewModel eventLogModel)
        {
            InitializeComponent();

            _selectionViewModel = selectionViewModel;
            _loadingViewModel = loadingViewModel;
            _contentsViewModel = contentsViewModel;

            comboBoxDiskList.DataSource = _selectionViewModel.AvailableDisks;
            comboBoxDiskList.DataBindings.Add(new Binding(
                "SelectedValue",
                _selectionViewModel,
                "SelectedDisk",
                true,
                DataSourceUpdateMode.OnPropertyChanged));

            (bindingSourceDiskSelection as ISupportInitialize).BeginInit();
            (bindingSourceDiskLoading as ISupportInitialize).BeginInit();
            (bindingSourceDiskContents as ISupportInitialize).BeginInit();
            (bindingSourceEventLog as ISupportInitialize).BeginInit();

            bindingSourceDiskSelection.DataSource = selectionViewModel;
            bindingSourceDiskLoading.DataSource = loadingViewModel;
            bindingSourceDiskContents.DataSource = contentsViewModel;
            bindingSourceEventLog.DataSource = eventLogModel;

            (bindingSourceDiskSelection as ISupportInitialize).EndInit();
            (bindingSourceDiskLoading as ISupportInitialize).EndInit();
            (bindingSourceDiskContents as ISupportInitialize).EndInit();
            (bindingSourceEventLog as ISupportInitialize).EndInit();

            _contentsViewModel.PropertyChanged += contentsViewModel_PropertyChanged;
            LoadContents();
        }

        private void FileSystemViewForm_Load(object sender, EventArgs e)
        {
            _selectionViewModel.Init();
            _loadingViewModel.Init();
        }

        private void buttonDiskRefresh_Click(object sender, EventArgs e)
        {
            _selectionViewModel.Refresh();
        }

        private void buttonDiskLoad_Click(object sender, EventArgs e)
        {
            _loadingViewModel.Load();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            _loadingViewModel.Close();
        }

        private void contentsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.ByOrdinal("RootDirectory"))
            {
                LoadContents();
            }
        }

        private void treeViewDiskContents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _contentsViewModel.SelectedEntity = e.Node.Tag as IFileSystemEntityViewModel;
        }

        private void buttonProtect_Click(object sender, EventArgs e)
        {
            _contentsViewModel.Protect();

            // this code must be in the model, the view must just be listening to the model (as usual)
            treeViewDiskContents.SelectedNode.Text = _contentsViewModel.SelectedEntity.Name;
        }

        private void buttonUnprotect_Click(object sender, EventArgs e)
        {
            _contentsViewModel.Unprotect();

            // this code must be in the model, the view must just be listening to the model (as usual)
            treeViewDiskContents.SelectedNode.Text = _contentsViewModel.SelectedEntity.Name;
        }

        private void LoadContents()
        {
            treeViewDiskContents.SuspendLayout();

            treeViewDiskContents.Nodes.Clear();

            if (_contentsViewModel.IsEnabled)
            {
                LoadContentsRecursive(treeViewDiskContents.Nodes, _contentsViewModel.RootDirectory);
            }

            treeViewDiskContents.ResumeLayout();
        }
    }
}