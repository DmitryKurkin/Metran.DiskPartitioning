// -----------------------------------------------------------------------
// <copyright file="IDiskLoadingViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDiskLoadingViewModel : INotifyPropertyChanged
    {
        bool IsEnabled { get; }

        bool IsLoadEnabled { get; }

        bool IsCloseEnabled { get; }

        bool IsFatSelectorsEnabled { get; }

        bool IsFat16Checked { get; set; }

        bool IsFat32Checked { get; set; }

        void Init();

        void Load();

        void Close();
    }
}