#region Usings

#endregion

namespace DotsMapper
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            viewModel.ProjectionPrepare();
        }
    }
}