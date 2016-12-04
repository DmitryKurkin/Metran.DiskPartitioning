// -----------------------------------------------------------------------
// <copyright file="DiskSelectionViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Gets.Utils;
using System.ComponentModel;
using System.IO;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DiskSelectionViewModel : IDiskSelectionViewModel
    {
        private readonly IEventLogViewModel _eventLogModel;

        private readonly BindingList<string> _availableDisks;

        private string _selectedDisk;

        private bool _areContentsLoaded;

        public DiskSelectionViewModel(IEventLogViewModel eventLogModel)
        {
            _eventLogModel = eventLogModel;
            _availableDisks = new BindingList<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEnabled => IsRefreshEnabled && !_areContentsLoaded;

        public IBindingList AvailableDisks => _availableDisks;

        public string SelectedDisk
        {
            get { return _selectedDisk; }
            set
            {
                _selectedDisk = value;

                PropertyChanged.OnPropertyChanged(() => SelectedDisk);
            }
        }

        public bool IsRefreshEnabled { get; private set; }

        public bool AreContentsLoaded
        {
            set
            {
                _areContentsLoaded = value;

                PropertyChanged.OnPropertyChanged(() => IsEnabled);
            }
        }

        public void Init()
        {
            Refresh();

            IsRefreshEnabled = true;

            PropertyChanged.OnPropertyChanged(() => IsEnabled);
            PropertyChanged.OnPropertyChanged(() => IsRefreshEnabled);
        }

        public void Refresh()
        {
            _eventLogModel.AppendEvent("Refreshing the disk list...");

            SelectedDisk = null;

            _availableDisks.Clear();
            foreach (var i in DriveInfo.GetDrives())
            {
                // where d.DriveType == DriveType.Removable
                _availableDisks.Add(i.Name);
            }

            PropertyChanged.OnPropertyChanged(() => AvailableDisks);

            _eventLogModel.AppendEvent("Success");
        }
    }
}