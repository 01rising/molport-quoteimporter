using System.Windows;
using QuoteImporter.ViewModels;

namespace QuoteImporter
{
    // Code-behind for the main WPF window. It currently only attaches the view model.
    // TODO: Keep UI event handling thin and move import behavior into MainViewModel.
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
