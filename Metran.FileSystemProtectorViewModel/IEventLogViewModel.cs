// -----------------------------------------------------------------------
// <copyright file="IEventLogViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IEventLogViewModel : INotifyPropertyChanged
    {
        IBindingList Events { get; }

        void AppendEvent(string eventInfo);
    }
}