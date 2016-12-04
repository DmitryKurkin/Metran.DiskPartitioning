// -----------------------------------------------------------------------
// <copyright file="EventLogViewModel.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Gets.Utils;
using System.Collections.Generic;
using System.ComponentModel;

namespace Metran.FileSystemViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class EventLogViewModel : IEventLogViewModel
    {
        private readonly IList<string> _eventList;

        public EventLogViewModel()
        {
            _eventList = new List<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IBindingList Events => new BindingList<string>(_eventList);

        public void AppendEvent(string eventInfo)
        {
            _eventList.Insert(0, eventInfo);

            PropertyChanged.OnPropertyChanged(() => Events);
        }
    }
}