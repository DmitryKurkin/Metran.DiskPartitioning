using Gets.Utils;
using Metran.FileSystemViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Metran
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IDiskSelectionViewModel _selectionViewModel;

        private readonly IDiskLoadingViewModel _loadingViewModel;

        private readonly IDiskContentsViewModel _contentsViewModel;

        private readonly IEventLogViewModel _eventLogModel;

        public MainWindow(
            IDiskSelectionViewModel selectionViewModel,
            IDiskLoadingViewModel loadingViewModel,
            IDiskContentsViewModel contentsViewModel,
            IEventLogViewModel eventLogModel)
        {
            InitializeComponent();

            _selectionViewModel = selectionViewModel;
            _loadingViewModel = loadingViewModel;
            _contentsViewModel = contentsViewModel;
            _eventLogModel = eventLogModel;

            SetupDataBinding();

            _contentsViewModel.PropertyChanged += contentsViewModel_PropertyChanged;
            LoadContents();
        }

        private void SetupDataBinding()
        {
            //////////////// Disk Selection setup start ////////////////
            var bindingGroupBoxDiskSelection = new Binding("IsEnabled") {Source = _selectionViewModel};
            BindingOperations.SetBinding(groupBoxDiskSelection, IsEnabledProperty, bindingGroupBoxDiskSelection);

            var bindingComboBoxDiskListAvailableDisks = new Binding("AvailableDisks") {Source = _selectionViewModel};
            BindingOperations.SetBinding(comboBoxDiskList, ItemsControl.ItemsSourceProperty,
                bindingComboBoxDiskListAvailableDisks);

            var bindingComboBoxDiskListSelectedDisk = new Binding("SelectedDisk") {Source = _selectionViewModel};
            BindingOperations.SetBinding(comboBoxDiskList, Selector.SelectedItemProperty,
                bindingComboBoxDiskListSelectedDisk);

            var bindingButtonRefresh = new Binding("IsRefreshEnabled") {Source = _selectionViewModel};
            BindingOperations.SetBinding(buttonRefresh, IsEnabledProperty, bindingButtonRefresh);
            ////////////////////////////////////////////////////////////////

            //////////////// Disk Loading setup start ////////////////
            var bindingGroupBoxDiskLoading = new Binding("IsEnabled") {Source = _loadingViewModel};
            BindingOperations.SetBinding(groupBoxDiskLoading, IsEnabledProperty, bindingGroupBoxDiskLoading);

            var bindingRadioButtonFat16Enabled = new Binding("IsFatSelectorsEnabled") {Source = _loadingViewModel};
            BindingOperations.SetBinding(radioButtonFat16, IsEnabledProperty, bindingRadioButtonFat16Enabled);

            var bindingRadioButtonFat16Checked = new Binding("IsFat16Checked") {Source = _loadingViewModel};
            BindingOperations.SetBinding(radioButtonFat16, ToggleButton.IsCheckedProperty,
                bindingRadioButtonFat16Checked);

            var bindingRadioButtonFat32Enabled = new Binding("IsFatSelectorsEnabled") {Source = _loadingViewModel};
            BindingOperations.SetBinding(radioButtonFat32, IsEnabledProperty, bindingRadioButtonFat32Enabled);

            var bindingRadioButtonFat32Checked = new Binding("IsFat326Checked") {Source = _loadingViewModel};
            BindingOperations.SetBinding(radioButtonFat32, ToggleButton.IsCheckedProperty,
                bindingRadioButtonFat32Checked);

            var bindingButtonDiskLoad = new Binding("IsLoadEnabled") {Source = _loadingViewModel};
            BindingOperations.SetBinding(buttonDiskLoad, IsEnabledProperty, bindingButtonDiskLoad);

            var bindingButtonClose = new Binding("IsCloseEnabled") {Source = _loadingViewModel};
            BindingOperations.SetBinding(buttonClose, IsEnabledProperty, bindingButtonClose);
            ////////////////////////////////////////////////////////////////

            //////////////// Disk Contents setup start ////////////////
            var bindingGroupBoxDiskContents = new Binding("IsEnabled") {Source = _contentsViewModel};
            BindingOperations.SetBinding(groupBoxDiskContents, IsEnabledProperty, bindingGroupBoxDiskContents);

            var bindingButtonProtect = new Binding("IsButtonProtectEnabled") {Source = _contentsViewModel};
            BindingOperations.SetBinding(buttonProtect, IsEnabledProperty, bindingButtonProtect);

            var bindingButtonUnprotect = new Binding("IsButtonUnprotectEnabled") {Source = _contentsViewModel};
            BindingOperations.SetBinding(buttonUnprotect, IsEnabledProperty, bindingButtonUnprotect);
            ////////////////////////////////////////////////////////////////

            var bindingListBoxEventLog = new Binding("Events") {Source = _eventLogModel};
            BindingOperations.SetBinding(listBoxEventLog, ItemsControl.ItemsSourceProperty, bindingListBoxEventLog);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((Window) sender).Title = $"{ProjectConstants.Product} v{ProjectConstants.Version}";

            _selectionViewModel.Init();
            _loadingViewModel.Init();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            _selectionViewModel.Refresh();
        }

        private void buttonDiskLoad_Click(object sender, RoutedEventArgs e)
        {
            _loadingViewModel.Load();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            _loadingViewModel.Close();
        }

        private void treeViewDiskContents_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                _contentsViewModel.SelectedEntity = (e.NewValue as TreeViewItem)?.Tag as IFileSystemEntityViewModel;
            }
        }

        private void buttonProtect_Click(object sender, RoutedEventArgs e)
        {
            _contentsViewModel.Protect();
        }

        private void buttonUnprotect_Click(object sender, RoutedEventArgs e)
        {
            _contentsViewModel.Unprotect();
        }

        private void contentsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.ByOrdinal("RootDirectory"))
            {
                LoadContents();
            }
        }

        private void LoadContents()
        {
            using (Dispatcher.DisableProcessing())
            {
                treeViewDiskContents.Items.Clear();

                if (_contentsViewModel.IsEnabled)
                {
                    LoadContentsRecursive(treeViewDiskContents.Items, _contentsViewModel.RootDirectory);
                }
            }
        }

        private static void LoadContentsRecursive(ItemCollection nodes, IDirectoryViewModel directoryViewModel)
        {
            foreach (var d in directoryViewModel.Directories)
            {
                var newDirectoryNode = new TreeViewItem
                {
                    Header = d.Name,
                    Tag = d
                };

                nodes.Add(newDirectoryNode);

                LoadContentsRecursive(newDirectoryNode.Items, d);
            }

            foreach (var f in directoryViewModel.Files)
            {
                var newFileNode = new TreeViewItem
                {
                    Header = f.Name,
                    Tag = f
                };

                nodes.Add(newFileNode);
            }
        }
    }
}