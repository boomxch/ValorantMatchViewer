using System.Windows;

namespace ValorantMatchViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var mm = new MatchManager();
        }
    }
}
