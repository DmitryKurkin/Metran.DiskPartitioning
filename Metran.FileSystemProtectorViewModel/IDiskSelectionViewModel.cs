// -----------------------------------------------------------------------
// <copyright file="IDiskSelectionViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace Metran.FileSystemViewModel
{
    public interface IDiskSelectionViewModel : INotifyPropertyChanged
    {
        bool IsEnabled { get; }

        IBindingList AvailableDisks { get; }

        string SelectedDisk { get; set; }

        bool IsRefreshEnabled { get; }

        void Init();

        void Refresh();
    }
}