#region Usings

using System.Windows;

#endregion

namespace DiffImagesCollector
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            viewModel.TakeBackgroundCapture();
        }

        private void CaptureButton_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.TakeCapture();
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.TakeBackgroundCapture();
        }
    }
}