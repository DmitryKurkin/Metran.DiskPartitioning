using Metran.FileSystemViewModel;
using System.Windows;

namespace Metran
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var eventLogModel = new EventLogViewModel();

            var selectionModel = new DiskSelectionViewModel(eventLogModel);
            var contentsModel = new DiskContentsViewModel();
            var loadingModel = new DiskLoadingViewModel(contentsModel, selectionModel, eventLogModel);

            var mainWindow = new MainWindow(selectionModel, loadingModel, contentsModel, eventLogModel);
            mainWindow.Show();
        }
    }
}