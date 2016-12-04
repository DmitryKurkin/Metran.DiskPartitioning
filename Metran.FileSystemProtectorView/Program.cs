// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Emerson">
// Rosemount Inc.
// </copyright>
// -----------------------------------------------------------------------

using Metran.FileSystemViewModel;
using System;
using System.Windows.Forms;

namespace Metran.FileSystemView
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var eventLogModel = new EventLogViewModel();

            var selectionModel = new DiskSelectionViewModel(eventLogModel);
            var contentsModel = new DiskContentsViewModel();
            var loadingModel = new DiskLoadingViewModel(contentsModel, selectionModel, eventLogModel);

            Application.Run(new FileSystemProtectorViewForm(selectionModel, loadingModel, contentsModel, eventLogModel));
        }
    }
}